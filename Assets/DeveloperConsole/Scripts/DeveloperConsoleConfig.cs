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
        public bool showunitylogstacktrace;

        public int maxloglines = 999;

        [Tooltip("Whether to send a warning to the console if there is no startup script in the resources folder.")]
        public bool warnaboutinitscript;

        public int maxhistory;
        public List<string> commandHistory;
    }
}
