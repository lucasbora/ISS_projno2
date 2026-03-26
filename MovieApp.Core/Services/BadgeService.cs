#nullable enable
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Data;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

/// <summary>
/// Service for badge/achievement management and awarding.
/// </summary>
public class BadgeService : IBadgeService
{
    private readonly MovieAppDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="BadgeService"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public BadgeService(MovieAppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all badges earned by a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of badges the user has earned.</returns>
    public async Task<List<Badge>> GetUserBadges(int userId)
    {
        return await _context.UserBadges
            .Where(ub => ub.UserId == userId)
            .Include(ub => ub.Badge)
            .Select(ub => ub.Badge!)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all available badges in the system.
    /// </summary>
    /// <returns>A list of all badges.</returns>
    public async Task<List<Badge>> GetAllBadges()
    {
        return await _context.Badges.ToListAsync();
    }

    /// <summary>
    /// Checks all badge criteria and awards any newly earned badges to the user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    public async Task CheckAndAwardBadges(int userId)
    {
        var existingBadgeIds = await _context.UserBadges
            .Where(ub => ub.UserId == userId)
            .Select(ub => ub.BadgeId)
            .ToListAsync();

        var allBadges = await _context.Badges.ToListAsync();

        var userReviews = await _context.Reviews
            .Include(r => r.Movie)
            .Where(r => r.UserId == userId)
            .ToListAsync();

        int totalReviews = userReviews.Count;
        int extraReviews = userReviews.Count(r => r.IsExtraReview);

        // Count reviews where all extra fields are completed
        int fullyCompletedExtraReviews = userReviews.Count(r =>
            r.IsExtraReview &&
            !string.IsNullOrEmpty(r.CinematographyText) &&
            !string.IsNullOrEmpty(r.ActingText) &&
            !string.IsNullOrEmpty(r.CgiText) &&
            !string.IsNullOrEmpty(r.PlotText) &&
            !string.IsNullOrEmpty(r.SoundText));

        // Count comedy genre reviews
        int comedyReviews = userReviews.Count(r =>
            r.Movie != null && r.Movie.Genre.Equals("Comedy", StringComparison.OrdinalIgnoreCase));
        double comedyPercentage = totalReviews > 0 ? (double)comedyReviews / totalReviews * 100 : 0;

        foreach (var badge in allBadges)
        {
            if (existingBadgeIds.Contains(badge.BadgeId))
                continue;

            bool shouldAward = badge.Name switch
            {
                "The Snob" => extraReviews >= 10,
                "The Super Serious" => fullyCompletedExtraReviews >= 50,
                "The Joker" => comedyPercentage > 70,
                "The Godfather I" => totalReviews >= 100,
                "The Godfather II" => totalReviews >= 200,
                "The Godfather III" => totalReviews >= 300,
                _ => false
            };

            if (shouldAward)
            {
                _context.UserBadges.Add(new UserBadge
                {
                    UserId = userId,
                    BadgeId = badge.BadgeId
                });
            }
        }

        await _context.SaveChangesAsync();
    }
}
