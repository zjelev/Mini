using System.Data;
using System.Globalization;
using System.Text;
using ExcelUtils;
using OfficeOpenXml;

namespace WeightNotes
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string currentDirectory = Environment.CurrentDirectory + "\\AvtoveznaMonthly";
            var parentDir = Directory.GetParent(currentDirectory).FullName;
            string todaysNotesFile = Directory.GetFiles("\\\\10.3.62.110\\d$\\Spravki_avtovezna", "*.txt").FirstOrDefault();
            try
            {
                File.Copy(todaysNotesFile, parentDir + Path.DirectorySeparatorChar + DateTime.Now.Date.ToString("yyyy-MM-dd") + ".TXT", true);
                Console.WriteLine("Справката за деня беше копирана от " + todaysNotesFile);
            }
            catch (IOException iox)
            {
                Console.WriteLine(iox.Message);
            }

            string[] weightNotesTxtFiles = Directory.GetFiles(parentDir, "*.txt");

            var measures = new List<Measure>();

            if (weightNotesTxtFiles.Length > 62)
            {
                Console.WriteLine("В директорията има повече от 62 файла. Моля махнете тези, които са от предходни месеци.");
            }
            else if (weightNotesTxtFiles.Length == 0)
            {
                Console.WriteLine("В директорията няма файлове.");
            }
            else
            {
                var dailyWeights = new Dictionary<(int, int), DailyTrucksGeologInfo>();

                for (int day = 1; day <= 31; day++)
                {
                    for (int shift = 1; shift <= 2; shift++)
                    {
                        (int, int) dayShift = new(day, shift);
                        dailyWeights.Add(dayShift, null);
                    }
                }

                foreach (var note in weightNotesTxtFiles)
                {
                    DailyTrucksGeologInfo dailyTrucksInfo = Controller.GetTotalForTheDay(note);
                    List<Measure> currentMeasures = Controller.GetMeasures(note);
                    string dateInFileName = Path.GetFileName(note).Substring(8, 2);
                    string dateInFile = dailyTrucksInfo.Date.ToString("dd");
                    if (dateInFileName == dateInFile)
                    {
                        (int, int) dayShift = new(dailyTrucksInfo.Date.Day, dailyTrucksInfo.Shift);
                        dailyWeights[dayShift] = dailyTrucksInfo;
                    }
                    else
                    {
                        Console.WriteLine($"Името на файла {Path.GetFileName(note)} не съответства на датата в него. Файлът ще бъде преименуван като датата в него.");
                        // try
                        // {
                        //     File.Move(note, parentDir + "\\"
                        //     + dailyTrucksInfo.Date.ToString("yyyy-MM-dd") + ".TXT");
                        // }
                        // catch (IOException)
                        // {
                        //     Console.WriteLine("Изтрийте файла с днешна дата.");
                        //     File.Delete(parentDir + "\\" + dailyTrucksInfo.Date.ToString("yyyy-MM-dd") + ".TXT");
                        // }

                    }

                    measures.AddRange(currentMeasures);
                }

                int month;
                var first = measures.FirstOrDefault();
                var last = measures.Last();
                if (first.FromDate.Month == last.FromDate.Month)
                {
                    month = last.FromDate.Month;
                }
                else
                {
                    throw new Exception("В папката има файлове от различни месеци");
                }

                Controller.InsertDailyTotals(
                    dailyWeights, parentDir + "\\Справка по дни.xlsx",
                    first.FromDate.ToString("dd/MM/yy ", CultureInfo.InvariantCulture) + first.BrutoHour,
                    last.FromDate.ToString("dd/MM/yy ", CultureInfo.InvariantCulture) + last.BrutoHour);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("N ; Протокол; От дата ; Рег.номер ; Бруто ; Бруто час ; Тара ; Тара час ; Нето");
                foreach (var measure in measures)
                {
                    sb.AppendLine(measure.ToString());
                }
                string allMeasuresFile = parentDir + "\\Всички м." + month + ".csv";
                File.WriteAllText(allMeasuresFile, sb.ToString(), Common.srcEncoding);

                string? planXlxFile = Directory.GetFiles(parentDir, "ТРОЯНОВО*.xls?").Where(name => !name.Contains("попълнен")).FirstOrDefault();

                if (planXlxFile != null)
                {
                    // Dictionary<(DateTime, string), Measure> speditorDaily = Controller.GetPlannedTrucks(planXlxFile);

                    // sb = new StringBuilder();
                    // sb.AppendLine("№;Дата;ВЛЕКАЧ;РЕМАРКЕ;ШОФЬОР;ЕГН;ТЕЛЕФОН;тонаж");
                    // int nraCounter = 0;
                    // foreach (var truckInfo in speditorDaily)
                    // {
                    //     foreach (var measure in measures)
                    //     {
                    //         try
                    //         {
                    //             string regNumVezna = measure.RegNum.ToUpper().TrimEnd();
                    //             string trailerNumSpeditor = truckInfo.Value.RegNum.ToUpper().TrimEnd();
                    //             regNumVezna = Common.ReplaceCyrillic(regNumVezna);
                    //             trailerNumSpeditor = Common.ReplaceCyrillic(trailerNumSpeditor);

                    //             if (measure.FromDate.Date == truckInfo.Key.Item1.Date && regNumVezna == trailerNumSpeditor)
                    //             {
                    //                 truckInfo.Value.Netto = measure.Netto;
                    //             }
                    //             measure.RegNum = regNumVezna;
                    //             truckInfo.Value.RegNum = trailerNumSpeditor;
                    //         }
                    //         catch (NullReferenceException nre)
                    //         {
                    //             Console.WriteLine($"{nre.Message} No. {nraCounter++} ");
                    //         }
                    //     }
                    //     sb.AppendLine($"{truckInfo.Value.Id};{truckInfo.Key.Item1.Date.ToString("dd.MM.yyyy")};{truckInfo.Value.TractorNum};" +
                    //         $"{truckInfo.Value.RegNum};{truckInfo.Value.Driver};{truckInfo.Value.Egn};" +
                    //         $"{truckInfo.Value.Phone};{truckInfo.Value.Netto}");

                    //     // //reversed
                    //     // measure.TractorNum = truckInfo.Value.TractorNum;
                    //     // measure.Egn = truckInfo.Value.Egn;
                    //     // measure.Phone = truckInfo.Value.Phone;

                    //     // //byte[] isoBytes = Encoding.Convert(Encoding.UTF8, Common.srcEncoding, Encoding.UTF8.GetBytes(truckInfo.Value.Driver));
                    //     // measure.Driver =
                    //     //     //Encoding.UTF8.GetString(
                    //     //     truckInfo.Value.Driver
                    //     //     //)
                    //     //     ;

                    //     // sb.AppendLine($"{measure.Id};{measure.FromDate.Date.ToString("dd.MM.yyyy")};{measure.TractorNum};" +
                    //     //     $"{measure.RegNum};{measure.Driver};{measure.Egn};" +
                    //     //     $"{measure.Phone};{measure.Netto}");
                    // }
                    // File.WriteAllText(planXlxFile.Substring(0, planXlxFile.LastIndexOf('.')) + "-попълнен.csv", sb.ToString(), Common.srcEncoding);

                    var speditorDailySheets = Controller.GetPlannedTrucksDaily(planXlxFile);
                    int knfeCounter = 0;
                    using (ExcelPackage package = new ExcelPackage(new FileInfo(planXlxFile.Substring(0, planXlxFile.LastIndexOf('.')) + "-попълнен.xlsx")))
                    {
                        while (package.Workbook.Worksheets.Count > 0)
                        {
                            package.Workbook.Worksheets.Delete(0);
                        }

                        foreach (var sheet in speditorDailySheets)
                        {
                            ExcelWorksheet ws = package.Workbook.Worksheets.Add(sheet.Key);
                            ws.DefaultRowHeight = 15;
                            ws.Column(1).Width = 3;
                            ws.Column(2).Width = 11;
                            ws.Column(3).Width = 11;
                            ws.Column(4).Width = 30;
                            ws.Column(5).Width = 13;
                            ws.Column(6).Width = 10;
                            ws.Column(7).Width = 10;
                            ws.Row(1).Style.Font.Bold = true;

                            DataTable dataTable = new DataTable();
                            dataTable.Columns.Add("№", typeof(string));
                            dataTable.Columns.Add("ВЛЕКАЧ", typeof(string));
                            dataTable.Columns.Add("РЕМАРКЕ", typeof(string));
                            dataTable.Columns.Add("ШОФЬОР", typeof(string));
                            dataTable.Columns.Add("ЕГН", typeof(string));
                            dataTable.Columns.Add("ТЕЛЕФОН", typeof(string));
                            dataTable.Columns.Add("тонаж", typeof(int));

                            var measuresCurrentDay = measures.Where(x => x.FromDate.ToString("dd.M") == sheet.Key).ToList();

                            foreach (var truckInfo in sheet.Value)
                            {
                                string trailerNumSpeditor = truckInfo.Key.ToUpper().TrimEnd();
                                sheet.Value[trailerNumSpeditor].Id = 0;

                                foreach (var measure in measuresCurrentDay)
                                {
                                    try
                                    {
                                        trailerNumSpeditor = Common.ReplaceCyrillic(trailerNumSpeditor);

                                        if (Common.ReplaceCyrillic(measure.RegNum.ToUpper().TrimEnd()) == trailerNumSpeditor && !measure.IsPlanned)
                                        {
                                            sheet.Value[trailerNumSpeditor].Netto = measure.Netto;
                                            sheet.Value[trailerNumSpeditor].Id = measure.Id;
                                            measure.IsPlanned = true;
                                            break;
                                        }
                                    }
                                    catch (KeyNotFoundException knfe)
                                    {
                                        Console.WriteLine($"{knfe.Message} No. {knfeCounter++} ");
                                    }
                                }
                                dataTable.Rows.Add(truckInfo.Value.Id, truckInfo.Value.TractorNum, truckInfo.Key,
                                            truckInfo.Value.Driver, truckInfo.Value.Egn, truckInfo.Value.Phone, truckInfo.Value.Netto);
                            }

                            var unplannedMeasuresForTheDay = measuresCurrentDay.Where(m => m.IsPlanned == false);
                            foreach (var measure in unplannedMeasuresForTheDay)
                            {
                                dataTable.Rows.Add(measure.Id, measure.TractorNum, measure.RegNum,
                                            measure.Driver, measure.Egn, measure.Phone, measure.Netto);
                            }
                            //add all the content from the DataTable, starting at cell A1
                            ws.Cells["A1"].LoadFromDataTable(dataTable, true);
                        }
                        package.Save();
                    }
                }
            }
        }
    }
}