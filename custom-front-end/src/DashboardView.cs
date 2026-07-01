using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H5.Core;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;
using Mosaik;
using Mosaik.Components;
using Mosaik.Schema;

namespace TechnicalSupport.FrontEnd
{
    // Landing dashboard. Replaces the old all-cases search home and the support
    // chat page: gives an at-a-glance overview of the workspace (headline counts,
    // an open/closed case breakdown) and a live list of the most recent cases.
    public class DashboardView : IComponent
    {
        private readonly IComponent _container;

        public DashboardView(Parameters state)
        {
            _container = HubStack(HubTitle("Dashboard", DefaultRoutes.Home), DefaultRoutes.Home)
                            .Section(CreateStats())
                            .Section(CreateStatusBreakdown())
                            .Section(CreateRecentCases(state), grow: true);
        }

        // ---- Top row: headline counts -----------------------------------------

        private IComponent CreateStats()
        {
            return HStack().WS().Class("cz-stat-grid").Children(
                StatCard(UIcons.Boxes,            "Devices",       "#/devices",       () => CountOfTypeAsync(N.Device.Type)),
                StatCard(UIcons.Tools,            "Parts",         "#/parts",         () => CountOfTypeAsync(N.Part.Type)),
                StatCard(UIcons.CommentsQuestion, "Support cases", "#/support-cases", () => CountOfTypeAsync(N.SupportCase.Type)),
                StatCard(UIcons.MessageQuestion,  "Open cases",    "#/support-cases", () => CountByStatusAsync("Open")));
        }

        private IComponent StatCard(UIcons icon, string label, string route, Func<Task<int>> count)
        {
            var number = TextBlock("—").Class("cz-stat-num");
            LoadCount(number, count);

            var content = HStack().WS().AlignItemsCenter().Class("cz-stat").Children(
                            HStack().AlignItemsCenter().AlignCenter().Class("cz-tile").Children(Icon(icon)),
                            VStack().Class("cz-stat-body").Children(number, TextBlock(label).Class("cz-stat-label")));

            return Button().NoMargin().Class("cz-stat-btn").ReplaceContent(content).OnClick(() => Router.Navigate(route));
        }

        // ---- Open / closed case breakdown --------------------------------------

        private IComponent CreateStatusBreakdown()
        {
            var section = VStack().WS().Class("cz-panel").Children(
                            TextBlock("Case status").Class("cz-panel-title"));

            section.Add(Defer(async () =>
            {
                var open   = await SafeCountAsync(() => CountByStatusAsync("Open"));
                var closed = await SafeCountAsync(() => CountByStatusAsync("Closed"));
                var total  = open + closed;

                var openPct   = total > 0 ? (int)Math.Round(open   * 100.0 / total) : 0;
                var closedPct = total > 0 ? (int)Math.Round(closed * 100.0 / total) : 0;

                var bar = HStack().WS().Class("cz-bar").Children(
                            HStack().Class("cz-bar-seg").Class("cz-bar-open").W(openPct.percent()),
                            HStack().Class("cz-bar-seg").Class("cz-bar-closed").W(closedPct.percent()));

                var legend = HStack().WS().Class("cz-legend").Children(
                            LegendItem("cz-dot-open",   "Open",   open),
                            LegendItem("cz-dot-closed", "Closed", closed));

                return VStack().WS().Children(bar, legend);
            }));

            return section;
        }

        private IComponent LegendItem(string dotClass, string label, int count)
        {
            return HStack().AlignItemsCenter().Class("cz-legend-item").Children(
                        HStack().Class("cz-dot").Class(dotClass),
                        TextBlock(label).Class("cz-legend-label"),
                        TextBlock(count.ToString("n0")).Class("cz-legend-num"));
        }

        // ---- Recent support cases ----------------------------------------------

        private IComponent CreateRecentCases(Parameters state)
        {
            var heading = TextBlock("Recent support cases").Class("cz-panel-title");

            var sa = SearchArea();
            sa.OnSearch(s => s.SetBeforeTypesFacet(N.SupportCase.Type).WithSortMode(SortModeEnum.RecentFirst));
            sa.Renderer(r => r.WithCustomizedRenderer((sh, rr) => BrowseCards.RenderSupportCase(sh, rr)));

            return VStack().WS().Grow().Class("cz-panel").Children(heading, sa.S());
        }

        // ---- Count helpers -----------------------------------------------------

        // Total number of nodes of a type. An empty-query search with a type facet
        // is the workspace's "browse all" mode, so its total count is the type count.
        private static async Task<int> CountOfTypeAsync(string nodeType)
        {
            var req = new SearchRequest("").SetBeforeTypesFacet(nodeType);
            var res = await Mosaik.API.Search.SearchAsync(req, 0, 1, CancellationToken.None, null);
            return ParseCount(res?.Count);
        }

        // Number of support cases linked to a given Status node (Open / Closed),
        // counted via the StatusOf edge without materialising the cases.
        private static async Task<int> CountByStatusAsync(string statusValue)
        {
            var statusNode = await Mosaik.API.Nodes.GetAsync(N.Status.Type, statusValue);
            if (statusNode == null) return 0;
            return await Mosaik.API.Aggregated.GetNeighborCountAsync(statusNode.UID, N.SupportCase.Type, E.StatusOf);
        }

        // Fire-and-forget: fills the card's number in once the count resolves, so
        // the dashboard renders immediately rather than blocking on every count.
        private static async void LoadCount(TextBlock target, Func<Task<int>> count)
        {
            target.Text = (await SafeCountAsync(count)).ToString("n0");
        }

        private static async Task<int> SafeCountAsync(Func<Task<int>> count)
        {
            try { return await count(); }
            catch (Exception) { return 0; }
        }

        private static int ParseCount(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            var digits = new string(value.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out var n) ? n : 0;
        }

        public dom.HTMLElement Render() => _container.Render();
    }
}
