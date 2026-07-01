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
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.Extensions.Logging;

string token = Environment.GetEnvironmentVariable("CURIOSITY_API_TOKEN");
string workspaceUrl = Environment.GetEnvironmentVariable("CURIOSITY_URL") ?? "http://localhost:8080/";
string connectorName = Environment.GetEnvironmentVariable("CURIOSITY_CONNECTOR_NAME") ?? "Technical Support Connector";

if (string.IsNullOrWhiteSpace(token))
{
    PrintHelp();
    return;
}

var loggerFactory = LoggerFactory.Create(l => l.AddConsole());
var logger = loggerFactory.CreateLogger("Data Connector");

using (var graph = Graph.Connect(workspaceUrl, token, connectorName).WithLoggingFactory(loggerFactory))
{
    loggerFactory.AddProvider(graph.GetServerLoggingProvider());

    try
    {
        logger.LogInformation("Creating schemas");
        await CreateSchemasAsync(graph);

        logger.LogInformation("Ingesting data");
        await UploadDataAsync(graph);
        logger.LogInformation("Done");

        var response = await graph.QueryAsync(q => q.StartAt(nameof(Nodes.Device)).EmitCount("C"));
        var count = response.GetEmittedCount("C");

        var response2 = await graph.QueryAsync(q => q.StartAt(nameof(Nodes.Device)).Take(10).Emit("N", [nameof(Nodes.Device.Name)]));
        var nodes = response2.GetEmitted("N").ToDictionary(n => n.UID, n => n.GetField<string>(nameof(Nodes.Device.Name)));

        logger.LogInformation("Finished data connector");
    }
    catch(Exception E)
    {
        logger.LogError(E, "Error running data connector");
        throw;
    }
}


void PrintHelp()
{
    Console.WriteLine("Missing token. Set the CURIOSITY_API_TOKEN environment variable (and optionally CURIOSITY_URL).");
}

async Task CreateSchemasAsync(Graph graph)
{
    await graph.CreateNodeSchemaAsync<Nodes.Device>();
    await graph.CreateNodeSchemaAsync<Nodes.Part>();
    await graph.CreateNodeSchemaAsync<Nodes.Manufacturer>();
    await graph.CreateNodeSchemaAsync<Nodes.SupportCase>();
    await graph.CreateNodeSchemaAsync<Nodes.SupportCaseMessage>();
    await graph.CreateNodeSchemaAsync<Nodes.Status>();
    await graph.CreateEdgeSchemaAsync(typeof(Edges));
}

async Task UploadDataAsync(Graph graph)
{
    var dataDir = FindDataDir();
    var devices = JsonConvert.DeserializeObject<DeviceJson[]>(File.ReadAllText(Path.Combine(dataDir, "devices.json")));
    var parts   = JsonConvert.DeserializeObject<PartJson[]>(File.ReadAllText(Path.Combine(dataDir, "parts.json")));
    var cases   = JsonConvert.DeserializeObject<SupportCaseJson[]>(File.ReadAllText(Path.Combine(dataDir, "support-cases.json")));

    logger.LogInformation("Ingesting {0:n0} devices", devices.Length);
    foreach (var device in devices)
    {
        var devideNode = graph.TryAdd(new Nodes.Device() { Name = device.Name });
        graph.AddAlias(devideNode, Mosaik.Core.Language.Any, device.Name.Replace("-", " "), ignoreCase: false);
        graph.AddAlias(devideNode, Mosaik.Core.Language.Any, device.Name.Replace("-", "."), ignoreCase: false);
    }

    logger.LogInformation("Ingesting {0:n0} parts", parts.Length);
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
    logger.LogInformation("Ingesting {0:n0} cases", cases.Length);
    foreach (var supportCase in cases.OrderBy(t => t.Time))
    {
        var supportCaseNode = graph.AddOrUpdate(new Nodes.SupportCase() { Id = $"SC-{supportCaseId:0000}", Content = supportCase.Content, SupportCaseSummary = supportCase.Summary, Time = supportCase.Time, Status = supportCase.Status });

        var statusNode = graph.TryAdd(new Nodes.Status { Value = supportCase.Status });
        graph.UnlinkExcept(supportCaseNode, statusNode, Edges.HasStatus, Edges.StatusOf);
        graph.Link(supportCaseNode, statusNode, Edges.HasStatus, Edges.StatusOf);

        graph.Link(supportCaseNode, Node.FromKey(nameof(Nodes.Device), supportCase.Device), Edges.ForDevice, Edges.HasSupportCase);

        var sb = new StringBuilder();
        bool isUser = false;
        int msgId = 0;
        var time = supportCase.Time;
        foreach (var line in supportCase.Content.Split(['\r','\n']))
        {
            if(line.StartsWith("User: "))
            {
                if(sb.Length > 0)
                {
                    var msgNode = graph.AddOrUpdate(new Nodes.SupportCaseMessage() { Id = $"SC-{supportCaseId:0000}-{msgId:000}", Author = isUser ? "User" : "Support", Message = sb.ToString(), Time = time });
                    graph.Link(supportCaseNode, msgNode, Edges.HasMessage, Edges.MessageOf);
                    time += TimeSpan.FromSeconds(Random.Shared.Next(60) * Random.Shared.Next(60));
                    msgId++;
                    sb.Length = 0;
                }
                isUser = true;
                sb.AppendLine(line.Substring("User: ".Length));
            }
            else if (line.StartsWith("Support: "))
            {
                if (sb.Length > 0)
                {
                    var msgNode = graph.AddOrUpdate(new Nodes.SupportCaseMessage() { Id = $"SC-{supportCaseId:0000}-{msgId:000}", Author = isUser ? "User" : "Support", Message = sb.ToString(), Time = time });
                    graph.Link(supportCaseNode, msgNode, Edges.HasMessage, Edges.MessageOf);
                    time += TimeSpan.FromSeconds(Random.Shared.Next(60) * Random.Shared.Next(60));
                    msgId++;
                    sb.Length = 0;
                }
                isUser = false;
                sb.AppendLine(line.Substring("Support: ".Length));
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        if (sb.Length > 0)
        {
            var msgNode = graph.AddOrUpdate(new Nodes.SupportCaseMessage() { Id = $"SC-{supportCaseId:0000}-{msgId:000}", Author = isUser ? "User" : "Support", Message = sb.ToString(), Time = time });
            graph.Link(supportCaseNode, msgNode, Edges.HasMessage, Edges.MessageOf);
            time += TimeSpan.FromSeconds(Random.Shared.Next(60) * Random.Shared.Next(60));
            msgId++;
            sb.Length = 0;
        }

        supportCaseId++;
    }

    await graph.CommitPendingAsync();
}


// Locate the dataset folder by walking up from the working directory, so the
// connector runs both from its own project folder and from the repo root (e.g. when
// the workspace-demo CLI runs it with the demo folder as the working directory).
string FindDataDir()
{
    var dir = Directory.GetCurrentDirectory();
    for (int i = 0; i < 8 && dir is not null; i++)
    {
        var candidate = Path.Combine(dir, "data");
        if (File.Exists(Path.Combine(candidate, "devices.json")))
            return candidate;
        dir = Directory.GetParent(dir)?.FullName;
    }
    throw new FileNotFoundException("Could not locate the 'data' folder (with devices.json) from " + Directory.GetCurrentDirectory());
}