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
                        description = "Shows <field> or calls <func->string> in <instance> (type <type>) with <title> in slot 0.",
                        parameters = new string[] { "type", "instance", "field/func", "title"}
                    },
                    new CommandUsage
                    {
                        description = "Shows <field> or calls <func->string> in <instance> (type <type>) with <title> in slot <x>.",
                        parameters = new string[] { "type", "instance", "field/func", "title", "x"}
                    }
                }
            );
        }

        public override bool Execute(string[] args)
        {
            if (!TryGetField(typeof(DeveloperConsoleBehavior), "debugSlots", out DebugSlot[] slots)) return false;

            if (args.Length == 2)
            {
                if (!StringEquals(args[0], "clear"))
                {
                    output = $"Unrecognize subcommand {args[0]}.";
                    return false;
                }

                if (!TryCast(args[1], out int slot))
                {
                    return false;
                }

                slots[slot].SetEvaluation(() => "");
                return true;
            }

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

            bool foundField = TryGetField(Type.GetType(args[0]), args[2], out object field, args[1]);
            bool foundFunction = TryInvokeFunction(Type.GetType(args[0]), args[2], out object ret, instanceName: args[1]);

            // Clear output internally set from Try functions because we don't want it to print with a return true
            output = "";

            if (foundField)
            {
                eval = () =>
                {
                    TryGetField(Type.GetType(args[0]), args[2], out object field, args[1]);
                    return $"{args[3]}: {field.ToString()}";
                };
            }
            else if (foundFunction)
            {
                eval = () =>
                {
                    TryInvokeFunction(Type.GetType(args[0]), args[2], out object ret, instanceName: args[1]);
                    return $"{args[3]}: {ret.ToString()}";
                };
            }
            else
            {
                output = $"No field or function with name {args[2]} found in {args[1]} of type {args[0]}";
                return false;
            }

            slots[index].SetEvaluation(eval);
            return true;
        }
    }
}