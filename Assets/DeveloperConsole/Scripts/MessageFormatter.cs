using System;
using UnityEngine;
using Unity.VisualScripting;
using System.Text.RegularExpressions;

namespace DeveloperConsole
{
    /// <summary>
    /// Handles formatting strings, primarily for console output.
    /// </summary>
    public static class MessageFormatter
    {
        /// <summary>
        /// Light blue.
        /// </summary>
        public static Color LightBlue
        {
            get
            {
                Color color = Color.white;
                UnityEngine.ColorUtility.TryParseHtmlString("#00A8DC", out color);
                return color;
            }
            private set
            {

            }
        }

        /// <summary>
        /// Prepends the input with a white log tag.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <returns>The formatted warning message.</returns>
        public static string CreateLogMessage(string message)
        {
            return ("[Log] " + message);
        }

        /// <summary>
        /// Prepents the input with a yellow warning tag.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <returns>The formatted warning message.</returns>
        public static string CreateWarningMessage(string message)
        {
            return AddColor("[Warning] ", Color.yellow) + message;
        }

        /// <summary>
        /// Prepends the input with a red error tag.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>The formatted error message.</returns>
        public static string CreateErrorMessage(string message)
        {
            return AddColor("[Error] ", Color.red) + message;
        }

        /// <summary>
        /// Changes the text to have a color.
        /// </summary>
        /// <param name="text">The text to change.</param>
        /// <param name="color">The color to make the text.</param>
        /// <returns>The colored text.</returns>
        public static string AddColor(string text, Color color)
        {
            string hexcode = color.ToHexString();
            return $"<color=#{hexcode}>" + text + "</color>";
        }

        /// <summary>
        /// Formates an array of lines into an aligned grid on the first column. Optionally also adds colors to rows or columns.
        /// </summary>
        /// <param name="lines">The lines to align.</param>
        /// <param name="colors">Colors to assign in order.</param>
        /// <param name="columnColors">Assign colors to columns or rows.</param>
        /// <returns>Array of formatted lines.</returns>
        public static string[] Align(string[] lines, Color[] colors = null, bool columnColors = true)
        {
            int longestWord = 0;
            for (int j = 0; j < lines.Length; j++)
            {
                string[] line = Regex.Split(lines[j].Trim(), @"\s+");
                    
                if (line.Length == 0) continue;

                longestWord = Mathf.Max(line[0].Length, longestWord);
            }

            for (int j = 0; j < lines.Length; j++)
            {
                string[] line = Regex.Split(lines[j].Trim(), @"\s+");
                if (line.Length == 0) continue;

                int length = line[0].Length;
                int bufferSize = longestWord - length;
                   
                if (bufferSize > 0)
                {
                    string buffer = new string(' ', bufferSize);
                    line[0] = line[0] + buffer;
                    lines[j] = string.Join(" ", line);
                }
            }

            if (colors == null) return lines;
            
            for (int index = 0; index < colors.Length; index++)
            {
                Color color = colors[index];

                if (columnColors)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        // Match words followed by optional spaces
                        var matches = Regex.Matches(lines[i], @"\S+\s*(</[^>]+>)?");
                        string[] line = new string[matches.Count];

                        // Convert matched results to string array
                        for (int j = 0; j < matches.Count; j++)
                        {
                            line[j] = matches[j].Value;
                        }

                        if (index >= line.Length) continue;
                        line[index] = AddColor(line[index], color);
                        lines[i] = string.Join("", line);
                    }
                }
                else
                {
                    if (index >= lines.Length) return lines;
                    lines[index] = AddColor(lines[index], colors[index]);
                }
            }
            
            return lines;
        }

        /// <summary>
        /// Get a single string from an array of lines.
        /// </summary>
        /// <param name="lines">The lines to concatonate.</param>
        /// <param name="prepend">An optional string to prepend.</param>
        /// <param name="newLineAtEnd">Whether to add an extra new line at the end.</param>
        /// <returns>A single string with all concatonated lines.</returns>
        public static string FromLines(string[] lines, string prepend = null, bool newLineAtEnd = false)
        {
            string result = prepend;

            foreach (string line in lines)
            {
                result += line + Environment.NewLine;
            }

            if (newLineAtEnd) result += Environment.NewLine;

            return result;
        }
    }
}

