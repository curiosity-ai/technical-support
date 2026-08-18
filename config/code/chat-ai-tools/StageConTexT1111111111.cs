[tools: Curiosity.ChatAITools.UID("StageConTexT1111111111")]
[tools: Curiosity.ChatAITools.DisplayName("Stage Context")]
[tools: Curiosity.ChatAITools.Description("Lists the files an admin has uploaded to the private Stage area (files awaiting review before they are promoted into the workspace). Returns for each file its uid, name, contentType, size in bytes and whether it is a CSV-style file. Use the 'read-staged-file' tool with a file uid to read its content or, for CSV files, its column metadata. Call this first when the user refers to 'the staged files', 'the stage area' or a file they just uploaded.")]
[tools: Curiosity.ChatAITools.Icon("fi-rr-layers")]
[tools: Curiosity.ChatAITools.AccessMode("AdminOnly")]

// This tool is implemented in code, not as a script.
// Its implementation lives in BuiltInSkills.SkillsTool (or sibling
// classes registered in InCodeBuiltInTools). Editing this body has
// no effect — clone the tool and provide your own [Tool] methods
// if you need a custom version.
return null;

