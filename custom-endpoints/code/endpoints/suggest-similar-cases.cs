[endpoint: Curiosity.Endpoints.Path("suggest-similar-cases")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]
[endpoint: Curiosity.Endpoints.ReadOnly]

// The support cases most similar to the given one, for the same device, best first. Similarity is measured by the
// AI search (embeddings) index on SupportCase, from the case's summary and its first message.

var caseUID = ParseBody<UID128>();

if (!Graph.TryGet(caseUID, out var caseNode) || caseNode.Type != N.SupportCase.Type) return BadRequest("Expected the UID of a support case");

var summary       = caseNode.GetString(N.SupportCase.SupportCaseSummary);
var firstQuestion = Q().StartAt(caseUID).Out(N.SupportCaseMessage.Type, E.HasMessage).AsEnumerable().FirstOrDefault()?.GetString(N.SupportCaseMessage.Message);
var device        = Q().StartAt(caseUID).Out(N.Device.Type, E.ForDevice).AsUIDEnumerable().FirstOrDefault();

var similar = await Q().StartAtSimilarTextAsync(summary + "\n" + firstQuestion, count: 500, nodeTypes: [N.SupportCase.Type]);

if (device.IsNotNull()) similar = similar.IsRelatedTo(device);

return similar.AsScoredUIDEnumerable()
              .Where(s => s.UID.UID != caseUID)
              .OrderByDescending(s => s.Score)
              .Select(s => new SimilarCase { UID = s.UID.UID, Score = s.Score })
              .ToArray();

public class SimilarCase
{
    public UID128 UID   { get; set; }
    public float  Score { get; set; }
}
