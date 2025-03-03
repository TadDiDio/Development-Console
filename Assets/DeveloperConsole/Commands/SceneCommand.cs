using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace DeveloperConsole
{
    public class SceneCommand : Command
    {
        public SceneCommand()
        {
            // Fill with all valid invoking command words
            commandWords = new string[]
            {
                "scene"
            };

            // This block must be filled out or the command will not execute
            help = new CommandHelp
            (
                "Scene",
                "Allows you to manipulate scenes.",
                commandWords,
                new List<CommandUsage>
                {
                    new CommandUsage
                    {
                        description = "Prints a list of all scenes in build settings.",
                    },
                    new CommandUsage
                    {
                        subcommand = "reload",
                        description = "Reloads the current scene.",
                    },
                    new CommandUsage
                    {
                        subcommand = "set",
                        parameters = new string[] {"sceneName"},
                        description = "Sets the scene to <sceneName>."
                    },
                }
            );
        }

        public override bool Execute(string[] args)
        {
            if (args.Length == 0)
            {
                output = $"Registered scenes are:{Environment.NewLine}";

                for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    string name = SceneUtility.GetScenePathByBuildIndex(i);
                    name = Path.GetFileNameWithoutExtension(name);
                    output += MessageFormatter.AddColor(name, MessageFormatter.Blue) + Environment.NewLine;
                }

                return true;
            }
            if (args.Length == 1)
            {
                if (!StringEquals(args[0], "reload")) return UnrecognizedSubcommand(args[0]);

                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            if (args.Length == 2)
            {
                if (!StringEquals(args[0], "set"))
                {
                    output = $"Invalid subcommand {args[0]}.";
                    return false;
                }
                else if (TryCast(args[1], out string sceneName))
                {
                    int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);

                    if (buildIndex < 0)
                    {
                        output = $"Parameter {args[1]} could not be resolved to a valid scene name or index. Registered scenes are:{Environment.NewLine}";

                        for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                        {
                            string name = SceneUtility.GetScenePathByBuildIndex(i);
                            name = Path.GetFileNameWithoutExtension(name);
                            output += MessageFormatter.AddColor(name, MessageFormatter.Blue) + Environment.NewLine;
                        }
                        return false;
                    }
                    SceneManager.LoadScene(sceneName);
                }
                else
                {
                    output = $"Parameter {args[1]} could not be resolved to a valid scene name or index. Is the target scene registered in build settings?";
                    return false;
                }
            }

            return true;
        }
    }
}