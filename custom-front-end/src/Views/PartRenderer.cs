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
    public class PartRenderer : NodeRendererBase
    {
        public PartRenderer() : base(new SchemaStyleInfo()
        {
            Name        = N.Part.Type,
            DisplayName = "Part",
            LabelField  = N.Part.Name,
            Color       = "#0443D3", // brand-600
            Icon        = UIconHelper.ToCssClass(UIcons.Microchip),
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
