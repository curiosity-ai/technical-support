[endpoint: Curiosity.Endpoints.Path("long-running-hello-world")]
[endpoint: Curiosity.Endpoints.AccessMode("Unrestricted")]

var sw = ValueStopwatch.StartNew();
await Task.Delay(Random.Shared.Next(15_000));
return $"Hello World from Curiosity - this endpoint took {sw.GetElapsedTime().TotalMilliseconds:n0} milliseconds to run";
