[endpoint: Curiosity.Endpoints.Path("bulk-extract-questions")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

// Batch driver over the "Extract Support Questions" AI tool (UID ExtracTQ11111111111111 - the same agent
// the single-case `extract-questions` endpoint uses). Samples support cases at random, but only from cases
// where extracting the Support agent's questions makes sense, runs the agent over each sampled case, and
// stores an ExtractedQuestions node per case (keyed "support-questions-{caseUID}") - back-filling the graph
// that `suggest-questions` reads from.
//
// One agent call per sampled case, so configure it to run in Pooling mode. It writes nodes (not read-only).

var request = string.IsNullOrWhiteSpace(Body)
    ? new BulkExtractQuestionsRequest()
    : Body.FromJson<BulkExtractQuestionsRequest>();

var sampleSize = request.Sample > 0 ? request.Sample : 100;
var rng = request.Seed.HasValue ? new Random(request.Seed.Value) : Random.Shared;

// -------------------------------------------------------------------------
// 1. Heuristic pre-selection: only keep conversations where extracting the
//    Support agent's questions is meaningful. Runs over the cheap stored
//    `Content` field so we can scan the whole dataset without per-case queries.
// -------------------------------------------------------------------------
await RelayStatusAsync("Scanning support cases for eligible conversations...");

var eligible = new List<(UID128 UID, string Id)>();
var totalCases = 0;

foreach (var caseNode in Q().StartAt(N.SupportCase.Type).AsEnumerable())
{
    totalCases++;

    var content = caseNode.GetString(N.SupportCase.Content);
    if (string.IsNullOrWhiteSpace(content)) continue;
    if (!MakesSense(ParseTurns(content))) continue;

    eligible.Add((caseNode.UID, caseNode.GetString(N.SupportCase.Id)));
}

// -------------------------------------------------------------------------
// 2. Randomly sample up to `sampleSize` of the eligible cases.
// -------------------------------------------------------------------------
var sampled = eligible.OrderBy(_ => rng.Next()).Take(sampleSize).ToList();

await RelayStatusAsync($"{eligible.Count:n0} of {totalCases:n0} cases are eligible; extracting from {sampled.Count:n0}...");

// -------------------------------------------------------------------------
// 3. Run the Extract Support Questions agent over each sampled case and
//    persist an ExtractedQuestions node (same shape as `extract-questions`).
// -------------------------------------------------------------------------
var results = new List<CaseExtractionResult>();
var done = 0;

foreach (var c in sampled)
{
    CancellationToken.ThrowIfCancellationRequested();

    var result = new CaseExtractionResult { CaseUID = c.UID, CaseId = c.Id, Questions = new List<string>() };

    try
    {
        var messages = Q().StartAt(c.UID)
                          .Out(N.SupportCaseMessage.Type, E.HasMessage)
                          .SortByTimestamp(oldestFirst: true)
                          .AsEnumerable()
                          .ToList();

        var transcript = string.Join("\n", messages.Select(m => $"{m.GetString(N.SupportCaseMessage.Author)}: {m.GetString(N.SupportCaseMessage.Message)}"));

        var toolResult = await RunToolAsync<string>(AI_Tools.ExtractSupportQuestions, "ExtractQuestions", new { conversation = transcript }.ToJson(), user: CurrentUser);

        if (toolResult is null || string.IsNullOrWhiteSpace(toolResult.Result))
        {
            result.Error = "Extract Support Questions tool returned no output";
        }
        else
        {
            var extracted = StripJsonFences(toolResult.Result).FromJson<ExtractedPayload>();
            var questions = extracted?.Questions ?? new List<string>();
            var topic = extracted?.Topic ?? "";

            var id = $"support-questions-{c.UID}";
            var node = await Graph.GetOrAddLockedAsync(N.ExtractedQuestions.Type, id);

            node.SetInt(N.ExtractedQuestions.MessageCount, messages.Count);
            node.SetString(N.ExtractedQuestions.Topic, topic);
            node.SetBool(N.ExtractedQuestions.Sanitized, false);

            var questionsList = node.GetStringList(N.ExtractedQuestions.Questions);
            questionsList.Clear();
            questionsList.AddRange(questions);

            var caseNode = await Graph.TryGetLockedAsync(c.UID);
            if (caseNode is object)
            {
                node.AddUniqueEdge(E.ForSupportCase, caseNode);
                caseNode.AddUniqueEdge(E.HasExtractedQuestions, node);
                await Graph.CommitAsync(node, caseNode);
            }
            else
            {
                await Graph.CommitAsync(node);
            }

            result.ExtractedQuestionsUID = node.UID;
            result.Topic = topic;
            result.Questions = questions;
        }
    }
    catch (Exception ex)
    {
        result.Error = ex.Message;
        Logger.LogWarning(ex, "Failed to extract questions for case {0}", c.Id);
    }

    results.Add(result);

    done++;
    if (done % 5 == 0 || done == sampled.Count)
    {
        await RelayStatusAsync($"Extracted questions from {done:n0}/{sampled.Count:n0} cases...");
    }
}

return new BulkExtractQuestionsResponse
{
    TotalCases = totalCases,
    EligibleCases = eligible.Count,
    SampledCases = sampled.Count,
    TotalQuestions = results.Sum(r => r.Questions.Count),
    Results = results,
};

// -------------------------------------------------------------------------
// Helpers
// -------------------------------------------------------------------------

// Splits a stored `Content` chat into ordered (Author, Message) turns, using
// the same "User: " / "Support: " markers that the data connector ingests.
static List<(string Author, string Message)> ParseTurns(string content)
{
    var turns = new List<(string, string)>();
    var sb = new System.Text.StringBuilder();
    string current = null;

    void Flush()
    {
        if (current != null && sb.Length > 0) turns.Add((current, sb.ToString().Trim()));
        sb.Clear();
    }

    foreach (var raw in content.Split('\n'))
    {
        var line = raw.TrimEnd('\r');
        if (line.StartsWith("User: "))
        {
            Flush();
            current = "User";
            sb.AppendLine(line.Substring("User: ".Length));
        }
        else if (line.StartsWith("Support: "))
        {
            Flush();
            current = "Support";
            sb.AppendLine(line.Substring("Support: ".Length));
        }
        else
        {
            sb.AppendLine(line);
        }
    }
    Flush();
    return turns;
}

// The heuristic: only keep conversations where extracting the Support agent's
// questions is meaningful:
//   - a real dialogue (>= 4 turns, with at least 2 turns from each side), and
//   - Support actually asked at least one question (a turn containing '?').
// This filters out one-shot exchanges and instruction-only cases, so the agent
// is only ever run on chats that make sense.
static bool MakesSense(List<(string Author, string Message)> turns)
{
    if (turns.Count < 4) return false;

    var userTurns = turns.Count(t => t.Author == "User");
    var supportTurns = turns.Count(t => t.Author == "Support");
    if (userTurns < 2 || supportTurns < 2) return false;

    return turns.Any(t => t.Author == "Support" && t.Message.Contains('?'));
}

// The Extract Support Questions tool returns {"questions":[...],"topic":"..."};
// strip any accidental markdown fences before parsing (mirrors extract-questions).
static string StripJsonFences(string text)
{
    if (string.IsNullOrWhiteSpace(text)) return "{}";

    var trimmed = text.Trim();

    if (trimmed.StartsWith("```"))
    {
        var firstNewLine = trimmed.IndexOf('\n');
        if (firstNewLine >= 0) trimmed = trimmed.Substring(firstNewLine + 1);
        if (trimmed.EndsWith("```")) trimmed = trimmed.Substring(0, trimmed.Length - 3);
    }

    return trimmed.Trim();
}

public class BulkExtractQuestionsRequest
{
    public int Sample { get; set; } = 100; // how many eligible cases to sample
    public int? Seed { get; set; }         // optional seed for reproducible sampling
}

public class ExtractedPayload
{
    public List<string> Questions { get; set; }
    public string Topic { get; set; }
}

public class CaseExtractionResult
{
    public UID128 CaseUID { get; set; }
    public string CaseId { get; set; }
    public UID128 ExtractedQuestionsUID { get; set; }
    public string Topic { get; set; }
    public List<string> Questions { get; set; }
    public string Error { get; set; }
}

public class BulkExtractQuestionsResponse
{
    public int TotalCases { get; set; }
    public int EligibleCases { get; set; }
    public int SampledCases { get; set; }
    public int TotalQuestions { get; set; }
    public List<CaseExtractionResult> Results { get; set; }
}

