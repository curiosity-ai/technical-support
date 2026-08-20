[migration: Curiosity.Tasks.TaskName("Import Test Probe (Migration)")]
[migration: Curiosity.Tasks.UID("importTestMigr11111111")]

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

// Fixture only - logs the resolved import chain and mutates nothing. The importer always stores
// migration tasks with triggerRunOnStartup: false, so this never fires unattended; run it from
// Manage / Migrations.

ImportTestScope.Trace(Logger, "migration-task", importTestLevel3);

Logger.LogInformation("import-test migration-task scope: {0}", ImportTestScope.Describe(Graph));

