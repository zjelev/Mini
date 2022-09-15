using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace ExcelUtils
{
    public class CodeSumsTotal
    {
        public static void AddHeaderToExistingFile(string file)
        {
            string fileName, month, podName, podNumberStr;
            int year;
            Common.GetHeaderFromLeon(file, out fileName, out year, out month, out podName, out podNumberStr);

            using (ExcelPackage package = new ExcelPackage(new FileInfo(file)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.First(); //select sheet here
                // int totalRows = worksheet.Dimension.End.Row;
                // int totalColumns = worksheet.Dimension.End.Column;
                string a1 = Convert.ToString(worksheet.Cells[1, 1].Value);
                if (a1 == "Код")
                {
                    worksheet.InsertRow(1, 5);
                    worksheet.Cells[1, 2].Value = "ОБЩА";
                    //worksheet.Cells[1, 2].Style.Font.Bold = true;
                    worksheet.Cells[2, 2].Value = "СПРАВКА";
                    worksheet.Cells[3, 2].Value = $"НА {podName}{podNumberStr}";
                    string period = "МЕСЕЦ";
                    if (fileName.Length == 17)
                    {
                        month = String.Empty;
                        period = "ГОДИНА";
                    }
                    worksheet.Cells[4, 2].Value = $"ЗА {period} {month}{year}";
                    worksheet.Cells[5, 2].Value = $"";
                    worksheet.Cells["B1:B5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    package.Save();
                }
            }
        }

        internal static void AddSumsByCode(string file, Dictionary<(int, string), CodeSum> codesSums)
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(file)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.First();

                for (int i = 7; i <= worksheet.Dimension.End.Row; i++)
                {
                    int code = Convert.ToInt32(worksheet.Cells[i, 1].Value);
                    string name = Convert.ToString(worksheet.Cells[i, 2].Value);
                    decimal sum = Convert.ToDecimal(worksheet.Cells[i, 3].Value);
                    decimal hours = Convert.ToDecimal(worksheet.Cells[i, 4].Value);
                    (int, string) key = new (code, name);
                    CodeSum codeSum = new CodeSum(code, sum, hours);
                    if (codesSums.ContainsKey(key))
                    {
                        codesSums[key].Sum += sum;
                        codesSums[key].Hours += hours;
                    }
                    else
                    {
                        codesSums.Add(key, codeSum);
                    }
                }
            }
        }

        internal static void InsertCodesSumsInXlsx(Dictionary<(int, string), CodeSum> codesSums, string fileName)
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(fileName)))
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("Код", typeof(int));
                dataTable.Columns.Add("Име", typeof(string));
                dataTable.Columns.Add("Сума", typeof(decimal));
                dataTable.Columns.Add("Часове", typeof(decimal));

                foreach (var row in codesSums)
                {
                    dataTable.Rows.Add(row.Key.Item1, row.Key.Item2, row.Value.Sum, row.Value.Hours);
                }

                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet 1");
                //add all the content from the DataTable, starting at cell A1
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);

                int rows = dataTable.Rows.Count;
                int columns = dataTable.Columns.Count;
                worksheet.Cells[2, 1, rows + 1, columns].Sort(new[] { 0, 1 }); // skip the header
                package.Save();
            }
        }
    }
}