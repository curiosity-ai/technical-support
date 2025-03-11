using Curiosity.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TechnicalSupport;
using static TechnicalSupport.Schema;
using UID;

string token = Environment.GetEnvironmentVariable("CURIOSITY_API_TOKEN");

if (string.IsNullOrWhiteSpace(token))
{
    PrintHelp();
    return;
}

using (var graph = Graph.Connect("http://localhost:8080/", token, "Curiosity Connector"))
{
    try
    {
        await graph.LogAsync("Starting Curiosity connector");
        Console.WriteLine("Creating schemas");
        await CreateSchemasAsync(graph);

        Console.WriteLine("Ingesting data");
        await UploadDataAsync(graph);
        Console.WriteLine("Done");

        await graph.LogAsync("Finished Curiosity connector");
    }
    catch(Exception E)
    {
        await graph.LogErrorAsync(E.ToString());
        throw;
    }
}


void PrintHelp()
{
    Console.WriteLine("Missing API token, you can set it using the CURIOSITY_API_TOKEN environment variable.");
}

async Task CreateSchemasAsync(Graph graph)
{
    await graph.CreateNodeSchemaAsync<Nodes.Device>();
    await graph.CreateNodeSchemaAsync<Nodes.Part>();
    await graph.CreateNodeSchemaAsync<Nodes.Manufacturer>();
    await graph.CreateNodeSchemaAsync<Nodes.SupportCase>();
    await graph.CreateNodeSchemaAsync<Nodes.Status>();
    await graph.CreateEdgeSchemaAsync(typeof(Edges));
}

async Task UploadDataAsync(Graph graph)
{
    var devices = JsonConvert.DeserializeObject<DeviceJson[]>(File.ReadAllText(Path.Combine("..", "data", "devices.json")));
    var parts   = JsonConvert.DeserializeObject<PartJson[]>(File.ReadAllText(Path.Combine("..", "data", "parts.json")));
    var cases   = JsonConvert.DeserializeObject<SupportCaseJson[]>(File.ReadAllText(Path.Combine("..", "data", "support-cases.json")));

    Console.WriteLine("> Ingesting devices");
    foreach (var device in devices)
    {
        graph.TryAdd(new Nodes.Device() { Name = device.Name });
    }

    Console.WriteLine("> Ingesting parts");
    foreach (var part in parts)
    {
        var partNode = graph.TryAdd(new Nodes.Part() { Name = part.Name });

        if (!string.IsNullOrWhiteSpace(part.Manufacturer))
        {
            var manufacturerNode = graph.TryAdd(new Nodes.Manufacturer() { Name = part.Manufacturer });
            graph.Link(partNode, manufacturerNode, Edges.HasManufacturer, Edges.ManufacturerOf);
        }

        foreach (var device in part.Devices)
        {
            graph.Link(partNode, Node.Key(nameof(Nodes.Device), device), Edges.PartOf, Edges.HasPart);
        }
    }

    var supportCaseId = 0;
    Console.WriteLine("> Ingesting cases");
    foreach (var supportCase in cases.OrderBy(t => t.Time))
    {
        var supportCaseNode = graph.TryAdd(new Nodes.SupportCase() { Id = $"SC-{supportCaseId:0000}", Content = supportCase.Content, Summary = supportCase.Summary, Time = supportCase.Time });

        var statusNode = graph.TryAdd(new Nodes.Status { Value = supportCase.Status });
        graph.Link(supportCaseNode, statusNode, Edges.HasStatus, Edges.StatusOf);
        supportCaseId++;
    }

    Console.WriteLine(" > Commiting pending changes");
    await graph.CommitPendingAsync();
}
