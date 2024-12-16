using System;
using System.Collections.Generic;

namespace DeveloperConsole
{
    public class ConfigCommand : Command
    {
        public ConfigCommand()
        {
            // Fill with all valid invoking command words
            commandWords = new string[]
            {
                "config",
                "configuration"
            };

            // Add 0 or more arg types to quickly validate parameters.
            argParser = new ArgumentParser(true);
            argParser.AddArgList(new List<Type>
            {
                typeof(string), // 'set'
                typeof(string), // Field name
                typeof(object)  // Value
            });

            help = new CommandHelp
            (
                "Config",
                "Holds actions to modify or see the console config.",
                new List<HelpArg>
                {
                    new HelpArg
                    {
                        name = "set <field> <value>",
                        type = "type of field",
                        description = "Sets <field> to <value>."
                    }
                }
            );
        }

        public override bool Execute(string[] args)
        {
            if (!ValidateArgs(args)) return false;

            if (args.Length == 0)
            {
                return ShowConfig();
            }
            if (args.Length == 3)
            {
                return SetConfig(args);
            }

            output = "Something unexpected happened.";
            return false;
        }

        private bool ShowConfig()
        {
            return true;
        }
        private bool SetConfig(string[] args)
        {
            if (!args[0].Equals("set", StringComparison.OrdinalIgnoreCase))
            {
                output = $"Unrecognized argument {args[0]}";
                return false;
            }

            // Search for field in config named args[1]
            FieldResult result = GetField(typeof(DeveloperConsoleBehavior), "config");

            if (!result.success)
            {
                output = ErrorGenerator.ReflectionError(result);
                return false;
            }

            DeveloperConsoleConfig config = (DeveloperConsoleConfig)result.value;
            result = SetField(config, args[1], args[2]);
            
            if (!result.success)
            {
                output = ErrorGenerator.ReflectionError(result);
                return false;
            }

            return true;
        }
    }
}