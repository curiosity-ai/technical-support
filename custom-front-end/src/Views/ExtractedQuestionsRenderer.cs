using UID;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mosaik;
using Mosaik.Components;
using Mosaik.Schema;
using Mosaik.Views;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;

namespace TechnicalSupport.FrontEnd
{
    // Dev/debug view for ExtractedQuestions nodes: renders the raw extracted questions
    // against the sanitized ones as a side-by-side diff, so the effect of the
    // sanitization step (what was removed or rewritten) is visible at a glance.
    public class ExtractedQuestionsRenderer : NodeRendererBase
    {
        public ExtractedQuestionsRenderer() : base(new SchemaStyleInfo()
        {
            Name        = N.ExtractedQuestions.Type,
            DisplayName = "Extracted Questions",
            LabelField  = N.ExtractedQuestions.Topic,
            Color       = "#B45309", // amber-700, marks this as a debug node type
            Icon        = UIconHelper.ToCssClass(UIcons.MessagesQuestion),
        })
        { }

        public override async Task<OmniResult<Node>> PreviewAsync(Node node, Parameters state)
        {
            return NodeResult.For(this, node)
                             .SetModalContent(CreateView(node))
                             .ModalSize(80.vw(), 80.vh());
        }

        private IComponent CreateView(Node node)
        {
            var extracted   = GetStringList(node, N.ExtractedQuestions.Questions);
            var sanitized   = GetStringList(node, N.ExtractedQuestions.SanitizedQuestions);
            var isSanitized = node.GetBool(N.ExtractedQuestions.Sanitized);

            var stack = VStack().S().ScrollY().Children(
                Label("Support Case").WS().Inline().AutoWidth().SetContent(NeighborsLinks(node.UID, N.SupportCase.Type, E.ForSupportCase).WS()),
                Label("Messages").WS().Inline().AutoWidth().SetContent(TextBlock(node.GetInt(N.ExtractedQuestions.MessageCount).ToString())),
                Label("Topic").WS().Inline().AutoWidth().SetContent(TextBlock(node.GetString(N.ExtractedQuestions.Topic))),
                Label("Sanitized Topic").WS().Inline().AutoWidth().SetContent(
                    isSanitized ? TextBlock(node.GetString(N.ExtractedQuestions.SanitizedTopic)) : TextBlock("(not sanitized yet)").Secondary()));

            if (extracted.Length == 0 && sanitized.Length == 0)
            {
                stack.Add(Label("Questions"));
                stack.Add(TextBlock("No questions extracted yet.").Secondary());
            }
            else if (!isSanitized || sanitized.Length == 0)
            {
                stack.Add(Label($"Extracted Questions ({extracted.Length})"));
                stack.Add(TextBlock("Not sanitized yet — showing the raw extracted questions only.").Secondary().PB(8));

                foreach (var question in extracted)
                {
                    stack.Add(TextBlock("• " + question).WS().BreakSpaces());
                }
            }
            else
            {
                stack.Add(Label($"Extracted ({extracted.Length}) vs. Sanitized ({sanitized.Length})"));

                var diff = CodeDiff(BuildUnifiedDiff(extracted, sanitized), Tesserae.CodeDiff.Format.SideBySide);
                diff.DrawFileList  = false;
                diff.HighlightCode = false;
                diff.LineMatching  = Tesserae.CodeDiff.Matching.Words;

                stack.Add(diff.WS());
            }

            return stack;
        }

        private static string[] GetStringList(Node node, string field)
        {
            node.UnsafeTryGetAs(field, out string[] values);
            return values ?? new string[0];
        }

        // Builds a single-hunk unified diff (LCS on whole questions) that the CodeDiff
        // component can render side-by-side: extracted on the left, sanitized on the right.
        private static string BuildUnifiedDiff(string[] extracted, string[] sanitized)
        {
            var left  = extracted.Select(CleanLine).ToArray();
            var right = sanitized.Select(CleanLine).ToArray();

            var lcs = new int[left.Length + 1][];

            for (int i = 0; i <= left.Length; i++)
            {
                lcs[i] = new int[right.Length + 1];
            }

            for (int i = left.Length - 1; i >= 0; i--)
            {
                for (int j = right.Length - 1; j >= 0; j--)
                {
                    lcs[i][j] = left[i] == right[j] ? lcs[i + 1][j + 1] + 1 : Math.Max(lcs[i + 1][j], lcs[i][j + 1]);
                }
            }

            var sb = new StringBuilder();
            sb.Append("--- a/extracted-questions\n");
            sb.Append("+++ b/sanitized-questions\n");
            sb.Append($"@@ -{(left.Length == 0 ? 0 : 1)},{left.Length} +{(right.Length == 0 ? 0 : 1)},{right.Length} @@\n");

            int x = 0, y = 0;

            while (x < left.Length && y < right.Length)
            {
                if (left[x] == right[y])
                {
                    sb.Append(' ').Append(left[x]).Append('\n');
                    x++; y++;
                }
                else if (lcs[x + 1][y] >= lcs[x][y + 1])
                {
                    sb.Append('-').Append(left[x]).Append('\n');
                    x++;
                }
                else
                {
                    sb.Append('+').Append(right[y]).Append('\n');
                    y++;
                }
            }

            while (x < left.Length)
            {
                sb.Append('-').Append(left[x]).Append('\n');
                x++;
            }

            while (y < right.Length)
            {
                sb.Append('+').Append(right[y]).Append('\n');
                y++;
            }

            return sb.ToString();
        }

        // Each question must map to exactly one diff line
        private static string CleanLine(string question)
        {
            return (question ?? "").Replace("\r", " ").Replace("\n", " ").Trim();
        }
    }
}
