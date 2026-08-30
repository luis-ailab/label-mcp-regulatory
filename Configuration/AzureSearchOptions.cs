namespace Label.Mcp.Regulatory.Configuration;

public sealed class AzureSearchOptions
{
    public const string SectionName = "AzureSearch";

    public string Endpoint { get; set; } = string.Empty;

    public string IndexName { get; set; } = string.Empty;

    // Leave empty when using managed identity or Azure CLI authentication.
    public string ApiKey { get; set; } = string.Empty;

    // Searchable text field containing each regulation chunk.
    public string ContentField { get; set; } = "content";

    // Optional title or document-name field.
    public string TitleField { get; set; } = "title";

    // Optional SharePoint or source-document URL field.
    public string SourceUrlField { get; set; } = "url";

    // Optional document identifier field.
    public string DocumentIdField { get; set; } = "id";

    // Vector field created by your Azure AI Search ingestion process.
    // Leave empty to disable vector search.
    public string VectorField { get; set; } = string.Empty;

    // Semantic configuration defined on the index.
    // Leave empty to disable semantic ranking.
    public string SemanticConfiguration { get; set; } = string.Empty;

    public int DefaultTop { get; set; } = 5;

    public int MaximumTop { get; set; } = 20;

    public int DefaultKNearestNeighbors { get; set; } = 20;
}