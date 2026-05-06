using System;
using System.IO;
using System.Threading.Tasks;
using Curiosity.Library;

LoadDotEnv();

var workspaceUrl  = Environment.GetEnvironmentVariable("CURIOSITY_URL")            ?? "http://localhost:8080/";
var apiToken      = Environment.GetEnvironmentVariable("CURIOSITY_API_TOKEN");
var connectorName = Environment.GetEnvironmentVariable("CURIOSITY_CONNECTOR_NAME") ?? "Workspace Cleaner";

if (string.IsNullOrWhiteSpace(apiToken) || apiToken == "replace-with-api-token")
{
    Console.Error.WriteLine("Missing CURIOSITY_API_TOKEN.");
    Console.Error.WriteLine("Create a .env file at the repo root (copy from .env.example) and set CURIOSITY_API_TOKEN,");
    Console.Error.WriteLine("or set it via the launchSettings.json profile.");
    return;
}

Console.WriteLine($"Connecting to {workspaceUrl} ...");
using var graph = Graph.Connect(workspaceUrl, apiToken, connectorName);

// Keep this list in sync with data-connector/src/Schema.cs
string[] nodeTypes =
[
    "Device",
    "Part",
    "Manufacturer",
    "SupportCase",
    "SupportCaseMessage",
    "Status",
    "SupportChatContext",
];

long totalDeleted = 0;

foreach (var nodeType in nodeTypes)
{
    Console.WriteLine($"Deleting {nodeType} nodes...");
    long deleted = await DeleteAllOfTypeAsync(graph, nodeType);
    Console.WriteLine($"  Deleted {deleted:n0} {nodeType} nodes.");
    totalDeleted += deleted;
}

Console.WriteLine($"Done. {totalDeleted:n0} nodes deleted total.");

static async Task<long> DeleteAllOfTypeAsync(Graph graph, string nodeType, int batchSize = 5_000)
{
    long deleted = 0;

    while (true)
    {
        var results = await graph.QueryAsync(q => q.StartAt(nodeType).Emit("Batch"));
        var nodes   = results.GetEmitted("Batch");

        if (nodes.Count == 0) break;

        int i = 0;
        foreach (var n in nodes)
        {
            graph.Delete(Node.FromUID(n.UID));
            if (++i % batchSize == 0)
                await graph.CommitPendingAsync();
        }
        await graph.CommitPendingAsync();

        deleted += nodes.Count;
        Console.WriteLine($"    ...{deleted:n0} deleted so far");

        if (nodes.Count < batchSize) break;
    }

    return deleted;
}

static void LoadDotEnv()
{
    var dir = Directory.GetCurrentDirectory();
    for (int i = 0; i < 4; i++)
    {
        var envFile = Path.Combine(dir, ".env");
        if (File.Exists(envFile))
        {
            foreach (var line in File.ReadAllLines(envFile))
            {
                var trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith("#")) continue;
                var eq = trimmed.IndexOf('=');
                if (eq < 0) continue;
                var key = trimmed.Substring(0, eq).Trim();
                var val = trimmed.Substring(eq + 1).Trim().Trim('"').Trim('\'');
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                    Environment.SetEnvironmentVariable(key, val);
            }
            Console.WriteLine($"Loaded .env from {envFile}");
            return;
        }
        var parent = Directory.GetParent(dir);
        if (parent == null) break;
        dir = parent.FullName;
    }
}
