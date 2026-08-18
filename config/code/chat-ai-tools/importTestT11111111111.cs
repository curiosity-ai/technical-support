[tools: Curiosity.ChatAITools.UID("importTestT11111111111")]
[tools: Curiosity.ChatAITools.DisplayName("Import Test Probe")]
[tools: Curiosity.ChatAITools.Description("Diagnostics only. Reports whether the shared import-test endpoint library resolved inside the AI tool compilation scope. Never useful for answering a user's question - do not call it unless explicitly asked to run the import test.")]
[tools: Curiosity.ChatAITools.Icon("fi-rr-link")]
[tools: Curiosity.ChatAITools.AccessMode("AdminOnly")]

// Deliberately lowercase directive and mixed-case path: the regex carries RegexOptions.IgnoreCase
// and _CodeEndpoint.For() hashes the endpoint path case-insensitively, so both halves are covered.
//importendpoint("SHARED/Import-Test/Level3-Report")

// The imported bodies run above this line, so importTestLevel3 is already bound. It is a top-level
// variable of the script submission, though, and a class cannot see one - top-level locals become
// members of the submission and a nested type has no reference to it. Hand it over through a
// static field before returning the tool instance.
ImportTestProbeTool.ObservedChain = importTestLevel3;

// AI tools are one of only two consumer kinds whose compile passes a node UID, so this tool shows
// up under "Imported by" on shared/import-test/level3-report - but not on level1-core, which it
// only reaches transitively.
public class ImportTestProbeTool
{
    public static string ObservedChain;

    [Tool("Reports which import-test layers resolved inside the AI tool scope.")]
    public static Task<string> Probe(ToolScope scope)
    {
        // ToolScope has a Graph but no Logger, so only the graph half of the handshake applies.
        var scopeDetail = ImportTestScope.Describe(scope.Graph);

        return Task.FromResult(ImportTestReport.For("chat-ai-tool", ObservedChain, scopeDetail));
    }
}

return new ImportTestProbeTool();
