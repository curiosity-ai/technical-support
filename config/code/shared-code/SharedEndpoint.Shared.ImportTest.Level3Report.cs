[shared: Curiosity.SharedCode.Name("SharedEndpoint.Shared.ImportTest.Level3Report")]

// Extracted from code endpoint: shared/import-test/level3-report

using SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level1Core.Shared;
using SharedCode.SharedEndpoint.Shared.ImportTest.Level2Text;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level2Text.Shared;
using SharedCode.SharedEndpoint.Shared.ImportTest.Level2Graph;
using static SharedCode.SharedEndpoint.Shared.ImportTest.Level2Graph.Shared;

public static class ImportTestScope
{
    public static string Describe(Mosaik.GraphDB.Safe.Graph graph) => graph is null ? "graph:none" : "graph:writable";

    public static string Describe(Mosaik.GraphDB.Safe.ReadOnlyGraph graph) => graph is null ? "graph:none" : "graph:read-only";

    public static void Trace(Microsoft.Extensions.Logging.ILogger logger, string consumerKind, string chain)
    {
        if (logger is null) return;
        logger.LogInformation("import-test {0}: {1}", consumerKind, ImportTestReport.For(consumerKind, chain));
    }
}

public static class ImportTestReport
{
    public const string MARKER = "level3-report";

    public static string For(string consumerKind, string chain) => For(consumerKind, chain, null);

    public static string For(string consumerKind, string chain, string scopeDetail)
    {
        // Every element arrives through the interface, but the four instances behind it are a
        // record (level1), an instance class deriving from an abstract base in another import
        // (level2-text), and two more records - so this one loop covers the whole type zoo.
        var layers = new System.Collections.Generic.List<IImportTestLayer>
        {
            ImportTestCore.Layer(ImportTestCore.MARKER, "root"),
            new ImportTestTextLayer("level3-report"),
            ImportTestGraphShape.Layer(),
            ImportTestCore.Layer(MARKER, "report builder")
        };

        var sb = new System.Text.StringBuilder();

        sb.Append("{\"consumer\":").Append(ImportTestCore.Quote(consumerKind));
        sb.Append(",\"chain\":").Append(ImportTestCore.Quote(ImportTestText.Normalize(chain)));
        sb.Append(",\"scope\":").Append(ImportTestCore.Quote(scopeDetail ?? "not-reported"));
        sb.Append(",\"signature\":").Append(ImportTestCore.Quote(ImportTestGraphShape.Signature));
        sb.Append(",\"layers\":[");

        for (int i = 0; i < layers.Count; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append("{\"name\":").Append(ImportTestCore.Quote(layers[i].Name))
              .Append(",\"detail\":").Append(ImportTestCore.Quote(layers[i].Detail)).Append('}');
        }

        sb.Append("],\"ok\":").Append(layers.Count == 4 ? "true" : "false").Append('}');

        return sb.ToString();
    }

    public static string Describe<T>(ImportTestBox<T> box) => MARKER + "<" + box.Describe() + ">";
}

public static class Shared
{
    public static
    // A top-level local function in an imported body stays callable from the consumer's own top-level
    // code - and, unlike a class member, it could legally touch a scope global. This one does not, so
    // that it works in all nine scopes.
    string ImportTestChainTail(string chain)
    {
        var parts = (chain ?? "").Split('>');
        return parts.Length == 0 ? "" : parts[parts.Length - 1].Trim();
    }

    /// <summary>The endpoint's top-level statements, moved here and run against the calling scope.</summary>
    public static async global::System.Threading.Tasks.Task<object> Run(global::GraphDB.Schema.CodeExecutionScopes.ICodeExecutionScope scope)
    {
        var importTestLevel3 = importTestLevel2Text + " + " + importTestLevel2Graph + " > level3-report";

        return null;
    }

}

