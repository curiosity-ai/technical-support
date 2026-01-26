[endpoint: Curiosity.Endpoints.Path("support-chat/post-message")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

var request = Body.FromJson<SupportChatMessageRequest>();

var systemPrompt = $"You're an AI assistent specialized in {request.Context.Topic}.";
var messageUID = await ChatAI.AddUserMessageAsync(request.ChatUID, CurrentUser, request.Message);
await ChatAI.TriggerChatAsync(request.ChatUID, CurrentUser, messageUID, enabledTools: request.Tools, systemPrompt: systemPrompt);


public class SupportChatMessageRequest
{
    public string Message { get; set; }
    public UID128 ChatUID { get; set; }
    public UID128 ViewingUID { get; set; }
    public UID128[] Tools { get; set; }
    public SupportChatContext Context { get; set; }
}

public class SupportChatContext
{
    public string Topic { get; set; }
}
