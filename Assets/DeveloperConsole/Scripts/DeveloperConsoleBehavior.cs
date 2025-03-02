using TMPro;
using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace DeveloperConsole
{

    /// <summary>
    /// A wrapper around the developer console to allow it to interact with Unity.
    /// </summary>
    public class DeveloperConsoleBehavior : MonoBehaviour
    {
        [Header("Full screen")]
        [SerializeField] private GameObject fullScreenCanvas;
        [SerializeField] private TMP_InputField fullScreenCommandLine;
        [SerializeField] private TextMeshProUGUI fullScreenLog;

        [Header("Small screen")]
        [SerializeField] private GameObject smallScreenCanvas;
        [SerializeField] private TMP_InputField smallScreenCommandLine;
        [SerializeField] private TextMeshProUGUI smallScreenLog;

        [Header("Display screen")]
        [SerializeField] private GameObject displayCanvas;
        [SerializeField] private TMP_Text frameCounter;
        [SerializeField] private DebugSlot[] debugSlots = new DebugSlot[8];

        /// <summary>
        /// A singleton instance of the Wrapper around the developer console.
        /// </summary>
        public static DeveloperConsoleBehavior Instance { get; private set; }

        private GameObject canvas;
        private TextMeshProUGUI log;
        private TMP_InputField commandLine;

        private DeveloperConsole console;
        private DeveloperConsoleInput input;
        private DeveloperConsoleConfig config;

        private Dictionary<string, string> aliases = new Dictionary<string, string>();

        private float timeScaleOnPause = 1;

        private string bufferedCommand;

        private int historyIndex;
        private int logLines = 0;

        private bool isFullScreen;
        private bool pauseTime;


        #region Initialization

        #if DEVELOPMENT_BUILD || UNITY_EDITOR

        /// <summary>
        /// Enables loading the console without needing to have an instance in the scene.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BootstrapConsole()
        {
            GameObject consolePrefab = Resources.Load<GameObject>("System/DevelopmentConsole");

            if (consolePrefab != null)
            {
                GameObject console = Instantiate(consolePrefab);
                DontDestroyOnLoad(console);
            }
            else
            {
                Debug.LogWarning("Could not initialize development console because the prefab was not found by a call to Resources.Load()");
            }

            // Disable unity's stupid ass hardcoded rendering debugger that prevents you from fast deleting words because of ctrl+bcksp collision
            // Only works in SRP
            #if SRP
            DebugManager.instance.enableRuntimeUI = false;
            #endif
        }
        #endif

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            else if (Instance == null)
            {
                Instance = this;
            }

            config = Resources.Load<DeveloperConsoleConfig>("System/PersistentConfig");
            if (config == null)
            {
                Debug.LogError("Could not find persistent config in resources. " +
                    "Please make sure there is a DeveloperConsoleConfig in a Resources/System folder named PersistentConfig.");
            }

            if (config.commandHistory == null)
            {
                config.commandHistory = new List<string>();
            }
            historyIndex = -1;

            // Add other config settings which need to be updated in real time instead of on console close here:
            isFullScreen = config.fullscreen;
            pauseTime = config.pausetime;

            SetFullScreen(isFullScreen);
            canvas.SetActive(false);

            console = new DeveloperConsole();
            input = new DeveloperConsoleInput();
            input.DeveloperConsole.Enable();
            timeScaleOnPause = Time.timeScale;

            StartCoroutine(FrameCounter());
        }
        private void OnEnable()
        {
            fullScreenCommandLine.onSubmit.AddListener(OnSubmitCommand);
            smallScreenCommandLine.onSubmit.AddListener(OnSubmitCommand);
            Application.logMessageReceived += OnUnityLogMessage;

            if (input != null)
            {
                input.DeveloperConsole.Toggle.performed += OnToggleConsole;
                input.DeveloperConsole.Exit.performed += OnExitConsole;
                input.DeveloperConsole.Backspace.performed += OnBackspace;
                input.DeveloperConsole.MoreRecentCommand.performed += OnLessRecentCommand;
                input.DeveloperConsole.LessRecentCommand.performed += OnMoreRecentCommand;
            }
        }
        private void OnDisable()
        {
            fullScreenCommandLine.onSubmit.RemoveListener(OnSubmitCommand);
            smallScreenCommandLine.onSubmit.RemoveListener(OnSubmitCommand);
            Application.logMessageReceived -= OnUnityLogMessage;

            if (input != null)
            {
                input.DeveloperConsole.Toggle.performed -= OnToggleConsole;
                input.DeveloperConsole.Exit.performed -= OnExitConsole;
                input.DeveloperConsole.Backspace.performed -= OnBackspace;
                input.DeveloperConsole.MoreRecentCommand.performed -= OnLessRecentCommand;
                input.DeveloperConsole.LessRecentCommand.performed -= OnMoreRecentCommand;
            }

            if (Application.isEditor)
            {
                if (config != null)
                {
                    EditorUtility.SetDirty(config);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        private void RunInitializationScript()
        {
            TextAsset startScript = Resources.Load<TextAsset>("System/on_console_start");
            if (startScript == null)
            {
                if (config.warnaboutinitscript)
                Debug.LogWarning("No developer console initialization script was found. If you intended to have one, make sure it is " +
                    "named 'on_console_start.txt' and is in the Resources/System directory. Otherwise, use `config set warnonstart false` to " +
                    "disable this warning.");
                return;
            }

            string[] rawLines = startScript.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

            List<string> lines = new List<string>();
            for (int i = 0; i < rawLines.Length; i++)
            {
                string strippedLine = Regex.Replace(rawLines[i], @"//.*", "").TrimEnd();
                if (!string.IsNullOrEmpty(strippedLine))
                {
                    lines.Add(strippedLine);
                }
            }

            foreach (string line in lines)
            {
                RunCommand(line, false);
            }
        }
        #endregion
   
        #region Input
        private void OnToggleConsole(InputAction.CallbackContext context)
        {
            canvas.SetActive(!canvas.activeInHierarchy);

            if (canvas.activeInHierarchy)
            {
                commandLine.ActivateInputField();
                timeScaleOnPause = Time.timeScale;

                if (config.pausetime)
                {
                    Time.timeScale = 0;
                }
            }
            else
            {
                OnExitConsole(default);
            }
        }
        private void OnExitConsole(InputAction.CallbackContext context)
        {
            Time.timeScale = timeScaleOnPause;

            ClearCommandLine();
            canvas.SetActive(false);
        }
        private void OnBackspace(InputAction.CallbackContext context)
        {
            if (!canvas.activeInHierarchy) return;

            if (Keyboard.current.ctrlKey.isPressed)
            {
                string current = commandLine.text;

                int index = current.Length - 1;
                while (index > 0 && char.IsWhiteSpace(current[index]))
                {
                    index--;
                }

                if (index == 0)
                {
                    commandLine.text = string.Empty;
                    Canvas.ForceUpdateCanvases();
                    return;
                }

                while (index > 0 && !char.IsWhiteSpace(current[index]))
                {
                    index--;
                }

                commandLine.text = current.Substring(0, index + 1);
                Canvas.ForceUpdateCanvases();
            }
        }
        private void OnLessRecentCommand(InputAction.CallbackContext context)
        {
            if (!canvas.activeInHierarchy) return;
            
            List<string> history = config.commandHistory;

            if (historyIndex + 1 >= history.Count) return;

            historyIndex++;

            commandLine.text = history[historyIndex];
            MoveCaretToEnd();
        }
        private void OnMoreRecentCommand(InputAction.CallbackContext context)
        {
            if (!canvas.activeInHierarchy) return;

            List<string> history = config.commandHistory;

            if (historyIndex - 1 < 0) return;

            historyIndex--;

            commandLine.text = history[historyIndex];
            MoveCaretToEnd();
        }
        #endregion

        #region UI
        private void SetFullScreen(bool fullScreen)
        {
            if (fullScreen)
            {
                fullScreenCanvas.SetActive(true);
                smallScreenCanvas.SetActive(false);

                canvas = fullScreenCanvas;
                commandLine = fullScreenCommandLine;
                log = fullScreenLog;
                log.text = smallScreenLog.text;
            }
            else
            {
                fullScreenCanvas.SetActive(false);
                smallScreenCanvas.SetActive(true);

                canvas = smallScreenCanvas;
                commandLine = smallScreenCommandLine;
                log = smallScreenLog;
                log.text = fullScreenLog.text;
            }

            commandLine.ActivateInputField();
        }
        private void AddMessage(string message)
        {
            log.text += message + Environment.NewLine;
            logLines++;

            if (logLines > config.maxloglines)
            {
                int index = log.text.IndexOf(Environment.NewLine);
                log.text = log.text.Substring(index + 1);
                logLines--;
            }
        }
        private void ClearCommandLine()
        {
            if (string.IsNullOrEmpty(bufferedCommand))
            {
                commandLine.text = string.Empty;
            }
            else
            {
                commandLine.text = bufferedCommand;
                MoveCaretToEnd();
                bufferedCommand = string.Empty;
            }
        }
        private IEnumerator FrameCounter()
        {
            while (true)
            {
                frameCounter.text = "fps = " + (int)(1 / Time.deltaTime);
                yield return new WaitForSeconds(0.2f);
            }
        }
        #endregion

        #region Interface
        /// <summary>
        /// Adds an alias.
        /// </summary>
        /// <param name="key">The alias to add.</param>
        /// <param name="value">The command to replace it with.</param>
        public void AddAlias(string key, string value)
        {
            aliases[key.ToLower()] = value.ToLower();
        }

        /// <summary>
        /// Removes an alias.
        /// </summary>
        /// <param name="key">The alias to remove.</param>
        public bool RemoveAlias(string key)
        {
            return aliases.Remove(key.ToLower());
        }

        /// <summary>
        /// Sets the command line to a command in history. Will automatically add 
        /// one from the input index to account for the command running this.
        /// </summary>
        /// <param name="index">The index to set to.</param>
        /// <returns>True if the index is in range.</returns>
        public bool SetCMDToHistory(int index)
        {
            // Account for the command which was run to invoke this.
            index++;
            if (index < 0 || index >= config.commandHistory.Count) return false;

            bufferedCommand = config.commandHistory[index];
            return true;
        }

        /// <summary>
        /// Deletes all old history when the max hisotry is updated.
        /// </summary>
        /// <param name="maxCount">The new max histroy to record.</param>
        /// <returns>True if the index is positive.</returns>
        public bool SetMaxHistory(int maxCount)
        {
            if (maxCount < 0) return false;

            config.maxhistory = maxCount;

            if (maxCount > config.commandHistory.Count) return true;

            List<string> toRemove = new List<string>();
            for (int i = 0; i < config.commandHistory.Count; i++)
            {
                if (i >= maxCount)
                {
                    toRemove.Add(config.commandHistory[i]);
                }
            }

            foreach (string removeable in toRemove) 
            { 
                config.commandHistory.Remove(removeable);
            }

            return true;
        }

        /// <summary>
        /// Clears the log.
        /// </summary>
        public void ClearLog()
        {
            log.text = string.Empty;
            logLines = 0;
        }
        #endregion

        private bool first = true, second = false;
        private void Update()
        {
            // Need to check for this because it takes an update loop to 
            // delete any false references from the scene and commands referencing
            // this won't be able to find it
            if (first)
            {
                first = false;
                second = true;
            }
            if (second)
            {
                second = false;
                RunInitializationScript();
            }
            
            // Update state from config sets 
            if (isFullScreen != config.fullscreen)
            {
                SetFullScreen(config.fullscreen);
                isFullScreen = config.fullscreen;
            }
            if (pauseTime != config.pausetime)
            {
                pauseTime = config.pausetime;
                if (pauseTime)
                {
                    timeScaleOnPause = Time.timeScale;
                    Time.timeScale = 0;
                }
                else
                {
                    Time.timeScale = timeScaleOnPause;
                }
            }
        }
        private void OnSubmitCommand(string input)
        {
            RunCommand(input);
        }

        private List<string> ChunkCommand(string command)
        {
            var matches = Regex.Matches(command, @"[^\s""']+|""([^""]*)""|'([^']*)'");
            List<string> result = new List<string>();

            foreach (Match match in matches)
            {
                if (!string.IsNullOrEmpty(match.Groups[1].Value)) result.Add(match.Groups[1].Value); 
                else if (!string.IsNullOrEmpty(match.Groups[2].Value)) result.Add(match.Groups[2].Value);
                else result.Add(match.Value);
            }

            return result;
        }

        private void RunCommand(string command, bool addTohistory = true)
        {
            if (addTohistory)
            {
                bool commandEmpty = string.IsNullOrEmpty(command);
                bool valid = config.commandHistory.Count == 0 || !config.commandHistory[0].Equals(command);

                if (valid && !commandEmpty)
                {
                    config.commandHistory.Insert(0, command);

                    if (config.commandHistory.Count > config.maxhistory)
                    {
                        config.commandHistory.RemoveAt(config.commandHistory.Count - 1);
                    }
                }
            }

            historyIndex = -1;

            string commandWord = Regex.Split(command.Trim(), @"\s+")[0];
            Command cmd = console.FindCommand(commandWord);

            if (cmd != null)
            {
                try
                {
                    // Test to make sure help is declared
                    cmd.Name();
                }
                catch
                {
                    AddMessage(MessageFormatter.CreateErrorMessage($"The base class field commandHelp has not been set for the command {commandWord}. The command " +
                        $"cannot run without this so it needs to be filled out. See the ExampleCommand class for an example."));
                    ClearCommandLine();
                    commandLine.ActivateInputField();
                    return;
                }
            }

            List<string> chunkedCommand = ChunkCommand(command);
            List<string> aliasedCommand = new List<string>();
            if (cmd == null || !cmd.Name().Equals("alias", StringComparison.OrdinalIgnoreCase))
            {
                for (int i = 0; i < chunkedCommand.Count; i++)
                {
                    if (aliases.ContainsKey(chunkedCommand[i]))
                    {
                        string[] aliasParts = aliases[chunkedCommand[i]].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        for (int j = 0; j < aliasParts.Length; j++) 
                        {
                            aliasedCommand.Add(aliasParts[j]);
                        }
                    }
                    else
                    {
                        aliasedCommand.Add(chunkedCommand[i]);
                    }
                }
            }
            else if (cmd != null)
            {
                aliasedCommand = chunkedCommand;
            }
           
            string result = console.ProcessCommand(aliasedCommand.ToArray());

            AddMessage(result);

            ClearCommandLine();
            commandLine.ActivateInputField();
        }
        private void OnUnityLogMessage(string message, string stackTrace, LogType type) 
        {
            if (!config.showunitylog) return;

            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    AddMessage(MessageFormatter.CreateErrorMessage("<Unity> " + message + stackTrace));
                    break;
                case LogType.Warning:
                    AddMessage(MessageFormatter.CreateWarningMessage("<Unity> " + message));
                    break;
                default:
                    AddMessage(MessageFormatter.CreateLogMessage("<Unity> " + message));
                    break;
            }
        }
        private void MoveCaretToEnd()
        {
            commandLine.DeactivateInputField();
            commandLine.ActivateInputField();
            commandLine.caretPosition = commandLine.text.Length;
            commandLine.selectionAnchorPosition = commandLine.text.Length;
            Canvas.ForceUpdateCanvases();
        }
    }
}

