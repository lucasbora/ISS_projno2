#nullable enable
using Microsoft.Data.SqlClient;

namespace MovieApp.Core.Repositories;

public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void EnsureCreatedAndSeeded()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        var createTablesSql = @"
IF OBJECT_ID(N'Users', N'U') IS NULL
BEGIN
    CREATE TABLE Users (
        UserId INT IDENTITY(1,1) PRIMARY KEY
    );
END;

IF OBJECT_ID(N'Movies', N'U') IS NULL
BEGIN
    CREATE TABLE Movies (
        MovieId INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(500) NOT NULL,
        [Year] INT NOT NULL,
        PosterUrl NVARCHAR(2000) NOT NULL,
        Genre NVARCHAR(100) NOT NULL,
        AverageRating FLOAT NOT NULL
    );
END;

IF OBJECT_ID(N'Reviews', N'U') IS NULL
BEGIN
    CREATE TABLE Reviews (
        ReviewId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        MovieId INT NOT NULL,
        StarRating REAL NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        IsExtraReview BIT NOT NULL,
        CinematographyRating INT NOT NULL,
        CinematographyText NVARCHAR(MAX) NULL,
        ActingRating INT NOT NULL,
        ActingText NVARCHAR(MAX) NULL,
        CgiRating INT NOT NULL,
        CgiText NVARCHAR(MAX) NULL,
        PlotRating INT NOT NULL,
        PlotText NVARCHAR(MAX) NULL,
        SoundRating INT NOT NULL,
        SoundText NVARCHAR(MAX) NULL,
        CONSTRAINT FK_Reviews_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
        CONSTRAINT FK_Reviews_Movies FOREIGN KEY (MovieId) REFERENCES Movies(MovieId)
    );
END;

IF OBJECT_ID(N'Comments', N'U') IS NULL
BEGIN
    CREATE TABLE Comments (
        MessageId INT IDENTITY(1,1) PRIMARY KEY,
        AuthorId INT NOT NULL,
        MovieId INT NOT NULL,
        ParentCommentId INT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_Comments_Users FOREIGN KEY (AuthorId) REFERENCES Users(UserId),
        CONSTRAINT FK_Comments_Movies FOREIGN KEY (MovieId) REFERENCES Movies(MovieId),
        CONSTRAINT FK_Comments_Parent FOREIGN KEY (ParentCommentId) REFERENCES Comments(MessageId)
    );
END;

IF OBJECT_ID(N'Battles', N'U') IS NULL
BEGIN
    CREATE TABLE Battles (
        BattleId INT IDENTITY(1,1) PRIMARY KEY,
        FirstMovieId INT NOT NULL,
        SecondMovieId INT NOT NULL,
        InitialRatingFirstMovie FLOAT NOT NULL,
        InitialRatingSecondMovie FLOAT NOT NULL,
        StartDate DATETIME2 NOT NULL,
        EndDate DATETIME2 NOT NULL,
        Status NVARCHAR(50) NOT NULL,
        CONSTRAINT FK_Battles_FirstMovie FOREIGN KEY (FirstMovieId) REFERENCES Movies(MovieId),
        CONSTRAINT FK_Battles_SecondMovie FOREIGN KEY (SecondMovieId) REFERENCES Movies(MovieId)
    );
END;

IF OBJECT_ID(N'Bets', N'U') IS NULL
BEGIN
    CREATE TABLE Bets (
        UserId INT NOT NULL,
        BattleId INT NOT NULL,
        MovieId INT NOT NULL,
        Amount INT NOT NULL,
        CONSTRAINT PK_Bets PRIMARY KEY (UserId, BattleId),
        CONSTRAINT FK_Bets_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
        CONSTRAINT FK_Bets_Battles FOREIGN KEY (BattleId) REFERENCES Battles(BattleId),
        CONSTRAINT FK_Bets_Movies FOREIGN KEY (MovieId) REFERENCES Movies(MovieId)
    );
END;

IF OBJECT_ID(N'Badges', N'U') IS NULL
BEGIN
    CREATE TABLE Badges (
        BadgeId INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        CriteriaValue INT NOT NULL
    );
END;

IF OBJECT_ID(N'UserBadges', N'U') IS NULL
BEGIN
    CREATE TABLE UserBadges (
        UserId INT NOT NULL,
        BadgeId INT NOT NULL,
        CONSTRAINT PK_UserBadges PRIMARY KEY (UserId, BadgeId),
        CONSTRAINT FK_UserBadges_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
        CONSTRAINT FK_UserBadges_Badges FOREIGN KEY (BadgeId) REFERENCES Badges(BadgeId)
    );
END;

IF OBJECT_ID(N'UserStats', N'U') IS NULL
BEGIN
    CREATE TABLE UserStats (
        StatsId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        TotalPoints INT NOT NULL,
        WeeklyScore INT NOT NULL,
        CONSTRAINT FK_UserStats_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
        CONSTRAINT UQ_UserStats_UserId UNIQUE (UserId)
    );
END;";

        using (var createCmd = new SqlCommand(createTablesSql, connection))
        {
            createCmd.ExecuteNonQuery();
        }

        var seedSql = @"
IF NOT EXISTS (SELECT 1 FROM Users)
BEGIN
    INSERT INTO Users DEFAULT VALUES;
    INSERT INTO Users DEFAULT VALUES;
    INSERT INTO Users DEFAULT VALUES;
END;

IF NOT EXISTS (SELECT 1 FROM UserStats)
BEGIN
    INSERT INTO UserStats (UserId, TotalPoints, WeeklyScore)
    SELECT UserId,
           CASE WHEN UserId = 1 THEN 50 WHEN UserId = 2 THEN 30 ELSE 20 END,
           CASE WHEN UserId = 1 THEN 10 WHEN UserId = 2 THEN 5 ELSE 3 END
    FROM Users;
END;

IF NOT EXISTS (SELECT 1 FROM Badges)
BEGIN
    INSERT INTO Badges (Name, CriteriaValue)
    VALUES
    ('The Snob', 10),
    ('The Super Serious', 50),
    ('The Joker', 70),
    ('The Godfather I', 100),
    ('The Godfather II', 200),
    ('The Godfather III', 300);
END;

IF NOT EXISTS (SELECT 1 FROM Movies)
BEGIN
    INSERT INTO Movies (Title, [Year], Genre, PosterUrl, AverageRating)
    VALUES
    ('The Shawshank Redemption', 1994, 'Drama', 'https://m.media-amazon.com/images/M/MV5BMDAyY2FhYjctNDc5OS00MDNlLThiMGUtY2UxYWVkNGY2ZjljXkEyXkFqcGc@._V1_.jpg', 4.5),
    ('The Dark Knight', 2008, 'Action', 'https://m.media-amazon.com/images/M/MV5BMTMxNTMwODM0NF5BMl5BanBnXkFtZTcwODAyMTk2Mw@@._V1_.jpg', 4.3),
    ('Inception', 2010, 'Sci-Fi', 'https://m.media-amazon.com/images/M/MV5BMjAxMzY3NjcxNF5BMl5BanBnXkFtZTcwNTI5OTM0Mw@@._V1_.jpg', 4.2),
    ('Pulp Fiction', 1994, 'Crime', 'https://m.media-amazon.com/images/M/MV5BNGNhMDIzZTUtNTBlZi00MTRlLWFjMDYtZjYwMjY2ZWU5ZjljXkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_.jpg', 4.4),
    ('The Hangover', 2009, 'Comedy', 'https://m.media-amazon.com/images/M/MV5BNGQwZjg5YmYtY2VkNC00NzliLTljYTctNzI5NmU3MjE2ODQzXkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_.jpg', 3.5),
    ('Interstellar', 2014, 'Sci-Fi', 'https://m.media-amazon.com/images/M/MV5BZjdkOTU3MDktN2IxOS00OGEyLWFmMjktY2FiMmZkNWIyODZiXkEyXkFqcGdeQXVyMTMxODk2OTU@._V1_.jpg', 4.6);
END;";

        using var seedCmd = new SqlCommand(seedSql, connection);
        seedCmd.ExecuteNonQuery();
    }
}
