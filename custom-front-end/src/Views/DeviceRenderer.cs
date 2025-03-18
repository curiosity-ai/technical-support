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

using H5;
using static H5.Core.dom;
using Node = Mosaik.Schema.Node;

namespace TechnicalSupport.FrontEnd
{
    public class DeviceRenderer : INodeRenderer
    {
        public string NodeType    => N.Device.Type;
        public string DisplayName => "Device";
        public string LabelField  => "Name";
        public string Color       => "#346eeb";
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
            return Pivot().S().Pivot("product", PivotTitle("Product Page"), () => RenderDevicePage(node))
                              .Pivot("support", PivotTitle("Support"),      () => RenderSupport(node))
                              .Pivot("graph",   PivotTitle("Graph"),        () => RenderGraph(node));
        }

        private IComponent RenderDevicePage(Node node)
        {
            return VStack().S().Children(
                        Label("Name").WS().Inline().AutoWidth().SetContent(TextBlock(node.GetString(N.Device.Name))),
                        Label("Manufacturer").WS().Inline().AutoWidth().SetContent(NeighborsLinks(node.UID, N.Manufacturer.Type, E.HasManufacturer).WS()),
                        Label("Parts"),
                        Neighbors(() => Mosaik.API.Query.StartAt(node.UID).Out(N.Part.Type, E.HasPart).TakeAll().GetUIDsAsync(), new[] { N.Part.Type }, showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible).S());
        }

        private IComponent RenderSupport(Node node)
        {
            return Neighbors(() => Mosaik.API.Query.StartAt(node.UID).Out(N.SupportCase.Type).TakeAll().GetUIDsAsync(),
                             new[] { N.SupportCase.Type}, true, FacetDisplayOptions.Visible, defaultSortMode: SortModeEnum.RecentFirst);
        }

        private IComponent RenderGraph(Node node)
        {
            return Defer(async () =>
            {
                var queryResult = await Mosaik.API.Query.StartAt(node.UID).Out().TakeAll().GetUIDsAsync();
                return GraphExplorerView.ComponentFor(enableInteraction: true, uids: queryResult.UIDs.Append(node.UID).ToArray()).S();
            }).S();
        }
    }
}