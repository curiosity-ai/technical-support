[endpoint: Curiosity.Endpoints.Path("support-chat/set-context")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

var request = Body.FromJson<SupportChatSetContextRequest>();

if (Graph.HasNodeOfType(request.ChatUID, _MessageChannel.Type))
{
    var id = request.ChatUID.ToString();

    var contextNode = await Graph.GetOrAddLockedAsync(N.SupportChatContext.Type, id);
    contextNode.SetString(N.SupportChatContext.Topic, request.Context.Topic);
    await Graph.CommitAsync(contextNode);
}

public class SupportChatSetContextRequest
{
    public UID128 ChatUID { get; set; }
    public SupportChatContext Context { get; set; }
}

public class SupportChatContext
{
    public string Topic { get; set; }
}
