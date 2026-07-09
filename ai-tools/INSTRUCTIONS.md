# Curiosity Workspace // AI Tools

AI Tools are C# classes the chat LLM (and your own endpoints) can invoke. A tool is a class with one or
more methods marked `[Tool("…")]`; the first parameter is always a `ToolScope` (which exposes `Graph`,
`ChatAI`, `CurrentUser`, `CancellationToken`, …), and each remaining parameter is annotated with
`[Parameter("…", required: …)]`. The script ends by returning an instance of the tool class. The LLM only
ever sees the tool's description and parameter signatures — never its code.

The support-question pipeline (see [`/custom-endpoints/INSTRUCTIONS.md`](/custom-endpoints/INSTRUCTIONS.md))
is backed by two such tools:

- **Extract Support Questions** (`ExtractQuestions`) — reads a support conversation transcript and returns
  `{"questions":[…],"topic":"…"}` containing the distinct questions the support agent asked the customer.
  UID `ExtracTQ11111111111111`.
- **Sanitize Support Questions** (`SanitizeQuestions`) — takes a JSON array of questions plus a topic and
  returns `{"sanitizedQuestions":[…],"sanitizedTopic":"…"}` with PII (names, emails, phone numbers,
  addresses, account/order/serial numbers) replaced by neutral placeholders. UID `SaniTizeQ1111111111111`.

Both tools call the workspace's configured LLM through `scope.ChatAI.GetCompletionAsync(...)`, so make sure
an AI Assistant / chat provider is configured (Manage → AI) before using them.

## Where the tools live

All of this workspace's ChatAI tools — including the two above and the read/act support tools (**Find
Similar Support Cases**, **Support Graph Lookup**, **Resolve Support Case**) — live in the repository's main
[`config/`](/config) workspace-definitions bundle under
[`config/code/chat-ai-tools/`](/config/code/chat-ai-tools). Each file is named after the tool's UID and is
the tool's exported-code form: a header of `[tools: Curiosity.ChatAITools.*]` attributes (UID, DisplayName,
Description, Icon, AccessMode) followed by a blank line and the C# body. The UID in the header is the tool's
stable identity — re-importing the same file updates the existing tool in place. The endpoints in
[`config/code/endpoints/`](/config/code/endpoints) reference these exact UIDs.

## Importing

The tools are imported as part of the whole `config/` bundle — there is no separate AI-tool import step.
The [`workspace-demo`](/README.md) CLI (`run-demo`) imports the bundle with
`curiosity-cli import-workspace-definitions`, so a normal deploy brings every AI tool and its endpoints up
with the rest of the workspace:

```bash
curiosity-cli import-workspace-definitions \
  --server http://localhost:8080/ \
  --token  <admin-library-token> \
  --import-path config \
  --zip
```

Or from the workspace UI: **Manage → Settings → Workspace Definitions → Compare & Import Workspace
Definitions**, and upload a zip of the `config/` directory. The import upserts (existing definitions are
updated, new ones created) and never deletes definitions that are only present in the target, so it is safe
to re-run. After importing, the tools appear under **Manage → AI Tools**.

## Editing a tool

Edit the C# body below the attribute header (in the file under
[`config/code/chat-ai-tools/`](/config/code/chat-ai-tools)), keep the header attribute values on a single
line (they are parsed literally), and re-import. To create a brand-new tool, mint a fresh round-tripping
`UID128` first — in the workspace repo, `dotnet run --project Other/FindHardcodedUID -- <prefix>` prints
valid candidates.
