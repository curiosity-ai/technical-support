[tools: Curiosity.ChatAITools.UID("ExtracTQ11111111111111")]
[tools: Curiosity.ChatAITools.DisplayName("Extract Support Questions")]
[tools: Curiosity.ChatAITools.Description("Extracts the distinct questions a support agent asked the customer in a support conversation, plus a short topic. Returns JSON.")]
[tools: Curiosity.ChatAITools.Icon("fi-rr-interrogation")]
[tools: Curiosity.ChatAITools.AccessMode("AllUsers")]

public class ExtractSupportQuestionsTool
{
    [Tool("Read a support conversation transcript and extract the distinct questions the SUPPORT agent asked the customer, plus a short topic for the conversation. Returns a JSON object of the form {\"questions\":[\"...\"],\"topic\":\"...\"}.")]
    public static async Task<string> ExtractQuestions(ToolScope scope,
          [Parameter("The full support conversation transcript, with each line prefixed by its author (e.g. 'Support: ...' / 'User: ...').", required: true)] string conversation)
    {
        if (string.IsNullOrWhiteSpace(conversation)) return "{\"questions\":[],\"topic\":\"\"}";

        var systemPrompt =
            "You analyze technical-support conversations. Identify every distinct question the SUPPORT agent "
          + "asked the customer (ignore questions the customer asked, and ignore rhetorical or purely conversational "
          + "phrases). Deduplicate near-identical questions. Also produce a short (2-5 word) topic describing what the "
          + "conversation is about. Respond with ONLY a single JSON object, no markdown fences and no extra text, in "
          + "the exact form {\"questions\":[\"question 1\",\"question 2\"],\"topic\":\"short topic\"}. "
          + "If the support agent asked no questions, return an empty questions array.";

        var prompts = new List<Mosaik.AI.IChatAIMessage>();
        prompts.Add(new Mosaik.AI.ChatAIMessage(Mosaik.AI.ChatAuthorRole.System, systemPrompt));
        prompts.Add(new Mosaik.AI.ChatAIMessage(Mosaik.AI.ChatAuthorRole.User, conversation));

        var completion = await scope.ChatAI.GetCompletionAsync(scope.CurrentUser, prompts);

        scope.SetToolCallDisplayName("Extract support questions");

        return completion.Content;
    }
}

return new ExtractSupportQuestionsTool();
