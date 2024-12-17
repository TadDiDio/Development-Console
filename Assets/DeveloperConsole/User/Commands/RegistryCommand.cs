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
                "registry"
            };

            argParser = new ArgumentParser(true);

            help = new CommandHelp
            (
                "Registry",
                "Gives a list of all commands recognized by the console.",
                new List<HelpArg>{}
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

            DeveloperConsole console = (DeveloperConsole)result.value;

            output = "=====" + MessageFormatter.AddColor(" Command Registry ", MessageFormatter.LightBlue) + "=====" + Environment.NewLine;

            string[] lines = new string[console.commands.Count];
            for (int i = 0; i < console.commands.Count; i++)
            {
                Command command = console.commands[i];
                lines[i] = command.Name() + " : " + command.Description();
            }

            lines = MessageFormatter.Align(lines, new Color[] { Color.green });
            output = MessageFormatter.FromLines(lines, output, true);

            return true;
        }
    }
}