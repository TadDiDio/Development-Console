using UnityEngine;
using System.Collections.Generic;

namespace DeveloperConsole
{
    /// <summary>
    /// Validates arguments based on the help description set up by the command constructor.
    /// </summary>
    public class ArgumentParser
    {
        /// <summary>
        /// Validates that the length of the arguments passed in is correct. This does not 
        /// do type validation.
        /// </summary>
        /// <param name="args">The command line args.</param>
        /// <param name="help">The command's help object.</param>
        /// <returns>ArgParseResult which tells if it succeeded or not with information.</returns>
        public ArgParseResult Validate(string[] args, CommandHelp help)
        {
            if (help == null)
            {
                Debug.LogError($"Command type {this.GetType()} was called before it's CommandHelp was initialized. " +
                    $"Filling out the help field object in the constructor of the command is enforced to keep a maintainable system." +
                    $"It is also used to validate arguments, so make sure to fill out every use case fully. Happy coding :D");
                return ArgParseResult.UninitializedHelp;
            }

            List<int> argLengths = help.GetArgLengths();

            if (argLengths == null || argLengths.Count == 0)
            {
                Debug.LogError($"There are no usages defined in the help block for command {help.Name()}." +
                    $"Filling out the help field object in the constructor of the command is enforced to keep a maintainable system." +
                    $"It is also used to validate arguments, so make sure to fill out every use case fully. Happy coding :D");
                return ArgParseResult.UninitializedHelp;
            }

            if (args.Length > 0 && help.AllowOneOrMoreArgsLength()) return ArgParseResult.Success;
            if (!argLengths.Contains(args.Length)) return ArgParseResult.LengthError;
            return ArgParseResult.Success;
        }
    }

    /// <summary>
    /// Holds information about the result of an argument parse operation.
    /// </summary>
    public enum ArgParseResult
    {
        Success,
        LengthError,
        UninitializedHelp
    }
}
