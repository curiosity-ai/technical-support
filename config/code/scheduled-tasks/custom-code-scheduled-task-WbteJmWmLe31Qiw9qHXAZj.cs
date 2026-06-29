[task: Curiosity.Tasks.Name("Custom Code Scheduled Task")]
[task: Curiosity.Tasks.UID("WbteJmWmLe31Qiw9qHXAZj")]
[task: Curiosity.Tasks.Schedule("0 1 * * *")]

Logger.LogInformation("Task run at: {0:u}", DateTimeOffset.UtcNow);

