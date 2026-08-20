[shared: Curiosity.SharedCode.Name("SharedEndpoint.Shared.ImportTest.Level2Graph")]

// Extracted from code endpoint: shared/import-test/level2-graph

using SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core.Shared;

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

public static class Shared
{
    /// <summary>The endpoint's top-level statements, moved here and run against the calling scope.</summary>
    public static async global::System.Threading.Tasks.Task<object> Run(global::GraphDB.Schema.CodeExecutionScopes.ICodeExecutionScope scope)
    {
        var importTestLevel2Graph = importTestLevel1 + " > level2-graph";

        return null;
    }

}

