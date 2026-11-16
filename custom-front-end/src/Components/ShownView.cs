using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mosaik;
using Mosaik.Components;
using Mosaik.Helpers;
using Mosaik.Schema;
using Mosaik.Views;
using Tesserae;
using TNT;
using UID;
using static Tesserae.UI;
using static Mosaik.UI;
using static TNT.T;
using static Transpose.Core.dom;
using API  = Mosaik.API;
using Node = Mosaik.Schema.Node;


namespace TechnicalSupport.FrontEnd
{
    /// <summary>
    /// Which of this application's pages is on screen, for the sidebar entries' highlight.
    /// </summary>
    /// <remarks>
    /// The route handlers activate their views asynchronously (AppRouting.ActivateAsync loads the view's code first),
    /// so the sidebar has already refreshed for the navigation when the new page appears. The page claims the slot when
    /// it is mounted and asks the sidebar to refresh again; whichever page was mounted last wins, whatever order the old
    /// page is removed in. A page that is removed without a successor of ours (a node page, the search page) releases
    /// the slot and refreshes the sidebar again.
    /// </remarks>
    internal static class ShownView
    {
        private static IComponent s_current;

        /// <summary>Tracks <paramref name="view"/> as the shown page while <paramref name="content"/> is mounted.</summary>
        public static T Track<T>(IComponent view, T content) where T : IComponent
        {
            content.WhenMounted(() =>
            {
                s_current = view;
                AppSidebar.RefreshSelected();
            });

            content.WhenRemoved(() =>
            {
                if (s_current != view) return; //Another page has claimed it already

                s_current = null;
                AppSidebar.RefreshSelected();
            });

            return content;
        }

        public static bool Is<TView>() where TView : IComponent => s_current is TView;
    }
}
