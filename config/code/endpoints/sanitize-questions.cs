[endpoint: Curiosity.Endpoints.Path("sanitize-questions")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

// Runs the "Sanitize Support Questions" AI tool (UID below) over the questions already stored on an
// ExtractedQuestions node, and writes the PII-sanitized questions and topic back into the same node.

var extractedUID = UID128.Parse(Body.Trim('"'));

if (!Graph.HasNodeOfType(extractedUID, N.ExtractedQuestions.Type))
{
    return new SanitizeQuestionsResponse() { Error = $"No ExtractedQuestions node found for {extractedUID}" };
}

var node = await Graph.TryGetLockedAsync(extractedUID);

if (node is null)
{
    return new SanitizeQuestionsResponse() { Error = $"Could not lock ExtractedQuestions node {extractedUID}" };
}

var questions = node.GetStringList(N.ExtractedQuestions.Questions).ToList();
var topic     = node.GetString(N.ExtractedQuestions.Topic);

if (questions.Count == 0)
{
    Graph.AbandonChanges(node);
    return new SanitizeQuestionsResponse() { Error = "The ExtractedQuestions node has no questions to sanitize" };
}

var toolResult = await RunToolAsync<string>(AI_Tools.SanitizeSupportQuestions, "SanitizeQuestions", new { questionsJson = questions.ToJson(), topic = topic ?? "" }.ToJson(), user: CurrentUser);

if (toolResult is null || string.IsNullOrWhiteSpace(toolResult.Result))
{
    Graph.AbandonChanges(node);
    return new SanitizeQuestionsResponse() { Error = "Sanitize Support Questions tool returned no output" };
}

var sanitized          = StripJsonFences(toolResult.Result).FromJson<SanitizedPayload>();
var sanitizedQuestions = sanitized?.SanitizedQuestions ?? new List<string>();
var sanitizedTopic     = sanitized?.SanitizedTopic ?? "";

node.SetString(N.ExtractedQuestions.SanitizedTopic, sanitizedTopic);
node.SetBool(N.ExtractedQuestions.Sanitized, true);

var sanitizedList = node.GetStringList(N.ExtractedQuestions.SanitizedQuestions);
sanitizedList.Clear();
sanitizedList.AddRange(sanitizedQuestions);

await Graph.CommitAsync(node);

return new SanitizeQuestionsResponse()
{
    ExtractedQuestionsUID = extractedUID,
    Sanitized             = true,
    SanitizedQuestions    = sanitizedQuestions,
    SanitizedTopic        = sanitizedTopic
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

public class SanitizedPayload
{
    public List<string> SanitizedQuestions { get; set; }
    public string       SanitizedTopic     { get; set; }
}

public class SanitizeQuestionsResponse
{
    public UID128       ExtractedQuestionsUID { get; set; }
    public bool         Sanitized             { get; set; }
    public List<string> SanitizedQuestions    { get; set; }
    public string       SanitizedTopic        { get; set; }
    public string       Error                 { get; set; }
}
