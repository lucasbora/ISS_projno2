#nullable enable
using Microsoft.EntityFrameworkCore;
using Moq;
using MovieApp.Core.Data;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Tests;

/// <summary>
/// Unit tests for the ReviewService class.
/// </summary>
public class ReviewServiceTests
{
    /// <summary>Creates an in-memory DbContext for testing.</summary>
    private static MovieAppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<MovieAppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new MovieAppDbContext(options);
    }

    /// <summary>
    /// Tests that AddReview creates a review successfully (happy path).
    /// </summary>
    [Fact]
    public async Task AddReview_HappyPath_CreatesReview()
    {
        // Arrange
        var context = CreateContext(nameof(AddReview_HappyPath_CreatesReview));
        context.Users.Add(new User { UserId = 1 });
        context.Movies.Add(new Movie { MovieId = 1, Title = "Test Movie", Genre = "Drama", AverageRating = 0 });
        await context.SaveChangesAsync();

        var mockPointService = new Mock<IPointService>();
        mockPointService.Setup(p => p.AddPoints(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        var service = new ReviewService(context, mockPointService.Object);

        // Act
        var review = await service.AddReview(1, 1, 4.5f, "Great movie!");

        // Assert
        Assert.NotNull(review);
        Assert.Equal(1, review.UserId);
        Assert.Equal(1, review.MovieId);
        Assert.Equal(4.5f, review.StarRating);
        Assert.Equal("Great movie!", review.Content);
        Assert.False(review.IsExtraReview);
    }

    /// <summary>
    /// Tests that AddReview throws when a user tries to review the same movie twice.
    /// </summary>
    [Fact]
    public async Task AddReview_DuplicateReview_ThrowsException()
    {
        // Arrange
        var context = CreateContext(nameof(AddReview_DuplicateReview_ThrowsException));
        context.Users.Add(new User { UserId = 1 });
        context.Movies.Add(new Movie { MovieId = 1, Title = "Test Movie", Genre = "Drama" });
        context.Reviews.Add(new Review { UserId = 1, MovieId = 1, StarRating = 4, Content = "First" });
        await context.SaveChangesAsync();

        var mockPointService = new Mock<IPointService>();
        var service = new ReviewService(context, mockPointService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddReview(1, 1, 3.0f, "Second review"));
    }

    /// <summary>
    /// Tests that AddReview throws for an invalid rating value.
    /// </summary>
    [Fact]
    public async Task AddReview_InvalidRating_ThrowsException()
    {
        // Arrange
        var context = CreateContext(nameof(AddReview_InvalidRating_ThrowsException));
        context.Users.Add(new User { UserId = 1 });
        context.Movies.Add(new Movie { MovieId = 1, Title = "Test Movie", Genre = "Drama" });
        await context.SaveChangesAsync();

        var mockPointService = new Mock<IPointService>();
        var service = new ReviewService(context, mockPointService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddReview(1, 1, 6.0f, "Invalid rating"));
    }

    /// <summary>
    /// Tests that the average rating is correctly updated after adding a review.
    /// </summary>
    [Fact]
    public async Task AddReview_UpdatesAverageRating()
    {
        // Arrange
        var context = CreateContext(nameof(AddReview_UpdatesAverageRating));
        context.Users.Add(new User { UserId = 1 });
        context.Users.Add(new User { UserId = 2 });
        context.Movies.Add(new Movie { MovieId = 1, Title = "Test Movie", Genre = "Drama", AverageRating = 0 });
        await context.SaveChangesAsync();

        var mockPointService = new Mock<IPointService>();
        mockPointService.Setup(p => p.AddPoints(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        var service = new ReviewService(context, mockPointService.Object);

        // Act
        await service.AddReview(1, 1, 4.0f, "Good");
        await service.AddReview(2, 1, 2.0f, "Average");

        // Assert
        var movie = await context.Movies.FindAsync(1);
        Assert.NotNull(movie);
        Assert.Equal(3.0, movie.AverageRating);
    }
}
