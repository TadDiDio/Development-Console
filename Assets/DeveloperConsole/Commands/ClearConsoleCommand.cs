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

            FieldResult result = GetField(typeof(DeveloperConsoleBehavior), "log");
            
            if (!result.success)
            {
                return ReturnError(result);
            }

            ((TextMeshProUGUI)result.value).text = string.Empty;
            return true;
        }
    }
}
