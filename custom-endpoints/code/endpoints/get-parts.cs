[endpoint: Curiosity.Endpoints.Path("get-parts")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]
[endpoint: Curiosity.Endpoints.ReadOnly]

const int pageSize = 50;

var request = ParseBody<PartsRequest>();

return Q().StartAt(N.Part.Type).Skip(request.Page * pageSize).Take(pageSize).Emit();

public class PartsRequest
{
    public int Page { get; set; }
}
