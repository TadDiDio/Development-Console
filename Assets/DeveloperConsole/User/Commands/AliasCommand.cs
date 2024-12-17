using System;
using System.Collections.Generic;
using System.Linq;

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
                "new command name",
                "new command description.",
                new List<HelpArg>
                {
                    new HelpArg
                    {
                        name = "arg1 name",
                        type = "arg1 type",
                        description = "arg1 description",
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
                output = ErrorGenerator.ReflectionError(result);
            }
            return true;
        }
    }
}