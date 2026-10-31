using CalcKall.Models;

namespace CalcKall.Services;

public class CalorieCalculator
{
    public static readonly Dictionary<ActivityLevel, double> Coefficients = new()
    {
        [ActivityLevel.Min] = 1.2,
        [ActivityLevel.Low] = 1.375,
        [ActivityLevel.Medium] = 1.55,
        [ActivityLevel.High] = 1.725,
        [ActivityLevel.VeryHigh] = 1.9
    };

    public double CalculateBmr(Gender gender, double weightKg, double heightCm, int age)
    {
        if (weightKg <= 0) throw new ArgumentException("Вес должен быть положительным.");
        if (heightCm <= 0) throw new ArgumentException("Рост должен быть положительным.");
        if (age < 14 || age > 100) throw new ArgumentException("Возраст 14–100.");

        return gender == Gender.Male
            ? 10 * weightKg + 6.25 * heightCm - 5 * age + 5
            : 10 * weightKg + 6.25 * heightCm - 5 * age - 161;
    }

    public double CalculateDailyCalories(UserProfile profile)
    {
        if (profile == null) throw new ArgumentNullException(nameof(profile));
        double bmr = CalculateBmr(profile.Gender, profile.WeightKg, profile.HeightCm, profile.Age);
        return Math.Round(bmr * Coefficients[profile.Activity], 0);
    }

    public (double Min, double Max) CalculateRange(UserProfile profile)
    {
        double bmr = CalculateBmr(profile.Gender, profile.WeightKg, profile.HeightCm, profile.Age);
        return (Math.Round(bmr * Coefficients[ActivityLevel.Min], 0),
                Math.Round(bmr * Coefficients[ActivityLevel.VeryHigh], 0));
    }
}