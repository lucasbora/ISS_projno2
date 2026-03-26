#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MovieApp.Core.Data;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Services;
using MovieApp.UI.ViewModels;

namespace MovieApp.UI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// Configures dependency injection for all services and ViewModels.
/// </summary>
public partial class App : Application
{
    private Window? _window;

    /// <summary>Gets the service provider for dependency injection.</summary>
    public static IServiceProvider Services { get; private set; } = null!;

    /// <summary>
    /// Initializes the singleton application object.
    /// </summary>
    public App()
    {
        this.InitializeComponent();

        // Configure services
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Ensure database is created and seeded
        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MovieAppDbContext>();
            await DatabaseSeeder.SeedAsync(context);
        }

        _window = new MainWindow();
        _window.Activate();
    }

    /// <summary>
    /// Configures all services for dependency injection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    private static void ConfigureServices(IServiceCollection services)
    {
        // Database context — InMemory for demo (no SQL Server needed)
        services.AddDbContext<MovieAppDbContext>(options =>
            options.UseInMemoryDatabase("MovieAppDb"));

        // Core services
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IPointService, PointService>();
        services.AddScoped<IBadgeService, BadgeService>();
        services.AddScoped<IBattleService, BattleService>();
        services.AddScoped<ICommentService, CommentService>();

        // External review service (HttpClient singleton)
        services.AddHttpClient<ExternalReviewService>();

        // ViewModels
        services.AddTransient<CatalogViewModel>();
        services.AddTransient<MovieDetailViewModel>();
        services.AddTransient<BattleViewModel>();
        services.AddTransient<ForumViewModel>();
        services.AddTransient<MainWindowViewModel>();
    }
}
