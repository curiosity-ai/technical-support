[task: Curiosity.Tasks.Name("Data Connector Scheduled Task")]
[task: Curiosity.Tasks.UID("Skf8wrT9nMLCsqUVRqWs3f")]
[task: Curiosity.Tasks.Schedule("0 1 * * *")]

Logger.LogInformation("Data Connector run at: {0:u}", DateTimeOffset.UtcNow);

