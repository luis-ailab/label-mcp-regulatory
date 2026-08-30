using System.ComponentModel;
using System.Text.Json;
using Label.Mcp.Regulatory.Models;
using Label.Mcp.Regulatory.Services;
using ModelContextProtocol.Server;

namespace Label.Mcp.Regulatory.Tools;

[McpServerToolType]
public sealed class RegulatorySearchTools
{
    private readonly IRegulatorySearchService _searchService;
    private readonly ILogger<RegulatorySearchTools> _logger;

    public RegulatorySearchTools(
        IRegulatorySearchService searchService,
        ILogger<RegulatorySearchTools> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    [McpServerTool(
        Name = "search_regulations",
        Title = "Search regulatory documents",
        ReadOnly = true,
        Idempotent = true)]
    [Description(
        "Searches the approved regulatory document index. " +
        "Use this tool when answering questions about product labels, " +
        "claims, warnings, required statements, ingredients, formatting, " +
        "FDA regulations, FTC guidance, or internal regulatory policies. " +
        "Returns grounded passages with document titles and source URLs.")]
    public async Task<string> SearchRegulationsAsync(
        [Description(
            "The complete regulatory question or search query.")]
        string query,

        [Description(
            "Maximum number of passages to return. Use 5 unless more context is required.")]
        int top = 5,

        [Description(
            "Optional Azure AI Search OData filter. Use only fields configured as filterable.")]
        string? filter = null,

        [Description(
            "Whether to use vector retrieval in addition to keyword retrieval.")]
        bool useVectorSearch = true,

        [Description(
            "Whether to apply the configured semantic ranker.")]
        bool useSemanticRanking = true,

        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "MCP tool search_regulations called for query: {Query}",
            query);

        try
        {
            var response = await _searchService.SearchAsync(
                new RegulatorySearchRequest
                {
                    Query = query,
                    Top = top,
                    Filter = filter,
                    UseVectorSearch = useVectorSearch,
                    UseSemanticRanking = useSemanticRanking
                },
                cancellationToken);

            return JsonSerializer.Serialize(
                response,
                JsonOptions);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Regulatory search failed.");

            return JsonSerializer.Serialize(
                new
                {
                    success = false,
                    error = exception.Message,
                    guidance =
                        "Verify the Azure AI Search endpoint, index name, " +
                        "field mappings, semantic configuration, vector field, " +
                        "and Search Index Data Reader permissions."
                },
                JsonOptions);
        }
    }

    [McpServerTool(
        Name = "get_regulatory_index_schema",
        Title = "Get regulatory index schema",
        ReadOnly = true,
        Idempotent = true)]
    [Description(
        "Returns the Azure AI Search field definitions, semantic configurations, " +
        "and vector profiles for the regulatory index. Use this tool for diagnostics " +
        "and configuration validation.")]
    public async Task<string> GetRegulatoryIndexSchemaAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _searchService.GetIndexSchemaAsync(
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Could not retrieve the regulatory index schema.");

            return JsonSerializer.Serialize(
                new
                {
                    success = false,
                    error = exception.Message
                },
                JsonOptions);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };
}