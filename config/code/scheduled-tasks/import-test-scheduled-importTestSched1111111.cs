[task: Curiosity.Tasks.Name("Import Test Probe (Scheduled)")]
[task: Curiosity.Tasks.UID("importTestSched1111111")]
[task: Curiosity.Tasks.Schedule("0 3 1 1 *")]

//ImportEndpoint("shared/import-test/level3-report")

// Fixture only - logs the resolved import chain and mutates nothing. Compilation happens on the
// first run, so use "Run now" in Manage / Scheduled Tasks rather than waiting for the cron.

Logger.LogInformation("import-test scheduled task: {0}", ImportTestReport.For("scheduled-task", importTestLevel3));
