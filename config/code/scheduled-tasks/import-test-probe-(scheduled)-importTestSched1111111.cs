[task: Curiosity.Tasks.Name("Import Test Probe (Scheduled)")]
[task: Curiosity.Tasks.UID("importTestSched1111111")]
[task: Curiosity.Tasks.Schedule("0 3 1 1 *")]

//ImportEndpoint("shared/import-test/level3-report")

// Fixture only - logs the resolved import chain and mutates nothing. Compilation happens on the
// first run, so use "Run now" in Manage / Scheduled Tasks rather than waiting for the cron.

ImportTestScope.Trace(Logger, "scheduled-task", importTestLevel3);

Logger.LogInformation("import-test scheduled-task scope: {0}", ImportTestScope.Describe(Graph));

