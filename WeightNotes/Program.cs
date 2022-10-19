using System.Data;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Common;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace WeightNotes
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var config = File.ReadAllText("config.json");
            string veznaHost = JsonSerializer.Deserialize<ConfigWeightNotes>(config)?.Vezna.Host!;
            string veznaPath = JsonSerializer.Deserialize<ConfigWeightNotes>(config)?.Vezna.Path!;
            string veznaFile = JsonSerializer.Deserialize<ConfigWeightNotes>(config)?.Vezna.File!;

            string todaysFile = Directory.GetFiles($"\\\\{veznaHost}\\{veznaPath}", veznaFile).FirstOrDefault()!;
            DailyTrucksGeologInfo dailyNote = Controller.GetTotalForTheDay(todaysFile);
            string dateInTodaysFile = dailyNote.Date.ToString("dd.MM");
            string todaysNotesFileRenamed = Environment.CurrentDirectory + Path.DirectorySeparatorChar + DateTime.Now.Date.ToString("yyyy-MM-dd") + ".TXT";
            bool haveTrucksToday = dateInTodaysFile == DateTime.Now.Date.ToString("dd.MM");
            bool spravkaUpdated = !TextFile.FilesMatch(todaysFile, todaysNotesFileRenamed);

            if (haveTrucksToday)
            {
                if (spravkaUpdated)
                {
                    try
                    {
                        File.Copy(todaysFile, todaysNotesFileRenamed, true);
                        Console.WriteLine("Справката за деня беше копирана от " + todaysFile);
                    }
                    catch (IOException iox)
                    {
                        TextFile.Log(iox.Message);
                    }
                }
                else
                {
                    TextFile.Log("Справката не е обновена.");
                }
            }
            else
            {
                TextFile.Log("Днес не е правена справка");
            }


            string[] weightNotesTxtFiles = Directory.GetFiles(Environment.CurrentDirectory, "20*.txt");
            var measures = new List<Measure>();

            if (weightNotesTxtFiles.Length > 62)
            {
                string error = " В директорията има повече от 62 файла. Моля махнете тези, които са от предходни месеци.";
                TextFile.Log(error);
                throw new ArgumentException(error);
            }

            if (weightNotesTxtFiles.Length == 0)
            {
                string error = "В директорията няма файлове.";
                TextFile.Log(error);
                throw new ArgumentException(error);
            }

            var dailyWeights = new Dictionary<(int, int), DailyTrucksGeologInfo>();
            int days = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

            for (int day = 1; day <= days; day++)
            {
                for (int shift = 1; shift <= 2; shift++)
                {
                    (int, int) dayShift = new(day, shift);
                    dailyWeights.Add(dayShift, null!);
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
                    TextFile.Log($" Името на файла {Path.GetFileName(note)} не съответства на датата в него. Файлът ще бъде преименуван като датата в него.");
                }
                measures.AddRange(currentMeasures);
            }

            int month;
            var first = measures.FirstOrDefault();
            var last = measures.Last();
            if (first?.FromDate.Month == last.FromDate.Month)
            {
                month = last.FromDate.Month;
            }
            else
            {
                string error = "В папката има файлове от различни месеци";
                TextFile.Log(error);
                throw new ArgumentException(error);
            }

            Controller.InsertDailyTotals(
                dailyWeights, Environment.CurrentDirectory + "\\Справка по дни.xlsx",
                first.FromDate.ToString("dd/MM/yy ", CultureInfo.InvariantCulture) + first.BrutoHour,
                last.FromDate.ToString("dd/MM/yy ", CultureInfo.InvariantCulture) + last.BrutoHour);
            Console.WriteLine("Файлът Справка по дни.xlsx беше обновен.");

            StringBuilder missingProtokols = new StringBuilder();
            missingProtokols.AppendLine("Дата;Номер");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("N ; Протокол; От дата ; Рег.номер ; Бруто ; Бруто час ; Тара ; Тара час ; Нето");
            int firstProtokolNum = measures.FirstOrDefault()!.ProtokolNum;
            int countProtokolNum = firstProtokolNum - 1;
            foreach (var measure in measures)
            {
                sb.AppendLine(measure.ToString());
                countProtokolNum++;
                if (measure.ProtokolNum != countProtokolNum)
                {
                    while (measure.ProtokolNum != countProtokolNum)
                    {
                        Measure missingMeasure = new Measure() { ProtokolNum = countProtokolNum, FromDate = measure.FromDate };
                        missingProtokols.AppendLine($"{missingMeasure.FromDate.ToString("dd.MM.yyyy")};{countProtokolNum}");
                        countProtokolNum++;
                    }
                }
            }
            string allMeasuresFile = Environment.CurrentDirectory + "\\Всички м." + month + ".csv";
            File.WriteAllText(allMeasuresFile, sb.ToString(), Excel.srcEncoding);
            Console.WriteLine($"Файлът {allMeasuresFile.Substring(allMeasuresFile.LastIndexOf('\\') + 1, allMeasuresFile.Length - allMeasuresFile.LastIndexOf('\\') - 1)} беше обновен.");

            string missingMeasuresFile = "Липсващи бележки.csv";
            File.WriteAllText(missingMeasuresFile, missingProtokols.ToString(), Excel.srcEncoding);
            string lastMissingProtokol = missingProtokols.Remove(0, missingProtokols.Length - 17).ToString().Substring(0, 10);
            // int lastMissingProtokolDay = int.Parse(lastMissingProtokol.Substring(0, 2));
            // int lastMissingProtokolMonth = int.Parse(lastMissingProtokol.Substring(3, 2));
            // int lastMissingProtokolYear = int.Parse(lastMissingProtokol.Substring(6, 4));
            // DateTime lastMissingProtokolDate = new DateTime(lastMissingProtokolYear, lastMissingProtokolMonth, lastMissingProtokolDay);

            if (DateTime.Now.Date.ToString("dd.MM.yyyy").Equals(lastMissingProtokol) && args.Length == 2)
            {
                try
                {
                    string passwd = args[0];
                    string senderName = JsonSerializer.Deserialize<ConfigWeightNotes>(config)?.User.Name!;
                    Email.Send(passwd, args[1], new List<string>(),
                        "Липсващи кантарни бележки за месеца", "Поздрави,\n" + senderName, new string[] { missingMeasuresFile });
                    TextFile.Log("Изпратена справка за липсващи бележки");
                }
                catch (System.Exception e)
                {
                    TextFile.Log(e.Message);
                }

            }

            #region Попълва файла от спедиторите

            string file = JsonSerializer.Deserialize<ConfigWeightNotes>(config)?.Speditor.File!;
            string? planXlxFile = Directory.GetFiles(Environment.CurrentDirectory, file).Where(name => !name.Contains("попълнен")).FirstOrDefault();

            if (planXlxFile != null)
            {
                var speditorDailySheets = Controller.GetPlannedTrucksDaily(planXlxFile);
                string planXlxFileFilled = planXlxFile.Substring(0, planXlxFile.LastIndexOf('.')) + "-" + DateTime.Now.Day + "-попълнен.xlsx";

                StringBuilder body = new StringBuilder();
                body.AppendLine("Това е автоматично генериран е-мейл.");
                body.AppendLine("Warnings:");

                using (ExcelPackage package = new ExcelPackage(new FileInfo(planXlxFileFilled)))
                {
                    while (package.Workbook.Worksheets.Count > 0)
                    {
                        package.Workbook.Worksheets.Delete(0);
                    }

                    //foreach (var sheet in speditorDailySheets)
                    //{
                    string sheetKey = DateTime.Now.Day + "." + DateTime.Now.Month;
                    var sheetValue = speditorDailySheets[sheetKey];

                    ExcelWorksheet ws = package.Workbook.Worksheets.Add(sheetKey);

                    ws.DefaultRowHeight = 15;
                    ws.Column(1).Width = 5;
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

                    var measuresCurrentDay = measures.Where(x => x.FromDate.ToString("dd.M") == sheetKey).ToList();
                    int sumMeasuresCurrentDay = measuresCurrentDay.Sum(m => m.Netto);

                    int knfeCounter = 0;
                    int countFilled = 0;
                    foreach (var truckInfo in sheetValue)
                    {
                        try
                        {
                            sheetValue[truckInfo.Key].ProtokolNum = 0;

                            foreach (var measure in measuresCurrentDay)
                            {
                                if (TextFile.ReplaceCyrillic(measure.RegNum?.ToUpper().Trim()) == truckInfo.Key)
                                {
                                    sheetValue[truckInfo.Key].Netto = measure.Netto;
                                    sheetValue[truckInfo.Key].ProtokolNum = measure.ProtokolNum;
                                    countFilled++;
                                    measuresCurrentDay.Remove(measure);
                                    break;
                                }
                            }
                        }
                        catch (KeyNotFoundException knfe)
                        {
                            body.AppendLine($"{ws.Name} - No. {++knfeCounter}: {knfe.Message}");
                        }
                        dataTable.Rows.Add(truckInfo.Value.ProtokolNum, truckInfo.Value.TractorNum, truckInfo.Key,
                                    truckInfo.Value.Driver, truckInfo.Value.Egn, truckInfo.Value.Phone, truckInfo.Value.Netto);
                    }

                    foreach (var measure in measuresCurrentDay)
                    {
                        dataTable.Rows.Add(measure.ProtokolNum, measure.TractorNum, measure.RegNum,
                                    measure.Driver, measure.Egn, measure.Phone, measure.Netto);
                        countFilled++;
                    }

                    //add all the content from the DataTable, starting at cell A1
                    ws.Cells["A1"].LoadFromDataTable(dataTable, true);

                    int sumInXlxFile = 0;

                    for (int row = 1; row <= dataTable.Rows.Count + 1; row++)
                    {
                        for (int col = 1; col <= dataTable.Columns.Count; col++)
                        {
                            ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.LightGrid;
                            ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                            ws.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            ws.Cells[row, col].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            ws.Cells[row, col].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            ws.Cells[row, col].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        }

                        sumInXlxFile += ws.Cells[row + 1, dataTable.Columns.Count].GetValue<int>();
                    }

                    if (sumInXlxFile != sumMeasuresCurrentDay)
                    {
                        string error = "Нетото за деня се различава.";

                        if (args.Length > 0 && haveTrucksToday)
                        {
                            string passwd = args[0];
                            Email.Send(passwd, "sender", new List<string>(), "Справка автo ERROR", body.ToString(),
                            new string[] { todaysNotesFileRenamed, planXlxFileFilled });
                            error.Concat(" Беше изпратен мейл до разработчика");
                        }
                        TextFile.Log(error);
                        throw new ArgumentException(error);
                    }
                    else
                    {
                        ws.Cells[dataTable.Rows.Count + 2, dataTable.Columns.Count].Value = sumInXlxFile;
                        ws.Cells[dataTable.Rows.Count + 2, 1].Formula = countFilled.ToString();
                    }
                    //}
                    package.Save();

                    if (args.Length > 1)
                    {
                        string passwd = args[0];
                        string recipient = args[1];
                        List<string> ccRecipients = new List<string>();

                        if (args.Length > 2)
                        {
                            for (int i = 2; i < args.Length; i++)
                            {
                                ccRecipients.Add(args[i]);
                            }
                        }
                        string bodyStr = body.ToString();

                        if (haveTrucksToday && spravkaUpdated)
                        {
                            Email.Send(passwd, recipient, ccRecipients, "Справка автовезна р-к 3", body.ToString(),
                                new string[] { todaysNotesFileRenamed, planXlxFileFilled });
                            TextFile.Log(" Беше изпратен е-мейл до спедиторите");
                        }
                    }
                }
                TextFile.Log("OK");
            }
            else
            {
                TextFile.Log("За днес не са планирани камиони.");
            }
            #endregion
        }
    }
}