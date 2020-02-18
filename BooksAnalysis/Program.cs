using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BooksAnalysis
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            ReadAllFiles();
        }

        public static void ReadAllFiles()
        {
            var folderPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            var fullPath = folderPath + @"\10k-livros";
            var contents = "";
            string[] words;
            var numberOfFilesRead = 0;
            Dictionary<string, int> wordDictionary = new Dictionary<string, int>();
            foreach (var file in Directory.EnumerateFiles(fullPath, "*.txt"))
            {
                contents = File.ReadAllText(file);
                words = contents.Split();
                foreach (var word in words)
                {
                    if (word == "" || word == " ")
                        continue;
                    try
                    {
                        wordDictionary.Add(formatWord(word), 0);
                    }
                    catch (ArgumentException)
                    {
                        wordDictionary[formatWord(word)] = wordDictionary[formatWord(word)] + 1;
                    }
                }

                numberOfFilesRead++;
                Console.Write("Arquivos lidos: ");
                Console.WriteLine(numberOfFilesRead);
                Console.Write("Total de palavras: ");
                Console.WriteLine(wordDictionary.Count);
                
                var list = wordDictionary.Keys.ToList();
                var sortedDict = from entry in wordDictionary orderby entry.Value ascending select entry;
                var sortedKeys = sortedDict.Select(p => p.Key);
                
                for(int i = 0; i < 10; i++)
                {
                    Console.WriteLine("{0}: {1}", list[i], wordDictionary[list[i]]);
                }
            }
        }

        public static string formatWord(string pWord)
        {
            return pWord.ToLower().Replace(".", "").Replace(",", "")
                .Replace("!", "").Replace("?", "").Replace(":", "")
                .Replace(";", "").Replace("\"", "").Trim();
        }
        
    }
}