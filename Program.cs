using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using OfficeOpenXml;

namespace ExcelUtils
{
    class Program
    {
        public static Encoding srcEncoding = Encoding.GetEncoding("windows-1251");
        public static Dictionary<Tuple<int, string>, CodeSum> codesSums;
        public static Dictionary<Tuple<int, int>, DailyTrucksInfo> dailyWeights;
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string currentDirectory = Directory.GetCurrentDirectory();
            if (args.Length == 1)
            {
                currentDirectory = args[0];
            }
            var parentDir = Directory.GetParent(currentDirectory).FullName;
            string[] directories = Directory.GetDirectories(currentDirectory);
            string[] weightNotesTxtFiles = Directory.GetFiles(currentDirectory);

            foreach (string dir in directories)
            {
                string dirName = new DirectoryInfo(dir).Name;
                if (!(dirName.Equals("bin") || dirName.Equals("obj") || dirName.Equals(".vscode") || dirName.ToLower().StartsWith("avtokantar")))
                {
                    string[] subDirectories = Directory.GetDirectories(dir);
                    foreach (string subDir in subDirectories)
                    {
                        string[] xlsxFilesInSubDir = Directory.GetFiles(subDir, "*.xlsx");
                        List<string> xlsxFiles = new List<string>();
                        xlsxFiles.AddRange(xlsxFilesInSubDir);
                        if (xlsxFiles.Count > 0)
                        {
                            codesSums = new Dictionary<Tuple<int, string>, CodeSum>();
                            foreach (string file in xlsxFiles)
                            {
                                CodeSumsTotal.AddHeader(file);
                                CodeSumsTotal.AddSumsByCode(file, codesSums);
                            }
                            Console.WriteLine("В директория {0} бяха обработени {1} файла.", subDir, xlsxFiles.Count);
                            string upperFolderName = new DirectoryInfo(Path.GetDirectoryName(subDir)).Name;
                            CodeSumsTotal.InsertCodesSumsInXlsx(codesSums, subDir + "_Recap_" + upperFolderName.ToUpper() + "_MMI.xlsx");
                        }
                    }
                }
                //else if (dirName.ToLower().StartsWith("avtokantar"))
                //{
            }
            weightNotesTxtFiles = Directory.GetFiles(
                parentDir,
                "*.txt");
            if (weightNotesTxtFiles.Length > 62)
            {
                Console.WriteLine("В директорията има повече от 62 файла. Моля махнете тези, които са от предходни месеци.");
            }
            else if (weightNotesTxtFiles.Length <= 0)
            {
                Console.WriteLine("В директорията няма файлове.");
            }
            else
            {
                List<Measure> measures = new List<Measure>();
                List<DailyTrucksInfo> trucksInfos = new List<DailyTrucksInfo>();
                dailyWeights = new Dictionary<Tuple<int, int>, DailyTrucksInfo>();
                for (int day = 1; day <= 31; day++)
                {
                    for (int shift = 1; shift <= 2; shift++)
                    {
                        Tuple<int, int> dayShift = new Tuple<int, int>(day, shift);
                        dailyWeights.Add(dayShift, null);
                    }
                }

                foreach (var note in weightNotesTxtFiles)
                {
                    DailyTrucksInfo dailyTrucksInfo = WeightNotes.GetTotalForTheDay(note);
                    List<Measure> currentMeasures = WeightNotes.GetMeasures(note);
                    if (int.Parse(Path.GetFileName(note).Substring(8, 2)) == dailyTrucksInfo.Date.Day)
                    {
                        //trucksInfos.Add(dailyTrucksInfo);
                        Tuple<int, int> dayShift = new Tuple<int, int>(dailyTrucksInfo.Date.Day, dailyTrucksInfo.Shift);
                        dailyWeights[dayShift] = dailyTrucksInfo;
                    }
                    else
                    {
                        Console.WriteLine($"Името на файла {Path.GetFileName(note)} не съответства на датата в него. Данните от този файл няма да бъдат обработени!");
                    }

                    measures.AddRange(currentMeasures);
                }


                // DailyTrucksInfo[] trucksInfos = new DailyTrucksInfo[62];
                // for (int i = 1; i <= 62; i++)
                // {
                //     DailyTrucksInfo dailyTrucksInfo = WeightNotes.GetTotalForTheDay(weightNotesTxtFiles[i]);
                //     if (int.Parse(Path.GetFileName(weightNotesTxtFiles[i]).Substring(8, 2)) == dailyTrucksInfo.Date.Day)
                //     {
                //         if (dailyTrucksInfo.Date.Day == i)
                //         {
                //             trucksInfos[i] = dailyTrucksInfo;
                //         }
                //     }
                //     else
                //     {
                //         Console.WriteLine($"Името на файла {weightNotesTxtFiles[i]} не съответства на датата в него. Данните от този файл няма да бъдат обработени!");
                //     }
                // }

                string startDate = measures.FirstOrDefault().FromDate + " " + measures.FirstOrDefault().BrutoHour;
                string endDate = measures.Last().FromDate + " " + measures.Last().BrutoHour;

                WeightNotes.InsertDailyTotals(
                    //trucksInfos,
                    dailyWeights, parentDir + "\\Справка по дни.xlsx", startDate, endDate);

                string allMeasuresFile = parentDir + "\\Всички.csv";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("N ; Протокол; От дата ; Рег.номер ; Бруто ; Бруто час ; Тара ; Тара час ; Нето");
                foreach (var measure in measures)
                {
                    sb.AppendLine(measure.ToString());
                }
                File.WriteAllText(allMeasuresFile, sb.ToString(), Program.srcEncoding);
            }
        }
    }
}
