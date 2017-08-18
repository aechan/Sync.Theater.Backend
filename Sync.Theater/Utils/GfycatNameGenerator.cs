using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sync.Theater.Utils
{
    class GfycatNameGenerator
    {
        static string Animals = @"../../../animals.txt";
        static string Adjectives = @"../../../adjectives.txt";

        public static string GetName()
        {
            Random rnd1 = new Random();
            string Animal = GetRandomLine(Animals, rnd1);
            string Adjective1 = GetRandomLine(Adjectives, rnd1);
            string Adjective2 = GetRandomLine(Adjectives, rnd1);

            
            return FirstLetterToUpper(Adjective1) + FirstLetterToUpper(Adjective2) + FirstLetterToUpper(Animal);
            
        }

        private static string GetRandomLine(string file, Random rnd1)
        {
            string[] allLines = File.ReadAllLines(file);
            
            
            return allLines[rnd1.Next(allLines.Length)];
        }

        public static string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }
    }
}
