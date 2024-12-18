using System;
using System.Collections.Generic;
using UnityEngine;

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
                        invokeWord = "get",
                        parameters = new string[] { "value" },
                        description = "Gets the value of the given parameter."
                    },
                    new CommandUsage
                    {
                        invokeWord = "set",
                        parameters = new string[] { "field", "value" },
                        description = "Sets field to value."
                    }
                }
            );
        }

        public override bool Execute(string[] args)
        {
            if (InvalidArgs(args)) return false;

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
                return ReturnError($"Unrecognized argument {args[0]}");
            }
        }

        private bool Get(string field)
        {
            if (StringEquals(field, "time"))
            {
                output = Time.time + "";
            }
            else if (StringEquals(field, "scale"))
            {
                output = Time.timeScale + "";
            }
            else
            {
                return ReturnError($"Unrecognized field {field}");
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
                if (Cast<float>(value, out float scale))
                {
                    FieldResult result = SetField(typeof(DeveloperConsoleBehavior), "timeScaleOnPause", scale);
                    if (!result.success)
                    {
                        return ReturnError(result);
                    }

                    result = GetField(typeof(DeveloperConsoleBehavior), "config");
                    if (!result.success)
                    {
                        return ReturnError(result);
                    }

                    if (!((DeveloperConsoleConfig)result.value).pausetime)
                    {
                        Time.timeScale = scale;
                    }
                    return true;
                }
                return ReturnError($"Could not cast {value} to a float");
            }
            else
            {
                return ReturnError($"Unrecognized field {field}");
            }
        }
    }
}