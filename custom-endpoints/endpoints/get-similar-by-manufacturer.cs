[endpoint: Curiosity.Endpoints.Path("get-similar-by-manufacturer")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

class SimilarCasesRequest
{
    public string Query { get; set; }
    public string Manufacturer { get; set; }
}

var request = Body.FromJson<SimilarCasesRequest>();
return Q().StartAtSimilarText(request.Query, nodeTypes: [N.SupportCase.Type], count: 500).IsRelatedTo(Node.GetUID(N.Manufacturer.Type, request.Manufacturer)).EmitWithScores();
