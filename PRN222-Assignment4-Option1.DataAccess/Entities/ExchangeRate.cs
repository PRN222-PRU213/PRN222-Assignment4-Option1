namespace PRN222_Assignment4_Option1.DataAccess.Entities;

public class ExchangeRate
{
    public int Id { get; set; }
    public string BaseCurrency { get; set; } = string.Empty;
    public string TargetCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime CreatedAt { get; set; }
}
