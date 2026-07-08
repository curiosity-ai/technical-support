# Curiosity Workspace // AI Tools

AI Tools are C# classes the chat LLM (and your own endpoints) can invoke. A tool is a class with one or
more methods marked `[Tool("…")]`; the first parameter is always a `ToolScope` (which exposes `Graph`,
`ChatAI`, `CurrentUser`, `CancellationToken`, …), and each remaining parameter is annotated with
`[Parameter("…", required: …)]`. The script ends by returning an instance of the tool class. The LLM only
ever sees the tool's description and parameter signatures — never its code.

This folder ships two tools that back the support-question pipeline (see
[`/custom-endpoints/INSTRUCTIONS.md`](/custom-endpoints/INSTRUCTIONS.md)):

- **Extract Support Questions** (`ExtractQuestions`) — reads a support conversation transcript and returns
  `{"questions":[…],"topic":"…"}` containing the distinct questions the support agent asked the customer.
  UID `ExtracTQ11111111111111`.
- **Sanitize Support Questions** (`SanitizeQuestions`) — takes a JSON array of questions plus a topic and
  returns `{"sanitizedQuestions":[…],"sanitizedTopic":"…"}` with PII (names, emails, phone numbers,
  addresses, account/order/serial numbers) replaced by neutral placeholders. UID `SaniTizeQ1111111111111`.

Both tools call the workspace's configured LLM through `scope.ChatAI.GetCompletionAsync(...)`, so make sure
an AI Assistant / chat provider is configured (Manage → AI) before using them.

## Layout

The tools are laid out as a **workspace definitions** bundle so the folder can be imported as-is:

```
ai-tools/
└── definitions/
    └── code/
        └── chat-ai-tools/
            ├── extract-support-questions.ExtracTQ11111111111111.cs
            └── sanitize-support-questions.SaniTizeQ1111111111111.cs
```

Each file is the tool's exported-code form: a header of `[tools: Curiosity.ChatAITools.*]` attributes
(UID, DisplayName, Description, Icon, AccessMode) followed by a blank line and the C# body. The UID in the
header is the tool's stable identity — re-importing the same file updates the existing tool in place. The
endpoints in `/custom-endpoints` reference these exact UIDs.

## Importing

Both tools (and the `extract-questions` / `sanitize-questions` / `suggest-questions` endpoints that call
them) also ship inside the repository's main [`config/`](/config) workspace-definitions bundle — the same
bundle the [`workspace-demo`](/README.md) CLI imports with `curiosity-cli import-workspace-definitions`. So
a normal `run-demo` deploy brings these AI tools and their endpoints up with the rest of the workspace; no
separate step is needed.

The standalone bundle below is the same content laid out on its own, kept as a focused example of how to
author and import an AI tool by itself. Import it with the Curiosity CLI (zips the directory before
uploading):

```bash
curiosity-cli import-workspace-definitions \
  --server http://localhost:8080/ \
  --token  <admin-library-token> \
  --import-path ai-tools/definitions \
  --zip
```

Or from the workspace UI: **Manage → Settings → Workspace Definitions → Compare & Import Workspace
Definitions**, and upload a zip of the `definitions/` directory. After importing, both tools appear under
**Manage → AI Tools**.

The import upserts (existing definitions are updated, new ones created) and never deletes definitions that
are only present in the target, so it is safe to re-run.

## Editing a tool

Edit the C# body below the attribute header, keep the header attribute values on a single line (they are
parsed literally), and re-import. To create a brand-new tool, mint a fresh round-tripping `UID128` first —
in the workspace repo, `dotnet run --project Other/FindHardcodedUID -- <prefix>` prints valid candidates.
