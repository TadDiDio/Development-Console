using UnityEngine;
using System.Collections.Generic;

namespace DeveloperConsole
{
    public class FPSCommand : Command
    {
        public FPSCommand()
        {
            // Fill with all valid invoking command words
            commandWords = new string[]
            {
                "fps",
                "frame"
            };

            // Follow the syntax and fill this out
            help = new CommandHelp
            (
                "fps",
                "Shows or hides the fps counter.",
                commandWords,
                new List<CommandUsage>
                {
                    new CommandUsage
                    {
                        description = "Toggles the fps counter."
                    }
                }
            );
        }

        public override bool Execute(string[] args)
        {
            if (InvalidArgs(args)) return false;
            if (!TryGetField(typeof(DeveloperConsoleBehavior), "frameCanvas", out GameObject canvas)) return false;
            
            canvas.SetActive(!canvas.activeSelf);
            return true;
        }
    }
}