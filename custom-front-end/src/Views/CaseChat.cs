using System;
using System.Collections.Generic;
using H5.Core;
using Mosaik;
using Mosaik.Components;
using Mosaik.Schema;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;
using Newtonsoft.Json;

namespace TechnicalSupport.FrontEnd
{
    // Case-scoped AI chat embedded in the support-case view ("AI Chat" pivot).
    // Wraps the standard ChatAIView with a case welcome, example prompts,
    // feedback actions and a rich renderer for the support tool results. The
    // support tools (plus the resolve-case tools) are enabled by default so the
    // worker can research and close the case in place. The support context comes
    // from those ChatAI tools, not from a topic system prompt.
    internal class CaseChat : IComponent
    {
        private readonly ChatAIView _chatView;

        private readonly string _caseId;
        private readonly string _caseSummary;

        public dom.HTMLElement Render() => _chatView.Render();

        public CaseChat(Parameters state, Node caseNode)
        {
            _caseId      = caseNode.GetString(N.SupportCase.Id);
            _caseSummary = caseNode.GetString(N.SupportCase.SupportCaseSummary);

            var endpoints = new CustomChatView();

            // Pre-select the support tools plus the resolve-case tools by default
            // (other workspace tools stay available but switched off). Matched by
            // display name.
            var defaultTools = new HashSet<string>
            {
                "Find Similar Support Cases",
                "Support Graph Lookup",
                "Resolve Support Case"
            };

            var listAvailableTools = endpoints.ListTools;
            endpoints.ListTools = async (context) =>
            {
                var tools = await listAvailableTools(context);
                foreach (var tool in tools)
                {
                    tool.InitiallySelected = defaultTools.Contains(tool.DisplayName);
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
                        TextBlock("Case assistant").SemiBold().WS().TextCenter(),
                        TextBlock($"Working on case {_caseId}. I can search similar cases, look up the knowledge graph, and resolve this case once it's fixed.")
                            .Secondary().WS().TextCenter().PT(4));
        }

        private bool CreateChatExamples(CurrentChat chat, Stack stack, TextArea area, ChatAISendStopButton button, bool arg5)
        {
            var examples = new[]
            {
                $"Summarize case {_caseId} and suggest the next step.",
                $"Find cases similar to this one: {_caseSummary}",
                $"Draft a reply for case {_caseId} based on how similar cases were resolved.",
                $"Resolve case {_caseId} — mark it as fixed."
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
