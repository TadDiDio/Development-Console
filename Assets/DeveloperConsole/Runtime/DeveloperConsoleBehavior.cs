using TMPro;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor.ShaderKeywordFilter;

namespace DeveloperConsole
{
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

        private GameObject canvas;
        private TMP_InputField commandLine;
        private TextMeshProUGUI log;
        /// <summary>
        /// A singleton instance of the Wrapper around the developer console.
        /// </summary>
        public static DeveloperConsoleBehavior Instance { get; private set; }

        private DeveloperConsole console;
        private DeveloperConsoleInput input;
        private DeveloperConsoleConfig config;

        private WorldStateOnToggle worldStateOnToggle;
        private bool isFullScreen;

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

            isFullScreen = config.fullscreen;
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

            if (input != null)
            {
                input.DeveloperConsole.Toggle.performed += OnToggleConsole;
                input.DeveloperConsole.Exit.performed += OnExitConsole;
            }
        }
        private void OnDisable()
        {
            fullScreenCommandLine.onSubmit.RemoveListener(OnSubmitCommand);
            smallScreenCommandLine.onSubmit.RemoveListener(OnSubmitCommand);

            if (input != null)
            {
                input.DeveloperConsole.Toggle.performed -= OnToggleConsole;
                input.DeveloperConsole.Exit.performed -= OnExitConsole;
            }
        }
        #endregion

        #region UI
        private void Update()
        {
            if (isFullScreen != config.fullscreen)
            {
                SetFullScreen(config.fullscreen);
                isFullScreen = config.fullscreen;
            }
        }
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
        private void AddMessage(string message)
        {
            log.text += message + Environment.NewLine;
        }
 
        private void ClearCommandLine()
        {
            commandLine.text = string.Empty;
        }
        #endregion

        private void OnSubmitCommand(string input)
        {
            string result = console.ProcessCommand(input);

            AddMessage(result);

            ClearCommandLine();
            commandLine.ActivateInputField();
        }

        private struct WorldStateOnToggle
        {
            public float timescale;
        }
    }
}

