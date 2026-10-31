using System;
using System.Data;
using System.Linq;
using System.Windows;
using CalcKall.Models;
using CalcKall.Services;

namespace CalcKall;

public partial class MainWindow : Window
{
    private readonly CalorieCalculator _calc = new();
    private readonly CalorieTableBuilder _tableBuilder;
    private readonly CalorieService _calorieService; // Новый сервис для расчета
    private readonly IProfileService _profileService; // Новый сервис для БД

    public MainWindow()
    {
        InitializeComponent();

        // Инициализируем сервисы
        _tableBuilder = new CalorieTableBuilder(_calc);
        _calorieService = new CalorieService(_calc);

        // ВНИМАНИЕ: Здесь мы используем DbProfileService. 
        // Если БД еще не готова, программа упадет при нажатии на кнопку сохранения/загрузки.
        _profileService = new DbProfileService();
    }

    // --- ВКЛАДКА 1: КАЛЬКУЛЯТОР ---

    private void Calculate_Click(object sender, RoutedEventArgs e)
    {
        var gender = GenderBox.SelectedIndex == 0 ? Gender.Male : Gender.Female;

        // Пока активность Medium. Позже можем добавить ComboBox на первую вкладку.
        var activity = ActivityLevel.Medium;

        // Вызываем сервис, он сам распарсит строки, проверит ошибки и вернет готовый текст
        ResultBlock.Text = _calorieService.CalculateFromStrings(
            WeightBox.Text, HeightBox.Text, AgeBox.Text, gender, activity);
    }

    private void SaveProfile_Click(object sender, RoutedEventArgs e)
    {
        if (!double.TryParse(WeightBox.Text, out double weight) ||
            !double.TryParse(HeightBox.Text, out double height) ||
            !int.TryParse(AgeBox.Text, out int age))
        {
            ResultBlock.Text = "Введите корректные данные для сохранения.";
            return;
        }

        var profile = new UserProfile
        {
            WeightKg = weight,
            HeightCm = height,
            Age = age,
            Gender = GenderBox.SelectedIndex == 0 ? Gender.Male : Gender.Female,
            Activity = ActivityLevel.Medium
        };

        try
        {
            _profileService.SaveProfile(1, profile); // userId = 1 (временная заглушка)
            ResultBlock.Text = "Профиль сохранен в БД!";
        }
        catch (Exception ex)
        {
            ResultBlock.Text = "Ошибка сохранения: " + ex.Message;
        }
    }

    private void LoadProfile_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var profile = _profileService.LoadProfile(1); // userId = 1
            if (profile == null)
            {
                ResultBlock.Text = "Профиль не найден.";
                return;
            }

            // Заполняем TextBox'ы
            WeightBox.Text = profile.WeightKg.ToString();
            HeightBox.Text = profile.HeightCm.ToString();
            AgeBox.Text = profile.Age.ToString();
            GenderBox.SelectedIndex = profile.Gender == Gender.Male ? 0 : 1;

            ResultBlock.Text = "Профиль загружен из БД!";
        }
        catch (Exception ex)
        {
            ResultBlock.Text = "Ошибка загрузки: " + ex.Message;
        }
    }

    // --- ВКЛАДКА 2: ТАБЛИЦА ---

    private void BuildTable_Click(object sender, RoutedEventArgs e)
    {
        TableInfoBlock.Text = "";

        if (!int.TryParse(TableAgeBox.Text, out int age) || age < 14 || age > 100)
        {
            TableInfoBlock.Text = "Возраст должен быть от 14 до 100.";
            return;
        }

        var gender = TableGenderBox.SelectedIndex == 0 ? Gender.Male : Gender.Female;
        var activity = (ActivityLevel)Math.Max(0, TableActivityBox.SelectedIndex);

        try
        {
            var rows = _tableBuilder.Build(
                CalorieTableBuilder.DefaultHeights(),
                CalorieTableBuilder.DefaultWeights(),
                gender, age, activity);

            var table = new DataTable();

            table.Columns.Add("Рост\\Вес", typeof(string));
            foreach (var w in CalorieTableBuilder.DefaultWeights())
                table.Columns.Add($"{w} кг", typeof(double));

            foreach (var row in rows)
            {
                var dr = table.NewRow();
                dr[0] = $"{row.HeightCm} см";
                int i = 1;
                foreach (var w in CalorieTableBuilder.DefaultWeights())
                    dr[i++] = row.ByWeight[w];
                table.Rows.Add(dr);
            }

            CalorieGrid.ItemsSource = table.DefaultView;
            TableInfoBlock.Text =
                $"Таблица для {gender}, {age} лет, активность {activity}. " +
                $"Диапазон: рост 150–200 см, вес 40–100 кг.";
        }
        catch (Exception ex)
        {
            TableInfoBlock.Text = "Ошибка: " + ex.Message;
        }
    }
}