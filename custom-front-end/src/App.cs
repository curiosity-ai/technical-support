// The entry point: routes, AppSettings and the sidebar entries - nothing else.
//
// Every C# file in this project opens with the same usings block; an unused using costs nothing.
// N, E, Endpoints, AI_Tools and AIAssistants come from generated/ and need no using: they are global types.

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
    internal static class SupportApp
    {
        private static void Main()
        {
            Mosaik.Admin.LazyLoad();

            //You can point the front-end at another server for localhost testing by setting App.ServerURL = "http://localhost:8080/api" here.

            App.Name = "Technical Support";

            // Custom routes are registered BEFORE App.Initialize. A handler activates its view (which keeps the
            // view out of the code every visitor downloads first) and hands it to App.ShowDefault.
            Router.Register("hello-world", state => App.ShowDefault(TextBlock("Hello World !".t())));

            Router.Register(DevicesView.Route,      async state => App.ShowDefault(await AppRouting.ActivateAsync<DevicesView>(state),      title: "Devices".t()));
            Router.Register(PartsView.Route,        async state => App.ShowDefault(await AppRouting.ActivateAsync<PartsView>(state),        title: "Parts".t()));
            Router.Register(SupportCasesView.Route, async state => App.ShowDefault(await AppRouting.ActivateAsync<SupportCasesView>(state), title: "Support Cases".t()));
            Router.Register(SupportChatView.Route,  async state => App.ShowDefault(await AppRouting.ActivateAsync<SupportChatView>(state),  title: "Support".t()));

            App.Initialize(Configure, OnLoad).FireAndForget();
        }

        /// <summary>Runs while the app is being built: everything this application overrides.</summary>
        private static void Configure(AppSettings settings)
        {
            settings.HomeView = state => new SupportHomeView(state);

            // The sidebar only offers what a support team uses: Search, the AI assistant and the entries below.
            settings.OverrideInterfaceSettings = interfaceSettings =>
            {
                interfaceSettings.EnableInbox    = false;
                interfaceSettings.EnableContacts = false;
                interfaceSettings.EnableNotes    = false;
                interfaceSettings.EnableApps     = false;
                interfaceSettings.DisableSpaces  = true;
            };

            AppSidebar.OnSidebarRebuild_BeforeFooter += (sidebar, mode, refresh) =>
            {
                if (mode != AppSidebar.Mode.Default) return;

                // UIcons VALUE, never its name as text: the string overload of SidebarButton binds an emoji.
                var support = new SidebarButton("support", UIcons.ChatbotSpeechBubble, "Support".t()).OnClick(() => Router.Navigate(SupportChatView.Route));
                refresh.Add(() => support.IsSelected = SupportChatView.IsShowing());
                sidebar.AddContent(support);

                var devices = new SidebarButton("devices", UIcons.Boxes, "Devices".t()).OnClick(() => Router.Navigate(DevicesView.Route));
                refresh.Add(() => devices.IsSelected = DevicesView.IsShowing());
                sidebar.AddContent(devices);

                var parts = new SidebarButton("parts", UIcons.Tools, "Parts".t()).OnClick(() => Router.Navigate(PartsView.Route));
                refresh.Add(() => parts.IsSelected = PartsView.IsShowing());
                sidebar.AddContent(parts);

                var cases = new SidebarButton("support-cases", UIcons.CommentsQuestion, "Support Cases".t()).OnClick(() => Router.Navigate(SupportCasesView.Route));
                refresh.Add(() => cases.IsSelected = SupportCasesView.IsShowing());
                sidebar.AddContent(cases);
            };
        }

        /// <summary>Runs once the app has loaded and the user is signed in.</summary>
        private static void OnLoad()
        {
        }
    }
}
