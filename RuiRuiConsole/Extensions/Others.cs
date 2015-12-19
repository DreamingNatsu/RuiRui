using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuiRuiConsole.Extensions
{
    public static class Others
    {
        public static string PadZero(this int integer, int decimals){
            var amtdecimals = integer.ToString().Length;
            var returner = integer.ToString();
            if (amtdecimals >= decimals) return returner;
            while (returner.Length<decimals) {
                returner = "0" + returner;
            }
            return returner;
        }
    }
}
