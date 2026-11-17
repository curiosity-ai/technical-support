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
    /// <summary>A manufacturer: the devices and parts it makes.</summary>
    public sealed class ManufacturerRenderer : NodeRendererBase
    {
        public ManufacturerRenderer() : base(new SchemaStyleInfo
        {
            Name        = N.Manufacturer.Type,
            DisplayName = "Manufacturer",
            LabelField  = N.Manufacturer.Name,
            Color       = "#106ebe",
            Icon        = UIcons.IndustryAlt.ToString(),
        })
        {
        }

        protected override IEnumerable<NodeTab> BuildTabs(Node node, IReadOnlyList<RelationshipGroup> groups, NodeView nodeView)
        {
            yield return new NodeTab { Id = "products", Label = "Products".t(), TabTitle = SegmentTitle("Products".t()), TabContent = () => RenderProducts(node), Scrolls = false };
        }

        private static IComponent RenderProducts(Node node)
        {
            return Neighbors(() => API.Query.StartAt(node.UID).Out(new[] { N.Device.Type, N.Part.Type }, new[] { E.ManufacturerOf }).TakeAll().GetUIDsAsync(),
                             new[] { N.Device.Type, N.Part.Type }, showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible, defaultSortMode: SortModeEnum.Connectivity).S();
        }
    }
}
