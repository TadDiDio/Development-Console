using System.Collections.Generic;

namespace DeveloperConsole
{
    public class PrintCommand : Command
    {
        public PrintCommand()
        {
            commandWords = new string[]
            {
                "print",
                "log",
                "echo",
                "say"
            };

            help = new CommandHelp
            (
                "Print",
                "Prints something to the console.",
                commandWords,
                new List<CommandUsage>()
                {
                    new CommandUsage
                    {
                        subcommand = "",
                        parameters = new string[] { "value1", "value2", "..." },
                        description = "prints the given values to the screen as inputted"
                    }
                }
            );
        }
        public override bool Execute(string[] args)
        {
            output = string.Join(" ", args);
            
            return true;
        }
    }
}
