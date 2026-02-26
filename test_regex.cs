using System;
using System.Text.RegularExpressions;
class Program {
    static void Main() {
        string input = "9043288,0,1,3,0,18,12,1888,caldavar,02/22/2026,Hero_Jeraziah";
        string pattern = "(.+),(.+),(.+),(.+),(.+),(.+),(.+),(.+),(.+),(.+),(.+)";
        Match match = Regex.Match(input, pattern);
        Console.WriteLine("Match Success: " + match.Success);
        if (match.Success) {
            for (int i = 1; i <= match.Groups.Count - 1; i++) {
                Console.WriteLine("Group " + i + ": " + match.Groups[i].Value);
            }
        }
    }
}
