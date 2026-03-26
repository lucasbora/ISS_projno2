#nullable enable
using Microsoft.EntityFrameworkCore;
using Moq;
using MovieApp.Core.Data;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Tests;

/// <summary>
/// Unit tests for the BattleService class.
/// </summary>
public class BattleServiceTests
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
    /// Tests that CreateBattle throws when rating difference exceeds 0.5.
    /// </summary>
    [Fact]
    public async Task CreateBattle_RatingDifferenceTooHigh_ThrowsException()
    {
        // Arrange
        var context = CreateContext(nameof(CreateBattle_RatingDifferenceTooHigh_ThrowsException));
        context.Movies.Add(new Movie { MovieId = 1, Title = "Movie A", Genre = "Action", AverageRating = 4.5 });
        context.Movies.Add(new Movie { MovieId = 2, Title = "Movie B", Genre = "Drama", AverageRating = 2.0 });
        await context.SaveChangesAsync();

        var mockPointService = new Mock<IPointService>();
        var service = new BattleService(context, mockPointService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateBattle(1, 2));
    }

    /// <summary>
    /// Tests that PlaceBet throws when user has already bet on the battle.
    /// </summary>
    [Fact]
    public async Task PlaceBet_DuplicateBet_ThrowsException()
    {
        // Arrange
        var context = CreateContext(nameof(PlaceBet_DuplicateBet_ThrowsException));
        context.Users.Add(new User { UserId = 1 });
        context.Movies.Add(new Movie { MovieId = 1, Title = "Movie A", Genre = "Action", AverageRating = 4.0 });
        context.Movies.Add(new Movie { MovieId = 2, Title = "Movie B", Genre = "Drama", AverageRating = 4.2 });
        context.Battles.Add(new Battle
        {
            BattleId = 1,
            FirstMovieId = 1,
            SecondMovieId = 2,
            InitialRatingFirstMovie = 4.0,
            InitialRatingSecondMovie = 4.2,
            Status = "Active"
        });
        context.Bets.Add(new Bet { UserId = 1, BattleId = 1, MovieId = 1, Amount = 5 });
        context.UserStats.Add(new UserStats { UserId = 1, TotalPoints = 50 });
        await context.SaveChangesAsync();

        var mockPointService = new Mock<IPointService>();
        mockPointService.Setup(p => p.FreezePoints(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var service = new BattleService(context, mockPointService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.PlaceBet(1, 1, 2, 10));
    }

    /// <summary>
    /// Tests that DetermineWinner returns the movie with highest rating improvement.
    /// </summary>
    [Fact]
    public async Task DetermineWinner_ReturnsCorrectMovieId()
    {
        // Arrange
        var context = CreateContext(nameof(DetermineWinner_ReturnsCorrectMovieId));
        context.Movies.Add(new Movie { MovieId = 1, Title = "Movie A", Genre = "Action", AverageRating = 4.5 });
        context.Movies.Add(new Movie { MovieId = 2, Title = "Movie B", Genre = "Drama", AverageRating = 4.0 });
        context.Battles.Add(new Battle
        {
            BattleId = 1,
            FirstMovieId = 1,
            SecondMovieId = 2,
            InitialRatingFirstMovie = 4.0,
            InitialRatingSecondMovie = 3.8,
            Status = "Active"
        });
        await context.SaveChangesAsync();

        var mockPointService = new Mock<IPointService>();
        var service = new BattleService(context, mockPointService.Object);

        // Act
        int winnerId = await service.DetermineWinner(1);

        // Assert
        // Movie A: 4.5 - 4.0 = 0.5 improvement
        // Movie B: 4.0 - 3.8 = 0.2 improvement
        Assert.Equal(1, winnerId);
    }
}
