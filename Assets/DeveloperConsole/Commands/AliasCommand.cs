using System.Linq;
using System.Collections.Generic;
using System;

namespace DeveloperConsole
{
    public class AliasCommand : Command
    {
        public AliasCommand()
        {
            // Fill with all valid invoking command words
            commandWords = new string[]
            {
                "alias",
            };

            help = new CommandHelp
            (
                "Alias",
                "Creates a session long alias for a command",
                commandWords,
                new List<CommandUsage>
                {
                    new CommandUsage
                    {
                        parameters = new string[] { "alias", "arg1", "arg2", "arg3", "..." },
                        description = "Reads all args as shown when alias is seen."
                    },
                    new CommandUsage
                    {
                        subcommand = "remove",
                        parameters = new string[] { "alias", },
                        description = "Removes the alias from <alias> if there is one."
                    },
                    new CommandUsage
                    {
                        subcommand = "list",
                        description = "Lists all current aliases."
                    }
                }
            );
        }

        public override bool Execute(string[] args)
        {
            if (StringEquals(args[0], "remove"))
            {
                object[] p1 = new object[]
                {
                    args[1]
                };

                if (!TryInvokeFunction(typeof(DeveloperConsoleBehavior), "RemoveAlias", out bool success, p1)) return false;
                if (!success)
                {
                    output = $"No alias named {args[1]}";
                }
                return true;
            }

            else if (StringEquals(args[0], "list"))
            {
                if (!TryGetField(typeof(DeveloperConsoleBehavior), "aliases", out Dictionary<string, string> aliases)) return false;

                foreach (string key in aliases.Keys)
                {
                    output += MessageFormatter.AddColor(key, MessageFormatter.LightBlue) + " = " + aliases[key] + Environment.NewLine;
                }
                return true;
            }

            object[] p2 = new object[]
            {
                args[0],
                string.Join(" ", args.Skip(1))
            };
            if (!TryInvokeFunction(typeof(DeveloperConsoleBehavior), "AddAlias", p2)) return false;

            return true;
        }
    }
}