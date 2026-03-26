#nullable enable
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;

namespace MovieApp.Core.Data;

/// <summary>
/// Seeds the database with initial data if it is empty.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds the database with initial movies, users, badges, and sample reviews.
    /// Only seeds if the database is empty.
    /// </summary>
    /// <param name="context">The database context.</param>
    public static async Task SeedAsync(MovieAppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Movies.AnyAsync())
            return;

        // Seed Users
        var users = new List<User>
        {
            new User(),
            new User(),
            new User()
        };
        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        // Seed UserStats for each user
        var userStats = new List<UserStats>
        {
            new UserStats { UserId = users[0].UserId, TotalPoints = 50, WeeklyScore = 10 },
            new UserStats { UserId = users[1].UserId, TotalPoints = 30, WeeklyScore = 5 },
            new UserStats { UserId = users[2].UserId, TotalPoints = 20, WeeklyScore = 3 }
        };
        context.UserStats.AddRange(userStats);
        await context.SaveChangesAsync();

        // Seed Badges (all 5 types)
        var badges = new List<Badge>
        {
            new Badge { Name = "The Snob", CriteriaValue = 10 },
            new Badge { Name = "The Super Serious", CriteriaValue = 50 },
            new Badge { Name = "The Joker", CriteriaValue = 70 },
            new Badge { Name = "The Godfather I", CriteriaValue = 100 },
            new Badge { Name = "The Godfather II", CriteriaValue = 200 },
            new Badge { Name = "The Godfather III", CriteriaValue = 300 }
        };
        context.Badges.AddRange(badges);
        await context.SaveChangesAsync();

        // Seed Movies (10+ movies)
        var movies = new List<Movie>
        {
            new Movie { Title = "The Shawshank Redemption", Year = 1994, Genre = "Drama", PosterUrl = "https://m.media-amazon.com/images/M/MV5BMDAyY2FhYjctNDc5OS00MDNlLThiMGUtY2UxYWVkNGY2ZjljXkEyXkFqcGc@._V1_.jpg", AverageRating = 4.5 },
            new Movie { Title = "The Dark Knight", Year = 2008, Genre = "Action", PosterUrl = "https://m.media-amazon.com/images/M/MV5BMTMxNTMwODM0NF5BMl5BanBnXkFtZTcwODAyMTk2Mw@@._V1_.jpg", AverageRating = 4.3 },
            new Movie { Title = "Inception", Year = 2010, Genre = "Sci-Fi", PosterUrl = "https://m.media-amazon.com/images/M/MV5BMjAxMzY3NjcxNF5BMl5BanBnXkFtZTcwNTI5OTM0Mw@@._V1_.jpg", AverageRating = 4.2 },
            new Movie { Title = "Pulp Fiction", Year = 1994, Genre = "Crime", PosterUrl = "https://m.media-amazon.com/images/M/MV5BNGNhMDIzZTUtNTBlZi00MTRlLWFjMDYtZjYwMjY2ZWU5ZjljXkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_.jpg", AverageRating = 4.4 },
            new Movie { Title = "The Hangover", Year = 2009, Genre = "Comedy", PosterUrl = "https://m.media-amazon.com/images/M/MV5BNGQwZjg5YmYtY2VkNC00NzliLTljYTctNzI5NmU3MjE2ODQzXkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_.jpg", AverageRating = 3.5 },
            new Movie { Title = "Superbad", Year = 2007, Genre = "Comedy", PosterUrl = "https://m.media-amazon.com/images/M/MV5BMTc0NjIyMjA2OF5BMl5BanBnXkFtZTcwMzIxMjA1MQ@@._V1_.jpg", AverageRating = 3.3 },
            new Movie { Title = "Interstellar", Year = 2014, Genre = "Sci-Fi", PosterUrl = "https://m.media-amazon.com/images/M/MV5BZjdkOTU3MDktN2IxOS00OGEyLWFmMjktY2FiMmZkNWIyODZiXkEyXkFqcGdeQXVyMTMxODk2OTU@._V1_.jpg", AverageRating = 4.6 },
            new Movie { Title = "The Godfather", Year = 1972, Genre = "Crime", PosterUrl = "https://m.media-amazon.com/images/M/MV5BM2MyNjYxNmUtYTAwNi00MTYxLWJmNWYtYzZlODY3ZTk3OTFlXkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_.jpg", AverageRating = 4.7 },
            new Movie { Title = "Fight Club", Year = 1999, Genre = "Drama", PosterUrl = "https://m.media-amazon.com/images/M/MV5BMmEzNTkxYjQtZTc0MC00YTVjLTg5ZTEtZWMwOWVlYzY0NWIwXkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_.jpg", AverageRating = 4.4 },
            new Movie { Title = "Forrest Gump", Year = 1994, Genre = "Drama", PosterUrl = "https://m.media-amazon.com/images/M/MV5BNWIwODRlZTUtY2U3ZS00Yzg1LWJhNzYtMmZiYmEyNjU1MjQ0XkEyXkFqcGdeQXVyMTQxNzMzNDI@._V1_.jpg", AverageRating = 4.3 },
            new Movie { Title = "The Matrix", Year = 1999, Genre = "Sci-Fi", PosterUrl = "https://m.media-amazon.com/images/M/MV5BNzQzOTk3OTAtNDQ0Zi00ZTVkLWI0MTEtMDllZjNlYzNjNTc4L2ltYWdlXkEyXkFqcGdeQXVyNjU0OTQ0OTY@._V1_.jpg", AverageRating = 4.5 },
            new Movie { Title = "Bridesmaids", Year = 2011, Genre = "Comedy", PosterUrl = "https://m.media-amazon.com/images/M/MV5BMjAyOTg4MDMwNl5BMl5BanBnXkFtZTcwOTQyMDM4NA@@._V1_.jpg", AverageRating = 3.4 }
        };
        context.Movies.AddRange(movies);
        await context.SaveChangesAsync();

        // Seed sample reviews
        var reviews = new List<Review>
        {
            new Review
            {
                UserId = users[0].UserId,
                MovieId = movies[0].MovieId,
                StarRating = 5.0f,
                Content = "An absolute masterpiece. The storytelling is beyond compare and the performances are incredible.",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Review
            {
                UserId = users[1].UserId,
                MovieId = movies[0].MovieId,
                StarRating = 4.0f,
                Content = "One of the best films ever made. The pacing is perfect and the ending is satisfying.",
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new Review
            {
                UserId = users[0].UserId,
                MovieId = movies[1].MovieId,
                StarRating = 4.5f,
                Content = "Heath Ledger's performance as the Joker is legendary. A defining superhero film.",
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new Review
            {
                UserId = users[2].UserId,
                MovieId = movies[2].MovieId,
                StarRating = 4.0f,
                Content = "Mind-bending and visually stunning. Nolan at his finest.",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Review
            {
                UserId = users[1].UserId,
                MovieId = movies[3].MovieId,
                StarRating = 4.5f,
                Content = "Tarantino's magnum opus. The dialogue is razor-sharp and unforgettable.",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Review
            {
                UserId = users[2].UserId,
                MovieId = movies[4].MovieId,
                StarRating = 3.5f,
                Content = "Hilarious from start to finish. A comedy classic for the ages.",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };
        context.Reviews.AddRange(reviews);
        await context.SaveChangesAsync();

        // Seed sample comments
        var comments = new List<Comment>
        {
            new Comment
            {
                AuthorId = users[0].UserId,
                MovieId = movies[0].MovieId,
                Content = "This movie changed my life. Everyone should watch it at least once.",
                CreatedAt = DateTime.UtcNow.AddDays(-9)
            },
            new Comment
            {
                AuthorId = users[1].UserId,
                MovieId = movies[0].MovieId,
                Content = "I agree, it's a timeless classic!",
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                ParentCommentId = null // Will be set after first save
            },
            new Comment
            {
                AuthorId = users[2].UserId,
                MovieId = movies[1].MovieId,
                Content = "The Dark Knight set a new standard for superhero films.",
                CreatedAt = DateTime.UtcNow.AddDays(-6)
            }
        };
        context.Comments.AddRange(comments);
        await context.SaveChangesAsync();

        // Set the reply parent
        comments[1].ParentCommentId = comments[0].MessageId;
        await context.SaveChangesAsync();
    }
}
