using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.ComponentModel;

using System.Globalization;

namespace Ilac;


public partial class MedCalendarPage : ContentPage, INotifyPropertyChanged
{
    public DateTime currentDate { get; set; }
    private List<DateTime> medicationDates;

    private string _currentMonthYear;
    public string CurrentMonthYear
    {
        get => _currentMonthYear;
        set
        {
            if (_currentMonthYear != value)
            {
                _currentMonthYear = value;
                OnPropertyChanged();
            }
        }
    }
    private readonly Database _database;

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public MedCalendarPage()
    {
        InitializeComponent();
        currentDate = DateTime.Now;
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "medications.db");
        _database = new Database(dbPath);
        BindingContext = this;


        // İlaç tarihlerinin listesi

    }

    public async void OnloadTitle(object Sender, EventArgs e)
    {
        var medications = await _database.GetMedicationsAsync();
        string format = "dd.MM.yyyy"; // giriş formatı
        CultureInfo culture = CultureInfo.InvariantCulture;
        medicationDates = new List<DateTime> { };
        foreach (var Item in medications)
        {
            Console.WriteLine(Item.NextDay);

            DateTime.TryParseExact(Item.NextDay, format, culture, DateTimeStyles.None, out DateTime tarih);
            Console.WriteLine(tarih);

            var strTarih = tarih.ToString("yyyy.MM.dd");
            Console.WriteLine(strTarih);

            DateTime.TryParseExact(strTarih, "yyyy.MM.dd", culture, DateTimeStyles.None, out DateTime changedtarih);
            Console.WriteLine(changedtarih);
            medicationDates.Add(changedtarih);

        }


        BindingContext = this;
        UpdateCalendar();
        
    }

    private void UpdateCalendar()
    {

        // Ay ve yıl başlığını güncelle
        CurrentMonthYear = currentDate.ToString("MMMM yyyy");

        // Takvim gridini temizle
        CalendarGrid.Children.Clear();
        CalendarGrid.RowDefinitions.Clear();

        // Ayın ilk günü ve toplam gün sayısı
        DateTime firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
        int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);

        // İlk günün haftanın hangi gününe denk geldiğini bul
        int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
        startDayOfWeek = (startDayOfWeek == 0) ? 6 : startDayOfWeek - 1; // Pazar -> 6, Pazartesi -> 0

        // Kaç satır gerektiğini hesapla
        int totalCells = daysInMonth + startDayOfWeek;
        int rows = (int)Math.Ceiling(totalCells / 7.0);

        for (int i = 0; i < rows; i++)
        {
            CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        // Günleri Grid'e ekle
        int day = 1;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                if (row == 0 && col < startDayOfWeek || day > daysInMonth)
                {
                    // Boş hücreler için boş bir kutu ekle
                    var emptyBox = new BoxView { Color = Colors.Transparent };
                    Grid.SetRow(emptyBox, row);
                    Grid.SetColumn(emptyBox, col);
                    CalendarGrid.Children.Add(emptyBox);

                }
                else
                {
                    // Gün kutusunu oluştur
                    var isMedicationDay = medicationDates.Contains(new DateTime(currentDate.Year, currentDate.Month, day));
                    var dayBox = new Frame
                    {
                        BackgroundColor = isMedicationDay ? Colors.LightGreen : Colors.LightGray,
                        BorderColor = Colors.DarkGray,
                        CornerRadius = 5,
                        Padding = 5,
                        Content = new Label
                        {
                            Text = day.ToString(),
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center,
                            TextColor = isMedicationDay ? Colors.White : Colors.Black
                        }
                    };
                    var tapGestureRecognizer = new TapGestureRecognizer();
                    int currentDay = day; // day değişkenini local bir değişkene kopyala
                    tapGestureRecognizer.Tapped += (s, e) =>
                    {
                        var selectedDate = new DateTime(currentDate.Year, currentDate.Month, currentDay);
                        OnDaySelected(selectedDate);
                    };
                    dayBox.GestureRecognizers.Add(tapGestureRecognizer);


                    // Kutuyu grid'e ekle
                    Grid.SetRow(dayBox, row);
                    Grid.SetColumn(dayBox, col);
                    CalendarGrid.Children.Add(dayBox);

                    day++;
                }
            }
        }
        BindingContext = this;

    }

    private async void OnDaySelected(DateTime selectedDate)
{
    // Seçilen tarihte yapılacak işlemleri burada gerçekleştirin
    if (medicationDates.Contains(selectedDate))
    {
        var medications = await _database.GetMedicationsAsync(); // Seçilen tarihteki ilaçları al
         // İlaçları bir string olarak birleştir
        var list = medications
        .Where(m => m.NextDay == selectedDate.ToString("dd.MM.yyyy"))
        .Select(m => m.Name)
        .ToList();

        await DisplayAlert("İlaç Günü", 
            $"Seçilen tarih ({selectedDate.ToShortDateString()}) için planlanmış ilaçlarınız:\n{string.Join(", ", list)}", 
            "Tamam");
    }
    else
    {
        await DisplayAlert("Bilgi", 
            $"Seçilen tarih: {selectedDate.ToShortDateString()}", 
            "Tamam");
    }
}

    private void PreviousMonth_Clicked(object sender, EventArgs e)
    {
        currentDate = currentDate.AddMonths(-1);
        UpdateCalendar();
    }

    private void NextMonth_Clicked(object sender, EventArgs e)
    {
        currentDate = currentDate.AddMonths(1);
        UpdateCalendar();
    }
}