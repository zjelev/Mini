using System.Globalization;
using System.Text;
using System.Text.Json;
using Common;

namespace WeightNotes
{
    internal class Program
    {
        private static string config = File.ReadAllText("config.json");
        private static string logPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "AvtoveznaMonthly" + Path.DirectorySeparatorChar;

        private static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

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
                        TextFile.Log(iox.Message, logPath);
                    }
                }
                else
                {
                    TextFile.Log("Справката не е обновена.", logPath);
                }
            }
            else
            {
                TextFile.Log("Днес не е правена справка", logPath);
            }

            string[] weightNotesTxtFiles = Directory.GetFiles(Environment.CurrentDirectory, "20*.txt");
            var measures = new List<Measure>();

            int days = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            if (weightNotesTxtFiles.Length > days * 2)
            {
                string error = $" В директорията има повече от {days * 2} файла. Моля махнете тези, които са от предходни месеци.";
                TextFile.Log(error, logPath);
                throw new ArgumentException(error);
            }

            if (weightNotesTxtFiles.Length == 0)
            {
                string error = "В директорията няма файлове.";
                TextFile.Log(error, logPath);
                throw new ArgumentException(error);
            }

            var dailyWeights = new Dictionary<(int, int), DailyTrucksGeologInfo>();

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
                    TextFile.Log($" Името на файла {Path.GetFileName(note)} не съответства на датата в него. Файлът ще бъде преименуван като датата в него.", logPath);
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
                TextFile.Log(error, logPath);
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

            string missingMeasuresFile = logPath + "Липсващи бележки.csv";
            File.WriteAllText(missingMeasuresFile, missingProtokols.ToString(), Excel.srcEncoding);
            string lastMissingProtokol = missingProtokols.Remove(0, missingProtokols.Length - 17).ToString().Substring(0, 10);

            if (DateTime.Now.Date.ToString("dd.MM.yyyy").Equals(lastMissingProtokol) && args.Length == 2)
            {
                try
                {
                    string passwd = args[0];
                    string senderName = JsonSerializer.Deserialize<ConfigWeightNotes>(config).User.Name;
                    Email.Send(passwd, args[1], new List<string>(),
                        "Липсващи кантарни бележки за месеца", "Поздрави,\n" + senderName, new string[] { missingMeasuresFile });
                    TextFile.Log("Изпратена справка за липсващи бележки", logPath);
                }
                catch (System.Exception e)
                {
                    TextFile.Log(e.Message, logPath);
                }
            }

            string speditorFile = JsonSerializer.Deserialize<ConfigWeightNotes>(config).Speditor.File;
            string planXlxFileFilled = Controller.FillPlan(measures, speditorFile, logPath);

            StringBuilder body = new StringBuilder();

            if (planXlxFileFilled == "Нетото за деня се различава.")
            {
                if (args.Length > 0 && haveTrucksToday)
                {
                    string passwd = args[0];
                    Email.Send(passwd, "sender", new List<string>(), "Справка автo ERROR", body.ToString(),
                    new string[] { todaysNotesFileRenamed, planXlxFileFilled });
                    planXlxFileFilled.Concat(" Беше изпратен мейл до разработчика");
                }
                TextFile.Log(planXlxFileFilled, logPath);
                return;
            }

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

                body.AppendLine("Това е автоматично генериран е-мейл.");

                if (haveTrucksToday && spravkaUpdated)
                {
                    Email.Send(passwd, recipient, ccRecipients, "Справка автовезна р-к 3", body.ToString(),
                        new string[] { todaysNotesFileRenamed, planXlxFileFilled });
                    TextFile.Log(" Беше изпратен е-мейл до спедиторите", logPath);
                }
            }

            TextFile.Log("OK", logPath);
        }
    }
}