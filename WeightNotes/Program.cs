using System.Text;
using System.Text.Json;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using Utils;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var newVeznaFiles = TextFile.CompareDirs($"\\\\{Config.veznaHost}\\{Config.veznaPath}", Config.veznaFilePattern, Environment.CurrentDirectory, "20*.txt");
string newFileName = string.Empty;
foreach (var newVeznaFile in newVeznaFiles)
{
    newFileName = Controller.SetFileName(newVeznaFile.FullName);
    File.Copy(newVeznaFile.FullName, newFileName, true);
    TextFile.Log("### Справката за деня беше копирана от " + newVeznaFile.FullName + " в " + newFileName, Config.logPath);
}

var files = Directory.GetFiles(Environment.CurrentDirectory, "*.TXT").Where(name => !name.EndsWith("на.TXT"));
Dictionary<int, Measure> measures = new Dictionary<int, Measure>();

foreach (var file in files)
{
    Dictionary<int, Measure> currentMeasures = Controller.GetMeasures(file);
    foreach (var measure in currentMeasures)
    {
        if (!measures.ContainsKey(measure.Key))
        {
            measures.Add(measure.Key, measure.Value);
        }
    }
}
measures = measures.OrderBy(m => m.Key).ToDictionary(m => m.Key, m => m.Value);

Controller.FillGeologInfo(measures);

string allMeasuresFile = Controller.FillAllMeasures(measures);

string missingNotesFile = Controller.FillMissingNotes(measures);

string filledPlan = Controller.FillPlan(measures);

if (args.Length > 1)
{
    try
    {
        string passwd = args[0];
        string recipient = args[1];
        string senderName = JsonSerializer.Deserialize<ConfigEmail>(Config.config).User.Name;
        if (missingNotesFile != string.Empty)
        {
            Email.Send(Config.config, passwd, recipient, new List<string>(),
                "Липсващи кантарни бележки за месеца", "Поздрави,\n" + senderName, new string[] { missingNotesFile });

            TextFile.Log("### Изпратена справка за липсващи бележки", Config.logPath);
        }

        List<string> ccRecipients = new List<string>();

        if (args.Length > 2)
        {
            for (int i = 2; i < args.Length; i++)
            {
                ccRecipients.Add(args[i]);
            }
        }

        StringBuilder body = new StringBuilder();
        body.AppendLine("Това е автоматично генериран е-мейл.");

        if (allMeasuresFile != string.Empty)
        {
            string log = "### Изпратен е-мейл до спедитори";
            Email.Send(Config.config, passwd, recipient, ccRecipients, "Справка автовезна р-к 3", body.ToString(),
                new string[] { newFileName, filledPlan });
            Email.Send(Config.config, passwd, "admin@admin", new List<string>(), log,
                measures.LastOrDefault().Key + " - " + measures.LastOrDefault().Value.BrutoTime.ToString("dd.MM HH:mm"),
                new string[] { });
            TextFile.Log(log, Config.logPath);
        }
    }
    catch (System.Exception e)
    {
        TextFile.Log(e.Message, Config.logPath);
    }
}


// NPOI.Core
List<Measure> measuresN = new List<Measure>();
Measure measure1 = new Measure(1, 3123, DateTime.Now, "ki2548uy", 40020, 15840);
measuresN.Add(measure1);

string header = "No.;Дата;Ремарке;Доставчик;Място на разтоварване;Клиент;Вид товар;Бруто kg;Кант. бележка №;Нето kg;Време бруто;1;01/10/22";

var newFile = "Всички м." + ".NPOICore..xlsx";

using (var fs = new FileStream(newFile, FileMode.Create, FileAccess.Write))
{
    IWorkbook workbook = new XSSFWorkbook();
    ISheet sheet1 = workbook.CreateSheet("Sheet1");
    var rowIndex = 0;
    foreach (var measure in measures)
    {
        IRow currentRow = sheet1.CreateRow(rowIndex);
        currentRow.CreateCell(0).SetCellValue(measure.Value.Num);
        currentRow.CreateCell(1).SetCellValue(measure.Value.Id);
        currentRow.CreateCell(2).SetCellValue(measure.Value.BrutoTime.AddDays(-20).ToString("dd/MM/yy"));
        currentRow.CreateCell(3).SetCellValue(measure.Value.RegNum);
        currentRow.CreateCell(4).SetCellValue(measure.Value.Bruto);
        currentRow.CreateCell(5).SetCellValue(measure.Value.Tara);
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
}