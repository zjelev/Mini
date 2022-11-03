class CodeSum
{
    private int code;
    private string? name;
    private decimal sum;
    private decimal hours;

    public CodeSum(int code, string name, decimal sum, decimal hours)
    {
        this.Code = code;
        this.Name = name;
        this.Sum = sum;
        this.Hours = hours;
    }

    public CodeSum(int code, decimal sum, decimal hours)
    {
        this.Code = code;
        this.Sum = sum;
        this.Hours = hours;
    }

    public int Code { get => code; private set => code = value; }
    public string? Name { get => name; private set => name = value; }
    public decimal Sum { get => sum; set => sum = value; }
    public decimal Hours { get => hours; set => hours = value; }
}