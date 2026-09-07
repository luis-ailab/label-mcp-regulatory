Here's a README.md draft you can include in the Label.Mcp.Regulatory repository.

Label.Mcp.Regulatory
Overview

Label.Mcp.Regulatory is an MCP (Model Context Protocol) server responsible for providing regulatory knowledge to the Label Creation Platform.

Rather than relying on static files stored locally, this service retrieves regulatory information from Azure AI Search, which contains regulatory documents that have been uploaded to SharePoint Online and indexed through an Azure AI Search ingestion pipeline.

The service enables agents within the Label Creation Platform to perform regulatory research, retrieve supporting documentation, and answer compliance-related questions using Retrieval-Augmented Generation (RAG).

Solution Architecture
Regulatory PDF Documents
            │
            ▼
      SharePoint Online
            │
            ▼
      Azure AI Search
      (Indexed Content)
            │
            ▼
   Label.Mcp.Regulatory
            │
            ▼
 Regulatory Agent
            │
            ▼
 Label Agent Orchestrator
            │
            ▼
         Label.Web

Regulatory Content Management

The repository contains a Regulations folder with sample regulatory PDF documents.

These PDF files are provided for reference and initial testing purposes only.

Important

Before running the complete solution:

Upload the PDF documents from the Regulations folder to a SharePoint document library.
Configure Azure AI Search to index the SharePoint library.
Verify that document chunks, vectors, titles, and source URLs are being populated in the Azure AI Search index.
Update the application's configuration to point to the correct Azure AI Search service and index.

Once indexed, Label.Mcp.Regulatory will query Azure AI Search rather than accessing PDF files directly.

Prerequisites

Before running the project, ensure the following resources are available:

Azure AI Search
SharePoint Online document library
Azure OpenAI vectorization pipeline (if vector search is enabled)
Indexed regulatory documents
.NET SDK (latest supported version)
Configuration

Create an appsettings.json file based on appsettings.example.json.

appsettings.example.json
{
  "AzureSearch": {
    "Endpoint": "YOUR_AZURE_SEARCH_ENDPOINT",
    "IndexName": "YOUR_INDEX_NAME",
    "ApiKey": "",
    "ContentField": "chunk",
    "TitleField": "title",
    "SourceUrlField": "metadata_spo_item_path",
    "DocumentIdField": "chunk_id",
    "VectorField": "text_vector",
    "SemanticConfiguration": "YOUR_SEMANTIC_CONFIGURATION",
    "DefaultTop": 5,
    "MaximumTop": 20,
    "DefaultKNearestNeighbors": 20
  }
}

Configuration Parameters
Setting	DescriptionEndpoint	Azure AI Search service endpoint
IndexName	Name of the Azure AI Search index
ApiKey	Azure AI Search query key (if using key-based authentication)
ContentField	Field containing indexed text chunks
TitleField	Document title field
SourceUrlField	Original SharePoint document URL
DocumentIdField	Unique chunk identifier
VectorField	Vector embedding field
SemanticConfiguration	Azure AI Search semantic ranking configuration
DefaultTop	Default number of results returned
MaximumTop	Maximum number of results allowed
DefaultKNearestNeighbors	Default vector search neighborhood size
Expected Azure AI Search Schema

The service expects the search index to contain fields similar to:

chunk
title
metadata_spo_item_path
chunk_id
text_vector


If your ingestion pipeline uses different field names, update the configuration accordingly.

Running the Project

Restore packages:

dotnet restore


Build the project:

dotnet build


Run the service:

dotnet run

Role Within the Label Creation Platform

Label.Mcp.Regulatory serves as the regulatory knowledge provider for the overall system.

Responsibilities include:

Regulatory document retrieval
Semantic search of compliance documentation
Citation and source retrieval
Regulatory guidance support
RAG-based context generation for downstream agents

The service is typically consumed by the Regulatory Agent, which in turn is orchestrated by the Label.Agent.Orchestrator during label generation and compliance review workflows.

Repository Structure
Label.Mcp.Regulatory
│
├── Regulations/
│   ├── *.pdf
│
├── appsettings.example.json
├── Program.cs
├── Services/
├── Models/
└── README.md

Notes
appsettings.json is intentionally excluded from source control.
Use appsettings.example.json as the template when configuring a new environment.
Regulatory PDFs should be stored in SharePoint and indexed by Azure AI Search for production deployments.
The local Regulations folder is intended as a source repository for regulatory content and should not be treated as the primary runtime data source.
The quality of search results depends heavily on proper chunking, vectorization, and indexing of regulatory documents within Azure AI Search.