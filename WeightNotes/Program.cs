using System.Text;
using Utils;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var newVeznaFiles = TextFile.CompareDirs($"\\\\{Config.veznaHost}\\{Config.veznaPath}", Config.veznaFilePattern, Environment.CurrentDirectory, "20*.txt");

if (newVeznaFiles.Count() > 0)
{
    string newFileName = string.Empty;
    foreach (var newVeznaFile in newVeznaFiles)
    {
        newFileName = WeightNotes.Controller.SetFileName(newVeznaFile.FullName);
        File.Copy(newVeznaFile.FullName, newFileName, true);
        TextFile.Log("### Справката за деня беше копирана от " + newVeznaFile.FullName + " в " + newFileName, Utils.Config.logPath);
    }
    var files = Directory.GetFiles(Environment.CurrentDirectory, "*.TXT");//.Where(name => !name.EndsWith("на.TXT"));
    var measures = new Dictionary<int, Measure>();

    foreach (var file in files)
    {
        Dictionary<int, Measure> currentMeasures = WeightNotes.Controller.GetMeasures(file);
        foreach (var measure in currentMeasures)
        {
            if (!measures.ContainsKey(measure.Key))
            {
                measures.Add(measure.Key, measure.Value);
            }
        }
    }
    measures = measures.OrderBy(m => m.Key).ToDictionary(m => m.Key, m => m.Value);

    WeightNotes.Controller.FillGeologInfo(measures);
    string fileNameXlsx = "Всички м." + measures.FirstOrDefault().Value.BrutoTime.Month + ".xlsx";
    WeightNotes.Controller.FillAllMeasures(measures, fileNameXlsx);
    WeightNotes.Controller.SendMail(args, newFileName, measures);
}
else
{
    TextFile.Log("Няма нова справка", Utils.Config.logPath);
}