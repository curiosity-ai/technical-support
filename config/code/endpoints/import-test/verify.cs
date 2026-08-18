[endpoint: Curiosity.Endpoints.Path("import-test/verify")]
[endpoint: Curiosity.Endpoints.AccessMode("AdminOnly")]

using System.Linq;

// The consumer owns the usings: only THIS block is hoisted above the imported bodies, which is why
// the shared layer below is written fully qualified.

//ImportEndpoint("shared/import-test/level3-report")

var chainSegments = importTestLevel3.Split('>').Select(s => s.Trim()).ToArray();

Logger.LogInformation("import-test/verify resolved {0} chain segments: {1}", chainSegments.Length, importTestLevel3);

// These two are already inside level3's closure and must be deduplicated rather than re-emitted.
// They also sit below a statement instead of in the header block - the regex is not anchored to the
// top of the file, so position does not matter.
//ImportEndpoint("shared/import-test/level1-core")
//ImportEndpoint("shared/import-test/level2-text")

return ImportTestReport.For("code-endpoint", importTestLevel3);
