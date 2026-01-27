# Curiosity Workspace Implementation Guide

This guide provides the essential information for developing and deploying a Curiosity Workspace. It covers setting up the environment, ingesting data, configuring NLP and search, and building custom endpoints and front-ends.

## 🛠️ Prerequisites

Before you begin, ensure you have the following tools installed:

1.  **.NET SDK 10.0 or later**: Required for building and running projects.
2.  **h5 Compiler**: Transpiles C# code to JavaScript for the front-end.
    ```bash
    dotnet tool install --global h5-compiler
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
Endpoints are scripts that run within the workspace.
*   **Modes**: `Sync` (immediate) or `Pooling` (long-running with `MSK-ENDPOINT-KEY`).
*   **Authorization**: Unrestricted, Logged Users, or Admin Only.

### Global Scope
The following objects and methods are available in the endpoint's global scope:
*   `Graph` (or `G`): Access to the graph database.
*   `Q()`: Starting point for graph queries.
*   `Body`: The raw request body as a string.
*   `CurrentUser`: UID of the authenticated user.
*   `CancellationToken`: For handling request cancellation.
*   `Logger`: For writing to system logs.
*   `ChatAI`: Access to LLM completion and tools.

---

## 🖥️ Custom Front-Ends

Front-ends are Single-Page Applications (SPAs) built with C# and the **h5** compiler. The primary namespace for Curiosity Workspace front-ends is `Mosaik`.

### Mandatory Imports
Ensure the following `using` statements are included in your front-end project to access Tesserae and Curiosity components:
```csharp
using Tesserae;
using static Tesserae.UI;
using Mosaik;
using Mosaik.Components;
using Mosaik.Schema;
using Mosaik.Views;
using static Mosaik.UI;
using H5;
using static H5.Core.dom;
using Node = Mosaik.Schema.Node; // To avoid conflicts with dom.Node
```

### Frameworks
*   **Tesserae**: A lightweight UI framework for C# (uses `IComponent` and `Render()`).
*   **Curiosity UI Toolkit**: Provides high-level components like `SearchArea`, `Neighbors`, and `GraphExplorerView`.

### Node Renderers
Implement `INodeRenderer` to define visual representations of node types:
```csharp
public class MyRenderer : INodeRenderer {
    public string NodeType => "MyType";
    public string DisplayName => "My Type";
    // ... other properties (Icon, Color, LabelField)

    public CardContent CompactView(Node node) => CardContent(Header(this, node), null);
    public async Task<CardContent> PreviewAsync(Node node, Parameters p) => CardContent(Header(this, node), TextBlock(node.GetString("Description")));
    public async Task<IComponent> ViewAsync(Node node, Parameters p) => (await PreviewAsync(node, p)).Merge();
}
```

### Routing
Map URL hashes to views:
```csharp
Router.Register("home", state => App.ShowDefault(new HomeView(state)));
Router.Register("settings", state => App.ShowDefault(new SettingsView(state)));
```

### Development Tips
*   **Cross-Platform Paths**: In C# code, always use `Path.Combine` to ensure compatibility across different operating systems. When using `Exec` in `.csproj` files, use forward slashes (`/`) instead of backslashes (`\`) to ensure compatibility with both Windows and Linux/macOS environments.
*   **CLI Uploads**: You can use `ContinueOnError="true"` in your project file's upload target if you want the build to succeed even if the workspace is temporarily unreachable.

### Deployment
Once you have compiled your front-end project using h5, you can deploy it to your Curiosity Workspace using one of the following methods:

*   **Manual Upload**: Zip the contents of the `h5` output folder and upload the `.zip` file via the **Interfaces** section in the Management interface.
*   **Curiosity CLI**: Use the CLI to upload the project directly. There is no need to zip the folder when using this method:
    ```bash
    curiosity-cli upload-front-end -s <workspace-url> -t <interface-token> -p <path-to-h5-folder>
    ```

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
8.  **Execute transaction**: `await Q().StartAt("UID").Tx().AddProperty("NewProp", "Value").CommitAsync();`
9.  **AI search similarity**: `return Q().StartAtSimilarText("my query").EmitWithScores();`
