
USE master;
GO
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'CalorieDB')
BEGIN
    CREATE DATABASE CalorieDB COLLATE Cyrillic_General_CI_AS;
END
GO

USE CalorieDB;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Login NVARCHAR(50) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(100) NOT NULL,
        DailyNorm FLOAT NOT NULL DEFAULT 2000
    );
END


IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Foods' AND xtype='U')
BEGIN
    CREATE TABLE Foods (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        CaloriesPer100 FLOAT NOT NULL
    );
END


IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MealEntries' AND xtype='U')
BEGIN
    CREATE TABLE MealEntries (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        FoodId INT NOT NULL,
        Grams FLOAT NOT NULL,
        Date DATE NOT NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id),
        FOREIGN KEY (FoodId) REFERENCES Foods(Id)
    );
END


IF NOT EXISTS (SELECT * FROM Users WHERE Login = '1')
BEGIN
    INSERT INTO Users (Login, PasswordHash, DailyNorm)
    VALUES ('1', '6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b', 2000);
END


MERGE INTO Foods AS target
USING (
    VALUES 
        (N'Куриная грудка', 165),
        (N'Рис отварной', 130),
        (N'Гречка', 132),
        (N'Хлеб белый', 265),
        (N'Яйцо куриное', 155),
        (N'Картофель вареный', 82),
        (N'Лосось', 208),
        (N'Овсянка', 88),
        (N'Банан', 89),
        (N'Яблоко', 52)
        (N'Творог 5%', 121),
        (N'Шоколад молочный', 546),
        (N'Огурец', 15),
        (N'Помидор', 18),
        (N'Сок апельсиновый', 45),
        (N'Чай без сахара', 0)";
) AS source (Name, CaloriesPer100)
ON target.Name = source.Name
WHEN NOT MATCHED THEN
    INSERT (Name, CaloriesPer100) VALUES (source.Name, source.CaloriesPer100);

SELECT * FROM Foods;
GO
