using UnityEditor;
using System.Collections.Generic;

namespace DeveloperConsole
{
    public class ExitCommand : Command
    {
        public ExitCommand()
        {
            commandWords = new string[]
            {
                "quit",
                "exit",
                "end",
                "leave",
                "close"
            };

            help = new CommandHelp
            (
                "Exit",
                "Exits playmode if in the editor or closes the application if in a build",
                commandWords,
                new List<CommandUsage>
                {
                    new CommandUsage
                    {
                        description = "Exits the application or editor"
                    }
                }
            );
        }

        // The commands passed in already have the command word stripped away
        public override bool Execute(string[] args)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else

            Application.Quit();
#endif
            return true;
        }
    }
}