using SQLite;
namespace Ilac;
using System.Linq;

public class Database
{
    private readonly SQLiteAsyncConnection _database;

    public Database(string dbPath)
    {
        // Veritabanı bağlantısını başlat
        _database = new SQLiteAsyncConnection(dbPath);

        // Gerekli tabloları oluştur 
        _database.CreateTableAsync<Medication>().Wait();
        _database.CreateTableAsync<UsedMedication>().Wait();
    }

    // Tüm ilaçları listele
    public Task<List<Medication>> GetMedicationsAsync()
    {
        return _database.Table<Medication>().ToListAsync();
    }
    public Task<List<UsedMedication>> GetUsedMedicationsAsync()
    {
        return _database.Table<UsedMedication>().ToListAsync();
    }

    // Yeni ilaç ekle
    public Task<int> AddMedicationAsync(Medication medication)
    {
        return _database.InsertAsync(medication);
    }
    public Task<int> AddUsedMedicationAsync(UsedMedication medication)
    {
        return _database.InsertAsync(medication);
    }

    // İlaç güncelle
    public Task<int> UpdateMedicationAsync(Medication medication)
    {
        return _database.UpdateAsync(medication);
    }

    // İlaç sil
    public Task<int> DeleteMedicationAsync(int id)
    {
        return _database.DeleteAsync<Medication>(id);
    }

    // İlaç ID'sine göre getir
    public Task<Medication?> GetMedicationByIdAsync(int id)
    {
        return _database.Table<Medication?>().FirstOrDefaultAsync(m => m.ID == id);
    }
    public async Task<List<string>> GetMedicationsForDateAsync(DateTime date)
{
    var medications = await _database.Table<Medication>()
                                     .Where(m => m.NextDay == date.ToString("dd.MM.yyyy"))
                                     .ToListAsync();

    return medications.Select(m => m.Name).ToList();
}
}
