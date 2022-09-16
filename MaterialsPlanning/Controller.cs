using System.Data;
using System.Globalization;

namespace MaterialsPlanning
{
    internal class Controller
    {
        internal static Dictionary<string, Material> InitializeMaterials(List<DataTable> tables)
        {
            var materials = new Dictionary<string, Material>();
            foreach (var table in tables)
            {
                foreach (DataRow dataRow in table.Rows)
                {
                    var material = new Material();
                    try
                    {
                        material = new Material()
                        {
                            Id = int.Parse(dataRow.Field<string>("№")),
                            SapNum = dataRow.Field<string>("SAP №"),
                            Name = dataRow.Field<string>("Наименование"),
                            ProductNum = dataRow.Field<string>("Продуктов номер"),
                            Producer = dataRow.Field<string>("Производител"),
                            Measure = dataRow.Field<string>("МЕ")
                        };
                    }
                    catch (NullReferenceException nre)
                    {
                        Console.WriteLine(nre.Message);
                    }

                    if (materials.ContainsKey(material.SapNum))
                    {
                        Console.WriteLine("Материал \"{0}\" със SAP № {1} присъства в таблицата повече от веднъж", material.Name, material.SapNum);
                    }
                    else
                    {
                        materials.Add(material.SapNum, material);
                    }
                }
            }
            return materials;
        }
        internal static void InsertLastContract(Dictionary<string, Material> materials, string? me3mFile)
        {
            string[] me3m = File.ReadAllLines(me3mFile);
            var lastContractForMaterial = new Dictionary<string, Material>();

            for (int i = 5; i < me3m.Length; i++)
            {
                string[] lineSplit = me3m[i].Split('\t', StringSplitOptions.RemoveEmptyEntries);
                string lastContract = "";
                string sapNum = "", name, measure, lastSupplier = "";
                decimal price = 0;

                if (lineSplit.Length > 17)
                {
                    lastContract = lineSplit[1];
                    sapNum = lineSplit[5];
                    DateTime dateOfContract = DateTime.ParseExact(lineSplit[0], "d.M.yyyy", CultureInfo.InvariantCulture);
                    name = lineSplit[9];
                    measure = lineSplit[10];
                    price = decimal.Parse(lineSplit[15].Trim().Replace(',', '.'));
                    var material = new Material()
                    {
                        SapNum = sapNum,
                        Name = name,
                        Measure = measure,
                        PriceLastContract = price,
                        lastContractDate = dateOfContract,
                        LastContract = lastContract
                    };

                    if (lastContractForMaterial.ContainsKey(sapNum))
                    {
                        if (lastContractForMaterial[sapNum].lastContractDate < dateOfContract)
                        {
                            lastContractForMaterial[sapNum] = material;
                        }
                    }
                    else
                    {
                        lastContractForMaterial.Add(sapNum, material);
                    }
                }

                try
                {
                    foreach (var material in materials)
                    {
                        if (materials.ContainsKey(sapNum))
                        {
                            materials[sapNum].lastContractDate = lastContractForMaterial[sapNum].lastContractDate;
                            materials[sapNum].LastContract = lastContractForMaterial[sapNum].LastContract;
                            materials[sapNum].PriceLastContract = lastContractForMaterial[sapNum].PriceLastContract;
                        }
                    }

                }
                catch (NullReferenceException nre)
                {
                    Console.WriteLine(nre.Message);
                }
            }
        }

        internal static void InsertPastExpenses(Dictionary<string, Material> materials, string? zcoelFile)
        {
            string[] zcoel = File.ReadAllLines(zcoelFile);

            for (int i = 6; i < zcoel.Length; i++)
            {
                string[] lineSplit = zcoel[i].Split('\t', StringSplitOptions.RemoveEmptyEntries);
                string sapNum = "", name, measure, structure, warehouse;
                string? group, partida;
                int quantity = 0;
                decimal price = 0;

                if (lineSplit.Length > 8)
                {
                    sapNum = lineSplit[3];
                    name = lineSplit[4];
                    quantity = int.Parse(lineSplit[5].Trim());
                    measure = lineSplit[6];
                    price = decimal.Parse(lineSplit[7].Trim().Replace(',', '.'));
                }

                try
                {
                    if (materials.ContainsKey(sapNum))
                    {
                        materials[sapNum].QtySpentForPeriod += quantity;
                        materials[sapNum].PriceAverageForPeriod = price;
                    }
                }
                catch (NullReferenceException nre)
                {
                    Console.WriteLine(nre.Message);
                }
            }
        }

        internal static void InsertQtysOnStock(Dictionary<string, Material> materials, string? mb52File) // Format FinCtrl
        {
            string[] mb52 = File.ReadAllLines(mb52File);
            for (int i = 3; i < mb52.Length; i++)
            {
                string[] lineSplit = mb52[i].Split('\t', StringSplitOptions.RemoveEmptyEntries);
                string sapNum = "", name, measure, structure, warehouse;
                string? group, partida;
                int quantity = 0;
                decimal price = 0;

                if (lineSplit.Length > 7)
                {
                    sapNum = lineSplit[2];
                    name = lineSplit[3];
                    quantity = int.Parse(lineSplit[6].Trim());
                    measure = lineSplit[4];
                    partida = lineSplit[5];
                    price = decimal.Parse(lineSplit[7].Trim().Replace(',', '.'));
                    structure = lineSplit[0];
                    warehouse = lineSplit[1];
                }

                try
                {
                    if (materials.ContainsKey(sapNum))
                    {
                        materials[sapNum].QtyOnStock += quantity;
                        materials[sapNum].PriceStockSupply = price;
                    }
                }
                catch (NullReferenceException nre)
                {
                    Console.WriteLine(nre.Message);
                }
            }
        }
        
    }
}