[tools: Curiosity.ChatAITools.UID("AiTooLsEarCh1111111111")]
[tools: Curiosity.ChatAITools.DisplayName("Search Files")]
[tools: Curiosity.ChatAITools.Description("Search for files, documents, slides, email, notes, sheets, etc. using keyword and similarity search on query, then returns up to 3 snippets per file that are most similar to the query. IMPORTANT: Always cite snippets using [snippetId] format (e.g., [1], [2]) and use direct quotes from the snippets when referencing information.")]
[tools: Curiosity.ChatAITools.Icon("fi-rr-search")]
[tools: Curiosity.ChatAITools.AccessMode("AllUsers")]

public class SearchTool
{
    private const int CHUNKS_PER_FILE = 3;

    [Tool("Search for files, documents, slides, email, notes, sheets, etc. using keyword and similarity search on query, then returns up to 3 snippets per file that are most similar to the query. IMPORTANT: Always cite snippets using [snippetId] format (e.g., [1], [2]) and use direct quotes from the snippets when referencing information.")]
    public static async Task<string> SearchFiles(ToolScope scope,
          [Parameter("Search query to find files (keyword + similarity) and extract relevant snippets (similarity only)", required: true)] string query,
          [Parameter("Maximum total results to return (default: 10)", required: false)] int limit = 10,
          [Parameter("Number of results to skip for pagination (default: 0)", required: false)] int skip = -1)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return "[]";
        }
        else
        {
            var searchReq = SearchRequest.For(query);

            searchReq.SortMode = SortModeEnum.Relevance;
            searchReq.UserUID = scope.CurrentUser;
            searchReq.BeforeTypesFacet = new HashSet<string>
            {
                _FileEntry.Type
            };

            var results = new List<object>();

            var search = await scope.Graph.CreateSearchAsUserAsync(searchReq, searchReq.UserUID, scope.CancellationToken);

            foreach (var node in search.Skip(skip > 0 ? skip : 0).Take(limit > 0 ? limit : 10).AsEnumerable())
            {
                foreach (var chunk in (await scope.ChatAI.GetSimilarChunksAsync(node.UID, query)).OrderByDescending(e => e.Score).Take(CHUNKS_PER_FILE))
                {
                    //You can add snippets to the tool calling response so they show as results in the interface
                    var snippetID = scope.AddSnippet(uid: node.UID, text: chunk.Text, page: chunk.Page ?? 1, endPage: chunk.EndPage ?? 1);

                    results.Add(new
                    {
                        name = node.GetString(nameof(_FileEntry.OriginalName)),
                        chunkContent = chunk.Text,
                        snippetId = snippetID
                    });
                }
            }

            scope.SetToolCallDisplayName($"Search for '{query}'");

            return results.ToJson();
        }
    }
}

return new SearchTool();

