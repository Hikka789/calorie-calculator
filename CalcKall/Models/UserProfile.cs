namespace CalcKall.Models;

public class UserProfile
{
    public Gender Gender { get; set; }
    public double WeightKg { get; set; }
    public double HeightCm { get; set; }
    public int Age { get; set; }
    public ActivityLevel Activity { get; set; }
}