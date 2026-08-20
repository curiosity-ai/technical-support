[indexes: Curiosity.Indexes.SearchIndex("Manufacturer")]
[indexes: Curiosity.Indexes.Name("Import Test Probe")]
[indexes: Curiosity.Indexes.SearchCode]

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

// The probe runs on every search that reaches Manufacturer and always returns no results, so it
// cannot change what the demo's search UI shows. KeyedScoredUIDs has a private constructor -
// Empty() is the factory for "no results".

ImportTestScope.Trace(Logger, "search-index", importTestLevel3);

Logger.LogInformation("import-test search index scope: {0} (query: {1})", ImportTestScope.Describe(Graph), SearchQuery.OriginalQuery);

return KeyedScoredUIDs.Empty();
