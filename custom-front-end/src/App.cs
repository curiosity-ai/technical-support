using System;
using System.Linq;
using Mosaik;
using Mosaik.Components;
using Mosaik.Schema;
using Mosaik.Views;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;
using static H5.Core.dom;

namespace TechnicalSupport.FrontEnd
{
    internal static class SupportApp
    {
        private static void Main()
        {
            Mosaik.Admin.LazyLoad();

            //App.ServerURL = "http://localhost:8080/api";

            App.Name = "Technical Support";

            // If there are any custom routes to register, do that here (via Router.Register) before App.Initialize is called
            Router.Register("hello-world", state => App.ShowDefault(TextBlock("Hello World !")));

            Router.Register("#/devices",       (state) => App.ShowDefault(new DevicesView(state)));
            Router.Register("#/parts",         (state) => App.ShowDefault(new PartsView(state)));
            Router.Register("#/support-cases", (state) => App.ShowDefault(new SupportCasesView(state)));
            Router.Register("#/support",       (state) => App.ShowDefault(new SupportChat(state)));

            App.Initialize(Configure, OnLoad);
        }


        private static void Configure(App.DefaultSettings settings)
        {
            // You can configure the system default settings here
            // Check the DefaultSettings class for more details of what can be configured

            settings.HomeView = (state) => new DashboardView(state);


            App.Sidebar.OnSidebarRebuild_BeforeFooter += (sidebar, mode, tracker) =>
            {
                switch (mode)
                {
                    case App.Sidebar.Mode.Default:
                    {
                        var support = new SidebarButton("support", UIcons.ChatbotSpeechBubble, "Support").OnClick(() => Router.Navigate("#/support"));
                        tracker.Add(() => support.IsSelected = IsOnRoute("#/support"));
                        sidebar.AddContent(support);

                        var kbDevices = new SidebarButton("devices", UIcons.Boxes, "Devices").OnClick(() => Router.Navigate("#/devices"));
                        tracker.Add(() => kbDevices.IsSelected = IsOnRoute("#/devices"));
                        sidebar.AddContent(kbDevices);

                        var kbParts = new SidebarButton("parts", UIcons.Tools, "Parts").OnClick(() => Router.Navigate("#/parts"));
                        tracker.Add(() => kbParts.IsSelected = IsOnRoute("#/parts"));
                        sidebar.AddContent(kbParts);

                        var kbCases = new SidebarButton("support-cases", UIcons.CommentsQuestion, "Support Cases").OnClick(() => Router.Navigate("#/support-cases"));
                        tracker.Add(() => kbCases.IsSelected = IsOnRoute("#/support-cases"));
                        sidebar.AddContent(kbCases);
                        break;
                    }
                    case App.Sidebar.Mode.UserPreferences:
                    {
                        break;
                    }
                    case App.Sidebar.Mode.AdminSettings:
                    {
                        break;
                    }
                }
            };
        }

        // Exact route match (allowing ?query and /sub-path suffixes) so overlapping
        // prefixes like #/support and #/support-cases don't both show as selected
        private static bool IsOnRoute(string route)
        {
            var hash = window.location.hash;
            return hash == route || hash.StartsWith(route + "?") || hash.StartsWith(route + "/");
        }

        private static void OnLoad()
        {
            // Any code to run after the system loads should go here
        }
    }
}