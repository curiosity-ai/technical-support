[tools: Curiosity.ChatAITools.UID("SaniTizeQ1111111111111")]
[tools: Curiosity.ChatAITools.DisplayName("Sanitize Support Questions")]
[tools: Curiosity.ChatAITools.Description("Removes personally identifiable information (PII) from a list of support questions and a topic. Returns JSON.")]
[tools: Curiosity.ChatAITools.Icon("fi-rr-shield-check")]
[tools: Curiosity.ChatAITools.AccessMode("AllUsers")]

public class SanitizeSupportQuestionsTool
{
    [Tool("Remove personally identifiable information (PII) from a list of support questions and a topic. PII includes names, email addresses, phone numbers, physical addresses, account/order/serial numbers, and any other identifying values; replace each with a neutral placeholder such as [NAME], [EMAIL], [PHONE], [ADDRESS] or [ID] while keeping the question readable. Input 'questionsJson' is a JSON array of strings. Returns a JSON object of the form {\"sanitizedQuestions\":[\"...\"],\"sanitizedTopic\":\"...\"}.")]
    public static async Task<string> SanitizeQuestions(ToolScope scope,
          [Parameter("A JSON array of the questions to sanitize, e.g. [\"question 1\",\"question 2\"].", required: true)] string questionsJson,
          [Parameter("The topic to sanitize.", required: false)] string topic = "")
    {
        if (string.IsNullOrWhiteSpace(questionsJson)) return "{\"sanitizedQuestions\":[],\"sanitizedTopic\":\"\"}";

        var systemPrompt =
            "You sanitize technical-support text by removing personally identifiable information (PII). "
          + "PII includes people's names, email addresses, phone numbers, physical/postal addresses, and "
          + "account, order, serial or ticket numbers. Replace each occurrence with a neutral placeholder such "
          + "as [NAME], [EMAIL], [PHONE], [ADDRESS] or [ID], preserving the meaning and readability of each "
          + "question. Do not add, drop or reorder questions. Respond with ONLY a single JSON object, no markdown "
          + "fences and no extra text, in the exact form "
          + "{\"sanitizedQuestions\":[\"question 1\",\"question 2\"],\"sanitizedTopic\":\"short topic\"}.";

        var userPrompt = "Topic: " + (topic ?? "") + "\nQuestions (JSON array): " + questionsJson;

        var prompts = new List<Mosaik.AI.IChatAIMessage>();
        prompts.Add(new Mosaik.AI.ChatAIMessage(Mosaik.AI.ChatAuthorRole.System, systemPrompt));
        prompts.Add(new Mosaik.AI.ChatAIMessage(Mosaik.AI.ChatAuthorRole.User, userPrompt));

        var completion = await scope.ChatAI.GetCompletionAsync(scope.CurrentUser, prompts);

        scope.SetToolCallDisplayName("Sanitize support questions");

        return completion.Content;
    }
}

return new SanitizeSupportQuestionsTool();
