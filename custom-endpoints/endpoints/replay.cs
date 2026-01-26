[endpoint: Curiosity.Endpoints.Path("replay")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

return $"Hello World! You sent: {(Body ?? "Nothing")}";
