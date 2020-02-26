using System.Collections.Generic;

namespace BooksAnalysis
{
    public class ProjectDisclaimerFilter
    {
        public static bool isBeginningOfDisclaimer(string line)
        {
            return line.StartsWith("*** START");
        }
        
        public static bool isEndingOfDisclaimer(string line)
        {
            return line.Contains("*** END");
        }
    }
}