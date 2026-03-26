#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieApp.Core.Models;

/// <summary>
/// Represents a weekly battle between two movies.
/// </summary>
public class Battle
{
    /// <summary>Gets or sets the unique battle identifier.</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BattleId { get; set; }

    /// <summary>Gets or sets the first movie's ID.</summary>
    public int FirstMovieId { get; set; }

    /// <summary>Gets or sets the second movie's ID.</summary>
    public int SecondMovieId { get; set; }

    /// <summary>Gets or sets the first movie's rating at battle start.</summary>
    public double InitialRatingFirstMovie { get; set; }

    /// <summary>Gets or sets the second movie's rating at battle start.</summary>
    public double InitialRatingSecondMovie { get; set; }

    /// <summary>Gets or sets the battle start date (Monday).</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Gets or sets the battle end date (Sunday).</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Gets or sets the battle status (Active, Finished).</summary>
    [MaxLength(50)]
    public string Status { get; set; } = "Active";

    // Navigation properties
    /// <summary>Gets or sets the first movie.</summary>
    [ForeignKey(nameof(FirstMovieId))]
    public Movie? FirstMovie { get; set; }

    /// <summary>Gets or sets the second movie.</summary>
    [ForeignKey(nameof(SecondMovieId))]
    public Movie? SecondMovie { get; set; }

    /// <summary>Gets or sets the collection of bets on this battle.</summary>
    public ICollection<Bet> Bets { get; set; } = new List<Bet>();
}
