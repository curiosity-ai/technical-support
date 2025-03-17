using System;
using System.Linq;
using System.Threading.Tasks;
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

            // If there are any custom routes to register, do that here (via Router.Register) before App.Initialize is called
            Router.Register("hello-world", state => App.ShowDefault(TextBlock("Hello World !")));

            Router.Register("#/devices", (state) => App.ShowDefault(new DevicesView(state)));
            Router.Register("#/parts", (state) => App.ShowDefault(new PartsView(state)));
            Router.Register("#/support-cases", (state) => App.ShowDefault(new SupportCasesView(state)));

            App.Initialize(Configure, OnLoad);
        }

        private static void Configure(App.DefaultSettings settings)
        {
            // You can configure the system default settings here
            // Check the DefaultSettings class for more details of what can be configured

            settings.HomeView = (state) => new SupportHomeView(state);


            App.Sidebar.OnSidebarRebuild_BeforeHeader
            App.Sidebar.OnSidebarRebuild_AfterHeader
            App.Sidebar.OnSidebarRebuild_BeforeFooter
            App.Sidebar.OnSidebarRebuild_AfterFooter

            App.Sidebar.OnSidebarRebuild_BeforeFooter += (sidebar, mode, tracker) =>
            {
                switch(mode)
                {
                    case App.Sidebar.Mode.Default:
                    {
                        var kbDevices = new SidebarButton("devices", UIcons.Boxes, "Devices").OnClick(() => Router.Navigate("#/devices"));
                        tracker.Add(() => kbDevices.IsSelected = window.location.hash.Contains("#/devices"));
                        sidebar.AddContent(kbDevices);

                        var kbParts = new SidebarButton("parts", UIcons.Tools, "Parts").OnClick(() => Router.Navigate("#/parts"));
                        tracker.Add(() => kbParts.IsSelected = window.location.hash.Contains("#/parts"));
                        sidebar.AddContent(kbParts);

                        var kbCases= new SidebarButton("support-cases", UIcons.CommentsQuestion, "Support Cases").OnClick(() => Router.Navigate("#/support-cases"));
                        tracker.Add(() => kbCases.IsSelected = window.location.hash.Contains("#/support-cases"));
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

        private static void OnLoad()
        {
            // Any code to run after the system loads should go here
        }
    }
}