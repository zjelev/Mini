var dbPath = "\\\\" + Config.veznaHost + "\\" + Config.veznaParadoxDb;
dbPath = "..\\..\\DBs\\ParadoxDB"; //test
var tableName = "Docums";
var measures = new Dictionary<int, Measure>();
int month = ReadDb(dbPath, tableName, measures);
Controller.FillAllMeasures(measures, "Всички м." + month + ".xlsx");
Controller.FillGeologInfo(measures);
Controller.FillMissingNotes(measures);
Controller.FillPlan(measures);

static int ReadDb(string dbPath, string tableName, Dictionary<int, Measure> measures)
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
        return measures.FirstOrDefault().Value.BrutoTime.Month;
    }
    else
    {
        return 0;
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