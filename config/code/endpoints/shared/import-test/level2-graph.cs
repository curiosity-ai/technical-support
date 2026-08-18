[endpoint: Curiosity.Endpoints.Path("shared/import-test/level2-graph")]
[endpoint: Curiosity.Endpoints.AccessMode("AdminOnly")]

//ImportEndpoint("shared/import-test/level1-core")

// Level 2b - the diamond's second arm. It shares level1-core with level2-text, so a consumer that
// pulls both must still see level1-core emitted exactly once.
//
// Deliberately graph-free despite the name: the search-index materialize scope has no `Graph`, and
// keeping the whole shared layer free of it lets the same chain be imported by every consumer kind.

var importTestLevel2Graph = importTestLevel1 + " > level2-graph";

public static class ImportTestGraphShape
{
    public const string MARKER = "level2-graph";

    public static string Describe(string nodeType, int count) => nodeType + ":" + count.ToString();

    public static ImportTestLayer Layer() => ImportTestCore.Layer(MARKER, "graph-shape helpers");
}
