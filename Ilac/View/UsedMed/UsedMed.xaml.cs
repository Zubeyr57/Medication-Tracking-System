namespace Ilac;

public partial class UsedMedPage:ContentPage{
    private readonly Database _database;

    public UsedMedPage(){
        InitializeComponent();

        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "medications.db");
        _database = new Database(dbPath);

    }
    public async void getUsedMed(object sender, EventArgs e){
        UsedMedListView.ItemsSource = await _database.GetUsedMedicationsAsync();
    }
    private async void OnMedicationTapped(object sender, ItemTappedEventArgs e){
        if (e.Item is UsedMedication tappedMedication){
            await DisplayAlert("Medication", tappedMedication.Name+" "+
            tappedMedication.Detail+" "+
            tappedMedication.CurrDay, "OK");
            }
        
        
    }
    public void OnLoaded(object sender, EventArgs e){
        getUsedMed(null,null);
    }
}