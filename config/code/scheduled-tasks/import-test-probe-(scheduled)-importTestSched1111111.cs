[task: Curiosity.Tasks.Name("Import Test Probe (Scheduled)")]
[task: Curiosity.Tasks.UID("importTestSched1111111")]
[task: Curiosity.Tasks.Schedule("0 3 1 1 *")]

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

// Fixture only - logs the resolved import chain and mutates nothing. Compilation happens on the
// first run, so use "Run now" in Manage / Scheduled Tasks rather than waiting for the cron.

ImportTestScope.Trace(Logger, "scheduled-task", importTestLevel3);

Logger.LogInformation("import-test scheduled-task scope: {0}", ImportTestScope.Describe(Graph));

