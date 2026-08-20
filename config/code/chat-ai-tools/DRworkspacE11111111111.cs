[tools: Curiosity.ChatAITools.UID("DRworkspacE11111111111")]
[tools: Curiosity.ChatAITools.DisplayName("Workspace Deep Research")]
[tools: Curiosity.ChatAITools.Description("Researches a question in depth against everything in this workspace: files, documents, emails, notes and the rest of the knowledge graph. Plans the question into steps, searches and reads across them, and answers with a written, sourced report.\n\nThis starts a full research run: it plans the question into steps, searches and reads across them, and answers with a written report. It takes minutes and the workspace runs a limited number at once, so use it for questions that genuinely need research, and ask a single, self-contained question — not for a lookup a single search answers.")]
[tools: Curiosity.ChatAITools.Icon("fi-rr-microscope")]
[tools: Curiosity.ChatAITools.AccessMode("AllUsers")]

// A deep researcher is an AI tool whose code returns an IDeepResearcher instead of a class
// with [Tool] methods. It does not do the research itself: it says which prompt drives each
// phase of the research loop, which tools the loop may call while gathering evidence, and
// how much a single run may spend. Calling it from a chat starts a run on the workspace's
// shared research queue, which reports its plan back into the conversation as it works.
//
// [ToolSystemPrompt] is appended to the system prompt of every chat this tool is enabled
// in — what has to be true for the whole conversation, rather than for one call. Keep it
// short: it is paid for on every turn, whether or not a research is ever started.
[ToolSystemPrompt("""
                  A deep research run takes minutes and the workspace runs only a few at once. Start one
                  when the question needs several searches weighed against each other; answer from a
                  single search when a single search would do, and say what you are about to do before
                  starting a run.
                  The research answers with a written, sourced report. Give the user that report — keep
                  its structure and its sources rather than summarising it into a paragraph — and do not
                  add claims of your own to it. While a research is running the conversation accepts no
                  further messages, so do not ask the user anything you need the answer to first.
                  """)]
public class WorkspaceDeepResearcher : IDeepResearcher
{
    // What the assistant reads when it decides whether a question deserves a research run.
    public string Description => "Researches a question in depth against everything in this workspace: files, documents, emails, notes and the rest of the knowledge graph. Plans the question into steps, searches and reads across them, and answers with a written, sourced report.";

    // The six phases of the loop. These point at the built-in templates, which are editable
    // under Prompt Templates in the Build interface — change one there and every researcher
    // using it changes with it. To tune this researcher alone, copy a template and point at
    // your copy instead.
    public PromptTemplateUID PlanPrompt => new PromptTemplateUID(BuiltInUIDs.PromptTemplate_DeepResearchPlan);
    public PromptTemplateUID QueriesPrompt => new PromptTemplateUID(BuiltInUIDs.PromptTemplate_DeepResearchQueries);
    public PromptTemplateUID SearchPrompt => new PromptTemplateUID(BuiltInUIDs.PromptTemplate_DeepResearchSearch);
    public PromptTemplateUID FindingsPrompt => new PromptTemplateUID(BuiltInUIDs.PromptTemplate_DeepResearchFindings);
    public PromptTemplateUID DirectionsPrompt => new PromptTemplateUID(BuiltInUIDs.PromptTemplate_DeepResearchDirections);
    public PromptTemplateUID ReportPrompt => new PromptTemplateUID(BuiltInUIDs.PromptTemplate_DeepResearchReport);

    // What the gather phase may call. `search` finds things in the workspace and `consult`
    // reads one of them in full, which is the pair that makes a finding worth citing: the
    // search says where to look, the read is what it is sourced from. The caller's own access
    // is enforced on every call, so a run can never read what the user could not.
    public ToolUID[] ResearchTools => new[]
    {
        new ToolUID(BuiltInUIDs.Tool_LLMSearch),
        new ToolUID(BuiltInUIDs.Tool_Consult),
    };

    // Budgets for a single run. Zero means "use the workspace default"; the workspace's own
    // ceiling (Deep Research, under AI in the admin settings) always wins, so raising these
    // beyond it has no effect.
    public int MaxIterations => 12;   // directions explored across the whole run
    public int MaxDurationSeconds => 600;  // wall clock for the whole run
    public int QueriesPerDirection => 3;    // searches generated per direction
}

return new WorkspaceDeepResearcher();

