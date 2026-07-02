using H5.Core;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;
using Node = Mosaik.Schema.Node;

namespace TechnicalSupport.FrontEnd
{
    internal class SupportCasesView : IComponent
    {
        private IComponent _container;

        public SupportCasesView(Parameters state)
        {
            _container = HubStack(HubTitle("Support Cases", "#/support-cases"), "#/home")
                            .Section(CreateView(state), grow: true);
        }

        // Optional ?status=Open|Closed route parameter pre-selects the matching
        // Status facet (used by the dashboard's "Open cases" card). The facet is
        // applied as a regular facet, so it shows in the facet bar and can be removed.
        private IComponent CreateView(Parameters state)
        {
            var status = state != null && state.ContainsKey("status") ? state["status"] : null;

            if (string.IsNullOrEmpty(status))
                return CreateSearchArea(null);

            return Defer(async () =>
            {
                var statusNode = await Mosaik.API.Nodes.GetAsync(N.Status.Type, status);
                return CreateSearchArea(statusNode);
            }).S();
        }

        private IComponent CreateSearchArea(Node statusNode)
        {
            return SearchArea().WithFacets().OnSearch(s =>
                            {
                                s.SetBeforeTypesFacet(N.SupportCase.Type);
                                if (statusNode != null) s.SetRelatedFacet(N.Status.Type, statusNode.UID);
                            })
                            .Renderer(r => r.WithCustomizedRenderer((sh, rr) =>
                            {
                                return BrowseCards.RenderSupportCase(sh, rr);
                            })).S();
        }

        public dom.HTMLElement Render() => _container.Render();
    }
}
