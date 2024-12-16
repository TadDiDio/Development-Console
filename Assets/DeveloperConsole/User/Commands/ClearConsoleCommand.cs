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
                new List<HelpArg>()
            );
        }
        public override bool Execute(string[] args)
        {
            FieldResult result = GetField(typeof(DeveloperConsoleBehavior), "log");
            
            if (!result.success)
            {
                output = ErrorGenerator.ReflectionError(result);
                return false;
            }

            ((TextMeshProUGUI)result.value).text = string.Empty;
            return true;
        }
    }
}
