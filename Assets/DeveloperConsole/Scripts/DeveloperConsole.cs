using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DeveloperConsole
{
    public class DeveloperConsole
    {
        public List<Command> commands { get; private set; } = new List<Command>();
        public DeveloperConsole()
        {
            var commandTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(Command)) && !type.IsAbstract)
                .ToList();
            
            foreach (var type in commandTypes) 
            {
                var instance = (Command)Activator.CreateInstance(type);
                commands.Add(instance);
            }

            // Check for colliding command words
            Dictionary<string, Command> set = new();
            foreach (var command in commands)
            {
                foreach (var word in command.CommandWords())
                {
                    if (set.ContainsKey(word))
                    {
                        Debug.LogError($"Commands {command.Name()} and {set[word].Name()} both use '{word}' as an invoking command word. This will lead to unpredicable behavior.");
                    }
                    else
                    {
                        set[word] = command;
                    }
                }
            }
        }

        /// <summary>
        /// Processes the input on the command line.
        /// </summary>
        /// <param name="chunkedInput">The input coming directly from the command line.</param>
        /// <returns>
        /// A message to display in the console as a result of processing the input. 
        /// This could the output from a command or an error message.
        /// </returns>
        public string ProcessCommand(string[] chunkedInput)
        {
            if (chunkedInput.Length == 0) return string.Empty;

            string commandWord = chunkedInput[0];
            string[] args = chunkedInput.Skip(1).ToArray();

            Command command = FindCommand(commandWord);

            if (command == null)
            {
                return MessageFormatter.CreateErrorMessage($"Unrecognized command {commandWord}.");
            }

            bool validation = command.Validate(args);
            string message = new string(command.Output());
            
            if (!validation)
            {
                command.Reset();
                return MessageFormatter.CreateErrorMessage(message);
            }

            bool success = command.Execute(args);

            message = new string(command.Output());
            command.Reset();

            return success ? message : MessageFormatter.CreateErrorMessage(message);
        }

        /// <summary>
        /// Finds a command associated with the given command word.
        /// </summary>
        /// <param name="commandWord">The command word.</param>
        /// <returns>The command if one exists, otherwise null.</returns>
        public Command FindCommand(string commandWord)
        {
            foreach (Command command in commands)
            {
                if (command.IsCommandWord(commandWord))
                {
                    return command;
                }
            }
            return null;
        }
    }
}
