using SQLite;

namespace Ilac;

public class Medication
{
    [PrimaryKey][AutoIncrement]
    public int ID { get; set; }
    public string? Name { get; set; }
    public string? Times { get; set; } 
    public string? Detail { get; set; }
    public string? CurrDay { get; set; }
    public string? NextDay { get; set; }
    public int TotalPiece { get; set; }
    public int UsagePiece { get; set; }
    public int Freq { get; set; }
    public int IsDeleted { get; set; }
}