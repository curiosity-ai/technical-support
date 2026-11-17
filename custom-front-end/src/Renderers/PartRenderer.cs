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
    /// <summary>A spare part: its manufacturer and the devices it fits, and the support cases about it.</summary>
    public sealed class PartRenderer : NodeRendererBase
    {
        public PartRenderer() : base(new SchemaStyleInfo
        {
            Name        = N.Part.Type,
            DisplayName = "Part",
            LabelField  = N.Part.Name,
            Color       = "#b9babd",
            Icon        = UIcons.BoxOpenFull.ToString(),
        })
        {
        }

        protected override IEnumerable<NodeTab> BuildTabs(Node node, IReadOnlyList<RelationshipGroup> groups, NodeView nodeView)
        {
            yield return new NodeTab { Id = "overview", Label = "Overview".t(), TabTitle = SegmentTitle("Overview".t()), TabContent = () => RenderOverview(node) };
            yield return new NodeTab { Id = "support",  Label = "Support".t(),  TabTitle = SegmentTitle("Support".t()),  TabContent = () => RenderSupportCases(node), Scrolls = false };
        }

        private static IComponent RenderOverview(Node node)
        {
            return VStack().S().Children(
                Label("Name".t()).WS().Inline().AutoWidth().SetContent(TextBlock(node.GetString(N.Part.Name))),
                Label("Manufacturer".t()).WS().Inline().AutoWidth().SetContent(NeighborsLinks(node.UID, N.Manufacturer.Type, E.HasManufacturer).WS()),
                Label("Devices".t()),
                Neighbors(() => API.Query.StartAt(node.UID).Out(N.Device.Type, E.PartOf).TakeAll().GetUIDsAsync(),
                          new[] { N.Device.Type }, showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible).S());
        }

        // A case names its device, not the part: the cases of every device this part fits.
        private static IComponent RenderSupportCases(Node node)
        {
            return Neighbors(() => API.Query.StartAt(node.UID).Out(N.Device.Type, E.PartOf).Out(N.SupportCase.Type, E.HasSupportCase).TakeAll().GetUIDsAsync(),
                             new[] { N.SupportCase.Type }, showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible, defaultSortMode: SortModeEnum.RecentFirst).S();
        }
    }
}
