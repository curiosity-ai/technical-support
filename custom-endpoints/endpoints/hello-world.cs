[endpoint: Curiosity.Endpoints.Path("hello-world")]
[endpoint: Curiosity.Endpoints.AccessMode("Unrestricted")]

return $"Hello World from Curiosity - today is {DateTimeOffset.UtcNow:u}";
