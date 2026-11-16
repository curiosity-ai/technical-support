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
    /// <summary>Every device, searchable and faceted.</summary>
    public sealed class DevicesView : IComponent
    {
        internal const string Route = "#/devices";

        private readonly IComponent _content;

        public DevicesView(Parameters state)
        {
            _content = ShownView.Track(this, Hub("Devices".t(), Route)
               .Section(CreateSearch(), grow: true)
               .S());
        }

        internal static bool IsShowing() => ShownView.Is<DevicesView>();

        // OnSearch runs on every search the area performs, so the scope survives the user's first keystroke.
        private static IComponent CreateSearch()
        {
            return SearchArea()
               .WithFacets()
               .OnSearch(sr => sr.SetBeforeTypesFacet(N.Device.Type))
               .S();
        }

        public HTMLElement Render() => _content.Render();
    }
}
