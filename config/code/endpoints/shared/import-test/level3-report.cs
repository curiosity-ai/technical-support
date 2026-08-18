[endpoint: Curiosity.Endpoints.Path("shared/import-test/level3-report")]
[endpoint: Curiosity.Endpoints.AccessMode("AdminOnly")]

//ImportEndpoint("shared/import-test/level2-text")
//ImportEndpoint("shared/import-test/level2-graph")

// Level 3 - closes the diamond and builds the report every consumer returns or logs.

var importTestLevel3 = importTestLevel2Text + " + " + importTestLevel2Graph + " > level3-report";

public static class ImportTestReport
{
    public const string MARKER = "level3-report";

    public static string For(string consumerKind, string chain)
    {
        var layers = new System.Collections.Generic.List<ImportTestLayer>
        {
            ImportTestCore.Layer(ImportTestCore.MARKER, "root"),
            ImportTestText.Layer(),
            ImportTestGraphShape.Layer(),
            ImportTestCore.Layer(MARKER, "report builder")
        };

        var sb = new System.Text.StringBuilder();

        sb.Append("{\"consumer\":").Append(ImportTestCore.Quote(consumerKind));
        sb.Append(",\"chain\":").Append(ImportTestCore.Quote(ImportTestText.Normalize(chain)));
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
}
