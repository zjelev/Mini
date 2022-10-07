using System.Data;
using System.Globalization;
using System.Text;

namespace MaterialsPlanning
{

    internal class Program
    {
        private static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string currentDirectory = Environment.CurrentDirectory;

            string? materialsFile = Directory.GetFiles(currentDirectory, "Материали*.xls?")?.FirstOrDefault();
            var tables = Common.Excel.ReadFromExcel<List<DataTable>>(materialsFile);
            var materials = Controller.InitializeMaterials(tables);

            string mb52File = Directory.GetFiles(currentDirectory, "mb52*.XLS")?.FirstOrDefault();
            Controller.InsertQtysOnStock(materials, mb52File);

            string zcoelFile = Directory.GetFiles(currentDirectory, "zcoel*.XLS")?.FirstOrDefault();
            Controller.InsertPastExpenses(materials, zcoelFile);

            string me3mFile = Directory.GetFiles(currentDirectory, "me3m*.XLS")?.FirstOrDefault();
            Controller.InsertLastContract(materials, me3mFile);

            int startYear = int.Parse(zcoelFile.Substring(zcoelFile.IndexOf('-') + 1, 4));
            int periodInMonths = (DateTime.Now.Year - startYear)*12 + DateTime.Now.Month - 1;

            var sb = new StringBuilder();
            sb.AppendLine(
                $"№;SAP №;Наименование;Продуктов номер;Производител;МЕ;Гар.запас;Нал.к-во;Разход {periodInMonths} м.;Ср.разход 12 м.;"
                +"Необх.к-во;Посл.договор;От дата;С цена;Общо лв.");

            foreach (var material in materials)
            {
                int expensePerYear = (int)Math.Ceiling((double)material.Value.QtySpentForPeriod / periodInMonths * 12);
                int qtyGuarantee = expensePerYear < 1 ? 1 : (int)Math.Ceiling(expensePerYear * 0.5);
                int qtyNeeded = qtyGuarantee - material.Value.QtyOnStock + expensePerYear < 1 ? 1 : qtyGuarantee - material.Value.QtyOnStock + expensePerYear;
                sb.AppendLine($"{material.Value.Id};{material.Value.SapNum};{material.Value.Name};{material.Value.ProductNum};{material.Value.Producer};" +
                    $"{material.Value.Measure};{qtyGuarantee};{material.Value.QtyOnStock};{material.Value.QtySpentForPeriod};" +
                    $"{expensePerYear};{qtyNeeded};{material.Value.LastContract};" +
                    $"{material.Value.lastContractDate.Date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)};{material.Value.PriceLastContract};" + 
                    $"{qtyNeeded * material.Value.PriceLastContract:f2}");
            }

            File.WriteAllText($"{materialsFile.Substring(0, materialsFile.Length - 4)}-анализ.csv", sb.ToString(), Common.Excel.srcEncoding);
        }
    }
}