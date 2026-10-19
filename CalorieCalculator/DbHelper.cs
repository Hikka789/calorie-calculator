using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace CalorieCounter
{
    public static class DbHelper
    {
        private static readonly string connectionString =
            @"Server=(localdb)\MSSQLLocalDB;Database=CalorieDB;Trusted_Connection=True;";

        static DbHelper()
        {
            InitializeDatabase();
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private static void InitializeDatabase()
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            string dbName = builder.InitialCatalog;
            builder.InitialCatalog = "master";


            using (var conn = new SqlConnection(builder.ConnectionString))
            {
                conn.Open();
                string createDbSql = $@"
                    IF DB_ID('{dbName}') IS NULL
                    BEGIN
                        CREATE DATABASE [{dbName}]
                    END";
                using (var cmd = new SqlCommand(createDbSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }


            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();


                string createTablesSql = @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
                    BEGIN
                        CREATE TABLE Users (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Login NVARCHAR(100) NOT NULL UNIQUE,
                            PasswordHash NVARCHAR(256) NOT NULL,
                            DailyNorm FLOAT NOT NULL DEFAULT 2000
                        )
                    END

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Foods')
                    BEGIN
                        CREATE TABLE Foods (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Name NVARCHAR(200) NOT NULL,
                            CaloriesPer100 FLOAT NOT NULL
                        )
                    END

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MealEntries')
                    BEGIN
                        CREATE TABLE MealEntries (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            UserId INT NOT NULL,
                            FoodId INT NOT NULL,
                            Grams FLOAT NOT NULL,
                            Date DATE NOT NULL,
                            FOREIGN KEY (UserId) REFERENCES Users(Id),
                            FOREIGN KEY (FoodId) REFERENCES Foods(Id)
                        )
                    END";
                using (var cmd = new SqlCommand(createTablesSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }


                string checkFoodsSql = "SELECT COUNT(*) FROM Foods";
                using (var cmd = new SqlCommand(checkFoodsSql, conn))
                {
                    int foodsCount = (int)cmd.ExecuteScalar();
                    if (foodsCount == 0)
                    {
                        string insertFoodsSql = @"
                            INSERT INTO Foods (Name, CaloriesPer100) VALUES 
                            (N'Куриная грудка', 113),
                            (N'Гречка отварная', 110),
                            (N'Рис отварной', 116),
                            (N'Яйцо куриное', 155),
                            (N'Молоко 2.5%', 52),
                            (N'Хлеб белый', 265),
                            (N'Сыр', 350),
                            (N'Яблоко', 52),
                            (N'Банан', 89),
                            (N'Картофель', 77),
                            (N'Овсянка', 68),
                            (N'Макароны отварные', 112),
                            (N'Свинина', 259),
                            (N'Лосось', 208),
                            (N'Творог 5%', 121),
                            (N'Шоколад молочный', 546),
                            (N'Огурец', 15),
                            (N'Помидор', 18),
                            (N'Сок апельсиновый', 45),
                            (N'Чай без сахара', 0)";
                        using (var insertCmd = new SqlCommand(insertFoodsSql, conn))
                        {
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }

                EnsureTestUserInternal(conn);
            }
        }

        private static void EnsureTestUserInternal(SqlConnection conn)
        {
            string checkSql = "SELECT COUNT(*) FROM Users WHERE Login = '1'";
            using (var cmd = new SqlCommand(checkSql, conn))
            {
                int count = (int)cmd.ExecuteScalar();
                if (count == 0)
                {
                    string hash = HashPassword("1");
                    string insertSql = "INSERT INTO Users (Login, PasswordHash, DailyNorm) VALUES ('1', @hash, 2000)";
                    using (var insertCmd = new SqlCommand(insertSql, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@hash", hash);
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void EnsureTestUser()
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                EnsureTestUserInternal(conn);
            }
        }

        public static int Login(string login, string password)
        {
            string hash = HashPassword(password);
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT Id FROM Users WHERE Login = @login AND PasswordHash = @hash";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@hash", hash);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    return Convert.ToInt32(result);
                return -1;
            }
        }

        public static bool Register(string login, string password, double dailyNorm)
        {
            string hash = HashPassword(password);
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "INSERT INTO Users (Login, PasswordHash, DailyNorm) VALUES (@login, @hash, @norm)";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@hash", hash);
                cmd.Parameters.AddWithValue("@norm", dailyNorm);
                try
                {
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static double GetDailyNorm(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT DailyNorm FROM Users WHERE Id = @id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", userId);
                return Convert.ToDouble(cmd.ExecuteScalar());
            }
        }


        public static DataTable GetFoods()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT Id, Name, CaloriesPer100 FROM Foods";
                SqlCommand cmd = new SqlCommand(sql, conn);
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

        public static void AddMeal(int userId, int foodId, double grams, DateTime date)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "INSERT INTO MealEntries (UserId, FoodId, Grams, Date) VALUES (@uid, @fid, @g, @date)";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@fid", foodId);
                cmd.Parameters.AddWithValue("@g", grams);
                cmd.Parameters.AddWithValue("@date", date.Date);
                cmd.ExecuteNonQuery();
            }
        }

        public static DataTable GetMealsByDate(int userId, DateTime date)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT f.Name, m.Grams, (m.Grams * f.CaloriesPer100 / 100) AS Calories
                    FROM MealEntries m
                    JOIN Foods f ON m.FoodId = f.Id
                    WHERE m.UserId = @uid AND m.Date = @date";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@date", date.Date);
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }
    }
}
