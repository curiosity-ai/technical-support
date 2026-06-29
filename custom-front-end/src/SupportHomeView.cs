using H5.Core;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;
using static H5.Core.dom;
using Mosaik;
using System;
using Mosaik.Helpers;
using Mosaik.Components.Nodes;
using Mosaik.Components;
using Mosaik.Schema;

namespace TechnicalSupport.FrontEnd
{
    public class SupportHomeView : IComponent
    {
        private IComponent _container;
        public SupportHomeView(Parameters state)
        {
            _container = HubStack(HubTitle("Support backlog", DefaultRoutes.Home), DefaultRoutes.Home)
                            .Section(CreateSearch(state).S(), grow: true);
        }

        private IComponent CreateSearch(Parameters state)
        {
            var sa = SearchArea();
            sa.OnSearch(s => s.SetBeforeTypesFacet(N.SupportCase.Type));
            sa.WithFacets();
            sa.Renderer(r => r.WithCustomizedRenderer((sh, rr) =>
            {
                return RenderSupportCase(sh, rr);
            }));

            return sa.S();
        }

        public static ReplacedResult RenderSupportCase(SearchHit sh, RenderedSearchResult rr)
        {
            var isClosed = sh.Node.GetString(N.SupportCase.Status) == "Closed";

            // Round status badge: open = an open question (brand), closed = resolved (success).
            var status = HStack().AlignItemsCenter().Class("cz-status-icon")
                            .Class(isClosed ? "cz-status-closed" : "cz-status-open")
                            .Tooltip(sh.Node.GetString(N.SupportCase.Status))
                            .Children(Icon(isClosed ? UIcons.CommentAltCheck : UIcons.MessageQuestion));

            var title = TextBlock(sh.Node.GetString(N.SupportCase.SupportCaseSummary)).NoWrap().Ellipsis().Class("cz-card-title");

            // Device chip — resolved asynchronously by following the ForDevice edge.
            var deviceName = TextBlock("").Tiny().NoWrap().Ellipsis();
            Mosaik.API.Aggregated.GetNodeNeighbors(sh.Node.UID, N.Device.Type, E.ForDevice, (uid) =>
            {
                if (uid.Length > 0)
                {
                    Mosaik.API.Aggregated.GetNode(uid[0], n => deviceName.Text = n.GetString("Name"));
                }
            });
            var deviceChip = HStack().AlignItemsCenter().Class("cz-chip").Children(Icon(UIcons.MobileNotch).Class("cz-chip-icon"), deviceName);

            var caseId = TextBlock(sh.Node.GetString(N.SupportCase.Id)).Class("cz-meta-mono");

            var meta = HStack().AlignItemsCenter().Class("cz-card-meta").Children(deviceChip, caseId);

            var body = VStack().Grow().Class("cz-card-body").Children(title, meta);

            var chevron = Icon(UIcons.AngleSmallRight).Class("cz-chevron");

            var content = HStack().NoWrap().WS().AlignItemsCenter().Class("cz-row").Class("cz-card")
                            .Children(status, body, chevron);

            var btn = Button().WS().NoMargin().Class("cz-row-btn").ReplaceContent(content);
            btn.OnClick(() => NodePreview.For(sh.Node));

            return new ReplacedResult(btn, rr);
        }

        public dom.HTMLElement Render() => _container.Render();
    }
}