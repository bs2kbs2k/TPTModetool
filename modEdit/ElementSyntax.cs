using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace modEdit
{
    class ElementSyntax
    {
        public static bool isElement(string s) { return Regex.IsMatch(s, "^([A-Za-z0-9]){1,4}$") && s != ""; }
        public static bool isDescription(string s) { return Regex.IsMatch(s, "^([A-Za-z0-9\\s\\.\\,\\?\\!\\:\\;\\'])+$") && s != ""; }
        public static bool isType(string s) { return Regex.IsMatch(s, "^(GAS|LIQUID|SOLID|POWDER)+$") && s != ""; }
        public static bool isTab(string s) { return Regex.IsMatch(s, "^(ELEC|POWERED|SENSOR|FORCE|EXPLOSIVE|GAS|LIQUID|POWDERS|SOLIDS|NUCLEAR|SPECIAL)$") && s != ""; }
    }
}
