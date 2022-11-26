using System.Data;
using System.Globalization;
using System.Text;
using Utils;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class Controller
{
    internal static Dictionary<int, Measure> GetMeasures(string veznaFile)
    {
        List<string> lines = File.ReadAllLines(veznaFile, Excel.srcEncoding).ToList();
        var measures = new Dictionary<int, Measure>();
        int counter = 1;
        foreach (var line in lines)
        {
            var currentLine = Array.ConvertAll(line.Split("|"), cl => cl.Trim());

            if (!(line.StartsWith(" | N ")) && currentLine.Length == 11)
            {
                int id = int.Parse(currentLine[2]);
                string date = currentLine[3].Trim();
                string brutoTime = currentLine[6].Trim();

                Measure measure = new Measure(
                        counter++,
                        id,
                        DateTime.ParseExact(date + " " + brutoTime, "dd/MM/yy H:mm:ss", CultureInfo.InvariantCulture),
                        currentLine[4],
                        int.Parse(currentLine[5]),
                        int.Parse(currentLine[7])
                    );
                measures.TryAdd(id, measure);
            }
        }

        return measures;
    }

    internal static string SetFileName(string veznaFile)
    {
        var measures = GetMeasures(veznaFile);
        var period = File.ReadAllLines(veznaFile, Excel.srcEncoding)[3];
        var fromTo = period.Split(new string[] {"за периода от", "до"}, StringSplitOptions.TrimEntries);
        string from = fromTo[1];
        string to = fromTo[2];
        DateTime fromDateTime = DateTime.Parse(from);
        DateTime toDateTime = DateTime.Parse(to);
        string shift = string.Empty;
        // var firstMeasureTime = measures.FirstOrDefault().Value.BrutoTime;
        // var lastMeasureTime = measures.LastOrDefault().Value.BrutoTime;

        if (fromDateTime.Day + 1 == toDateTime.Day)
        {
            shift = "-Нощна";
        }
        else if (fromDateTime.Day == toDateTime.Day
            && fromDateTime.TimeOfDay > Config.beginShift
            && toDateTime.TimeOfDay < Config.endShift)
        {
            shift = "-Дневна";
        }

        return fromDateTime.ToString("yyyy-MM-dd") + shift + ".TXT";
    }

    private static DataTable InsertGeologHeader()
    {
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("Дата", typeof(string));
        dataTable.Columns.Add("Смяна", typeof(byte));
        dataTable.Columns.Add("Нето [тон]", typeof(decimal));
        dataTable.Columns.Add("Пепел [%]", typeof(string));
        dataTable.Columns.Add("Брой", typeof(int));
        return dataTable;
    }

    public static void FillGeologInfo(Dictionary<int, Measure> measures)
    {
        int days = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

        DataTable dataTable = InsertGeologHeader();

        for (int day = 1; day <= days; day++)
        {
            var dailyMeasures = measures.Where(m =>
                m.Value.BrutoTime.Day == day && m.Value.BrutoTime.TimeOfDay >= Config.beginShift && m.Value.BrutoTime.TimeOfDay < Config.endShift);

            var nightMeasures = measures.Where(m =>
                (m.Value.BrutoTime.Day == day && m.Value.BrutoTime.TimeOfDay >= Config.endShift) ||
                (m.Value.BrutoTime.Day == day + 1 && m.Value.BrutoTime.TimeOfDay < Config.beginShift));

            for (int shift = 1; shift <= 2; shift++)
            {
                var shiftMeasures = dailyMeasures;
                if (shift == 2)
                {
                    shiftMeasures = nightMeasures;
                }

                if (shiftMeasures.Count() == 0)
                {
                    dataTable.Rows.Add(day, shift);
                }
                else
                {
                    dataTable.Rows.Add(day, shift, shiftMeasures.Sum(s => s.Value.Neto) / 1000.0, "N/A", shiftMeasures.Count());
                }
            }
        }

        using (ExcelPackage package = new ExcelPackage(new FileInfo("Справка по смени.xlsx")))
        {
            while (package.Workbook.Worksheets.Count > 0)
            {
                package.Workbook.Worksheets.Delete(0);
            }
            ExcelWorksheet ws = package.Workbook.Worksheets.Add("Автовезна");
            ws.DefaultRowHeight = 13.5;
            ws.Cells["A1:D67"].Style.Font.Size = 10;
            ws.Cells["A1:D67"].Style.Font.Name = "Arial";

            ws.Column(1).Width = 11;
            ws.Column(2).Width = 7;
            ws.Column(3).Width = 11;
            ws.Column(4).Width = 11;
            ws.Column(5).Width = 9;

            ws.Row(5).Style.Font.Bold = true;
            ws.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[3, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            ws.Cells[3, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            ws.Cells[1, 2].Value = "Рекапитулация по дни";
            ws.Cells["A1"].Style.Font.Bold = true;
            ws.Cells["A3"].Style.Font.Bold = true;
            ws.Cells[3, 1].Value = "Период: ";
            ws.Cells[3, 2].Value = "от " + measures.FirstOrDefault().Value.BrutoTime;
            ws.Cells[3, 4].Value = " до " + measures.LastOrDefault().Value.BrutoTime;

            //add all the content from the DataTable, starting at given cell
            ws.Cells["A5"].LoadFromDataTable(dataTable, true);

            package.Save();
        }
    }

    public static void FillAllMeasures(Dictionary<int, Measure> measures)
    {
        string header = "No.;Дата;Ремарке;Доставчик;Дестинация;Клиент;Вид товар;Бруто kg;Кант.бел.№;Нето kg;Време бруто";
        string fileName = "Всички м." + measures.FirstOrDefault().Value.BrutoTime.Month;

        StringBuilder allMeasures = new StringBuilder();
        allMeasures.AppendLine(header);
        foreach (var measure in measures)
        {
            allMeasures.AppendLine($"{measure.Value.TimeRegNum};{Config.supplier};{Config.destination};{Config.client};{Config.load};" +
                            $"{measure.Value.BrutoNeto};{measure.Value.BrutoTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture)}");
        }

        TextFile.SaveNew(Config.logPath, allMeasures, fileName + ".csv");
    }

    public static void FillAllMeasures(Dictionary<int, Measure> measures, string fileNameXlsx)
    {
        string header = "No.;Дата;Ремарке;Доставчик;Дестинация;Клиент;Вид товар;Бруто kg;Кант.бел.№;Нето kg;Време бруто";

        using (var fs = new FileStream(fileNameXlsx, FileMode.Create, FileAccess.Write))
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet1 = workbook.CreateSheet("м." + measures.FirstOrDefault().Value.BrutoTime.Month);

            string[] headerSplit = header.Split(";", StringSplitOptions.RemoveEmptyEntries);

            var rowIndex = 0;
            IRow currentRow = sheet1.CreateRow(rowIndex);
            for (int col = 0; col <= 10; col++)
            {
                currentRow.CreateCell(col).SetCellValue(headerSplit[col]);
                // sheet1.AutoSizeColumn(col); // throws FileNotFoundException: Could not load file or assembly 'System.Drawing.Common, Version=4.0.0.0,
            }

            rowIndex++;
            foreach (var measure in measures)
            {
                currentRow = sheet1.CreateRow(rowIndex);
                currentRow.CreateCell(0).SetCellValue(measure.Value.Num);
                currentRow.CreateCell(1).SetCellValue(measure.Value.BrutoTime.ToString("dd/MM/yy", CultureInfo.InvariantCulture));
                currentRow.CreateCell(2).SetCellValue(measure.Value.RegNum);
                currentRow.CreateCell(3).SetCellValue(Config.supplier);
                currentRow.CreateCell(4).SetCellValue(Config.destination);
                currentRow.CreateCell(5).SetCellValue(Config.client);
                currentRow.CreateCell(6).SetCellValue(Config.load);
                currentRow.CreateCell(7).SetCellValue(measure.Value.Bruto);
                currentRow.CreateCell(8).SetCellValue(measure.Value.Id);
                currentRow.CreateCell(9).SetCellValue(measure.Value.Bruto - measure.Value.Tara);
                currentRow.CreateCell(10).SetCellValue(measure.Value.BrutoTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
                rowIndex++;
            }

            // sheet1.AddMergedRegion(new CellRangeAddress(0, 0, 0, 10));
            // row.Height = 30 * 80;
            // row.CreateCell(0).SetCellValue("this is content");
            // sheet1.AutoSizeColumn(0);
            // rowIndex++;

            // var sheet2 = workbook.CreateSheet("Sheet2");
            // var style1 = workbook.CreateCellStyle();
            // style1.FillForegroundColor = HSSFColor.Blue.Index2;
            // style1.FillPattern = FillPattern.SolidForeground;

            // var style2 = workbook.CreateCellStyle();
            // style2.FillForegroundColor = HSSFColor.Yellow.Index2;
            // style2.FillPattern = FillPattern.SolidForeground;

            // var cell2 = sheet2.CreateRow(0).CreateCell(0);
            // cell2.CellStyle = style1;
            // cell2.SetCellValue(0);

            workbook.Write(fs);
            TextFile.Log($"Файлът {fileNameXlsx} е обновен", Config.logPath);
        }
    }

    public static string FillMissingNotes(Dictionary<int, Measure> measures)
    {
        StringBuilder missingNotes = new StringBuilder();
        missingNotes.AppendLine("Дата;Номер");
        int firstId = measures.FirstOrDefault().Key;
        int countId = firstId - 1;
        foreach (var measure in measures)
        {
            countId++;
            if (measure.Key != countId)
            {
                while (measure.Key != countId)
                {
                    Measure missingMeasure = new Measure(countId, measure.Value.BrutoTime);
                    missingNotes.AppendLine($"{missingMeasure.BrutoTime.ToString("dd.MM.yyyy")};{countId}");
                    countId++;
                }
            }
        }
        return TextFile.SaveNew(Config.logPath, missingNotes, Config.logPath + "Липсващи бележки.csv");
    }

    public static string FillPlan(Dictionary<int, Measure> measures)
    {
        string planXlxFile = Directory.GetFiles(Config.logPath, Config.speditorFile).Where(name => !name.Contains("попълнен")).FirstOrDefault();

        if (planXlxFile != null)
        {
            List<string> warnings = new List<string>();
            Dictionary<string, Dictionary<string, Measure>> speditorDailySheets = GetPlannedTrucks(planXlxFile, out warnings);

            string planFileFilled = planXlxFile.Substring(0, planXlxFile.LastIndexOf('.')) + "-" + DateTime.Now.Day + "-попълнен.xlsx";

            using (ExcelPackage package = new ExcelPackage(new FileInfo(planFileFilled)))
            {
                while (package.Workbook.Worksheets.Count > 0)
                {
                    package.Workbook.Worksheets.Delete(0);
                }

                string today = DateTime.Today.ToString("dd.MM");
                string yesterday = DateTime.Today.AddDays(-1).ToString("dd.MM");

                Dictionary<string, Dictionary<string, Measure>> sheetsFromLastTwoDays = speditorDailySheets
                    .Where(s => s.Key == today || s.Key == yesterday)
                    .ToDictionary(x => x.Key, x => x.Value);

                foreach (var sheet in sheetsFromLastTwoDays)
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets.Add(sheet.Key);

                    ws.DefaultRowHeight = 15;
                    ws.Column(1).Width = 5;
                    ws.Column(2).Width = 11;
                    ws.Column(3).Width = 11;
                    ws.Column(4).Width = 40;
                    ws.Column(5).Width = 13;
                    ws.Column(6).Width = 11;
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
                    dataTable.Columns.Add("Време бруто", typeof(string));

                    var measuresCurrentDay = measures.Where(x => x.Value.BrutoTime.ToString("dd.MM") == sheet.Key).ToList();
                    int sumMeasuresCurrentDay = measuresCurrentDay.Sum(m => m.Value.Neto);

                    int knfeCounter = 0;
                    int countFilled = 0;
                    foreach (var truckInfo in sheet.Value)
                    {
                        try
                        {
                            sheet.Value[truckInfo.Key].Id = 0;

                            foreach (var measure in measuresCurrentDay)
                            {
                                if (TextFile.ReplaceCyrillic(measure.Value.RegNum.ToUpper().Trim()) == truckInfo.Key)
                                {
                                    sheet.Value[truckInfo.Key].Bruto = measure.Value.Bruto;
                                    sheet.Value[truckInfo.Key].Tara = measure.Value.Tara;
                                    sheet.Value[truckInfo.Key].BrutoTime = measure.Value.BrutoTime;
                                    sheet.Value[truckInfo.Key].Id = measure.Value.Id;
                                    countFilled++;
                                    measuresCurrentDay.Remove(measure);
                                    break;
                                }
                            }
                        }
                        catch (KeyNotFoundException knfe)
                        {
                            TextFile.Log($"{ws.Name} - No. {++knfeCounter}: {knfe.Message}", Config.logPath);
                        }
                        dataTable.Rows.Add(truckInfo.Value.Id, truckInfo.Value.TractorNum, truckInfo.Key,
                                    truckInfo.Value.Driver, truckInfo.Value.Egn, truckInfo.Value.Phone,
                                    truckInfo.Value.Bruto - truckInfo.Value.Tara, truckInfo.Value.BrutoTime.ToString("HH:mm"));
                    }

                    foreach (var measure in measuresCurrentDay)
                    {
                        dataTable.Rows.Add(measure.Value.Id, measure.Value.TractorNum, measure.Value.RegNum,
                                    measure.Value.Driver, measure.Value.Egn, measure.Value.Phone,
                                    measure.Value.Bruto - measure.Value.Tara, measure.Value.BrutoTime.ToString("HH:mm"));
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

                        sumInXlxFile += ws.Cells[row + 1, dataTable.Columns.Count - 1].GetValue<int>();
                    }

                    if (sumInXlxFile != sumMeasuresCurrentDay)
                    {
                        string error = "Нетото за деня се различава.";
                        TextFile.Log(error, Config.logPath);
                        return error;
                    }
                    else
                    {
                        ws.Cells[dataTable.Rows.Count + 2, dataTable.Columns.Count - 1].Value = sumInXlxFile;
                        ws.Cells[dataTable.Rows.Count + 2, 1].Formula = countFilled.ToString();
                    }
                }
                var lastWs = package.Workbook.Worksheets.LastOrDefault();
                int counter = 0;
                foreach (var warning in warnings)
                {
                    lastWs.Cells[lastWs.Dimension.Rows + 1, 1].Value = warning;
                }
                package.Save();
            }
            return planFileFilled;
        }
        else
        {
            TextFile.Log("За днес не са планирани камиони.", Config.logPath);
            return null;
        }
    }

    private static Dictionary<string, Dictionary<string, Measure>> GetPlannedTrucks(string planXlxFile, out List<string> warnings)
    {
        var worksheets = Excel.ReadWithEPPlus<List<DataTable>>(planXlxFile);
        Dictionary<string, Dictionary<string, Measure>> trucksPlannedMonthly = new Dictionary<string, Dictionary<string, Measure>>();
        warnings = new List<string>();

        foreach (var ws in worksheets)
        {
            var trucksPlannedDaily = new Dictionary<string, Measure>();

            foreach (DataRow dataRow in ws.Rows)
            {
                try
                {
                    var truck = new Measure(
                        String.IsNullOrEmpty(dataRow.Field<string>("№")) ? 0 : int.Parse(dataRow.Field<string>("№")),
                        DateTime.ParseExact(ws.TableName, "d.M", CultureInfo.InvariantCulture),
                        TextFile.ReplaceCyrillic(dataRow.Field<string>("ВЛЕКАЧ")?.Replace(" ", string.Empty).Replace("-", string.Empty).ToUpper().Trim()),
                        TextFile.ReplaceCyrillic(dataRow.Field<string>("РЕМАРКЕ")?.Replace(" ", string.Empty).Replace("-", string.Empty).ToUpper().Trim()),
                        dataRow.Field<string>("ШОФЬОР"),
                        dataRow.Field<string>("ЕГН"),
                        dataRow.Field<string>("ТЕЛЕФОН")
                        );

                    if (!trucksPlannedDaily.ContainsKey(truck.RegNum))
                    {
                        trucksPlannedDaily.Add(truck.RegNum, truck);
                    }
                    else if (truck.BrutoTime >= DateTime.Now.AddDays(-1))
                    {
                        string warning = ($"Ремарке с № {truck.RegNum} е планирано повече от 1 път за {truck.BrutoTime.Date.ToString("dd.M.yyyy")}");
                        warnings.Add(warning);
                    }
                }
                catch (NullReferenceException nre)
                {
                    Console.WriteLine(nre.Message);
                }
            }

            trucksPlannedMonthly.Add(ws.TableName, trucksPlannedDaily);
        }
        return trucksPlannedMonthly;
    }
}