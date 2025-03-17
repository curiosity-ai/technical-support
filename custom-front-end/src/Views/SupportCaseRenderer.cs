using System.Threading.Tasks;
using Mosaik;
using Mosaik.Components;
using Mosaik.Schema;
using System.Collections.Generic;
using System.Linq;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;
using TNT;
using static TNT.T;
using static H5.Core.dom;
using UID;
using System;
using System.Text;
using Mosaik.FrontEnd.Core.External;
using Node = Mosaik.Schema.Node;
using Newtonsoft.Json;
using Mosaik.Components.Data;
using Mosaik.Helpers;

namespace TechnicalSupport.FrontEnd
{
    public class SupportCaseRenderer : INodeRenderer
    {
        public string NodeType => N.SupportCase.Type;
        public string DisplayName => "Case";
        public string LabelField => "Summary";
        public string Color => "#17a6bf";
        public UIcons Icon => UIcons.MessageQuestion;

        public CardContent CompactView(Node node)
        {
            return CardContent(Header(this, node), null);
        }

        public async Task<CardContent> PreviewAsync(Node node, Parameters state)
        {
            return CardContent(Header(this, node), CreateView(node, state));
        }

        public async Task<IComponent> ViewAsync(Node node, Parameters state)
        {
            return (await PreviewAsync(node, state)).Merge();
        }

        private IComponent CreateView(Node node, Parameters state)
        {
            var aiEnabled   = new SettableObservable<bool>();
            var scoresCases = new ObservableDictionary<UID128, float>();

            aiEnabled.Value = LocalStorage.GetBool("ai-enabled");

            var toggle = Button("AI suggestions");
            aiEnabled.Observe(v =>
            {
                toggle.SetIcon(v ? UIcons.Bolt : UIcons.BoltSlash, weight: v ? UIconsWeight.Solid : UIconsWeight.Regular);
                LocalStorage.SetBool("ai-enabled", v);
                if (!v)
                {
                    scoresCases.Clear();
                }
            });

            toggle.OnClick(() =>
            {
                aiEnabled.Value = !aiEnabled.Value;
            });

            return SplitView().S().LeftIsSmaller(400.px())
                    .Left(RenderConversation(node, scoresCases))
            .Right(DeferSync(aiEnabled, ai =>
            {

                Func<Task<UIDResults>> queryCases;

                if (ai)
                {
                    queryCases = async () =>
                    {
                        var scores = await Mosaik.API.Endpoints.CallAsync<Dictionary<UID128, float>>("suggest-similar-cases", node.UID);
                        scoresCases.Clear();
                        scores.Remove(node.UID);
                        foreach (var kv in scores) scoresCases[kv.Key] = kv.Value;
                        var others = await Mosaik.API.Query.StartAt(node.UID).Union(Mosaik.API.Query.StartAt(node.UID).Out(N.Device.Type).Out(new[] { N.SupportCase.Type })).Skip(1).TakeAll().GetUIDsAsync();
                        return UIDResults.FromResults(new ReadOnlyArray<UID128>(scores.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).Concat(others.UIDs).Distinct().ToArray()));
                    };
                }
                else
                {
                    queryCases = async () => await Mosaik.API.Query.StartAt(node.UID).Union(Mosaik.API.Query.StartAt(node.UID).Out(N.Device.Type).Out(new[] { N.SupportCase.Type })).Skip(1).TakeAll().GetUIDsAsync();
                }


                return 
                    VStack().Class("support-case-info").PT(16).S().Children(
                    HStack().WS().NoWrap().Children(Label("Similar Cases"), Empty().Grow(), toggle),
                    Neighbors(queryCases,
                                new[] { N.SupportCase.Type }, true, FacetDisplayOptions.Visible, defaultSortMode: ai ? SortModeEnum.TargetQueryOrder : SortModeEnum.RecentFirst,
                              renderer: r => r.WithCardCustomizer((n, c) => AppendScoresIfAny(n, c, scoresCases))
                             ).WS().H(10).Grow()
                            );
            }));
        }

        private void AppendScoresIfAny(Node node, CardContent card, ObservableDictionary<UID128, float> scores)
        {
            if (scores is null) return;
            card.Header.Title.WhenMounted(() =>
            {
                var parent = card.Header.Title.Render();
                while(parent is object)
                {
                    if(parent.classList.contains("rendered-search-result-inner"))
                    {
                        if (scores.TryGetValue(node.UID, out var score))
                        {
                            int percent = (int)(score * 100);
                            parent.style.background = $"linear-gradient(90deg, rgba(var(--tss-default-foreground-color-root), 0.03) 0%, rgba(var(--tss-default-foreground-color-root), 0.13) {percent}%, var(--tss-default-background-color) {percent}%, var(--tss-default-background-color) 100% ";
                        }
                        else
                        {
                            parent.style.background = "";
                        }
                        break;
                    }
                    parent = parent.parentElement;
                }
            });
        }

        private IComponent RenderConversation(Node node, ObservableDictionary<UID128, float> scoresCases)
        {
            return Defer(async () =>
            {
                var device = (await Mosaik.API.Query.StartAt(node.UID).Out(N.Device.Type, E.ForDevice).GetAsync()).Nodes.First();
                var stack = VStack().WS().H(10).ScrollY();
                var messages = await Mosaik.API.Query.StartAt(node.UID).Out(N.SupportCaseMessage.Type, E.HasMessage).GetAsync();
                var text = new StringBuilder();
                text.Append("Case Title: ").Append(node.GetString(N.SupportCase.Summary)).AppendLine();
                foreach (var msg in messages.Nodes)
                {
                    var hs = HStack().Children(
                        TextBlock(msg.GetString(N.SupportCaseMessage.Message)).BreakSpaces().MaxWidth(300.px())
                        );

                    var author = TextBlock(msg.GetString(N.SupportCaseMessage.Author)).Tiny().MB(20);

                    if (msg.GetString(N.SupportCaseMessage.Author) == "Support")
                    {
                        text.Append("Support: ").Append(msg.GetString(N.SupportCaseMessage.Message)).AppendLine();
                        hs.AlignStart();
                        author.AlignStart();
                        hs.Class("support-case-message-support");
                    }
                    else
                    {
                        text.Append("User: ").Append(msg.GetString(N.SupportCaseMessage.Message)).AppendLine();
                        hs.AlignEnd();
                        author.AlignEnd();
                        hs.Class("support-case-message-user");
                    }

                    hs.Class("support-case-message");

                    stack.Add(VStack().WS().Children(hs, author));
                }

                stack.WhenMounted(() =>
                {
                    stack.Render().parentElement.style.flexGrow = "1";
                    stack.Render().parentElement.style.overflow = "hidden";
                    stack.Render().style.overflow = "hidden auto";
                });

                var fullText = text.ToString();

                var btnDraft = Button("Draft Answer").SetIcon(UIcons.CommentAlt, weight: UIconsWeight.Solid);
                var btnWriteKnowledgeBaseEntry = Button("Capture Knowledge").SetIcon(UIcons.JournalAlt, weight: UIconsWeight.Solid);
                var reply = TextArea().Class("support-case-chat-area").Grow();
                
                btnDraft.OnClickSpinWhile(async () =>
                {
                    var sbKnowledge = new StringBuilder();
                    if(scoresCases is object && scoresCases.Count > 0)
                    {
                        var simCases = scoresCases.Where(kv => kv.Value > 0.7).Select(k => k.Key).ToArray();
                        var cases = await Mosaik.API.Query.StartAt(simCases).GetAsync();
                        foreach (var doc in cases.Nodes)
                        {
                            sbKnowledge.Append("--- BEGIN OF PREVIOUS SUPPORT CASE ---").AppendLine();
                            sbKnowledge.Append("Title: ").Append(doc.GetString(N.SupportCase.Summary)).AppendLine();
                            var caseMessages= await Mosaik.API.Query.StartAt(doc.UID).Out(N.SupportCaseMessage.Type).TakeAll().GetAsync();
                            foreach(var msg in caseMessages.Nodes)
                            {
                                sbKnowledge.Append(msg.GetString(N.SupportCaseMessage.Author) == "Support" ? "Support: " : "User: ").Append(msg.GetString(N.SupportCaseMessage.Message).Trim('\r','\n')).AppendLine();
                            }
                            sbKnowledge.Append("--- END OF PREVIOUS SUPPORT CASE ---").AppendLine();
                        }
                    }
                    await RunChatOnText($"I need you to draft an answer for the following support case:\n--- BEGIN CASE ---\n{fullText}\n--- END CASE ---\n\n" + (sbKnowledge.Length > 0 ? ("Here's some internal previous support cases that might be useful to answer the current support case: \n" + sbKnowledge.ToString()) : "") + "Answer with a short and concise draft answer only, and nothing else.", (v) => reply.Text = v, 400.px(), 300.px());
                });

                btnWriteKnowledgeBaseEntry.OnClickSpinWhile(async () =>
                {
                    await RunChatOnText($"I need you to capture a knowledge base entry based on the following support case for the device {device.GetString(N.Device.Name)}:\n--- BEGIN CASE ---\n{fullText}\n--- END CASE ---\n\nThe knowledge base entry should be structured in markdown, include the details of the case, and be structured in 3 sections: 'Applicable Device', 'Problem Description' and 'Solution'. Use markdown formating. Answer with the requested knowledge base entry and nothing else.",
                        (v) => Toast().Information("Thanks for capturing this knowledge entry"), 600.px(), 800.px());
                });

                if(scoresCases is object)
                {
                    scoresCases.Observe(d =>
                    {
                        if (d.Count == 0)
                        {
                            btnDraft.Collapse();
                            btnWriteKnowledgeBaseEntry.Collapse();
                        }
                        else
                        {
                            btnDraft.Show();
                            btnWriteKnowledgeBaseEntry.Show();
                        }
                    });
                }

                return HorizontalSplitView().Resizable().BottomIsSmaller(128.px(), minBottomSize: 100.px(), maxBottomSize: 50.vw())
                .Top(VStack().S().Children(
                    Label("Device").WS().Inline().SetContent(NeighborsLinks(node.UID, N.Device.Type)),
                    Label("Conversation"),
                    stack.Class("support-case-chat")))
                .Bottom(VStack().S().Children(
                    Label("Reply"),
                    HStack().WS().NoWrap().H(10).Grow().Children(reply.HS(), Button().SetIcon(UIcons.PaperPlane).Tooltip("Send")),
                    HStack().WS().NoWrap().Children(btnDraft, btnWriteKnowledgeBaseEntry)));
            }).S();
        }

        public static async Task RunChatOnText(string inputPrompt, Action<string> useText, UnitSize width, UnitSize height)
        {
            await Marked.LoadLibraries();

            var header = HStack().WS().AlignItemsCenter();

            var modal = Modal().W(width).H(height).LightDismiss().ShowCloseButton().SetHeader(header).Draggable().Class("chat-ai-quick-generation");
            modal.Show(); //Mount already so we can use it as the hook component for the websocket

            var isDone = new SettableObservable<bool>();
            ChatMetadata chat = null;

            modal.OnHide((_) =>
            {
                if (!isDone.Value && chat is object)
                {
                    Mosaik.API.ChatAI.CancelChat(chat.UID).FireAndForget();
                }
            });

            var textBlock = TextBlock(selectable: true);

            //TODO: Only header should drag, show cancel button when generating, show close button and cancel, add close button to GetThinkingText() 

            var actions = HStack().WS().AlignItemsCenter().NoWrap().JustifyContent(ItemJustify.Around);

            var btnCopy = Button("Use text".t()).Compact().SetIcon(UIcons.Copy);
            var btnRegenerate = Button("Rewrite".t()).Compact().SetIcon(UIcons.Refresh);
            var btnStop = Button("Stop".t()).Compact().SetIcon(UIcons.Stop);

            var btnContinueOnChat = Button("Go to chat".t()).Compact().SetIcon(UIcons.Comments);

            btnStop.OnClick(() =>
            {
                Mosaik.API.ChatAI.CancelChat(chat.UID).FireAndForget();
                isDone.Value = true;
            });

            actions.Add(btnCopy);
            actions.Add(btnRegenerate);

            btnCopy.OnClick(() =>
            {
                useText(GetTextToCopy(textBlock.Render()));
                modal.Hide();
                //ChatAIView.CopyToClipboardMaybeWithWarningMessage(isAssistant: true, text: GetTextToCopy(textBlock.Render()), customMessage: "Copied!".t(), html: GetHtmlToCopy(textBlock.Render()));
            });

            isDone.Observe(d =>
            {
                if (d)
                {
                    btnCopy.Show();
                    btnRegenerate.Show();
                    btnContinueOnChat.Show();
                    btnStop.Collapse();
                }
                else
                {
                    btnCopy.LightFade();
                    btnRegenerate.LightFade();
                    btnContinueOnChat.LightFade();
                    btnStop.Show();
                }

                btnCopy.IsEnabled = d;
                btnRegenerate.IsEnabled = d;
                btnContinueOnChat.IsEnabled = d;

                header.Children(d ? GetDoneText() : GetThinkingText(chat));
            });

            actions.Add(btnStop);

            modal.Content(VStack().S().Children(VStack().ScrollY().Children(textBlock.WS()).WS().H(10).Grow(), actions));

            chat = await Mosaik.API.ChatAI.NewChat(App.InterfaceSettings.ChatAIProvider?.TaskUID, App.InterfaceSettings.SelectedAIAssistantTemplate?.UID);

            bool receivedDone = false;

            var streamingMessage = new ObservableDictionary<int, string>();
            var streamingProcesses = new Dictionary<int, ChatAI_Process>();

            streamingMessage.Clear();
            streamingProcesses.Clear();
            streamingMessage[-1] = "...";

            Action<int, string> onMessageCompletionReceived = (count, msg) =>
            {
                if (streamingMessage.ContainsKey(-1))
                {
                    streamingMessage.Clear();
                    streamingProcesses.Clear();
                }

                if (msg == ChatCompletionTypes.DONE)
                {
                    isDone.Value = true;
                    receivedDone = true;
                    streamingMessage[int.MaxValue] = "";
                    App.CloseAllProgressModals();
                }
                else if (msg.StartsWith(ChatCompletionTypes.FAIL))
                {
                    var errorMessage = msg.Substring(ChatCompletionTypes.FAIL.Length);

                    isDone.Value = true;
                    receivedDone = true;
                    streamingMessage[int.MaxValue] = "";
                }
                else if (msg == ChatCompletionTypes.CANCELED)
                {
                    isDone.Value = true;
                    receivedDone = true;
                    streamingMessage[int.MaxValue] = "";
                }
                else if (msg.StartsWith(ChatCompletionTypes.PROC))
                {
                    var process = JsonConvert.DeserializeObject<ChatAI_Process>(msg.Substring(ChatCompletionTypes.PROC.Length));
                    streamingProcesses[process.Id] = process;
                    streamingMessage[count] = $"[PROCESS:{process.Id}]";
                }
                else
                {
                    streamingMessage[count] = msg;
                    isDone.Value = receivedDone;
                }
            };

            Mosaik.API.Websocket.Subscribe(modal, SocketMsgType.CHAT_COMPLETION, msg =>
            {
                var parts = msg.Split(new[] { '§' }, 3);
                var chatUID2 = new UID128(parts[0]);
                var count = int.Parse(parts[1]);
                var message = parts[2];

                if (chatUID2 == chat?.UID)
                {
                    onMessageCompletionReceived(count, message);
                }
            });

            streamingMessage.Observe((dict) =>
            {
                if (dict.Count == 0)
                {
                    textBlock.Text = "";
                    textBlock.RemoveClass("chat-thinking");
                }
                else if (streamingMessage.ContainsKey(-1))
                {
                    textBlock.Text = "...";
                    textBlock.Class("chat-thinking");
                }
                else
                {
                    var isDone2 = streamingMessage.ContainsKey(int.MaxValue);
                    streamingMessage.Remove(-1);

                    if (streamingMessage.Count > 1 || streamingMessage.First().Value != "...")
                    {
                        textBlock.RemoveClass("chat-thinking");
                    }

                    textBlock.HTML = Marked.Shared.ConvertMarkdownSanitized(string.Join("", streamingMessage.OrderBy(kv => kv.Key).Select(kv => kv.Value)) + (isDone2 ? "" : "▮")).Trim(' ', '\n', '\r', '"', '\'');
                }
                //ScrollInToView(textBlock, ScrollLogicalPosition.end);
            });

            UID128 messageUID;

            try
            {
                messageUID = await Mosaik.API.ChatAI.PostMessage(chat.UID, inputPrompt, simple: true);
            }
            catch (Exception ex)
            {
                console.error(ex);
                throw;
            }

            btnContinueOnChat.OnClick(() => Router.Navigate(DefaultRoutes.ChatAI.OpenChat(chat.UID, messageUID), reload: true));

            btnRegenerate.OnClickSpinWhile(async () =>
            {
                streamingMessage.Clear();
                streamingProcesses.Clear();
                streamingMessage[-1] = "...";
                receivedDone = false;
                isDone.Value = false;

                try
                {
                    await Mosaik.API.ChatAI.DeleteChat(chat.UID);
                }
                catch (Exception)
                {
                    //Ignore
                }

                chat = await Mosaik.API.ChatAI.NewChat(App.InterfaceSettings.ChatAIProvider?.TaskUID, App.InterfaceSettings.SelectedAIAssistantTemplate?.UID ?? AIAssistantSettings.DefaultAIAssistantUID);
                messageUID = await Mosaik.API.ChatAI.PostMessage(chat.UID, inputPrompt, simple: true);
            });
        }
        private static IComponent[] GetThinkingText(ChatMetadata forChat)
        {
            return new IComponent[] { Image(Mosaik.API.ChatAI.GetAIAssistantTemplatePhotoUrl(forChat?.AIAssistantTemplate?.UID ?? App.InterfaceSettings.SelectedAIAssistantTemplate?.UID)).Circle().Cover().W(20).H(20), TextBlock("Thinking...".t()).NoWrap().PL(8) };
        }

        private static IComponent[] GetDoneText()
        {
            return new IComponent[] { Image("./assets/img/icons/check.svg").W(20).H(20), TextBlock("Done !".t()).NoWrap().PL(8) };
        }

        internal static string GetTextToCopy(HTMLElement el)
        {
            var clone = el.cloneNode(true).As<HTMLElement>();

            foreach (var b in clone.querySelectorAll(".uid-btn-text"))
            {
                b.As<HTMLElement>().textContent = " " + b.textContent.Split(new[] { '>' }).First();
            }

            foreach (var e in clone.querySelectorAll(".tss-btn"))
            {
                var sp = P(_(text: "§"));
                e.parentElement.replaceChild(sp, e.As<HTMLElement>());
            }

            foreach (var e in clone.querySelectorAll(".node-as-label"))
            {
                e.As<HTMLElement>().remove();
            }

            return clone.textContent.Replace("§", "\n");
        }

        internal static string GetHtmlToCopy(HTMLElement el)
        {
            var clone = el.cloneNode(true).As<HTMLElement>();

            foreach (var b in clone.querySelectorAll(".uid-btn-text"))
            {
                b.As<HTMLElement>().textContent = " " + b.textContent.Split(new[] { '>' }).First();
            }

            foreach (var e in clone.querySelectorAll(".tss-btn"))
            {
                var sp = P(_(text: "§"));
                e.parentElement.replaceChild(sp, e.As<HTMLElement>());
            }

            foreach (var e in clone.querySelectorAll(".node-as-label"))
            {
                e.As<HTMLElement>().remove();
            }

            return clone.innerHTML.Replace("§", "<br>");
        }
    }
}