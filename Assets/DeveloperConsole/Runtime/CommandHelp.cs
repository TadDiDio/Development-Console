using System;
using UnityEngine;
using System.Collections.Generic;

namespace DeveloperConsole
{
    public class CommandHelp
    {
        private string help = "Uninitialized. Please set the help message for this command.";

        public CommandHelp(string name, string description, List<HelpArg> args)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(description))
            {
                Debug.LogWarning($"Some part of the help documentation for {name} is null or empty. The command help will not be displayed.");
                return;
            }

            string newLine = Environment.NewLine;
            string bar = "======================" + newLine;

            if (!char.IsUpper(name[0])) name = char.ToUpper(name[0]) + name.Substring(1);
            if (!description.EndsWith('.')) description += '.';

            help = newLine + MessageFormatter.AddColor(name, Color.green) + newLine + bar + description + newLine + newLine;

            if (args.Count > 0)
            {
                help += "Args:" + newLine;
            }

            foreach (var arg in args)
            {
                if (string.IsNullOrEmpty(arg.name))
                {
                    Debug.LogWarning($"An argument in the help documentation for {name} is null and will not be displayed.");
                    continue;
                }

                ColorUtility.TryParseHtmlString("#00A8DC", out Color color);
                help += "[" + MessageFormatter.AddColor(arg.name.ToLower(), color) + ": " + arg.type.ToLower() + "] " + arg.description;

                if (!arg.description.EndsWith('.'))
                {
                    help += '.';
                }
                help += newLine;
            }
        }

        public string Help() => help;
    }

    public struct HelpArg
    {
        public string name;
        public string type;
        public string description;
    }

}