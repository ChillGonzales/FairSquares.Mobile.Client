using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MobileClient.Utility
{
    public static class StringUtility
    {
        public static string RemoveEmptyLines(string value)
        {
            return Regex.Replace(value, @"\r\n?|\n", " ");
        }
    }
}
