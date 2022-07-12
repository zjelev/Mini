namespace ExcelUtils
{
    public class Measure
    {
        public Measure(int id, int protokolNum, string fromdate, string regNum, int bruto, 
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
        public string FromDate { get; set; }
        public string RegNum { get; set; }
        public int Bruto { get; set; }
        public string BrutoHour { get; set; }
        public int Tara { get; set; }
        public string TaraHour { get; set; }
        public int Netto { get; set; }

        public override string ToString()
        {
            return $"{this.Id} ; {this.ProtokolNum} ; {this.FromDate} ; {this.RegNum} ;"
                  +$"{this.Bruto} ; {this.BrutoHour} ; {this.Tara} ; {this.TaraHour} ; {this.Netto}";
        }
    }
}