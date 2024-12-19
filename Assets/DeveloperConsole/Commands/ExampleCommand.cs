using System.Collections.Generic;

namespace DeveloperConsole
{
    public class ExampleCommand : Command
    {
        public ExampleCommand()
        {
            // Fill with all valid invoking command words
            commandWords = new string[]
            {
                "example"
            };

            // Follow the syntax and fill this out
            help = new CommandHelp
            (
                "example",
                "This command is just an example.",
                commandWords,
                new List<CommandUsage>
                {
                    new CommandUsage
                    {
                        description = "This is an example."
                    }
                }
            );
        }

        public override bool Execute(string[] args)
        {
            if (InvalidArgs(args)) return false;

            output = "You just ran an example command";

            return true;
        }
    }
}