using System.Net.Http.Json;

namespace Ilac;

public class MedicationService
{
    private readonly HttpClient _httpClient;

    public MedicationService(string baseAddress)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseAddress)
        };
    }

    // İlaçları listele
    public async Task<List<Medication>> GetMedicationsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Medication>>("/medications") ?? new List<Medication>();
    }

    // Yeni ilaç ekle
    public async Task<bool> AddMedicationAsync(Medication medication)
    {
        var response = await _httpClient.PostAsJsonAsync("/medications", medication);
        return response.IsSuccessStatusCode;
    }

    // İlaç sil
    public async Task<bool> DeleteMedicationAsync(int? medicationId)
    {
        var response = await _httpClient.DeleteAsync($"/medications/{medicationId}");
        return response.IsSuccessStatusCode;
    }

    // İlaç güncelle
    public async Task<bool> UpdateMedicationAsync(Medication medication)
    {
        var response = await _httpClient.PutAsJsonAsync($"/medications/{medication.ID}", medication);
        return response.IsSuccessStatusCode;
    }
}
