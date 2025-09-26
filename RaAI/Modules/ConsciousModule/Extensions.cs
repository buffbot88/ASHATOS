using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaAI.Modules.ConsciousModule
{
    public static class StringExtensions
    {
        public static bool ContainsIgnoreCase(this string source, string value)
        {
            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
