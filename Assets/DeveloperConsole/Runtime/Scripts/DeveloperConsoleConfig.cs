using UnityEngine;
using System.Collections.Generic;

namespace DeveloperConsole
{
    [CreateAssetMenu(fileName = "New console config", menuName = "Developer Console/")]
    public class DeveloperConsoleConfig : ScriptableObject
    {
        public bool fullscreen;
        public bool pausetime;
        public bool showunitylog;
        public bool warnonstart;

        public int maxHistory;
        public LinkedList<string> commandHistory;
    }
}
