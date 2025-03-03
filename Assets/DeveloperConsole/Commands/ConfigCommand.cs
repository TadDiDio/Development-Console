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
                        subcommand = "",
                        description = "Prints all config values to the screen as well as available presets."
                    },
                    new CommandUsage
                    {
                        subcommand = "get",
                        parameters = new string[] { "field" },
                        description = "Gets the value of the field."
                    },
                    new CommandUsage
                    {
                        subcommand = "set",
                        parameters = new string[] { "config" },
                        description = "Applies all settings in <config>."
                    },
                    new CommandUsage
                    {
                        subcommand = "set",
                        parameters = new string[] { "field", "value"},
                        description = "Sets field in config to value."
                    }
                }
            );
        }

        public override bool Execute(string[] args)
        {
            if (args.Length == 0)
            {
                return ShowConfig();
            }
            if (args.Length == 2)
            {
                return StringEquals(args[0], "get") ? GetConfigSetting(args) : SetConfig(args);
            }
            if (args.Length == 3)
            {
                return SetConfigSetting(args);
            }

            return false;
        }

        private bool ShowConfig()
        {
            if (!TryGetField(typeof(DeveloperConsoleBehavior), "config", out DeveloperConsoleConfig config)) return false;

            string[] lines =
            { 
                $"fullscreen : {config.fullscreen}",
                $"pausetime : {config.pausetime}",
                $"fpsshown : {config.fpsshown}",
                $"showunitylog : {config.showunitylog}",
                $"showunitylogstacktrace : {config.showunitylogstacktrace}",
                $"maxhistory : {config.maxhistory}",
                $"maxloglines : {config.maxloglines}",
                $"warnaboutinitscript : {config.warnaboutinitscript}",
            };

            output = Environment.NewLine + "Current config settings" + Environment.NewLine + "=======================" + Environment.NewLine;
            output = MessageFormatter.FromLines(MessageFormatter.Align(lines, new Color[] { MessageFormatter.Blue}), output);
            output += $"{Environment.NewLine}{MessageFormatter.AddColor("Available presets:", MessageFormatter.Green)}" +
                $"{Environment.NewLine}======================={Environment.NewLine}";

            foreach (var preset in Resources.LoadAll<DeveloperConsoleConfig>("Configs"))
            {
                output += $"{MessageFormatter.AddColor(preset.name, MessageFormatter.Blue)}{Environment.NewLine}";
            }
            
            return true;
        }
        private bool GetConfigSetting(string[] args)
        {
            string settingName = args[1];
            if (!TryGetField(typeof(DeveloperConsoleBehavior), "config", out DeveloperConsoleConfig config)) return false;

            if (StringEquals(settingName, "fullscreen"))
            {
                output = $"fullscreen : {config.fullscreen}";
                return true;
            }
            if (StringEquals(settingName, "pausetime"))
            {
                output = $"pausetime : {config.pausetime}";
                return true;
            }
            if (StringEquals(settingName, "showunitylog"))
            {
                output = $"showunitylog : {config.showunitylog}";
                return true;
            }
            if (StringEquals(settingName, "warnaboutinitscript"))
            {
                output = $"warnonstart : {config.warnaboutinitscript}";
                return true;
            }
            if (StringEquals(settingName, "maxhistory"))
            {
                output = $"maxhistory : {config.maxhistory}";
                return true;
            }
            if (StringEquals(settingName, "showunitylogstacktrace"))
            {
                output = $"showunitylogstacktrace : {config.showunitylogstacktrace}";
                return true;
            }
            if (StringEquals(settingName, "maxloglines"))
            {
                output = $"maxloglines : {config.maxloglines}";
                return true;
            }
            if (StringEquals(settingName, "fpsshown"))
            {
                output = $"fpsshown : {config.fpsshown}";
                return true;
            }

            output = $"There is no field named {settingName} in the config.";
            return false;
        }
        private bool SetConfig(string[] args)
        {
            if (!StringEquals(args[0], "set")) return UnrecognizedArgument(args[0]);

            DeveloperConsoleConfig newConfig = Resources.Load<DeveloperConsoleConfig>($"Configs/{args[1]}");
            if (newConfig == null)
            {
                output = $"Could not find a config called {args[1]}. Please make sure that all configs are in the Resources/Configs directory.";
                return false;
            }

            // Set maxhistory
            if (!TryInvokeFunction(typeof(DeveloperConsoleBehavior), "SetMaxHistory", out bool success1, new object[] { newConfig.maxhistory })) return false;
            if (!success1)
            {
                output = "The new max must be 0 or higher.";
                return false;
            }

            // Set fpsshown
            if (!TryInvokeFunction(typeof(DeveloperConsoleBehavior), "SetFrameCounterActive", new object[] { newConfig.fpsshown })) return false;

            if (!TryGetField(typeof(DeveloperConsoleBehavior), "config", out DeveloperConsoleConfig config)) return false;

            // ADD ALL COPY SETTINGS HERE
            config.pausetime = newConfig.pausetime;
            config.fullscreen = newConfig.fullscreen;
            config.showunitylog = newConfig.showunitylog;
            config.warnaboutinitscript = newConfig.warnaboutinitscript;
            config.showunitylogstacktrace = newConfig.showunitylogstacktrace;
            config.maxloglines = newConfig.maxloglines;
            config.maxhistory = newConfig.maxhistory;
            config.fpsshown = newConfig.fpsshown;
            // END COPY

            return true;
        }
        private bool SetConfigSetting(string[] args)
        {
            if (!StringEquals(args[0], "set")) return UnrecognizedArgument(args[0]);

            if (StringEquals(args[1], "maxhistory"))
            {
                if (!TryCast(args[2], out int max)) return false;
                if (!TryInvokeFunction(typeof(DeveloperConsoleBehavior), "SetMaxHistory", out bool success, new object[] { max })) return false;

                if (!success) output = "The new max must be 0 or higher.";

                return success;
            }
            if (StringEquals(args[1], "fpsshown"))
            {
                if (!TryCast(args[2], out bool active)) return false;
                return TryInvokeFunction(typeof(DeveloperConsoleBehavior), "SetFrameCounterActive", new object[] { active });
            }

            if (!TryGetField(typeof(DeveloperConsoleBehavior), "config", out DeveloperConsoleConfig config)) return false;
            if (!TrySetField(config, args[1], args[2])) return false;
            
            return true;
        }
    }
}