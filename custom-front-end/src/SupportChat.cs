using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using H5.Core;
using Mosaik.Components;
using Mosaik.Schema;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;
using UID;
using System.Linq;
using Mosaik;

namespace TechnicalSupport.FrontEnd
{
    internal class SupportChat : IComponent
    {
        private readonly ChatAIView _chatView;
        public dom.HTMLElement Render() => _chatView.Render();
        private const string CONTEXT_FIELD = "SUPPORT_CONTEXT";
        public SupportChat(Parameters state)
        {
            _chatView = ChatAIView.New(SupportChat.Endpoints, state)
                                  .WithCustomHeader(CreateChatHeader)
                                  .WithCustomExamples(CreateChatExamples)
                                  .WithCustomChatContextRenderer(CustomizeChatContext)
                                  .WithCustomMessageRenderer(CustomizeChatMessages)
                                  .WithMessageActions(RenderMessageActions)
                                  .WithCustomToolResultRenderer(RenderTools);
        }

        private static bool TryGetCustomState(ChatMetadata metadata, out SupportChatContext supportChatContext)
        {
            if (metadata.HasOwnProperty(CONTEXT_FIELD))
            {
                supportChatContext = metadata[CONTEXT_FIELD].As<SupportChatContext>();
                return true;
            }
            supportChatContext = null;
            return false;
        }

        private static void StoreContext(ChatMetadata metadata, SupportChatContext supportChatContext)
        {
            metadata[CONTEXT_FIELD] = supportChatContext;
        }

        private ChatAIView.ChatContextComponent CustomizeChatContext(ChatMetadata metadata, SettableObservable<UID128AndPage> vieweing)
        {
            //metadata can be null on new empty chats.
            //In this case, we can create a new chat manually as needed to store the context, and then set the current chat to it

            var contextForChat = Tesserae.UI.Defer(async () =>
            {
                SupportChatContext ctx;
                if (metadata is object)
                {
                    if (!TryGetCustomState(metadata, out ctx))
                    {
                        ctx = await LoadOrInitializeContextForChatAsync(metadata.UID);
                        StoreContext(metadata, ctx);
                    }
                }
                else
                {
                    ctx = new SupportChatContext()
                    {
                        Topic = "All"
                    };
                }

                var topic = new SettableObservable<string>(ctx.Topic);

                var dropdown = Dropdown().Items(ItemFor("All",         topic),
                                                ItemFor("Smartphones", topic),
                                                ItemFor("Laptops",     topic),
                                                ItemFor("Cameras",     topic));

                topic.ObserveFutureChanges(newTopic =>
                {
                    ctx.Topic = newTopic;
                    StoreContext(metadata, ctx);
                    StoreChatContext(metadata?.UID, ctx).FireAndForget();
                });

                var content  = HStack().WS().AlignItemsCenter();
                content.Add(Label("Topic:").Inline().SetContent(dropdown));
                return content;
            });

            return ChatAIView.ChatContext(contextForChat, hasContext: true);

            Dropdown.Item ItemFor(string topic, SettableObservable<string> observable)
            {
                return DropdownItem(topic).SelectedIf(observable.Value == topic).OnSelected(_ => observable.Value = topic);
            }
        }

        private async Task StoreChatContext(UID128 chatUID, SupportChatContext ctx)
        {
            if (UID128.IsNull(chatUID))
            {
                var newChat = await Mosaik.API.ChatAI.NewChat(App.InterfaceSettings.ChatAIProvider?.TaskUID, App.InterfaceSettings.SelectedAIAssistantTemplate?.UID);

                await Mosaik.API.Endpoints.CallAsync<SupportChatContext>("support-chat/set-context", new SupportChatSetContextRequest()
                {
                    ChatUID = chatUID,
                    Context = ctx
                });
                
                StoreContext(newChat, ctx);

                _chatView.SelectChat(newChat);
            }
            else
            {
                await Mosaik.API.Endpoints.CallAsync<SupportChatContext>("support-chat/set-context", new SupportChatSetContextRequest()
                {
                    ChatUID = chatUID,
                    Context = ctx
                });
            }
        }

        private static async Task<SupportChatContext> LoadOrInitializeContextForChatAsync(UID128 chatUID)
        {
            return await Mosaik.API.Endpoints.CallAsync<SupportChatContext>("support-chat/get-context", chatUID);
        }

        private IComponent CreateChatHeader(SelectAIAssistantTemplateDropdown dropdown)
        {
            return VStack().AlignItemsCenter().WS().Children( 
                        Icon(UIcons.ChatbotSpeechBubble, size:TextSize.Large).PB(8),
                        TextBlock("Welcome to our Support Chat").SemiBold().WS().TextCenter());
        }

        private void CreateChatExamples(ChatMetadata metadata, Stack stack, TextArea area, Button sendButton)
        {
            //TODO: Implement examples for chat based on current context
        }

        private IComponent CustomizeChatMessages(ChatMetadata metadata, ChatAI_Message message, IComponent component)
        {
            return component.Class("support-chat-message");
        }

        private IEnumerable<IComponent> RenderMessageActions(ChatMetadata metadata, ChatAI_Message message)
        {
            if (message.Author == FixedUIDs.AssistantAuthor) //Only for assistant messages
            {
                yield return ChatAIView.MessageAction(UIcons.ThumbsUp  ).Tooltip("Positive Feedback").OnClickSpinWhile(() => CaptureFeedback(metadata.UID, message.UID, positive: true));
                yield return ChatAIView.MessageAction(UIcons.ThumbsDown).Tooltip("Negative Feedback").OnClickSpinWhile(() => CaptureFeedback(metadata.UID, message.UID, positive: false));
            }
        }

        private async Task CaptureFeedback(UID128 chatUID, UID128 messageUID, bool positive)
        {
            //TODO: Store feedback on server
            if (positive)
            {
                Toast().Success("Thanks for your positive feedback");
            }
            else
            {
                Toast().Warning("Sorry to hear about the feedback");
            }
        }

        private IComponent RenderTools(ChatToolCall call)
        {
            return TextBlock($"Tool Call: {call.ToolName}");
        }

        public static ChatEndpoints Endpoints { get; } = new ChatEndpoints()
        {
            PostMessage = PostSupportMessage
        };

        private static async Task<UID128> PostSupportMessage(ChatEndpoints.PostMessageRequest request)
        {
            if (!TryGetCustomState(request.ActiveChat, out var ctx))
            {
                ctx = await LoadOrInitializeContextForChatAsync(request.ActiveChat.UID);
                StoreContext(request.ActiveChat, ctx);
            }

            return await Mosaik.API.Endpoints.CallAsync<UID128>("support-chat/post-message", new SupportChatMessageRequest()
            {
                Message = request.Message,
                ChatUID = request.ActiveChat.UID,
                Tools = request.ActiveTools,
                ViewingUID = request.ViewingUID,
                Context = ctx,
            });
            
            //The endpoint above replaces the following default internal API:
            // return await Mosaik.API.ChatAI.PostMessage(request.Context.ChatUID, request.Message, useTools: request.ActiveTools, cancellationToken: request.CancellationToken);
        }

    }

    public class SupportChatMessageRequest
    {
        public string Message { get; set; }
        public UID128 ChatUID { get; set; }
        public UID128 ViewingUID { get; set; }
        public UID128[] Tools { get; set; }
        public SupportChatContext Context { get; set; }
    }
    public class SupportChatSetContextRequest
    {
        public UID128 ChatUID { get; set; }
        public SupportChatContext Context { get; set; }
    }

    public class SupportChatContext
    {
        public string Topic { get; set;  }
    }
}