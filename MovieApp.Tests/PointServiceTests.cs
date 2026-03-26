#nullable enable
using Microsoft.EntityFrameworkCore;
using Moq;
using MovieApp.Core.Data;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Tests;

/// <summary>
/// Unit tests for the PointService class.
/// </summary>
public class PointServiceTests
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
    /// Tests that AddPoints awards +2 when movie average is above 3.5.
    /// </summary>
    [Fact]
    public async Task AddPoints_HighRating_AwardsTwoPoints()
    {
        // Arrange
        var context = CreateContext(nameof(AddPoints_HighRating_AwardsTwoPoints));
        context.Users.Add(new User { UserId = 1 });
        context.Movies.Add(new Movie { MovieId = 1, Title = "Good Movie", Genre = "Drama", AverageRating = 4.0 });
        context.UserStats.Add(new UserStats { UserId = 1, TotalPoints = 10 });
        await context.SaveChangesAsync();

        var mockBadgeService = new Mock<IBadgeService>();
        mockBadgeService.Setup(b => b.CheckAndAwardBadges(It.IsAny<int>())).Returns(Task.CompletedTask);
        var service = new PointService(context, mockBadgeService.Object);

        // Act
        await service.AddPoints(1, 1, false);

        // Assert
        var stats = await context.UserStats.FirstAsync(s => s.UserId == 1);
        Assert.Equal(12, stats.TotalPoints);
    }

    /// <summary>
    /// Tests that AddPoints awards +1 when movie average is below 2.0.
    /// </summary>
    [Fact]
    public async Task AddPoints_LowRating_AwardsOnePoint()
    {
        // Arrange
        var context = CreateContext(nameof(AddPoints_LowRating_AwardsOnePoint));
        context.Users.Add(new User { UserId = 1 });
        context.Movies.Add(new Movie { MovieId = 1, Title = "Bad Movie", Genre = "Drama", AverageRating = 1.5 });
        context.UserStats.Add(new UserStats { UserId = 1, TotalPoints = 10 });
        await context.SaveChangesAsync();

        var mockBadgeService = new Mock<IBadgeService>();
        mockBadgeService.Setup(b => b.CheckAndAwardBadges(It.IsAny<int>())).Returns(Task.CompletedTask);
        var service = new PointService(context, mockBadgeService.Object);

        // Act
        await service.AddPoints(1, 1, false);

        // Assert
        var stats = await context.UserStats.FirstAsync(s => s.UserId == 1);
        Assert.Equal(11, stats.TotalPoints);
    }

    /// <summary>
    /// Tests that AddPoints awards +5 bonus for battle movies.
    /// </summary>
    [Fact]
    public async Task AddPoints_BattleMovie_AwardsFiveBonus()
    {
        // Arrange
        var context = CreateContext(nameof(AddPoints_BattleMovie_AwardsFiveBonus));
        context.Users.Add(new User { UserId = 1 });
        context.Movies.Add(new Movie { MovieId = 1, Title = "Battle Movie", Genre = "Action", AverageRating = 4.0 });
        context.UserStats.Add(new UserStats { UserId = 1, TotalPoints = 10 });
        await context.SaveChangesAsync();

        var mockBadgeService = new Mock<IBadgeService>();
        mockBadgeService.Setup(b => b.CheckAndAwardBadges(It.IsAny<int>())).Returns(Task.CompletedTask);
        var service = new PointService(context, mockBadgeService.Object);

        // Act
        await service.AddPoints(1, 1, true);

        // Assert
        var stats = await context.UserStats.FirstAsync(s => s.UserId == 1);
        Assert.Equal(17, stats.TotalPoints); // +2 (high rating) + +5 (battle)
    }

    /// <summary>
    /// Tests that FreezePoints throws when user has insufficient points.
    /// </summary>
    [Fact]
    public async Task FreezePoints_InsufficientBalance_ThrowsException()
    {
        // Arrange
        var context = CreateContext(nameof(FreezePoints_InsufficientBalance_ThrowsException));
        context.Users.Add(new User { UserId = 1 });
        context.UserStats.Add(new UserStats { UserId = 1, TotalPoints = 5 });
        await context.SaveChangesAsync();

        var mockBadgeService = new Mock<IBadgeService>();
        var service = new PointService(context, mockBadgeService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.FreezePoints(1, 10));
    }
}
