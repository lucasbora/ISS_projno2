#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieApp.Core.Models;

/// <summary>
/// Represents a comment in the movie discussion forum.
/// Supports threaded replies via ParentCommentId.
/// </summary>
public class Comment
{
    /// <summary>Gets or sets the unique message identifier.</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MessageId { get; set; }

    /// <summary>Gets or sets the author's user ID.</summary>
    public int AuthorId { get; set; }

    /// <summary>Gets or sets the movie this comment belongs to.</summary>
    public int MovieId { get; set; }

    /// <summary>Gets or sets the parent comment ID for threaded replies. Null for root comments.</summary>
    public int? ParentCommentId { get; set; }

    /// <summary>Gets or sets the comment content (max 10000 characters).</summary>
    [MaxLength(10000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    /// <summary>Gets or sets the author user.</summary>
    [ForeignKey(nameof(AuthorId))]
    public User? Author { get; set; }

    /// <summary>Gets or sets the associated movie.</summary>
    [ForeignKey(nameof(MovieId))]
    public Movie? Movie { get; set; }

    /// <summary>Gets or sets the parent comment (for replies).</summary>
    [ForeignKey(nameof(ParentCommentId))]
    public Comment? ParentComment { get; set; }

    /// <summary>Gets or sets the collection of replies to this comment.</summary>
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
