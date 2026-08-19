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
    public class ManufacturerRenderer : NodeRendererBase
    {
        public ManufacturerRenderer() : base(new SchemaStyleInfo()
        {
            Name        = N.Manufacturer.Type,
            DisplayName = "Manufacturer",
            LabelField  = N.Manufacturer.Name,
            Color       = "#555555", // ink-700 — neutral, per design
            Icon        = UIconHelper.ToCssClass(UIcons.IndustryAlt),
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
            return Neighbors(() => Mosaik.API.Query.StartAt(node.UID).Out(N.Device.Type, E.ManufacturerOf).Union(Mosaik.API.Query.StartAt(node.UID).Out(N.Device.Type, E.ManufacturerOf).TakeAll()).TakeAll().GetUIDsAsync(),
                                    new[] {N.Device.Type, N.Part.Type}, true, FacetDisplayOptions.Visible, defaultSortMode: SortModeEnum.Connectivity);
        }
    }
}
