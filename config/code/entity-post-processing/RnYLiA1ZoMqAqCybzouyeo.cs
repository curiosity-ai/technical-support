[entityPostProcessing: Curiosity.EntityPostProcessing.UID("RnYLiA1ZoMqAqCybzouyeo")]
[entityPostProcessing: Curiosity.EntityPostProcessing.EntityType("_Organization")]

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

// Read-only probe: it logs the resolved chain and never calls OverrideUID / OverrideValue /
// Ignore, so entity linking behaves exactly as it did before this fixture existed.
//
// The UID above is NOT free to choose. The importer stores whatever the file names, but the
// runtime looks the hook up by _EntityPostProcessing.For(entityType) - so it must be
// Hashes.Combine("_EntityPostProcessing".Hash128(), "_Organization".Hash128()), which is the
// value above. Changing EntityType means recomputing the UID.

ImportTestScope.Trace(Logger, "entity-post-processing", importTestLevel3);

Logger.LogInformation("import-test entity post-processing scope: {0}", ImportTestScope.Describe(Graph));

