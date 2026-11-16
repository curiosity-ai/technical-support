[endpoint: Curiosity.Endpoints.Path("call-another-endpoint")]
[endpoint: Curiosity.Endpoints.AccessMode("Unrestricted")]

// The route is the generated helper, not a string: deleting hello-world makes this a compile error.
return await RunEndpointAsync<string>(Endpoints.HelloWorld);
