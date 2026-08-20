[migration: Curiosity.Tasks.TaskName("Import Test Probe (Migration)")]
[migration: Curiosity.Tasks.UID("importTestMigr11111111")]

//ImportEndpoint("shared/import-test/level3-report")

// Fixture only - logs the resolved import chain and mutates nothing. The importer always stores
// migration tasks with triggerRunOnStartup: false, so this never fires unattended; run it from
// Manage / Migrations.

ImportTestScope.Trace(Logger, "migration-task", importTestLevel3);

Logger.LogInformation("import-test migration-task scope: {0}", ImportTestScope.Describe(Graph));

