#nullable enable
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Data;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Tests;

/// <summary>
/// Unit tests for the BadgeService class.
/// </summary>
public class BadgeServiceTests
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
    /// Tests that CheckAndAwardBadges awards "The Godfather I" when user has 100+ reviews.
    /// </summary>
    [Fact]
    public async Task CheckAndAwardBadges_AwardsGodfatherI_When100Reviews()
    {
        // Arrange
        var context = CreateContext(nameof(CheckAndAwardBadges_AwardsGodfatherI_When100Reviews));
        context.Users.Add(new User { UserId = 1 });
        context.Badges.Add(new Badge { BadgeId = 1, Name = "The Godfather I", CriteriaValue = 100 });

        // Add 100 movies and reviews
        for (int i = 1; i <= 100; i++)
        {
            context.Movies.Add(new Movie { MovieId = i, Title = $"Movie {i}", Genre = "Drama" });
            context.Reviews.Add(new Review
            {
                ReviewId = i,
                UserId = 1,
                MovieId = i,
                StarRating = 4.0f,
                Content = $"Review for Movie {i}"
            });
        }
        await context.SaveChangesAsync();

        var service = new BadgeService(context);

        // Act
        await service.CheckAndAwardBadges(1);

        // Assert
        var userBadges = await context.UserBadges
            .Where(ub => ub.UserId == 1)
            .ToListAsync();
        Assert.Single(userBadges);
        Assert.Equal(1, userBadges[0].BadgeId);
    }

    /// <summary>
    /// Tests that CheckAndAwardBadges does not award duplicate badges.
    /// </summary>
    [Fact]
    public async Task CheckAndAwardBadges_DoesNotAwardDuplicate()
    {
        // Arrange
        var context = CreateContext(nameof(CheckAndAwardBadges_DoesNotAwardDuplicate));
        context.Users.Add(new User { UserId = 1 });
        context.Badges.Add(new Badge { BadgeId = 1, Name = "The Godfather I", CriteriaValue = 100 });

        // Add user badge (already earned)
        context.UserBadges.Add(new UserBadge { UserId = 1, BadgeId = 1 });

        // Add 100 movies and reviews to meet criteria
        for (int i = 1; i <= 100; i++)
        {
            context.Movies.Add(new Movie { MovieId = i, Title = $"Movie {i}", Genre = "Drama" });
            context.Reviews.Add(new Review
            {
                ReviewId = i,
                UserId = 1,
                MovieId = i,
                StarRating = 4.0f,
                Content = $"Review {i}"
            });
        }
        await context.SaveChangesAsync();

        var service = new BadgeService(context);

        // Act
        await service.CheckAndAwardBadges(1);

        // Assert — should still only have one badge
        var userBadges = await context.UserBadges
            .Where(ub => ub.UserId == 1)
            .ToListAsync();
        Assert.Single(userBadges);
    }

    /// <summary>
    /// Tests that CheckAndAwardBadges awards "The Snob" when user has 10+ extra reviews.
    /// </summary>
    [Fact]
    public async Task CheckAndAwardBadges_AwardsSnob_When10ExtraReviews()
    {
        // Arrange
        var context = CreateContext(nameof(CheckAndAwardBadges_AwardsSnob_When10ExtraReviews));
        context.Users.Add(new User { UserId = 1 });
        context.Badges.Add(new Badge { BadgeId = 1, Name = "The Snob", CriteriaValue = 10 });

        for (int i = 1; i <= 10; i++)
        {
            context.Movies.Add(new Movie { MovieId = i, Title = $"Movie {i}", Genre = "Drama" });
            context.Reviews.Add(new Review
            {
                ReviewId = i,
                UserId = 1,
                MovieId = i,
                StarRating = 4.0f,
                Content = $"Review {i}",
                IsExtraReview = true
            });
        }
        await context.SaveChangesAsync();

        var service = new BadgeService(context);

        // Act
        await service.CheckAndAwardBadges(1);

        // Assert
        var userBadges = await context.UserBadges
            .Where(ub => ub.UserId == 1)
            .ToListAsync();
        Assert.Single(userBadges);
        Assert.Equal(1, userBadges[0].BadgeId);
    }
}
