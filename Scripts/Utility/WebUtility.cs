using System;

namespace LeeFramework.Scripts.Utility
{
    public static class WebUtility
    {
        public static string EscapeString(string stringToEscape)
        {
            return Uri.EscapeDataString(stringToEscape);
        }

        public static string UnescapeString(string stringToUnescape)
        {
            return Uri.UnescapeDataString(stringToUnescape);
        }
    }
}