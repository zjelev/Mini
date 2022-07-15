using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace ExcelUtils
{
    public class WeightNotes
    {
        public static DailyTrucksInfo GetTotalForTheDay(string note)
        {
            string[] lines = File.ReadAllLines(note, Program.srcEncoding);
            string supplier = lines[0].Trim();
            string header = lines[1].Trim();
            string periodTemp = lines[3].Trim();
            string period = periodTemp.Substring(0, 13);
            string from = periodTemp.Substring(14, 14).Trim();

            DateTime fromDate;
            DateTime toDate;
            string to;

            if (from.Length > 8)
            {
                fromDate = DateTime.ParseExact(from, "dd/MM/yy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                to = periodTemp.Substring(periodTemp.Length - 14, 14).Trim();
                toDate = DateTime.ParseExact(to, "dd/MM/yy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                fromDate = DateTime.ParseExact(from, "dd/MM/yy", System.Globalization.CultureInfo.InvariantCulture);
                to = periodTemp.Substring(periodTemp.Length - 8, 8).Trim();
                toDate = DateTime.ParseExact(to, "dd/MM/yy", System.Globalization.CultureInfo.InvariantCulture);
            }

            string condition = lines[4].Trim();

            int totalLineNum = 5;
            while (!lines[totalLineNum].StartsWith(" | ВСИЧКО:"))
            {
                totalLineNum++;
            }

            string[] total = Array.ConvertAll(lines[totalLineNum].Split(new char[] { '|', ' ' },
                StringSplitOptions.RemoveEmptyEntries), cl => cl.Trim());
            int totalNet = int.Parse(total[total.Length - 1]);

            int lastMeasureLineNum = totalLineNum - 2;
            string[] lastMeasure = new string[] { };

            while (lastMeasure.Length != 10)
            {
                lastMeasure = Array.ConvertAll(lines[lastMeasureLineNum--].Split('|', StringSplitOptions.RemoveEmptyEntries), cl => cl.Trim());
            }
            string noteFileName = Path.GetFileName(note);
            int shift = 1;
            if (noteFileName.Length == 16)
            {
                shift = int.Parse(noteFileName.Substring(noteFileName.Length - 5, 1));
                shift = shift > 2 ? 1 : shift;
            }
            DailyTrucksInfo dailyTrucksInfo = new DailyTrucksInfo(fromDate, shift, totalNet / 1000.0m, int.Parse(lastMeasure[1]));

            return dailyTrucksInfo;
        }

        private static DataTable InsertHeader()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Дата", typeof(string));
            dataTable.Columns.Add("Смяна", typeof(byte));
            dataTable.Columns.Add("Нето [тон]", typeof(decimal));
            dataTable.Columns.Add("Пепел [%]", typeof(string));
            dataTable.Columns.Add("Брой", typeof(int));
            return dataTable;
        }

        public static void InsertDailyTotals(
            // List<DailyTrucksInfo> trucksInfos,
            Dictionary<Tuple<int, int>, DailyTrucksInfo> dailyWeights,
            string xlsxFile, string startDate, string endDate)
        {
            DataTable dataTable = InsertHeader();

            foreach (var row in dailyWeights)
            {
                // dataTable.Rows.Add(trucksInfo.DateString, trucksInfo.Shift, trucksInfo.NetWeightInTons, trucksInfo.AshesPercent, trucksInfo.NumOfTrucks);
                if (row.Value == null)
                {
                    dataTable.Rows.Add(row.Key.Item1, row.Key.Item2);

                }
                else
                {
                    dataTable.Rows.Add(row.Key.Item1, row.Key.Item2, row.Value.NetWeightInTons, row.Value.AshesPercent, row.Value.NumOfTrucks);
                }

            }

            using (ExcelPackage package = new ExcelPackage(new FileInfo(xlsxFile)))
            {
                if (package.Workbook.Worksheets.Count > 0)
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
                ws.Column(5).Width = 14;

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
                ws.Cells[3, 2].Value = "от " + startDate;
                ws.Cells[3, 4].Value = " до " + endDate;

                //add all the content from the DataTable, starting at cell A1
                ws.Cells["A5"].LoadFromDataTable(dataTable, true);

                int rows = dataTable.Rows.Count;
                int columns = dataTable.Columns.Count;
                //worksheet.Cells[2, 1, rows + 1, columns].Sort(new[] { 0, 1 }); // skip the header
                package.Save();
            }
        }

        public static List<Measure> GetMeasures(string file)
        {
            string[] lines = File.ReadAllLines(file, Program.srcEncoding);
            int headerLineNum = 5;
            do
            {
                headerLineNum++;
            } while (!lines[headerLineNum].StartsWith(" | N"));

            string[] tableHeader = Array.ConvertAll(lines[headerLineNum].Split('|'), th => th.Trim());

            List<Measure> measures = new List<Measure>();
            int measuresLineNum = headerLineNum + 2;

            while (measuresLineNum < lines.Length - 1)
            {
                string[] currentLine = Array.ConvertAll(lines[measuresLineNum].Split("|"), cl => cl.Trim());
                int lineLength = currentLine.Length;

                if (!(lines[measuresLineNum].StartsWith(" | N ")) && lineLength == 11)
                {
                    Measure measure = new Measure(
                            int.Parse(currentLine[1]),
                            int.Parse(currentLine[2]),
                            currentLine[3],
                            currentLine[4],
                            int.Parse(currentLine[5]),
                            currentLine[6],
                            int.Parse(currentLine[7]),
                            currentLine[8],
                            int.Parse(currentLine[9])
                        );
                    measures.Add(measure);
                }
                measuresLineNum++;
            }

            // int totalLoadNum = measuresLineNum;
            // while (!lines[totalLoadNum].StartsWith(" |  Общо товар:"))
            // {
            //     totalLoadNum++;
            // }
            // string[] totalLoad = Array.ConvertAll(lines[totalLoadNum].Split(new char[] { '|', ' ' },
            //     StringSplitOptions.RemoveEmptyEntries), cl => cl.Trim());

            // int totalContactorLineNum = totalLoadNum;
            // while (!lines[totalContactorLineNum].StartsWith(" |  Общо за Контрагент:"))
            // {
            //     totalContactorLineNum++;
            // }
            // string[] totalContractor = Array.ConvertAll(lines[totalContactorLineNum].Split(new char[] { '|', ' ' },
            //     StringSplitOptions.RemoveEmptyEntries), cl => cl.Trim());

            return measures;
        }
    }
}