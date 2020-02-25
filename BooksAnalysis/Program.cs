using System;
using System.CodeDom;
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
            var wordDictionary = new ConcurrentDictionary<string, long>();
            var orderedDictionary = new Dictionary<string, long>();
            var top10words = new Dictionary<string, long>();
            var threadsNum = 8;
            var folderPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            var fullPath = folderPath + @"\10k-livros";
            
            var numFilesInPath = Directory.GetFiles(fullPath).Length;
            var files = Directory.EnumerateFiles(fullPath, "*.txt").ToList();
            var stack = new ConcurrentStack<Thread>();


            //100
            var totalFiles = files.GetRange(1, 40);
            numFilesInPath = totalFiles.Count();

            List<int> intervalos = new List<int>();
            
            for(int i = 0; i < numFilesInPath; i += (numFilesInPath/threadsNum))
                intervalos.Add(i);
            
            for (var i = 0; i < threadsNum-1; i++)
            {
                var partialFilesList = files.GetRange(intervalos[i], (numFilesInPath/threadsNum)-1);
                stack.Push(new Thread(() => ReadAllFiles(partialFilesList, wordDictionary))); 
            }
            
            foreach(Thread thread in stack)
            {
                thread.Start();
                thread.Join();
            }
            
            while (stack.Any(t => !t.IsAlive))
            {
                orderedDictionary = wordDictionary.OrderByDescending(pair => pair.Value).Take(10)
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
                var filePath = folderPath + @"\word-analysis.txt";
                File.WriteAllText(filePath, "Total files: " + numFilesInPath);
                File.WriteAllText(filePath, "Total words: " + orderedDictionary.Count());
                File.WriteAllLines(filePath,
                    orderedDictionary.Select(x => "[" + x.Key + " " + x.Value + "]"));
                break;
            }
            
        }

        public static void ReadAllFiles(List<string> files, ConcurrentDictionary<string, Int64> wordDictionary)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            var contents = "";
            List<string> words = new List<string>();
            foreach (var file in files)
            {
                contents = File.ReadAllText(file);
                words = contents.Split().ToList();
                foreach (var word in words)
                {
                    if (word == "" || word == " ")
                        continue;

                    wordDictionary.AddOrUpdate(rgx.Replace(word.ToLower(), ""), 1,
                        (key, oldValue) => oldValue + 1);
                }
            }

            Console.WriteLine(wordDictionary.Count());
        }
    }
}