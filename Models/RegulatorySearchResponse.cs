namespace Label.Mcp.Regulatory.Models;

public sealed class RegulatorySearchResponse
{
    public required string Query { get; init; }

    public required string SearchMode { get; init; }

    public int ResultCount { get; init; }

    public IReadOnlyList<RegulatorySearchResult> Results { get; init; }
        = [];
}