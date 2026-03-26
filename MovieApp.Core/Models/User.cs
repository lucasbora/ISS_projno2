#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieApp.Core.Models;

/// <summary>
/// Represents a registered user of the application.
/// </summary>
public class User
{
    /// <summary>Gets or sets the unique user identifier.</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    // Navigation properties
    /// <summary>Gets or sets the collection of reviews written by this user.</summary>
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    /// <summary>Gets or sets the collection of comments by this user.</summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>Gets or sets the collection of bets placed by this user.</summary>
    public ICollection<Bet> Bets { get; set; } = new List<Bet>();

    /// <summary>Gets or sets the user's stats.</summary>
    public UserStats? UserStats { get; set; }

    /// <summary>Gets or sets the collection of badges earned by this user.</summary>
    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
}
