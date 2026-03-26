#nullable enable
using System.Collections.ObjectModel;
using System.Windows.Input;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;

namespace MovieApp.UI.ViewModels;

/// <summary>
/// ViewModel for the movie catalog view with search and filter capabilities.
/// </summary>
public class CatalogViewModel : ViewModelBase
{
    private readonly ICatalogService _catalogService;
    private string _searchQuery = string.Empty;
    private string _selectedGenre = string.Empty;
    private double _minimumRating;
    private Movie? _selectedMovie;

    /// <summary>
    /// Event raised when a movie is selected for detail view.
    /// </summary>
    public event Action<Movie>? MovieSelected;

    /// <summary>
    /// Initializes a new instance of <see cref="CatalogViewModel"/>.
    /// </summary>
    /// <param name="catalogService">The catalog service.</param>
    public CatalogViewModel(ICatalogService catalogService)
    {
        _catalogService = catalogService;
        SearchCommand = new AsyncRelayCommand(async _ => await SearchAsync());
        FilterCommand = new AsyncRelayCommand(async _ => await FilterAsync());
        SelectMovieCommand = new RelayCommand(param =>
        {
            if (param is Movie movie)
            {
                SelectedMovie = movie;
                MovieSelected?.Invoke(movie);
            }
        });
        LoadMoviesCommand = new AsyncRelayCommand(async _ => await LoadMoviesAsync());
    }

    /// <summary>Gets the collection of movies to display.</summary>
    public ObservableCollection<Movie> Movies { get; } = new();

    /// <summary>Gets the list of available genres.</summary>
    public ObservableCollection<string> Genres { get; } = new()
    {
        "", "Action", "Comedy", "Crime", "Drama", "Sci-Fi"
    };

    /// <summary>Gets or sets the search query text.</summary>
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                _ = SearchAsync();
            }
        }
    }

    /// <summary>Gets or sets the selected genre filter.</summary>
    public string SelectedGenre
    {
        get => _selectedGenre;
        set
        {
            if (SetProperty(ref _selectedGenre, value))
            {
                _ = FilterAsync();
            }
        }
    }

    /// <summary>Gets or sets the minimum rating filter.</summary>
    public double MinimumRating
    {
        get => _minimumRating;
        set
        {
            if (SetProperty(ref _minimumRating, value))
            {
                _ = FilterAsync();
            }
        }
    }

    /// <summary>Gets or sets the currently selected movie.</summary>
    public Movie? SelectedMovie
    {
        get => _selectedMovie;
        set => SetProperty(ref _selectedMovie, value);
    }

    /// <summary>Gets the command to search movies.</summary>
    public ICommand SearchCommand { get; }

    /// <summary>Gets the command to filter movies.</summary>
    public ICommand FilterCommand { get; }

    /// <summary>Gets the command to select a movie.</summary>
    public ICommand SelectMovieCommand { get; }

    /// <summary>Gets the command to load all movies.</summary>
    public ICommand LoadMoviesCommand { get; }

    /// <summary>
    /// Loads all movies from the catalog.
    /// </summary>
    public async Task LoadMoviesAsync()
    {
        var movies = await _catalogService.GetAllMovies();
        Movies.Clear();
        foreach (var movie in movies)
            Movies.Add(movie);
    }

    /// <summary>
    /// Searches movies by the current search query.
    /// </summary>
    private async Task SearchAsync()
    {
        List<Movie> movies;
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            movies = await _catalogService.GetAllMovies();
        }
        else
        {
            movies = await _catalogService.SearchMovies(SearchQuery);
        }

        Movies.Clear();
        foreach (var movie in movies)
            Movies.Add(movie);
    }

    /// <summary>
    /// Filters movies by genre and minimum rating.
    /// </summary>
    private async Task FilterAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedGenre) && MinimumRating <= 0)
        {
            await LoadMoviesAsync();
            return;
        }

        var movies = await _catalogService.FilterMovies(SelectedGenre, (float)MinimumRating);
        Movies.Clear();
        foreach (var movie in movies)
            Movies.Add(movie);
    }
}
