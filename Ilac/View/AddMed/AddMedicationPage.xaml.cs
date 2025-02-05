namespace Ilac;

public partial class AddMedicationPage : ContentPage
{
    public AddMedicationPage()
    {
        InitializeComponent();
    }

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        string name = MedicationNameEntry.Text;
        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("Hata", "İlaç adı boş olamaz.", "Tamam");
            return;
        }
        string detail = MedicationDetailEntry.Text;

        if (!int.TryParse(UsageCountEntry.Text, out int usageCount) || usageCount <= 0)
        {
            await DisplayAlert("Hata", "Geçerli bir kullanım sayısı girin.", "Tamam");
            return;
        }

        if (!int.TryParse(TotalPieceEntry.Text, out int totalPiece) || totalPiece <= 0)
        {
            await DisplayAlert("Hata", "Geçerli bir adet sayısı girin.", "Tamam");
            return;
        }

        if (!int.TryParse(DailyUsageEntry.Text, out int dailyUsage) || dailyUsage <= 0)
        {
            await DisplayAlert("Hata", "Geçerli bir günlük kullanım sayısı girin.", "Tamam");
            return;
        }

        // Saat seçme ekranına geç
        await Navigation.PushAsync(new SetMedicationTimesPage(name, usageCount, detail, totalPiece, dailyUsage));
    }
}
