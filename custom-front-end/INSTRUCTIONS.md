# Curiosity Workspace // Custom Front Ends

## Table of Contents

1. [Introduction](#introduction)
1. [Installing h5](#installing-h5)
1. [The H5 Compiler](#the-h5-compiler)
1. [Downloading a Front End Template](#downloading-a-front-end-template)
1. [Debugging a Front-End Locally](#debugging-a-front-end-locally)
1. [Deploying a Front-End](#deploying-a-front-end)
1. [Tesserae Components](#tesserae-components)
1. [Curiosity Components](#curiosity-components)
1. [Node Renderers](#node-renderers)
1. [Routing and Sidebar](#routing-and-sidebar)
1. [Useful Curiosity Component](#useful-curiosity-components)
1. [Conclusion](#conclusion)

## Introduction

Curiosity Workspaces also allow you to create custom front-ends, enabling you to design tailored user interfaces that meet your specific needs. A custom front-end is a web-based interface that interacts with your workspace, providing a seamless and intuitive way for users to engage with your data, workflows, and AI models.

Custom front-ends are built as single-page applications, using Curiosity-provided components and libraries, and securely connect to your workspace's graph database, NLP models, and other functionalities. You can design them to be as simple or complex as needed, from lightweight search apps, simple dashboards to fully interactive applications. With authentication controls, you can restrict access to certain users or roles, ensuring a secure and personalized experience.

## Installing h5

h5 is a C# to JavaScript compiler that allows developers to write web applications using C# while targeting the JavaScript runtime. By translating C# code into JavaScript, h5 enables developers to leverage their existing C# skills for client-side development. It supports the majority of C# language features, up to version 7.2, including LINQ, async/await, and custom attributes, while enabling easy integration with existing Javascript libraries.

To get started, first install the [h5 compiler](https://www.nuget.org/packages/h5-compiler) as a dotnet global tool. 

```bash
dotnet tool update --global h5-compiler
```

## Downloading a Front End Template

To generate a custom front-end, navigate to the Interface section of the management interface and select Download template. This will download a ZIP file containing a C# project pre-configured with views for all schemas in the database and the necessary dependencies. The template provides a structured starting point, allowing for further customization and integration with the workspace. By using this template, you can accelerate development and ensure seamless integration with your Curiosity Workspace.

## Debugging a Front-End Locally

To test and debug a custom front-end before deployment, you can use the **serve** action in the Curiosity CLI. This allows you to locally host the front-end and interact with your workspace in a development environment.  

First install the CLI:

```bash
dotnet tool update --global Curiosity.CLI
```

After downloading and extracting the front-end template, navigate to its directory and run the following command:  

```sh
curiosity-cli serve -s <url-of-workspace> -p <path-to-front-end-project>/bin/Debug/netstandard2.0/h5 -port <port-number>
```  

Replace `url-of-workspace` with the address of your server (for example, http://localhost:8080 if you're running a workspace locally), `<path-to-front-end>` with the location of your project and `<port-number>` with the desired local port (default: `5000`). This command will launch a local server for the front-end, enabling real-time testing and debugging. Once you've this running, simply compile the front-end project and test any changes on your browser of choice.

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

After you compile a front-end project using h5, you can deploy that front-end to your workspace by simply zipping the contents of the h5 folder in your project output folder.

For example, you can run the following on your terminal:
```bash
> zip -r front-end.zip <path-to-front-end-project>/h5/*
```

Or on Windows PowerShell: 
```powershell
PS > Compress-Archive -Path $OutDir\h5\  -DestinationPath front-end.zip
```

The Curiosity CLI also provides a way to automatically upload the project output folder to your workspace. This can be useful for quickly releasing changes, or for building automation pipelines to publish a front-end project.

```bash
curiosity-cli upload-front-end -s http://localhost:8080/ -t $(CURIOSITY_INTERFACE_TOKEN) $ -p <path-to-front-end-project>/bin/Debug/netstandard2.0/h5/
```

You can generate the required front-end token under the `Tokens` settings page in the Management interface.

## Tesserae Components

Tesserae is an open-source, lightweight UI framework designed for building modern single-page applications using C# and h5. It provides a flexible and component-based architecture, allowing developers to create dynamic user interfaces without writing JavaScript. Tesserae integrates with Curiosity Workspaces, making it well-suited for developing custom front-ends. 

The Tesserae framework includes a range of UI components, routing and observable patterns, and can be customized as needed. For a full list of components and sample code, check out [the Tesserae components list](http://curiosity.ai/tesserae). Tesserae also provides strongly typed icons from the [Interface Icons](https://github.com/freepik-company/flaticon-uicons) open-source project.


### IComponent interface
The core of the tesserae library is the IComponent interface:

```csharp
public interface IComponent
{
    dom.HTMLElement Render();
}
```

This interface is used for UI components that can render HTML elements in a web application. It includes a single method, Render(), which returns an instance of dom.HTMLElement. This method is responsible for generating the HTML structure that represents the component's user interface. By implementing the IComponent interface, developers can create custom UI components that seamlessly integrate into a web-based environment, ensuring that each component is capable of rendering itself as a standard HTML element when needed.

### Implementing a component

New components can be easily implemented using Tesserae on top of other existing components, following the pattern bellow:

```csharp
public class SampleComponent : IComponent
{
    private readonly IComponent _content;

    public SampleComponent()
    {
        _content = VStack().Children(Icon(UIcons.Box), TextBlock("This is a sample component"));
    }

    public dom.HTMLElement Render() => _content.Render();
}
```

Tesserae provide many useful components to compose other components, such as Stacks, Grids, Labels, Text blocks, Icons and more.

Components can also be sized and aligned as needed using the built-in fluent-style interface:

```csharp
var tb = TextBlock("My Label")
                .BreakSpaces()          //Equivalent of white-space: break-spaces;
                .SemiBold()             //Semibold text style
                .TextCenter()           //Centers text
                .H(50)                  //Height = 50px
                .W(50.vw());            //Width = 50% of viewport width
```

Custom CSS classes can also be added to your components, to enable further customization of styles:

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
        _content = Defer(async () => {
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

    public dom.HTMLElement Render() => _content.Render();
}   
```

Deferable components can also be used in conjunction with observables and observable collections, to implement reactive-style rendering:

```csharp
var obsList = new ObservableList<int>();
var btnAdd  = Button("Add item").OnClick(obsList.Add(obsList.Count + 1));
var status  = DeferSync(obsList, l => TextBlock($"The list has {l.Count:n0} items"));
document.body.appendChild(HStack().Children(status, btnAdd).Render());
```

### Using Tesserae components

In order to use Tesserae components on your front-end, you need to make sure to have the following includings:

```csharp
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;
```

We also recommend the following h5 statements to be added whenever you need to interact with lower level javascript methods:

```csharp
using H5;
using static H5.Core.dom;
using Node = Mosaik.Schema.Node; //To avoid a conflict between the dom.Node and Schema.Node classes
```

## Curiosity Components

The [Curiosity Front End framework](https://www.nuget.org/packages/mosaik.frontend) is a built-in toolkit designed to streamline the development of full applications based on a Curiosity Workspace. It provides means for interacting with the workspace APIs, execute graph queries, and call custom endpoints. Additionally, it includes a set of pre-built components for key functionalities such as search, data exploration, graph and NLP visualization, and dashboards. The framework also integrates the management and admin interfaces, enabling easy configuration and monitoring.

In order to use curiosity components on your front-end, make sure the followin imports are used (in adition to the Tesserae imports above):

```csharp
using Mosaik;
using Mosaik.Components;
using Mosaik.Schema;
using Mosaik.Views;
using static Mosaik.UI;
```


### Node Renderers

The `INodeRenderer` interface is a key interface used to build applications. It is used to register an auto-discoverable custom view for specific node schemas within the Curiosity Workspace. It extends the `INodeStyle` interface, which defines metadata such as the node type, display name, label field, color, and icon. Implementing `INodeRenderer` allows developers to define how nodes are visually represented in different contexts. The `CompactView` method generates a summarized card-style representation of a node, while `PreviewAsync` provides previews with additional details. The `ViewAsync` method is used to render the full node view, utilizing any query parameters passed via the URL. 

```csharp
public interface INodeRenderer : INodeStyle
{
    CardContent CompactView(Node node);

    Task<CardContent> PreviewAsync(Node node, Parameters parameters);

    Task<IComponent> ViewAsync(Node node, Parameters parameters);
}

public interface INodeStyle
{
    string NodeType { get; }

    string DisplayName { get; }

    string LabelField { get; }

    string Color { get; }

    UIcons Icon { get; }
}
```

A typical INodeRenderer implementation can be seen in the provided project example for this dataset:

```csharp
public class DeviceRenderer : INodeRenderer
{
    public string NodeType    => N.Device.Type;
    public string DisplayName => "Device";
    public string LabelField  => "Name";
    public string Color       => "#346eeb";
    public UIcons Icon        => UIcons.BoxOpenFull;

    public CardContent CompactView(Node node)
    {
        return CardContent(Header(this, node), null);
    }

    public async Task<CardContent> PreviewAsync(Node node, Parameters state)
    {
        return CardContent(Header(this, node), CreateView(node, state));
    }

    public async Task<IComponent> ViewAsync(Node node, Parameters state)
    {
        return (await PreviewAsync(node, state)).Merge();
    }

    private IComponent CreateView(Node node, Parameters state)
    {
        return Pivot().S().Pivot("product", PivotTitle("Product Page"),    () => RenderDevicePage(node))
                            .Pivot("support", PivotTitle("Support"),         () => RenderSupport(node));
    }

    private IComponent RenderDevicePage(Node node)
    {
        return VStack().S().Children(
                    Label("Name").WS().Inline().AutoWidth().SetContent(TextBlock(node.GetString(N.Device.Name))),
                    Label("Manufacturer").WS().Inline().AutoWidth().SetContent(NeighborsLinks(node.UID, N.Manufacturer.Type, E.HasManufacturer).WS()),
                    Label("Parts"),
                    Neighbors(() => Mosaik.API.Query.StartAt(node.UID).Out(N.Part.Type, E.HasPart).TakeAll().GetUIDsAsync(), new[] { N.Part.Type }, showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible).S());
    }

    private IComponent RenderSupport(Node node)
    {
        return Neighbors(() => Mosaik.API.Query.StartAt(node.UID).Out(N.SupportCase.Type).TakeAll().GetUIDsAsync(),
                            new[] { N.SupportCase.Type}, true, FacetDisplayOptions.Visible, defaultSortMode: SortModeEnum.RecentFirst);
    }
}
```

The `DeviceRenderer` class above is an implementation of the `INodeRenderer` interface, defining how nodes of type `Device` are displayed in different views within a Curiosity Workspace.  

This renderer specifies metadata such as the node type (`Device`), display name (`"Device"`), field used for rendering labels (`"Name"`), a color (`#346eeb`), and an icon (`UIcons.BoxOpenFull`).  

- **CompactView**: Generates a minimal card representation of the device using the `Header` component.  
- **PreviewAsync**: Provides a more detailed preview, incorporating the `CreateView` method to display additional information.  
- **ViewAsync**: Returns the full node view by merging the preview content into a full view.  

The `CreateView` method organizes device-related information into a **tabbed interface** (`Pivot`), with two sections:  
1. **Product Page** – Displays key details like the device’s name, manufacturer, and associated parts.  
2. **Support** – Lists related support cases using `Neighbors`, which queries and retrieves related nodes dynamically.  

### Routing and Sidebar

Routing in the Tesserae SPA framework allows for navigation between different views using the URL hash (The URL hash is a fragment identifier in a URL, which is a string starting with a # symbol followed by the fragment identifier. This part of the URL is often used to navigate to a specific section within a web page, and in single page applications to implement browser-based navigation).

Routes are registered using the `Router.Register` method, which maps a specific URL hash path to a corresponding action. When a user navigates to a registered route, the associated function is executed to render the appropriate content. For example, the registration:  

```csharp
Router.Register("hello-world", state => App.ShowDefault(TextBlock("Hello World !")));
```  

defines a route for `hello-world`, which, when accessed, displays a simple "Hello World!" message using `TextBlock`. The `state` parameter contains any query parameters passed in the URL, allowing for dynamic content based on user input. This mechanism enables structured and declarative routing within Curiosity Workspaces.

The `App.ShowDefault` method is used to show the content within the normal view of the app (i.e. inside the content area next to the sidebar).

The workspace sidebar can be customized using one of the event hooks that allow customization of the sidebar at different points during its rendering process. These events — `OnSidebarRebuild_BeforeHeader`, `OnSidebarRebuild_AfterHeader`, `OnSidebarRebuild_BeforeFooter`, and `OnSidebarRebuild_AfterFooter` — enable developers to inject additional UI components into specific areas of the sidebar.  

Each event passes three parameters: 
- `sidebar` – The sidebar component to which elements can be added.  
- `mode` – The current sidebar mode, such as `Default`, `UserPreferences`, or `AdminSettings`.  
- `tracker` – A mechanism for dynamically updating UI elements based on state changes.  

In the example below, the `OnSidebarRebuild_BeforeFooter` event is used to append custom navigation buttons (`SidebarButton`) for "Devices," "Parts" and "Support Cases" in the default sidebar mode. Each button triggers navigation to a corresponding route when clicked, and the `tracker.Add` method is used to show the button as selected when its associated route is active. 

```csharp
App.Sidebar.OnSidebarRebuild_BeforeFooter += (sidebar, mode, tracker) =>
{
    switch(mode)
    {
        case App.Sidebar.Mode.Default:
        {
            var kbDevices = new SidebarButton("devices", UIcons.Boxes, "Devices")
                                 .OnClick(() => Router.Navigate("#/devices"));
            tracker.Add(() => kbDevices.IsSelected = window.location.hash.Contains("#/devices"));
            sidebar.AddContent(kbDevices);

            var kbParts = new SidebarButton("parts", UIcons.Tools, "Parts")
                                 .OnClick(() => Router.Navigate("#/parts"));
            tracker.Add(() => kbParts.IsSelected = window.location.hash.Contains("#/parts"));
            sidebar.AddContent(kbParts);

            var kbCases= new SidebarButton("support-cases", UIcons.CommentsQuestion, "Support Cases")
                                 .OnClick(() => Router.Navigate("#/support-cases"));
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
```

This approach enables flexible sidebar customization, allowing different elements to be displayed based on the sidebar mode and enhancing navigation within the application.

### Useful Curiosity Components


#### CardContent, Header

#### Neighbors

The neighbors component wrap around a normal search area component to provide a simple list view (with or without search box and filtering) based on a query or node type and edge type to traverse:

```csharp
var neighbors = Neighbors(node.UID, N.Part.Type, E.HasPart, showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible);

//or alternativelly using the Query approach:
var neighbors = Neighbors(() => Mosaik.API.Query.StartAt(node.UID).Out(N.Part.Type, E.HasPart).TakeAll().GetUIDsAsync(), 
                          new[] { N.Part.Type }, showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible);
```

#### Search Area

The SearchArea component is one of the most commonly used components from the Curiosity UI framework, as they provide a native integration with all the search and filtering features of the workspace. It comprises of a search box, a filter bar and a search results list with infinite scrolling. It also automatically integrates with the default workspace search API, for a seamless search implementation. 

In the example below, from the `SupportHomeView.cs` file, we create a default search area, apply a pre-filter to the search request on every search (to only search on `SupportCase` node types), enable facets (i.e. filtering) and apply a custom renderer to the search result.

```csharp
private IComponent CreateSearch(Parameters state)
{
    var sa = SearchArea()
                .OnSearch(s => s.SetBeforeTypesFacet(N.SupportCase.Type))
                .WithFacets()
                .Renderer(r => r.WithCustomizedRenderer((sh, rr) =>
                {
                    return RenderSupportCase(sh, rr);
                }));

    return sa.S();
}
```

Customization and configuration of a search request usually happens by passing the `.OnSearch(...)` lambda, such as configuring target node types to search, passing a target query to restrict search on specific nodes, or even passing the results of a similarity request to sort results based on some pre-computed scores.

```csharp
Dictionary<UID128, float> similarityScores = ...; // call endpoint to retrieve scores
var orderedUIDs = similarityScores.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).ToArray();
sa.OnSearch(s => s.SetBeforeTypesFacet(N.SupportCase.Type).WithTargetUIDs(orderedUIDs).WithSortMode(SortModeEnum.TargetQueryOrder));
```

#### HubStack, HubTitle

These components are used to render standard looking pages with a title, optional commands, and a content area:

```csharp
class DevicesView : IComponent
{
    private IComponent _container;

    public DevicesView(Parameters state)
    {
        _container = HubStack(HubTitle("Devices", "#/devices"), "#/home")
                        .Section(CreateView(), grow: true);
    }

    private IComponent CreateView()
    {
        return SearchArea().WithFacets().OnSearch(s => s.SetBeforeTypesFacet(N.Device.Type)).S();
    }

    public dom.HTMLElement Render() => _container.Render();
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
var node = await Mosaik.API.Nodes.GetAsync(new UID128("... support case uid ..."));
var doc = DocumentFromField(node, N.SupportCase.Content);
```

#### NeighborsLinks

This component can be used to render clickable buttons that will have the node icon, label and will open node previews when clicked. It is useful to provide clickable navigation elements for users to navigate to related nodes in the graph. For this dataset, it can be used for example to show the manufacturer of a given part, or related devices to a part:

```csharp
var manufacturer = NeighborsLinks(partNode.UID, N.Manufacturer.Type, E.HasManufacturer).WS();
var devices      = NeighborsLinks(() => Mosaik.API.Query.StartAt(partNode.UID).Out(N.Device.Type, E.PartOf).WS();
```

#### IFrameFromHTML, IFrameWithSearchbarFromHTML, IFrameFromURL, IFrameWithSearchbarFromURL

Useful to render inline HTML or external websites as sandboxed iframes wrapped inside a component, without or with a text search box. Iframes will automatically get injected the appropriate handlers for command navigation events. The can be useful to render HTML pages based on node contents exported from other applications.

```csharp
var html = IFrameFromHTML("<html><body><div>Hello World</div></body></html>", allowScripts: false);
var html2 = IFrameFromHTML(node.GetString("Html"), allowScripts: false);
```

#### GraphExplorerView

This component can be used to render a static or interactive graph view of a series of UIDs.

```csharp
return Defer(async () =>
{
    var queryResult = await Mosaik.API.Query.StartAt(node.UID).Out().TakeAll().GetUIDsAsync();
    return GraphExplorerView.ComponentFor(enableInteraction: true, uids: queryResult.UIDs.Append(node.UID).ToArray()).S();
}).S();
```

#### Plotly

Curiosity provides a strongly typed wrapper to the [Plotly JavaScript library](https://plotly.com/javascript/) , that can be used to render charts for dashboards. For this, you can use the plotly component to wrap the Plot traces, such as:

```csharp
var xvalues = new float[]{1,2,3,4};
var yvalues = new float[]{5,2,4,2};
var plot = Plotly(Plot.traces(
                Traces.bar(
                    Bar.Orientation.v(),
                    Bar.y(yvalues),
                    Bar.x(xvalues),
                    Bar.marker(Marker.color(Color.EvalVar(Theme.Primary.Background)))
                )),
            Plot.layout(Layout.autosize(true),
                Layout.margin(Margin.pad(0), Margin.t(5), Margin.b(5), Margin.l(5), Margin.r(5)),
                Layout.height(150),
                Layout.yaxis(Yaxis.automargin(true),
                    Yaxis.Autorange._true()),
                Layout.showlegend(false),
                Layout.xaxis(Xaxis.automargin(true),
                    Xaxis.Autorange._true()),
                PlotlyConfig.Background(),
                PlotlyConfig.Font(),
                PlotlyConfig.PaperBackground()),
            PlotlyConfig.Default2D())).WS(),
```

### Front-end Query methods

You can write direct graph query calls from the front-end, and execute them asyncronously or pass them as input to certain components. The query methods exposed on the front-end are limited to a subset of the query methods available on the back-end, and focus on simple query tasks such as finding neighbors of a node.

Queries can be constructed using the Mosaik.API.Query static class, from the starting point of a node unique identifier(s) or a list of one node type and one or more keys, such as:

```csharp
var q1 = Mosaik.API.Query.StartAt(new UID128("... uid ..."));
var q2 = Mosaik.API.Query.StartAt(new UID128("... uid1 ..."), new UID128("... uid2 ..."));
var q3 = Mosaik.API.Query.StartAt(N.Manufacturer.Type, "Apple");
var q4 = Mosaik.API.Query.StartAt(N.Manufacturer.Type, "Apple", "Google");
```

Available methods once you start the query can be used to traverse via certain node and edge types. Methods can also be chained in a fluent interface, such as:
```csharp
var q5 = Mosaik.API.Query.StartAt(N.Manufacturer.Type, "Apple").Out(N.Device.Type, E.ManufacturerOf);
var q6 = Mosaik.API.Query.StartAt(N.Manufacturer.Type, "Apple").Out(N.Device.Type, E.ManufacturerOf).Out(N.Part.Type, E.HasPart);
```

You can also merge two or more queries to efficiently retrieve all required nodes in a single operation (or to pass the query to one of the supported components or as part of a search request). You can also use .Skip(...) and .Take(...) to paginate through results:

```csharp
var q7 = q5.Union(q6).Skip(10).Take(10);
```

Finally, you can run the query by using the methods GetAsync() (to retrieve the nodes with contents) or GetUIDsAsync() to retrieve only unique identifiers.

```csharp
var nodes = await Mosaik.API.Query.StartAt(N.Manufacturer.Type, "Apple").Out(N.Device.Type, E.ManufacturerOf).GetAsync();
var uids  = await Mosaik.API.Query.StartAt(N.Manufacturer.Type, "Apple").Out(N.Device.Type, E.ManufacturerOf).GetUIDsAsync();
```

Note that by default, these methods will return up to 50 nodes or UIDs in the results set. If you want more than that, you need to manually add a .Take(...) with the desired number of results, or a .TakeAll() to retrieve all possible values. Use this with care when reading large amounts of data from the back-end, specially when reading full node contents, as your requests might timeout.

```csharp
var allNodes = await Mosaik.API.Query.StartAt(N.Manufacturer.Type, "Apple").Out(N.Device.Type, E.ManufacturerOf).TakeAll().GetAsync();
var allUids  = await Mosaik.API.Query.StartAt(N.Manufacturer.Type, "Apple").Out(N.Device.Type, E.ManufacturerOf).TakeAll().GetUIDsAsync();
```

Front-end queries can also be passed to some components and as part of a search request target query without having to materialize the results, such as:

```csharp
var neighbors = Neighbors(() => Mosaik.API.Query.StartAt(node.UID).Out(N.Part.Type, E.HasPart).TakeAll().GetUIDsAsync(), new[] { N.Part.Type }, showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible).S());

var request = new SearchRequest("iphone").WithTargetQuery(Mosaik.API.Query.StartAt(node.UID).Out(N.Part.Type, E.HasPart).TakeAll());
```

## Conclusion

In this guide, we have explored the steps and components involved in building custom user interfaces for Curiosity Workspaces. By leveraging tools like the Curiosity CLI, Tesserae UI framework, and Curiosity Components, you can create tailored, interactive applications that seamlessly integrate with your workspace. We've also discussed how to utilize node renderers, implement routing, customize the sidebar, and optimize search functionality, all of which contribute to a more dynamic and user-friendly experience. With the flexibility provided by Curiosity Workspaces, developers can craft solutions that meet their unique needs while ensuring security and performance. Whether you're building a simple dashboard or a complex interactive app, these features enable efficient and scalable development for a wide range of use cases.