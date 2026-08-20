[endpoint: Curiosity.Endpoints.Path("shared/import-test/level2-graph")]
[endpoint: Curiosity.Endpoints.AccessMode("AdminOnly")]

using SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core.Shared;
using SharedCode.SharedEndpoint.Shared.ImportTest.Level2Graph;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level2Graph.Shared;

// Declarations extracted to shared code SharedCode.SharedEndpoint.Shared.ImportTest.Level2Graph (endpoint shared/import-test/level2-graph)
await global::SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core.Shared.Run(Self);
return await global::SharedCode.SharedEndpoint.Shared.ImportTest.Level2Graph.Shared.Run(Self);

