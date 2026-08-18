[indexes: Curiosity.Indexes.SearchIndex("Manufacturer")]
[indexes: Curiosity.Indexes.Name("Import Test Probe")]
[indexes: Curiosity.Indexes.MaterializeCode]

//ImportEndpoint("shared/import-test/level2-text")

// CodeSearchIndexMaterializeNodeExecutionScope exposes only Content, IndexUID, CreateHttpClient,
// CancellationToken and Logger - there is no Graph here, which is why the whole shared layer is
// kept graph-free. Importing level2-text also pulls level1-core transitively, so ImportTestCore
// resolves in a scope that never names it directly.

Logger.LogInformation("import-test materialize: {0}", ImportTestText.Normalize(ImportTestCore.MARKER + "   /   " + importTestLevel2Text));
