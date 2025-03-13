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
1. [Conclusion](#conclusion)

## Introduction

One of the features of Curiosity Workspaces is the ability to write custom endpoints that can extend the functionality and write custom business logic for your specific needs. A custom endpoint is a hosted function inside your workspace that handles incoming requests for a specific URL, does some processing and returns a response. 

Endpoints have direct access to the underlying graph database, NLP models and other functionality. Custom Endpoints are written in C# from within the workspace, and can be restricted to logged users or admin users of your workspace. You can also call endpoints externally by creating and Endpoint Token.

## Creating an Endpoint

To get started, navigate to the Management interface and select `Endpoints`. Click on the `+` icon next to the search box to create a new endpoint. 

You can write your endpoint logic using the Code area. The response of your endpoint will be given by the value you return from your code (the example above always returns the string "hello word").

There are a few options for you to configure:

- **Endpoint Path**: This defines the path of your final endpoint URL. You can use slashes ('/') to create hierarchy of endpoints (this can be useful for accessing endpoints directly using Endpoint Tokens, which can be scoped to a given endpoint name or path)

- **Cache Duration**: Specifies if the application should cache the result of the computation, and for how long. Set to zero to always run the endpoint code on every call. Caching can be useful for endpoints that perform expensive computations and might be called often by your end-users or external APIs - an example could be an endpoint providing daily aggregates or analytics that have to iterate through a large amount of data.

- **Authorization**: When being called from within the front-end, endpoints can be restricted to either all logged users, or to only admin users.

- **Mode**: Select between Pooling Endpoints (run in the background and can execute long-running tasks without timing out) and Sync endpoints (useful for short requests, easier to call from outside applications not using Curiosity-based code).

- **Read Only**: Run the endpoint in read-only mode. Useful for implemeting endpoints that shoudn't change data, and for endpoints that can also run on read-only replicas.

For this introduction, let's create 2 endpoints:

- Endpoint 1: 
  - Path: hello-world
  - Mode: Run in sync
  - Authorization: Unrestricted
  - Code: `return $"hello world from Curiosity - today is {DateTimeOffset.UtcNow:u}";`

- Endpoint 2: 
  - Path: long-running-hello-world
  - Mode: Pooling
  - Authorization: Unrestricted
  - Code: 
  
  ```csharp 
  var sw = ValueStopwatch.StartNew();
  await Task.Delay(Random.Shared.Next(15_000));
  return $"Hello World from Curiosity - this endpoint took {sw.GetElapsedTime().TotalMilliseconds:n0} milliseconds to run";
  ````

- Endpoint 3: 
  - Path: replay
  - Mode: Run in sync
  - Authorization: Restricted to logged users
  - Code: 
  
  ```csharp 
  return $"Hello World! You sent: {(Body ?? "Nothing")}";
  ````

- Endpoint 4:
  - Path: sum-values
  - Mode: Run in sync
  - Authorization: Restricted to logged users
  - Code:

  ```csharp
  class SumRequest
  {
      public int A { get; set; }
      public int B { get; set; }
  }

  class SumResponse
  {
      public int Value { get; set; }
      public string Error { get; set; }
  }

  try
  {
      var request = Body.FromJson<SumRequest>();
      return new SumResponse() { Value = request.A + request.B };
  }
  catch (Exception E)
  {
      return new SumResponse() { Error = E.Message };
  }
  ```


## How to call an endpoint

In general, in order to call an endpoint, you need to send an HTTP request to the full endpoint URL, which will depend on where your workspace is hosted. The format for an endpoint url is: 

- For endpoints that need authorization to run: `{workspace-url}/api/endpoints/token/run/{endpoint-path}`

- For endpoints that do not need authorization: `{workspace-url}/api/endpoints/external/{endpoint-path}`

For endpoint that require authorization, only `POST` requests are valid. They must include a valid user or endpoint Bearer token in the `Authentication` header.

For unrestricted endpoints, both `GET` and `POST` calls are valid. If you need to pass a value in the body of the request, then only `POST` requests can be used.

For endpoints that run in sync with the call, the response of the endpoint will always be the return value from executing the code, or an error code if it failed. For endpoints that run in pooling mode, if the execution of the endpoint takes more than a few seconds, you will receive a `202 Accepted` status code. You can then either retry the exact same request (with exact the same request body), or use the value returned under the header `MSK-ENDPOINT-KEY` to try again the request without passing the same request body. Until you receive a valid final answer, retrying the call will not trigger again the endpoint. You might need to repeat the call as long as you're receiving a `202 Accepted` status code.

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
            headers['MSK-ENDPOINT-KEY'] = msk_endpoint_key
            response = requests.post(url, headers=headers)
        else:
            response = requests.post(url, json=payload, headers=headers)

        if response.status_code == 202:
            print("Processing... retrying in", retry_delay, "seconds")
            msk_endpoint_key = response.headers.get('MSK-ENDPOINT-KEY')
            time.sleep(retry_delay)
            attempt += 1
        elif response.status_code == 200:
            return response.json()
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

For example, to call the first endpoint created above, which does not use authorization and is not pooling:

```bash
curl -X POST -H "Accept: application/json" -H "Content-Type: application/json" http://localhost:8080/api/endpoints/external/hello-world
```

This should print `hello world` on your terminal.

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
< Access-Control-Expose-Headers: MSK-ENDPOINT-KEY
< Access-Control-Expose-Headers: CalculationProgress
< X-Frame-Options: SAMEORIGIN
< MSK-ENDPOINT-KEY: CNH9EjUAN532tLzNgcUvrh
< MSK-TTFB: 1009.891
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
< Access-Control-Expose-Headers: MSK-ENDPOINT-KEY
< X-Frame-Options: SAMEORIGIN
< MSK-ENDPOINT-KEY: KGs8RMsKGndPcBLZkVnQbK
< MSK-TTFB: 359.5499
<
Hello World from Curiosity - this endpoint took 359 milliseconds to run
* Connection #0 to host localhost left intact
```

As you can see in the response, the `MSK-ENDPOINT-KEY` header is set by the server. You can use that to repeat the requests that returned 202 Accepted using the logic described above.


For calling endpoints that require Authorization, you'll need to pass a bearer token and use a different URL. You can generate an endpoint token under the Management interface, in the Endpoints page (key button on the upper right corner). For example, using the replay endpoint we defined above, we can use the following command to call the endpoint with a token:

```bash
curl -X POST -H "Accept: application/json" -H "Content-Type: application/json" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJGb3IiOiJDQ0UiLCJDcmVhdGVkQnkiOiJhZG1pbiIsIkNyZWF0ZWRCeVVJRCI6Ik16bVlDcVpyamVkZGFxdUZBcTR3d2YiLCJFbmRwb2ludCI6IioiLCJuYmYiOjE3NDE5MDA3OTcsImV4cCI6MjA1NzI2MDg1NywiaXNzIjoiQ3VyaW9zaXR5LlNlY3VyaXR5LkJlYXJlciIsImF1ZCI6IkN1cmlvc2l0eSJ9.u3_IGGy5dudcapmH61d7ehSTpwgPy05CZFMNHzzVgH0" --data "C’est la fin des haricots" http://localhost:8080/api/endpoints/token/run/replay
```

*Don't forget to replace the token above with one created for your workspace*

As this is a sync endpoint, you should see the following printed on your terminal:

```
Hello World! You sent: C’est la fin des haricots
```

## Calling an Endpoint from a Data Connector

For calling endpoints from a data connector, the Curiosity Library nuget package provides a helper class to encapsulate the logic for pooling endpoints. You can use it as follows:

```csharp
var endpointToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJGb3IiOiJDQ0UiLCJDcmVhdGVkQnkiOiJhZG1pbiIsIkNyZWF0ZWRCeVVJRCI6Ik16bVlDcVpyamVkZGFxdUZBcTR3d2YiLCJFbmRwb2ludCI6IioiLCJuYmYiOjE3NDE5MDA3OTcsImV4cCI6MjA1NzI2MDg1NywiaXNzIjoiQ3VyaW9zaXR5LlNlY3VyaXR5LkJlYXJlciIsImF1ZCI6IkN1cmlvc2l0eSJ9.u3_IGGy5dudcapmH61d7ehSTpwgPy05CZFMNHzzVgH0"
var endpointClient     = new EndpointsClient("http://localhost:8080/", endpointToken);
var responseHelloWorld = await endpointClient.CallAsync<string>("hello-world");
var responsePooling    = await endpointClient.CallAsync<string>("long-running-hello-world");
var responseReplay     = await endpointClient.CallAsync<string>("replay", "C’est la fin des haricots");
```

If your endpoint returns a JSON response, you can also use the type parameter to get a response in the right format, for example:
```csharp
class SumRequest
{
    public int A { get; set;}
    public int B { get; set;}
}
class SumResponse
{
    public int Value { get; set;}
    public string Error { get; set;}
}
var sumResponse   = await endpointClient.CallAsync<SumResponse>("sum-values", new SumRequest(){ A = 10, B = 25 });
Console.WriteLine($"The sum of 10 + 25 is {sumResponse.Value}");
var errorResponse = await endpointClient.CallAsync<SumResponse>("sum-values", "invalid body");
Console.WriteLine($"Sending invalid body -> {errorResponse.Error}");
```

## Calling an Endpoint from the Front-End

For calling endpoints from the front-end, the approach is very similar to that of a data-connector, except you don't have to provide the Authorization header, as that is automatically taken from the logged-in user in the application.

Calling an endpoint from the front-end can be done as follows:

```csharp
var helloResponse = await Mosaik.API.Endpoints.CallAsync<string>("hello-world");
var sumResponse   = await Mosaik.API.Endpoints.CallAsync<SumResponse>("sum-values", new SumRequest(){ A = 10, B = 25});
```

## Calling an Endpoint from another Endpoint

Sometimes it can be useful to call an endpoint from another endpoint (for example, if you need to re-use the code across endpoints, you can store the shared code as an endpoint, and invoke it as necessary from other endpoints). For that, you can use the following method that is always available from within an endpoint.

For example, you can create the following endpoint to test this:

- Endpoint 5:
  - Path: sum-values
  - Mode: Run in sync
  - Authorization: Restricted to logged users
  - Code:
  ```csharp
  return await RunEndpoint<string>("hello-world");
  ```

## Exporting and Importing Endpoints

You can export and import all the code endpoints in your workspace by using the respective Import and Export endpoints button on the Endpoints setting page. This is useful for saving your endpoints source-code on an external code repository, or for migrating endpoints from a developer instance to a production instance.


## Conclusion

Curiosity AI provides a flexible and configurable search engine with support for multiple languages, synonym handling, filtering, embeddings support and access control. Developers can customize search behavior to match their application's requirements and ensure efficient, secure data retrieval.

## Next steps
- Setup your own API endpoints in the [Custom Endpoints Guide](/custom-endpoints/INSTRUCTIONS.md)
- Build a custom user interface in the [User Interface Guide](/custom-front-end/INSTRUCTIONS.md)