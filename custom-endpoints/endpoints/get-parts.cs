[endpoint: Curiosity.Endpoints.Path("get-parts")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

class PartsRequest
{
    public int Page { get; set; }
}

const int pageSize = 50;
var request = Body.FromJson<PartsRequest>();
return Q().StartAt(N.Part.Type).Skip(request.Page * pageSize).Take(pageSize).Emit();
