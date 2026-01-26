[endpoint: Curiosity.Endpoints.Path("long-running-endpoint-with-status")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

for (int i = 0; i < 100; i++)
{
    await RelayStatusAsync($"Computing, at {i}%");
    await Task.Delay(100); //Simulates a long running task
}

return "Done, that took a while!";
