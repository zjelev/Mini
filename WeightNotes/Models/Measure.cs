using System.Globalization;

namespace WeightNotes
{
    public class Measure
    {
        public Measure()
        {
            
        }
        public Measure(int protokolNum, string tractorNum, string regNum, string driver, string egn, string phone, DateTime fromDate  )
        {
            this.ProtokolNum = protokolNum;
            this.TractorNum = tractorNum;
            this.RegNum = regNum;
            this.Driver = driver;
            this.Egn = egn;
            this.Phone = phone;
            this.FromDate = fromDate;
        }
        public Measure(int id, int protokolNum, DateTime fromdate, string regNum, int bruto, 
            string brutoHour, int tara, string taraHour, int netto)
        {
            this.Id = id;
            this.ProtokolNum = protokolNum;
            this.FromDate = fromdate;
            this.RegNum = regNum;
            this.Bruto = bruto;
            this.BrutoHour = brutoHour;
            this.Tara = tara;
            this.TaraHour = taraHour;
            this.Netto = netto;
        }

        public int Id { get; set; }
        public int ProtokolNum { get; set; }
        public DateTime FromDate { get; set; }
        public string RegNum { get; set; }
        public int Bruto { get; set; }
        public string BrutoHour { get; set; }
        public int Tara { get; set; }
        public string TaraHour { get; set; }
        public int Netto { get; set; }

        public string TractorNum { get; set; }
        public string Driver { get; set; }
        public string Egn { get; set; }
        public string Phone { get; set; }

        public override string ToString()
        {
            return $"{this.Id} ; {this.ProtokolNum} ; {this.FromDate.ToString("dd/MM/yy", CultureInfo.InvariantCulture)} ; {this.RegNum} ;"
                  +$"{this.Bruto} ; {this.BrutoHour} ; {this.Tara} ; {this.TaraHour} ; {this.Netto}";
        }
    }
}