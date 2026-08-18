[endpoint: Curiosity.Endpoints.Path("shared/import-test/level2-text")]
[endpoint: Curiosity.Endpoints.AccessMode("AdminOnly")]

//ImportEndpoint("shared/import-test/level1-core")

// Level 2a. Its marker is built FROM level 1's top-level variable, which is what makes the
// dependency-first ordering observable: type declarations are hoisted by Roslyn scripting, so a
// class-only fixture would behave the same whatever order the bodies were spliced in. Only a
// top-level variable binds in textual order. Get it wrong and this is either CS0841 or a chain
// string that no longer matches - which is why the fixtures assert on the string, not on
// "it compiled".
//
// Construct coverage here: an instance class with a constructor, an auto-property and an override,
// deriving from a base declared in a DIFFERENT imported endpoint; a nested type; and a delegate
// instance typed by level 1.

var importTestLevel2Text = importTestLevel1 + " > level2-text";

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
