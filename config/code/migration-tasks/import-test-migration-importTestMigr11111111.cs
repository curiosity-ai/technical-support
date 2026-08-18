[migration: Curiosity.Tasks.TaskName("Import Test Probe (Migration)")]
[migration: Curiosity.Tasks.UID("importTestMigr11111111")]

//ImportEndpoint("shared/import-test/level3-report")

// Fixture only - logs the resolved import chain and mutates nothing. The importer always stores
// migration tasks with triggerRunOnStartup: false, so this never fires unattended; run it from
// Manage / Migrations.

Logger.LogInformation("import-test migration task: {0}", ImportTestReport.For("migration-task", importTestLevel3));
