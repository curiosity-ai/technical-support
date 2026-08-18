[indexes: Curiosity.Indexes.SearchIndex("Manufacturer")]
[indexes: Curiosity.Indexes.Name("Import Test Probe")]
[indexes: Curiosity.Indexes.SearchCode]

//ImportEndpoint("shared/import-test/level3-report")

// The probe runs on every search that reaches Manufacturer and always returns no results, so it
// cannot change what the demo's search UI shows. KeyedScoredUIDs has a private constructor -
// Empty() is the factory for "no results".

Logger.LogInformation("import-test search index: {0} (query: {1})", ImportTestReport.For("search-index", importTestLevel3), SearchQuery.OriginalQuery);

return KeyedScoredUIDs.Empty();
