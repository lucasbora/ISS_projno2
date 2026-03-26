#nullable enable
using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces;

/// <summary>
/// Service interface for battle/betting operations.
/// </summary>
public interface IBattleService
{
    /// <summary>Gets the currently active battle.</summary>
    Task<Battle?> GetActiveBattle();

    /// <summary>Creates a new battle between two movies.</summary>
    Task<Battle> CreateBattle(int firstMovieId, int secondMovieId);

    /// <summary>Places a bet on a battle.</summary>
    Task<Bet> PlaceBet(int userId, int battleId, int movieId, int amount);

    /// <summary>Gets a user's bet for a specific battle.</summary>
    Task<Bet?> GetBet(int userId, int battleId);

    /// <summary>Determines the winning movie of a battle.</summary>
    Task<int> DetermineWinner(int battleId);

    /// <summary>Distributes payouts to winning bettors.</summary>
    Task DistributePayouts(int battleId);
}
