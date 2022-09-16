using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using OfficeOpenXml;
// using NPOI.XSSF.UserModel;
// using NPOI.HSSF.UserModel;
// using NPOI.SS.UserModel;

namespace ExcelUtils
{
    public class Common
    {
        public static Encoding srcEncoding = Encoding.GetEncoding("windows-1251");
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

        // public IWorkbook ReadExcelWorkbook(string path)
        // {
        //     IWorkbook book;

        //     try
        //     {
        //         FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        //         // Try to read workbook as XLSX:
        //         try
        //         {
        //             book = new XSSFWorkbook(fs);
        //         }
        //         catch
        //         {
        //             book = null;
        //         }

        //         // If reading fails, try to read workbook as XLS:
        //         if (book == null)
        //         {
        //             book = new HSSFWorkbook(fs);
        //         }

        //         return book;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine(ex.Message, "Excel read error");
        //         return null;
        //     }
        // }

        public static T ReadFromExcel<T>(string path, bool hasHeader = true)
        {
            using (var excelPack = new ExcelPackage())
            {
                //Load excel stream
                using (var stream = File.OpenRead(path))
                {
                    excelPack.Load(stream);
                }

                List<DataTable> excelSheetsAsTables = new List<DataTable>();
                //Lets Deal with first worksheet.(You may iterate here if dealing with multiple sheets)
                foreach (var ws in excelPack.Workbook.Worksheets)
                {
                    DataTable excelasTable = new DataTable(ws.Name);
                    //Get all details as DataTable -because Datatable make life easy :)
                    foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                    {
                        //Get colummn details
                        if (!string.IsNullOrEmpty(firstRowCell.Text))
                        {
                            string firstColumn = string.Format("Column {0}", firstRowCell.Start.Column);
                            excelasTable.Columns.Add(hasHeader ? firstRowCell.Text : firstColumn);
                        }
                    }
                    var startRow = hasHeader ? 2 : 1;
                    //Get row details
                    for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                    {
                        var wsRow = ws.Cells[rowNum, 1, rowNum, excelasTable.Columns.Count];
                        DataRow row = excelasTable.Rows.Add();
                        foreach (var cell in wsRow)
                        {
                            row[cell.Start.Column - 1] = cell.Text;
                        }
                    }

                    excelSheetsAsTables.Add(excelasTable);
                }

                //Get everything as generics and let end user decides on casting to required type
                //string json = JsonSerializer.Serialize(excelasTable);
                var generatedType =
                    //JsonSerializer.Deserialize<T>(json)
                    excelSheetsAsTables
                    ;
                return (T)Convert.ChangeType(generatedType, typeof(T));
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
    }
}
