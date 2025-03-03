using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace DeveloperConsole
{
    public class EventSystemManager : MonoBehaviour
    {
        [SerializeField] private InputActionAsset developerConsoleActionMap;

        private EventSystem eventSystem;

        private void Awake()
        {
            eventSystem = GetComponent<EventSystem>();

            #if ENABLE_INPUT_SYSTEM
            InputSystemUIInputModule module = eventSystem.AddComponent<InputSystemUIInputModule>();
            module.actionsAsset = developerConsoleActionMap;
            #else
            eventSystem.AddComponent<StandaloneInputModule>();
            #endif
        }

        /// <summary>
        /// Sets the input map.
        /// </summary>
        /// <param name="input">The map to set to.</param>
        public void SetInputMap(DeveloperConsoleInput input)
        {
            if (eventSystem.TryGetComponent(out InputSystemUIInputModule module))
            {
                //module.actionsAsset = input;
            }
        }
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            eventSystem.enabled = eventSystems.Length == 1;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            eventSystem.enabled = false;
        }
    }
}