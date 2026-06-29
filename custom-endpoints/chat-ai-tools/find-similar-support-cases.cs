[tools: Curiosity.ChatAITools.UID("SupporTSimiLarCases111")]
[tools: Curiosity.ChatAITools.DisplayName("Find Similar Support Cases")]
[tools: Curiosity.ChatAITools.Description("Search past support cases (open and resolved) by a problem description and return the most similar ones, including the device, the case id, status and the full conversation, so a support worker can reuse known fixes. Call this whenever the user describes a device problem or asks how a similar case was handled before answering.")]
[tools: Curiosity.ChatAITools.Icon("fi-rr-comments-question")]
[tools: Curiosity.ChatAITools.AccessMode("AllUsers")]

// Support-worker context tool. Replaces the old custom Support chat view that
// injected a topic system prompt via the support-chat/* endpoints: instead of a
// fixed prompt, the assistant pulls the relevant cases on demand.
//
// Enable this for the AI Assistant by default in the workspace AI tool / assistant
// settings (there is no per-tool "default on" directive).
public class SupportCasesTool
{
    private const int MAX_MESSAGES_PER_CASE = 12;

    [Tool("Search support cases by a problem description and return the most similar cases with their device, id, status and conversation. Use this to find how similar problems were resolved before drafting an answer.")]
    public static async Task<string> FindSimilarCases(ToolScope scope,
          [Parameter("A description of the problem or question to find similar support cases for", required: true)] string query,
          [Parameter("Maximum number of cases to return (default: 5)", required: false)] int limit = 5)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return "[]";
        }

        if (limit <= 0) limit = 5;

        var searchReq = SearchRequest.For(query);
        searchReq.SortMode = SortModeEnum.Relevance;
        searchReq.UserUID = scope.CurrentUser;
        searchReq.BeforeTypesFacet = new HashSet<string>
        {
            N.SupportCase.Type
        };

        var search = await scope.Graph.CreateSearchAsUserAsync(searchReq, searchReq.UserUID, scope.CancellationToken);

        var results = new List<object>();

        foreach (var caseNode in search.Take(limit).AsEnumerable())
        {
            string device = null;
            foreach (var deviceNode in scope.Graph.Query().StartAt(caseNode.UID).Out(N.Device.Type, E.ForDevice).AsEnumerable())
            {
                device = deviceNode.GetString(N.Device.Name);
                break;
            }

            var conversation = new List<object>();
            foreach (var messageNode in scope.Graph.Query().StartAt(caseNode.UID).Out(N.SupportCaseMessage.Type, E.HasMessage).Take(MAX_MESSAGES_PER_CASE).AsEnumerable())
            {
                conversation.Add(new
                {
                    author = messageNode.GetString(N.SupportCaseMessage.Author),
                    message = messageNode.GetString(N.SupportCaseMessage.Message)
                });
            }

            results.Add(new
            {
                id = caseNode.GetString(N.SupportCase.Id),
                summary = caseNode.GetString(N.SupportCase.SupportCaseSummary),
                status = caseNode.GetString(N.SupportCase.Status),
                device,
                conversation
            });
        }

        scope.SetToolCallDisplayName($"Similar cases for '{query}'");

        return results.ToJson();
    }
}

return new SupportCasesTool();
