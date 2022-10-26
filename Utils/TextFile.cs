using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Utils
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
                // TODO: Use foreach
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
            Log(message, Environment.CurrentDirectory);
        }

        public static void Log(string message, string path)
        {
            using StreamWriter logFileStream = new StreamWriter(new FileStream(path + "\\log.txt", FileMode.Append));
            string log = String.Empty;
            log = DateTime.Now.ToString() + " " + message;
            Console.WriteLine(log);
            logFileStream.WriteLine(log);
        }


        public static IEnumerable<FileInfo> CompareDirs(string pathA, string patternA, string pathB, string patternB)
        {
            // Take a snapshot of the file system.  
            IEnumerable<FileInfo> list1 = new DirectoryInfo(pathA).GetFiles(patternA, SearchOption.AllDirectories);
            IEnumerable<FileInfo> list2 = new DirectoryInfo(pathB).GetFiles(patternB, SearchOption.AllDirectories);

            //A custom file comparer 
            FileCompare fileCompare = new FileCompare();

            // This query determines whether the two folders contain  
            // identical file lists, based on the custom file comparer  
            // that is defined in the FileCompare class.  
            // The query executes immediately because it returns a bool.  
            bool areIdentical = list1.SequenceEqual(list2, fileCompare);

            if (areIdentical == true)
            {
                Console.WriteLine("the two folders are the same");
                return null;
            }
            else
            {
                Console.WriteLine("The two folders are not the same");

                // // Find the common files. It produces a sequence and doesn't execute until the foreach statement.  
                // var queryCommonFiles = list1.Intersect(list2, fileCompare);

                // if (queryCommonFiles.Any())
                // {
                //     Console.WriteLine("The following files are in both folders:");
                //     foreach (var v in queryCommonFiles)
                //     {
                //         Console.WriteLine(v.FullName); //shows which items end up in result list  
                //     }
                // }
                // else
                // {
                //     Console.WriteLine("There are no common files in the two folders.");
                // }

                // Find the set difference between the two folders. For this example we only check one way.  
                var queryList1Only =
                    list1.Except(list2, fileCompare);
                //(from file in list1 select file).Except(list2, fileCompare);

                Console.WriteLine($"The following files are in {list1.FirstOrDefault().Directory.ToString()} " +
                    $"but not in {list2.FirstOrDefault().Directory.ToString()}:");

                foreach (var v in queryList1Only)
                {
                    Console.WriteLine(v.FullName);
                }

                return queryList1Only;
            }
        }

        public static string SaveNew(string logPath, StringBuilder sb, string file)
        {
            string newFileName = string.Empty;
            if (File.Exists(file))
            {
                File.WriteAllText(file + "-temp", sb.ToString(), Excel.srcEncoding);
                if (!FilesMatch(file, file + "-temp"))
                {
                    File.Copy(file + "-temp", file, true);
                    Log($" ### Файлът {Path.GetFileName(file)} беше обновен.", logPath);
                    newFileName = file;
                }
                else
                {
                    Log($"Няма нова информация. Файлът {Path.GetFileName(file)} не беше обновен.", logPath);
                }
                File.Delete(file + "-temp");
            }
            else
            {
                File.WriteAllText(file, sb.ToString(), Excel.srcEncoding);
                Log($" ### Файлът {Path.GetFileName(file)} беше създаден.", logPath);
                newFileName = file;
            }
            return newFileName;
        }
    }

    // This implementation defines a very simple comparison  
    // between two FileInfo objects. It only compares the name  
    // of the files being compared and their length in bytes.  
    class FileCompare : IEqualityComparer<FileInfo>
    {
        public FileCompare() { }

        public bool Equals(FileInfo f1, FileInfo f2)
        {
            return (f1.LastWriteTime == f2.LastWriteTime &&
                    f1.Length == f2.Length);
        }

        // Return a hash that reflects the comparison criteria. According to the
        // rules for IEqualityComparer<T>, if Equals is true, then the hash codes must  
        // also be equal. Because equality as defined here is a simple value equality, not  
        // reference identity, it is possible that two or more objects will produce the same  
        // hash code.  
        public int GetHashCode(FileInfo fi)
        {
            string s = $"{fi.LastWriteTime}{fi.Length}";
            return s.GetHashCode();
        }
    }
}