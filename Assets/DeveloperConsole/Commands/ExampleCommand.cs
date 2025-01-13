using UnityEngine;
using System.Collections.Generic;

// eep all commands in the DeveloperConsole namespace to avoid possible name collisions
namespace DeveloperConsole
{
    /**
     * This is created as an example to showcase all built in functionality that can be used, as well as which parts
     * need to be filled out in order to activate the command. Reference this file when ever you need a tutorial.
     */
    public class ExampleCommand : Command
    {
        /**
         * The constructor MUST instantiate both the commandWords array and the CommandHelp object.
         */
        public ExampleCommand()
        {
            // This is a list of all valid invoking words. Must contain at least one or your command will not be found.
            commandWords = new string[]
            {
                "example",  // This word is valid to invoke this command
                "ex"        // So is this one
            };

            // This provides documentation for your command. You MUST complete this documentation or it will not be registered.
            // In the console run `help example` to see the result of your hard documentation work!
            help = new CommandHelp
            (
                "example",                              // This is the name of the command. Typically, it should match one of the valid invoking words, but not always.
                "This command is just an example.",     // This is the general description. Do not put specific use cases here, just generally what the command handles.
                commandWords,                           // Don't change this, it just needs to be passed in so the help object is aware of all valid invoking words and can show that documentation.
                new List<CommandUsage>                  // This is the list of all possible usages. Add one CommandUsage object per use case of the command. See below for examples.
                {
                    /**
                     * This object takes in up to 3 variables:
                     * 
                     * subcommand <string>   : The subcommand word to invoke this usage.
                     * parameters <string[]> : A list of parameters that should be given.
                     * description <string>  : A specific description of this subcommand.
                     * 
                     * You only need to fill out which ones are relevant to this usage. 
                     * You should always fill out the description.
                     */
                    new CommandUsage
                    {
                        // No subcommand or parameters means this usage is used when the user simply types "example"
                        description = "Prints that you ran the example to the screen."
                    },
                    new CommandUsage
                    {
                        // No parameters means this usage is used when the user types "example" followed by the subcommand word.
                        subcommand = "time",
                        description = "Shows the current game time."
                    },
                    new CommandUsage
                    {
                        // No subcommand means this usage is used when the user types "example" followed by the given paraters
                        parameters = new string[] { "addend1", "addend2" }, // This usage is only run if the user provides exactly two arguments, it fails otherwise.
                                                                            // If you have versions that do the same thing but require different numbers of parameters,
                                                                            // either make new CommandUsages for each, or add "..." as a parameter to indicate any number 
                                                                            // of parameters are accepted.
                        description = "Adds the two numbers given together",
                    },
                    new CommandUsage
                    {
                        subcommand = "get",
                        parameters = new string[] { "field" }, // This usage will accept exactly one argument which should represent the field name.
                        description = "Shows the value of <field> in the DeveloperConsoleBehavior object in the scene."
                    },
                    new CommandUsage
                    {
                        parameters = new string[] {"arg1", "arg2", "..."}, // Include a '...' arg to indicate to the system that any number of parameters are allowed in this usecase.
                        description = "Prints all arguments to the screen"
                    }
                }
            );

            // In the console run `help example` to see the result of your hard documentation work!
        }

        /**
         * This command is called when you invoke this from the command line using one of the valid command words you added previously.
         * The args passed in already have the command word stripped away so args[0] is the first argument to the command and not the 
         * command word itself.
         */
        public override bool Execute(string[] args)
        {
            // This method is only called if the args match one of the valid CommandUsages you defined in help. 
            // At this point, we simply need to check which usage it is and run it, no error checking on args is needed.

            // output is the message you want to print to the console. If you have nothing to say, there is no need to 
            // initialize it to empty, that is already done. Just ignore it in that case.
            
            output = "Set this to your message."; // Try to remember to add periods for console output for similarity across the outputs of other commands.
                                                  // In some cases, the parser will detect this and add it for you.

            // If length is 0, it must be the default usage defined first in the CommandHelp object.
            if (args.Length == 0)
            {
                return NoArgsCase();
            }
            if (args.Length == 1)
            {
                return OneArgCase(args[0]);
            }
            if (args.Length == 2)
            {
                return TwoArgsCase(args);
            }

            // Fallback on any number of args usage
            output = string.Join(" ", args);
            return true;
        }

        private bool NoArgsCase()
        {
            output = "You just ran the example command.";

            // Return true to indicate success.
            return true;
        }
        private bool OneArgCase(string arg) 
        {
            // Always use the base class method StringEquals to compare strings since it ignores case sensitivity.
            if (StringEquals(arg, "time"))
            {
                output = $"{Time.time}";
            }
            else
            {
                // Run the last usage defined in our CommandHelp block: print this to the scree.
                output = arg;
            }

            // Either its the subcommand "time" or its the last usage which allows any number of args. Both are successes.
            return true;
        }
        private bool TwoArgsCase(string[] args)
        {
            if (StringEquals(args[0], "get"))
            {
                string fieldName = args[1];
                object value = null; // Make this of type object because in this case we don't know what its actual type will be. 
                                     // If you know ahead of time, you can just use that type instead of object.

                // Try get field is a wrapper around the base class method GetField.
                // It sets output to a helpful error message depending on what part failed and returns false if it failed.
                // You can also use GetField if you need to, but if you have the choice TryGetField is always recommended.
                if (!TryGetField(typeof(DeveloperConsoleBehavior), fieldName, out value))
                {
                    return false;
                }

                // There are also methods for TrySetField() and TryInvokeFunction, with corresponding non 'Try' versions. Hover 
                // over the names of these methods with the mouse to see more information about parameters and returns.

                // Set output to the value of the field and return success
                output = value.ToString();
                return true;
            }
            else
            {
                // Give precedence to the add functionality, then fallback to printing with any number of arguments.
                if (TryCast(args[0], out float num1) && TryCast(args[1], out float num2))
                {
                    // We can use the base class method TryCast to tell if casting the string arg to the correct value worked.
                    output = $"{num1 + num2}";
                }
                else
                {
                    output = string.Join(" ", args);
                }

                // Again, either case is acceptable.
                return true;
            }
        }
    }
}