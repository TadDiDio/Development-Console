using UnityEditor;
using UnityEngine;
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
                new List<HelpArg>()
            );
        }

        // The commands passed in already have the command word stripped away
        public override bool Execute(string[] args)
        {
            if (Application.isEditor)
            {
                EditorApplication.isPlaying = false;
            }
            else
            {
                Application.Quit();
            }
            return true;
        }
    }
}