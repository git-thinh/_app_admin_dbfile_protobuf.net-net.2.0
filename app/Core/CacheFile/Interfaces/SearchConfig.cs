using System;
using System.Collections.Generic;
using System.Text;

namespace app.Core
{
    public class SearchConfig
    {
        public const int selectTop = 100;
        public static string[] OpString = "Equals,NotEquals,Contains,StartsWith,EndsWith".Split(',');
        public static string[] OpNumber = "Equals,NotEquals,Contains,GreaterThan,LessThan,GreaterThanOrEqual,LessThanOrEqual".Split(',');
    }
}
