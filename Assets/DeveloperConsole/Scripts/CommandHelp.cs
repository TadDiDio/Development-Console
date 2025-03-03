using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
        private string[] commandWords;

        private bool allowOneOrMoreArgs = false;
        private List<int> argLengths = new List<int>();

        private const string TEMP_OPEN_ANGLE_BRACKET = "§";
        private const string TEMP_CLOSE_ANGLE_BRACKET = "ô";


        /// <summary>
        /// Creates and stores metadata about a command.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command.</param>
        /// <param name="args">A list of associated arguments for the command.</param>
        public CommandHelp(string name, string description, string[] commandWords, List<CommandUsage> usages)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(description))
            {
                Debug.LogWarning($"Some part of the help documentation for {name} is null or empty. The command help will not be displayed.");
                return;
            }

            this.name = name.ToLower();
            this.description = description;
            this.commandWords = commandWords;

            string newLine = Environment.NewLine;
            string bar = "======================" + newLine;


            if (char.IsUpper(name[0])) name = char.ToLower(name[0]) + name.Substring(1);
            if (!this.description.EndsWith('.')) this.description += '.';

            help = newLine + MessageFormatter.AddColor(name, MessageFormatter.Green);
            help += " (";

            for (int i = 0; i < commandWords.Length; i++)
            {
                help += commandWords[i];
                if (i < commandWords.Length - 1)
                {
                    help += ", ";
                }
            }

            help += ")" + newLine + bar + description + newLine;

            if (usages == null || usages.Count == 0)
            {
                return;
            }

            help += newLine + "Usages:" + newLine;

            string[] usageLines = new string[usages.Count];
            for (int i = 0; i < usageLines.Length; i++)
            {
                CommandUsage usage = usages[i];
                string line = name;

                if (!string.IsNullOrEmpty(usage.subcommand))
                {
                    line += " " + MessageFormatter.AddColor(usage.subcommand.ToLower(), MessageFormatter.Blue);
                }
                
                if (usage.parameters != null)
                {
                    line += " ";
                    for (int p = 0; p < usage.parameters.Length; p++)
                    {
                        string parameter = usage.parameters[p];
                        
                        if (parameter.StartsWith("<"))
                        {
                            parameter = TEMP_OPEN_ANGLE_BRACKET + parameter.Substring(1);
                        }
                        else
                        {
                            parameter = TEMP_OPEN_ANGLE_BRACKET + parameter;
                        }
                        if (parameter.EndsWith(">"))
                        {
                            parameter = parameter.Substring(0, parameter.Length - 2) + TEMP_CLOSE_ANGLE_BRACKET;
                        }
                        else
                        {
                            parameter = parameter + TEMP_CLOSE_ANGLE_BRACKET;
                        }

                        if (parameter.Equals($"{TEMP_OPEN_ANGLE_BRACKET}...{TEMP_CLOSE_ANGLE_BRACKET}", StringComparison.OrdinalIgnoreCase))
                        {
                            allowOneOrMoreArgs = true;
                        }

                        line += parameter;

                        if (p != usage.parameters.Length - 1)
                        {
                            line += " ";
                        }
                    }
                }

                argLengths.Add(Regex.Split(line.Trim(), @"\s+").Length - 1);
                usageLines[i] = line;
            }

            // Remove HTML tags so we don't include those in the perceived length
            string[] strippedLines = usageLines.ToArray();
            for (int i = 0; i < usageLines.Length; i++)
            {
                if (string.IsNullOrEmpty(usageLines[i]))
                {
                    Debug.LogWarning("Found an empty usage line when initializing the developer console.");
                    strippedLines[i] = usageLines[i];
                }
                else
                {
                    strippedLines[i] = Regex.Replace(usageLines[i], @"<.*?>", string.Empty);
                }
            }

            // Replace temporary tags
            for (int i = 0; i < usageLines.Length; i++)
            {
                usageLines[i] = Regex.Replace(usageLines[i], $@"{TEMP_OPEN_ANGLE_BRACKET}", "<");
                usageLines[i] = Regex.Replace(usageLines[i], $@"{TEMP_CLOSE_ANGLE_BRACKET}", ">");
            }

            int longest = strippedLines.OrderByDescending(s => s.Length).First().Length;

            for (int i = 0; i < usageLines.Length; i++)
            {
                int bufferSpace = longest - strippedLines[i].Length;
                
                string buffer = bufferSpace > 0 ? new string(' ', bufferSpace) : string.Empty;
                
                usageLines[i] += buffer;
            }

            for (int i = 0; i < usageLines.Length; i++)
            {
                CommandUsage usage = usages[i];
                usageLines[i] += " : ";

                string usageDescription = usage.description;
                if (!string.IsNullOrEmpty(usage.description))
                {
                    if (!char.IsUpper(usage.description[0]))
                    {
                        usageDescription = char.ToUpper(usage.description[0]) + usage.description.Substring(1);
                    }
                }
                else
                {
                    usageDescription = MessageFormatter.AddColor("Please add documentation for this usage in the CommandHelp constructor for this command.", MessageFormatter.Red);
                }

                usageLines[i] += usageDescription;
                
                if (!usageLines[i].EndsWith('.'))
                {
                    usageLines[i] += '.';
                }
            }

            foreach (string line in usageLines)
            {
                help += line + newLine;
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

        /// <summary>
        /// Gets all valid invoking words for this command.
        /// </summary>
        /// <returns>The invoking words.</returns>
        public string[] CommandWords() => commandWords;

        /// <summary>
        /// Returns an array containing all valid numbers of arguments for this command.
        /// </summary>
        /// <returns></returns>
        public List<int> GetArgLengths()
        {
            return argLengths;
        }

        /// <summary>
        /// Returns whether one or more args is valid.
        /// </summary>
        /// <returns>True if one or more args are all valid, false otherwise.</returns>
        public bool AllowOneOrMoreArgsLength()
        {
            return allowOneOrMoreArgs;
        }
    }

    /// <summary>
    /// Holds information about how to use a command.
    /// </summary>
    public struct CommandUsage
    {
        public string subcommand;
        public string[] parameters;
        public string description;

        /// <summary>
        /// Creates a new command usage.
        /// </summary>
        /// <param name="invokeWord">The word to invoke this sub command.</param>
        /// <param name="parameters">The parameters like names or numbers or values.</param>
        /// <param name="description">The description of what this subcommand does.</param>
        public CommandUsage(string invokeWord, string[] parameters, string description)
        {
            this.subcommand = invokeWord;
            this.parameters = parameters;
            this.description = description;
        }
    }
}