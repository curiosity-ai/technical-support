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
            _container = HubStack(HubTitle("Technical Support Backlog", DefaultRoutes.Home), DefaultRoutes.Home)
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
            var prodName = TextBlock().SemiBold().Tiny().W(10).Grow().Secondary().Ellipsis();

            Mosaik.API.Aggregated.GetNodeNeighbors(sh.Node.UID, N.Device.Type, E.ForDevice, (uid) =>
            {
                Mosaik.API.Aggregated.GetNode(uid[0], n =>
                {
                    prodName.Text = n.GetString("Name");
                });
            });

            var title = TextBlock(sh.Node.GetString(N.SupportCase.Summary)).SemiBold().W(10).Grow().TextLeft().ML(16).NoWrap().Ellipsis();

            var status = Button().Tooltip(sh.Node.GetString(N.SupportCase.Status)).W(32).H(32).NoPadding().NoMargin().NoHover()
                            .Class($"support-case-status-{sh.Node.GetString(N.SupportCase.Status).ToLower()}")
                            .SetIcon(isClosed ? UIcons.CommentAltCheck : UIcons.MessageQuestion);

            var prod = Button().W(200).H(32).NoPadding().NoMargin().NoHover()
                            .ReplaceContent(HStack().AlignItemsCenter().S().Children(
                                prodName.TextLeft().PL(4)));

            var content = HStack().NoWrap().WS().AlignItemsCenter().OverflowHidden();

            content.Children(status, prod, title/*, user*/).Class("support-case-card");
            var btn = Button().WS().ReplaceContent(content);
            btn.OnClick(() => NodePreview.For(sh.Node));

            return new ReplacedResult(btn, rr);
        }

        public dom.HTMLElement Render() => _container.Render();
    }
}