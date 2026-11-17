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

using Newtonsoft.Json;

namespace TechnicalSupport.FrontEnd
{
    /// <summary>
    /// Runs one prompt through the AI assistant in a throw-away chat and streams the answer into a modal. The user
    /// can use the text (handed to <c>useText</c>), rewrite it, stop it or continue in the chat.
    /// </summary>
    /// <remarks>
    /// The answer arrives over the websocket as typed <see cref="ChatStreamPart"/> frames (<c>&lt;PART&gt;</c>), the same
    /// frames the chat view renders. Only the reply's text is shown; thinking and tool calls are dropped.
    /// </remarks>
    internal static class AIDraftModal
    {
        public static async Task ShowAsync(string inputPrompt, Action<string> useText, UnitSize width, UnitSize height)
        {
            var header = HStack().WS().AlignItemsCenter();

            var modal = Modal().W(width).H(height).LightDismiss().ShowCloseButton().SetHeader(header).Draggable();
            modal.Show(); //Mount already so we can use it as the hook component for the websocket

            var          isDone = new SettableObservable<bool>();
            var          text   = new SettableObservable<string>("");
            ChatMetadata chat   = null;

            modal.OnHide(_ =>
            {
                if (!isDone.Value && chat is object) API.ChatAI.CancelChat(chat.UID).FireAndForget();
            });

            var answer = DeltaComponent(Spinner().Medium()).Animated();

            var btnUse            = Button("Use text".t()).Compact().SetIcon(UIcons.Copy);
            var btnRegenerate     = Button("Rewrite".t()).Compact().SetIcon(UIcons.Refresh);
            var btnStop           = Button("Stop".t()).Compact().SetIcon(UIcons.Stop);
            var btnContinueOnChat = Button("Go to chat".t()).Compact().SetIcon(UIcons.Comments);

            btnStop.OnClick(() =>
            {
                API.ChatAI.CancelChat(chat.UID).FireAndForget();
                isDone.Value = true;
            });

            btnUse.OnClick(() =>
            {
                useText(text.Value);
                modal.Hide();
            });

            isDone.Observe(done =>
            {
                foreach (var b in new[] { btnUse, btnRegenerate, btnContinueOnChat })
                {
                    if (done) b.Show(); else b.LightFade();
                    b.IsEnabled = done;
                }

                if (done) btnStop.Collapse(); else btnStop.Show();

                header.Children(done ? DoneHeader() : ThinkingHeader(chat));
            });

            // The answer is drawn from the text and from whether it is finished (the cursor goes when it is), so a
            // change to either redraws it. DeltaComponent patches what changed instead of replacing the whole answer.
            void DrawAnswer()
            {
                if (text.Value.Length == 0 && !isDone.Value) return;

                var block = TextBlock("", treatAsHTML: true, selectable: true).Wrap().WS();
                block.HTML = Tesserae.Markdown.ConvertMarkdownSanitized(text.Value + (isDone.Value ? "" : "▮"));
                answer.ReplaceContent(block);
            }

            text.ObserveFutureChanges(_ => DrawAnswer());
            isDone.ObserveFutureChanges(_ => DrawAnswer());

            var actions = HStack().WS().AlignItemsCenter().NoWrap().JustifyContent(ItemJustify.Around).Children(btnUse, btnRegenerate, btnContinueOnChat, btnStop);

            modal.Content(VStack().S().Children(VStack().ScrollY().Children(answer.WS()).WS().H(10).Grow(), actions));

            // Each frame carries its index in the reply, so the text is rebuilt from the pieces in order.
            var pieces = new Dictionary<int, string>();

            API.Websocket.Subscribe(modal, SocketMsgType.CHAT_COMPLETION, msg =>
            {
                var parts = msg.Split(new[] { '§' }, 3);

                if (chat is null || new UID128(parts[0]) != chat.UID) return;
                if (!parts[2].StartsWith(ChatCompletionTypes.PART)) return;

                var part = JsonConvert.DeserializeObject<ChatStreamPart>(parts[2].Substring(ChatCompletionTypes.PART.Length));

                switch (part?.Type)
                {
                    case ChatStreamPartType.TextDelta:
                        pieces[int.Parse(parts[1])] = part.Content;
                        text.Value = Join(pieces);
                        break;
                    case ChatStreamPartType.Error:
                        isDone.Value = true;
                        text.Value   = part.Content ?? "";
                        break;
                    case ChatStreamPartType.End:
                    case ChatStreamPartType.Canceled:
                        isDone.Value = true;
                        text.Value   = Join(pieces);
                        break;
                }
            });

            async Task StartAsync()
            {
                pieces.Clear();
                isDone.Value = false;
                answer.ReplaceContent(Spinner().Medium());

                chat = await API.ChatAI.NewChat(App.InterfaceSettings.ChatAIProvider?.TaskUID, App.InterfaceSettings.SelectedAIAssistantTemplate?.UID);

                var messageUID = await API.ChatAI.PostMessage(chat.UID, inputPrompt, simple: true);

                btnContinueOnChat.OnClick(() => Router.Navigate(DefaultRoutes.ChatAI.OpenChat(chat.UID, messageUID), reload: true));
            }

            btnRegenerate.OnClickSpinWhile(async () =>
            {
                await API.ChatAI.DeleteChat(chat.UID);
                await StartAsync();
            });

            await StartAsync();
        }

        private static string Join(Dictionary<int, string> pieces) => string.Join("", pieces.OrderBy(kv => kv.Key).Select(kv => kv.Value));

        private static IComponent[] ThinkingHeader(ChatMetadata forChat)
        {
            return new IComponent[]
            {
                Image(API.ChatAI.GetAIAssistantTemplatePhotoUrl(forChat?.AIAssistantTemplate?.UID ?? App.InterfaceSettings.SelectedAIAssistantTemplate?.UID)).Circle().Cover().W(20).H(20),
                TextBlock("Thinking...".t()).NoWrap().PL(8)
            };
        }

        private static IComponent[] DoneHeader()
        {
            return new IComponent[] { Icon(UIcons.Check).Foreground(Theme.Success.Background), TextBlock("Done !".t()).NoWrap().PL(8) };
        }
    }
}
