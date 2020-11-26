using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrateSOtoADS.Extensions
{
    public static class StringExtensions
    {
        public static string EscapeString(this string input)
        {
            return "\"" + input + "\"";
        }
    }
}
