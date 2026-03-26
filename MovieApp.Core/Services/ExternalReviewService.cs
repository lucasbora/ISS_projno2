#nullable enable
using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

/// <summary>
/// Service for fetching external critic reviews from third-party sources.
/// Currently uses mock data; designed for easy swap to real API endpoints.
/// </summary>
public class ExternalReviewService
{
    private readonly HttpClient _httpClient;

    // Common stop words to filter out in lexicon analysis
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for",
        "of", "with", "by", "from", "is", "it", "this", "that", "was", "are",
        "be", "has", "have", "had", "not", "no", "as", "its", "so", "than",
        "into", "about", "out", "up", "what", "which", "who", "when", "where",
        "how", "all", "each", "every", "both", "few", "more", "most", "other",
        "some", "such", "only", "own", "same", "very", "can", "will", "just",
        "do", "does", "did", "been", "being", "would", "could", "should",
        "may", "might", "must", "shall", "we", "they", "he", "she", "you",
        "i", "me", "my", "your", "his", "her", "our", "their"
    };

    /// <summary>
    /// Initializes a new instance of <see cref="ExternalReviewService"/>.
    /// </summary>
    /// <param name="httpClient">The HTTP client (injected as singleton).</param>
    public ExternalReviewService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets external critic reviews for a movie (mock data).
    /// Structured for easy replacement with real API calls.
    /// </summary>
    /// <param name="movieTitle">The movie title to search for.</param>
    /// <returns>A list of critic reviews from various sources.</returns>
    public async Task<List<CriticReview>> GetExternalReviews(string movieTitle)
    {
        // Simulate API latency
        await Task.Delay(500);

        // Mock data structured to match real API response formats
        return new List<CriticReview>
        {
            new CriticReview
            {
                Source = "New York Times",
                Score = 4.2,
                Headline = $"'{movieTitle}' — A Cinematic Tour de Force",
                Snippet = $"'{movieTitle}' delivers an extraordinary experience that captivates from the opening frame to the final credits. The director's vision is both bold and nuanced.",
                Url = $"https://www.nytimes.com/reviews/{movieTitle.Replace(" ", "-").ToLower()}"
            },
            new CriticReview
            {
                Source = "The Guardian",
                Score = 3.8,
                Headline = $"{movieTitle} Review — Ambitious and Thought-Provoking",
                Snippet = $"While '{movieTitle}' occasionally stumbles in its ambition, the performances are uniformly excellent and the cinematography is breathtaking.",
                Url = $"https://www.theguardian.com/film/{movieTitle.Replace(" ", "-").ToLower()}-review"
            },
            new CriticReview
            {
                Source = "OMDb / Metacritic",
                Score = 4.0,
                Headline = $"{movieTitle} — Critical Consensus",
                Snippet = $"Critics agree: '{movieTitle}' is a masterfully crafted film that rewards patient viewers with a deeply satisfying narrative arc.",
                Url = $"https://www.omdbapi.com/?t={Uri.EscapeDataString(movieTitle)}"
            }
        };
    }

    /// <summary>
    /// Gets aggregate critic and audience scores for a movie (mock data).
    /// </summary>
    /// <param name="movieTitle">The movie title.</param>
    /// <returns>A tuple with CriticScore and AudienceScore.</returns>
    public async Task<(double CriticScore, double AudienceScore)> GetAggregateScores(string movieTitle)
    {
        await Task.Delay(200);

        // Mock aggregate scores
        var hash = Math.Abs(movieTitle.GetHashCode());
        double criticScore = 3.0 + (hash % 20) / 10.0;
        double audienceScore = 2.5 + (hash % 25) / 10.0;

        criticScore = Math.Min(5.0, Math.Round(criticScore, 1));
        audienceScore = Math.Min(5.0, Math.Round(audienceScore, 1));

        return (criticScore, audienceScore);
    }

    /// <summary>
    /// Analyzes the lexicon of critic reviews, returning top 10 non-stop-words.
    /// </summary>
    /// <param name="reviews">The list of critic reviews to analyze.</param>
    /// <returns>Top 10 words by frequency.</returns>
    public List<(string Word, int Count)> AnalyseLexicon(List<CriticReview> reviews)
    {
        var wordCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var review in reviews)
        {
            var words = (review.Snippet + " " + review.Headline)
                .Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '"', '\'', '(', ')', '-', '—', '\n', '\r', '\t' },
                    StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                var cleanWord = word.Trim().ToLower();
                if (cleanWord.Length < 3 || StopWords.Contains(cleanWord))
                    continue;

                if (wordCounts.ContainsKey(cleanWord))
                    wordCounts[cleanWord]++;
                else
                    wordCounts[cleanWord] = 1;
            }
        }

        return wordCounts
            .OrderByDescending(kv => kv.Value)
            .Take(10)
            .Select(kv => (kv.Key, kv.Value))
            .ToList();
    }

    /// <summary>
    /// Determines whether the critic and audience scores are polarized.
    /// </summary>
    /// <param name="criticScore">The critic score.</param>
    /// <param name="audienceScore">The audience score.</param>
    /// <param name="threshold">The threshold for polarization (default 2.0).</param>
    /// <returns>True if scores differ by more than the threshold.</returns>
    public bool IsPolarized(double criticScore, double audienceScore, double threshold = 2.0)
    {
        return Math.Abs(criticScore - audienceScore) > threshold;
    }
}
