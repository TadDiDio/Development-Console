using System;
using UnityEngine;
using System.Collections.Generic;

namespace DeveloperConsole
{
    public class TimeCommand : Command
    {
        public TimeCommand()
        {
            // Fill with all valid invoking command words
            commandWords = new string[]
            {
                "time"
            };

            // Follow the syntax and fill this out
            help = new CommandHelp
            (
                "time",
                "Gets and sets values of the Time class like <time> and <scale>.",
                commandWords,
                new List<CommandUsage>
                {
                    new CommandUsage
                    {
                        description = "Shows the valid fields in the Time class."
                    },
                    new CommandUsage
                    {
                        subcommand = "get",
                        parameters = new string[] { "value" },
                        description = "Gets the value of <value> in the Time class."
                    },
                    new CommandUsage
                    {
                        subcommand = "set",
                        parameters = new string[] { "field", "value" },
                        description = "Sets <field> in the Time class to <value>."
                    }
                }
            );
        }

        public override bool Execute(string[] args)
        {
            if (args.Length == 0)
            {
                output =  "time" + Environment.NewLine;
                output += "scale" + Environment.NewLine;
                return true;
            }
            if (StringEquals(args[0], "get"))
            {
                return Get(args[1]);
            }
            else if (StringEquals(args[0], "set"))
            {
                return Set(args[1], args[2]);
            }
            else
            {
                return UnrecognizedSubcommand(args[0]);
            }
        }

        private bool Get(string field)
        {
            if (StringEquals(field, "time"))
            {
                output = Time.time.ToString();
            }
            else if (StringEquals(field, "scale"))
            {
                if (!TryGetField<DeveloperConsoleConfig>(typeof(DeveloperConsoleBehavior), "config", out DeveloperConsoleConfig config)) return false;

                if (config.pausetime)
                {
                    if (!TryInvokeFunction(typeof(DeveloperConsoleBehavior), "GetUnpauseTimeScale", out float scale)) return false;
                    output = $"Currently: 0{Environment.NewLine}When unpaused: {scale.ToString()}";
                }
                else output = Time.timeScale.ToString();
            }
            else
            {
                return UnrecognizedArgument(field);
            }
            return true;
        }
        private bool Set(string field, string value)
        {
            if (StringEquals(field, "time"))
            {
                return ReturnError("Cannot set time in class Time");
            }
            else if (StringEquals(field, "scale"))
            {
                if (!TryCast<float>(value, out float scale)) return false;
                if (!TrySetField(typeof(DeveloperConsoleBehavior), "timeScaleOnPause", scale)) return false;

                if (!TryGetField<DeveloperConsoleConfig>(typeof(DeveloperConsoleBehavior), "config", out DeveloperConsoleConfig config)) return false;

                if (!config.pausetime)
                {
                    Time.timeScale = scale;
                }
                else if (!TryInvokeFunction(typeof(DeveloperConsoleBehavior), "OverrideUnpauseTimeScale", new object[] { scale })) return false;

                return true;
            }
            return UnrecognizedArgument(field);
        }
    }
}