[endpoint: Curiosity.Endpoints.Path("import-test/verify-readonly")]
[endpoint: Curiosity.Endpoints.AccessMode("AdminOnly")]
[endpoint: Curiosity.Endpoints.ReadOnly]

//ImportEndpoint("shared/import-test/level3-report")

// Same closure as import-test/verify, but compiled against ReadOnlyCodeEndpointExecutionScope
// instead of CodeEndpointExecutionScope - the two scopes have different ambient import sets, so
// this proves the shared layer does not depend on anything only the writable scope provides.

return ImportTestReport.For("read-only-endpoint", importTestLevel3);
