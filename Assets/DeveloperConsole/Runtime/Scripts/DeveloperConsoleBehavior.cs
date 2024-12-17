using TMPro;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using Unity.VisualScripting.FullSerializer;

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

        private WorldStateOnToggle worldStateOnToggle;

        private LinkedListNode<string> historyIndex; 

        private bool isFullScreen;
        private bool pauseTime;


        #region Initialization

        /// <summary>
        /// Enables loading the console without needing to have an instance in the scene.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BootstrapConsole()
        {
            GameObject consolePrefab = Resources.Load<GameObject>("DevelopmentConsole");

            if (consolePrefab != null)
            {
                GameObject console = Instantiate(consolePrefab);
                DontDestroyOnLoad(console);
            }
            else
            {
                Debug.LogWarning("Could not initialize development console because the prefab was not found by a call to Resources.Load()");
            }
        }

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

            config = Resources.Load<DeveloperConsoleConfig>("PersistentConfig");
            if (config == null)
            {
                Debug.LogError("Could not find persistent config in resources. " +
                    "Please make sure there is a DeveloperConsoleConfig in a resources folder named PersistentConfig.");
            }

            if (config.commandHistory == null) config.commandHistory = new LinkedList<string>();
            historyIndex = config.commandHistory.First;

            isFullScreen = config.fullscreen;
            pauseTime = config.pauseTime;

            SetFullScreen(isFullScreen);
            canvas.SetActive(false);

            console = new DeveloperConsole();
            input = new DeveloperConsoleInput();
            input.DeveloperConsole.Enable();
            worldStateOnToggle = new WorldStateOnToggle();
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
                input.DeveloperConsole.MoreRecentCommand.performed += OnMoreRecentCommand;
                input.DeveloperConsole.LessRecentCommand.performed += OnLessRecentCommand;
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
                input.DeveloperConsole.MoreRecentCommand.performed -= OnMoreRecentCommand;
                input.DeveloperConsole.LessRecentCommand.performed -= OnLessRecentCommand;
            }
        }

        private void RunInitializationScript()
        {
            TextAsset startScript = Resources.Load<TextAsset>("on_console_start");
            if (startScript == null) return;

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
                worldStateOnToggle.timescale = Time.timeScale;

                if (config.pauseTime)
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
            Time.timeScale = worldStateOnToggle.timescale;

            ClearCommandLine();
            canvas.SetActive(false);
        }
        private void OnBackspace(InputAction.CallbackContext context)
        {
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
        private void OnMoreRecentCommand(InputAction.CallbackContext context)
        {
            LinkedList<string> history = config.commandHistory;

            if (history.Count == 0) return;

            if (historyIndex == null)
            {
                historyIndex = history.First;
            }
            else if(historyIndex.Next != null)
            {
                historyIndex = historyIndex.Next;
            }

            commandLine.text = historyIndex.Value;
            MoveCaretToEnd();
        }
        private void OnLessRecentCommand(InputAction.CallbackContext context)
        {
            LinkedList<string> history = config.commandHistory;

            if (history.Count == 0) return;
            if (historyIndex == history.First) return;

            historyIndex = historyIndex.Previous;

            commandLine.text = historyIndex.Value;
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
        } 
        private void ClearCommandLine()
        {
            commandLine.text = string.Empty;
        }
        #endregion

        #region Interface
        public void AddAlias(string key, string value)
        {
            aliases[key] = value;
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
            if (pauseTime != config.pauseTime)
            {
                pauseTime = config.pauseTime;
                if (pauseTime)
                {
                    worldStateOnToggle.timescale = Time.timeScale;
                    Time.timeScale = 0;
                }
                else
                {
                    Time.timeScale = worldStateOnToggle.timescale;
                }
            }
        }
        private void OnSubmitCommand(string input)
        {
            RunCommand(input);
        }
        private void RunCommand(string command, bool addTohistory = true)
        {
            if (addTohistory)
            {
                bool empty = string.IsNullOrEmpty(command);
                bool valid = config.commandHistory.First == null || !config.commandHistory.First.Value.Equals(command);

                if (valid && !empty)
                {
                    config.commandHistory.AddFirst(command);

                    if (config.commandHistory.Count > config.maxHistory)
                    {
                        config.commandHistory.RemoveLast();
                    }
                }
            }

            historyIndex = null;

            foreach (var keyValpair in aliases)
            {
                command = command.Replace(keyValpair.Key, keyValpair.Value);
            }

            string result = console.ProcessCommand(command);

            AddMessage(result);

            ClearCommandLine();
            commandLine.ActivateInputField();
        }
        private struct WorldStateOnToggle
        {
            public float timescale;
        }
        private void OnUnityLogMessage(string message, string stackTrace, LogType type) 
        {
            if (!config.showUnityLog) return;

            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    AddMessage(MessageFormatter.CreateErrorMessage("<Unity> " + message));
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

