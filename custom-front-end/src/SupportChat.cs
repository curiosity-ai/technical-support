using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using H5.Core;
using Mosaik;
using Mosaik.Components;
using Mosaik.Schema;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;
using UID;
using System.Linq;
using Newtonsoft.Json;

namespace TechnicalSupport.FrontEnd
{
    // Custom Support chat: the standard ChatAIView with a support welcome,
    // example prompts, feedback actions and a rich renderer for the support
    // tool results. The support context comes from the ChatAI tools (enabled
    // by default below), not from a topic system prompt.
    internal class SupportChat : IComponent
    {
        private readonly ChatAIView _chatView;
        public dom.HTMLElement Render() => _chatView.Render();

        public SupportChat(Parameters state)
        {
            var endpoints = new CustomChatView();

            // Enable the workspace's AI tools by default so a support worker gets
            // graph / similar-case lookups without enabling them for every chat.
            var listAvailableTools = endpoints.ListTools;
            endpoints.ListTools = async (context) =>
            {
                var tools = await listAvailableTools(context);
                foreach (var tool in tools)
                {
                    tool.InitiallySelected = true;
                }
                return tools;
            };

            _chatView = ChatView(endpoints, state)
                             .WithCustomHeader(CreateChatHeader)
                             .WithCustomExamples(CreateChatExamples)
                             .WithCustomMessageRenderer(CustomizeChatMessages)
                             .WithMessageCommands(CreateMessageCommands)
                             .WithCustomToolResultRenderer(RenderTools);
        }

        private IComponent CreateChatHeader(SelectAIAssistantTemplateDropdown dropdown)
        {
            return VStack().AlignItemsCenter().WS().Children(
                        Icon(UIcons.ChatbotSpeechBubble, size: TextSize.Large).PB(8),
                        TextBlock("Support assistant").SemiBold().WS().TextCenter(),
                        TextBlock("Ask about a device, part or case — I can search past support cases and the knowledge graph.")
                            .Secondary().WS().TextCenter().PT(4));
        }

        private bool CreateChatExamples(CurrentChat chat, Stack stack, TextArea area, ChatAISendStopButton button, bool arg5)
        {
            var examples = new[]
            {
                "What's the known fix for screen flicker at low brightness after a firmware update?",
                "Find similar cases for a phone that won't power on.",
                "Look up the Samsung Galaxy A53 5G — what parts and cases does it have?"
            };

            var list = VStack().WS().AlignItemsCenter().Class("support-chat-examples");
            foreach (var example in examples)
            {
                var text = example;
                list.Add(Button().Class("support-chat-example")
                            .ReplaceContent(TextBlock(text).WS().TextLeft())
                            .OnClick(() => area.Text = text));
            }
            stack.Add(list);
            return true;
        }

        private IComponent CustomizeChatMessages(CurrentChat currentChat, Mosaik.Schema.ChatMessage message, IComponent component)
        {
            return component.Class("support-chat-message");
        }

        private IEnumerable<MessageCommand> CreateMessageCommands(CurrentChat chat, Mosaik.Schema.ChatMessage message)
        {
            if (message.Author == FixedUIDs.AssistantAuthor) // Only for assistant messages
            {
                yield return new MessageCommand(UIcons.ThumbsUp, "Positive Feedback").OnClick(() => CaptureFeedback(positive: true));
                yield return new MessageCommand(UIcons.ThumbsDown, "Negative Feedback").OnClick(() => CaptureFeedback(positive: false));
            }
        }

        private void CaptureFeedback(bool positive)
        {
            // Feedback persistence is a server-side TODO — acknowledge with a toast for now.
            if (positive)
            {
                Toast().Success("Thanks for your positive feedback");
            }
            else
            {
                Toast().Warning("Sorry to hear that — we'll use this to improve");
            }
        }

        // Renders a support tool result as a styled card: tool name + a count badge,
        // and one row per case when the result is a list of cases (find-similar /
        // device cases). Falls back to a simple header for other tools.
        private IComponent RenderTools(CurrentChat currentChat, ChatToolCall chatToolCall)
        {
            var name = string.IsNullOrEmpty(chatToolCall.DisplayName) ? chatToolCall.ToolName : chatToolCall.DisplayName;

            var header = HStack().AlignItemsCenter().Class("support-tool-call-header").Children(
                            Icon(UIcons.Bolt).Class("support-tool-call-icon"),
                            TextBlock(name).Class("support-tool-call-name"));

            var card = VStack().Class("support-tool-call").Children(header);

            var rows = TryParseCases(chatToolCall.ResultContent);
            if (rows != null && rows.Count > 0)
            {
                header.Add(Empty().Grow());
                header.Add(TextBlock($"{rows.Count} match{(rows.Count == 1 ? "" : "es")}").Class("support-tool-call-badge"));

                var list = VStack().WS().Class("support-tool-call-list");
                foreach (var row in rows)
                {
                    var meta = HStack().AlignItemsCenter().Class("support-tool-call-row-meta").Children(
                                    TextBlock(row.id).Class("cz-meta-mono"));
                    if (!string.IsNullOrEmpty(row.device)) meta.Add(TextBlock(row.device).Class("support-tool-call-device"));
                    if (!string.IsNullOrEmpty(row.status)) meta.Add(TextBlock(row.status).Class("support-tool-call-status"));

                    list.Add(VStack().WS().Class("support-tool-call-row").Children(
                                TextBlock(row.summary).Class("support-tool-call-row-title"),
                                meta));
                }
                card.Add(list);
            }
            else if (chatToolCall.InvocationSucceeded == false && !string.IsNullOrEmpty(chatToolCall.ErrorMessage))
            {
                card.Add(TextBlock(chatToolCall.ErrorMessage).Class("support-tool-call-error"));
            }

            return card;
        }

        private static List<ToolCaseRow> TryParseCases(string resultContent)
        {
            if (string.IsNullOrWhiteSpace(resultContent)) return null;
            try
            {
                var rows = JsonConvert.DeserializeObject<List<ToolCaseRow>>(resultContent);
                // Only treat it as a case list if the rows actually look like cases.
                if (rows != null && rows.Count > 0 && rows.Exists(r => !string.IsNullOrEmpty(r.id) || !string.IsNullOrEmpty(r.summary)))
                {
                    return rows;
                }
            }
            catch (Exception)
            {
                // Not a case-list shaped result — fall back to the simple header.
            }
            return null;
        }
    }

    public class ToolCaseRow
    {
        public string id { get; set; }
        public string summary { get; set; }
        public string status { get; set; }
        public string device { get; set; }
    }
}
