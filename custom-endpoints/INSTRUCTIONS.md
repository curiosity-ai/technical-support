# Curiosity Workspace // Endpoints

## Table of Contents

1. [Introduction](#introduction)
1. [Creating an Endpoint](#creating-an-endpoint)
1. [How to call an endpoint](#how-to-call-an-endpoint)
1. [Calling an Endpoint using curl](#calling-an-endpoint-using-curl)
1. [Calling an Endpoint from a Data Connector](#calling-an-endpoint-from-a-data-connector)
1. [Calling an Endpoint from the Front-End](#calling-an-endpoint-from-the-front-end)
1. [Calling an Endpoint from another Endpoint](#calling-an-endpoint-from-another-endpoint)
1. [Exporting and Importing Endpoints](#exporting-and-importing-endpoints)
1. [Global endpoints scope](#global-endpoints-scope)
1. [Conclusion](#conclusion)

## Introduction

One of the features of Curiosity Workspaces is the ability to write custom endpoints that can extend the functionality and write custom business logic for your specific needs. A custom endpoint is a hosted function inside your workspace that handles incoming requests for a specific URL, does some processing and returns a response. 

Endpoints have direct access to the underlying graph database, NLP models and other functionality. Custom Endpoints are written in C# from within the workspace, and can be restricted to logged users or admin users of your workspace. You can also call endpoints externally by creating and Endpoint Token.

## Creating an Endpoint

To get started, navigate to the Management interface and select `Endpoints`. Click on the `+` icon next to the search box to create a new endpoint. 

You can write your endpoint logic using the Code area. The response of your endpoint will be given by the value you return from your code (for example `return "hello world";`). You can also return a status helper such as `Ok(...)`, `BadRequest(...)` or `Forbid()`.

There are a few options for you to configure:

- **Endpoint Path**: This defines the path of your final endpoint URL. You can use slashes ('/') to create hierarchy of endpoints (this can be useful for accessing endpoints directly using Endpoint Tokens, which can be scoped to a given endpoint name or path)

- **Cache Duration**: Specifies if the application should cache the result of the computation, and for how long. Set to zero to always run the endpoint code on every call. Caching can be useful for endpoints that perform expensive computations and might be called often by your end-users or external APIs - an example could be an endpoint providing daily aggregates or analytics that have to iterate through a large amount of data.

- **Authorization**: When being called from within the front-end, endpoints can be restricted to all logged users, to members of access groups, or to only admin users. Unrestricted endpoints can be called without any token.

- **Mode**: Select between Pooling Endpoints (run in the background and can execute long-running tasks without timing out) and Sync endpoints (useful for short requests, easier to call from outside applications not using Curiosity-based code).

- **Read Only**: Run the endpoint in read-only mode. Useful for implemeting endpoints that shoudn't change data, and for endpoints that can also run on read-only replicas. In a read-only endpoint, `Graph` cannot be written to, which is checked when the code is compiled.

### The endpoints of this guide

All endpoints used in this repository are in the [`code/endpoints`](/custom-endpoints/code/endpoints/) folder, one file per endpoint, named after its path (`support-chat/post-message` lives in `code/endpoints/support-chat/post-message.cs`). This is the layout of the workspace definitions, so you can import them all at once (see [Exporting and Importing Endpoints](#exporting-and-importing-endpoints)).

Each file starts with the endpoint settings as `[endpoint: Curiosity.Endpoints.*]` attributes, followed by a blank line and the code. The options above map to these attributes:

```csharp
[endpoint: Curiosity.Endpoints.Path("my/route")]           // Endpoint Path (the only mandatory one)
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]     // Authorization: Unrestricted, AllUsers, AccessControlled or AdminOnly
[endpoint: Curiosity.Endpoints.CacheFor(60)]               // Cache Duration
[endpoint: Curiosity.Endpoints.SyncWithRequest]            // Mode: run in sync (without it, the endpoint is a pooling endpoint)
[endpoint: Curiosity.Endpoints.EnableMonitoring]
[endpoint: Curiosity.Endpoints.ReadOnly]                   // Read Only
```

For this introduction, let's look at the following endpoints. None of them sets `SyncWithRequest`, so they all run in pooling mode: a call that finishes within a few seconds answers directly, a longer one answers `202 Accepted` (see below).

- **Endpoint 1**: [`hello-world.cs`](/custom-endpoints/code/endpoints/hello-world.cs)

  ```csharp
  [endpoint: Curiosity.Endpoints.Path("hello-world")]
  [endpoint: Curiosity.Endpoints.AccessMode("Unrestricted")]
  [endpoint: Curiosity.Endpoints.ReadOnly]

  return $"Hello World from Curiosity - today is {DateTimeOffset.UtcNow:u}";
  ```

- **Endpoint 2**: [`long-running-hello-world.cs`](/custom-endpoints/code/endpoints/long-running-hello-world.cs)

  ```csharp
  [endpoint: Curiosity.Endpoints.Path("long-running-hello-world")]
  [endpoint: Curiosity.Endpoints.AccessMode("Unrestricted")]
  [endpoint: Curiosity.Endpoints.ReadOnly]

  var sw = ValueStopwatch.StartNew();

  await Task.Delay(Random.Shared.Next(15_000), CancellationToken);

  return $"Hello World from Curiosity - this endpoint took {sw.GetElapsedTime().TotalMilliseconds:n0} milliseconds to run";
  ```

- **Endpoint 3**: [`replay.cs`](/custom-endpoints/code/endpoints/replay.cs). `Body` is the request stream; `ReadBodyAsText()` reads it as text.

  ```csharp
  [endpoint: Curiosity.Endpoints.Path("replay")]
  [endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]
  [endpoint: Curiosity.Endpoints.ReadOnly]

  // Body is the request stream: read it as text to answer it back unchanged.
  return ReadBodyAsText();
  ```

- **Endpoint 4**: [`sum-values.cs`](/custom-endpoints/code/endpoints/sum-values.cs). `ParseBody<T>()` deserializes a JSON body into a class declared at the end of the file.

  ```csharp
  [endpoint: Curiosity.Endpoints.Path("sum-values")]
  [endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]
  [endpoint: Curiosity.Endpoints.ReadOnly]

  var request = ParseBody<SumRequest>();

  if (request is null) return BadRequest("Expected a JSON body with A and B");

  return Ok(new SumResponse { Value = request.A + request.B });

  public class SumRequest
  {
      public int A { get; set; }
      public int B { get; set; }
  }

  public class SumResponse
  {
      public int Value { get; set; }
  }
  ```

- **Endpoint 5**: [`long-running-endpoint-with-status.cs`](/custom-endpoints/code/endpoints/long-running-endpoint-with-status.cs). `RelayStatusAsync` sends a progress message to the caller while the endpoint runs.

  ```csharp
  [endpoint: Curiosity.Endpoints.Path("long-running-endpoint-with-status")]
  [endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]
  [endpoint: Curiosity.Endpoints.ReadOnly]

  for (int i = 0; i < 100; i++)
  {
      await RelayStatusAsync($"Computing, at {i}%");
      await Task.Delay(100, CancellationToken); //Simulates a long running task
  }

  return "Done, that took a while!";
  ```

## How to call an endpoint

In general, in order to call an endpoint, you need to send an HTTP request to the full endpoint URL, which will depend on where your workspace is hosted. The format for an endpoint url is: 

- For endpoints that need authorization to run: `{workspace-url}/api/endpoints/token/run/{endpoint-path}`

- For endpoints that do not need authorization: `{workspace-url}/api/endpoints/external/{endpoint-path}`

For endpoint that require authorization, only `POST` requests are valid. They must include a valid user or endpoint Bearer token in the `Authorization` header.

For unrestricted endpoints, both `GET` and `POST` calls are valid. If you need to pass a value in the body of the request, then only `POST` requests can be used.

For endpoints that run in sync with the call, the response of the endpoint will always be the return value from executing the code, or an error code if it failed. For endpoints that run in pooling mode, if the execution of the endpoint takes more than a few seconds, you will receive a `202 Accepted` status code. You can then either retry the exact same request (with exact the same request body), or use the value returned under the header `X-MSK-ENDPOINT-KEY` to try again the request without passing the same request body. Until you receive a valid final answer, retrying the call will not trigger again the endpoint. You might need to repeat the call as long as you're receiving a `202 Accepted` status code. If a retry answers `410 Gone` with `X-MSK-ENDPOINT-KEY: unknown`, the server no longer knows the call (for example after a restart): send the request again with its body.

For example, to call a pooling endpoint in Python, you might use the following logic:

```python
import requests
import time

def call_endpoint(url, payload, max_retries=100, retry_delay=5):
    headers = {'Content-Type': 'application/json'}
    attempt = 0
    msk_endpoint_key = None

    while attempt < max_retries:
        if msk_endpoint_key:
            response = requests.post(url, headers={**headers, 'X-MSK-ENDPOINT-KEY': msk_endpoint_key})
        else:
            response = requests.post(url, json=payload, headers=headers)

        if response.status_code == 202:
            print("Processing... retrying in", retry_delay, "seconds")
            msk_endpoint_key = response.headers.get('X-MSK-ENDPOINT-KEY')
            time.sleep(retry_delay)
            attempt += 1
        elif response.status_code == 410 and response.headers.get('X-MSK-ENDPOINT-KEY') == 'unknown':
            msk_endpoint_key = None  # the server lost the call: send it again with its body
            attempt += 1
        elif response.status_code == 200:
            return response.text
        else:
            response.raise_for_status()

    raise TimeoutError("Endpoint did not return a final response after max retries")

# Example usage
if __name__ == "__main__":
    endpoint_url = "http://localhost:8080/api/endpoints/external/long-running-hello-world"
    request_payload = {"key": "value"}

    try:
        result = call_endpoint(endpoint_url, request_payload)
        print("Final result:", result)
    except Exception as e:
        print("Error:", e)
```

For using endpoints from a Curiosity-based front-end or data connectors, you can simply rely on the appropriate built-in methods to call endpoints, as described in the next sections.

## Calling an Endpoint using curl

For example, to call the first endpoint above, which does not use authorization and returns immediately:

```bash
curl -X POST -H "Accept: application/json" -H "Content-Type: application/json" http://localhost:8080/api/endpoints/external/hello-world
```

This should print `Hello World from Curiosity - today is ...` on your terminal.

For pooling endpoints, the behaviour you'll see will depend on how long the endpoint takes to run:

```bash
curl -v -X POST -H "Accept: application/json" -H "Content-Type: application/json" http://localhost:8080/api/endpoints/external/long-running-hello-world
```

Depending on how long the endpoint will take to execute, you'll see one of the two responses:

- **Accepted** (Status code = 202)

```bash
* Host localhost:8080 was resolved.
* IPv6: ::1
* IPv4: 127.0.0.1
*   Trying [::1]:8080...
* Connected to localhost (::1) port 8080
* using HTTP/1.x
> POST /api/endpoints/external/long-running-hello-world HTTP/1.1
> Host: localhost:8080
> User-Agent: curl/8.10.1
> Accept: application/json
> Content-Type: application/json
>
* Request completely sent off
< HTTP/1.1 202 Accepted
< Content-Length: 0
< Date: Thu, 13 Mar 2025 21:10:36 GMT
< X-Frame-Options: SAMEORIGIN
< X-MSK-ENDPOINT-KEY: <cache-uid>:<proof>
< X-MSK-TTFB: 1009.891
<
* Connection #0 to host localhost left intact
```

or 

- **OK** (Status code = 200)

```bash
* Host localhost:8080 was resolved.
* IPv6: ::1
* IPv4: 127.0.0.1
*   Trying [::1]:8080...
* Connected to localhost (::1) port 8080
* using HTTP/1.x
> POST /api/endpoints/external/long-running-hello-world HTTP/1.1
> Host: localhost:8080
> User-Agent: curl/8.10.1
> Accept: application/json
> Content-Type: application/json
>
* Request completely sent off
< HTTP/1.1 200 OK
< Content-Length: 64
< Content-Type: text/plain
< Date: Thu, 13 Mar 2025 21:12:03 GMT
< X-Frame-Options: SAMEORIGIN
< X-MSK-TTFB: 359.5499
<
Hello World from Curiosity - this endpoint took 359 milliseconds to run
* Connection #0 to host localhost left intact
```

As you can see in the `202` response, the `X-MSK-ENDPOINT-KEY` header is set by the server. You can use that to repeat the requests that returned 202 Accepted using the logic described above.


For calling endpoints that require Authorization, you'll need to pass a bearer token and use a different URL. You can generate an endpoint token under the Management interface, in the Endpoints page (key button on the upper right corner). For example, using the replay endpoint above, we can use the following command to call the endpoint with a token:

```bash
curl -X POST -H "Accept: application/json" -H "Content-Type: application/json" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJGb3IiOiJDQ0UiLCJDcmVhdGVkQnkiOiJhZG1pbiIsIkNyZWF0ZWRCeVVJRCI6Ik16bVlDcVpyamVkZGFxdUZBcTR3d2YiLCJFbmRwb2ludCI6IioiLCJuYmYiOjE3NDE5MDA3OTcsImV4cCI6MjA1NzI2MDg1NywiaXNzIjoiQ3VyaW9zaXR5LlNlY3VyaXR5LkJlYXJlciIsImF1ZCI6IkN1cmlvc2l0eSJ9.u3_IGGy5dudcapmH61d7ehSTpwgPy05CZFMNHzzVgH0" --data "Why don’t APIs ever get lost? Because they always REST." http://localhost:8080/api/endpoints/token/run/replay
```

*Don't forget to replace the token above with one created for your workspace*

As the endpoint returns immediately, you should see the body you sent printed back on your terminal:

```
Why don’t APIs ever get lost? Because they always REST.
```

## Calling an Endpoint from a Data Connector

For calling endpoints from a data connector, the Curiosity Library nuget package provides a helper class to encapsulate the logic for pooling endpoints. The [data connector](/data-connector/src/App.cs) of this repository uses it as follows, with the endpoint token read from the `CURIOSITY_ENDPOINTS_TOKEN` environment variable:

```csharp
//Endpoints can be called using the EndpointsClient wrapper class.
var endpointClient = new EndpointsClient(serverUrl, endpointToken);

//The logic for calling a sync or pooling endpoint is encapsulated for you:
var responseHelloWorld = await endpointClient.CallAsync<string>("hello-world");
var responsePooling    = await endpointClient.CallAsync<string>("long-running-hello-world");

//If you need to pass a string value as the body:
var responseReplay     = await endpointClient.CallAsync<string, string>("replay", "Why don't APIs ever get lost? Because they always REST.");

//Or an object, which is sent as JSON:
var responseJson       = await endpointClient.CallAsync<Nodes.Device, Nodes.Device>("replay", new Nodes.Device() { Name = "Test Device" });
```

If you don't need the response, `await endpointClient.CallAsync("hello-world");` calls the endpoint without reading it. If your endpoint returns a JSON response, you can also use the type parameter to get a response in the right format, for example:

```csharp
class SumRequest
{
    public int A { get; set;}
    public int B { get; set;}
}
class SumResponse
{
    public int Value { get; set;}
}
var sumResponse = await endpointClient.CallAsync<SumRequest, SumResponse>("sum-values", new SumRequest(){ A = 10, B = 25 });
Console.WriteLine($"The sum of 10 + 25 is {sumResponse.Value}");
```

## Calling an Endpoint from the Front-End

For calling endpoints from the front-end, the approach is very similar to that of a data-connector, except you don't have to provide the Authorization header, as that is automatically taken from the logged-in user in the application.

Calling an endpoint from the front-end can be done as follows. `Endpoints` is a class generated by the workspace with one constant per endpoint path (see the [front-end guide](/custom-front-end/INSTRUCTIONS.md)), so a renamed or deleted endpoint becomes a compile error instead of a failing call:

```csharp
var helloResponse = await API.Endpoints.CallAsync<string>(Endpoints.HelloWorld);
var sumResponse   = await API.Endpoints.CallAsync<SumResponse>(Endpoints.SumValues, new SumRequest(){ A = 10, B = 25});
```

In the front-end of this repository, these calls are wrapped in one method per endpoint in [`src/Api/SupportApi.cs`](/custom-front-end/src/Api/SupportApi.cs), the only file that calls `API.Endpoints`. Pass `readOnlyCompatible: true` for an endpoint marked `ReadOnly`, so it can also run on a read-only replica:

```csharp
internal static Task<SimilarCase[]> SuggestSimilarCasesAsync(UID128 caseUID)
    => API.Endpoints.CallAsync<SimilarCase[]>(Endpoints.SuggestSimilarCases, caseUID, readOnlyCompatible: true);
```

You can also retrieve status messages emitted by the endpoint for long running pooling endpoints, using the following methods:
```csharp
//Show a standard toast with the status sent by the endpoint, if any
var response = await API.Endpoints.CallWithStatusAsync<string>(Endpoints.LongRunningEndpointWithStatus);

//Show the status message embedded in a different element
var status = TextBlock("Running endpoint, please wait...");
// ... add the status text block somewhere in the UI
var response = await API.Endpoints.CallAsync<string>(Endpoints.LongRunningEndpointWithStatus, statusUpdatusReceived: msg => status.Text = msg);
status.Text = response;
```

## Calling an Endpoint from another Endpoint

Sometimes it can be useful to call an endpoint from another endpoint (for example, if you need to re-use the code across endpoints, you can store the shared code as an endpoint, and invoke it as necessary from other endpoints). For that, you can use the following method that is always available from within an endpoint.

For example, [`call-another-endpoint.cs`](/custom-endpoints/code/endpoints/call-another-endpoint.cs) calls the `hello-world` endpoint. Endpoints also have access to the generated `Endpoints` class, so the route is checked when the code is compiled:

```csharp
[endpoint: Curiosity.Endpoints.Path("call-another-endpoint")]
[endpoint: Curiosity.Endpoints.AccessMode("Unrestricted")]

// The route is the generated helper, not a string: deleting hello-world makes this a compile error.
return await RunEndpointAsync<string>(Endpoints.HelloWorld);
```

## Exporting and Importing Endpoints

You can export and import all the code endpoints in your workspace by using the respective Import and Export endpoints button on the Endpoints setting page. This is useful for saving your endpoints source-code on an external code repository, or for migrating endpoints from a developer instance to a production instance.

The Curiosity CLI can do the same from the command line, using a Library token (Manage > Tokens > Library). To import every endpoint of this guide into your workspace, run from the repository root:

```bash
curiosity-cli import-workspace-definitions -s http://localhost:8080/ -t <library-token> --import-path custom-endpoints -z true
```

To export the definitions of a workspace into a folder with the same layout (`code/endpoints/<path>.cs`):

```bash
curiosity-cli export-workspace-definitions -s http://localhost:8080/ -t <library-token> --export-path <folder> -u true
```

## Global endpoints scope

Endpoints have a global scope defined that is available to be used directly from within the code. The main methods and global objects are listed below:

```csharp
// Global objects

ScopedGraph Graph;                   // Access to the Graph object (a ReadOnlyGraph in a ReadOnly endpoint)
ILogger Logger;                      // Logger object to write to the application logs
Stream Body;                         // Body of the request, if any (read it with ParseBody<T>() or ReadBodyAsText())
IReadOnlyDictionary<string, StringValues> Headers; // Headers of the HTTP request
ChatAI ChatAI;                       // Access to the generative AI methods
AgentAI AgentAI;                     // Access to the workspace's agents
QueryTracker Tracker;                // Can be used to track statistics about the endpoint
CancellationToken CancellationToken; // Cancelation token for the request, pass it to every awaited call
UID128 CurrentUser;                  // Returns the current user calling the endpoint, if any
UID128 CurrentEndpointUID;           // The UID of this endpoint

// Generated helpers: N.<Type>.<Field>, E.<Edge>, Endpoints.<Path>, AI_Tools.<Name>, AIAssistants.<Name>, ...

// Methods

IQuery Query();            // Create a new query
IQuery Q();                //   ""
T ParseBody<T>();          // Deserialize the JSON body (ParseBody<UID128>() reads a body that is a bare UID)
string ReadBodyAsText();   // Read the body as text
Task RelayStatusAsync(string message); // Send a progress message to the caller of a pooling endpoint (can be used to relay status to the front-end)
EndpointResponse Ok(object content = null, string contentType = null);         // Status helpers: also BadRequest, Unauthorized, NotFound, Forbid,
EndpointResponse BadRequest(object content = null, string contentType = null); // Redirect and StatusCode(HttpStatusCode, ...)
Task<T> RunEndpointAsync<T>(EndpointUID endpointName, string body = null, UID128? user = null, bool? runAsAdmin = null, ...) where T : class;          //Runs another endpoint by name
Task<T> RunEndpointOnPrimaryAsync<T>(EndpointUID endpointName, string body = null, UID128? user = null, bool? runAsAdmin = null, ...) where T : class; //Runs another endpoint on the primary application server. If on the primary, behaves the same as RunEndpointAsync
Task<string> GetSecretAsync(SecretUID secret); // Read a workspace secret
HttpClient CreateHttpClient();                 // An HttpClient for calling external services
string GetDownloadTokenForFile(UID128 fileUID, TimeSpan validity); // Generate a download token to be able to read files using the file endpoint
```

## Sample endpoints

Return the email from the user calling the endpoint ([`get-user-email.cs`](/custom-endpoints/code/endpoints/get-user-email.cs)):
```csharp
if (CurrentUser.IsNull()) return "Endpoint was not called by a user";

Logger.LogInformation("The endpoint was called by {0}", CurrentUser);

return Graph.Get(CurrentUser).GetString(nameof(_User.Email));
```

Paginate through Parts in the dataset ([`get-parts.cs`](/custom-endpoints/code/endpoints/get-parts.cs)):
```csharp
const int pageSize = 50;

var request = ParseBody<PartsRequest>();

return Q().StartAt(N.Part.Type).Skip(request.Page * pageSize).Take(pageSize).Emit();

public class PartsRequest
{
    public int Page { get; set; }
}
```

Do a full-text search based on a request ([`get-with-search.cs`](/custom-endpoints/code/endpoints/get-with-search.cs)):
```csharp
const int pageSize = 50;

var request = ParseBody<SimpleSearchRequest>();

return Q().StartSearch(N.SupportCase.Type, N.SupportCase.Content, SearchExpression.For(SearchToken.StartsWith(request.Query), request.Query))
          .Skip(request.Page * pageSize)
          .Take(pageSize)
          .Emit();

public class SimpleSearchRequest
{
    public string Query { get; set; }
    public int    Page  { get; set; }
}
```

Return similar results ([`get-similar.cs`](/custom-endpoints/code/endpoints/get-similar.cs)):
```csharp
var request = ParseBody<SimilarCasesRequest>();

return (await Q().StartAtSimilarTextAsync(request.Query, nodeTypes: [N.SupportCase.Type], count: 50)).EmitWithScores();

public class SimilarCasesRequest
{
    public string Query { get; set; }
}
```

Return similar results filtered by manufacturer ([`get-similar-by-manufacturer.cs`](/custom-endpoints/code/endpoints/get-similar-by-manufacturer.cs)):
```csharp
var request = ParseBody<SimilarCasesRequest>();

return (await Q().StartAtSimilarTextAsync(request.Query, nodeTypes: [N.SupportCase.Type], count: 500))
          .IsRelatedTo(Node.GetUID(N.Manufacturer.Type, request.Manufacturer))
          .EmitWithScores();

public class SimilarCasesRequest
{
    public string Query        { get; set; }
    public string Manufacturer { get; set; }
}
```

Return the cases most similar to a given case, for the same device, with their scores ([`suggest-similar-cases.cs`](/custom-endpoints/code/endpoints/suggest-similar-cases.cs), used by the support case workspace in the front-end). The body is the bare UID of the case:
```csharp
var caseUID = ParseBody<UID128>();

if (!Graph.TryGet(caseUID, out var caseNode) || caseNode.Type != N.SupportCase.Type) return BadRequest("Expected the UID of a support case");

var summary       = caseNode.GetString(N.SupportCase.SupportCaseSummary);
var firstQuestion = Q().StartAt(caseUID).Out(N.SupportCaseMessage.Type, E.HasMessage).AsEnumerable().FirstOrDefault()?.GetString(N.SupportCaseMessage.Message);
var device        = Q().StartAt(caseUID).Out(N.Device.Type, E.ForDevice).AsUIDEnumerable().FirstOrDefault();

var similar = await Q().StartAtSimilarTextAsync(summary + "\n" + firstQuestion, count: 500, nodeTypes: [N.SupportCase.Type]);

if (device.IsNotNull()) similar = similar.IsRelatedTo(device);

return similar.AsScoredUIDEnumerable()
              .Where(s => s.UID.UID != caseUID)
              .OrderByDescending(s => s.Score)
              .Select(s => new SimilarCase { UID = s.UID.UID, Score = s.Score })
              .ToArray();

public class SimilarCase
{
    public UID128 UID   { get; set; }
    public float  Score { get; set; }
}
```

Call an LLM (don't forget to configure an AI provider first) ([`generate-text-completion.cs`](/custom-endpoints/code/endpoints/generate-text-completion.cs)):
```csharp
var prompts = new List<Mosaik.AI.IChatAIMessage>
{
    new Mosaik.AI.ChatAIMessage(Mosaik.AI.ChatAuthorRole.System, "You're an AI assistant, answer only with a pirate language"),
    new Mosaik.AI.ChatAIMessage(Mosaik.AI.ChatAuthorRole.User,   "What's to do in Toulouse"),
};

var answer = await ChatAI.GetCompletionAsync(CurrentUser, prompts, cancellationToken: CancellationToken);

return answer.Content;
```


## Support for custom chat endpoints

In order to extend and implement your own custom Chat interface, you can implement endpoints responsible for posting messages to a chat, including your own specialization logic (for example, adapting the system prompt, adding and tracking context, etc.). Here's the endpoint used by the support chat of the front-end ([`support-chat/post-message.cs`](/custom-endpoints/code/endpoints/support-chat/post-message.cs)). It checks that the chat belongs to the caller, appends the chat's topic to the system prompt, adds the user's message to the chat (or replaces one, for "Edit" and "Regenerate") and triggers the completion using the LLM, with the tools, agents, skills and attachments the user picked:

```csharp
var request = ParseBody<SupportChatMessageRequest>();

if (!ChatAI.TryGetUserForChat(request.ChatUID, out var owner) || owner != CurrentUser) return Forbid();

var systemPrompt = $"You're an AI assistent specialized in {request.Context?.Topic ?? "All"}.";

var messageUID = await ChatAI.AddUserMessageAsync(request.ChatUID, CurrentUser, request.Message,
                                                  replaceMessageUID: request.ReplaceMessageUID,
                                                  attachmentUIDs:    request.Attachments);

// The turn relays what a user wrote, so the workspace's chat policies apply to it.
await ChatAI.TriggerChatAsync(request.ChatUID, CurrentUser, messageUID,
                              enabledTools:  request.Tools?.Select(uid => new ToolUID(uid)).ToArray(),
                              enabledAgents: request.Agents?.Select(uid => new AgentUID(uid)).ToArray(),
                              enabledSkills: request.Skills?.Select(uid => new SkillUID(uid)).ToArray(),
                              systemPrompt:  systemPrompt,
                              enforcePolicy: true);

return messageUID;

public class SupportChatMessageRequest
{
    public string             Message           { get; set; }
    public UID128             ChatUID           { get; set; }
    public UID128             ReplaceMessageUID { get; set; }
    public UID128[]           Tools             { get; set; }
    public UID128[]           Agents            { get; set; }
    public UID128[]           Skills            { get; set; }
    public UID128[]           Attachments       { get; set; }
    public SupportChatContext Context           { get; set; }
}

public class SupportChatContext
{
    public string Topic { get; set; }
}
```

`ChatAI.TriggerChatAsync` skips the workspace's chat policies unless `enforcePolicy: true` is passed, so pass it whenever the endpoint relays what a user wrote.

Context management can be achieved with endpoints to set and read the chat context, stored in a `SupportChatContext` node keyed by the chat's UID ([`support-chat/set-context.cs`](/custom-endpoints/code/endpoints/support-chat/set-context.cs)):

```csharp
var request = ParseBody<SupportChatSetContextRequest>();

if (!Graph.HasNodeOfType(request.ChatUID, _MessageChannel.Type)) return BadRequest("Expected the UID of a chat");

if (!ChatAI.TryGetUserForChat(request.ChatUID, out var owner) || owner != CurrentUser) return Forbid();

var contextNode = await Graph.GetOrAddLockedAsync(N.SupportChatContext.Type, request.ChatUID.ToString());

contextNode.SetString(N.SupportChatContext.Topic, request.Context.Topic);

await Graph.CommitAsync(contextNode);

return Ok();

public class SupportChatSetContextRequest
{
    public UID128             ChatUID { get; set; }
    public SupportChatContext Context { get; set; }
}

public class SupportChatContext
{
    public string Topic { get; set; }
}
```

and ([`support-chat/get-context.cs`](/custom-endpoints/code/endpoints/support-chat/get-context.cs), a `ReadOnly` endpoint whose body is the bare UID of the chat):

```csharp
var chatUID = ParseBody<UID128>();

if (Graph.HasNodeOfType(chatUID, _MessageChannel.Type)
 && ChatAI.TryGetUserForChat(chatUID, out var owner) && owner == CurrentUser
 && Graph.TryGet(N.SupportChatContext.Type, chatUID.ToString(), out var contextNode))
{
    return new SupportChatContext { Topic = contextNode.GetString(N.SupportChatContext.Topic) };
}

return new SupportChatContext { Topic = "All" };

public class SupportChatContext
{
    public string Topic { get; set; }
}
```


## Conclusion

Custom endpoints let you host your own business logic next to the data: they have direct access to the graph, the search and AI search indexes and the LLMs configured in your workspace, and can be called from data connectors, front-ends, other endpoints or any HTTP client.

## Next steps
- Build a custom user interface in the [User Interface Guide](/custom-front-end/INSTRUCTIONS.md)