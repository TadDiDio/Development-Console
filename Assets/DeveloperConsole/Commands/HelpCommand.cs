using System.Collections.Generic;

namespace DeveloperConsole
{
    public class HelpCommand : Command
    {
        public HelpCommand()
        {
            commandWords = new string[] 
            { 
                "help",
                "man",
                "manual"
            };

            help = new CommandHelp
            (
                "Help",
                "Provides a formatted help message to tell how a command works.",
                commandWords,
                new List<CommandUsage>
                {
                    new CommandUsage
                    {
                        subcommand = "",
                        parameters = new string[] { "command" },
                        description = "Prints help for command."
                    }
                }
            );
        }

        public override bool Execute(string[] args)
        {
            if (InvalidArgs(args)) return false;
            if (!TryGetField(typeof(DeveloperConsoleBehavior), "console", out DeveloperConsole console)) return false;

            // Ensure that command exists
            Command command = console.FindCommand(args[0]);
            if (command == null)
            {
                output = $"No command found with name {args[0]}.";
                return false;
            }

            output = command.Help();
            return true;
        }
    }
}