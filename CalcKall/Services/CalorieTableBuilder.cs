using CalcKall.Models;

namespace CalcKall.Services;

public class CalorieTableRow
{
    public double HeightCm { get; set; }
    public Dictionary<double, double> ByWeight { get; set; } = new();
}

public class CalorieTableBuilder
{
    private readonly CalorieCalculator _calc;

    public CalorieTableBuilder(CalorieCalculator calc) => _calc = calc;

    public List<CalorieTableRow> Build(
        IEnumerable<double> heights,
        IEnumerable<double> weights,
        Gender gender,
        int age,
        ActivityLevel activity)
    {
        var result = new List<CalorieTableRow>();

        foreach (var h in heights)
        {
            var row = new CalorieTableRow { HeightCm = h };
            foreach (var w in weights)
            {
                var profile = new UserProfile
                {
                    Gender = gender,
                    WeightKg = w,
                    HeightCm = h,
                    Age = age,
                    Activity = activity
                };
                row.ByWeight[w] = _calc.CalculateDailyCalories(profile);
            }
            result.Add(row);
        }

        return result;
    }

    public static double[] DefaultHeights() =>
        Enumerable.Range(0, 6).Select(i => 150.0 + i * 10).ToArray();

    public static double[] DefaultWeights() =>
        Enumerable.Range(0, 7).Select(i => 40.0 + i * 10).ToArray();
}