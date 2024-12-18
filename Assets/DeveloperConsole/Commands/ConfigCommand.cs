using System;
using UnityEngine;
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

            help = new CommandHelp
            (
                "Config",
                "Holds actions to modify or see the console config.",
                commandWords,
                new List<CommandUsage>
                {
                    new CommandUsage
                    {
                        invokeWord = "",
                        description = "Prints all config values to the screen."
                    },
                    new CommandUsage
                    {
                        invokeWord = "get",
                        parameters = new string[] { "field" },
                        description = "Gets the value of the field."
                    },
                    new CommandUsage
                    {
                        invokeWord = "set",
                        parameters = new string[] { "field", "value"},
                        description = "Sets field in config to value."
                    }
                }
            );
        }

        public override bool Execute(string[] args)
        {
            if (InvalidArgs(args)) return false;

            if (args.Length == 0)
            {
                return ShowConfig();
            }
            if (args.Length == 2)
            {
                return SetConfig(args);
            }
            if (args.Length == 3)
            {
                return SetConfigSetting(args);
            }

            output = "Something unexpected happened.";
            return false;
        }

        private bool ShowConfig()
        {
            FieldResult fieldResult = GetField(typeof(DeveloperConsoleBehavior), "config");

            if (!fieldResult.success)
            {
                return ReturnError(fieldResult);
            }

            DeveloperConsoleConfig config = (DeveloperConsoleConfig)fieldResult.value;

            string[] lines =
            { 
                $"fullscreen : {config.fullscreen}",
                $"pausetime : {config.pausetime}",
                $"showunitylog : {config.showunitylog}",
            };

            output = Environment.NewLine + "Current config settings" + Environment.NewLine + "=======================" + Environment.NewLine;
            output = MessageFormatter.FromLines(MessageFormatter.Align(lines, new Color[] { MessageFormatter.LightBlue}), output);
            return true;
        }

        private bool SetConfig(string[] args)
        {
            if (!args[0].Equals("set", StringComparison.OrdinalIgnoreCase))
            {
                output = $"Unrecognized argument {args[0]}.";
                return false;
            }

            DeveloperConsoleConfig newConfig = Resources.Load<DeveloperConsoleConfig>($"Configs/{args[1]}");
            if (newConfig == null)
            {
                output = $"Could not find a config called {args[1]}. Please make sure that all configs are in the Resources/Configs directory.";
                return false;
            }

            FieldResult fieldResult = GetField(typeof(DeveloperConsoleBehavior), "config");

            if (!fieldResult.success)
            {
                return ReturnError(fieldResult);
            }

            DeveloperConsoleConfig persistentConfig = (DeveloperConsoleConfig)fieldResult.value;

            // ADD ALL COPY SETTINGS HERE
            persistentConfig.pausetime = newConfig.pausetime;
            persistentConfig.fullscreen = newConfig.fullscreen;
            persistentConfig.showunitylog = newConfig.showunitylog;
            persistentConfig.maxHistory = newConfig.maxHistory;
            // END COPY

            return true;
        }

        private bool SetConfigSetting(string[] args)
        {
            if (!args[0].Equals("set", StringComparison.OrdinalIgnoreCase))
            {
                output = $"Unrecognized argument {args[0]}.";
                return false;
            }

            // Search for field in config named args[1]
            FieldResult result = GetField(typeof(DeveloperConsoleBehavior), "config");

            if (!result.success)
            {
                return ReturnError(result);
            }

            DeveloperConsoleConfig config = (DeveloperConsoleConfig)result.value;
            result = SetField(config, args[1], args[2]);
            
            if (!result.success)
            {
                return ReturnError(result);
            }

            return true;
        }
    }
}