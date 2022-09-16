using System;

namespace WeightNotes
{
    public class DailyTrucksGeologInfo
    {
        public DailyTrucksGeologInfo(DateTime date, int shift, decimal netWeightInTons, int numOfTrucks)
        {
            this.Date = date;
            this.Shift = shift;
            this.NetWeightInTons = netWeightInTons;
            this.NumOfTrucks = numOfTrucks;
            this.AshesPercent = "N/A";
        }

        public DailyTrucksGeologInfo()
        {
            //this.Client = "";
        }
        public string Client { get; set; }
        public int MineNum { get; set; }
        public DateTime Date { get; set; }
        public int Shift { get; set; }
        public decimal NetWeightInTons { get; set; }
        public string AshesPercent { get; set; }
        public int NumOfTrucks { get; set; }

        public string DateString => this.Date.ToString("d.M.yyyy г.");
    }
}