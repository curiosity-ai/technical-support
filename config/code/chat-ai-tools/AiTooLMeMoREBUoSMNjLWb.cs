[tools: Curiosity.ChatAITools.UID("AiTooLMeMoREBUoSMNjLWb")]
[tools: Curiosity.ChatAITools.DisplayName("Memory")]
[tools: Curiosity.ChatAITools.Description("Store a new memory for the user. Use this when the user explicitly asks to remember something or when important personal information is shared.")]
[tools: Curiosity.ChatAITools.Icon("fi-rr-brain")]
[tools: Curiosity.ChatAITools.AccessMode("AllUsers")]

public class MemoryTool
{
    [Tool("Store a new memory for the user. Use this when the user explicitly asks to remember something or when important personal information is shared.")]
    public static async Task<string> StoreMemory(ToolScope scope,
          [Parameter("The content of the memory to store", required: true)] string memoryContent)
    {
        if (string.IsNullOrWhiteSpace(memoryContent)) return "[]";
        await scope.ChatAI.StoreMemoryAsync(scope.CurrentUser, memoryContent);
        return new { ok = true, message = "Memory stored successfully." }.ToJson();
    }

    [Tool("Retrieve memories relevant to the given query. Use this to recall information about the user.")]
    public static async Task<string> RecallMemory(ToolScope scope,
          [Parameter("The query to search for relevant memories", required: true)] string query,
          [Parameter("Maximum number of memories to retrieve (default: 5)", required: false)] int limit = 5)
    {
        if (string.IsNullOrWhiteSpace(query)) return "[]";
        var memories = await scope.ChatAI.RetrieveMemoriesAsync(scope.CurrentUser, query, limit);
        return memories.Select(m => new { content = m.Content, score = m.Score }).ToJson();
    }

    [Tool("List all stored memories for the user. useful for managing or reviewing what is stored.")]
    public static async Task<string> ListMemories(ToolScope scope)
    {
        var memories = await scope.ChatAI.ListMemories(scope.CurrentUser);
        return memories.Select(m => new { uid = m.UID, content = m.Content }).ToJson();
    }

    [Tool("Forget a specific memory by its UID.")]
    public static async Task<string> ForgetMemory(ToolScope scope,
          [Parameter("The UID of the memory to delete", required: true)] string memoryUID)
    {
        if (UID128.TryParse(memoryUID, out var uid))
        {
            await scope.ChatAI.DeleteMemoryAsync(scope.CurrentUser, uid);
            return new { ok = true, message = "Memory deleted successfully." }.ToJson();
        }
        return new { ok = false, error = "Invalid memory UID." }.ToJson();
    }
}

return new MemoryTool();

