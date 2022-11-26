var dbPath = "..\\..\\ParadoxDB";

var table = new ParadoxTable(dbPath, "Docums");
var measures = new Dictionary<int, Measure>();
var recIndex = 1;
foreach (var rec in table.Enumerate())
{
    Console.WriteLine("Record #{0}", recIndex++);
    int id = 0;
    DateTime brutoTime = new DateTime();
    string regNum = string.Empty;
    int bruto = 0;
    int tara = 0;
    int counter = 0;

    for (int i = 0; i < table.FieldCount; i++)
    {
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
        Console.WriteLine("    {0} = {1}", table.FieldNames[i], rec.DataValues[i]);
    }
    var measure = new Measure(counter++, id, brutoTime, regNum, bruto, tara);
    if (!measures.ContainsKey(measure.Id))
    {
        measures.Add(measure.Id, measure);
    }
    if (recIndex > 10) break;
}
Console.WriteLine("-- press [enter] to continue --");
Console.ReadKey();
Console.Clear();

Console.WriteLine("Test 2: read 10 records by index (key range: last 10)");
Console.WriteLine("==========================================================");

var index = new ParadoxPrimaryKey(table, Path.Combine(dbPath, "Docums.PX"));
var condition = new ParadoxCondition.LogicalAnd(
        new ParadoxCondition.Compare(ParadoxCompareOperator.GreaterOrEqual, table.RecordCount - 10, 0, 0),
        new ParadoxCondition.Compare(ParadoxCompareOperator.LessOrEqual, table.RecordCount, 0, 0));
var qry = index.Enumerate(condition);
var rdr = new ParadoxDataReader(table, qry);
recIndex = 1;
while (rdr.Read())
{
    for (int i = 0; i < rdr.FieldCount; i++)
    {
        Console.WriteLine("    {0} = {1}", rdr.GetName(i), rdr[i]);
    }
}