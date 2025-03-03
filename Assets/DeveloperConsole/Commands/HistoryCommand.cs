using System;
using System.Collections.Generic;

namespace DeveloperConsole
{
    public class HistoryCommand : Command
    {
        public HistoryCommand()
        {
            commandWords = new string[]
            {
                "hist",
                "history"
            };

            help = new CommandHelp
            (
                "history",
                "Allows for seeing and modifying command history.",
                commandWords,
                new List<CommandUsage>
                {
                    new CommandUsage
                    {
                        description = "Shows the command history."
                    },
                    new CommandUsage
                    {
                        description = "Sets the command line to the command at <index> in history.",
                        parameters = new string[] { "index" }
                    },
                    new CommandUsage
                    {
                        subcommand = "clear",
                        description = "Clears the command history."
                    },
                    new CommandUsage
                    {
                        subcommand = "max",
                        description = "Sets the max history to <max>.",
                        parameters = new string[] {"max"}
                    }
                }
            );
        }

        public override bool Execute(string[] args)
        {
            if (args.Length == 0)
            {
                return ShowHistory();
            }
            else if (StringEquals(args[0], "clear"))
            {
                return ClearHistory();
            }
            else if (args.Length == 2)
            {
                return SetMax(args[0], args[1]);
            }
            else
            {
                return SetCommandLine(args[0]);
            }
        }

        bool ShowHistory()
        {
            if (!TryGetField(typeof(DeveloperConsoleBehavior), "config", out DeveloperConsoleConfig config)) return false;

            string[] lines = new string[config.commandHistory.Count];
            for (int i = 0; i <  config.commandHistory.Count; i++)
            {
                lines[i] = $"[{i}] : {config.commandHistory[i]}";
            }

            Array.Reverse(lines);
            
            output = MessageFormatter.FromLines(MessageFormatter.Align(lines));
            return true;
        }

        private bool ClearHistory()
        {
            if (!TryGetField(typeof(DeveloperConsoleBehavior), "config", out DeveloperConsoleConfig config)) return false;
            config.commandHistory.Clear();
            return true;
        }
        private bool SetMax(string subcommand, string maximum)
        {
            if (!StringEquals(subcommand, "max")) return UnrecognizedSubcommand(subcommand);
            if (!TryCast(maximum, out int max)) return false;
            if (!TryInvokeFunction(typeof(DeveloperConsoleBehavior), "SetMaxHistory", out bool success, new object[] { max })) return false;

            if (!success) output = "The new max must be 0 or higher.";
            
            return success;
        }
        private bool SetCommandLine(string index)
        {
            if (!TryCast(index, out int idx)) return false;
            if (!TryInvokeFunction(typeof(DeveloperConsoleBehavior), "SetCMDToHistory", out bool success, new object[] {idx})) return false;

            if (!success) output = $"Index {idx} was out of range.";
            
            return success;
        }
    }
}