[endpoint: Curiosity.Endpoints.Path("shared/import-test/level2-graph")]
[endpoint: Curiosity.Endpoints.AccessMode("AdminOnly")]

//ImportEndpoint("shared/import-test/level1-core")

// Level 2b - the diamond's second arm. It shares level1-core with level2-text, so a consumer that
// pulls both must still see level1-core emitted exactly once.
//
// Construct coverage here: a generic class, a static constructor, mutable static state, and an
// async instance method. Deliberately graph-free despite the name - see ImportTestScope in
// level3-report for how a consumer hands its own scope in.

var importTestLevel2Graph = importTestLevel1 + " > level2-graph";

public sealed class ImportTestBox<T>
{
    private readonly T _value;

    public ImportTestBox(T value)
    {
        _value = value;
    }

    public T Get() => _value;

    public string Describe() => typeof(T).Name + ":" + _value;

    public System.Threading.Tasks.Task<T> GetAsync() => System.Threading.Tasks.Task.FromResult(_value);
}

public static class ImportTestGraphShape
{
    public const string MARKER = "level2-graph";

    // A static constructor in imported code runs once per compiled submission, so a consumer can
    // read Signature without initialising anything itself.
    static ImportTestGraphShape()
    {
        Signature = MARKER + "/ready";
    }

    public static readonly string Signature;

    // Mutable static state shared between the imported body and the consumer.
    public static ImportTestCounter Observations = new ImportTestCounter(0);

    public static string Describe(string nodeType, int count) => nodeType + ":" + count.ToString();

    public static ImportTestLayer Layer() => ImportTestCore.Layer(MARKER, "graph-shape helpers");
}

