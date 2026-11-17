[endpoint: Curiosity.Endpoints.Path("support-chat/get-context")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]
[endpoint: Curiosity.Endpoints.ReadOnly]

// The topic stored for a support chat (a SupportChatContext node keyed by the chat's UID), or "All".

var chatUID = ParseBody<UID128>();

if (Graph.HasNodeOfType(chatUID, _MessageChannel.Type)
 && ChatAI.TryGetUserForChat(chatUID, out var owner) && owner == CurrentUser
 && Graph.TryGet(N.SupportChatContext.Type, chatUID.ToString(), out var contextNode))
{
    return new SupportChatContext { Topic = contextNode.GetString(N.SupportChatContext.Topic) };
}

return new SupportChatContext { Topic = "All" };

public class SupportChatContext
{
    public string Topic { get; set; }
}
