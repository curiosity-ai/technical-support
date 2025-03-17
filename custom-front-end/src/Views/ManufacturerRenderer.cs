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
    public class ManufacturerRenderer : INodeRenderer
    {
        public string NodeType    => N.Manufacturer.Type;
        public string DisplayName => "Manufacturer";
        public string LabelField  => "Name";
        public string Color       => "#106ebe";
        public UIcons Icon        => UIcons.IndustryAlt;

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
            return Neighbors(() => Mosaik.API.Query.StartAt(node.UID).Out(N.Device.Type, E.ManufacturerOf).Union(Mosaik.API.Query.StartAt(node.UID).Out(N.Device.Type, E.ManufacturerOf).TakeAll()).TakeAll().GetUIDsAsync(),
                                    new[] {N.Device.Type, N.Part.Type}, true, FacetDisplayOptions.Visible, defaultSortMode: SortModeEnum.Connectivity);
        }
    }
}