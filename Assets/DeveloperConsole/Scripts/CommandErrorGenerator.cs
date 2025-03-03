using static DeveloperConsole.Command;

namespace DeveloperConsole
{
    public class CommandErrorGenerator
    {
        /// <summary>
        /// Get an error message that describes why a reflection operation failed.
        /// </summary>
        /// <param name="result">The result of a failed reflection operation.</param>
        /// <returns>An error message.</returns>
        public string ReflectionError(ReflectionResult result)
        {
            if (result.success) return string.Empty;

            if (!result.validType) return $"Type {result.type} is not valid for this command. The type must derrive from UnityEngine.Object.";
            if (result.instancesFoundInScene == 0) return $"No instance of type {result.type} was found in the scene.";
            if (!result.foundInstanceByName) return $"No instance with name {result.instanceName} was found in the scene.";

            if (result is FunctionResult function)
            {
                if (!function.functionFound) return $"No function with name {function.functionName} was found in type {result.type}.";
                if (!function.correctParameters) return $"No overload for function {function.functionName} was found matching these parameters.";
            }
            else if (result is FieldResult field)
            {
                if (!field.fieldFound) return $"No field with name {field.fieldName} was found in type {result.type}.";
                if (!field.validValue) return $"The argument ${field.value} could not be cast to the correct type";
            }

            return "Something when wrong.";
        }

        /// <summary>
        /// Get an error message that describes why a parse failed.
        /// </summary>
        /// <param name="result">The result of the parse attempt.</param>
        /// <returns>An error message.</returns>
        public string ParseError(ArgParseResult result)
        {
            if (result == ArgParseResult.Success) return string.Empty;
            if (result == ArgParseResult.LengthError) return "Incorrect number of arguments.";
            if (result == ArgParseResult.UninitializedHelp) return "The help block for this command was uninitialized. See the Unity console.";

            return string.Empty;
        }
    }
}
