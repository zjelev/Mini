using System;
using System.IO;

namespace ExcelUtils
{
    public class Common
    {
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
    }
}