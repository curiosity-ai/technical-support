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

## 🚀 Workspace Setup

### Installation

*   **Windows**: Download and run the installer from the Curiosity website.
*   **Docker**: Run the workspace using the following command (adjust paths as needed):
    ```bash
    docker run -p 8080:8080 -v ~/curiosity/storage/:/data/ -e storage=/data/curiosity
    ```

### Initial Configuration
Navigate to `http://localhost:8080` and log in with the default credentials (`admin`/`admin`). Follow the setup wizard to name your workspace.

---

## 🧬 Data Connectors

Data connectors map external data into the Curiosity graph.

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
*   **Indexing**: Enable search on specific node types and fields in the Search settings.
*   **Ranking**: Uses BM25. Adjust field boosts to prioritize specific attributes (e.g., titles).

### AI Search (Vector Search)
*   Enable embedding-based search for semantic retrieval.
*   **Chunking**: Enable for large text fields to ensure context window compatibility.

### Filters and Facets
*   **Property Facets**: Filter by direct node attributes.
*   **Related Facets**: Filter based on graph relationships (e.g., filter nodes by their related "Category" node).

---

## ⚡ Custom API Endpoints

Endpoints allow hosting custom C# business logic within the workspace.

*   **Modes**:
    *   **Sync**: Returns immediate results.
    *   **Pooling**: For long-running tasks. Returns `202 Accepted` and a polling key (`MSK-ENDPOINT-KEY`).
*   **Authorization**: Can be Unrestricted, Restricted to Logged Users, or Admin Only.
*   **Global Objects**: `Graph`, `ChatAI`, `Logger`, `Body` (request body), `CurrentUser`.

---

## 🖥️ Custom Front-Ends

Front-ends are Single-Page Applications (SPAs) built with C# and the **h5** compiler.

### Frameworks
*   **Tesserae**: A lightweight UI framework for C# (uses `IComponent` and `Render()`).
*   **Curiosity UI Toolkit**: Provides high-level components like `SearchArea`, `Neighbors`, and `GraphExplorerView`.

### Core Concepts
*   **Node Renderers**: Implement `INodeRenderer` to define how specific node types are displayed (Compact, Preview, and Full views).
*   **Routing**: Use `Router.Register` to map URL hashes to views.
*   **Deployment**: Zip the `h5` output folder and upload it via the Management interface or the CLI:
    ```bash
    curiosity-cli upload-front-end -s <url> -t <token> -p <path_to_h5_folder>
    ```

---

## 🐚 Graph Shell

Use the built-in Shell to run ad-hoc queries and migrations:
```csharp
// Example: Find nodes of a type
return Q().StartAt("MyType").Take(10).Emit();
```
