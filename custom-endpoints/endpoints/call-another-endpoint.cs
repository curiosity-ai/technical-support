[endpoint: Curiosity.Endpoints.Path("call-another-endpoint")]
[endpoint: Curiosity.Endpoints.AccessMode("Unrestricted")]

return await RunEndpointAsync<string>("hello-world");
