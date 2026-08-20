[indexes: Curiosity.Indexes.CodeIndex("Manufacturer")]
[indexes: Curiosity.Indexes.Name("Import Test Probe")]

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

// Enrichment-free probe over the smallest node type in the dataset. It logs the resolved import
// chain once per batch and writes nothing, so it cannot perturb the demo. Per the code-index
// contract, returning null means the whole batch succeeded.
//
// This scope has both a Graph and a Logger, so it drives both halves of the shared handshake with
// exactly the same shared code the endpoint and AI tool compile - which is the whole point of
// keeping the shared layer free of scope globals.

ImportTestScope.Trace(Logger, "code-index", importTestLevel3);

Logger.LogInformation("import-test code index: {0} ({1} nodes)", ImportTestScope.Describe(Graph), ToIndex.Count);

return null;

