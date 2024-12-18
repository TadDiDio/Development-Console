using System.Linq;
using System.Collections.Generic;

namespace DeveloperConsole
{
    public class AliasCommand : Command
    {
        public AliasCommand()
        {
            // Fill with all valid invoking command words
            commandWords = new string[]
            {
                "alias"
            };

            help = new CommandHelp
            (
                "Alias",
                "Creates a session long alias for a command",
                commandWords,
                new List<CommandUsage>
                {
                    new CommandUsage
                    {
                        parameters = new string[] { "alias", "arg1", "arg2", "arg3", "..." },
                        description = "Reads all args as shown when alias is seen."
                    }
                }
            );
        }

        public override bool Execute(string[] args)
        {
            object[] parameters = new object[]
            {
                args[0],
                string.Join(" ", args.Skip(1))
            };

            FunctionResult result = InvokeFunction(typeof(DeveloperConsoleBehavior), "AddAlias", parameters);

            if (!result.success)
            {
                return ReturnError(result);
            }
            return true;
        }
    }
}