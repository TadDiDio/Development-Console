using UnityEngine;
using System.Collections.Generic;

namespace DeveloperConsole
{
    [CreateAssetMenu(fileName = "New console config", menuName = "Developer Console/")]
    public class DeveloperConsoleConfig : ScriptableObject
    {
        public bool fullscreen;
        public bool pauseTime;
        public bool showUnityLog;

        public int maxHistory;
        public LinkedList<string> commandHistory;
    }
}
