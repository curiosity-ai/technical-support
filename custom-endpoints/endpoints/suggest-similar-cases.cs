[endpoint: Curiosity.Endpoints.Path("suggest-similar-cases")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

var caseUID = UID128.Parse(Body.Trim('"'));
var caseNode = Graph.Get(caseUID);
var summary = caseNode.GetString(N.SupportCase.Summary);
var firstQuestion = Q().StartAt(caseUID).Out(N.SupportCaseMessage.Type).AsEnumerable().First().GetString(N.SupportCaseMessage.Message);
var device = Q().StartAt(caseUID).Out(N.Device.Type, E.ForDevice).AsUIDEnumerable().First();
var deviceSet = new HashSet<UID128>()
{
    device
};
var types = new[]
{
    N.SupportCase.Type
};
var scores = new Dictionary<UID128, float>();
foreach (var t in types)
{
    foreach (var index in Graph.Internals.Indexes.OfType<SentenceEmbeddingsIndex>(t))
    {
        var similarFromSummary = index.FindSimilar(CurrentUser, summary + "\n" + firstQuestion, 500);
        foreach (var s in similarFromSummary)
        {
            if (Graph.Internals.IsRelatedTo(s.UID, deviceSet))
            {
                if (scores.TryGetValue(s.UID, out var previousScore))
                {
                    scores[s.UID] = ScoredQueryInit.MergeScore(previousScore, s.Score);
                }
                else
                {
                    scores[s.UID] = s.Score;
                }
            }
        }
    }
}

scores.Remove(caseUID, out _);
return scores; // var similar = Q().StartAt(caseUID).Similar(count:500).IsRelatedTo(device).EmitUIDsWithScores();
