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

namespace TechnicalSupport.FrontEnd
{
    internal class SupportChat : IComponent
    {
        private readonly ChatAIView _chatView;
        public dom.HTMLElement Render() => _chatView.Render();

        public SupportChat(Parameters state)
        {
            _chatView = ChatAIView.New(SupportChat.Endpoints, state)
                                  .WithCustomHeader(CreateChatHeader)
                                  .WithCustomExamples(CreateChatExamples)
                                  .WithCustomMessageRendering(CustomizeChatMessages)
                                  .WithMessageActions(RenderMessageActions)
                                  .WithCustomToolRendering(RenderTools);
        }

        private IComponent CreateChatHeader(SelectAIAssistantTemplateDropdown dropdown)
        {
            return VStack().WS().Children( 
                        Icon(UIcons.ChatbotSpeechBubble, size:TextSize.Large).PB(8),
                        TextBlock("Welcome to our Support Chat").WS().TextCenter());
        }

        private void CreateChatExamples(ChatMetadata metadata, Stack stack, TextArea area, Button sendButton)
        {
            //TODO: Implement
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
            return await Mosaik.API.Endpoints.CallAsync<UID128>("post-chat-message", new ChatMessageRequest()
            {
                Message = request.Message,
                ChatUID = request.Context.ChatUID,
                Tools = request.ActiveTools,
                ViewingUID = request.ViewingUID
            });
            
            //The endpoint above replaces the following default internal API:
            // return await Mosaik.API.ChatAI.PostMessage(request.Context.ChatUID, request.Message, useTools: request.ActiveTools, cancellationToken: request.CancellationToken);
        }

        public class ChatMessageRequest
        {
            public string Message { get; set; }
            public UID128 ChatUID { get; set; }
            public UID128 ViewingUID { get; set; }
            public UID128[] Tools { get; set; }
        }
    }
}