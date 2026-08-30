using System.Text.Json;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Label.Mcp.Regulatory.Configuration;
using Label.Mcp.Regulatory.Models;
using Microsoft.Extensions.Options;

namespace Label.Mcp.Regulatory.Services;

public sealed class RegulatorySearchService : IRegulatorySearchService
{
    private readonly AzureSearchOptions _options;
    private readonly SearchClient _searchClient;
    private readonly SearchIndexClient _indexClient;
    private readonly ILogger<RegulatorySearchService> _logger;

    public RegulatorySearchService(
        IOptions<AzureSearchOptions> options,
        ILogger<RegulatorySearchService> logger)
    {
        _options = options.Value;
        _logger = logger;

        var endpoint = new Uri(_options.Endpoint);

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            var credential = new AzureKeyCredential(_options.ApiKey);

            _searchClient = new SearchClient(
                endpoint,
                _options.IndexName,
                credential);

            _indexClient = new SearchIndexClient(
                endpoint,
                credential);
        }
        else
        {
            TokenCredential credential = new DefaultAzureCredential();

            _searchClient = new SearchClient(
                endpoint,
                _options.IndexName,
                credential);

            _indexClient = new SearchIndexClient(
                endpoint,
                credential);
        }
    }

    public async Task<RegulatorySearchResponse> SearchAsync(
        RegulatorySearchRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            throw new ArgumentException(
                "A regulatory search query is required.",
                nameof(request));
        }

        var top = Math.Clamp(
            request.Top,
            1,
            _options.MaximumTop);

        var options = new SearchOptions
        {
            Size = top,
            IncludeTotalCount = true
        };

        if (!string.IsNullOrWhiteSpace(request.Filter))
        {
            options.Filter = request.Filter;
        }

        AddSelectField(options, _options.DocumentIdField);
        AddSelectField(options, _options.TitleField);
        AddSelectField(options, _options.ContentField);
        AddSelectField(options, _options.SourceUrlField);

        var vectorEnabled =
            request.UseVectorSearch &&
            !string.IsNullOrWhiteSpace(_options.VectorField);

        var semanticEnabled =
            request.UseSemanticRanking &&
            !string.IsNullOrWhiteSpace(_options.SemanticConfiguration);

        if (vectorEnabled)
        {
            options.VectorSearch = new VectorSearchOptions();

            var vectorQuery = new VectorizableTextQuery(request.Query)
            {
                KNearestNeighborsCount = Math.Max(
                    top,
                    _options.DefaultKNearestNeighbors)
            };

            vectorQuery.Fields.Add(_options.VectorField);

            options.VectorSearch.Queries.Add(vectorQuery);
        }

        if (semanticEnabled)
        {
            options.QueryType = SearchQueryType.Semantic;
            options.SemanticSearch = new SemanticSearchOptions
            {
                SemanticConfigurationName =
                    _options.SemanticConfiguration
            };
        }

        var mode = BuildSearchMode(
            vectorEnabled,
            semanticEnabled);

        _logger.LogInformation(
            "Searching index {IndexName}. Mode: {Mode}; Top: {Top}; Filter: {Filter}",
            _options.IndexName,
            mode,
            top,
            request.Filter ?? "(none)");

        var response = await _searchClient.SearchAsync<SearchDocument>(
            request.Query,
            options,
            cancellationToken);

        var results = new List<RegulatorySearchResult>();
        var rank = 0;

        await foreach (
            var result in response.Value.GetResultsAsync()
                .WithCancellation(cancellationToken))
        {
            rank++;

            results.Add(new RegulatorySearchResult
            {
                Rank = rank,
                Score = result.Score,
                RerankerScore = result.SemanticSearch?.RerankerScore,
                DocumentId = GetString(
                    result.Document,
                    _options.DocumentIdField),
                Title = GetString(
                    result.Document,
                    _options.TitleField),
                Content = GetString(
                    result.Document,
                    _options.ContentField),
                SourceUrl = GetString(
                    result.Document,
                    _options.SourceUrlField),
                Metadata = ToMetadata(result.Document)
            });
        }

        return new RegulatorySearchResponse
        {
            Query = request.Query,
            SearchMode = mode,
            ResultCount = results.Count,
            Results = results
        };
    }

    public async Task<string> GetIndexSchemaAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await _indexClient.GetIndexAsync(
            _options.IndexName,
            cancellationToken);

        var index = response.Value;

        var schema = new
        {
            index.Name,
            Fields = index.Fields.Select(field => new
            {
                field.Name,
                Type = field.Type.ToString(),
                field.IsKey,
                field.IsSearchable,
                field.IsFilterable,
                field.IsSortable,
                field.IsFacetable,
                field.IsHidden,
                field.VectorSearchDimensions,
                field.VectorSearchProfileName
            }),
            SemanticConfigurations =
                index.SemanticSearch?.Configurations.Select(
                    configuration => configuration.Name),
            VectorProfiles =
                index.VectorSearch?.Profiles.Select(
                    profile => profile.Name)
        };

        return JsonSerializer.Serialize(
            schema,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
    }

    private static void AddSelectField(
        SearchOptions options,
        string fieldName)
    {
        if (!string.IsNullOrWhiteSpace(fieldName) &&
            !options.Select.Contains(fieldName))
        {
            options.Select.Add(fieldName);
        }
    }

    private static string GetString(
        SearchDocument document,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return string.Empty;
        }

        if (!document.TryGetValue(fieldName, out var value) ||
            value is null)
        {
            return string.Empty;
        }

        return value switch
        {
            string text => text,
            JsonElement element when
                element.ValueKind == JsonValueKind.String
                    => element.GetString() ?? string.Empty,
            _ => value.ToString() ?? string.Empty
        };
    }

    private static IReadOnlyDictionary<string, object?> ToMetadata(
        SearchDocument document)
    {
        return new Dictionary<string, object?>(
            document.ToDictionary(
                entry => entry.Key,
                entry => (object?)entry.Value));
    }

    private static string BuildSearchMode(
        bool vectorEnabled,
        bool semanticEnabled)
    {
        if (vectorEnabled && semanticEnabled)
        {
            return "hybrid-vector-keyword-semantic";
        }

        if (vectorEnabled)
        {
            return "hybrid-vector-keyword";
        }

        if (semanticEnabled)
        {
            return "keyword-semantic";
        }

        return "keyword";
    }
}