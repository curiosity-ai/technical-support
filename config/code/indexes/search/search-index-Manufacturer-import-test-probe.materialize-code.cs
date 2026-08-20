[indexes: Curiosity.Indexes.SearchIndex("Manufacturer")]
[indexes: Curiosity.Indexes.Name("Import Test Probe")]
[indexes: Curiosity.Indexes.MaterializeCode]

using SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core.Shared;
using SharedCode.SharedEndpoint.Shared.ImportTest.Level2Text;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level2Text.Shared;
await global::SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core.Shared.Run(Self);
await global::SharedCode.SharedEndpoint.Shared.ImportTest.Level2Text.Shared.Run(Self);
// migrated: ImportEndpoint("shared/import-test/level2-text") -> using SharedCode.SharedEndpoint.Shared.ImportTest.Level2Text;

// CodeSearchIndexMaterializeNodeExecutionScope exposes only Content, IndexUID, CreateHttpClient,
// CancellationToken and Logger - there is no Graph at all here. This is the scope that forces the
// shared layer to stay global-free: a shared body that so much as mentioned Graph or CurrentUID at
// top level would be CS0103 the moment it was imported here.
//
// Importing level2-text also pulls level1-core transitively, so ImportTestCore and the instance
// class both resolve in a scope that never names them directly.

var materializeProbe = new ImportTestTextLayer("materialize");

Logger.LogInformation("import-test materialize: {0} / {1}", materializeProbe.Describe(), ImportTestText.Normalize(importTestLevel2Text));
