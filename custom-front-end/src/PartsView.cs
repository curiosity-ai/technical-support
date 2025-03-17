using H5.Core;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;
using System;

namespace TechnicalSupport.FrontEnd
{
    internal class PartsView : IComponent
    {
        private IComponent _container;

        public PartsView(Parameters state)
        {
            _container = HubStack(HubTitle("Parts", "#/parts"), "#/home")
                            .Section(CreateView(), grow: true);
        }

        private IComponent CreateView()
        {
            return SearchArea().WithFacets().OnSearch(s => s.SetBeforeTypesFacet(N.Part.Type)).S();
        }

        public dom.HTMLElement Render() => _container.Render();
    }
}