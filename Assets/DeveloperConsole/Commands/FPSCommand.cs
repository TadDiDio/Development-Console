using UnityEngine;
using System.Collections.Generic;
using TMPro;

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
            if (!TryGetField(typeof(DeveloperConsoleBehavior), "frameCounter", out TMP_Text text)) return false;
            
            text.gameObject.SetActive(!text.gameObject.activeSelf);

            return true;
        }
    }
}