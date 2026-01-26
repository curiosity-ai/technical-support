[endpoint: Curiosity.Endpoints.Path("support-chat/get-context")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

var chatUID = UID128.Parse(Body.Trim('"'));

if (Graph.HasNodeOfType(chatUID, _MessageChannel.Type))
{
    var id = chatUID.ToString();

    if (Graph.TryGet(N.SupportChatContext.Type, id, out var contextNode))
    {
        return new SupportChatContext()
        {
            Topic = contextNode.GetString(N.SupportChatContext.Topic)
        };
    }
}

return new SupportChatContext()
{
    Topic = "All"
};

public class SupportChatContext
{
    public string Topic { get; set; }
}
