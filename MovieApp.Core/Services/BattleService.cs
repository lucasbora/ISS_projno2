#nullable enable
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Data;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

/// <summary>
/// Service for battle management and betting operations.
/// </summary>
public class BattleService : IBattleService
{
    private readonly MovieAppDbContext _context;
    private readonly IPointService _pointService;

    /// <summary>
    /// Initializes a new instance of <see cref="BattleService"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="pointService">The point service for bet handling.</param>
    public BattleService(MovieAppDbContext context, IPointService pointService)
    {
        _context = context;
        _pointService = pointService;
    }

    /// <summary>
    /// Gets the currently active battle, if any.
    /// </summary>
    /// <returns>The active battle or null.</returns>
    public async Task<Battle?> GetActiveBattle()
    {
        return await _context.Battles
            .Include(b => b.FirstMovie)
            .Include(b => b.SecondMovie)
            .FirstOrDefaultAsync(b => b.Status == "Active");
    }

    /// <summary>
    /// Creates a new battle between two movies.
    /// Validates rating difference and ensures no other active battle exists.
    /// </summary>
    /// <param name="firstMovieId">The first movie's ID.</param>
    /// <param name="secondMovieId">The second movie's ID.</param>
    /// <returns>The created battle.</returns>
    /// <exception cref="InvalidOperationException">Thrown on validation failure.</exception>
    public async Task<Battle> CreateBattle(int firstMovieId, int secondMovieId)
    {
        // Check no active battle exists
        var activeBattle = await _context.Battles
            .AnyAsync(b => b.Status == "Active");
        if (activeBattle)
            throw new InvalidOperationException("An active battle already exists.");

        var firstMovie = await _context.Movies.FindAsync(firstMovieId)
            ?? throw new InvalidOperationException("First movie not found.");
        var secondMovie = await _context.Movies.FindAsync(secondMovieId)
            ?? throw new InvalidOperationException("Second movie not found.");

        // Validate rating difference
        if (Math.Abs(firstMovie.AverageRating - secondMovie.AverageRating) > 0.5)
            throw new InvalidOperationException(
                "Rating difference between movies must be 0.5 or less.");

        // Calculate start (Monday) and end (Sunday)
        var today = DateTime.UtcNow.Date;
        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0) daysUntilMonday = 0; // If today is Monday, start today
        var startDate = today.AddDays(daysUntilMonday);
        var endDate = startDate.AddDays(6);

        var battle = new Battle
        {
            FirstMovieId = firstMovieId,
            SecondMovieId = secondMovieId,
            InitialRatingFirstMovie = firstMovie.AverageRating,
            InitialRatingSecondMovie = secondMovie.AverageRating,
            StartDate = startDate,
            EndDate = endDate,
            Status = "Active"
        };

        _context.Battles.Add(battle);
        await _context.SaveChangesAsync();

        return battle;
    }

    /// <summary>
    /// Places a bet on a battle.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="battleId">The battle's ID.</param>
    /// <param name="movieId">The movie to bet on.</param>
    /// <param name="amount">Points to bet (must be > 0).</param>
    /// <returns>The created bet.</returns>
    /// <exception cref="InvalidOperationException">Thrown if user already bet or invalid amount.</exception>
    public async Task<Bet> PlaceBet(int userId, int battleId, int movieId, int amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Bet amount must be greater than 0.");

        // Check if user already bet on this battle
        var existingBet = await _context.Bets
            .AnyAsync(b => b.UserId == userId && b.BattleId == battleId);
        if (existingBet)
            throw new InvalidOperationException("User has already placed a bet on this battle.");

        // Freeze the points
        await _pointService.FreezePoints(userId, amount);

        var bet = new Bet
        {
            UserId = userId,
            BattleId = battleId,
            MovieId = movieId,
            Amount = amount
        };

        _context.Bets.Add(bet);
        await _context.SaveChangesAsync();

        return bet;
    }

    /// <summary>
    /// Gets a user's bet for a specific battle.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="battleId">The battle's ID.</param>
    /// <returns>The user's bet or null.</returns>
    public async Task<Bet?> GetBet(int userId, int battleId)
    {
        return await _context.Bets
            .Include(b => b.Movie)
            .FirstOrDefaultAsync(b => b.UserId == userId && b.BattleId == battleId);
    }

    /// <summary>
    /// Determines the winner of a battle based on rating improvement.
    /// </summary>
    /// <param name="battleId">The battle identifier.</param>
    /// <returns>The winning movie's ID.</returns>
    public async Task<int> DetermineWinner(int battleId)
    {
        var battle = await _context.Battles
            .Include(b => b.FirstMovie)
            .Include(b => b.SecondMovie)
            .FirstOrDefaultAsync(b => b.BattleId == battleId)
            ?? throw new InvalidOperationException("Battle not found.");

        double firstImprovement = (battle.FirstMovie?.AverageRating ?? 0) - battle.InitialRatingFirstMovie;
        double secondImprovement = (battle.SecondMovie?.AverageRating ?? 0) - battle.InitialRatingSecondMovie;

        return firstImprovement >= secondImprovement ? battle.FirstMovieId : battle.SecondMovieId;
    }

    /// <summary>
    /// Distributes payouts to winning bettors (Amount * 2).
    /// </summary>
    /// <param name="battleId">The battle identifier.</param>
    public async Task DistributePayouts(int battleId)
    {
        int winningMovieId = await DetermineWinner(battleId);

        var bets = await _context.Bets
            .Where(b => b.BattleId == battleId)
            .ToListAsync();

        foreach (var bet in bets)
        {
            if (bet.MovieId == winningMovieId)
            {
                // Winner gets Amount * 2
                await _pointService.RefundPoints(bet.UserId, bet.Amount * 2);
            }
            // Losers lose their frozen points (already deducted)
        }

        var battle = await _context.Battles.FindAsync(battleId);
        if (battle != null)
        {
            battle.Status = "Finished";
            await _context.SaveChangesAsync();
        }
    }
}
