using System.Globalization;

public class Measure
{
    public Measure(int id, DateTime brutoTime)
    {
        this.Id = id;
        this.BrutoTime = brutoTime;
    }
    public Measure(int id, DateTime brutoTime, string tractorNum, 
        string regNum, string driver, string egn, string phone)
    {
        this.Id = id;
        this.BrutoTime = brutoTime;
        this.TractorNum = tractorNum;
        this.RegNum = regNum;
        this.Driver = driver;
        this.Egn = egn;
        this.Phone = phone;
    }
    public Measure(int num, int id, DateTime brutoTime, string regNum, int bruto, int tara)
    {
        this.Num = num;
        this.Id = id;
        this.BrutoTime = brutoTime;
        this.RegNum = regNum;
        this.Bruto = bruto;
        this.Tara = tara;
    }

    public int Num { get; }
    public int Id { get; set; }
    public DateTime BrutoTime { get; set; }
    public string RegNum { get;}
    public int Bruto { get; set; }
    public int Tara { get; set; }

    public int Neto => Bruto - Tara;

    public string? TractorNum { get; }
    public string? Driver { get; }
    public string? Egn { get; }
    public string? Phone { get; }

    public string TimeRegNum => $"{this.Num} ; {this.BrutoTime.ToString("dd/MM/yy", CultureInfo.InvariantCulture)} ; {this.RegNum}";
    public string BrutoNeto => $"{this.Bruto} ; {this.Id} ; {this.Neto}";
}