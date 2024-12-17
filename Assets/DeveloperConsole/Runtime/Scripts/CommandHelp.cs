using System;
using UnityEngine;
using System.Collections.Generic;

namespace DeveloperConsole
{
    /// <summary>
    /// Stores metadata about a command.
    /// </summary>
    public class CommandHelp
    {
        private string help = "Uninitialized. Please set the help message for this command.";
        private string name = string.Empty;
        private string description = string.Empty;
 
        /// <summary>
        /// Creates and stores metadata about a command.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command.</param>
        /// <param name="args">A list of associated arguments for the command.</param>
        public CommandHelp(string name, string description, List<HelpArg> args)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(description))
            {
                Debug.LogWarning($"Some part of the help documentation for {name} is null or empty. The command help will not be displayed.");
                return;
            }

            this.name = name;
            this.description = description;

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

                
                help += "[" + MessageFormatter.AddColor(arg.name.ToLower(), MessageFormatter.LightBlue) + ": " + arg.type.ToLower() + "] " + arg.description;

                if (!arg.description.EndsWith('.'))
                {
                    help += '.';
                }
                help += newLine;
            }
        }

        /// <summary>
        /// Gets a formatter help string for this command to display.
        /// </summary>
        /// <returns>The formatted help message.</returns>
        public string Help() => help;

        /// <summary>
        /// Gets the name of the related command.
        /// </summary>
        /// <returns>The name.</returns>
        public string Name() => name;

        /// <summary>
        /// Gets a description of the related command.
        /// </summary>
        /// <returns>The description.</returns>
        public string Description() => description;
    }

    public struct HelpArg
    {
        public string name;
        public string type;
        public string description;
    }

}