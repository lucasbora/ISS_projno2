#nullable enable
using Microsoft.UI.Xaml.Controls;
using MovieApp.UI.ViewModels;

namespace MovieApp.UI.Views;

/// <summary>
/// Movie detail view showing full movie information, reviews, and comments.
/// </summary>
public sealed partial class MovieDetailView : UserControl
{
    /// <summary>Gets the ViewModel.</summary>
    public MovieDetailViewModel? ViewModel => DataContext as MovieDetailViewModel;

    /// <summary>
    /// Initializes a new instance of <see cref="MovieDetailView"/>.
    /// </summary>
    public MovieDetailView()
    {
        this.InitializeComponent();
        this.DataContextChanged += (s, e) => Bindings.Update();
    }
}
