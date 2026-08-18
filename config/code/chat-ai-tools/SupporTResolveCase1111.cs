[tools: Curiosity.ChatAITools.UID("SupporTResolveCase1111")]
[tools: Curiosity.ChatAITools.DisplayName("Resolve Support Case")]
[tools: Curiosity.ChatAITools.Description("Change the status of a support case: mark it resolved (status 'Closed') once the customer's problem is fixed, or reopen it (status 'Open') when a closed case needs more work. Use this only after a concrete fix has been agreed with the customer, and always confirm the exact case id first.")]
[tools: Curiosity.ChatAITools.Icon("fi-rr-check-circle")]
[tools: Curiosity.ChatAITools.AccessMode("AllUsers")]

// Companion to find-similar-support-cases.cs and support-graph-lookup.cs. While
// those tools only read the graph, this one lets the assistant act on a case by
// updating its status, so a support worker chatting about a case can close it (or
// reopen it) directly from the conversation.
//
// Enable this for the AI Assistant by default in the workspace AI tool / assistant
// settings (there is no per-tool "default on" directive). The support case chat in
// the custom front-end already pre-selects it when opened for a specific case.
public class ResolveSupportCaseTool
{
    [Tool("Mark a support case as resolved by setting its status to 'Closed'. Use once the customer's problem has been fixed. Returns the previous and new status.")]
    public static async Task<string> ResolveSupportCase(ToolScope scope,
          [Parameter("The support case id to resolve, e.g. 'SC-36556'", required: true)] string caseId)
    {
        return await SetStatusAsync(scope, caseId, "Closed");
    }

    [Tool("Reopen a support case by setting its status to 'Open'. Use when a closed case turns out to need more work. Returns the previous and new status.")]
    public static async Task<string> ReopenSupportCase(ToolScope scope,
          [Parameter("The support case id to reopen, e.g. 'SC-36556'", required: true)] string caseId)
    {
        return await SetStatusAsync(scope, caseId, "Open");
    }

    private static async Task<string> SetStatusAsync(ToolScope scope, string caseId, string status)
    {
        if (string.IsNullOrWhiteSpace(caseId))
        {
            return new { error = "A case id is required." }.ToJson();
        }

        caseId = caseId.Trim();

        // Confirm the case exists before locking it for a write.
        var existing = scope.Graph.Query().StartAt(N.SupportCase.Type, caseId).AsEnumerable().FirstOrDefault();
        if (existing is null)
        {
            return new { error = $"No support case found with id '{caseId}'." }.ToJson();
        }

        var previousStatus = existing.GetString(N.SupportCase.Status);

        var caseNode = await scope.Graph.GetOrAddLockedAsync(N.SupportCase.Type, caseId);
        caseNode.SetString(N.SupportCase.Status, status);
        await scope.Graph.CommitAsync(caseNode);

        var resolved = status == "Closed";
        scope.SetToolCallDisplayName($"{(resolved ? "Resolved" : "Reopened")} {caseId}");

        return new
        {
            id = caseId,
            summary = existing.GetString(N.SupportCase.SupportCaseSummary),
            previousStatus,
            status,
            resolved
        }.ToJson();
    }
}

return new ResolveSupportCaseTool();
