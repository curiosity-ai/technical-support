[endpoint: Curiosity.Endpoints.Path("shared/import-test/level1-core")]
[endpoint: Curiosity.Endpoints.AccessMode("AdminOnly")]

// Level 1 of the //ImportEndpoint fixture chain - the leaf every other level depends on, and the
// file that declares the type vocabulary the rest of the suite builds on.
//
// This endpoint is never called. It exists only to be imported, so it follows the three rules that
// make an endpoint safe to import (see config/code/IMPORT-TEST.md):
//   * declaration-only - imported bodies are spliced in ABOVE the consumer's own code, so a
//     top-level `return` here would short-circuit every consumer;
//   * no `using` directives - only the CONSUMER's leading using block is hoisted, so everything
//     below is fully qualified;
//   * nothing here touches a scope global. A member of a class in imported code cannot reach one
//     at all (CS0120), and a top-level statement that reaches one binds this file to that single
//     scope (CS0103 everywhere else). Consumers hand their scope in as a parameter instead.
//
// There is deliberately no `namespace` here: script submissions reject one outright with
// CS7021 "Cannot declare namespace in script code", so every imported declaration lands in one
// flat submission and names must be unique across the whole suite.

var importTestLevel1 = "level1-core";

public enum ImportTestKind
{
    Endpoint,
    Tool,
    Index,
    Task
}

public interface IImportTestLayer
{
    string Name   { get; }
    string Detail { get; }
}

// A positional record satisfies the interface through its generated properties.
public sealed record ImportTestLayer(string Name, string Detail) : IImportTestLayer;

// Derived from in level2-text, which proves an inheritance edge can cross the import boundary.
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
