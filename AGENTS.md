# Curiosity Workspace Implementation Guide

This guide provides the essential information for developing and deploying a Curiosity Workspace. It covers setting up the environment, ingesting data, configuring NLP and search, and building custom endpoints and front-ends.

## 🛠️ Prerequisites

Before you begin, ensure you have the following tools installed:

1.  **.NET SDK 11**: Required for building and running projects (pinned in `global.json`: `11.0.100`).
2.  **Transpose Compiler**: Compiles C# code to JavaScript for the front-end. The build runs it as `tps`, so it must be on `PATH`.
    ```bash
    dotnet tool install --global Transpose.Compiler
    export PATH="$PATH:$HOME/.dotnet/tools"
    ```
3.  **Curiosity CLI Tool**: For managing the workspace and local development.
    ```bash
    dotnet tool install --global Curiosity.CLI
    ```
4.  **SnapFrame**: For interacting with the workspace and capturing screenshots.
    ```bash
    dotnet tool install --global snapframe
    snapframe install
    ```
5.  **Docker**: Required if you plan to host the workspace using containerization.

## 🚀 Workspace Setup

### Installation

*   **Windows**: Download and run the installer from the Curiosity website.
*   **Docker**: Run the workspace using the following command (adjust paths as needed). The official image is `curiosityai/curiosity`.
    ```bash
    # Create a local storage directory
    mkdir -p ~/curiosity/storage
    # Run the container (using curiosityai/curiosity image)
    docker run -d -p 8080:8080 -v ~/curiosity/storage:/data/ -e storage=/data/curiosity curiosityai/curiosity
    ```
    *Note: If you encounter Docker Hub pull rate limits, ensure you are logged in or using a configured mirror.*

### Initial Configuration
Navigate to `http://localhost:8080` and log in with the default credentials (`admin`/`admin`). Follow the setup wizard to name your workspace.

You can verify the deployment using `snapframe`:
```bash
snapframe navigate http://localhost:8080
```

---

## 🧬 Data Connectors

Data connectors are external applications that run outside the Curiosity Workspace and are used to bring data in.

### Getting Started
The connector of this repository is in `data-connector/` (.NET 11, `Curiosity.Library`). Run it from that folder with `dotnet run`; it reads `../data/*.json` and needs `CURIOSITY_API_TOKEN`, `CURIOSITY_ENDPOINTS_TOKEN` and optionally `CURIOSITY_URL` (default `http://localhost:8080/`).

To create a new data connector project, use the following commands:
```bash
dotnet new console -n MyDataConnector
cd MyDataConnector
dotnet add package Curiosity.Library
```

### Modeling and Implementation
Before implementation, read and understand your datasets to decide how to model them as a graph database.
*   **Standard C# Features**: You can use any standard C# library or NuGet package (e.g., `CsvHelper`, `Parquet.Net`, `HttpClient` for APIs).
*   **Graph Mapping**: Appropriately mapping data as nodes and edges is crucial for efficient querying, building intuitive user interfaces, search filtering, and creating similarity engines.

### Defining Schemas
Define node types using C# classes with attributes:
```csharp
[Node]
public class MyNode
{
    [Key] public string Id { get; set; }
    [Property] public string Description { get; set; }
}
```
Register schemas in the graph:
```csharp
await graph.CreateNodeSchemaAsync<MyNode>();
await graph.CreateEdgeSchemaAsync(typeof(EdgesStaticClass));
```

### Data Ingestion Principles
*   **Add Nodes**: Use `graph.TryAdd(new MyNode { ... })`.
*   **Create Edges**: Use `graph.Link(sourceNode, targetNode, "ForwardEdge", "BackwardEdge")`.
*   **Commit**: Always call `await graph.CommitPendingAsync()` to persist changes.

---

## 🧠 Natural Language Processing (NLP)

Curiosity uses NLP pipelines to extract structure from text.

1.  **Languages**: Enable the desired language (e.g., English) in the Management interface.
2.  **Pipelines**: Create a "Data Parsing" pipeline and assign it to specific node fields.
3.  **Spotter Models**:
    *   **Node-based Spotters**: Automatically identify entities based on values in the graph.
    *   **Pattern Spotters**: Use regex-like patterns (e.g., `XX999`) to capture structured data.
4.  **Entity Linking**: Configure rules to automatically link extracted entities to existing nodes or create new ones.

---

## 🔍 Search and AI

### Full-Text Search (FTS)
*   **Indexing**: Enable search on **specific node types and fields** in the Search settings.
*   **Ranking**: Uses BM25. Adjust field boosts to prioritize specific attributes (e.g., titles).

### AI Search (Vector Search)
*   **Configuration**: Enable per **node type and field** for semantic retrieval.
*   **Chunking**: Enable for large text fields to ensure context window compatibility.

### Filters and Facets
*   **Property Facets**: Filter by direct node attributes.
*   **Related Facets**: Filter based on graph relationships.

---

## ⚡ Custom API Endpoints

Endpoints allow hosting custom C# business logic within the workspace.

### Writing Endpoints
Endpoints are scripts that run within the workspace. In this repository they live in `custom-endpoints/code/endpoints/<path>.cs` (the workspace-definitions layout), each file starting with its settings as attributes:
```csharp
[endpoint: Curiosity.Endpoints.Path("sum-values")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]
[endpoint: Curiosity.Endpoints.ReadOnly]
```
*   **Modes**: `Sync` (`[endpoint: Curiosity.Endpoints.SyncWithRequest]`) or `Pooling` (the default; long-running calls answer `202` with an `X-MSK-ENDPOINT-KEY` header to poll with).
*   **Authorization** (`AccessMode`): `Unrestricted`, `AllUsers`, `AccessControlled` or `AdminOnly`.
*   **Read only**: `[endpoint: Curiosity.Endpoints.ReadOnly]` for endpoints that do not write to the graph.
*   **Import/export**: `curiosity-cli import-workspace-definitions -s <url> -t <library-token> --import-path custom-endpoints -z true` and `curiosity-cli export-workspace-definitions -s <url> -t <library-token> --export-path <dir> -u true`.

### Global Scope
The following objects and methods are available in the endpoint's global scope:
*   `Graph`: Access to the graph database.
*   `Q()`: Starting point for graph queries.
*   `Body`: The request body as a stream. Read it with `ParseBody<T>()` (`ParseBody<UID128>()` for a bare UID) or `ReadBodyAsText()`.
*   `Ok(...)`, `BadRequest(...)`, `Forbid()`, `NotFound(...)`: Status responses.
*   `CurrentUser`: UID of the authenticated user.
*   `CancellationToken`: For handling request cancellation; pass it to every awaited call.
*   `Logger`: For writing to system logs.
*   `ChatAI`: Access to LLM completion and chats.
*   `RelayStatusAsync(message)`: Sends a progress message to the caller.
*   `RunEndpointAsync<T>(Endpoints.HelloWorld)`: Runs another endpoint.
*   `N`, `E`, `Endpoints`: Generated helpers for node types, edge types and endpoint paths.

---

## 🖥️ Custom Front-Ends

Front-ends are Single-Page Applications (SPAs) built with C# and the **Transpose** compiler (project SDK `Transpose.Build.Target`, packages `Transpose.BCL`, `Transpose.Newtonsoft.Json`, `Tesserae` and `Curiosity.FrontEnd`, `.API`, `.Core`, `.Admin`). The primary namespace for Curiosity Workspace front-ends is `Mosaik`. The compiler is configured by `tps.json` (plus `tps.Debug.json` / `tps.Release.json`) and writes the site to `bin/<Configuration>/netstandard2.0/tps/`.

### Project Layout
*   `src/App.cs`: `Main`, `Configure(AppSettings)` and `OnLoad` only (routes, settings, sidebar entries).
*   `src/Api/SupportApi.cs`: endpoint DTOs and one method per code endpoint; the only caller of `API.Endpoints`.
*   `src/Renderers/`: one `NodeRendererBase` per node type. `src/Views/`: one file per page. `src/Chat/`, `src/Components/`.
*   `generated/Schema.cs` (`N`, `E`) and `generated/Endpoints.cs` (`Endpoints`, `AI_Tools`, `AIAssistants`): produced by the workspace (the template download under Manage > Interfaces). Never write node types, fields, edges or endpoint paths as strings.

### Mandatory Imports
Every file opens with the same usings block (copy it from the top of `src/App.cs`):
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mosaik;
using Mosaik.Components;
using Mosaik.Helpers;
using Mosaik.Schema;
using Mosaik.Views;
using Tesserae;
using TNT;
using UID;
using static Tesserae.UI;
using static Mosaik.UI;
using static TNT.T;
using static Transpose.Core.dom;
using API  = Mosaik.API;
using Node = Mosaik.Schema.Node; // To avoid conflicts with dom.Node
```
Every user-visible string goes through `.t()`.

### Frameworks
*   **Tesserae**: A lightweight UI framework for C# (uses `IComponent` and `Render()`).
*   **Curiosity UI Toolkit**: Provides high-level components like `SearchArea`, `Neighbors`, `Hub`, `ChatView` and `GraphExplorerView`.

### Node Renderers
Derive from `NodeRendererBase` (found by reflection: public, non-abstract, parameterless constructor) to define how a node type is shown in search rows, previews and full pages:
```csharp
public sealed class DeviceRenderer : NodeRendererBase
{
    public DeviceRenderer() : base(new SchemaStyleInfo
    {
        Name        = N.Device.Type,
        DisplayName = "Device",
        LabelField  = N.Device.Name,
        Color       = "#346eeb",
        Icon        = UIcons.BoxOpenFull.ToString(),
    })
    {
    }

    protected override IEnumerable<NodeTab> BuildTabs(Node node, IReadOnlyList<RelationshipGroup> groups, NodeView nodeView)
    {
        yield return new NodeTab { Id = "product", Label = "Product Page".t(), TabTitle = SegmentTitle("Product Page".t()), TabContent = () => RenderProductPage(node) };
    }
}
```
Override `CompactView` / `PreviewAsync` to change the row or the preview, and implement `INodeCustomStyle` for a per-node icon and color (see `SupportCaseRenderer`).

### Routing
Register routes before `App.Initialize`; a handler activates its view and hands it to `App.ShowDefault`:
```csharp
Router.Register(DevicesView.Route, async state => App.ShowDefault(await AppRouting.ActivateAsync<DevicesView>(state), title: "Devices".t()));
App.Initialize(Configure, OnLoad).FireAndForget();
```
Sidebar entries are added in `AppSidebar.OnSidebarRebuild_BeforeFooter`; interface features are switched off with `settings.OverrideInterfaceSettings`.

### Development Tips
*   **Cross-Platform Paths**: In C# code, always use `Path.Combine` to ensure compatibility across different operating systems. When using `Exec` in `.csproj` files, use forward slashes (`/`) instead of backslashes (`\`) to ensure compatibility with both Windows and Linux/macOS environments.
*   **CLI Uploads**: You can use `ContinueOnError="true"` in your project file's upload target if you want the build to succeed even if the workspace is temporarily unreachable.
*   **Local development**: `curiosity-cli serve --server <url> --watch <project-folder>` compiles the project, serves it and rebuilds on every change.

### Deployment
Once you have compiled your front-end project, you can deploy it to your Curiosity Workspace using one of the following methods:

*   **Curiosity CLI**: Use the CLI to upload the project directly. There is no need to zip the folder when using this method:
    ```bash
    curiosity-cli upload-front-end -s <workspace-url> -t <interface-upload-token> -p bin/Release/netstandard2.0/tps/
    ```
    The project file runs this after a Release build when `CURIOSITY_URL` and `CURIOSITY_UPLOAD_TOKEN` are set.
*   **Manual Upload**: Zip the contents of the `tps` output folder (`index.html` at the root of the zip) and upload the `.zip` file via the **Interfaces** section in the Management interface.

---

## 🐚 Graph Shell

Use the built-in Shell to run ad-hoc queries:

1.  **Count nodes**: `return Q().StartAt("MyType").Count();`
2.  **Find by key**: `return Q().StartAt("MyType", "my-key").Emit();`
3.  **Outbound traversal**: `return Q().StartAt("MyType").Out("MyEdge").Emit();`
4.  **Filter by property**: `return Q().StartAt("MyType").Where(n => n.GetString("Status") == "Active").Emit();`
5.  **Get neighbors summary**: `return Q().StartAt("MyType").EmitNeighborsSummary();`
6.  **Pagination**: `return Q().StartAt("MyType").Skip(10).Take(5).Emit();`
7.  **Sort by timestamp**: `return Q().StartAt("MyType").SortByTimestamp(oldestFirst: false).Emit();`
8.  **Execute transaction**: `await Q().StartAt("UID").Tx().Set("NewProp", "Value").CommitAsync();`
9.  **AI search similarity**: `return (await Q().StartAtSimilarTextAsync("my query")).EmitWithScores();`
