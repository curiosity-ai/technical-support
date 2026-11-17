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
    /// <summary>Every spare part, searchable and faceted.</summary>
    public sealed class PartsView : IComponent
    {
        internal const string Route = "#/parts";

        private readonly IComponent _content;

        public PartsView(Parameters state)
        {
            _content = Hub("Parts".t(), Route)
               .Section(CreateSearch(), grow: true)
               .S();
        }

        // Read from the URL: the sidebar asks as soon as the address changes, before this view is activated.
        internal static bool IsShowing() => (window.location.hash ?? "").Split('?')[0] == Route;

        // OnSearch runs on every search the area performs, so the scope survives the user's first keystroke.
        private static IComponent CreateSearch()
        {
            return SearchArea()
               .WithFacets()
               .OnSearch(sr => sr.SetBeforeTypesFacet(N.Part.Type))
               .S();
        }

        public HTMLElement Render() => _content.Render();
    }
}
