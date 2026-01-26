[endpoint: Curiosity.Endpoints.Path("get-similar")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

class SimilarCasesRequest
{
    public string Query { get; set; }
}

var request = Body.FromJson<SimilarCasesRequest>();
return Q().StartAtSimilarText(request.Query, nodeTypes: [N.SupportCase.Type], count: 50).EmitWithScores();
