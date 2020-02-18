using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace BooksAnalysis
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var fileNumber = 0;
            ConcurrentDictionary<string, int> wordDictionary = new ConcurrentDictionary<string, int>();
            var threadsNum = 8;
            var folderPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            var fullPath = folderPath + @"\10k-livros";
            
            var numFilesInPath = Directory.GetFiles(fullPath).Length;
            var Files = Directory.EnumerateFiles(fullPath, "*.txt").ToList();
            
            //100
            var totalFiles = Files.GetRange(1, 100);
            numFilesInPath = 100;

            List<int> intervalos = new List<int>();
            
            for(int i = 0; i < numFilesInPath; i += (numFilesInPath/threadsNum))
                intervalos.Add(i);
            
            for (int i = 0; i < intervalos.Count-1; i++)
            { 
                // var partialFilesList = totalFiles.GetRange(intervalos[i], (numFilesInPath/threadsNum));
                var partialFilesList = totalFiles.GetRange(intervalos[i], (numFilesInPath/threadsNum));
                new Thread(() => ReadAllFiles(partialFilesList, wordDictionary, fileNumber)).Start();    
            }
            
        }

        public static void ReadAllFiles(List<string> files, ConcurrentDictionary<string, int> wordDictionary, int fileNumber)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            var contents = "";
            string[] words;
            foreach (var file in files)
            {
                contents = File.ReadAllText(file);
                words = contents.Split();
                foreach (var word in words)
                {
                    if (word == "" || word == " ")
                        continue;
                    
                    wordDictionary.AddOrUpdate(rgx.Replace(word, ""), 0, 
                        (key, oldValue) => oldValue + 1);
                }
            }

            Console.WriteLine(wordDictionary.Count());
            
        }

    }
}