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
        private static ConcurrentDictionary<string, long> wordDictionary = new ConcurrentDictionary<string, long>();
        public static void Main(string[] args)
        {
            var orderedDictionary = new Dictionary<string, long>();
            var threadsNum = 8;
            var folderPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            var fullPath = folderPath + @"\10k-livros";
            
            var numFilesInPath = Directory.GetFiles(fullPath).Length;
            var files = Directory.EnumerateFiles(fullPath, "*.txt").ToList();
            var stack = new ConcurrentStack<Thread>();


            //100
            //var totalFiles = files.GetRange(1, 40);
            //numFilesInPath = totalFiles.Count();

            List<int> intervalos = new List<int>();
            
            for(int i = 0; i < numFilesInPath; i += (numFilesInPath/threadsNum))
                intervalos.Add(i);
            
            for (var i = 0; i < threadsNum-1; i++)
            {
                var partialFilesList = files.GetRange(intervalos[i], (numFilesInPath/threadsNum)-1);
                stack.Push(new Thread(() => ReadAllFiles(partialFilesList))); 
            }
            
            foreach(var thread in stack)
            {
                thread.Start();
                thread.Join();
            }
            
            while (stack.Any(t => !t.IsAlive))
            {
                orderedDictionary = wordDictionary.OrderByDescending(pair => pair.Value).Take(20)
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
                var filePath = folderPath + @"\word-analysis.csv";
                var fileSummaryPath = folderPath + @"\word-summary-analysis.txt";
                File.WriteAllText(fileSummaryPath, "Total files: " + numFilesInPath + "\n" +
                                                   "Total words: " + wordDictionary.Count());
                File.WriteAllLines(filePath,
                    orderedDictionary.Select(x =>  x.Value + ", " + x.Key));
                break;
            }
            
        }

        public static void ReadAllFiles(List<string> files)
        {
            var rgx = new Regex("[^a-zA-Z0-9 -]");
            var isRealBookText = false;
            bool textStartFound;
            bool textEndFound;
            var lines = new List<string>();
            var words = new List<string>();
            foreach (var file in files)
            {
                textStartFound = false;
                textEndFound = false;
                lines = File.ReadAllLines(file).ToList();
                foreach (var line in lines)
                {
                    if (textStartFound == false)
                    {
                        textStartFound = ProjectDisclaimerFilter.isBeginningOfDisclaimer(line);
                        continue;
                    }

                    if (textStartFound && textEndFound == false)
                    {
                        textEndFound = ProjectDisclaimerFilter.isEndingOfDisclaimer(line);
                        if (textEndFound)
                            break;
                        words = line.Split().ToList();
                        foreach (var word in words)
                        {
                            rgx.Replace(word.ToLower(), "");
                            if (word.Trim() == "" || StopWordsFilter.isStopWord(word.ToLower()))
                                continue;

                            wordDictionary.AddOrUpdate(word.ToLower(), 1,
                                (key, oldValue) => oldValue + 1);
                        }
                    }
                    
                }
               
            }
            
            Console.WriteLine(wordDictionary.Count());
        }
    }
}