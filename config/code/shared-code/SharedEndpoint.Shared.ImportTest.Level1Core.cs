[shared: Curiosity.SharedCode.Name("SharedEndpoint.Shared.ImportTest.Level1Core")]

// Extracted from code endpoint: shared/import-test/level1-core

public enum ImportTestKind
{
    Endpoint,
    Tool,
    Index,
    Task
}

public interface IImportTestLayer
{
    string Name { get; }
    string Detail { get; }
}

public sealed record ImportTestLayer(string Name, string Detail) : IImportTestLayer;

public abstract class ImportTestLayerBase : IImportTestLayer
{
    protected ImportTestLayerBase(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public abstract string Detail { get; }

    public virtual string Describe() => Name + "(" + Detail + ")";
}

public readonly struct ImportTestCounter
{
    public ImportTestCounter(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public ImportTestCounter Next() => new ImportTestCounter(Value + 1);
}

public delegate string ImportTestFormatter(string value);

public static class ImportTestCore
{
    public const string MARKER = "level1-core";

    public static ImportTestLayer Layer(string name, string detail) => new ImportTestLayer(name, detail);

    public static string Quote(string value) => "\"" + (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
}

public static class Shared
{
    /// <summary>The endpoint's top-level statements, moved here and run against the calling scope.</summary>
    public static async global::System.Threading.Tasks.Task<object> Run(global::GraphDB.Schema.CodeExecutionScopes.ICodeExecutionScope scope)
    {
        var importTestLevel1 = "level1-core";

        return null;
    }

}

