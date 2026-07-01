[tools: Curiosity.ChatAITools.UID("SheLLscriptTooL1111111")]
[tools: Curiosity.ChatAITools.DisplayName("Propose Shell Script")]
[tools: Curiosity.ChatAITools.Description("Hand the admin a one-off C# Shell script to review and run (e.g. a data backfill, a quick count, an export). Use this for code that should be run once by hand — NOT for configuration changes (use the propose.* tools for those), and NOT for code that should be exposed as an endpoint or AI tool. The script runs in the Shell code scope: it has Graph, Console.WriteLine, Q()/Query(), the typed N./E. helpers and CsvHelper already imported. The admin reviews the script in a Shell modal and runs it themselves — this tool never executes anything. Returns { ok, title, summary, script }.")]
[tools: Curiosity.ChatAITools.Icon("fi-rr-terminal")]
[tools: Curiosity.ChatAITools.AccessMode("AdminOnly")]

// This tool is implemented in code, not as a script.
// Its implementation lives in BuiltInSkills.SkillsTool (or sibling
// classes registered in InCodeBuiltInTools). Editing this body has
// no effect — clone the tool and provide your own [Tool] methods
// if you need a custom version.
return null;

