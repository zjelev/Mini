using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.IO;
using System;
using NPOI.SS.UserModel;

namespace ExcelUtils
{
    public class Npoi
    {
        public IWorkbook ReadWorkbook(string path)
        {
            IWorkbook book;

            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                // Try to read workbook as XLSX:
                try
                {
                    book = new XSSFWorkbook(fs);
                }
                catch
                {
                    book = null;
                }

                // If reading fails, try to read workbook as XLS:
                if (book == null)
                {
                    book = new HSSFWorkbook(fs);
                }

                return book;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "Excel read error");
                return null;
            }
        }
    }
}