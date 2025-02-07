using UnityEngine;
using System.Collections.Generic;

namespace DeveloperConsole
{
    [CreateAssetMenu(fileName = "New console config", menuName = "Developer Console/")]
    public class DeveloperConsoleConfig : ScriptableObject
    {
        // IMPORTANT: When adding new fields, make sure to add support in the DeveloperConsoleBehaviour awake method (where the comment is)
        // and also change the ConfigCommand class to properly copy new settings over when using the set subcommand.
        public bool fullscreen;
        public bool pausetime;
        public bool showunitylog;
        public bool warnonstart;

        public int maxHistory;
        public LinkedList<string> commandHistory;
    }
}
