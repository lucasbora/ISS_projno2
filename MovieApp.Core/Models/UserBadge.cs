#nullable enable
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieApp.Core.Models;

/// <summary>
/// Junction table for User-Badge many-to-many relationship. Composite PK: (UserId, BadgeId).
/// </summary>
public class UserBadge
{
    /// <summary>Gets or sets the user's ID (part of composite PK).</summary>
    public int UserId { get; set; }

    /// <summary>Gets or sets the badge's ID (part of composite PK).</summary>
    public int BadgeId { get; set; }

    // Navigation properties
    /// <summary>Gets or sets the associated user.</summary>
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    /// <summary>Gets or sets the associated badge.</summary>
    [ForeignKey(nameof(BadgeId))]
    public Badge? Badge { get; set; }
}
