using System.Text;
using System.Text.Json;
using Utils;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var newVeznaFiles = TextFile.CompareDirs($"\\\\{Config.veznaHost}\\{Config.veznaPath}", Config.veznaFilePattern, Environment.CurrentDirectory, "20*.txt");

if (newVeznaFiles.Count() > 0)
{
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

    string fileNameXlsx = "Всички м." + measures.FirstOrDefault().Value.BrutoTime.Month + ".xlsx";

    Controller.FillAllMeasures(measures, fileNameXlsx);

    string missingNotesCsvFile = Controller.FillMissingNotes(measures);

    if (args.Length > 1)
    {
        try
        {
            string passwd = args[0];
            string recipient = args[1];
            string senderName = JsonSerializer.Deserialize<ConfigEmail>(Config.config).User.Name;
            if (missingNotesCsvFile != string.Empty)
            {
                Email.Send(Config.config, passwd, recipient, new List<string>(),
                    "Липсващи кантарни бележки за месеца", "Поздрави,\n" + senderName, new string[] { missingNotesCsvFile });

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

            // if (newFileName != string.Empty)
            // {
            Email.Send(Config.config, passwd, recipient, ccRecipients, "Справка автовезна р-к 3", body.ToString(),
                new string[] { newFileName, Controller.FillPlan(measures) });
            string log = "### Изпратен е-мейл до спедитори";
            Email.Send(Config.config, passwd, "admin@mail", new List<string>(), log,
                measures.LastOrDefault().Key + " - " + measures.LastOrDefault().Value.BrutoTime.ToString("dd.MM HH:mm"),
                new string[] { });
            TextFile.Log(log, Config.logPath);
            // }
        }
        catch (System.Exception e)
        {
            TextFile.Log(e.Message, Config.logPath);
        }
    }
}
else
{
    TextFile.Log("Няма нова справка", Config.logPath);
}