#nullable enable
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;

namespace MovieApp.Core.Data;

/// <summary>
/// Entity Framework Core database context for the MovieApp application.
/// </summary>
public class MovieAppDbContext : DbContext
{
    /// <summary>Gets or sets the movies table.</summary>
    public DbSet<Movie> Movies => Set<Movie>();

    /// <summary>Gets or sets the users table.</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>Gets or sets the reviews table.</summary>
    public DbSet<Review> Reviews => Set<Review>();

    /// <summary>Gets or sets the comments table.</summary>
    public DbSet<Comment> Comments => Set<Comment>();

    /// <summary>Gets or sets the battles table.</summary>
    public DbSet<Battle> Battles => Set<Battle>();

    /// <summary>Gets or sets the bets table.</summary>
    public DbSet<Bet> Bets => Set<Bet>();

    /// <summary>Gets or sets the user stats table.</summary>
    public DbSet<UserStats> UserStats => Set<UserStats>();

    /// <summary>Gets or sets the badges table.</summary>
    public DbSet<Badge> Badges => Set<Badge>();

    /// <summary>Gets or sets the user-badge junction table.</summary>
    public DbSet<UserBadge> UserBadges => Set<UserBadge>();

    /// <summary>
    /// Initializes a new instance of the <see cref="MovieAppDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public MovieAppDbContext(DbContextOptions<MovieAppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configures the entity model relationships and constraints.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Bet: composite PK (UserId, BattleId)
        modelBuilder.Entity<Bet>()
            .HasKey(b => new { b.UserId, b.BattleId });

        // UserBadge: composite PK (UserId, BadgeId)
        modelBuilder.Entity<UserBadge>()
            .HasKey(ub => new { ub.UserId, ub.BadgeId });

        // Movie relationships
        modelBuilder.Entity<Movie>()
            .HasMany(m => m.Reviews)
            .WithOne(r => r.Movie)
            .HasForeignKey(r => r.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Movie>()
            .HasMany(m => m.Comments)
            .WithOne(c => c.Movie)
            .HasForeignKey(c => c.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        // User relationships
        modelBuilder.Entity<User>()
            .HasMany(u => u.Reviews)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Comments)
            .WithOne(c => c.Author)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Bets)
            .WithOne(b => b.User)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<User>()
            .HasOne(u => u.UserStats)
            .WithOne(us => us.User)
            .HasForeignKey<UserStats>(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.UserBadges)
            .WithOne(ub => ub.User)
            .HasForeignKey(ub => ub.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Battle relationships
        modelBuilder.Entity<Battle>()
            .HasOne(b => b.FirstMovie)
            .WithMany()
            .HasForeignKey(b => b.FirstMovieId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Battle>()
            .HasOne(b => b.SecondMovie)
            .WithMany()
            .HasForeignKey(b => b.SecondMovieId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Battle>()
            .HasMany(b => b.Bets)
            .WithOne(bt => bt.Battle)
            .HasForeignKey(bt => bt.BattleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Bet → Movie
        modelBuilder.Entity<Bet>()
            .HasOne(b => b.Movie)
            .WithMany()
            .HasForeignKey(b => b.MovieId)
            .OnDelete(DeleteBehavior.NoAction);

        // Badge → UserBadge
        modelBuilder.Entity<Badge>()
            .HasMany(b => b.UserBadges)
            .WithOne(ub => ub.Badge)
            .HasForeignKey(ub => ub.BadgeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Comment self-referencing (threaded replies)
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.NoAction);

        // UserStats unique index on UserId
        modelBuilder.Entity<UserStats>()
            .HasIndex(us => us.UserId)
            .IsUnique();
    }
}
