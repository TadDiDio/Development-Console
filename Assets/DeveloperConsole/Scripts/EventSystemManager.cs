using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace DeveloperConsole
{
    public class EventSystemManager : MonoBehaviour
    {
        private EventSystem eventSystem;

        private void Awake()
        {
            eventSystem = GetComponent<EventSystem>();

            #if !ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<StandaloneInputModule>();
            Destroy(eventSystem.GetComponent<InputSystemUIInputModule>());
            #endif
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