[endpoint: Curiosity.Endpoints.Path("similar-cases-engine")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

// Multi-signal Similarity Engine: given a support case, recommend related cases by fusing
// text-embedding similarity (summary + conversation) with graph-relationship signals
// (same device, same manufacturer), then applying optional post-fusion rules.
// See https://docs.curiosity.ai/workspace-build/search-and-nlp/similarity-and-vector-search/similarity-engine
// Text signals require AI Search to be enabled on SupportCase.SupportCaseSummary / SupportCase.Content
// (see search-configuration/INSTRUCTIONS.md); without embeddings the graph signals still return results.

class SimilarCasesEngineRequest
{
    public UID128 CaseUID        { get; set; } // seed support case
    public bool   ResolvedOnly   { get; set; } // rule: keep only Status == "Closed"
    public bool   SameDeviceOnly { get; set; } // rule: keep only cases for the seed's device
    public int    Count          { get; set; } // cap on fused candidates; default 25
}

var request = Body.FromJson<SimilarCasesEngineRequest>();
var seedUID = request.CaseUID;
var count   = request.Count > 0 ? request.Count : 25;

var seedNode = Graph.Get(seedUID);
var summary  = seedNode.GetString(N.SupportCase.SupportCaseSummary) ?? "";
var content  = seedNode.GetString(N.SupportCase.Content) ?? "";
var device   = Q().StartAt(seedUID).Out(N.Device.Type, E.ForDevice).AsUIDEnumerable().FirstOrDefault();

var result = await Q()
    .StartAt(seedUID)
    .ToSimilarity(o => o
        .MaxCandidates(count)
        .MaxCandidatesPerSignal(100))
    .AddSignal("SimilarSummary", s => s
        .Describe("Cases with a semantically similar summary")
        .Weight(1.0f)
        .Limit(100)
        .FromAsync(async ctx => await ctx.Graph.Query()
            .StartAtSimilarTextAsync(summary, count: 100, nodeTypes: [N.SupportCase.Type])))
    .AddSignal("SimilarContent", s => s
        .Describe("Cases with a semantically similar conversation")
        .Weight(0.7f)
        .Limit(100)
        .FromAsync(async ctx => await ctx.Graph.Query()
            .StartAtSimilarTextAsync(content, count: 100, nodeTypes: [N.SupportCase.Type])))
    .AddSignal("SameDevice", s => s
        .Describe("Other cases reported for the same device")
        .Weight(0.6f)
        .From(ctx => ctx.Graph.Query()
            .StartAt(ctx.Subjects)
            .Out(N.Device.Type, E.ForDevice)
            .Out(N.SupportCase.Type, E.HasSupportCase)))
    .AddSignal("SameManufacturer", s => s
        .Describe("Cases for other devices from the same manufacturer")
        .Weight(0.3f)
        .From(ctx => ctx.Graph.Query()
            .StartAt(ctx.Subjects)
            .Out(N.Device.Type, E.ForDevice)
            .Out(N.Part.Type, E.HasPart)
            .Out(N.Manufacturer.Type, E.HasManufacturer)
            .Out(N.Part.Type, E.ManufacturerOf)
            .Out(N.Device.Type, E.PartOf)
            .Out(N.SupportCase.Type, E.HasSupportCase)))
    .Fuse(Fusion.Sum)
    .AddRule("ResolvedOnly", r =>
    {
        r.Enabled(request.ResolvedOnly);
        r.Filter((ctx, candidates) => candidates.Where(uid => Graph.Get(uid).GetString(N.SupportCase.Status) == "Closed"));
    })
    .AddRule("SameDeviceOnly", r =>
    {
        r.Enabled(request.SameDeviceOnly && device.IsNotNull());
        r.Filter((ctx, candidates) => ctx.Graph.Query().StartAt(candidates).IsRelatedTo(device).AsUIDEnumerable());
    })
    .ExecuteAsync(CancellationToken);

return result;
