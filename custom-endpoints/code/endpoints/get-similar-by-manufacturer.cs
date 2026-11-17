[endpoint: Curiosity.Endpoints.Path("get-similar-by-manufacturer")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]
[endpoint: Curiosity.Endpoints.ReadOnly]

var request = ParseBody<SimilarCasesRequest>();

return (await Q().StartAtSimilarTextAsync(request.Query, nodeTypes: [N.SupportCase.Type], count: 500))
          .IsRelatedTo(Node.GetUID(N.Manufacturer.Type, request.Manufacturer))
          .EmitWithScores();

public class SimilarCasesRequest
{
    public string Query        { get; set; }
    public string Manufacturer { get; set; }
}
