[shared: Curiosity.SharedCode.Name("SharedEndpoint.Shared.ImportTest.Level2Text")]

// Extracted from code endpoint: shared/import-test/level2-text

using SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core.Shared;

public sealed class ImportTestTextLayer : ImportTestLayerBase
{
    public ImportTestTextLayer(string source) : base("level2-text")
    {
        Source = source;
    }

    public string Source { get; }

    public override string Detail => "text helpers via " + Source;

    public override string Describe() => base.Describe() + "[instance]";
}

public static class ImportTestText
{
    public const string MARKER = "level2-text";

    public static readonly ImportTestFormatter Shout = value => (value ?? "").ToUpperInvariant();

    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        return System.Text.RegularExpressions.Regex.Replace(value.Trim(), "\\s+", " ");
    }

    public static ImportTestLayer Layer() => ImportTestCore.Layer(MARKER, "text helpers");

    public sealed class Nested
    {
        public string Describe() => MARKER + ".Nested";
    }
}

public static class Shared
{
    /// <summary>The endpoint's top-level statements, moved here and run against the calling scope.</summary>
    public static async global::System.Threading.Tasks.Task<object> Run(global::GraphDB.Schema.CodeExecutionScopes.ICodeExecutionScope scope)
    {
        var importTestLevel2Text = importTestLevel1 + " > level2-text";

        return null;
    }

}

