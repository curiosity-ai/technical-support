[endpoint: Curiosity.Endpoints.Path("import-test/verify-readonly")]
[endpoint: Curiosity.Endpoints.AccessMode("AdminOnly")]
[endpoint: Curiosity.Endpoints.ReadOnly]

//ImportEndpoint("shared/import-test/level3-report")

// Same closure as import-test/verify, but compiled against ReadOnlyCodeEndpointExecutionScope,
// whose Graph is a Safe.ReadOnlyGraph rather than a Safe.Graph. Handing it to the shared helper
// therefore binds the OTHER Describe overload - which is the point: the shared layer adapts to the
// scope through overload resolution instead of reaching for a global it cannot see.

return ImportTestReport.For("read-only-endpoint", importTestLevel3, ImportTestScope.Describe(Graph));
