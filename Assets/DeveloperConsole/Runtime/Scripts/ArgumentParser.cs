using System;
using System.Collections.Generic;

namespace DeveloperConsole
{
    /// <summary>
    /// Validates arguments.
    /// </summary>
    public class ArgumentParser
    {
        private List<List<Type>> configs = new List<List<Type>>();
        private bool allowNoArgs = false;

        /// <summary>
        /// Constructs a new arg parser.
        /// </summary>
        /// <param name="allowNoArgs">Whether to allow no argument inputs.</param>
        public ArgumentParser(bool allowNoArgs)
        {
            this.allowNoArgs = allowNoArgs;
        }

        /// <summary>
        /// Adds a list of valid argument types to check for.
        /// </summary>
        /// <param name="config">A list of Types which need to be accepted in order.</param>
        public void AddArgList(List<Type> config)
        {
            configs.Add(config);
        }

        /// <summary>
        /// Validates whether the given arguments match a registered arg list in length and can be casted to the given type.
        /// </summary>
        /// <param name="args">The arguemnts from the command line.</param>
        /// <returns>A result holding data about the operation.</returns>
        public ArgParseResult Validate(string[] args)
        {
            ArgParseResult result = ArgParseResult.Success;

            if (args.Length == 0)
            {
                if (allowNoArgs) return result;
                return ArgParseResult.LengthError;
            }

            foreach (var config in configs)
            {
                bool next = false;
                if (args.Length != config.Count)
                {
                    result = ArgParseResult.LengthError;
                    continue;
                }

                for (int i = 0; i < args.Length; i++)
                {
                    try
                    {
                        Convert.ChangeType(args[i], config[i]);
                    }
                    catch
                    {
                        next = true;
                        result = ArgParseResult.TypeError;
                        break;
                    }
                }

                if (next) continue;

                return ArgParseResult.Success;
            }

            return result;
        }
    }

    /// <summary>
    /// Holds information about the result of an argument parse operation.
    /// </summary>
    public enum ArgParseResult
    {
        Success,
        LengthError,
        TypeError
    }
}
