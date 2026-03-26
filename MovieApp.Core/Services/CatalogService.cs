#nullable enable
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Data;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

/// <summary>
/// Service for movie catalog operations including search and filtering.
/// </summary>
public class CatalogService : ICatalogService
{
    private readonly MovieAppDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="CatalogService"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CatalogService(MovieAppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all movies in the catalog.
    /// </summary>
    /// <returns>A list of all movies.</returns>
    public async Task<List<Movie>> GetAllMovies()
    {
        return await _context.Movies
            .OrderBy(m => m.Title)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a single movie by its ID.
    /// </summary>
    /// <param name="movieId">The movie identifier.</param>
    /// <returns>The movie if found.</returns>
    /// <exception cref="InvalidOperationException">Thrown when movie is not found.</exception>
    public async Task<Movie> GetMovieById(int movieId)
    {
        var movie = await _context.Movies.FindAsync(movieId);
        return movie ?? throw new InvalidOperationException($"Movie with ID {movieId} not found.");
    }

    /// <summary>
    /// Searches movies by title containing the query (case-insensitive).
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <returns>A list of matching movies.</returns>
    public async Task<List<Movie>> SearchMovies(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetAllMovies();

        return await _context.Movies
            .Where(m => m.Title.ToLower().Contains(query.ToLower()))
            .OrderBy(m => m.Title)
            .ToListAsync();
    }

    /// <summary>
    /// Filters movies by genre and minimum average rating.
    /// </summary>
    /// <param name="genre">The genre to filter by.</param>
    /// <param name="minRating">The minimum average rating.</param>
    /// <returns>A list of filtered movies.</returns>
    public async Task<List<Movie>> FilterMovies(string genre, float minRating)
    {
        var query = _context.Movies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(genre))
        {
            query = query.Where(m => m.Genre.ToLower() == genre.ToLower());
        }

        query = query.Where(m => m.AverageRating >= minRating);

        return await query
            .OrderBy(m => m.Title)
            .ToListAsync();
    }
}
