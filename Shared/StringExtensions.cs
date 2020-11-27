using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timelog.Extensions
{
    public static class StringExtensions
    {
        public static string EscapeString(this string input)
        {
            return "\"" + input + "\"";
        }

        public static string RemoveEscapeAtStartAndEnd(this string input)
        {
            return input.Remove(input.Length - 1, 1).Remove(0, 1);
        }
    }
}
