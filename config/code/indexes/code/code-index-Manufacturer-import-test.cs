[indexes: Curiosity.Indexes.CodeIndex("Manufacturer")]
[indexes: Curiosity.Indexes.Name("Import Test Probe")]

//ImportEndpoint("shared/import-test/level3-report")

// Enrichment-free probe over the smallest node type in the dataset. It logs the resolved import
// chain once per batch and writes nothing, so it cannot perturb the demo. Per the code-index
// contract, returning null means the whole batch succeeded.

Logger.LogInformation("import-test code index: {0} ({1} nodes)", ImportTestReport.For("code-index", importTestLevel3), ToIndex.Count);

return null;
