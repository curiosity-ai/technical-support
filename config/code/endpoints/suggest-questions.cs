[endpoint: Curiosity.Endpoints.Path("suggest-questions")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

// Given the text of the current case, find similar support cases and return the de-duplicated questions
// that support agents asked in those cases (from their ExtractedQuestions nodes). Used to suggest starter
// questions when the case AI chat opens. Prefers the PII-sanitized questions when available.

var request = Body.FromJson<SuggestQuestionsRequest>();

if (request is null || string.IsNullOrWhiteSpace(request.Text))
{
    return new SuggestQuestionsResponse() { Error = "Text is required" };
}

var caseCount = request.CaseCount > 0 ? request.CaseCount : 20;
var maxQuestions = request.MaxQuestions > 0 ? request.MaxQuestions : 8;

var similar = await Q().StartAtSimilarTextAsync(request.Text, nodeTypes: [N.SupportCase.Type], count: caseCount);

var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
var result = new List<string>();

foreach (var caseUID in similar.AsUIDEnumerable())
{
    if (caseUID == request.ExcludeCaseUID) continue;

    foreach (var eq in Q().StartAt(caseUID).Out(N.ExtractedQuestions.Type, E.HasExtractedQuestions).AsEnumerable())
    {
        var questions = eq.GetBool(N.ExtractedQuestions.Sanitized)
            ? eq.GetStringList(N.ExtractedQuestions.SanitizedQuestions)
            : eq.GetStringList(N.ExtractedQuestions.Questions);

        foreach (var q in questions)
        {
            var trimmed = q?.Trim();
            if (!string.IsNullOrEmpty(trimmed) && seen.Add(trimmed)) result.Add(trimmed);
        }

        if (result.Count >= maxQuestions) break;
    }

    if (result.Count >= maxQuestions) break;
}

return new SuggestQuestionsResponse() { Questions = result.Take(maxQuestions).ToList() };

public class SuggestQuestionsRequest
{
    public string Text { get; set; }
    public UID128 ExcludeCaseUID { get; set; }
    public int MaxQuestions { get; set; }
    public int CaseCount { get; set; }
}

public class SuggestQuestionsResponse
{
    public List<string> Questions { get; set; }
    public string Error { get; set; }
}

