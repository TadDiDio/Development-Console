using UnityEngine;

namespace DeveloperConsole
{
    public class CameraToggler : MonoBehaviour
    {
        private Camera thisCamera;
        private void Awake()
        {
            thisCamera = GetComponent<Camera>();
        }

        private void Update()
        {
            Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);

            foreach (var cam in cameras)
            {
                if (cam != thisCamera && cam.targetTexture == null)
                {
                    thisCamera.enabled = false;
                    return;
                }
            }

            thisCamera.enabled = true;
        }
    }
}
