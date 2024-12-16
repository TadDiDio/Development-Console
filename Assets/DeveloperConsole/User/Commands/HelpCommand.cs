using System;
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

            argParser = new ArgumentParser(false);
            argParser.AddArgList(new List<Type>
            {
                typeof(string)
            });

            help = new CommandHelp
            (
                "Help",
                "Provides a formatted help message to tell how a command works.",
                new List<HelpArg>
                {
                    new HelpArg
                    {
                        name = "command",
                        type = "string",
                        description = "The command to search for"
                    }
                }
            );
        }

        public override bool Execute(string[] args)
        {
            if (!ValidateArgs(args)) return false;

            FieldResult result = GetField(typeof(DeveloperConsoleBehavior), "console");

            // Failed to get field for some reason
            if (!result.success)
            {
                output = ErrorGenerator.ReflectionError(result);
                return false;
            }

            // Ensure that command exists
            Command command = ((DeveloperConsole)result.value).FindCommand(args[0]);
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