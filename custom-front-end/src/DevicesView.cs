using H5.Core;
using Tesserae;
using static Mosaik.UI;

namespace TechnicalSupport.FrontEnd
{
    internal class DevicesView : IComponent
    {
        private IComponent _container;

        public DevicesView(Parameters state)
        {
            _container = HubStack(HubTitle("Devices", "#/devices"), "#/home")
                            .Section(CreateView(), grow: true);
        }

        private IComponent CreateView()
        {
            return SearchArea().WithFacets().OnSearch(s => s.SetBeforeTypesFacet(N.Device.Type)).S();
        }

        public dom.HTMLElement Render() => _container.Render();
    }
}