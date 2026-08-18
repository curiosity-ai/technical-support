using Transpose.Core;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;
using static Transpose.Core.dom;
using Mosaik;
using System;
using System.Linq;
using Mosaik.Helpers;
using Mosaik.Components.Nodes;
using Mosaik.Components;
using Mosaik.Schema;
using Node = Mosaik.Schema.Node;

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
            sa.Renderer(r => r.CustomizeResult(RenderSupportCase));

            return sa.S();
        }

        // The result row is now an OmniResult, so a customized support case is the standard row with
        // the case-specific pieces filled in (status badge, the device it was reported for) instead of
        // a hand-built replacement component. The icon tile and its color, the title, the timestamp and
        // the click that opens the case all come from SupportCaseRenderer via NodeResult.For.
        public static OmniResult<Node> RenderSupportCase(OmniResult<Node> result)
        {
            var node   = result.Result;
            var status = node.GetString(N.SupportCase.Status);

            var device = InlineLabel(async label =>
            {
                var devices = await Mosaik.API.Query.StartAt(node.UID).Out(N.Device.Type, E.ForDevice).GetAsync();
                var first   = devices.Nodes.FirstOrDefault();

                if (first is object)
                {
                    label.SetText(first.GetString(N.Device.Name)).SetIcon(UIcons.BoxOpenFull);
                }
            });

            return result.SetBadge(status)
                         .AddFooterEntry(device)
                         .Class($"support-case-status-{status.ToLower()}")
                         .Class("support-case-card");
        }

        public dom.HTMLElement Render() => _container.Render();
    }
}
