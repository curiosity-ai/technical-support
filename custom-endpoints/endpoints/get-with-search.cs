[endpoint: Curiosity.Endpoints.Path("get-with-search")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

class SimpleSearchRequest
{
    public string Query { get; set; }
    public int Page { get; set; }
}

const int pageSize = 50;
var request = Body.FromJson<SimpleSearchRequest>();
return Q().StartSearch(N.SupportCase.Type, N.SupportCase.Content, SearchExpression.For(SearchToken.StartsWith(request.Query), request.Query)).Skip(request.Page * pageSize).Take(pageSize).Emit();
