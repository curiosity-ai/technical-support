[endpoint: Curiosity.Endpoints.Path("import-test/verify-readonly")]
[endpoint: Curiosity.Endpoints.ReadOnly]
[endpoint: Curiosity.Endpoints.AccessMode("AdminOnly")]

using SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core.Shared;
using SharedCode.SharedEndpoint.Shared.ImportTest.Level2Text;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level2Text.Shared;
using SharedCode.SharedEndpoint.Shared.ImportTest.Level2Graph;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level2Graph.Shared;
using SharedCode.SharedEndpoint.Shared.ImportTest.Level3Report;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level3Report.Shared;
await global::SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core.Shared.Run(Self);
await global::SharedCode.SharedEndpoint.Shared.ImportTest.Level2Text.Shared.Run(Self);
await global::SharedCode.SharedEndpoint.Shared.ImportTest.Level2Graph.Shared.Run(Self);
await global::SharedCode.SharedEndpoint.Shared.ImportTest.Level3Report.Shared.Run(Self);
// migrated: ImportEndpoint("shared/import-test/level3-report") -> using SharedCode.SharedEndpoint.Shared.ImportTest.Level3Report;

// Same closure as import-test/verify, but compiled against ReadOnlyCodeEndpointExecutionScope,
// whose Graph is a Safe.ReadOnlyGraph rather than a Safe.Graph. Handing it to the shared helper
// therefore binds the OTHER Describe overload - which is the point: the shared layer adapts to the
// scope through overload resolution instead of reaching for a global it cannot see.

return ImportTestReport.For("read-only-endpoint", importTestLevel3, ImportTestScope.Describe(Graph));

