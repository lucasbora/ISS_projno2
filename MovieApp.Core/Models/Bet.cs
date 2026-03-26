#nullable enable
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieApp.Core.Models;

/// <summary>
/// Represents a user's bet on a movie battle. Composite PK: (UserId, BattleId).
/// </summary>
public class Bet
{
    /// <summary>Gets or sets the betting user's ID (part of composite PK).</summary>
    public int UserId { get; set; }

    /// <summary>Gets or sets the battle ID (part of composite PK).</summary>
    public int BattleId { get; set; }

    /// <summary>Gets or sets the movie the user is betting on.</summary>
    public int MovieId { get; set; }

    /// <summary>Gets or sets the amount of points bet.</summary>
    public int Amount { get; set; }

    // Navigation properties
    /// <summary>Gets or sets the betting user.</summary>
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    /// <summary>Gets or sets the battle.</summary>
    [ForeignKey(nameof(BattleId))]
    public Battle? Battle { get; set; }

    /// <summary>Gets or sets the movie being bet on.</summary>
    [ForeignKey(nameof(MovieId))]
    public Movie? Movie { get; set; }
}
