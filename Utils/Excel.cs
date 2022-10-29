using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using OfficeOpenXml;

// using NPOI.XSSF.UserModel;
// using NPOI.HSSF.UserModel;
// using NPOI.SS.UserModel;

namespace Utils
{
    public class Excel
    {
        public static Encoding srcEncoding = Encoding.GetEncoding("windows-1251");

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

        public static bool SaveNew(string logPath, FileInfo oldFile, string newInfo)
        {
            if (File.Exists(oldFile.FullName))
            {
                var oldTable = Import(oldFile.FullName);
                var newTable = Import(newInfo);
                if (AreTablesTheSame(oldTable, newTable))
                {
                    //File.WriteAllText(oldFile.FullName, newInfo);
                    TextFile.Log($" ### Файлът {oldFile.Name} беше обновен.", logPath);
                    return true;
                }
                else
                {
                    TextFile.Log($"Няма нова информация. Файлът {oldFile.Name} не беше обновен.", logPath);
                    return false;
                }
            }
            else
            {
                //File.WriteAllText(oldFile.FullName, newInfo);
                TextFile.Log($" ### Файлът {oldFile.Name} беше създаден.", logPath);
            }

            return true;
        }

        public static DataTable Import(string fileName)
        {
            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(fileName, false))
            {
                Sheet sheet = doc.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();  //Read the first Sheet from Excel file.

                Worksheet worksheet = (doc.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet; //Get the Worksheet instance.

                IEnumerable<Row> rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>(); //Fetch all the rows present in the Worksheet.

                // Create a new DataTable.
                DataTable dt = new DataTable();

                //Loop through the Worksheet rows.
                foreach (Row row in rows)
                {
                    //Use the first row to add columns to DataTable.
                    if (row.RowIndex.Value == 1)
                    {
                        foreach (Cell cell in row.Descendants<Cell>())
                        {
                            dt.Columns.Add(GetValue(doc, cell));
                        }
                    }
                    else
                    {
                        //Add rows to DataTable.
                        dt.Rows.Add();
                        int i = 0;
                        foreach (Cell cell in row.Descendants<Cell>())
                        {
                            dt.Rows[dt.Rows.Count - 1][i] = (GetValue(doc, cell));
                            i++;
                        }
                    }
                }
                // GridView1.DataSource = dt;
                // GridView1.DataBind();
                return dt;
            }
        }

        private static string GetValue(SpreadsheetDocument doc, Cell cell)
        {
            string? value = cell.CellValue?.InnerText;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements.GetItem(int.Parse(value)).InnerText;
            }
            return value;
        }

        private static bool AreTablesTheSame(DataTable tbl1, DataTable tbl2)
        {
            if (tbl1.Rows.Count != tbl2.Rows.Count || tbl1.Columns.Count != tbl2.Columns.Count)
                return false;


            for (int i = 0; i < tbl1.Rows.Count; i++)
            {
                for (int c = 0; c < tbl1.Columns.Count; c++)
                {
                    if (!Equals(tbl1.Rows[i][c], tbl2.Rows[i][c]))
                        return false;
                }
            }
            return true;
        }
    }
}