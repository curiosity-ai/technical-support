[task: Curiosity.Tasks.Name("Import Test Probe (Data Connector)")]
[task: Curiosity.Tasks.UID("impTestConn11111111111")]
[task: Curiosity.Tasks.Schedule("0 4 1 1 *")]

//ImportEndpoint("shared/import-test/level3-report")

// Same body as the scheduled-task fixture, imported through the data-connector folder instead, to
// prove both task flavours route through the same import expansion.

Logger.LogInformation("import-test data connector task: {0}", ImportTestReport.For("data-connector-task", importTestLevel3));
