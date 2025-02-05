namespace Ilac;
using System.Net.Http.Json;
using SQLite;
using System.Collections.Generic;


public partial class MainPage : ContentPage
{
    private readonly MedicationService _medicationService;
    private readonly Database _database;
    private readonly DateTime _today;
    private List<Medication> _allMedications;

    public MainPage()
    {
        InitializeComponent();
        _medicationService = new MedicationService("http://172.16.106.179:8000"); // GoLang API'nin adresi

        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "medications.db");
        _database = new Database(dbPath);

        _today = DateTime.Today;
    }
    //api bağlantı kontrolü
    public async Task<bool> CheckApiConnectionAsync()
    {
        try
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync("http://172.16.106.179:8000");
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
    // İlaçları listele
    private async void GetFromSqlite_Clicked(object sender, EventArgs e)
    {
        try
        {
            var medications = await _database.GetMedicationsAsync();
            _allMedications = await _database.GetMedicationsAsync();
            MedicationsListView.ItemsSource = medications.Where(m => m.IsDeleted != 1).ToList();
            
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
            var medications = await _database.GetMedicationsAsync();
            MedicationsListView.ItemsSource = medications;
        }
    }
    
    public async void OnloadTitle(object Sender, EventArgs e)
    {
        Title = "Ilac Takip";
        try{
            var medications = await _database.GetMedicationsAsync();
            if (!await CheckApiConnectionAsync())
            {
                Console.WriteLine("hop bagli deil");
                return;
            }
            var api = await _medicationService.GetMedicationsAsync();


            foreach (var dtbsItem in medications)
            {
                var apiItem = api.FirstOrDefault(x => x.ID == dtbsItem.ID);

                var isExist = api.Any(x => x.ID == dtbsItem.ID);
                if (isExist)
                {
                    DateTime apiCurrDay = DateTime.Parse(apiItem.CurrDay);
                    DateTime dtbsCurrDay = DateTime.Parse(dtbsItem.CurrDay);
                    if(dtbsItem.IsDeleted == 1){
                        Console.WriteLine("Deleted");
                        await _medicationService.DeleteMedicationAsync(dtbsItem.ID);
                        await _database.DeleteMedicationAsync(dtbsItem.ID);
                    }
                    else if (apiCurrDay < dtbsCurrDay)
                    {
                        await _medicationService.UpdateMedicationAsync(dtbsItem);
                    }
                    else
                    {
                        await _database.UpdateMedicationAsync(apiItem);
                    }
                }
                else await _medicationService.AddMedicationAsync(dtbsItem);
            }

            foreach (var apiItem in api)
            {
                var dtbsItem = medications.FirstOrDefault(x => x.ID == apiItem.ID);
                var isExist = medications.Any(x => x.ID == apiItem.ID);
                if (!isExist)
                {
                    await _database.AddMedicationAsync(apiItem);
                }
            }

        }
        catch(Exception ex){
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }
    //zamanı gelenler
    private async void OnTimeMedication_Clicked(object sender, EventArgs e)
    {
        string Today = _today.ToString(@"dd/MM/yyyy");
        try
        {
            var medications = await _database.GetMedicationsAsync();
            List<Medication> singleItemList = new List<Medication>();
            foreach (var warn in medications)
            {
                var timeList = warn.Times.Split(",");

                DateTime sonEleman = DateTime.ParseExact(
                    warn.NextDay + " " +
                    timeList.Last(), "dd.MM.yyyy HH:mm",
                    System.Globalization.CultureInfo.InvariantCulture);
                    
                var oneItem = await _database.GetMedicationByIdAsync(warn.ID);
                var usedItems = await _database.GetUsedMedicationsAsync();
                foreach (var list in timeList)
                {
                    var now = DateTime.Now;
                    var lTime = DateTime.Parse(list);
                    if (warn.NextDay == Today)//
                    {
                        if ((lTime - now).Duration() < TimeSpan.FromMinutes(10))//+- 10 dakika tolerans ekler
                        {
                            var isUsed = usedItems.Any(item => item.ID == oneItem.ID && 
                                (DateTime.Parse(item.CurrDay) - now).Duration() < TimeSpan.FromMinutes(10));
                            if(!isUsed){
                                singleItemList.Add(oneItem);
                            }
                            
                        }
                        else if (now > sonEleman)
                        {
                            oneItem.NextDay = now.AddDays(warn.Freq).ToString(@"dd/MM/yyyy");
                            oneItem.CurrDay = DateTime.Now.ToString(@"dd/MM/yyyy HH:mm");
                            Console.WriteLine(now);
                            await _database.UpdateMedicationAsync(oneItem);
                        }

                    }
                    Console.WriteLine(list + "list");  
                }
            }
            MedicationsListView.ItemsSource = singleItemList.Where(m => m.IsDeleted != 1).ToList();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }
    // Yeni ilaç ekle
    private async void AddMedicationButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddMedicationPage());
    }
    // arama kısmı
    private async void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
{
    _allMedications = await _database.GetMedicationsAsync();
    var searchText = e.NewTextValue.ToLower();
    var filteredItems = _allMedications.Where(m => m.Name.ToLower().Contains(searchText)).ToList();
    MedicationsListView.ItemsSource = filteredItems;
}
    // İlaç sil
    private async void DeleteMedicationButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var button = sender as Button;
            if (button != null && button.CommandParameter != null)
            {
                int medicationId = (int)button.CommandParameter;
                var medication = await _database.GetMedicationByIdAsync(medicationId);

                if (await CheckApiConnectionAsync())
                {
                    await _database.DeleteMedicationAsync(medicationId);
                    await _medicationService.DeleteMedicationAsync(medicationId);
                    await DisplayAlert("Başarılı", "ilaç Silindi", "Tamam");
                }
                else
                {
                    medication.IsDeleted = 1;
                    medication.CurrDay = DateTime.Now.ToString(@"dd/MM/yyyy HH:mm");
                    var res = await _database.UpdateMedicationAsync(medication);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }
    private async void UpdateMedicationButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var button = sender as Button;
            if (button != null && button.CommandParameter != null)
            {
                int medicationId = (int)button.CommandParameter;
                var medication = await _database.GetMedicationByIdAsync(medicationId);
                string result = await DisplayPromptAsync("Güncelle", "Yeni ilaç adedini girin:", "Tamam", "İptal", "Adet", maxLength: 3, keyboard: Keyboard.Numeric);
                
                int newPiece = int.Parse(result);
                if (newPiece <= 0) {
                    await DisplayAlert("Hata", "Adet sayısı yanlış", "Tamam");
                    return;
                }
                else{
                    medication.TotalPiece = newPiece;
                    medication.CurrDay = DateTime.Now.ToString(@"dd/MM/yyyy HH:mm");
                }
                var res = await _database.UpdateMedicationAsync(medication);

                if (res > 0) await DisplayAlert("Başarılı", "ilaç Adedi Güncellendi", "Tamam");

                else await DisplayAlert("Hata", "İlaç Adedi Güncellenemedi", "Tamam");
            }

        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }
    // ListView'deki bir ilaç tıklandığında 
    private async void OnMedicationTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is Medication tappedMedication)
        {
            bool confirm = await DisplayAlert("Onay", $"{tappedMedication.Name} adlı ilacı kullandınızmı?", "Evet", "Hayır");
            if (confirm)
            {
                if (tappedMedication.TotalPiece == 0)
                {
                    await DisplayAlert("Hata", "İlaç adedi kalmadı.", "Tamam");
                    return;
                }
                try
                {
                    var now = DateTime.Now;
                    var medication = await _database.GetMedicationByIdAsync(tappedMedication.ID);
                    var timeList = medication.Times.Split(",");
                    var lastTimeList = DateTime.Parse(timeList.Last());

                    medication.TotalPiece -= medication.UsagePiece;//güncelleme
                    medication.CurrDay = now.ToString(@"dd/MM/yyyy HH:mm");
                    medication.NextDay = now.AddDays(medication.Freq).ToString(@"dd/MM/yyyy");

                    var result = await _database.UpdateMedicationAsync(medication);
                    var Usedmed = new UsedMedication
                    {
                        ID = medication.ID,
                        Name = medication.Name,
                        UseTime = now.ToString(@"HH:mm"),
                        Detail = medication.Detail,
                        CurrDay = now.ToString(@"dd/MM/yyyy HH:mm"),
                        UsagePiece = medication.UsagePiece,
                        Freq = medication.Freq,
                        IsDeleted = 0
                    };
                    var result2 = await _database.AddUsedMedicationAsync(Usedmed);

                    if (result > 0)
                    {
                        await DisplayAlert("Başarılı", "İlaç adedi güncellendi.", "Tamam");
                    }
                    else
                    {
                        await DisplayAlert("Hata", "İlaç güncellenemedi.", "Tamam");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Hata", ex.Message, "Tamam");
                }
            }
            else
            {
                var medication = await _database.GetMedicationByIdAsync(tappedMedication.ID);

                await _database.UpdateMedicationAsync(medication);

                await DisplayAlert("Başarılı", "ilaç kullanılmadı", "Tamam");
            }
            // GetMedicationsButton_Clicked(null, null); 
        }
    }
    
}
