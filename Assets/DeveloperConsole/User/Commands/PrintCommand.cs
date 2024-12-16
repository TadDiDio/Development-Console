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
                new List<HelpArg>()
                {
                    new HelpArg
                    {
                        name = "message",
                        type = "string",
                        description = "The thing to print."
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
