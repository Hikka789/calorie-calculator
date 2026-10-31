using System;
using System.Data.SqlClient;
using CalcKall.Models;

namespace CalcKall.Services;

public class DbProfileService : IProfileService
{
    // СЮДА БД-РАЗРАБОТЧИК ВСТАВИТ СВОЮ СТРОКУ ПОДКЛЮЧЕНИЯ
    private readonly string _connectionString =
        "Server=localhost;Database=CalcKallDb;Trusted_Connection=True;TrustServerCertificate=True;";

    public void SaveProfile(int userId, UserProfile profile)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        // Сохраняем через MERGE (обновить, если есть, или вставить, если нет)
        const string sql = @"
            MERGE INTO UserProfiles AS target
            USING (SELECT @UserId AS UserId) AS source
            ON target.UserId = source.UserId
            WHEN MATCHED THEN
                UPDATE SET Weight = @Weight, Height = @Height, Age = @Age, 
                           Gender = @Gender, Activity = @Activity
            WHEN NOT MATCHED THEN
                INSERT (UserId, Weight, Height, Age, Gender, Activity)
                VALUES (@UserId, @Weight, @Height, @Age, @Gender, @Activity);";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@Weight", profile.WeightKg);
        cmd.Parameters.AddWithValue("@Height", profile.HeightCm);
        cmd.Parameters.AddWithValue("@Age", profile.Age);
        cmd.Parameters.AddWithValue("@Gender", (int)profile.Gender); // Enum в int
        cmd.Parameters.AddWithValue("@Activity", (int)profile.Activity); // Enum в int

        cmd.ExecuteNonQuery();
    }

    public UserProfile? LoadProfile(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = "SELECT Weight, Height, Age, Gender, Activity FROM UserProfiles WHERE UserId = @UserId";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@UserId", userId);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        return new UserProfile
        {
            WeightKg = reader.GetDouble(0),
            HeightCm = reader.GetDouble(1),
            Age = reader.GetInt32(2),
            Gender = (Gender)reader.GetInt32(3),
            Activity = (ActivityLevel)reader.GetInt32(4)
        };
    }
}