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
string endpointToken = Environment.GetEnvironmentVariable("CURIOSITY_ENDPOINTS_TOKEN");

if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(endpointToken))
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


        var response = await graph.QueryAsync(q => q.StartAt(nameof(Nodes.Device)).EmitCount("C"));
        var count = response.GetEmittedCount("C");

        var response2 = await graph.QueryAsync(q => q.StartAt(nameof(Nodes.Device)).Take(10).Emit("N", [nameof(Nodes.Device.Name)]));
        var nodes = response2.GetEmitted("N").ToDictionary(n => n.UID, n => n.GetField<string>(nameof(Nodes.Device.Name)));

        await graph.LogAsync("Finished Curiosity connector");
    }
    catch(Exception E)
    {
        await graph.LogErrorAsync(E.ToString());
        throw;
    }
}

await TestEndpointsAsync(endpointToken);


void PrintHelp()
{
    Console.WriteLine("Missing tokens, you can set it using the CURIOSITY_API_TOKEN and CURIOSITY_ENDPOINTS_TOKEN environment variables");
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
        var devideNode = graph.TryAdd(new Nodes.Device() { Name = device.Name });
        graph.AddAlias(devideNode, Mosaik.Core.Language.Any, device.Name.Replace("-", " "), ignoreCase: false);
        graph.AddAlias(devideNode, Mosaik.Core.Language.Any, device.Name.Replace("-", "."), ignoreCase: false);
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
            graph.Link(partNode, Node.FromKey(nameof(Nodes.Device), device), Edges.PartOf, Edges.HasPart);
        }
    }

    var supportCaseId = 0;
    Console.WriteLine("> Ingesting cases");
    foreach (var supportCase in cases.OrderBy(t => t.Time))
    {
        var supportCaseNode = graph.TryAdd(new Nodes.SupportCase() { Id = $"SC-{supportCaseId:0000}", Content = supportCase.Content, Summary = supportCase.Summary, Time = supportCase.Time });

        var statusNode = graph.TryAdd(new Nodes.Status { Value = supportCase.Status });
        graph.UnlinkExcept(supportCaseNode, statusNode, Edges.HasStatus, Edges.StatusOf);
        graph.Link(supportCaseNode, statusNode, Edges.HasStatus, Edges.StatusOf);

        graph.Link(supportCaseNode, Node.FromKey(nameof(Nodes.Device), supportCase.Device), Edges.ForDevice, Edges.HasSupportCase);

        supportCaseId++;
    }

    Console.WriteLine("> Commiting pending changes");
    await graph.CommitPendingAsync();
}


async Task TestEndpointsAsync(string endpointToken)
{
    //Endpoints can be called using the EndpointsClient wrapper class.
    var endpointClient = new EndpointsClient("http://localhost:8080/", endpointToken);
    
    var responseHelloWorld = await endpointClient.CallAsync<string>("hello-world");
    Console.WriteLine($"Endpoint 'hello-world' answered with {responseHelloWorld}");

    var responsePooling = await endpointClient.CallAsync<string>("long-running-hello-world");
    Console.WriteLine($"Endpoint 'long-running-hello-world' answered with {responsePooling}");

    var responseReplay = await endpointClient.CallAsync<string, string>("replay", "Why don’t APIs ever get lost? Because they always REST.");
    Console.WriteLine($"Endpoint 'replay' answered with {responseReplay}");

    var responseJson = await endpointClient.CallAsync<Nodes.Device, Nodes.Device>("replay", new Nodes.Device() { Name = "Test Device" });
    Console.WriteLine($"Endpoint 'replay' answered with {responseJson.Name}");
}