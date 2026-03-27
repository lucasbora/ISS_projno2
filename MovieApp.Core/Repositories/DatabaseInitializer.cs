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
        var builder = new SqlConnectionStringBuilder(_connectionString);
        var dbName = builder.InitialCatalog;
        builder.InitialCatalog = "master";

        using (var masterConnection = new SqlConnection(builder.ConnectionString))
        {
            masterConnection.Open();
            using var createDbCmd = new SqlCommand(
                $"IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'{dbName}') CREATE DATABASE [{dbName}];",
                masterConnection);
            createDbCmd.ExecuteNonQuery();
        }

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        var createTablesSql = @"
IF OBJECT_ID(N'[User]', N'U') IS NULL
BEGIN
    CREATE TABLE [User] (
        UserId INT IDENTITY(1,1) PRIMARY KEY
    );
END;

IF OBJECT_ID(N'Badge', N'U') IS NULL
BEGIN
    CREATE TABLE Badge (
        BadgeId INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        CriteriaValue INT NOT NULL,
        CONSTRAINT CK_Badge_CriteriaValue_NonNegative CHECK (CriteriaValue >= 0)
    );
END;

IF OBJECT_ID(N'Movie', N'U') IS NULL
BEGIN
    CREATE TABLE Movie (
        MovieId INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(255) NOT NULL,
        [Year] INT NOT NULL,
        Genre NVARCHAR(100),
        PosterUrl NVARCHAR(2083),
        AverageRating FLOAT DEFAULT 0.0,
        CONSTRAINT CK_Movie_Year_Reasonable CHECK ([Year] BETWEEN 1888 AND 2100),
        CONSTRAINT CK_Movie_AverageRating_Range CHECK (AverageRating >= 0 AND AverageRating <= 10)
    );
END;

IF OBJECT_ID(N'UserStats', N'U') IS NULL
BEGIN
    CREATE TABLE UserStats (
        StatsId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        TotalPoints INT NOT NULL DEFAULT 0,
        WeeklyScore INT NOT NULL DEFAULT 0,
        CONSTRAINT UQ_UserStats_UserId UNIQUE (UserId),
        CONSTRAINT CK_UserStats_TotalPoints_NonNegative CHECK (TotalPoints >= 0),
        CONSTRAINT CK_UserStats_WeeklyScore_NonNegative CHECK (WeeklyScore >= 0),
        CONSTRAINT FK_UserStats_User FOREIGN KEY (UserId) REFERENCES [User](UserId) ON DELETE CASCADE
    );
END;

IF OBJECT_ID(N'UserBadge', N'U') IS NULL
BEGIN
    CREATE TABLE UserBadge (
        UserId INT NOT NULL,
        BadgeId INT NOT NULL,
        PRIMARY KEY (UserId, BadgeId),
        CONSTRAINT FK_UserBadge_User FOREIGN KEY (UserId) REFERENCES [User](UserId) ON DELETE CASCADE,
        CONSTRAINT FK_UserBadge_Badge FOREIGN KEY (BadgeId) REFERENCES Badge(BadgeId) ON DELETE CASCADE
    );
END;

IF OBJECT_ID(N'Review', N'U') IS NULL
BEGIN
    CREATE TABLE Review (
        ReviewId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        MovieId INT NOT NULL,
        StarRating FLOAT NOT NULL,
        Content NVARCHAR(MAX),
        CreatedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        IsExtraReview BIT NOT NULL DEFAULT 0,
        CinematographyRating INT NULL,
        CinematographyText NVARCHAR(MAX) NULL,
        ActingRating INT NULL,
        ActingText NVARCHAR(MAX) NULL,
        CgiRating INT NULL,
        CgiText NVARCHAR(MAX) NULL,
        PlotRating INT NULL,
        PlotText NVARCHAR(MAX) NULL,
        SoundRating INT NULL,
        SoundText NVARCHAR(MAX) NULL,
        CONSTRAINT CK_Review_StarRating_Range CHECK (StarRating >= 0 AND StarRating <= 10),
        CONSTRAINT FK_Review_User FOREIGN KEY (UserId) REFERENCES [User](UserId) ON DELETE CASCADE,
        CONSTRAINT FK_Review_Movie FOREIGN KEY (MovieId) REFERENCES Movie(MovieId) ON DELETE CASCADE
    );
END;

IF OBJECT_ID(N'Battle', N'U') IS NULL
BEGIN
    CREATE TABLE Battle (
        BattleId INT IDENTITY(1,1) PRIMARY KEY,
        FirstMovieId INT NOT NULL,
        SecondMovieId INT NOT NULL,
        InitialRatingFirstMovie FLOAT NOT NULL,
        InitialRatingSecondMovie FLOAT NOT NULL,
        StartDate DATETIME2(3) NOT NULL,
        EndDate DATETIME2(3) NOT NULL,
        Status INT NOT NULL DEFAULT 0,
        CONSTRAINT CK_Battle_DifferentMovies CHECK (FirstMovieId <> SecondMovieId),
        CONSTRAINT FK_Battle_FirstMovie FOREIGN KEY (FirstMovieId) REFERENCES Movie(MovieId) ON DELETE NO ACTION,
        CONSTRAINT FK_Battle_SecondMovie FOREIGN KEY (SecondMovieId) REFERENCES Movie(MovieId) ON DELETE NO ACTION
    );
END;

IF OBJECT_ID(N'Comment', N'U') IS NULL
BEGIN
    CREATE TABLE Comment (
        MessageId INT IDENTITY(1,1) PRIMARY KEY,
        AuthorId INT NOT NULL,
        MovieId INT NOT NULL,
        ParentCommentId INT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        CreatedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Comment_Author FOREIGN KEY (AuthorId) REFERENCES [User](UserId) ON DELETE NO ACTION,
        CONSTRAINT FK_Comment_Movie FOREIGN KEY (MovieId) REFERENCES Movie(MovieId) ON DELETE CASCADE,
        CONSTRAINT FK_Comment_Parent FOREIGN KEY (ParentCommentId) REFERENCES Comment(MessageId) ON DELETE NO ACTION
    );
END;

IF OBJECT_ID(N'Bet', N'U') IS NULL
BEGIN
    CREATE TABLE Bet (
        BetId INT IDENTITY(1,1) PRIMARY KEY,
        BattleId INT NOT NULL,
        UserId INT NOT NULL,
        MovieId INT NOT NULL,
        Amount INT NOT NULL,
        CONSTRAINT CK_Bet_Amount_Positive CHECK (Amount > 0),
        CONSTRAINT FK_Bet_Battle FOREIGN KEY (BattleId) REFERENCES Battle(BattleId) ON DELETE CASCADE,
        CONSTRAINT FK_Bet_User FOREIGN KEY (UserId) REFERENCES [User](UserId) ON DELETE NO ACTION,
        CONSTRAINT FK_Bet_Movie FOREIGN KEY (MovieId) REFERENCES Movie(MovieId) ON DELETE NO ACTION
    );
END;";

        using (var createCmd = new SqlCommand(createTablesSql, connection))
        {
            createCmd.ExecuteNonQuery();
        }

        var seedSql = @"
IF NOT EXISTS (SELECT 1 FROM [User])
BEGIN
    INSERT INTO [User] DEFAULT VALUES;
    INSERT INTO [User] DEFAULT VALUES;
    INSERT INTO [User] DEFAULT VALUES;
END;

IF NOT EXISTS (SELECT 1 FROM UserStats)
BEGIN
    INSERT INTO UserStats (UserId, TotalPoints, WeeklyScore)
    SELECT UserId,
           CASE WHEN UserId = 1 THEN 50 WHEN UserId = 2 THEN 30 ELSE 20 END,
           CASE WHEN UserId = 1 THEN 10 WHEN UserId = 2 THEN 5 ELSE 3 END
    FROM [User];
END;

IF NOT EXISTS (SELECT 1 FROM Badge)
BEGIN
    INSERT INTO Badge (Name, CriteriaValue) VALUES
    ('First Review', 1),
    ('Critic in Training', 5),
    ('Movie Marathoner', 10),
    ('Discussion Starter', 3),
    ('Battle Strategist', 5),
    ('Top Contributor', 1000);
END;

IF NOT EXISTS (SELECT 1 FROM Movie)
BEGIN
    INSERT INTO Movie (Title, [Year], Genre, PosterUrl, AverageRating) VALUES
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
