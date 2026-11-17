# Curiosity Workspace // Custom Front Ends

## Table of Contents

1. [Introduction](#introduction)
1. [Installing Transpose](#installing-transpose)
1. [Downloading a Front End Template](#downloading-a-front-end-template)
1. [Project Layout](#project-layout)
1. [Debugging a Front-End Locally](#debugging-a-front-end-locally)
1. [Deploying a Front-End](#deploying-a-front-end)
1. [Tesserae Components](#tesserae-components)
1. [Curiosity Components](#curiosity-components)
1. [Node Renderers](#node-renderers)
1. [Routing and Sidebar](#routing-and-sidebar)
1. [Useful Curiosity Component](#useful-curiosity-components)
1. [Custom Chat Interface](#custom-chat-interface)
1. [Conclusion](#conclusion)

## Introduction

Curiosity Workspaces also allow you to create custom front-ends, enabling you to design tailored user interfaces that meet your specific needs. A custom front-end is a web-based interface that interacts with your workspace, providing a seamless and intuitive way for users to engage with your data, workflows, and AI models.

Custom front-ends are built as single-page applications, using Curiosity-provided components and libraries, and securely connect to your workspace's graph database, NLP models, and other functionalities. You can design them to be as simple or complex as needed, from lightweight search apps, simple dashboards to fully interactive applications. With authentication controls, you can restrict access to certain users or roles, ensuring a secure and personalized experience.

## Installing Transpose

Transpose is a C# to JavaScript compiler that allows developers to write web applications using C# while targeting the JavaScript runtime. By translating C# code into JavaScript, Transpose enables developers to leverage their existing C# skills for client-side development. It compiles at the latest C# language version, including LINQ, async/await, and custom attributes, while enabling easy integration with existing Javascript libraries.

The projects need the .NET 11 SDK (pinned in [`global.json`](/global.json)). Then install the [Transpose compiler](https://www.nuget.org/packages/Transpose.Compiler) as a dotnet global tool. The build runs it as `tps`, so its folder must be on your `PATH`:

```bash
dotnet tool install --global Transpose.Compiler
export PATH="$PATH:$HOME/.dotnet/tools"   # add to your shell profile to persist
```

If a build fails with `tps: not found`, the tools folder is missing from the `PATH` of that shell.

The project file uses the Transpose SDK and references the Curiosity front-end packages:

```xml
<Project Sdk="Transpose.Build.Target/26.10.7421">
  ...
  <ItemGroup>
    <PackageReference Include="Transpose.BCL"             Version="26.11.7972" />
    <PackageReference Include="Transpose.Newtonsoft.Json" Version="26.10.7422" />
    <PackageReference Include="Tesserae"                  Version="2026.11.73650" />
    <PackageReference Include="Curiosity.FrontEnd.API"    Version="26.11.8092" />
    <PackageReference Include="Curiosity.FrontEnd.Core"   Version="26.11.8092" />
    <PackageReference Include="Curiosity.FrontEnd.Admin"  Version="26.11.8092" />
    <PackageReference Include="Curiosity.FrontEnd"        Version="26.11.8092" />
  </ItemGroup>
</Project>
```

Keep these on the versions your workspace's own front end uses: a front end built against older packages than the server may not load.

The compiler is configured by `tps.json` (with `tps.Debug.json` and `tps.Release.json` for the two configurations). A build writes the site (`index.html`, the scripts and the `assets/` folder) to `bin/<Configuration>/netstandard2.0/tps/`. A Release build emits one JavaScript module per chunk, loaded on demand; a Debug build emits one readable bundle. The stylesheets in `assets/css/` are copied to the site and added to `index.html`.

```bash
cd custom-front-end
dotnet build                   # Debug: bin/Debug/netstandard2.0/tps/
dotnet build -c Release        # Release: bin/Release/netstandard2.0/tps/
```

## Downloading a Front End Template

To generate a custom front-end, navigate to the `Interfaces` section of the management interface, open `Custom Interface` and click on the C# icon next to "Get Started". This will download a ZIP file containing a C# project pre-configured for your workspace, with the necessary dependencies. The template provides a structured starting point, allowing for further customization and integration with the workspace. By using this template, you can accelerate development and ensure seamless integration with your Curiosity Workspace.

The template contains two generated files, which the project in this folder also uses:

- `generated/Schema.cs`: the `N` and `E` classes, with the node types, fields and edge types of your graph (`N.Device.Type`, `N.Device.Name`, `E.HasPart`, ...).
- `generated/Endpoints.cs`: the `Endpoints` class with one constant per code endpoint path (`Endpoints.SupportChat.PostMessage`), and the `AI_Tools` and `AIAssistants` UIDs.

Never type a node type, field, edge or endpoint path as a string: use these classes. When you change the schema or the endpoints, download the template again and replace the two files.

## Project Layout

```
TechnicalSupport.FrontEnd.csproj    the Transpose SDK and the package references
tps.json, tps.Debug.json,           the Transpose compiler's configuration
tps.Release.json
assets/css/support-style.css        the application's stylesheet
generated/Schema.cs                 N.<Type>.<Field> and E.<Edge> (generated by the workspace)
generated/Endpoints.cs              Endpoints.<Path>, AI_Tools.<Name>, AIAssistants.<Name> (generated by the workspace)
src/App.cs                          Main(): routes, AppSettings and the sidebar entries
src/Api/SupportApi.cs               the endpoint DTOs and one method per code endpoint (the only caller of API.Endpoints)
src/Renderers/                      one NodeRendererBase per node type: Device, Part, Manufacturer, SupportCase
src/Views/                          SupportHomeView, DevicesView, PartsView, SupportCasesView
src/Chat/SupportChatView.cs         the support chat (ChatView + ChatViewConfiguration)
src/Components/                     AIDraftModal, SupportCaseWorkspace, ShownView
```

## Debugging a Front-End Locally

To test and debug a custom front-end before deployment, you can use the **serve** action in the Curiosity CLI. This allows you to locally host the front-end and interact with your workspace in a development environment.

First install the CLI:

```bash
dotnet tool update --global Curiosity.CLI
```

With `--watch`, the CLI compiles the project itself, serves what it produced and rebuilds it on every change. After a successful rebuild the page reloads itself, and a change to a stylesheet only is applied without a reload:

```sh
curiosity-cli serve --server <url-of-workspace> --watch <path-to-front-end-project> --port <port-number>
```

Replace `url-of-workspace` with the address of your server (for example, http://localhost:8080 if you're running a workspace locally), `<path-to-front-end-project>` with the folder holding the `.csproj` (for example `custom-front-end`) and `<port-number>` with the desired local port (default: `5000`). A compile error leaves the previous build in place and prints the errors. To serve an already compiled site instead, pass its folder with `-p <path-to-front-end-project>/bin/Debug/netstandard2.0/tps/`.

You also need to add your localhost url to your workspace CORS exception list by setting the following environmental variable (adjust the port number to reflect the value above):

```bash
MSK_CORS=http://localhost:<port-number>
```
You can set this value locally on your system, or just before starting the workspace. If your workspace is already running, you'll need to stop it first.

```bash
cd <path-to-workspace-installation-folder>
MSK_CORS=http://localhost:<port-number>
./curiosity   # or curiosity.exe on Windows
```

## Deploying a Front-End

The Curiosity CLI uploads the compiled site to your workspace. There is no need to zip the folder when using this method, and it can be useful for quickly releasing changes, or for building automation pipelines to publish a front-end project:

```bash
curiosity-cli upload-front-end -s http://localhost:8080/ -t <interface-upload-token> -p bin/Release/netstandard2.0/tps/
```

You can generate the required token (an `Interface Upload` token) under the `Tokens` settings page in the Management interface.

The project file runs this command for you after every Release build when the `CURIOSITY_URL` and `CURIOSITY_UPLOAD_TOKEN` environment variables are set (no token is stored in the repository). Without them, the build only compiles:

```bash
export CURIOSITY_URL=http://localhost:8080/
export CURIOSITY_UPLOAD_TOKEN=<interface-upload-token>
dotnet build -c Release
```

You can also deploy by hand: zip the contents of the `tps` output folder (the zip must contain `index.html` at its root, not a folder) and upload it under `Interfaces` > `Custom Interface` in the Management interface.

For example, you can run the following on your terminal:
```bash
> cd bin/Release/netstandard2.0/tps/ && zip -r ../front-end.zip .
```

Or on Windows PowerShell, with the script included in this folder (it writes `output.zip` into the folder you pass):
```powershell
PS > ./zip-output-folder.ps1 bin/Release/netstandard2.0/tps/
```

## Tesserae Components

Tesserae is an open-source, lightweight UI framework designed for building modern single-page applications using C# and Transpose. It provides a flexible and component-based architecture, allowing developers to create dynamic user interfaces without writing JavaScript. Tesserae integrates with Curiosity Workspaces, making it well-suited for developing custom front-ends.

The Tesserae framework includes a range of UI components, routing and observable patterns, and can be customized as needed. For a full list of components and sample code, check out [the Tesserae components list](http://curiosity.ai/tesserae). Tesserae also provides strongly typed icons from the [Interface Icons](https://github.com/freepik-company/flaticon-uicons) open-source project.


### IComponent interface
The core of the tesserae library is the IComponent interface:

```csharp
public interface IComponent
{
    HTMLElement Render();
}
```

This interface is used for UI components that can render HTML elements in a web application. It includes a single method, Render(), which returns an instance of HTMLElement. This method is responsible for generating the HTML structure that represents the component's user interface. By implementing the IComponent interface, developers can create custom UI components that seamlessly integrate into a web-based environment, ensuring that each component is capable of rendering itself as a standard HTML element when needed.

### Implementing a component

New components can be easily implemented using Tesserae on top of other existing components, following the pattern bellow:

```csharp
public class SampleComponent : IComponent
{
    private readonly IComponent _content;

    public SampleComponent()
    {
        _content = VStack().Children(Icon(UIcons.Box), TextBlock("This is a sample component".t()));
    }

    public HTMLElement Render() => _content.Render();
}
```

Tesserae provide many useful components to compose other components, such as Stacks, Grids, Labels, Text blocks, Icons and more.

Components can also be sized and aligned as needed using the built-in fluent-style interface:

```csharp
var tb = TextBlock("My Label".t())
   .BreakSpaces()          //Equivalent of white-space: break-spaces;
   .SemiBold()             //Semibold text style
   .TextCenter()           //Centers text
   .H(50)                  //Height = 50px
   .W(50.vw());            //Width = 50% of viewport width
```

Custom CSS classes can also be added to your components, to enable further customization of styles (put the rules in a stylesheet under `assets/css/`):

```csharp
tb.Class("custom-label");
```

```css
.custom-label {
    border:1px solid red;
}
```

### Tesserae and Asyncronous Rendering

Asyncronous code is nativelly supported on Tesserae, and can be used in conjunction with asyncronous HTTP requests to render components as data is returned from endpoints. Implementing an asyncronous component is usually done using a deferable component using the following pattern:

```csharp
public class SampleAsyncComponent : IComponent
{
    private readonly IComponent _content;

    public SampleAsyncComponent()
    {
        _content = Defer(async () =>
        {
            var response = await CallExternalApiAsync();
            return VStack().WS().Children(
                Icon(UIcons.Box),
                TextBlock($"This is a sample async component, the endpoint returned: {response}"));
        });
    }

    private async Task<string> CallExternalApiAsync()
    {
        await Task.Delay(5000); //Simulates a long running network call
        return "hello world";
    }

    public HTMLElement Render() => _content.Render();
}
```

For content that changes while it is shown, update the component that already exists instead of rebuilding it. Many components take an observable directly, for example a text block bound to a value:

```csharp
var count  = new SettableObservable<int>(0);
var btnAdd = Button("Add item".t()).OnClick(() => count.Value++);
var status = TextBlock(count, c => $"The list has {c:n0} items");
document.body.appendChild(HStack().Children(status, btnAdd).Render());
```

When only one part of a view has to change, keep a reference to its container and swap that part. The support case workspace does this for its list of similar cases, which is a different query when the "AI suggestions" toggle flips:

```csharp
aiEnabled.Observe(ai =>
{
    ...
    var next = CreateSimilarCasesList(ai);
    panel.Replace(next, list);
    list = next;
});
```

### Using Tesserae components

Every C# file in this project opens with the same usings block (copied from the top of `src/App.cs`). An unused using costs nothing, so copy the block whole into new files:

```csharp
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
```

- `Tesserae` and `static Tesserae.UI` give you the Tesserae components (`VStack()`, `TextBlock(...)`, `Button(...)`, ...).
- `static Transpose.Core.dom` gives you the browser types (`HTMLElement`, `document`, `window`).
- `TNT` and `static TNT.T` give you `.t()`: every string the user reads goes through it, so the interface can be translated.
- `UID` gives you `UID128`, the identifier of every node.
- `API = Mosaik.API` gives you `API.Query`, `API.Nodes`, `API.Endpoints`, ...
- `Node = Mosaik.Schema.Node` avoids a conflict between the `dom.Node` and `Schema.Node` classes.

`N`, `E`, `Endpoints`, `AI_Tools` and `AIAssistants` come from `generated/` and need no using.

## Curiosity Components

The [Curiosity Front End framework](https://www.nuget.org/packages/Curiosity.FrontEnd) is a built-in toolkit designed to streamline the development of full applications based on a Curiosity Workspace. It provides means for interacting with the workspace APIs, execute graph queries, and call custom endpoints. Additionally, it includes a set of pre-built components for key functionalities such as search, data exploration, graph and NLP visualization, chat and dashboards. The framework also integrates the management and admin interfaces, enabling easy configuration and monitoring.

The `Mosaik`, `Mosaik.Components`, `Mosaik.Helpers`, `Mosaik.Schema`, `Mosaik.Views` and `static Mosaik.UI` usings in the block above give you the Curiosity components.


### Node Renderers

A node renderer defines how nodes of one node type are shown everywhere in the application: as a row in search results and lists, in the preview sheet, and as a full page. Renderers are found automatically: any public, non-abstract class with a parameterless constructor that derives from `NodeRendererBase` is registered for the node type it names.

A renderer passes its style to the base constructor as a `SchemaStyleInfo` (node type, display name, label field, color and icon), and overrides `BuildTabs` to return the tabs of the node. `NodeRendererBase` builds the search row, the preview and the full page from these tabs. Here is the `DeviceRenderer` from this project ([`src/Renderers/DeviceRenderer.cs`](/custom-front-end/src/Renderers/DeviceRenderer.cs)):

```csharp
public sealed class DeviceRenderer : NodeRendererBase
{
    public DeviceRenderer() : base(new SchemaStyleInfo
    {
        Name        = N.Device.Type,
        DisplayName = "Device",
        LabelField  = N.Device.Name,
        Color       = "#346eeb",
        Icon        = UIcons.BoxOpenFull.ToString(),
    })
    {
    }

    protected override IEnumerable<NodeTab> BuildTabs(Node node, IReadOnlyList<RelationshipGroup> groups, NodeView nodeView)
    {
        yield return new NodeTab { Id = "product", Label = "Product Page".t(), TabTitle = SegmentTitle("Product Page".t()), TabContent = () => RenderProductPage(node) };
        yield return new NodeTab { Id = "support", Label = "Support".t(),      TabTitle = SegmentTitle("Support".t()),      TabContent = () => RenderSupportCases(node), Scrolls = false };
        yield return new NodeTab { Id = "graph",   Label = "Graph".t(),        TabTitle = SegmentTitle("Graph".t()),        TabContent = () => RenderGraphSegment(node, nodeView.OpenGraphNodePeek), Scrolls = false };
    }

    private static IComponent RenderProductPage(Node node)
    {
        return VStack().S().Children(
            Label("Name".t()).WS().Inline().AutoWidth().SetContent(TextBlock(node.GetString(N.Device.Name))),
            Label("Manufacturer".t()).WS().Inline().AutoWidth().SetContent(NeighborsLinks(node.UID, N.Manufacturer.Type, E.HasManufacturer).WS()),
            Label("Parts".t()),
            Neighbors(() => API.Query.StartAt(node.UID).Out(N.Part.Type, E.HasPart).TakeAll().GetUIDsAsync(),
                      new[] { N.Part.Type }, showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible).S());
    }

    private static IComponent RenderSupportCases(Node node)
    {
        return Neighbors(() => API.Query.StartAt(node.UID).Out(N.SupportCase.Type, E.HasSupportCase).TakeAll().GetUIDsAsync(),
                         new[] { N.SupportCase.Type }, showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible, defaultSortMode: SortModeEnum.RecentFirst).S();
    }
}
```

The device is shown with three tabs:
1. **Product Page**: Displays key details like the device's name, manufacturer, and associated parts.
2. **Support**: Lists related support cases using `Neighbors`, which queries and retrieves related nodes dynamically.
3. **Graph**: The graph around the device, drawn by the base class's `RenderGraphSegment`.

A renderer can also override what the base class builds. The `SupportCaseRenderer` ([`src/Renderers/SupportCaseRenderer.cs`](/custom-front-end/src/Renderers/SupportCaseRenderer.cs)) implements `INodeCustomStyle`, so the status of each case decides its icon and color, overrides `CompactView` to show the device in the footer of each row, and overrides `PreviewAsync` to open the case workspace (`SupportCaseWorkspace`) in a large modal:

```csharp
public UIcons GetIcon(Node node)         => IsClosed(node) ? UIcons.CommentAltCheck : UIcons.MessageQuestion;
public string GetColor(Node node)        => IsClosed(node) ? Theme.Success.Background : Theme.Danger.Background;

public override OmniResult<Node> CompactView(Node node)
{
    return NodeResult.For(this, node)
       .WithNeighborFooterEntries(async () => (await API.Query.StartAt(node.UID).Out(N.Device.Type, E.ForDevice).GetUIDsAsync()).UIDs);
}

public override async Task<OmniResult<Node>> PreviewAsync(Node node, Parameters state)
{
    return NodeResult.For(this, node).SetModalContent(new SupportCaseWorkspace(node)).ModalSize(90.vw(), 90.vh());
}
```

Because every list of support cases uses this renderer, the home page backlog and the Support Cases page look the same without customizing their rows.

### Routing and Sidebar

Routing in the Tesserae SPA framework allows for navigation between different views using the URL hash (The URL hash is a fragment identifier in a URL, which is a string starting with a # symbol followed by the fragment identifier. This part of the URL is often used to navigate to a specific section within a web page, and in single page applications to implement browser-based navigation).

Routes are registered using the `Router.Register` method, which maps a specific URL hash path to a corresponding action, before `App.Initialize` is called in `Main` ([`src/App.cs`](/custom-front-end/src/App.cs)). When a user navigates to a registered route, the associated function is executed to render the appropriate content. For example, the registration:

```csharp
Router.Register("hello-world", state => App.ShowDefault(TextBlock("Hello World !".t())));
```

defines a route for `hello-world`, which, when accessed, displays a simple "Hello World!" message using `TextBlock`. The `state` parameter contains any query parameters passed in the URL, allowing for dynamic content based on user input. This mechanism enables structured and declarative routing within Curiosity Workspaces.

The `App.ShowDefault` method is used to show the content within the normal view of the app (i.e. inside the content area next to the sidebar).

A route to one of the application's pages activates the view with `AppRouting.ActivateAsync<TView>(state)`, instead of constructing it with `new`. This loads the view's code only when it is first needed, and calls the view's constructor that takes the route `Parameters`:

```csharp
Router.Register(DevicesView.Route,      async state => App.ShowDefault(await AppRouting.ActivateAsync<DevicesView>(state),      title: "Devices".t()));
Router.Register(PartsView.Route,        async state => App.ShowDefault(await AppRouting.ActivateAsync<PartsView>(state),        title: "Parts".t()));
Router.Register(SupportCasesView.Route, async state => App.ShowDefault(await AppRouting.ActivateAsync<SupportCasesView>(state), title: "Support Cases".t()));
Router.Register(SupportChatView.Route,  async state => App.ShowDefault(await AppRouting.ActivateAsync<SupportChatView>(state),  title: "Support".t()));

App.Initialize(Configure, OnLoad).FireAndForget();
```

`Configure` receives the `AppSettings` of the application. This project replaces the home page with the support backlog, and switches off the parts of the interface a support team does not use:

```csharp
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
    ...
}
```

Favorites has no such switch, so it is hidden by a rule in `assets/css/support-style.css`.

The workspace sidebar can be customized using one of the event hooks of `AppSidebar` that allow customization of the sidebar at different points during its rendering process. These events (`OnSidebarRebuild_BeforeHeader`, `OnSidebarRebuild_AfterHeader`, `OnSidebarRebuild_BeforeFooter`, and `OnSidebarRebuild_AfterFooter`) enable developers to inject additional UI components into specific areas of the sidebar.

Each event passes three parameters:
- `sidebar`: The sidebar component to which elements can be added.
- `mode`: The current sidebar mode, such as `AppSidebar.Mode.Default` or `AppSidebar.Mode.AdminSettings`.
- `refresh`: A mechanism for dynamically updating UI elements based on state changes.

In the example below (from `Configure`), the `OnSidebarRebuild_BeforeFooter` event is used to append custom navigation buttons (`SidebarButton`) for "Support", "Devices", "Parts" and "Support Cases" in the default sidebar mode. Each button triggers navigation to a corresponding route when clicked, and the `refresh.Add` method is used to show the button as selected when its page is shown:

```csharp
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

    ...
};
```

`IsShowing()` asks `ShownView` ([`src/Components/ShownView.cs`](/custom-front-end/src/Components/ShownView.cs)) which page is on screen: each page registers itself when it is mounted and calls `AppSidebar.RefreshSelected()`, because an activated view appears after the sidebar has already refreshed for the navigation.

This approach enables flexible sidebar customization, allowing different elements to be displayed based on the sidebar mode and enhancing navigation within the application.

### Useful Curiosity Components


#### NodeResult

`NodeResult.For(renderer, node)` builds the `OmniResult<Node>` a renderer returns for a row (`CompactView`) or a preview (`PreviewAsync`): the node's icon, label and color, plus whatever you add to it, such as footer entries or the content of the preview modal (see `SupportCaseRenderer` above).

#### Neighbors

The neighbors component wrap around a normal search area component to provide a simple list view (with or without search box and filtering) based on a query:

```csharp
var neighbors = Neighbors(() => API.Query.StartAt(node.UID).Out(N.Part.Type, E.HasPart).TakeAll().GetUIDsAsync(),
                          new[] { N.Part.Type }, showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible);
```

The query can also be your own code. In the support case workspace ([`src/Components/SupportCaseWorkspace.cs`](/custom-front-end/src/Components/SupportCaseWorkspace.cs)), the similar cases are ranked by the `suggest-similar-cases` endpoint, shown in that order (`SortModeEnum.TargetQueryOrder`), and each row gets a "NN% similar" badge in its footer:

```csharp
return Neighbors(query, new[] { N.SupportCase.Type }, showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible,
                 defaultSortMode: ai ? SortModeEnum.TargetQueryOrder : SortModeEnum.RecentFirst,
                 renderer: r => r.CustomizeResult(result => AppendScoreIfAny(result)))
   .WS().H(10).Grow();
```

```csharp
private OmniResult<Node> AppendScoreIfAny(OmniResult<Node> result)
{
    var similar = _similar.Value.FirstOrDefault(s => s.UID == result.Result.UID);

    if (similar is object)
    {
        result.AddFooterEntry(Badge(string.Format("{0:0}% similar".t(), similar.Score * 100)).Tooltip("Similarity to this case".t()));
    }

    return result;
}
```

#### Search Area

The SearchArea component is one of the most commonly used components from the Curiosity UI framework, as they provide a native integration with all the search and filtering features of the workspace. It comprises of a search box, a filter bar and a search results list with infinite scrolling. It also automatically integrates with the default workspace search API, for a seamless search implementation. Each result is drawn by the renderer of its node type.

In the example below, from the `SupportHomeView.cs` file, we create a default search area, enable facets (i.e. filtering) and apply a pre-filter to the search request on every search (to only search on `SupportCase` node types):

```csharp
private static IComponent CreateSearch()
{
    return SearchArea()
       .WithFacets()
       .OnSearch(sr => sr.SetBeforeTypesFacet(N.SupportCase.Type))
       .S();
}
```

Customization and configuration of a search request usually happens by passing the `.OnSearch(...)` lambda, such as configuring target node types to search, passing a target query to restrict search on specific nodes, or even passing the results of a similarity request to sort results based on some pre-computed scores. `OnSearch` runs on every search the area performs, so the scope survives the user's first keystroke.

```csharp
SimilarCase[] similar = await SupportApi.SuggestSimilarCasesAsync(caseUID); // call endpoint to retrieve scores, best first
var orderedUIDs = similar.Select(s => s.UID).ToArray();
sa.OnSearch(s => s.SetBeforeTypesFacet(N.SupportCase.Type).WithTargetUIDs(orderedUIDs).WithSortMode(SortModeEnum.TargetQueryOrder));
```

#### Hub

`Hub(...)` renders a standard looking page with a title, optional commands, and one or more sections. Here is the `DevicesView` ([`src/Views/DevicesView.cs`](/custom-front-end/src/Views/DevicesView.cs)):

```csharp
public sealed class DevicesView : IComponent
{
    internal const string Route = "#/devices";

    private readonly IComponent _content;

    public DevicesView(Parameters state)
    {
        _content = ShownView.Track(this, Hub("Devices".t(), Route)
           .Section(CreateSearch(), grow: true)
           .S());
    }

    internal static bool IsShowing() => ShownView.Is<DevicesView>();

    // OnSearch runs on every search the area performs, so the scope survives the user's first keystroke.
    private static IComponent CreateSearch()
    {
        return SearchArea()
           .WithFacets()
           .OnSearch(sr => sr.SetBeforeTypesFacet(N.Device.Type))
           .S();
    }

    public HTMLElement Render() => _content.Render();
}
```

#### FileView

This component can be used to render a file preview inline:

```csharp
var file = FileView(new UID128("... file uid ...")).S();
```

#### DocumentFromField

This component can be used to render a parsed NLP document, tokens and entities captured inline:

```csharp
var node = await API.Nodes.GetAsync(new UID128("... support case uid ..."));
var doc = DocumentFromField(node, N.SupportCase.Content);
```

#### NeighborsLinks

This component can be used to render clickable buttons that will have the node icon, label and will open node previews when clicked. It is useful to provide clickable navigation elements for users to navigate to related nodes in the graph. For this dataset, it can be used for example to show the manufacturer of a given part, or related devices to a part:

```csharp
var manufacturer = NeighborsLinks(partNode.UID, N.Manufacturer.Type, E.HasManufacturer).WS();
var devices      = NeighborsLinks(() => API.Query.StartAt(partNode.UID).Out(N.Device.Type, E.PartOf).GetUIDsAsync()).WS();
```

#### IFrameFromHTML

Useful to render inline HTML as a sandboxed iframe wrapped inside a component. The can be useful to render HTML pages based on node contents exported from other applications.

```csharp
var html = IFrameFromHTML("<html><body><div>Hello World</div></body></html>", allowScripts: false);
var html2 = IFrameFromHTML(node.GetString("Html"), allowScripts: false);
```

#### GraphExplorerView

This component can be used to render a static or interactive graph view of a series of UIDs.

```csharp
return Defer(async () =>
{
    var queryResult = await API.Query.StartAt(node.UID).Out().TakeAll().GetUIDsAsync();
    return GraphExplorerView.ComponentFor(enableInteraction: true, uids: queryResult.UIDs.Append(node.UID).ToArray()).S();
}).S();
```

### Front-end Query methods

You can write direct graph query calls from the front-end, and execute them asyncronously or pass them as input to certain components. The query methods exposed on the front-end are limited to a subset of the query methods available on the back-end, and focus on simple query tasks such as finding neighbors of a node.

Queries can be constructed using the `API.Query` static class, from the starting point of a node unique identifier(s) or a list of one node type and one or more keys, such as:

```csharp
var q1 = API.Query.StartAt(new UID128("... uid ..."));
var q2 = API.Query.StartAt(new UID128("... uid1 ..."), new UID128("... uid2 ..."));
var q3 = API.Query.StartAt(N.Manufacturer.Type, "Apple");
var q4 = API.Query.StartAt(N.Manufacturer.Type, "Apple", "Google");
```

Available methods once you start the query can be used to traverse via certain node and edge types. Methods can also be chained in a fluent interface, such as:
```csharp
var q5 = API.Query.StartAt(N.Manufacturer.Type, "Apple").Out(N.Device.Type, E.ManufacturerOf);
var q6 = API.Query.StartAt(N.Manufacturer.Type, "Apple").Out(N.Device.Type, E.ManufacturerOf).Out(N.Part.Type, E.HasPart);
```

You can also merge two or more queries to efficiently retrieve all required nodes in a single operation (or to pass the query to one of the supported components or as part of a search request). You can also use .Skip(...) and .Take(...) to paginate through results:

```csharp
var q7 = q5.Union(q6).Skip(10).Take(10);
```

Finally, you can run the query by using the methods GetAsync() (to retrieve the nodes with contents) or GetUIDsAsync() to retrieve only unique identifiers.

```csharp
var nodes = await API.Query.StartAt(N.Manufacturer.Type, "Apple").Out(N.Device.Type, E.ManufacturerOf).GetAsync();
var uids  = await API.Query.StartAt(N.Manufacturer.Type, "Apple").Out(N.Device.Type, E.ManufacturerOf).GetUIDsAsync();
```

Note that by default, these methods will return up to 50 nodes or UIDs in the results set. If you want more than that, you need to manually add a .Take(...) with the desired number of results, or a .TakeAll() to retrieve all possible values. Use this with care when reading large amounts of data from the back-end, specially when reading full node contents, as your requests might timeout.

```csharp
var allNodes = await API.Query.StartAt(N.Manufacturer.Type, "Apple").Out(N.Device.Type, E.ManufacturerOf).TakeAll().GetAsync();
var allUids  = await API.Query.StartAt(N.Manufacturer.Type, "Apple").Out(N.Device.Type, E.ManufacturerOf).TakeAll().GetUIDsAsync();
```

Front-end queries can also be passed to some components and as part of a search request target query without having to materialize the results, such as:

```csharp
var neighbors = Neighbors(() => API.Query.StartAt(node.UID).Out(N.Part.Type, E.HasPart).TakeAll().GetUIDsAsync(), new[] { N.Part.Type }, showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible).S();

var request = new SearchRequest("iphone").WithTargetQuery(API.Query.StartAt(node.UID).Out(N.Part.Type, E.HasPart).TakeAll());
```

### Calling your endpoints

All calls to the code endpoints of this project go through [`src/Api/SupportApi.cs`](/custom-front-end/src/Api/SupportApi.cs): one method per endpoint, using the generated `Endpoints` constant for its path. The request and response classes in that file are declared a second time, identically, in the endpoint that reads or returns them, so keep the two in step:

```csharp
internal static Task<UID128> PostChatMessageAsync(SupportChatMessageRequest request)
    => API.Endpoints.CallAsync<UID128>(Endpoints.SupportChat.PostMessage, request);

internal static Task<SupportChatContext> GetChatContextAsync(UID128 chatUID)
    => API.Endpoints.CallAsync<SupportChatContext>(Endpoints.SupportChat.GetContext, chatUID, readOnlyCompatible: true);
```

See the [Custom Endpoints Guide](/custom-endpoints/INSTRUCTIONS.md) for the endpoints themselves.


## Custom Chat Interface

Out of the box, a Curiosity workspace comes with a standard Chat interface for interacting with LLMs, including support for File-based RAG using the built-in AI-search capabilities. This interface can be customized for a given use-cases similarly to how a search area can be customized.

The chat view is customized with a `ChatViewConfiguration`. The support chat of this project ([`src/Chat/SupportChatView.cs`](/custom-front-end/src/Chat/SupportChatView.cs)) customizes the interface header, the context area above the composer (a topic picker), message rendering, actions displayed next to a message, tool results, and how messages are posted:

```csharp
var configuration = new ChatViewConfiguration
{
    CustomHeader             = CreateChatHeader,
    CustomChatContext        = CreateChatContext,
    CustomMessageRenderer    = (chat, message, component) => component.Class("support-chat-message"),
    MessageCommands          = CreateMessageCommands,
    CustomToolResultRenderer = (chat, toolCall) => TextBlock(string.Format("Tool Call: {0}".t(), toolCall.ToolName)),

    // Both hooks go through the endpoint: PostMessage for a new message, ReplaceMessage for what
    // "Edit" and "Regenerate" send. Overriding only the first would run an edited turn without the topic.
    PostMessage    = request => PostSupportMessageAsync(request, replaceMessageUID: null),
    ReplaceMessage = request => PostSupportMessageAsync(request, replaceMessageUID: request.MessageUID),
};

_chatView = ShownView.Track(this, new ChatView(configuration, state));
```

`PostMessage` and `ReplaceMessage` replace the default calls the chat makes to post a message. Here both send the message through the `support-chat/post-message` endpoint, with the topic picked for the chat and everything the default call would send (the active tools, agents, skills and attachments):

```csharp
private async Task<UID128> PostSupportMessageAsync(ChatViewConfiguration.PostMessageRequest request, UID128 replaceMessageUID)
{
    var context = await GetContextAsync(request.ActiveChat.UID);

    // The endpoint replaces the default API.ChatAI.PostMessage / ReplaceMessage, so it receives everything those would.
    return await SupportApi.PostChatMessageAsync(new SupportChatMessageRequest
    {
        Message           = request.Message,
        ChatUID           = request.ActiveChat.UID,
        ReplaceMessageUID = replaceMessageUID,
        Tools             = request.ActiveTools,
        Agents            = request.ActiveAgents,
        Skills            = request.ActiveSkills,
        Attachments       = request.AttachmentUIDs,
        Context           = context,
    });
}
```

The assistant can also be used outside of the chat page. The "Draft Answer" and "Capture Knowledge" buttons of the support case workspace use `AIDraftModal` ([`src/Components/AIDraftModal.cs`](/custom-front-end/src/Components/AIDraftModal.cs)), which posts a prompt to a new chat with `API.ChatAI.PostMessage` and shows the answer as it streams in: it subscribes to the `CHAT_COMPLETION` websocket messages and reads the typed `ChatStreamPart` frames (`TextDelta`, `End`, `Error`, `Canceled`).

You can see more details of this custom chat interface by exploring the source code available in the [SupportChatView.cs](/custom-front-end/src/Chat/SupportChatView.cs) file and in the respective [endpoints](/custom-endpoints/code/endpoints/support-chat/) that implement the custom context handling and message posting feature.

## Conclusion

In this guide, we have explored the steps and components involved in building custom user interfaces for Curiosity Workspaces. By leveraging tools like the Transpose compiler, the Curiosity CLI, Tesserae UI framework, and Curiosity Components, you can create tailored, interactive applications that seamlessly integrate with your workspace. We've also discussed how to utilize node renderers, implement routing, customize the sidebar, and optimize search functionality, all of which contribute to a more dynamic and user-friendly experience. With the flexibility provided by Curiosity Workspaces, developers can craft solutions that meet their unique needs while ensuring security and performance. Whether you're building a simple dashboard or a complex interactive app, these features enable efficient and scalable development for a wide range of use cases.
