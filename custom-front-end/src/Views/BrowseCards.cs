using System;
using System.Linq;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;
using Mosaik;
using Mosaik.Components;
using Mosaik.Helpers;
using Mosaik.Schema;
using Node = Mosaik.Schema.Node;

namespace TechnicalSupport.FrontEnd
{
    // Row customizations for the Devices, Parts and Support Cases browse pages.
    //
    // A search result is an OmniResult now, so these fill the type-specific pieces into
    // the standard row - a badge, footer entries and the stylesheet hooks - instead of
    // replacing it with a hand-built card. The icon tile and its colour, the title and
    // the click that opens the node all come from the node's renderer, which is also why
    // a case's icon follows its status here without this file having to draw it.
    internal static class BrowseCards
    {
        // Support-case backlog row: status badge, plus the device it was reported for
        // and the case id as footer entries. Shared by the Support Cases page and the
        // dashboard's recent-cases list.
        public static OmniResult<Node> CustomizeSupportCase(OmniResult<Node> result)
        {
            var node   = result.Result;
            var status = node.GetString(N.SupportCase.Status);

            result.SetBadge(status)
                  .AddFooterEntry(NeighborLabel(node.UID, N.Device.Type, E.ForDevice, N.Device.Name, UIcons.MobileNotch));

            var caseId = node.GetString(N.SupportCase.Id);

            if (!string.IsNullOrEmpty(caseId))
            {
                result.AddFooterEntry(InlineLabel(caseId).SetIcon(UIcons.Hashtag).Class("cz-meta-mono"));
            }

            return result.Class("support-case-card")
                         .Class($"support-case-status-{status.ToLower()}");
        }

        // Devices have no manufacturer edge in the graph (only parts do), so the row
        // carries the connectivity counts only.
        public static OmniResult<Node> CustomizeDevice(OmniResult<Node> result)
        {
            var uid = result.Result.UID;

            return result.AddFooterEntry(CountLabel(uid, N.Part.Type,        E.HasPart,        "parts", UIcons.Tools))
                         .AddFooterEntry(CountLabel(uid, N.SupportCase.Type, E.HasSupportCase, "cases", UIcons.CommentsQuestion))
                         .Class("cz-browse-row");
        }

        public static OmniResult<Node> CustomizePart(OmniResult<Node> result)
        {
            var uid = result.Result.UID;

            return result.AddFooterEntry(NeighborLabel(uid, N.Manufacturer.Type, E.HasManufacturer, N.Manufacturer.Name, UIcons.IndustryAlt))
                         .AddFooterEntry(CountLabel(uid, N.Device.Type, E.PartOf, "devices", UIcons.MobileNotch))
                         .Class("cz-browse-row");
        }

        // A footer entry naming the first neighbor across an edge, resolved once the row
        // is built. The label stays empty (and so renders as nothing) when there is none.
        private static InlineLabel NeighborLabel(UID.UID128 nodeUID, string nodeType, string edge, string labelField, UIcons icon)
        {
            return InlineLabel(async label =>
            {
                var neighbors = await Mosaik.API.Query.StartAt(nodeUID).Out(nodeType, edge).GetAsync();
                var first     = neighbors.Nodes.FirstOrDefault();

                if (first is object)
                {
                    label.SetText(first.GetString(labelField)).SetIcon(icon);
                }
            });
        }

        // A footer entry with the number of neighbors across an edge ("12 parts").
        private static InlineLabel CountLabel(UID.UID128 nodeUID, string nodeType, string edge, string label, UIcons icon)
        {
            return InlineLabel(async entry =>
            {
                var count = await Mosaik.API.Aggregated.GetNeighborCountAsync(nodeUID, nodeType, edge);
                entry.SetText($"{count:n0} {label}").SetIcon(icon);
            });
        }
    }
}
