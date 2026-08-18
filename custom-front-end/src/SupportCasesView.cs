using Transpose.Core;
using Tesserae;
using static Mosaik.UI;

namespace TechnicalSupport.FrontEnd
{
    internal class SupportCasesView : IComponent
    {
        private IComponent _container;

        public SupportCasesView(Parameters state)
        {
            _container = HubStack(HubTitle("Support Cases", "#/support-cases"), "#/home")
                            .Section(CreateView(), grow: true);
        }

        private IComponent CreateView()
        {
            return SearchArea().WithFacets().OnSearch(s => s.SetBeforeTypesFacet(N.SupportCase.Type))
                            .Renderer(r => r.CustomizeResult(SupportHomeView.RenderSupportCase)).S();
        }

        public dom.HTMLElement Render() => _container.Render();
    }
}