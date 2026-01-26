[endpoint: Curiosity.Endpoints.Path("call-another-endpoint")]
[endpoint: Curiosity.Endpoints.AccessMode("Unrestricted")]

return await RunEndpoint<string>("hello-world");
