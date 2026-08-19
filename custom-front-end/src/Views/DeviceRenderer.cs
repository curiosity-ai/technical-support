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

using Transpose;
using static Transpose.Core.dom;
using Node = Mosaik.Schema.Node;

namespace TechnicalSupport.FrontEnd
{
    public class DeviceRenderer : NodeRendererBase
    {
        public DeviceRenderer() : base(new SchemaStyleInfo()
        {
            Name        = N.Device.Type,
            DisplayName = "Device",
            LabelField  = N.Device.Name,
            Color       = "#0443D3", // brand-600
            Icon        = UIconHelper.ToCssClass(UIcons.MobileNotch),
        })
        { }

        public override async Task<OmniResult<Node>> PreviewAsync(Node node, Parameters state)
        {
            return NodeResult.For(this, node)
                             .SetModalContent(CreateView(node, state))
                             .ModalSize(80.vw(), 80.vh());
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
