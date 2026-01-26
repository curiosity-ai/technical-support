[endpoint: Curiosity.Endpoints.Path("get-user-email")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

if (CurrentUser.IsNull())
    return "Endpoint was not called by a user";
Logger.LogInformation("The endpoint was called by {0}", CurrentUser);
return Graph.Get(CurrentUser).GetString(nameof(_User.Email));
