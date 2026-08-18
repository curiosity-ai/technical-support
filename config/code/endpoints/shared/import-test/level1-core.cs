[endpoint: Curiosity.Endpoints.Path("shared/import-test/level1-core")]
[endpoint: Curiosity.Endpoints.AccessMode("AdminOnly")]

// Level 1 of the //ImportEndpoint fixture chain - the leaf every other level depends on.
//
// This endpoint is never called. It exists only to be imported, so it follows the two rules that
// make an endpoint safe to import (see config/code/IMPORT-TEST.md):
//   * declaration-only - imported bodies are spliced in ABOVE the consumer's own code, so a
//     top-level `return` here would short-circuit every consumer;
//   * no `using` directives - only the CONSUMER's leading using block is hoisted, so everything
//     below is fully qualified.

var importTestLevel1 = "level1-core";

public sealed record ImportTestLayer(string Name, string Detail);

public static class ImportTestCore
{
    public const string MARKER = "level1-core";

    public static ImportTestLayer Layer(string name, string detail) => new ImportTestLayer(name, detail);

    public static string Quote(string value) => "\"" + (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
}
