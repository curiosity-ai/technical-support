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
    /// A support case. Its row shows the case's status as the icon (open or closed) and the device it is about in
    /// the footer, everywhere a case is listed. Opening it shows the case workspace (<see cref="SupportCaseWorkspace"/>).
    /// </summary>
    public sealed class SupportCaseRenderer : NodeRendererBase, INodeCustomStyle
    {
        private const string CLOSED = "Closed";

        public SupportCaseRenderer() : base(new SchemaStyleInfo
        {
            Name        = N.SupportCase.Type,
            DisplayName = "Case",
            LabelField  = N.SupportCase.SupportCaseSummary,
            Color       = "#17a6bf",
            Icon        = UIcons.MessageQuestion.ToString(),
        })
        {
        }

        private static bool IsClosed(Node node) => node.GetString(N.SupportCase.Status) == CLOSED;

        // INodeCustomStyle: the status decides the icon and the colour of each case.
        public UIcons GetIcon(Node node)         => IsClosed(node) ? UIcons.CommentAltCheck : UIcons.MessageQuestion;
        public string GetColor(Node node)        => IsClosed(node) ? Theme.Success.Background : Theme.Danger.Background;
        public string GetDisplayName(Node node)  => node.GetString(N.SupportCase.Status) ?? DisplayName;
        public string GetLabel(Node node)        => node.GetString(N.SupportCase.SupportCaseSummary);

        // A backlog row: the summary as the title, the status as the icon and the device in the footer. The case's
        // conversation is what opening it is for, so the row does not repeat it.
        public override OmniResult<Node> CompactView(Node node)
        {
            return NodeResult.For(this, node)
               .WithNeighborFooterEntries(async () => (await API.Query.StartAt(node.UID).Out(N.Device.Type, E.ForDevice).GetUIDsAsync()).UIDs);
        }

        public override async Task<OmniResult<Node>> PreviewAsync(Node node, Parameters state)
        {
            return NodeResult.For(this, node).SetModalContent(new SupportCaseWorkspace(node)).ModalSize(90.vw(), 90.vh());
        }
    }
}
