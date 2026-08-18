[entityPostProcessing: Curiosity.EntityPostProcessing.UID("RnYLiA1ZoMqAqCybzouyeo")]
[entityPostProcessing: Curiosity.EntityPostProcessing.EntityType("_Organization")]

//ImportEndpoint("shared/import-test/level2-text")

// Read-only probe: it logs the resolved chain and never calls OverrideUID / OverrideValue /
// Ignore, so entity linking behaves exactly as it did before this fixture existed.
//
// The UID above is NOT free to choose. The importer stores whatever the file names, but the
// runtime looks the hook up by _EntityPostProcessing.For(entityType) - so it must be
// Hashes.Combine("_EntityPostProcessing".Hash128(), "_Organization".Hash128()), which is the
// value above. Changing EntityType means recomputing the UID.

Logger.LogInformation("import-test entity post-processing: {0}", ImportTestText.Normalize(importTestLevel2Text));
