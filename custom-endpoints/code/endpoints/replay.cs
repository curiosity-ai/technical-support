[endpoint: Curiosity.Endpoints.Path("replay")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]
[endpoint: Curiosity.Endpoints.ReadOnly]

// Body is the request stream: read it as text to answer it back unchanged.
return ReadBodyAsText();
