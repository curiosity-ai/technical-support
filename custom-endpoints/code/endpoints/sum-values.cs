[endpoint: Curiosity.Endpoints.Path("sum-values")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]
[endpoint: Curiosity.Endpoints.ReadOnly]

var request = ParseBody<SumRequest>();

if (request is null) return BadRequest("Expected a JSON body with A and B");

return Ok(new SumResponse { Value = request.A + request.B });

public class SumRequest
{
    public int A { get; set; }
    public int B { get; set; }
}

public class SumResponse
{
    public int Value { get; set; }
}
