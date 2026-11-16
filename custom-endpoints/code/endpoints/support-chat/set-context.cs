[endpoint: Curiosity.Endpoints.Path("support-chat/set-context")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

// Stores the topic of a support chat, in a SupportChatContext node keyed by the chat's UID.

var request = ParseBody<SupportChatSetContextRequest>();

if (!Graph.HasNodeOfType(request.ChatUID, _MessageChannel.Type)) return BadRequest("Expected the UID of a chat");

if (!ChatAI.TryGetUserForChat(request.ChatUID, out var owner) || owner != CurrentUser) return Forbid();

var contextNode = await Graph.GetOrAddLockedAsync(N.SupportChatContext.Type, request.ChatUID.ToString());

contextNode.SetString(N.SupportChatContext.Topic, request.Context.Topic);

await Graph.CommitAsync(contextNode);

return Ok();

public class SupportChatSetContextRequest
{
    public UID128             ChatUID { get; set; }
    public SupportChatContext Context { get; set; }
}

public class SupportChatContext
{
    public string Topic { get; set; }
}
