using System.Text;
using Utils;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// var template = File.ReadAllLines("template.t4", Encoding.GetEncoding("windows-1251"));
// Console.WriteLine(File.ReadAllText("Sample.txt"));

var productionDbPath = "\\\\" + Config.veznaHost + "\\" + Config.veznaDb;
var copyDb = "..\\..\\DBs\\ParadoxDB";
//productionDbPath = copyDb + "-test";
var tableName = "Docums";
var productionDbFiles = Directory.GetFiles(productionDbPath, tableName + ".*");
foreach (var file in productionDbFiles)
{
    File.Copy(file, Path.Combine(copyDb, Path.GetFileName(file)), true);
}

var measures = new Dictionary<int, Measure>();
string lastMeasure = ReadDb(copyDb, tableName, measures);
if (TextFile.SaveNew(new StringBuilder(lastMeasure), "lastMeasure.txt"))
{
    string month = lastMeasure.Substring(0, lastMeasure.IndexOf(';'));
    WeightNotes.Controller.FillAllMeasures(measures, "Всички " + month + ".xlsx");
    WeightNotes.Controller.FillGeologInfo(measures);
    WeightNotes.Controller.FillPlan(measures);
    string newFileName = "template.txt";

    WeightNotes.Controller.SendMail(args, newFileName, measures);
}

static string ReadDb(string dbPath, string tableName, Dictionary<int, Measure> measures)
{
    var table = new ParadoxTable(dbPath, tableName);
    int counter = 1;
    int day = 1;
    //var recIndex = 1000;
    foreach (var rec in table.Enumerate())
    {
        int id = 0;
        DateTime brutoTime = new DateTime();
        string regNum = string.Empty;
        int bruto = 0;
        int tara = 0;
        string tovar = string.Empty;

        for (int i = 0; i < table.FieldCount; i++)
        {
            if (table.FieldNames[i] == "TOVAR")
            {
                tovar = rec.DataValues[i].ToString();
            }
            if (table.FieldNames[i] == "NOM_DOKUM")
            {
                id = int.Parse(rec.DataValues[i].ToString());
            }
            if (table.FieldNames[i] == "ATRIB2")
            {
                regNum = rec.DataValues[i].ToString();
            }
            if (table.FieldNames[i] == "BRUTO")
            {
                bruto = int.Parse(rec.DataValues[i].ToString());
            }
            if (table.FieldNames[i] == "TARA")
            {
                tara = int.Parse(rec.DataValues[i].ToString());
            }
            if (table.FieldNames[i] == "DATA_BRUTO")
            {
                brutoTime = DateTime.Parse(rec.DataValues[i].ToString());
            }
            if (table.FieldNames[i] == "4AS_BRUTO")
            {
                var timeOfDay = TimeSpan.Parse(rec.DataValues[i].ToString());
                brutoTime = brutoTime.Add(timeOfDay);
            }
        }

        if (tovar == "101" && brutoTime.Month == DateTime.Now.Month)
        {
            if (brutoTime.Day > day)
            {
                day = brutoTime.Day;
                counter = 1;
            }
            var measure = new Measure(counter, id, brutoTime, regNum, bruto, tara);
            if (!measures.ContainsKey(measure.Id))
            {
                measures.Add(measure.Id, measure);
                counter++;
            }
        }
        //if (recIndex > 1010) break;
    }
    if (measures.FirstOrDefault().Value.BrutoTime.Month == measures.LastOrDefault().Value.BrutoTime.Month)
    {
        var last = measures.LastOrDefault();
        return "м." + last.Value.BrutoTime.Month + ";" + last.Key + ";" + last.Value.BrutoTime + ";" + last.Value.BrutoNeto;
    }
    else
    {
        return string.Empty;
    }
}


static void ReadPx(string dbPath, string tableName, Dictionary<int, Measure> measures)
{
    var table = new ParadoxTable(dbPath, tableName);
    var index = new ParadoxPrimaryKey(table, Path.Combine(dbPath, tableName + ".PX"));
    var condition = new ParadoxCondition.LogicalAnd(
            new ParadoxCondition.Compare(ParadoxCompareOperator.GreaterOrEqual, 0, 0, 0),
            new ParadoxCondition.Compare(ParadoxCompareOperator.LessOrEqual, table.RecordCount, 0, 0));
    var qry = index.Enumerate(condition);
    var rdr = new ParadoxDataReader(table, qry);

    while (rdr.Read())
    {
        int id = 0;
        DateTime brutoTime = new DateTime();
        string regNum = string.Empty;
        int bruto = 0;
        int tara = 0;
        int counter = 0;
        for (int i = 0; i < rdr.FieldCount; i++)
        {
            if (rdr.GetName(i) == "NOM_DOKUM")
            {
                id = int.Parse(rdr[i].ToString());
            }
            if (rdr.GetName(i) == "ATRIB2")
            {
                regNum = rdr[i].ToString();
            }
            if (rdr.GetName(i) == "BRUTO")
            {
                bruto = int.Parse(rdr[i].ToString());
            }
            if (rdr.GetName(i) == "TARA")
            {
                tara = int.Parse(rdr[i].ToString());
            }
            if (rdr.GetName(i) == "DATA_BRUTO")
            {
                brutoTime = DateTime.Parse(rdr[i].ToString());
            }
            if (rdr.GetName(i) == "4AS_BRUTO")
            {
                var timeOfDay = TimeSpan.Parse(rdr[i].ToString());
                brutoTime = brutoTime.Add(timeOfDay);
            }
        }
        var measure = new Measure(counter++, id, brutoTime, regNum, bruto, tara);
        if (!measures.ContainsKey(measure.Id))
        {
            measures.Add(measure.Id, measure);
        }
    }
}
