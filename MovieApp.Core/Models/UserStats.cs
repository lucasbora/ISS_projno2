#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieApp.Core.Models;

/// <summary>
/// Represents a user's point statistics and rankings.
/// </summary>
public class UserStats
{
    /// <summary>Gets or sets the unique stats identifier.</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int StatsId { get; set; }

    /// <summary>Gets or sets the associated user's ID.</summary>
    public int UserId { get; set; }

    /// <summary>Gets or sets the user's total accumulated points.</summary>
    public int TotalPoints { get; set; }

    /// <summary>Gets or sets the user's weekly score.</summary>
    public int WeeklyScore { get; set; }

    // Navigation properties
    /// <summary>Gets or sets the associated user.</summary>
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
