using TMPro;
using System.Collections.Generic;

namespace DeveloperConsole
{
    public class ClearConsoleCommand : Command
    {
        public ClearConsoleCommand()
        {
            commandWords = new string[]
            {
                "clear",
                "cls"
            };

            help = new CommandHelp
            (
                "Clear",
                "Clears the console log.",
                commandWords,
                new List<CommandUsage>
                {
                    new CommandUsage
                    {
                        description = "Clears the screen."
                    }
                }
            );
        }
        public override bool Execute(string[] args)
        {
            if (InvalidArgs(args)) return false;

            if (!TryGetField(typeof(DeveloperConsoleBehavior), "log", out TMP_Text text)) return false;
            
            text.text = string.Empty;
            return true;
        }
    }
}
