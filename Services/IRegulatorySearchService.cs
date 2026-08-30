using Label.Mcp.Regulatory.Models;

namespace Label.Mcp.Regulatory.Services;

public interface IRegulatorySearchService
{
    Task<RegulatorySearchResponse> SearchAsync(
        RegulatorySearchRequest request,
        CancellationToken cancellationToken = default);

    Task<string> GetIndexSchemaAsync(
        CancellationToken cancellationToken = default);
}