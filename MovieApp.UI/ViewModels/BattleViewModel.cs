#nullable enable
using System.Collections.ObjectModel;
using System.Windows.Input;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;

namespace MovieApp.UI.ViewModels;

/// <summary>
/// ViewModel for the battle arena view showing active battles and betting.
/// </summary>
public class BattleViewModel : ViewModelBase
{
    private readonly IBattleService _battleService;
    private readonly IPointService _pointService;
    private readonly int _currentUserId;

    private Battle? _activeBattle;
    private bool _hasBattle;
    private bool _showBetForm;
    private int _betAmount;
    private int _selectedBetMovieId;
    private int _totalPoints;
    private string _statusMessage = string.Empty;
    private Bet? _userBet;
    private bool _hasBet;

    /// <summary>
    /// Initializes a new instance of <see cref="BattleViewModel"/>.
    /// </summary>
    public BattleViewModel(IBattleService battleService, IPointService pointService, int currentUserId = 1)
    {
        _battleService = battleService;
        _pointService = pointService;
        _currentUserId = currentUserId;

        LoadBattleCommand = new AsyncRelayCommand(async _ => await LoadBattleAsync());
        ShowBetFormCommand = new RelayCommand(_ => ShowBetForm = true);
        PlaceBetCommand = new AsyncRelayCommand(async _ => await PlaceBetAsync());
    }

    /// <summary>Gets the available movies to bet on.</summary>
    public ObservableCollection<Movie> BetMovieOptions { get; } = new();

    /// <summary>Gets or sets the active battle.</summary>
    public Battle? ActiveBattle
    {
        get => _activeBattle;
        set => SetProperty(ref _activeBattle, value);
    }

    /// <summary>Gets or sets whether there is an active battle.</summary>
    public bool HasBattle
    {
        get => _hasBattle;
        set => SetProperty(ref _hasBattle, value);
    }

    /// <summary>Gets or sets whether to show the bet form.</summary>
    public bool ShowBetForm
    {
        get => _showBetForm;
        set => SetProperty(ref _showBetForm, value);
    }

    /// <summary>Gets or sets the bet amount.</summary>
    public int BetAmount
    {
        get => _betAmount;
        set => SetProperty(ref _betAmount, value);
    }

    /// <summary>Gets or sets the movie ID the user is betting on.</summary>
    public int SelectedBetMovieId
    {
        get => _selectedBetMovieId;
        set => SetProperty(ref _selectedBetMovieId, value);
    }

    /// <summary>Gets or sets the user's total points.</summary>
    public int TotalPoints
    {
        get => _totalPoints;
        set => SetProperty(ref _totalPoints, value);
    }

    /// <summary>Gets or sets a status message.</summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>Gets or sets the user's existing bet.</summary>
    public Bet? UserBet
    {
        get => _userBet;
        set => SetProperty(ref _userBet, value);
    }

    /// <summary>Gets or sets whether the user has already placed a bet.</summary>
    public bool HasBet
    {
        get => _hasBet;
        set => SetProperty(ref _hasBet, value);
    }

    /// <summary>Gets the command to load the active battle.</summary>
    public ICommand LoadBattleCommand { get; }

    /// <summary>Gets the command to show the bet form.</summary>
    public ICommand ShowBetFormCommand { get; }

    /// <summary>Gets the command to place a bet.</summary>
    public ICommand PlaceBetCommand { get; }

    /// <summary>
    /// Loads the active battle and user's points.
    /// </summary>
    public async Task LoadBattleAsync()
    {
        StatusMessage = string.Empty;
        ShowBetForm = false;

        var stats = await _pointService.GetUserStats(_currentUserId);
        TotalPoints = stats.TotalPoints;

        ActiveBattle = await _battleService.GetActiveBattle();
        HasBattle = ActiveBattle != null;

        if (ActiveBattle != null)
        {
            BetMovieOptions.Clear();
            if (ActiveBattle.FirstMovie != null)
                BetMovieOptions.Add(ActiveBattle.FirstMovie);
            if (ActiveBattle.SecondMovie != null)
                BetMovieOptions.Add(ActiveBattle.SecondMovie);

            UserBet = await _battleService.GetBet(_currentUserId, ActiveBattle.BattleId);
            HasBet = UserBet != null;
        }
    }

    /// <summary>
    /// Places a bet on the active battle.
    /// </summary>
    private async Task PlaceBetAsync()
    {
        if (ActiveBattle == null || SelectedBetMovieId <= 0 || BetAmount <= 0)
        {
            StatusMessage = "Please select a movie and enter a valid bet amount.";
            return;
        }

        try
        {
            await _battleService.PlaceBet(_currentUserId, ActiveBattle.BattleId, SelectedBetMovieId, BetAmount);
            StatusMessage = $"Bet of {BetAmount} points placed successfully!";
            ShowBetForm = false;
            await LoadBattleAsync();
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = ex.Message;
        }
    }
}
