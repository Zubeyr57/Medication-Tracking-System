using SQLite;

namespace Ilac;

public class UsedMedication
{
    
    public int ID { get; set; }
    public string? Name { get; set; }
    public string? UseTime { get; set; } 
    public string? Detail { get; set; }
    public string? CurrDay { get; set; }
    public int UsagePiece { get; set; }
    public int Freq { get; set; }
    public int IsDeleted { get; set; }
}