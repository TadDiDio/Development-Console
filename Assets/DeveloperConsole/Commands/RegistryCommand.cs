using System;
using UnityEngine;
using System.Collections.Generic;

namespace DeveloperConsole
{
    public class RegistryCommand : Command
    {
        public RegistryCommand()
        {
            commandWords = new string[]
            {
                "reg",
                "registry",
                "ls"
            };

            help = new CommandHelp
            (
                "Registry",
                "Gives a list of all commands recognized by the console.",
                commandWords,
                new List<CommandUsage>
                {
                    new CommandUsage
                    {
                        description = "Prints a list of all commands to the console."
                    }
                }
            );
        }

        public override bool Execute(string[] args)
        {
            if (!TryGetField(typeof(DeveloperConsoleBehavior), "console", out DeveloperConsole console)) return false;

            output = "=====" + MessageFormatter.AddColor(" Command Registry ", MessageFormatter.Blue) + "=====" + Environment.NewLine;

            string[] lines = new string[console.commands.Count];
            for (int i = 0; i < console.commands.Count; i++)
            {
                Command command = console.commands[i];
                lines[i] = command.Name() + " : " + command.Description();
            }

            lines = MessageFormatter.Align(lines, new Color[] { MessageFormatter.Green });
            output = MessageFormatter.FromLines(lines, output, true);

            return true;
        }
    }
}