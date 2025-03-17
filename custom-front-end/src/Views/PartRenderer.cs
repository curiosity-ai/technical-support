using UID;
using System;
using System.Linq;
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
    public class PartRenderer : INodeRenderer
    {
        public string NodeType    => N.Part.Type;
        public string DisplayName => "Part";
        public string LabelField  => "Name";
        public string Color       => "#b9babd";
        public UIcons Icon        => UIcons.BoxOpenFull;

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
            return Pivot().S().Pivot("product", PivotTitle("Overview"), () => RenderOverview(node))
                              .Pivot("support", PivotTitle("Support"),  () => RenderSupport(node));
        }

        private IComponent RenderOverview(Node node)
        {
            return VStack().S().Children(
                        Label("Name").WS().Inline().AutoWidth().SetContent(TextBlock(node.GetString(N.Part.Name))),
                        Label("Manufacturer").WS().Inline().AutoWidth().SetContent(NeighborsLinks(node.UID, N.Manufacturer.Type, E.HasManufacturer).WS()),
                        Label("Devices"),
                        Neighbors(() => Mosaik.API.Query.StartAt(node.UID).Out(N.Device.Type, E.PartOf).TakeAll().GetUIDsAsync(), new[] { N.Device.Type }, showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible).S());
        }

        private IComponent RenderSupport(Node node)
        {
            return Neighbors(() => Mosaik.API.Query.StartAt(node.UID).Out(N.SupportCase.Type).TakeAll().GetUIDsAsync(),
                             new[] { N.SupportCase.Type}, true, FacetDisplayOptions.Visible, defaultSortMode: SortModeEnum.RecentFirst);
        }
    }
}