[endpoint: Curiosity.Endpoints.Path("long-running-endpoint-with-status")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]
[endpoint: Curiosity.Endpoints.ReadOnly]

for (int i = 0; i < 100; i++)
{
    await RelayStatusAsync($"Computing, at {i}%");
    await Task.Delay(100, CancellationToken); //Simulates a long running task
}

return "Done, that took a while!";
