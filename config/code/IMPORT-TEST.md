# `//ImportEndpoint` test fixtures

This folder carries a self-contained test case for the workspace's `//ImportEndpoint("path")`
feature, covering both the resolution rules (transitive chains, diamonds, dedup, ordering) and the
language surface (which C# constructs survive the splice, and how imported code reaches the
execution scope). Everything in it is a **fixture**: it exists to prove the feature works, not to do
any support work. All of it lives under the `import-test` / `shared/import-test` endpoint paths and can
be deleted in one pass without touching the rest of the demo.

## What `//ImportEndpoint` does

It textually pulls the body of a code-endpoint node into another piece of custom C# before Roslyn
compiles it — the workspace's only code-reuse mechanism. It is **not** endpoint-only: eight kinds of
custom code in a workspace-definitions bundle support it, plus ad-hoc shell code and queries.

Syntax is a plain C# line comment with **no whitespace tolerance**:

```csharp
//ImportEndpoint("shared/import-test/level1-core")
```

`// ImportEndpoint("x")` (with a space) does not match and is silently just a comment. The directive
itself is case-insensitive, and so is the endpoint path.

## The shared layer

Four declaration-only endpoints under `endpoints/shared/import-test/` form the dependency graph:

```
              level1-core
             /           \
    level2-text        level2-graph
             \           /
              level3-report
```

`import-test/verify` then imports `level3-report` **and** `level1-core` **and** `level2-text`, so
the same closure is named three times and must still be emitted once.

Each level's marker is built from the previous level's top-level variable:

| endpoint | top-level variable |
|---|---|
| `level1-core` | `importTestLevel1 = "level1-core"` |
| `level2-text` | `importTestLevel2Text = importTestLevel1 + " > level2-text"` |
| `level2-graph` | `importTestLevel2Graph = importTestLevel1 + " > level2-graph"` |
| `level3-report` | `importTestLevel3 = importTestLevel2Text + " + " + importTestLevel2Graph + " > level3-report"` |

That chaining is deliberate, and it is the only thing in a script submission that can prove the
order. Type declarations — including inheritance and static field initialisers — are hoisted, so a
class-only fixture would compile and run identically no matter what order the imports were spliced
in. Top-level variables are the exception: they bind in textual order. Get the order wrong and you
either get `CS0841` (cannot use a local before it is declared) or a chain string that does not match
the expected value. Either way the fixture catches it, which is why the assertion is on the runtime
string and not only on "it compiled".

## What survives the import

Imported bodies are concatenated into a single Roslyn **script** submission, which is not the same
language surface as a normal `.cs` file. This table is the measured result of compiling each
construct inside an imported endpoint and using it from a consumer:

| Construct in the imported endpoint | Result |
|---|---|
| Top-level variable (`var x = ...;`) | works — and it is the only construct that proves ordering |
| Top-level local function | works, and stays callable from the consumer |
| `static class` with static methods, `const`, static fields, static constructor | works |
| Instance `class` with constructor, properties, instance methods | works |
| `abstract` base + derived class, including deriving across two imported endpoints | works |
| `interface` + implementation | works |
| Generic class and generic static method | works |
| `enum`, `struct`, `delegate`, nested type | works |
| Positional `record` | works |
| `async` methods returning `Task<T>` | works |
| Overload resolution across the import boundary | works |
| **`namespace` (block or file-scoped)** | **`CS7021: Cannot declare namespace in script code`** |
| **Extension method** | **`CS1109: Extension methods must be defined in a top level static class`** — a top-level class in a script is *nested* inside the submission |
| **Class member reading a scope global** (`Graph`, `Logger`, `CurrentUser`) | **`CS0120: An object reference is required...`** |

The last two rows are the same underlying fact: what looks like a top-level type in a script is
compiled as a nested type of the submission class. It cannot be a host for extension methods, and it
has no reference to the submission instance that the scope globals hang off.

Because there are no namespaces, **every name in the suite is global**. Two imported endpoints
declaring the same type is `CS0101`.

## Reaching the execution scope

This is the part most likely to bite, and the fixtures are built around it. There are three
distinct behaviours:

1. **A class member cannot touch a scope global at all.** `public static string Who() => CurrentUser.ToString();`
   inside an imported class is `CS0120` in every scope, including the endpoint scope. The name
   resolves — it just has no instance behind it.
2. **A top-level statement in an imported body *can* touch a scope global** — but doing so welds
   that endpoint to one scope. `var u = CurrentUser;` compiles fine when imported into an endpoint,
   and is `CS0103: The name 'CurrentUser' does not exist in the current context` the moment the same
   endpoint is imported into a code index, an AI tool or a migration task.
3. **Passing the scope in as a parameter works everywhere.** This is what the shared layer does:

   ```csharp
   public static class ImportTestScope
   {
       public static string Describe(Mosaik.GraphDB.Safe.Graph graph)         => "graph:writable";
       public static string Describe(Mosaik.GraphDB.Safe.ReadOnlyGraph graph) => "graph:read-only";
       public static void   Trace(Microsoft.Extensions.Logging.ILogger logger, string kind, string chain) { ... }
   }
   ```

   Each consumer hands over whatever its own scope exposes. The overload pair matters: the writable
   endpoint scope's `Graph` is a `Safe.Graph`, the read-only endpoint scope's is a
   `Safe.ReadOnlyGraph`, `ToolScope` has a `Graph` but **no** `Logger`, and the search-index
   materialize scope has a `Logger` but **no** `Graph` at all. That last scope is the reason the
   shared layer must stay global-free — one mention of `Graph` at top level and it stops being
   importable there.

## The consumers

One fixture per kind that supports the feature:

| Kind | File |
|---|---|
| Code endpoint | `endpoints/import-test/verify.cs` |
| Code endpoint (read-only) | `endpoints/import-test/verify-readonly.cs` |
| AI tool | `chat-ai-tools/importTestT11111111111.cs` |
| Code scheduled task | `scheduled-tasks/import-test-scheduled-importTestSched1111111.cs` |
| Data-connector task | `data-connector-tasks/import-test-connector-impTestConn11111111111.cs` |
| Migration task | `migration-tasks/import-test-migration-importTestMigr11111111.cs` |
| Custom code index | `indexes/code/code-index-Manufacturer-import-test.cs` |
| Search code index | `indexes/search/search-index-Manufacturer-import-test.search-code.cs` |
| Search index materialize | `indexes/search/search-index-Manufacturer-import-test.materialize-code.cs` |
| NLP entity post-processing | `entity-post-processing/RnYLiA1ZoMqAqCybzouyeo.cs` |

Custom chat AI providers (`code/ai-providers`) and agent output schemas do **not** support imports —
they compile raw code. Their *editor diagnostics* do expand imports, so an import written there
lints clean and then fails at runtime. Agents, prompt templates and skills are prompt text, not C#.

Every consumer returns or logs `ImportTestReport.For("<kind>", importTestLevel3)`, so the same
formatter proves the closure resolved identically in every scope.

## Rules a shared endpoint must follow

These are properties of the expansion, not style preferences. The imported bodies are spliced in
**above** the consumer's own code, dependency-first, after the consumer's leading `using` block.

1. **Declaration-only — never `return`.** A top-level `return` in a shared endpoint short-circuits
   every consumer that imports it, because it runs first.
2. **No `using` directives.** Only the *consumer's* leading using block is hoisted. The first
   appended import may get away with usings; every later one is `CS1529`. The shared layer here is
   fully qualified instead (`System.Text.StringBuilder`, `System.Text.RegularExpressions.Regex`, …),
   which also keeps it independent of each scope's ambient import set — those differ, and only the
   intersection is safe to rely on.
3. **Never touch a scope global.** See "Reaching the execution scope" above — take the graph or
   logger as a parameter instead. This is what lets one shared layer serve all nine scopes.
4. **Watch for name collisions.** Everything lands in one flat Roslyn submission with no namespace
   isolation, so two imports declaring the same type is `CS0101`.
5. **A class cannot read a top-level variable either.** Same reason as the scope globals: top-level
   `var`s become members of the submission, and a nested type has no reference to it. Shared helpers
   take the chain as a *parameter* (`ImportTestReport.For(kind, chain)`), and the AI-tool fixture
   assigns its chain to a static field from top-level code before returning the tool instance.
6. **No `namespace`, no extension methods.** Both are rejected outright — see the table above.

## Running it

```bash
curiosity-cli import-workspace-definitions --import-path config --zip
```

Then, with a system-admin token (the run route is `POST /api/endpoints/run/{**endpointPath}`):

```bash
curl -s -X POST -H "Authorization: Bearer $CURIOSITY_API_TOKEN" \
     https://<workspace>/api/endpoints/run/import-test/verify | jq
```

Expected (this is real output, captured from the fixture set):

```json
{
  "consumer": "code-endpoint",
  "chain": "level1-core > level2-text + level1-core > level2-graph > level3-report",
  "scope": "graph:writable user:yes | level2-text(text helpers via import-test/verify)[instance], level3-report<String:...>, level2-text.Nested, counter:1, kind:Endpoint, delegate:OK, tail:level3-report",
  "signature": "level2-graph/ready",
  "layers": [
    { "name": "level1-core",   "detail": "root" },
    { "name": "level2-text",   "detail": "text helpers via level3-report" },
    { "name": "level2-graph",  "detail": "graph-shape helpers" },
    { "name": "level3-report", "detail": "report builder" }
  ],
  "ok": true
}
```

Each field is a receipt for something different:

| Field | Proves |
|---|---|
| `chain` | transitive resolution, diamond dedup, dependency-first ordering |
| `scope` | the instance class (deriving from a base in another import), the generic class, the nested type, the struct + mutable static state, the enum, the delegate, the imported top-level local function, and the scope handshake |
| `signature` | a static constructor in imported code ran |
| `layers` | polymorphic iteration over `IImportTestLayer`, whose four instances are a mix of records and an instance class |

`import-test/verify-readonly` returns the same shape with `"consumer": "read-only-endpoint"` and
`"scope": "graph:read-only"` — the shared helper binds its other overload because that scope hands
over a `Safe.ReadOnlyGraph`.

For the other kinds, each of which writes its report line to the server log:

| Kind | Trigger |
|---|---|
| Scheduled / data-connector task | Manage → Scheduled Tasks → **Run now**. Their crons are 1 January, so they never fire unattended. |
| Migration task | Manage → Migrations → run. The importer always stores migration tasks with `triggerRunOnStartup: false`. |
| Custom code index | Reindex `Manufacturer`. |
| Search code index | Any workspace search that reaches `Manufacturer`. It always returns no results. |
| Search index materialize | Never runs — the search half returns no rows to materialize, so this half is verified **compile-only**: its proof is the absence of `Error compiling materialize code index` in the server log. |
| Entity post-processing | Re-run the NLP pipeline over an `_Organization` node. |

### Dedup

Open `import-test/verify` in Manage → Build → Endpoints and use *Show generated code* (or add
`//#DEBUG-SCHEMA` to the endpoint temporarily and read the server log). Each of the four shared
bodies must appear exactly **once** despite three import lines naming an overlapping closure.

### `Imported by`

`shared/import-test/level1-core` should report *imported by 2* — the two level-2 endpoints only.
Not `level3-report`, not `verify`, not the AI tool: only **direct** imports are recorded as edges,
and only endpoints and AI tools record them at all.

## Known gaps this fixture set exposes

- **Cascade recompile is one hop.** Change `MARKER` in `level1-core.cs`, re-import, and call
  `import-test/verify` again. On re-import a changed endpoint evicts and recompiles its *direct*
  dependents, but two-hop dependents are not refreshed, so `verify` can keep serving a stale
  compiled body until something else invalidates it.
- **Import order within a bundle.** Endpoints are imported before every other code kind, but within
  the endpoints themselves the order is the archive's. A chain member that lands after its dependent
  makes that dependent fail to precompile (a warning in the log, `SuccessfullyPrecompiled = false`);
  it self-heals on the first request because the endpoint is compiled again then.

## Not covered here

By design, this set is happy-path only. The failure modes — missing import, self-cycle and indirect
cycle, the `// ImportEndpoint` typo, two imports each declaring usings (`CS1529`), an imported
endpoint with a top-level `return`, and `//#SKIP-INJECT` disabling expansion entirely — are not
shipped as broken fixtures, because every one of them would make this bundle import with warnings.
The first three are covered by unit tests in the product repo
(`Testing/Mosaik.Testing.Graph/src/GraphTests.Core.cs`).

## UIDs

The AI tool and the three tasks carry minted `UID128`s (`Other/FindHardcodedUID` in the product
repo). The entity-post-processing UID is **derived, not minted**: the runtime looks the hook up by
`_EntityPostProcessing.For(entityType)`, so `RnYLiA1ZoMqAqCybzouyeo` is
`Hashes.Combine("_EntityPostProcessing".Hash128(), "_Organization".Hash128())`. Changing its
`EntityType` means recomputing the UID — a minted one would import cleanly and then never fire.
