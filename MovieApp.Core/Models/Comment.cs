#nullable enable

namespace MovieApp.Core.Models;

/// <summary>
/// Represents a comment in the movie discussion forum.
/// Supports threaded replies via ParentCommentId.
/// </summary>
public class Comment
{
    /// <summary>Gets or sets the unique message identifier.</summary>
    public int MessageId { get; set; }

    /// <summary>Gets or sets the comment content (max 10000 characters).</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    /// <summary>Gets or sets the author user.</summary>
    public User? Author { get; set; }

    /// <summary>Gets or sets the associated movie.</summary>
    public Movie? Movie { get; set; }

    /// <summary>Gets or sets the parent comment (for replies).</summary>
    public Comment? ParentComment { get; set; }

    /// <summary>Gets or sets the collection of replies to this comment.</summary>
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();

    public int AuthorDisplayId => Author?.UserId ?? 0;

    public int ParentCommentDisplayId => ParentComment?.MessageId ?? 0;

    public bool HasParentComment => ParentComment is not null;
}
