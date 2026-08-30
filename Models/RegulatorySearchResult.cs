namespace Label.Mcp.Regulatory.Models;

public sealed class RegulatorySearchResult
{
    public int Rank { get; init; }

    public double? Score { get; init; }

    public double? RerankerScore { get; init; }

    public string DocumentId { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;

    public string SourceUrl { get; init; } = string.Empty;

    public IReadOnlyDictionary<string, object?> Metadata { get; init; }
        = new Dictionary<string, object?>();
}