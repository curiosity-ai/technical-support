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

using System.Text;

namespace TechnicalSupport.FrontEnd
{
    /// <summary>
    /// The case workspace: the conversation and a reply box on the left, the similar cases on the right. With AI
    /// suggestions on, the similar cases are ranked by the suggest-similar-cases endpoint and the assistant can draft
    /// an answer from the best of them, or capture the case as a knowledge base entry.
    /// </summary>
    public sealed class SupportCaseWorkspace : IComponent
    {
        private const string AI_ENABLED_KEY = "ai-enabled";
        private const string SUPPORT_AUTHOR = "Support";

        // Cases at least this similar are given to the assistant as examples when it drafts an answer.
        private const float DRAFT_SIMILARITY_THRESHOLD = 0.7f;

        private readonly Node _node;

        // The AI ranking of the similar cases, empty while AI suggestions are off.
        private readonly SettableObservable<SimilarCase[]> _similar = new SettableObservable<SimilarCase[]>(Array.Empty<SimilarCase>());

        private readonly IComponent _content;

        public SupportCaseWorkspace(Node node)
        {
            _node    = node;
            _content = SplitView().S().LeftIsSmaller(400.px())
               .Left(Defer(RenderConversationAsync).S())
               .Right(RenderSimilarCases());
        }

        public HTMLElement Render() => _content.Render();

        private IComponent RenderSimilarCases()
        {
            var aiEnabled = new SettableObservable<bool>(LocalStorage.GetBool(AI_ENABLED_KEY));
            var toggle    = Button("AI suggestions".t()).OnClick(() => aiEnabled.Value = !aiEnabled.Value);
            var panel     = VStack().Class("support-case-info").PT(16).S();
            var header    = HStack().WS().NoWrap().Children(Label("Similar Cases".t()), Empty().Grow(), toggle);

            IComponent list = Empty();

            panel.Children(header, list);

            // Only the list is rebuilt when the toggle flips: it is a different query with a different order.
            aiEnabled.Observe(ai =>
            {
                toggle.SetIcon(ai ? UIcons.Bolt : UIcons.BoltSlash, weight: ai ? UIconsWeight.Solid : UIconsWeight.Regular);
                LocalStorage.SetBool(AI_ENABLED_KEY, ai);

                if (!ai) _similar.Value = Array.Empty<SimilarCase>();

                var next = CreateSimilarCasesList(ai);
                panel.Replace(next, list);
                list = next;
            });

            return panel;
        }

        private IComponent CreateSimilarCasesList(bool ai)
        {
            Func<Task<UIDResults>> query;

            if (ai)
            {
                query = async () =>
                {
                    var similar = await SupportApi.SuggestSimilarCasesAsync(_node.UID);
                    _similar.Value = similar;

                    var others = await OtherCasesForSameDevice().GetUIDsAsync();
                    return UIDResults.FromResults(new ReadOnlyArray<UID128>(similar.Select(s => s.UID).Concat(others.UIDs).Distinct().ToArray()));
                };
            }
            else
            {
                query = () => OtherCasesForSameDevice().GetUIDsAsync();
            }

            return Neighbors(query, new[] { N.SupportCase.Type }, showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible,
                             defaultSortMode: ai ? SortModeEnum.TargetQueryOrder : SortModeEnum.RecentFirst,
                             renderer: r => r.CustomizeResult(result => AppendScoreIfAny(result)))
               .WS().H(10).Grow();
        }

        private SimpleQueryBuilder OtherCasesForSameDevice()
        {
            return API.Query.StartAt(_node.UID).Union(API.Query.StartAt(_node.UID).Out(N.Device.Type, E.ForDevice).Out(N.SupportCase.Type, E.HasSupportCase)).Skip(1).TakeAll();
        }

        private OmniResult<Node> AppendScoreIfAny(OmniResult<Node> result)
        {
            var similar = _similar.Value.FirstOrDefault(s => s.UID == result.Result.UID);

            if (similar is object)
            {
                result.AddFooterEntry(Badge(string.Format("{0:0}% similar".t(), similar.Score * 100)).Tooltip("Similarity to this case".t()));
            }

            return result;
        }

        private async Task<IComponent> RenderConversationAsync()
        {
            var device   = (await API.Query.StartAt(_node.UID).Out(N.Device.Type, E.ForDevice).GetAsync()).Nodes.First();
            var messages = await API.Query.StartAt(_node.UID).Out(N.SupportCaseMessage.Type, E.HasMessage).TakeAll().GetAsync();

            var conversation = VStack().WS().H(10).Grow().ScrollY().Class("support-case-chat");
            var transcript   = new StringBuilder();

            transcript.Append("Case Title: ").Append(_node.GetString(N.SupportCase.SupportCaseSummary)).AppendLine();

            foreach (var msg in messages.Nodes)
            {
                var text      = msg.GetString(N.SupportCaseMessage.Message);
                var isSupport = msg.GetString(N.SupportCaseMessage.Author) == SUPPORT_AUTHOR;
                var bubble    = HStack().Class("support-case-message").Children(TextBlock(text).BreakSpaces().MaxWidth(300.px()));
                var author    = TextBlock(msg.GetString(N.SupportCaseMessage.Author)).Tiny().MB(20);

                transcript.Append(isSupport ? "Support: " : "User: ").Append(text).AppendLine();

                if (isSupport)
                {
                    bubble.AlignStart().Class("support-case-message-support");
                    author.AlignStart();
                }
                else
                {
                    bubble.AlignEnd().Class("support-case-message-user");
                    author.AlignEnd();
                }

                conversation.Add(VStack().WS().Children(bubble, author));
            }

            var fullText = transcript.ToString();

            var reply     = TextArea().Class("support-case-chat-area").Grow();
            var btnDraft  = Button("Draft Answer".t()).SetIcon(UIcons.CommentAlt, weight: UIconsWeight.Solid);
            var btnHelper = Button("Capture Knowledge".t()).SetIcon(UIcons.JournalAlt, weight: UIconsWeight.Solid);

            btnDraft.OnClickSpinWhile(async () =>
            {
                var examples = await DescribeSimilarCasesAsync();

                await AIDraftModal.ShowAsync(
                    $"I need you to draft an answer for the following support case:\n--- BEGIN CASE ---\n{fullText}\n--- END CASE ---\n\n"
                    + (examples.Length > 0 ? "Here's some internal previous support cases that might be useful to answer the current support case: \n" + examples : "")
                    + "Answer with a short and concise draft answer only, and nothing else.",
                    v => reply.Text = v, 520.px(), 340.px());
            });

            btnHelper.OnClickSpinWhile(async () =>
            {
                await AIDraftModal.ShowAsync(
                    $"I need you to capture a knowledge base entry based on the following support case for the device {device.GetString(N.Device.Name)}:\n--- BEGIN CASE ---\n{fullText}\n--- END CASE ---\n\n"
                    + "The knowledge base entry should be structured in markdown, include the details of the case, and be structured in 3 sections: 'Applicable Device', 'Problem Description' and 'Solution'. Use markdown formating. Answer with the requested knowledge base entry and nothing else.",
                    v => Toast().Information("Thanks for capturing this knowledge entry".t()), 600.px(), 800.px());
            });

            // Both actions build on the AI ranking, so they are offered only once there is one.
            _similar.Observe(similar =>
            {
                if (similar.Length == 0)
                {
                    btnDraft.Collapse();
                    btnHelper.Collapse();
                }
                else
                {
                    btnDraft.Show();
                    btnHelper.Show();
                }
            });

            return HorizontalSplitView().S().Resizable().BottomIsSmaller(128.px(), minBottomSize: 100.px(), maxBottomSize: 50.vw())
               .Top(VStack().S().Children(
                    Label("Device".t()).WS().Inline().SetContent(NeighborsLinks(_node.UID, N.Device.Type, E.ForDevice)),
                    Label("Conversation".t()),
                    conversation))
               .Bottom(VStack().S().Children(
                    Label("Reply".t()),
                    HStack().WS().NoWrap().H(10).Grow().Children(reply.HS(), Button().SetIcon(UIcons.PaperPlane).Tooltip("Send".t())),
                    HStack().WS().NoWrap().Children(btnDraft, btnHelper)));
        }

        /// <summary>The most similar earlier cases, written out as examples for the assistant.</summary>
        private async Task<string> DescribeSimilarCasesAsync()
        {
            var sb      = new StringBuilder();
            var similar = _similar.Value.Where(s => s.Score > DRAFT_SIMILARITY_THRESHOLD).Select(s => s.UID).ToArray();

            if (similar.Length == 0) return "";

            var cases = await API.Query.StartAt(similar).GetAsync();

            foreach (var doc in cases.Nodes)
            {
                sb.Append("--- BEGIN OF PREVIOUS SUPPORT CASE ---").AppendLine();
                sb.Append("Title: ").Append(doc.GetString(N.SupportCase.SupportCaseSummary)).AppendLine();

                var caseMessages = await API.Query.StartAt(doc.UID).Out(N.SupportCaseMessage.Type, E.HasMessage).TakeAll().GetAsync();

                foreach (var msg in caseMessages.Nodes)
                {
                    sb.Append(msg.GetString(N.SupportCaseMessage.Author) == SUPPORT_AUTHOR ? "Support: " : "User: ")
                      .Append(msg.GetString(N.SupportCaseMessage.Message).Trim('\r', '\n')).AppendLine();
                }

                sb.Append("--- END OF PREVIOUS SUPPORT CASE ---").AppendLine();
            }

            return sb.ToString();
        }
    }
}
