using CalcKall.Models;

namespace CalcKall.Services;

public class CalorieService
{
    private readonly CalorieCalculator _calc;

    public CalorieService(CalorieCalculator calc)
    {
        _calc = calc;
    }

    // Принимает строки из TextBox'ов, возвращает готовый текст для UI
    public string CalculateFromStrings(string weightStr, string heightStr, string ageStr, Gender gender, ActivityLevel activity)
    {
        if (!double.TryParse(weightStr, out double weight) || weight <= 0)
            return "Ошибка: Вес должен быть положительным числом.";

        if (!double.TryParse(heightStr, out double height) || height <= 0)
            return "Ошибка: Рост должен быть положительным числом.";

        if (!int.TryParse(ageStr, out int age))
            return "Ошибка: Возраст должен быть целым числом.";

        var profile = new UserProfile
        {
            Gender = gender,
            WeightKg = weight,
            HeightCm = height,
            Age = age,
            Activity = activity
        };

        try
        {
            double norm = _calc.CalculateDailyCalories(profile);
            var (min, max) = _calc.CalculateRange(profile);

            return $"Норма ({activity}): {norm} ккал/день\n" +
                   $"Диапазон активности: {min}–{max} ккал/день";
        }
        catch (ArgumentException ex)
        {
            return "Ошибка: " + ex.Message;
        }
    }
}