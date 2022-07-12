using System;

namespace ExcelUtils
{
    public class DailyTrucksInfo
    {
        public DailyTrucksInfo(DateTime date, byte shift, decimal netWeightInTons, int numOfTrucks)
        {
            this.Date = date;
            this.Shift = shift;
            this.NetWeightInTons = netWeightInTons;
            this.NumOfTrucks = numOfTrucks;
            this.AshesPercent = "N/A";
        }

        public DailyTrucksInfo()
        {
            //this.Client = "";
        }
        public string Client { get; set; }
        public byte MineNum { get; set; }
        public DateTime Date { get; set; }
        public byte Shift { get; set; }
        public decimal NetWeightInTons { get; set; }
        public string AshesPercent { get; set; }
        public int NumOfTrucks { get; set; }

        public string DateString => this.Date.ToString("d.M.yyyy г.");
    }
}