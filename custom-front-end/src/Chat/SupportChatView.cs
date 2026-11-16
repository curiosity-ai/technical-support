using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mosaik;
using Mosaik.Components;
using Mosaik.Helpers;
using Mosaik.Schema;
using Mosaik.Views;
using Tesserae;
using TNT;
using UID;
using static Tesserae.UI;
using static Mosaik.UI;
using static TNT.T;
using static Transpose.Core.dom;
using API  = Mosaik.API;
using Node = Mosaik.Schema.Node;


namespace TechnicalSupport.FrontEnd
{
    /// <summary>
    /// The support chat: the product's ChatView, answering through the support-chat/post-message endpoint so every
    /// turn carries the topic the user picked above the composer.
    /// </summary>
    public sealed class SupportChatView : IComponent
    {
        internal const string Route = "#/support";

        private static readonly string[] Topics = { "All", "Smartphones", "Laptops", "Cameras" };

        private const string DEFAULT_TOPIC = "All";

        // The topic of each chat this page has seen, so a turn does not have to ask the server for it again.
        private readonly Dictionary<UID128, SupportChatContext> _contexts = new Dictionary<UID128, SupportChatContext>();

        private readonly ChatView _chatView;

        public SupportChatView(Parameters state)
        {
            var configuration = new ChatViewConfiguration
            {
                CustomHeader             = CreateChatHeader,
                CustomChatContext        = CreateChatContext,
                CustomMessageRenderer    = (chat, message, component) => component.Class("support-chat-message"),
                MessageCommands          = CreateMessageCommands,
                CustomToolResultRenderer = (chat, toolCall) => TextBlock(string.Format("Tool Call: {0}".t(), toolCall.ToolName)),

                // Both hooks go through the endpoint: PostMessage for a new message, ReplaceMessage for what
                // "Edit" and "Regenerate" send. Overriding only the first would run an edited turn without the topic.
                PostMessage    = request => PostSupportMessageAsync(request, replaceMessageUID: null),
                ReplaceMessage = request => PostSupportMessageAsync(request, replaceMessageUID: request.MessageUID),
            };

            _chatView = ShownView.Track(this, new ChatView(configuration, state));
        }

        internal static bool IsShowing() => ShownView.Is<SupportChatView>();

        public HTMLElement Render() => _chatView.Render();

        private static IComponent CreateChatHeader(SelectAIAssistantTemplateDropdown dropdown)
        {
            return VStack().AlignItemsCenter().WS().Children(
                Icon(UIcons.ChatbotSpeechBubble, size: TextSize.Large).PB(8),
                TextBlock("Welcome to our Support Chat".t()).SemiBold().WS().TextCenter());
        }

        private static IEnumerable<MessageCommand> CreateMessageCommands(CurrentChat chat, Mosaik.Schema.ChatMessage message)
        {
            if (message.Author != FixedUIDs.AssistantAuthor) yield break; //Only for assistant messages

            yield return new MessageCommand(UIcons.ThumbsUp,   "Positive Feedback".t()).OnClick(() => CaptureFeedback(positive: true));
            yield return new MessageCommand(UIcons.ThumbsDown, "Negative Feedback".t()).OnClick(() => CaptureFeedback(positive: false));
        }

        private static void CaptureFeedback(bool positive)
        {
            //TODO: Store feedback on server
            if (positive)
            {
                Toast().Success("Thanks for your positive feedback".t());
            }
            else
            {
                Toast().Warning("Sorry to hear about the feedback".t());
            }
        }

        /// <summary>The topic picker above the composer. A new, empty chat has no node yet: picking a topic there creates the chat.</summary>
        private IComponent CreateChatContext(CurrentChat currentChat, SettableObservable<ViewingContent> viewing, Tesserae.OmniBox omniBox)
        {
            var chat = currentChat?.Chat;

            return Defer(async () =>
            {
                var context = chat is object
                    ? await GetContextAsync(chat.UID)
                    : new SupportChatContext { Topic = DEFAULT_TOPIC };

                var topic = new SettableObservable<string>(context.Topic);

                var dropdown = Dropdown().Items(Topics.Select(name => DropdownItem(name.t()).SelectedIf(topic.Value == name).OnSelected(_ => topic.Value = name)).ToArray());

                topic.ObserveFutureChanges(newTopic =>
                {
                    context.Topic = newTopic;
                    StoreContextAsync(chat, context).FireAndForget();
                });

                return HStack().WS().AlignItemsCenter().Children(Label("Topic:".t()).Inline().SetContent(dropdown));
            });
        }

        private async Task<SupportChatContext> GetContextAsync(UID128 chatUID)
        {
            if (!_contexts.TryGetValue(chatUID, out var context))
            {
                context = await SupportApi.GetChatContextAsync(chatUID);
                _contexts[chatUID] = context;
            }

            return context;
        }

        private async Task StoreContextAsync(ChatMetadata chat, SupportChatContext context)
        {
            if (chat is object)
            {
                _contexts[chat.UID] = context;
                await SupportApi.SetChatContextAsync(chat.UID, context);
                return;
            }

            var newChat = await API.ChatAI.NewChat(App.InterfaceSettings.ChatAIProvider?.TaskUID, App.InterfaceSettings.SelectedAIAssistantTemplate?.UID);

            _contexts[newChat.UID] = context;
            await SupportApi.SetChatContextAsync(newChat.UID, context);

            _chatView.OpenChat(newChat);
        }

        private async Task<UID128> PostSupportMessageAsync(ChatViewConfiguration.PostMessageRequest request, UID128 replaceMessageUID)
        {
            var context = await GetContextAsync(request.ActiveChat.UID);

            // The endpoint replaces the default API.ChatAI.PostMessage / ReplaceMessage, so it receives everything those would.
            return await SupportApi.PostChatMessageAsync(new SupportChatMessageRequest
            {
                Message           = request.Message,
                ChatUID           = request.ActiveChat.UID,
                ReplaceMessageUID = replaceMessageUID,
                Tools             = request.ActiveTools,
                Agents            = request.ActiveAgents,
                Skills            = request.ActiveSkills,
                Attachments       = request.AttachmentUIDs,
                Context           = context,
            });
        }
    }
}
