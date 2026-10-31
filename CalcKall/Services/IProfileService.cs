using CalcKall.Models;

namespace CalcKall.Services;

public interface IProfileService
{
    void SaveProfile(int userId, UserProfile profile);
    UserProfile? LoadProfile(int userId);
}