using System;
using System.Reflection;
using System.Collections.Generic;

namespace DeveloperConsole
{
    public class DebugCommand : Command
    {
        public DebugCommand()
        {
            // Fill with all valid invoking command words
            commandWords = new string[]
            {
                "debug"
            };

            // This block must be filled out or the command will not execute
            help = new CommandHelp
            (
                "debug",
                "Debugs a live value to the screen.",
                commandWords,
                new List<CommandUsage>
                {
                    new CommandUsage
                    {
                        subcommand = "clear",
                        description = "clears <slot>.",
                        parameters = new string[] {"slot"}
                    },
                    new CommandUsage
                    {
                        description = "Shows <field> or calls <func->string> in <instance> (type <type>) with <title> in slot 0. Use _ to search in first instance found.",
                        parameters = new string[] { "type", "instance", "field/func", "title"}
                    },
                    new CommandUsage
                    {
                        description = "Shows <field> or calls <func->string> in <instance> (type <type>) with <title> in slot <x>. Use _ to search in first instance found.",
                        parameters = new string[] { "type", "instance", "field/func", "title", "x"}
                    }
                }
            );
        }

        public override bool Execute(string[] args)
        {
            if (!TryGetField(typeof(DeveloperConsoleBehavior), "debugSlots", out DebugSlot[] slots)) return false;

            string type = args[0];
            string instance = args[1];

            if (args.Length == 2)
            {
                if (!StringEquals(type, "clear"))
                {
                    output = $"Unrecognize subcommand {type}.";
                    return false;
                }

                if (!TryCast(instance, out int slot))
                {
                    return false;
                }

                slots[slot].SetEvaluation(() => "");
                return true;
            }

            string fieldFuncName = args[2];
            string title = args[3];

            int index = 0;
            if (args.Length == 5)
            {
                if (!TryCast(args[4], out int slotIndex))
                {
                    return false;
                }
                if (slotIndex < 0 || slotIndex > 7)
                {
                    output = $"Index {slotIndex} is invalid. Only 0-7 are allowed.";
                    return false;
                }

                index = slotIndex;
            }

            Func<string> eval = () => "";

            bool foundField = false;
            bool foundFunction = false;
            bool firstInstance = instance.Equals("_");

            if (firstInstance)
            {
                foundField = TryGetField(GetType(type), fieldFuncName, out object field);
                foundFunction = TryInvokeFunction(GetType(type), fieldFuncName, out object ret);
            }
            else
            {
                foundField = TryGetField(GetType(type), fieldFuncName, out object field, instanceName: instance);
                foundFunction = TryInvokeFunction(GetType(type), fieldFuncName, out object ret, instanceName: instance);
            }

            // Clear output internally set from Try functions because we don't want it to print with a return true
            output = "";
            if (foundField)
            {
                if (firstInstance)
                {
                    eval = () =>
                    {
                        TryGetField(GetType(type), fieldFuncName, out object field);
                        return $"{title}: {field.ToString()}";
                    };
                }
                else
                {
                    eval = () =>
                    {
                        TryGetField(GetType(type), fieldFuncName, out object field, instance);
                        return $"{title}: {field.ToString()}";
                    };
                }
            }
            else if (foundFunction)
            {
                if (firstInstance)
                {
                    eval = () =>
                    {
                        TryInvokeFunction(GetType(type), fieldFuncName, out object ret);
                        return $"{title}: {ret.ToString()}";
                    };
                }
                else
                {
                    eval = () =>
                    {
                        TryInvokeFunction(GetType(type), fieldFuncName, out object ret, instanceName: instance);
                        return $"{title}: {ret.ToString()}";
                    };
                }
            }
            else
            {
                output = $"No field or function with name {fieldFuncName} found in {instance} of type {type}";
                return false;
            }

            slots[index].SetEvaluation(eval);
            return true;
        }

        private Type GetType(string name)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Loop through each assembly
            foreach (var assembly in assemblies)
            {
                // Get all types in the current assembly
                Type[] types = assembly.GetTypes();

                // Search for a type matching the typeName in the current assembly
                foreach (var type in types)
                {
                    if (StringEquals(type.Name, name))
                    {
                        return type;  // Type found, return it
                    }
                }
            }

            return null;
        }
    }
}