#nullable enable
using System.Collections.ObjectModel;
using System.Windows.Input;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;

namespace MovieApp.UI.ViewModels;

/// <summary>
/// ViewModel for the user profile view showing points and earned badges.
/// </summary>
public class ProfileViewModel : ViewModelBase
{
    private readonly IPointService _pointService;
    private readonly IBadgeService _badgeService;
    private readonly int _currentUserId;

    private int _totalPoints;
    private int _weeklyScore;
    private bool _hasBadges;

    /// <summary>
    /// Initializes a new instance of <see cref="ProfileViewModel"/>.
    /// </summary>
    public ProfileViewModel(IPointService pointService, IBadgeService badgeService, int currentUserId = 1)
    {
        _pointService = pointService;
        _badgeService = badgeService;
        _currentUserId = currentUserId;

        Badges = new ObservableCollection<Badge>();
        LoadProfileCommand = new AsyncRelayCommand(async _ => await LoadProfileAsync());
    }

    /// <summary>Gets or sets the user's total points.</summary>
    public int TotalPoints
    {
        get => _totalPoints;
        set => SetProperty(ref _totalPoints, value);
    }

    /// <summary>Gets or sets the user's weekly score.</summary>
    public int WeeklyScore
    {
        get => _weeklyScore;
        set => SetProperty(ref _weeklyScore, value);
    }

    /// <summary>Gets or sets whether the user has any badges.</summary>
    public bool HasBadges
    {
        get => _hasBadges;
        set => SetProperty(ref _hasBadges, value);
    }

    /// <summary>Gets the collection of badges earned by the user.</summary>
    public ObservableCollection<Badge> Badges { get; }

    /// <summary>Gets the command to load profile data.</summary>
    public ICommand LoadProfileCommand { get; }

    /// <summary>
    /// Loads the user's points and badges from the service layer.
    /// </summary>
    public async Task LoadProfileAsync()
    {
        var stats = await _pointService.GetUserStats(_currentUserId);
        TotalPoints = stats.TotalPoints;
        WeeklyScore = stats.WeeklyScore;

        var badges = await _badgeService.GetUserBadges(_currentUserId);
        Badges.Clear();
        foreach (var badge in badges)
            Badges.Add(badge);

        HasBadges = Badges.Count > 0;
    }
}
