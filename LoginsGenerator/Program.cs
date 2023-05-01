using System;
using System.Collections.Generic;
using System.IO;

namespace LoginsGenerator
{
    class Program
    {
        static void Main(string[] args)
        {

            var inputFilePath = Directory.GetCurrentDirectory() + "/Resources/Input.txt";
            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException($"File not founded for path: {inputFilePath}");

            var inputStrings = File.ReadAllLines(inputFilePath);


            var logins = GenerateLogins(inputStrings, 20);
            var outputFilePath = Directory.GetCurrentDirectory() + "/Output.txt";
            File.WriteAllLines(outputFilePath, logins);
        }

        private static List<string> GenerateLogins(string[] inputStrings, int maxLength)
        {
            var usedLogins = new HashSet<string>();
            var resultLogins = new List<string>();

            foreach (var str in inputStrings)
            {
                var strData = str.ToLower().Split();
                var lastName = strData[0];
                var firstName = strData[1];
                var patronymic = strData[2];

                var attempts = new string[]
                {
                    $"{firstName}.{lastName}",
                    $"{firstName[0]}.{lastName}",
                    $"{firstName[0]}.{patronymic[0]}.{lastName}",
                    $"{lastName}",
                };

                bool isAttemptWasAdded = false;
                foreach (var attempt in attempts)
                {
                    if (attempt.Length > maxLength)
                        continue;

                    if (!usedLogins.Contains(attempt))
                    {
                        usedLogins.Add(attempt);
                        resultLogins.Add(attempt);
                        isAttemptWasAdded = true;
                        break;
                    }
                }

                if (isAttemptWasAdded)
                    continue;

                int curIndex = 1;
                while (!isAttemptWasAdded)
                {
                    string indexStr = curIndex.ToString();
                    string loginStr = $"{firstName[0]}.{lastName}{new string('\t', maxLength)}";
                    var attempt = loginStr
                        .Insert(maxLength - indexStr.Length, indexStr)
                        .Substring(0, maxLength)
                        .Replace("\t", string.Empty);

                    if (!usedLogins.Contains(attempt))
                    {
                        usedLogins.Add(attempt);
                        resultLogins.Add(attempt);
                        isAttemptWasAdded = true;
                        break;
                    }

                    curIndex++;
                }
            }
            
            return resultLogins;
        }
    }
}
