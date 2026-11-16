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
    /// A device: its product page (manufacturer and parts), its support cases and the graph around it. The search
    /// row, preview sheet and full page are all built by NodeRendererBase from these tabs.
    /// </summary>
    public sealed class DeviceRenderer : NodeRendererBase
    {
        public DeviceRenderer() : base(new SchemaStyleInfo
        {
            Name        = N.Device.Type,
            DisplayName = "Device",
            LabelField  = N.Device.Name,
            Color       = "#346eeb",
            Icon        = UIcons.BoxOpenFull.ToString(),
        })
        {
        }

        protected override IEnumerable<NodeTab> BuildTabs(Node node, IReadOnlyList<RelationshipGroup> groups, NodeView nodeView)
        {
            yield return new NodeTab { Id = "product", Label = "Product Page".t(), TabTitle = SegmentTitle("Product Page".t()), TabContent = () => RenderProductPage(node) };
            yield return new NodeTab { Id = "support", Label = "Support".t(),      TabTitle = SegmentTitle("Support".t()),      TabContent = () => RenderSupportCases(node), Scrolls = false };
            yield return new NodeTab { Id = "graph",   Label = "Graph".t(),        TabTitle = SegmentTitle("Graph".t()),        TabContent = () => RenderGraphSegment(node, nodeView.OpenGraphNodePeek), Scrolls = false };
        }

        private static IComponent RenderProductPage(Node node)
        {
            return VStack().S().Children(
                Label("Name".t()).WS().Inline().AutoWidth().SetContent(TextBlock(node.GetString(N.Device.Name))),
                Label("Manufacturer".t()).WS().Inline().AutoWidth().SetContent(NeighborsLinks(node.UID, N.Manufacturer.Type, E.HasManufacturer).WS()),
                Label("Parts".t()),
                Neighbors(() => API.Query.StartAt(node.UID).Out(N.Part.Type, E.HasPart).TakeAll().GetUIDsAsync(),
                          new[] { N.Part.Type }, showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible).S());
        }

        private static IComponent RenderSupportCases(Node node)
        {
            return Neighbors(() => API.Query.StartAt(node.UID).Out(N.SupportCase.Type, E.HasSupportCase).TakeAll().GetUIDsAsync(),
                             new[] { N.SupportCase.Type }, showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible, defaultSortMode: SortModeEnum.RecentFirst).S();
        }
    }
}
