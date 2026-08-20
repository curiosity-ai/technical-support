[endpoint: Curiosity.Endpoints.Path("shared/import-test/level2-text")]
[endpoint: Curiosity.Endpoints.AccessMode("AdminOnly")]

using SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core.Shared;
using SharedCode.SharedEndpoint.Shared.ImportTest.Level2Text;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level2Text.Shared;

// Declarations extracted to shared code SharedCode.SharedEndpoint.Shared.ImportTest.Level2Text (endpoint shared/import-test/level2-text)
await global::SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core.Shared.Run(Self);
return await global::SharedCode.SharedEndpoint.Shared.ImportTest.Level2Text.Shared.Run(Self);

