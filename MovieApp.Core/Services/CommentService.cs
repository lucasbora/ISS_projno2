#nullable enable
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Data;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

/// <summary>
/// Service for comment/forum operations including threaded replies.
/// </summary>
public class CommentService : ICommentService
{
    private readonly MovieAppDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="CommentService"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CommentService(MovieAppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all comments for a movie, ordered by creation date descending.
    /// Returns a flat list (UI builds the tree).
    /// </summary>
    /// <param name="movieId">The movie identifier.</param>
    /// <returns>A flat list of comments.</returns>
    public async Task<List<Comment>> GetCommentsForMovie(int movieId)
    {
        return await _context.Comments
            .Include(c => c.Author)
            .Where(c => c.MovieId == movieId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Adds a root-level comment on a movie.
    /// </summary>
    /// <param name="userId">The comment author's ID.</param>
    /// <param name="movieId">The movie's ID.</param>
    /// <param name="content">The comment content (max 10000 chars).</param>
    /// <returns>The created comment.</returns>
    /// <exception cref="InvalidOperationException">Thrown when content exceeds max length.</exception>
    public async Task<Comment> AddComment(int userId, int movieId, string content)
    {
        if (!string.IsNullOrEmpty(content) && content.Length > 10000)
            throw new InvalidOperationException("Comment content must not exceed 10000 characters.");

        var comment = new Comment
        {
            AuthorId = userId,
            MovieId = movieId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            ParentCommentId = null
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return comment;
    }

    /// <summary>
    /// Adds a reply to an existing comment. Inherits MovieId from parent.
    /// </summary>
    /// <param name="userId">The reply author's ID.</param>
    /// <param name="parentCommentId">The parent comment's ID.</param>
    /// <param name="content">The reply content (max 10000 chars).</param>
    /// <returns>The created reply comment.</returns>
    /// <exception cref="InvalidOperationException">Thrown when parent not found or content invalid.</exception>
    public async Task<Comment> AddReply(int userId, int parentCommentId, string content)
    {
        var parentComment = await _context.Comments.FindAsync(parentCommentId)
            ?? throw new InvalidOperationException("Parent comment not found.");

        if (!string.IsNullOrEmpty(content) && content.Length > 10000)
            throw new InvalidOperationException("Comment content must not exceed 10000 characters.");

        var reply = new Comment
        {
            AuthorId = userId,
            MovieId = parentComment.MovieId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            ParentCommentId = parentCommentId
        };

        _context.Comments.Add(reply);
        await _context.SaveChangesAsync();

        return reply;
    }

    /// <summary>
    /// Deletes a comment by its ID.
    /// </summary>
    /// <param name="commentId">The comment identifier.</param>
    public async Task DeleteComment(int commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId)
            ?? throw new InvalidOperationException("Comment not found.");

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
    }
}
