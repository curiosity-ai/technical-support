[endpoint: Curiosity.Endpoints.Path("get-similar")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]
[endpoint: Curiosity.Endpoints.ReadOnly]

var request = ParseBody<SimilarCasesRequest>();

return (await Q().StartAtSimilarTextAsync(request.Query, nodeTypes: [N.SupportCase.Type], count: 50)).EmitWithScores();

public class SimilarCasesRequest
{
    public string Query { get; set; }
}
