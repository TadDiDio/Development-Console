using UnityEngine;
using Unity.VisualScripting;

namespace DeveloperConsole
{
    public static class MessageFormatter
    {
        public static string CreateErrorMessage(string message)
        {
            return AddColor("[Error] ", Color.red) + message;
        }
        public static string AddColor(string text, Color color)
        {
            string hexcode = color.ToHexString();
            return $"<color=#{hexcode}>" + text + "</color>";
        }
    }
}

