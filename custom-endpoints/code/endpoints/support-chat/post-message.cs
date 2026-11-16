[endpoint: Curiosity.Endpoints.Path("support-chat/post-message")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

// Posts the user's message (or replaces it, for "Edit" and "Regenerate") and runs the assistant's turn, with a
// system prompt built from the topic the user picked in the support chat.

var request = ParseBody<SupportChatMessageRequest>();

if (!ChatAI.TryGetUserForChat(request.ChatUID, out var owner) || owner != CurrentUser) return Forbid();

var systemPrompt = $"You're an AI assistent specialized in {request.Context?.Topic ?? "All"}.";

var messageUID = await ChatAI.AddUserMessageAsync(request.ChatUID, CurrentUser, request.Message,
                                                  replaceMessageUID: request.ReplaceMessageUID,
                                                  attachmentUIDs:    request.Attachments);

// The turn relays what a user wrote, so the workspace's chat policies apply to it.
await ChatAI.TriggerChatAsync(request.ChatUID, CurrentUser, messageUID,
                              enabledTools:  request.Tools?.Select(uid => new ToolUID(uid)).ToArray(),
                              enabledAgents: request.Agents?.Select(uid => new AgentUID(uid)).ToArray(),
                              enabledSkills: request.Skills?.Select(uid => new SkillUID(uid)).ToArray(),
                              systemPrompt:  systemPrompt,
                              enforcePolicy: true);

return messageUID;

public class SupportChatMessageRequest
{
    public string             Message           { get; set; }
    public UID128             ChatUID           { get; set; }
    public UID128             ReplaceMessageUID { get; set; }
    public UID128[]           Tools             { get; set; }
    public UID128[]           Agents            { get; set; }
    public UID128[]           Skills            { get; set; }
    public UID128[]           Attachments       { get; set; }
    public SupportChatContext Context           { get; set; }
}

public class SupportChatContext
{
    public string Topic { get; set; }
}
