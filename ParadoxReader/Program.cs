var dbPath = "..\\..\\ParadoxDB";
var table = new ParadoxTable(dbPath, "Docums");
var measures = new Dictionary<int, Measure>();

// var recIndex = 1;
// foreach (var rec in table.Enumerate())
// {
//     int id = 0;
//     DateTime brutoTime = new DateTime();
//     string regNum = string.Empty;
//     int bruto = 0;
//     int tara = 0;
//     int counter = 0;

//     for (int i = 0; i < table.FieldCount; i++)
//     {
//         if (table.FieldNames[i] == "NOM_DOKUM")
//         {
//             id = int.Parse(rec.DataValues[i].ToString());
//         }
//         if (table.FieldNames[i] == "ATRIB2")
//         {
//             regNum = rec.DataValues[i].ToString();
//         }
//         if (table.FieldNames[i] == "BRUTO")
//         {
//             bruto = int.Parse(rec.DataValues[i].ToString());
//         }
//         if (table.FieldNames[i] == "TARA")
//         {
//             tara = int.Parse(rec.DataValues[i].ToString());
//         }
//         if (table.FieldNames[i] == "DATA_BRUTO")
//         {
//             brutoTime = DateTime.Parse(rec.DataValues[i].ToString());
//         }
//         if (table.FieldNames[i] == "4AS_BRUTO")
//         {
//             var timeOfDay = TimeSpan.Parse(rec.DataValues[i].ToString());
//             brutoTime = brutoTime.Add(timeOfDay);
//         }
//     }
//     var measure = new Measure(counter++, id, brutoTime, regNum, bruto, tara);
//     if (!measures.ContainsKey(measure.Id))
//     {
//         measures.Add(measure.Id, measure);
//     }
//     if (recIndex > 10) break;
// }


var index = new ParadoxPrimaryKey(table, Path.Combine(dbPath, "Docums.PX"));
var condition = new ParadoxCondition.LogicalAnd(
        new ParadoxCondition.Compare(ParadoxCompareOperator.GreaterOrEqual, table.RecordCount - 50, 0, 0),
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

Controller.FillAllMeasures(measures);