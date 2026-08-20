[endpoint: Curiosity.Endpoints.Path("import-test/verify")]
[endpoint: Curiosity.Endpoints.AccessMode("AdminOnly")]

using SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core.Shared;
using SharedCode.SharedEndpoint.Shared.ImportTest.Level2Text;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level2Text.Shared;
using SharedCode.SharedEndpoint.Shared.ImportTest.Level2Graph;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level2Graph.Shared;
using SharedCode.SharedEndpoint.Shared.ImportTest.Level3Report;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level3Report.Shared;
using System.Linq;
await global::SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core.Shared.Run(Self);
await global::SharedCode.SharedEndpoint.Shared.ImportTest.Level2Text.Shared.Run(Self);
await global::SharedCode.SharedEndpoint.Shared.ImportTest.Level2Graph.Shared.Run(Self);
await global::SharedCode.SharedEndpoint.Shared.ImportTest.Level3Report.Shared.Run(Self);

// The consumer owns the usings: only THIS block is hoisted above the imported bodies, which is why
// the shared layer is written fully qualified.

// migrated: ImportEndpoint("shared/import-test/level3-report") -> using SharedCode.SharedEndpoint.Shared.ImportTest.Level3Report;

// Everything below is declared in an imported endpoint and instantiated here, in the consumer's
// own top-level code - instance class deriving from a base in a different import, generic class, 
// enum, struct, delegate, nested type, mutable static state, and a top-level local function.
var textLayer = new ImportTestTextLayer("import-test/verify");
var box = new ImportTestBox<string>(importTestLevel3);
var nested = new ImportTestText.Nested();
var counter = ImportTestGraphShape.Observations.Next();
var kind = ImportTestKind.Endpoint;
ImportTestFormatter shout = ImportTestText.Shout;

ImportTestGraphShape.Observations = counter;

var constructs = string.Join(", ",
    textLayer.Describe(),
    ImportTestReport.Describe(box),
    nested.Describe(),
    "counter:" + counter.Value,
    "kind:" + kind,
    "delegate:" + shout("ok"),
    "tail:" + ImportTestChainTail(importTestLevel3));

// The scope handshake: the shared layer never touches Graph or Logger itself, so this endpoint
// hands over its own. Graph here is Safe.Graph, which picks the writable overload.
var scopeDetail = ImportTestScope.Describe(Graph) + " user:" + (CurrentUser.IsNotNull() ? "yes" : "no");

var chainSegments = importTestLevel3.Split('>').Select(s => s.Trim()).ToArray();

Logger.LogInformation("import-test/verify resolved {0} chain segments; constructs: {1}", chainSegments.Length, constructs);

// These two are already inside level3's closure and must be deduplicated rather than re-emitted.
// They also sit below a statement instead of in the header block - the regex is not anchored to the
// top of the file, so position does not matter.
// migrated: ImportEndpoint("shared/import-test/level1-core") -> using SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core;
// migrated: ImportEndpoint("shared/import-test/level2-text") -> using SharedCode.SharedEndpoint.Shared.ImportTest.Level2Text;

return ImportTestReport.For("code-endpoint", importTestLevel3, scopeDetail + " | " + constructs);

