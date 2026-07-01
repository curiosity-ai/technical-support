[endpoint: Curiosity.Endpoints.Path("sum-values")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

class SumRequest
{
    public int A { get; set; }
    public int B { get; set; }
}

class SumResponse
{
    public int Value { get; set; }
    public string Error { get; set; }
}

try
{
    var request = Body.FromJson<SumRequest>();
    return new SumResponse()
    {
        Value = request.A + request.B
    };
}
catch (Exception E)
{
    return new SumResponse()
    {
        Error = E.Message
    };
}
