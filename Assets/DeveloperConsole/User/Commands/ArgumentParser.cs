using System;
using System.Collections.Generic;
using Unity.Loading;
using UnityEngine;

namespace DeveloperConsole
{
    public class ArgumentParser
    {
        private List<List<Type>> configs = new List<List<Type>>();
        private bool allowNoArgs = false;

        public ArgumentParser(bool allowNoArgs)
        {
            this.allowNoArgs = allowNoArgs;
        }
        public void AddArgList(List<Type> config)
        {
            configs.Add(config);
        }

        public ParseResult Validate(string[] args)
        {
            ParseResult result = ParseResult.Success;

            if (args.Length == 0)
            {
                if (allowNoArgs) return result;
                return ParseResult.LengthError;
            }

            foreach (var config in configs)
            {
                bool next = false;
                if (args.Length != config.Count)
                {
                    result = ParseResult.LengthError;
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
                        result = ParseResult.TypeError;
                        break;
                    }
                }

                if (next) continue;

                return ParseResult.Success;
            }

            return result;
        }
    }
    public enum ParseResult
    {
        Success,
        LengthError,
        TypeError
    }
}
