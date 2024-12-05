using System.Data;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Utils.Services;

public class SpeditorService
{
    public const string sprt = ";";
    private IEnumerable<Izmervane> _measures;
    private DataTable dataTable;
    private static Dictionary<string, List<string>> _warnings;
    private string today;
    public SpeditorService(IEnumerable<Izmervane> measures)
    {
        _measures = measures;
        dataTable = new DataTable();
        _warnings = new Dictionary<string, List<string>>();
        today = DateTime.Now.AddSeconds(-20).ToString("d.M.yyyy");
    }

    public void GenerateSpravka(string fileName)
    {
        Dictionary<string, string> plannedTrucksToday = GetTrucksPlannedMonthly()?.Where(w => w.Key == today).FirstOrDefault().Value;

        string header = Config.supplier + File.ReadAllText(Config.wwwRootPath + "\\header.txt", Excel.srcEncoding);
        string[] headerSplit = header.Split(new string[] { sprt, "{0}", "{1}", "{2}", "{3}" }, StringSplitOptions.RemoveEmptyEntries);
        string product = _measures.FirstOrDefault().Product.ProductName == _measures.LastOrDefault().Product.ProductName ?
            _measures.FirstOrDefault().Product.ProductName : string.Empty;

        using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);

        IWorkbook workbook = new XSSFWorkbook();
        ISheet sheet1 = workbook.CreateSheet(_measures.LastOrDefault().Brutotm.ToString(Config.dateFormat));

        var style = (XSSFCellStyle)workbook.CreateCellStyle();
        style.Alignment = HorizontalAlignment.Left;
        style.VerticalAlignment = VerticalAlignment.Top;
        style.BorderBottom = BorderStyle.Thin;
        style.BorderTop = BorderStyle.Thin;
        style.BorderLeft = BorderStyle.Thin;
        style.BorderRight = BorderStyle.Thin;

        var rowIndex = 0;
        IRow currentRow = sheet1.CreateRow(rowIndex);
        currentRow.CreateCell(2).SetCellValue(headerSplit[0]);
        rowIndex++;
        currentRow = sheet1.CreateRow(rowIndex);
        currentRow.CreateCell(2).SetCellValue(headerSplit[1]);
        rowIndex++;
        currentRow = sheet1.CreateRow(rowIndex);
        currentRow.CreateCell(1).SetCellValue(headerSplit[2] + " " + _measures.FirstOrDefault().Brutotm.Date +
            " " + headerSplit[3] + " " + DateTime.Now.ToString() + " " + headerSplit[4] + product);
        rowIndex++;
        currentRow = sheet1.CreateRow(++rowIndex);
        for (int col = 0; col < headerSplit.Length - 5; col++)
        {
            currentRow.CreateCell(col).SetCellValue(headerSplit[col + 5]);
            currentRow.GetCell(col).CellStyle = style;
            //sheet1.AutoSizeColumn(col);
        }

        rowIndex++;
        int counter = 1;
        foreach (var measure in _measures)
        {
            var plrem = TextFile.ReplaceCyrillic(measure.Plrem);
            currentRow = sheet1.CreateRow(rowIndex);
            currentRow.CreateCell(0).SetCellValue(counter++);
            currentRow.CreateCell(1).SetCellValue((int)measure.Kanbel);
            currentRow.CreateCell(2).SetCellValue(measure.Brutotm.ToString(Config.dateFormat));
            currentRow.CreateCell(3).SetCellValue(measure.Truck != null ? measure.Truck.Plvl : string.Empty);
            currentRow.CreateCell(4).SetCellValue(measure.Plrem);
            currentRow.CreateCell(5).SetCellValue((int)measure.Bruto);
            currentRow.CreateCell(6).SetCellValue(measure.Brutotm.ToString("HH:mm"));
            currentRow.CreateCell(7).SetCellValue((int)measure.Tara);
            currentRow.CreateCell(8).SetCellValue(measure.Taratm != null ? measure.Taratm.ToString("HH:mm") : string.Empty);
            currentRow.CreateCell(9).SetCellValue((int)measure.Bruto - (int)measure.Tara);
            currentRow.CreateCell(10).SetCellValue(measure.Vodmp != null ? measure.Vodmp.Name + " " + measure.Vodmp.Sname + " " + measure.Vodmp.Fam : string.Empty);
            currentRow.CreateCell(11).SetCellValue(measure.Company != null ? measure.Company.Name : string.Empty);

            var key = plannedTrucksToday?.FirstOrDefault(kvp => kvp.Value == plrem).Key;
            if (key != null)
            {
                currentRow.CreateCell(12).SetCellValue(key);
                plannedTrucksToday.Remove(key);
            }

            var cells = currentRow.Cells;
            foreach (var cell in cells)
                cell.CellStyle = style;

            rowIndex++;
        }
        currentRow = sheet1.CreateRow(rowIndex);
        currentRow.CreateCell(8).SetCellValue("ВСИЧКО:");
        currentRow.CreateCell(9).SetCellValue((int)_measures.Sum(m => m.Neto));

        if (_warnings.ContainsKey(today))
        {
            List<string> warningsToday = _warnings[today];
            foreach (var warning in warningsToday)
            {
                rowIndex++;
                currentRow = sheet1.CreateRow(rowIndex);
                currentRow.CreateCell(0).SetCellValue(warning);
            }
        }

        sheet1.SetColumnWidth(0, 4 * 256);
        sheet1.SetColumnWidth(1, 9 * 256);
        sheet1.SetColumnWidth(3, 13 * 256);
        sheet1.SetColumnWidth(4, 12 * 256);
        sheet1.SetColumnWidth(6, 6 * 256);
        sheet1.SetColumnWidth(8, 8 * 256);
        sheet1.SetColumnWidth(10, 28 * 256);
        sheet1.SetColumnWidth(11, 20 * 256);
        sheet1.SetColumnWidth(12, 10 * 256);
        sheet1.SetColumnWidth(13, 10 * 256);

        workbook.Write(fs);
    }

    public static Dictionary<string, Dictionary<string, string>> GetTrucksPlannedMonthly()
    {
        string planXlxFile = Directory.GetFiles(Config.opisPath, Config.speditorFile).FirstOrDefault();
        var warnings = new Dictionary<string, List<string>>();

        if (planXlxFile != null)
        {
            var worksheets = Excel.ReadWithEPPlus<List<DataTable>>(planXlxFile);
            var trucksPlannedMonthly = new Dictionary<string, Dictionary<string, string>>();
            foreach (var ws in worksheets)
            {
                //ws = worksheets.Where(w => w.TableName == today).FirstOrDefault();
                var trucksPlannedDaily = new Dictionary<string, string>();

                foreach (DataRow dataRow in ws.Rows)
                {
                    try
                    {
                        var plRem = TextFile.ReplaceCyrillic(dataRow.Field<string>("РЕМАРКЕ")?.Replace(" ", string.Empty).Replace("-", string.Empty).ToUpper().Trim());
                        var nlReference = String.Empty;
                        if (dataRow.Table.Columns.Contains("NL"))
                            nlReference = dataRow.Field<string>("NL").Trim();

                        if (plRem != null && nlReference != null)
                        {
                            try
                            {
                                trucksPlannedDaily.TryAdd(nlReference, plRem);
                            }
                            catch
                            {
                                string warning = $"{nlReference} е планирано повече от 1 път";
                                if (!_warnings.ContainsKey(ws.TableName))
                                    _warnings.Add(ws.TableName, new List<string>());

                                _warnings[ws.TableName].Add(warning);
                            }
                        }
                    }
                    catch (NullReferenceException nre)
                    {
                        TextFile.Log(DateTime.Now.ToString() + " " + nre.Message);
                    }
                }

                trucksPlannedMonthly.Add(ws.TableName, trucksPlannedDaily);
            }
            return trucksPlannedMonthly;
        }
        else return null;
    }
}