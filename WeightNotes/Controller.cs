using System.Data;
using System.Globalization;
using System.Text;
using Common;
using OfficeOpenXml;
using OfficeOpenXml.Style;

class Controller
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
        string shift = string.Empty;
        var firstMeasureTime = measures.FirstOrDefault().Value.BrutoTime;
        var lastMeasureTime = measures.LastOrDefault().Value.BrutoTime;

        if (lastMeasureTime.Day == firstMeasureTime.Day + 1)
        {
            shift = "-Нощна";
        }
        else if (lastMeasureTime.Day == firstMeasureTime.Day
            && firstMeasureTime.TimeOfDay > Config.startShift
            && lastMeasureTime.TimeOfDay < Config.endShift)
        {
            shift = "-Дневна";
        }

        return firstMeasureTime.ToString("yyyy-MM-dd") + shift + ".TXT";
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

    internal static void FillGeologInfo(Dictionary<int, Measure> measures, string xlsxFile)
    {
        int days = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

        DataTable dataTable = InsertGeologHeader();

        for (int day = 1; day <= days; day++)
        {
            var dailyMeasures = measures.Where(m =>
                m.Value.BrutoTime.Day == day && m.Value.BrutoTime.TimeOfDay >= Config.startShift && m.Value.BrutoTime.TimeOfDay < Config.endShift);

            var nightMeasures = measures.Where(m =>
                (m.Value.BrutoTime.Day == day && m.Value.BrutoTime.TimeOfDay >= Config.endShift) ||
                (m.Value.BrutoTime.Day == day + 1 && m.Value.BrutoTime.TimeOfDay < Config.startShift));

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

        using (ExcelPackage package = new ExcelPackage(new FileInfo(xlsxFile)))
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

    internal static void FillAllMeasures(Dictionary<int, Measure> measures, string allMeasuresFile)
    {
        StringBuilder allMeasures = new StringBuilder();
        allMeasures.AppendLine("No. ; Дата ; Ремарке ; Доставчик ; Място на разтоварване ; Клиент ; Вид товар ; Бруто kg ; Кант. бележка № ; Нето kg");
        foreach (var measure in measures)
        {
            allMeasures.AppendLine($"{measure.Value.TimeRegNum} ; {Config.clientInfo} ; {measure.Value.BrutoNeto}");
        }
        TextFile.SaveNew(Config.logPath, allMeasures, allMeasuresFile);

        // using (ExcelPackage package = new ExcelPackage(new FileInfo(allMeasuresFile)))
        // {
        //     while (package.Workbook.Worksheets.Count > 0)
        //     {
        //         package.Workbook.Worksheets.Delete(0);
        //     }
        //     ExcelWorksheet ws = package.Workbook.Worksheets.Add(allMeasuresFile.Replace("Всички ", string.Empty).Replace(".xlsx", string.Empty));
        //     var format = new ExcelTextFormat();
        //     format.Delimiter = ';';
        //     ws.Column(1).Width = 4;
        //     ws.Column(2).Width = 9;
        //     ws.Column(3).Width = 11;
        //     ws.Column(4).Width = 18;
        //     ws.Column(5).Width = 23;
        //     ws.Column(6).Width = 18;
        //     ws.Column(7).Width = 24;
        //     ws.Column(8).Width = 9;
        //     ws.Column(9).Width = 18;
        //     ws.Column(10).Width = 8;
        //     ws.Cells.AutoFitColumns();

        //     // Thread.CurrentThread.CurrentCulture = new CultureInfo("bg-BG")
        //     // {
        //     //     DateTimeFormat = { ShortDatePattern = "dd/MM/yy" }
        //     // };
        //     // ws.Column(2).Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.YearMonthPattern;

        //     ws.Column(2).Style.Numberformat.Format = "dd.MM.yy";

        //     //ws.Cells["A1"].LoadFromText(allMeasures.ToString(), format);
        //     //ws.Cells["A1:A2000"].Style.Numberformat.Format = "@";

        //     package.Save();
        // }
    }
}

