namespace Label.Mcp.Regulatory.Models;

public sealed class RegulatorySearchRequest
{
    public required string Query { get; init; }

    public int Top { get; init; } = 5;

    public string? Filter { get; init; }

    public bool UseVectorSearch { get; init; } = true;

    public bool UseSemanticRanking { get; init; } = true;
}