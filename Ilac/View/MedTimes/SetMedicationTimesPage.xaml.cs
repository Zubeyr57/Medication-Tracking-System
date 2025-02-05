namespace Ilac;

using System.Net.Http.Json;


public partial class SetMedicationTimesPage : ContentPage
{

    private readonly string _medicationName;
    private readonly int _usageCount;
    private readonly string _detail;
    private readonly int _totalPiece;
    private readonly int _dailyUsage;


    private readonly Database _database;

    private readonly List<TimePicker> _timePickers;

    public SetMedicationTimesPage(string medicationName, int usageCount, string detail, int totalPiece, int dailyUsage)
    {
        InitializeComponent();
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "medications.db");
        _database = new Database(dbPath);

        _medicationName = medicationName;
        _usageCount = usageCount;
        _detail = detail;
        _timePickers = new List<TimePicker>();
        _totalPiece = totalPiece;
        _dailyUsage = dailyUsage;

        // kullanım saati seçme
        for (int i = 0; i < usageCount; i++)
        {
            var timePicker = new TimePicker
            {
                Time = TimeSpan.FromHours(8 + (i * 2))
            };
            _timePickers.Add(timePicker);
            TimeSelectors.Children.Add(new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children =
                {
                    new Label { Text = $"Saat {i + 1}:", VerticalOptions = LayoutOptions.Center },
                    timePicker
                }
            });
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        string freqEnter = FreqEntry.Text == null ? "1" : FreqEntry.Text;
        int freq = Int32.Parse(freqEnter);

        var times = _timePickers.Select(tp => tp.Time.ToString(@"hh\:mm"));

        var today = DateTime.Now;
        var nextDay = today.AddDays(freq);
        

        var newMedication = new Medication
        {
            Name = _medicationName.ToUpper(),
            Times = string.Join(",", times),
            Detail = _detail,
            CurrDay = today.ToString(@"dd/MM/yyyy HH:mm"),
            NextDay = nextDay.ToString(@"dd/MM/yyyy"),
            TotalPiece = _totalPiece,
            UsagePiece = _dailyUsage,
            Freq = freq
        };


        try
        {
            
                await _database.AddMedicationAsync(newMedication);
                await DisplayAlert("Başarılı", newMedication.Times + "Yeni ilaç eklendi.", "Tamam");
                await Navigation.PopToRootAsync();
          
            
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }

    }
}