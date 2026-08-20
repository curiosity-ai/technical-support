[indexes: Curiosity.Indexes.CodeIndex("ExtractedQuestions")]
[indexes: Curiosity.Indexes.Name("Sanitize Extracted Questions")]

// Code index over ExtractedQuestions nodes. As each node comes in to be indexed, it makes sure the
// extracted questions have been PII-sanitized: if they have not been yet, it runs the `sanitize-questions`
// endpoint, which invokes the "Sanitize Support Questions" AI tool (SaniTizeQ1111111111111) and writes the
// SanitizedQuestions / SanitizedTopic back onto the same node, setting Sanitized = true.
//
// This index is enrichment-only (it does not return indexable text). Per the code-index contract we return
// the UIDs that failed so the worker requeues just those with backoff, null when the whole batch succeeded,
// or the full ToIndex batch if the run was cancelled.

var failed = new List<UID128>();

foreach (var uid in ToIndex)
{
    if (CancellationToken.IsCancellationRequested) return ToIndex;

    if (!await TrySanitizeAsync(uid))
    {
        failed.Add(uid);
    }
}

return failed.Count > 0 ? failed : null;

async Task<bool> TrySanitizeAsync(UID128 uid)
{
    try
    {
        var node = Graph.Get(uid);

        if (node is null) return true;                                          // node went away, nothing to do

        if (node.GetBool(N.ExtractedQuestions.Sanitized)) return true;          // already sanitized
        if (node.GetStringList(N.ExtractedQuestions.Questions).Count == 0) return true; // nothing extracted yet

        // Runs the shared sanitize-questions endpoint, which calls the Sanitize Support Questions AI tool and
        // persists the sanitized questions back onto this ExtractedQuestions node.
        var result = await RunEndpointAsync<SanitizeQuestionsResult>("sanitize-questions", uid.ToString());

        if (result is object && !string.IsNullOrEmpty(result.Error))
        {
            Logger.LogWarning("sanitize-questions returned an error for {0}: {1}", uid, result.Error);
        }

        return true;
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Failed to sanitize ExtractedQuestions node {0}", uid);
        return false;
    }
}

public class SanitizeQuestionsResult
{
    public bool Sanitized { get; set; }
    public string Error { get; set; }
}

