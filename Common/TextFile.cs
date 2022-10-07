using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Common
{
    public class TextFile
    {
        public static void GetHeaderFromLeon(string file, out string fileName, out int year, out string month, out string podName, out string podNumberStr)
        {
            fileName = Path.GetFileName(file);
            year = int.Parse(fileName.Substring(0, 4));
            month = fileName.Substring(4, 2) + ".";
            podName = String.Empty;
            podNumberStr = fileName[fileName.Length - 6].ToString();
            if (Int32.TryParse(podNumberStr, out int podNumber))
            {
                podName = "РУДНИК ТРОЯНОВО-";
            }
            else
            {
                podNumberStr = "УПРАВЛЕНИЕ";
            }

            if (podNumberStr == "2")
            {
                podNumberStr = "СЕВЕР";
            }
        }

        public static string ReplaceCyrillic(string regNum)
        {
            if ((Regex.IsMatch(regNum, @"\p{IsCyrillic}")))
            {
                regNum = regNum.Replace('А', 'A');
                regNum = regNum.Replace('В', 'B');
                regNum = regNum.Replace('С', 'C');
                regNum = regNum.Replace('Е', 'E');
                regNum = regNum.Replace('К', 'K');
                regNum = regNum.Replace('М', 'M');
                regNum = regNum.Replace('Н', 'H');
                regNum = regNum.Replace('О', 'O');
                regNum = regNum.Replace('Р', 'P');
                regNum = regNum.Replace('Т', 'T');
                regNum = regNum.Replace('У', 'Y');
                regNum = regNum.Replace('Х', 'X');
            }

            return regNum;
        }

        public static bool FilesMatch(string path1, string path2)
        {
            if (File.Exists(path2))
            {
                using (FileStream fs1 = new FileStream(path1, FileMode.Open),
                  fs2 = new FileStream(path2, FileMode.Open))
                {
                    int c1 = 0, c2 = 0;
                    do
                    {
                        c1 = fs1.ReadByte();
                        c2 = fs2.ReadByte();
                    }
                    while (c1 == c2 && c1 != -1 && c2 != -1);

                    if (c1 != c2)
                    {
                        Console.WriteLine("Files are different:");
                        // var file1Lines = File.ReadLines(path1);
                        // var file2Lines = File.ReadLines(path2);
                        // IEnumerable<String> inFirstNotInSecond = file1Lines.Except(file2Lines);
                        // IEnumerable<String> inSecondNotInFirst = file2Lines.Except(file1Lines);

                        // Console.WriteLine($"-----In {path1} not in {path2}-----:");
                        // foreach (var line in inFirstNotInSecond)
                        // {
                        //     Console.WriteLine(line);
                        // }
                        // Console.WriteLine($"-----In {path2} not in {path1}-----:");
                        // foreach (var line in inSecondNotInFirst)
                        // {
                        //     Console.WriteLine(line);
                        // }
                        return false;
                    }
                    Console.WriteLine("Files match");
                    return true;
                }
            }
            return false;
        }

        public static void Log(string message)
        {
            using StreamWriter logFileStream = new StreamWriter(new FileStream("log.txt", FileMode.Append));
            string log = String.Empty;
            log = DateTime.Now.ToString() + " " + message;
            Console.WriteLine(log);
            logFileStream.WriteLine(log);
        }
    }
}