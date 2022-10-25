using System.Text;
using Common;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var newVeznaFiles = TextFile.CompareDirs($"\\\\{Config.veznaHost}\\{Config.veznaPath}", Config.veznaFilePattern, Environment.CurrentDirectory, "20*.txt");
foreach (var newVeznaFile in newVeznaFiles)
{
    string newFileName = Controller.SetFileName(newVeznaFile.FullName);
    File.Copy(newVeznaFile.FullName, newFileName, true);
    TextFile.Log("Справката за деня беше копирана от " + newVeznaFile.FullName + " в " + newFileName, Config.logPath);
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

Controller.FillGeologInfo(measures, "Справка по смени.xlsx");

string allMeasuresFile = "Всички м." + measures.FirstOrDefault().Value.BrutoTime.Month + ".csv";
Controller.FillAllMeasures(measures, allMeasuresFile);

StringBuilder missingNotes = new StringBuilder();
missingNotes.AppendLine("Дата;Номер");
int firstId = measures.FirstOrDefault().Key;
int countId = firstId - 1;
foreach (var measure in measures)
{
    countId++;
    if (measure.Key != countId)
    {
        while (measure.Key != countId)
        {
            Measure missingMeasure = new Measure(countId, measure.Value.BrutoTime);
            missingNotes.AppendLine($"{missingMeasure.BrutoTime.ToString("dd.MM.yyyy")};{countId}");
            countId++;
        }
    }
}
string missingNotesFile = Config.logPath + "Липсващи бележки.csv";
TextFile.SaveNew(Config.logPath, missingNotes, missingNotesFile);

// string lastMissingNote = missingNotes.Remove(0, missingNotes.Length - 17).ToString().Substring(0, 10);
// if (DateTime.Now.Date.ToString("dd.MM.yyyy").Equals(lastMissingNote) && args.Length == 2)
// {
//     try
//     {
//         string passwd = args[0];
//         string senderName = JsonSerializer.Deserialize<ConfigWeightNotes>(config).User.Name;
//         // Email.Send(passwd, args[1], new List<string>(),
//         //     "Липсващи кантарни бележки за месеца", "Поздрави,\n" + senderName, new string[] { missingMeasuresFile });
//         TextFile.Log("Изпратена справка за липсващи бележки", logPath);
//     }
//     catch (System.Exception e)
//     {
//         TextFile.Log(e.Message, logPath);
//     }
// }