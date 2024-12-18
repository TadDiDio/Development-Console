using System;
using System.Linq;
using UnityEngine;
using System.Reflection;
using Unity.VisualScripting;

namespace DeveloperConsole
{
    /// <summary>
    /// A base class representing commands that can be run in the Developer Console.
    /// </summary>
    public abstract class Command
    {
        /// <summary>
        /// Handles parsing arguments.
        /// </summary>
        private ArgumentParser argParser = new ArgumentParser();

        /// <summary>
        /// Generates error messages for common errors.
        /// </summary>
        private CommandErrorGenerator errorGenerator = new CommandErrorGenerator();

        /// <summary>
        /// A list of valid command words to invoke this command.
        /// </summary>
        protected string[] commandWords { get; set; }

        /// <summary>
        /// This will display a formatted help message.
        /// </summary>
        protected CommandHelp help;

        /// <summary>
        /// The output produced by the most recent call of this command. Will be reset after printing to console.
        /// </summary>
        protected string output = String.Empty;

        #region Public

        /// <summary>
        /// Runs the command.
        /// </summary>
        /// <param name="args">Arguments for the command.</param>
        /// <returns>True if execution was successful, false otherwise.</returns>
        public abstract bool Execute(string[] args);

        /// <summary>
        /// Tells if a given command word is valid for invoking this command.
        /// </summary>
        /// <param name="word">The command word.</param>
        /// <returns>True if the command word is valid.</returns>
        public bool IsCommandWord(string word)
        {
            return commandWords.Any(s => s.Equals(word, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the output that the last successful call of this command produced.
        /// </summary>
        /// <returns>The log output.</returns>
        public string Output() => output;
        
        /// <summary>
        /// Gets a help message that describes how to use this command.
        /// </summary>
        /// <returns>The help message.</returns>
        public string Help() => help.Help();

        /// <summary>
        /// Gets the name of this command.
        /// </summary>
        /// <returns>The name.</returns>
        public string Name() => help.Name();
        
        /// <summary>
        /// Gets a description of this command.
        /// </summary>
        /// <returns>The description.</returns>
        public string Description() => help.Description();

        /// <summary>
        /// Resets the meta data in the command to get ready for a new call.
        /// </summary>
        public virtual void Reset()
        {
            output = String.Empty;
        }
        #endregion

        #region Protected
        /// <summary>
        /// Invoke a function on an object in the scene.
        /// </summary>
        /// <param name="type">The type of the object.</param>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="parameters">Parameters if there are any.</param>
        /// <param name="instanceName">Name of the object in the scene if there are multiple.</param>
        /// <returns>FunctionResult which holds the return and data about the success of the call.</returns>
        protected FunctionResult InvokeFunction(Type type, string functionName, object[] parameters = null, string instanceName = null)
        {
            FunctionResult result = new FunctionResult
            {
                type = type,
                instanceName = instanceName,
                functionName = functionName,
                parameters = parameters,
                success = false,
                instancesFoundInScene = 0,
                foundInstanceByName = false,
                correctParameters = false,
                functionFound = false,
                returnValue = null
            };

            UnityEngine.Object instance = FindObjectInstance(result);
            if (instance == null) return result;

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            MethodInfo[] methods = type.GetMethods(flags)
                .Where(m => m.Name.Equals(functionName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            
            if (methods.Length == 0) return result;
            result.functionFound = true;
            
            // Verify that paramters is non null
            methods = methods
                      .Where(m => (m.GetParameters().Length == 0 && parameters == null) || 
                                   (parameters != null && m.GetParameters().Length == parameters.Length))
                      .ToArray();

            if (methods.Length == 0) return result;

            foreach (MethodInfo method in methods)
            {
                ParameterInfo[] methodParameters = method.GetParameters();
                
                // Make sure parameters match type
                for (int i = 0; i < methodParameters.Length; i++)
                {
                    if (!methodParameters[i].ParameterType.IsAssignableFrom(parameters[i].GetType()))
                    {
                        continue;
                    }
                }

                result.success = true;
                result.correctParameters = true;
                result.returnValue = method.Invoke(instance, parameters);
                return result;
            }

            return result;
        }
        
        /// <summary>
        /// Gets the value of a field of an object in the scene.
        /// </summary>
        /// <param name="type">The type of the object.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="instanceName">Name of the object in the scene if there are multiple.</param>
        /// <returns>FieldResult which holds the value and data about the success of the operation.</returns>
        protected FieldResult GetField(Type type, string fieldName, string instanceName = null)
        {
            return AccessField(type, fieldName, true, false, instanceName: instanceName);
        }
        
        /// <summary>
        /// Sets the value of a field of an object in the scene.
        /// </summary>
        /// <param name="type">The type of the object.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to set it to.</param>
        /// <param name="instanceName">Name of the object in the scene if there are multiple.</param>
        /// <param name="fromCLI">Is the value coming from the command line?</param>
        /// <returns>FieldResult which holds data about the success of the operation.</returns>
        protected FieldResult SetField(Type type, string fieldName, object value, string instanceName = null, bool fromCLI = true)
        {
            return AccessField(type, fieldName, false, fromCLI, value: value, instanceName: instanceName);
        }

        /// <summary>
        /// Sets the value of a field of an object passed in.
        /// </summary>
        /// <param name="instance">The object to set the field of.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to set the field to.</param>
        /// <param name="fromCLI">True if the value was from the command line.</param>
        /// <returns>FieldResult which holds data about the success of the operation.</returns>
        protected FieldResult SetField(object instance, string fieldName, object value, bool fromCLI = true)
        {
            // Artificially create a response to ensure that it recognizes our object instance
            FieldResult result = new FieldResult
            {
                type = null,
                success = false,
                validType = true,

                instanceName = string.Empty,
                foundInstanceByName = true,
                instancesFoundInScene = 1,
                fieldName = fieldName,
                fieldFound = false,
                value = value,
                get = false
            };

            if (instance == null)
            {
                return result;
            }

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            FieldInfo field = instance.GetType().GetField(fieldName, flags);

            if (field == null)
            {
                return result;
            }
            result.fieldFound = true;

            return TrySetField(fromCLI, value, result, instance, field);
        }

        /// <summary>
        /// Tries to cast the arg to a new type.
        /// </summary>
        /// <typeparam name="T">The type to cast to.</typeparam>
        /// <param name="arg">The argument to cast.</param>
        /// <param name="data">The sucessfully casted type.</param>
        /// <returns>True if the cast was successful, otherwise false.</returns>
        protected bool Cast<T>(string arg, out T data)
        {
            data = default(T);

            if (typeof(T).IsEnum)
            {
                try
                {
                    data = (T)Enum.Parse(typeof(T), arg);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                try
                {
                    data = (T)Convert.ChangeType(arg, typeof(T));
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Validates args and sets up output with any error message if it fails.
        /// </summary>
        /// <param name="args">The args from the command line.</param>
        /// <returns>True if the args are valid, false otherwise.</returns>
        protected bool InvalidArgs(string[] args)
        {
            if (argParser == null)
            {
                Debug.LogError($"Command type {this.GetType()} did not have an arg parser! Aborting command.");
                return true;
            }

            ArgParseResult result = argParser.Validate(args, help);

            if (result != ArgParseResult.Success)
            {
                output = errorGenerator.ParseError(result);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns false and sets the output to the input error message.
        /// </summary>
        /// <param name="error">The error message.</param>
        /// <returns>False.</returns>
        protected bool ReturnError(string error)
        {
            output = error;
            return false;
        }
        /// <summary>
        /// Returns false and sets the output to the correct error message.
        /// </summary>
        /// <param name="result">The result of a reflection call that had an error.</param>
        /// <returns>False.</returns>
        protected bool ReturnError(ReflectionResult result)
        {
            output = errorGenerator.ReflectionError(result);
            return false;
        }

        /// <summary>
        /// Compares two strings while ignoring case.
        /// </summary>
        /// <param name="s1">The first string.</param>
        /// <param name="s2">The second string.</param>
        /// <returns>True if they match, false otherwise.</returns>
        protected bool StringEquals(string s1, string s2)
        {
            return s1.Equals(s2, StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// Prints the input to the unity console.
        /// </summary>
        /// <param name="obj">The object to print.</param>
        protected void print(object obj)
        {
            if (obj == null)
            {
                Debug.Log(obj);
            }
            else
            {
                Debug.Log(obj.ToString());
            }
        }
        #endregion

        #region Private
        private FieldResult AccessField(Type type, string fieldName, bool get, bool fromCLI, object value = null, string instanceName = null)
        {
            FieldResult result = new FieldResult
            {
                type = type,
                validType = false,
                success = false,
                instanceName = instanceName,
                foundInstanceByName = false,
                instancesFoundInScene = 0,
                fieldName = fieldName,
                fieldFound = false,
                value = value,
                get = get
            };

            UnityEngine.Object instance = FindObjectInstance(result);
            if (instance == null) return result;

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            FieldInfo field = type.GetField(fieldName, flags);

            if (field == null)
            {
                return result;
            }

            result.fieldFound = true;
            if (get)
            {
                result.success = true;
                result.value = field.GetValue(instance);
                return result;
            }

            return TrySetField(fromCLI, value, result, instance, field);
        }
        private FieldResult TrySetField(bool fromCLI, object value, FieldResult result, object instance, FieldInfo field)
        {
            Type fieldType = field.FieldType;

            // Attempt to parse string
            if (fromCLI)
            {
                object convertedValue;
                string stringValue = value as string;

                if (fieldType.IsEnum)
                {
                    try
                    {
                        convertedValue = Enum.Parse(fieldType, stringValue);
                    }
                    catch
                    {
                        return result;
                    }
                }
                else
                {
                    try
                    {
                        convertedValue = Convert.ChangeType(value, fieldType);
                    }
                    catch
                    {
                        return result;
                    }
                }

                result.success = true;
                result.validValue = true;
                field.SetValue(instance, convertedValue);
            }
            else
            {
                if (!(value.GetType() == fieldType))
                {
                    return result;
                }

                result.success = true;
                result.validValue = true;
                field.SetValue(instance, value);
            }

            return result;
        }
        private UnityEngine.Object FindObjectInstance(ReflectionResult result)
        {
            result.validType = result.type.IsSubclassOf(typeof(UnityEngine.Object));
            if (!result.validType) return null;

            var instances = GameObject.FindObjectsOfType(result.type);
            result.instancesFoundInScene = instances.Length;

            if (instances.Length == 0)
            {
                return null;
            }

            var instance = instances[0];
            if (instances.Length == 1)
            {
                result.foundInstanceByName = true;
            }
            if (instances.Length > 1)
            {
                // If the instance name was not passed in, abort because there are multiple instances
                if (result.instanceName == null)
                {
                    return null;
                }

                // Search for the matching instance
                foreach (var instanceInScene in instances)
                {
                    if (instanceInScene.name == result.instanceName)
                    {
                        instance = instanceInScene;
                        result.foundInstanceByName = true;
                        break;
                    }
                }

                if (!result.foundInstanceByName)
                {
                    return null;
                }
            }

            return instance;
        }
        #endregion

        #region Types
        /// <summary>
        /// Holds data about the success of a reflection operation.
        /// </summary>
        public abstract class ReflectionResult
        {
            public bool success;
            public Type type;
            public bool validType;
            public string instanceName;
            public bool foundInstanceByName;
            public int instancesFoundInScene;
        }
        
        /// <summary>
        /// Holds data about the success of a reflection function call.
        /// </summary>
        public class FunctionResult : ReflectionResult
        {
            public string functionName;
            public object[] parameters;
            public bool functionFound;
            public bool correctParameters;
            public object returnValue;
        }
        
        /// <summary>
        /// Holds data about the success of a field accessed using reflection.
        /// </summary>
        public class FieldResult : ReflectionResult
        {
            public string fieldName;
            public bool get;
            public bool fieldFound;
            public bool validValue;
            public object value;
        }
        #endregion
    }
}
