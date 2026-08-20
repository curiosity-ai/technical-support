[endpoint: Curiosity.Endpoints.Path("extract-questions")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

// Runs the "Extract Support Questions" AI tool (UID below) over a support case's conversation and stores
// the result in an ExtractedQuestions node keyed "support-questions-{caseUID}".

var caseUID = UID128.Parse(Body.Trim('"'));

if (!Graph.HasNodeOfType(caseUID, N.SupportCase.Type))
{
    return new ExtractQuestionsResponse() { Error = $"No SupportCase found for {caseUID}" };
}

var messages = Q().StartAt(caseUID)
                  .Out(N.SupportCaseMessage.Type, E.HasMessage)
                  .SortByTimestamp(oldestFirst: true)
                  .AsEnumerable()
                  .ToList();

var messageCount = messages.Count;

var transcript = string.Join("\n", messages.Select(m => $"{m.GetString(N.SupportCaseMessage.Author)}: {m.GetString(N.SupportCaseMessage.Message)}"));

var toolResult = await RunToolAsync<string>(AI_Tools.ExtractSupportQuestions, "ExtractQuestions", new { conversation = transcript }.ToJson(), user: CurrentUser);

if (toolResult is null || string.IsNullOrWhiteSpace(toolResult.Result))
{
    return new ExtractQuestionsResponse() { Error = "Extract Support Questions tool returned no output" };
}

var extracted = StripJsonFences(toolResult.Result).FromJson<ExtractedPayload>();
var questions = extracted?.Questions ?? new List<string>();
var topic     = extracted?.Topic ?? "";

var id   = $"support-questions-{caseUID}";
var node = await Graph.GetOrAddLockedAsync(N.ExtractedQuestions.Type, id);

node.SetInt(N.ExtractedQuestions.MessageCount, messageCount);
node.SetString(N.ExtractedQuestions.Topic, topic);
node.SetBool(N.ExtractedQuestions.Sanitized, false);

var questionsList = node.GetStringList(N.ExtractedQuestions.Questions);
questionsList.Clear();
questionsList.AddRange(questions);

var caseNode = await Graph.TryGetLockedAsync(caseUID);

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

return new ExtractQuestionsResponse()
{
    ExtractedQuestionsUID = node.UID,
    ID                    = id,
    MessageCount          = messageCount,
    Questions             = questions,
    Topic                 = topic
};

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

public class ExtractedPayload
{
    public List<string> Questions { get; set; }
    public string       Topic     { get; set; }
}

public class ExtractQuestionsResponse
{
    public UID128       ExtractedQuestionsUID { get; set; }
    public string       ID                    { get; set; }
    public int          MessageCount          { get; set; }
    public List<string> Questions             { get; set; }
    public string       Topic                 { get; set; }
    public string       Error                 { get; set; }
}
