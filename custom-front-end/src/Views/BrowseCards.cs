using H5.Core;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;
using Mosaik;
using Mosaik.Components;
using Mosaik.Components.Nodes;
using Mosaik.Helpers;
using Mosaik.Schema;

namespace TechnicalSupport.FrontEnd
{
    // Faceted browse rows for the Devices and Parts pages. Each renders the
    // node as a card with an icon tile, label, manufacturer and connectivity
    // counts, matching the "Devices / Parts · browse" frames from the design.
    internal static class BrowseCards
    {
        // Support-case backlog row: round status badge, summary + device/case-id meta.
        // Shared by the Support Cases page and the dashboard's recent-cases list.
        public static ReplacedResult RenderSupportCase(SearchHit sh, RenderedSearchResult rr)
        {
            var isClosed = sh.Node.GetString(N.SupportCase.Status) == "Closed";

            // Round status badge: open = an open question (brand), closed = resolved (success).
            // AlignCenter keeps it vertically centered against the two-line body — stack
            // children get their own align-self wrapper, so container align-items is not enough.
            var status = HStack().AlignItemsCenter().AlignCenter().Class("cz-status-icon")
                            .Class(isClosed ? "cz-status-closed" : "cz-status-open")
                            .Tooltip(sh.Node.GetString(N.SupportCase.Status))
                            .Children(Icon(isClosed ? UIcons.CommentAltCheck : UIcons.MessageQuestion));

            var title = TextBlock(sh.Node.GetString(N.SupportCase.SupportCaseSummary)).NoWrap().Ellipsis().TextLeft().Class("cz-card-title");

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

            var chevron = Icon(UIcons.AngleSmallRight).AlignCenter().Class("cz-chevron");

            var content = HStack().NoWrap().WS().AlignItemsCenter().Class("cz-row").Class("cz-card")
                            .Children(status, body, chevron);

            return WrapRow(content, sh.Node, rr);
        }

        public static ReplacedResult RenderDevice(SearchHit sh, RenderedSearchResult rr)
        {
            // Devices have no manufacturer edge in the graph (only parts do), so the
            // card shows the name and connectivity counts only.
            var name = TextBlock(sh.Node.GetString(N.Device.Name)).NoWrap().Ellipsis().TextLeft().Class("cz-card-title");

            var body = VStack().Grow().Class("cz-card-body").Children(name);

            var partsCount = CountFor(sh.Node.UID, N.Part.Type, E.HasPart, "parts");
            var casesCount = CountFor(sh.Node.UID, N.SupportCase.Type, E.HasSupportCase, "cases");
            var counts = HStack().AlignItemsCenter().AlignCenter().Class("cz-counts").Children(partsCount, casesCount);

            var content = HStack().NoWrap().WS().AlignItemsCenter().Class("cz-row").Class("cz-card")
                            .Children(Tile(UIcons.MobileNotch), body, counts, Chevron());

            return WrapRow(content, sh.Node, rr);
        }

        public static ReplacedResult RenderPart(SearchHit sh, RenderedSearchResult rr)
        {
            var name = TextBlock(sh.Node.GetString(N.Part.Name)).NoWrap().Ellipsis().TextLeft().Class("cz-card-title");
            var mfr = ManufacturerLine(sh.Node.UID);

            var body = VStack().Grow().Class("cz-card-body").Children(name, mfr);

            var devicesCount = CountFor(sh.Node.UID, N.Device.Type, E.PartOf, "devices");
            var counts = HStack().AlignItemsCenter().AlignCenter().Class("cz-counts").Children(devicesCount);

            var content = HStack().NoWrap().WS().AlignItemsCenter().Class("cz-row").Class("cz-card")
                            .Children(Tile(UIcons.Microchip), body, counts, Chevron());

            return WrapRow(content, sh.Node, rr);
        }

        private static IComponent Tile(UIcons icon)
        {
            return HStack().AlignItemsCenter().AlignCenter().Class("cz-tile").Children(Icon(icon));
        }

        private static IComponent Chevron()
        {
            return Icon(UIcons.AngleSmallRight).AlignCenter().Class("cz-chevron");
        }

        // A secondary text line filled with the node's manufacturer name (HasManufacturer edge).
        private static IComponent ManufacturerLine(UID.UID128 nodeUID)
        {
            var mfr = TextBlock("").Tiny().NoWrap().Ellipsis().TextLeft().Class("cz-mfr");
            Mosaik.API.Aggregated.GetNodeNeighbors(nodeUID, N.Manufacturer.Type, E.HasManufacturer, (uid) =>
            {
                if (uid.Length > 0)
                {
                    Mosaik.API.Aggregated.GetNode(uid[0], n => mfr.Text = n.GetString("Name"));
                }
            });
            return mfr;
        }

        // A right-aligned "<count> <label>" block, count resolved asynchronously.
        private static IComponent CountFor(UID.UID128 nodeUID, string nodeType, string edge, string label)
        {
            var num = TextBlock("0").Class("cz-count-num");
            Mosaik.API.Aggregated.GetNodeNeighbors(nodeUID, nodeType, edge, (uid) =>
            {
                num.Text = uid.Length.ToString();
            });
            return VStack().Class("cz-count").Children(num, TextBlock(label).Class("cz-count-label"));
        }

        private static ReplacedResult WrapRow(IComponent content, Node node, RenderedSearchResult rr)
        {
            var btn = Button().WS().NoMargin().Class("cz-row-btn").ReplaceContent(content);
            btn.OnClick(() => NodePreview.For(node));
            return new ReplacedResult(btn, rr);
        }
    }
}
