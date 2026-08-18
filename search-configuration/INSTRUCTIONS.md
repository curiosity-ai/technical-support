# Curiosity Workspace // Full Text Search and AI Search

## Table of Contents

1. [Introduction](#introduction)
2. [Configuring Search](#configuring-search)
3. [Search Ranking](#search-ranking)
4. [Filters](#filters)
5. [AI Search](#ai-search)
6. [Code Index for Extracted Questions](#code-index-for-extracted-questions)
7. [Synonyms](#synonyms)
8. [Useful queries](#useful-queries)
9. [Conclusion](#conclusion)

## Introduction

Curiosity AI provides a search engine designed for structured and unstructured data. This search engine is tightly integrated with the graph database features, and allows one to use advanced graph queries and graph filters to narrow down the search as required. It also offers various configuration options to improve search relevance, handle multilingual content, define synonyms, configure filters. Access control mechanisms ensure users only retrieve data they are authorized to see. On top, AI-based search using embeddings can also be used to re-rank and surface content that would otherwise not be found by the keyword-based full-text search. 

The search engine supports 21 languages, including English, French, German, Italian, Spanish, Portuguese, Dutch, Swedish, Danish, Norwegian, Polish, Korean, Japanese, Chinese, Malay, Hindi, Hebrew, Serbian, Nepali, Greek, and Czech. Users can specify the languages relevant to their data to optimize search results.

## Configuring Search

In order to configure search, the system must first index the data. For that, one needs to enable which node types and respective fields will be searchable. To do so, navigate in the workspace to the Management interface, open `Data → Search`, and on the `Search` tab find `Full Text Search` and click on `+ Add more` to configure new types. For this dataset, you can add `SupportCase.SupportCaseSummary`, `SupportCase.Content`, `Part.Name` and `Device.Name`. Click on `Accept` to confirm.

Once you have enabled search for the required node types, you can use the toggle `Searchable`/`Not Searchable` to switch if each data type should be shown in the default search page of the workspace. 

Of course, for custom search areas implemented in a custom user interface, you've full control of which data types should be searchable when defining a search request.

## Search Ranking

Search results are ranked based on a BM25 algorithm. BM25 (Best Matching 25) is a method used to rank documents based on how well they match a search query. It looks at how often the search terms appear in a document and takes the document’s length into account — giving more weight to shorter, more focused documents. This approach helps balance term frequency and document length, making BM25 effective for ranking search results. On top of the BM25 algorithm, Curiosity provides a way to boost or demote search results scores depending on the node field where it was found. This can be useful to promote results on fields that are more relevant (for example a document identifier or document title).

To alter the boost value used for each field, you can use the `+` and `-` buttons next to each entry in the `Full Text Search` settings page.

## Filters

Filters help users refine their search results by applying constraints such as time period, sources or other values of the data. Filters should be customized based on the dataset structure and use-case requirements. 

On a Curiosity application, there are two default filter types:
- Type: This represents the Node Type of the data. For this dataset, this means a filter for SupportCase, Device and Part.
- Time: The time filter uses the value of the Timestamp field that is available for all nodes in the graph. As not every timestamp will have a value relevant to be used as a filter, it is recomended to exclude from the time filter any node types where the information is not relevant. For this, navigate to `Excluded from Time`.

Additionally, you can enable two types of filters on your data:
- Property Facets: These filters use values from the node object to filter by values.
- Related Facets: These filters use information from the graph relationships to allow you to filter by related data. 

In order to enable filters, you can use the `Property Facets` and `Related Facets` settings on the `Facets` tab under `Data → Search`.

For this dataset, we recomend enabling for Related facets: `Status`, `Manufacturer` and `Device`

You should also exclude from the time filter the types `Part` and `Device`.

## AI Search

Curiosity supports out of the box the usage of embedding models to retrieve data in adition to the full-text search approach. It uses behind the scene a fast CPU-capable embedding model (miniLM or ArcticXS) to index text data, and an HNSW-graph based index to enable fast retrieval of data.

To configure AI search, open `Data → Search` and then select `AI Search` on the `Search` tab. Click on `+ Add more` to configure new types. For this dataset, you can add `SupportCase.SupportCaseSummary`, `SupportCase.Content`, `Part.Name` and `Device.Name`. Click on `Accept` to confirm.

By default, all AI search indexes are created without chunking enabled. You should enable chunking if the text in the data might be bigger than the context size from the embedding model used. By default, Curiosity will use ArcticXS, which has a context size of 512 tokens. For this dataset, you should then enable chunking for the `SupportCase.Content` field. For that, click on the respective `...` button next, enable the `Chunk Text` option and click on save.

You can also use the `+` and `-` buttons to control the cutoff value used by the search engine when selecting similar results. Results will be added when their similarity score is above the `added` cutoff, and will be re-ranked if already present in the search results if their similarity score is above the `rerank` value.

## Code Index for Extracted Questions

Besides the built-in full-text and AI search indexes, Curiosity supports **code indexes**: small C# scripts that run over each node of a given type *as it comes in to be indexed*. A code index is enrichment-only — instead of returning text to index, it performs side effects on the graph (adding derived nodes/edges, calling endpoints or AI tools, …). Code indexes are managed under `Manage → Indexes → Code Indexes` and can be exported as source code for version control.

For this dataset we use a code index to sanitize the questions extracted from each support case. The extraction and sanitization logic already lives in two endpoints (see the [Custom Endpoints Guide](/custom-endpoints/INSTRUCTIONS.md)): `extract-questions` stores the raw questions on an `ExtractedQuestions` node, and `sanitize-questions` runs the *Sanitize Support Questions* AI tool to replace personally identifiable information (PII) with neutral placeholders, writing the result into `SanitizedQuestions` / `SanitizedTopic` and setting `Sanitized` to `true`.

Rather than having to trigger `bulk-extract-questions` by hand, the [`Sanitize Extracted Questions`](/config/code/code-indexes/sanitize-extracted-questions.cs) code index runs automatically over every `ExtractedQuestions` node: for each node in the incoming batch it skips nodes that are already sanitized (or have no questions yet) and otherwise calls the `sanitize-questions` endpoint. The index is attached to the `ExtractedQuestions` node type via its header:

```csharp
[indexes: Curiosity.Indexes.CodeIndex("ExtractedQuestions")]
[indexes: Curiosity.Indexes.Name("Sanitize Extracted Questions")]
```

Because the code runs whenever an `ExtractedQuestions` node is added or updated, questions are sanitized as soon as they are extracted, and the `Sanitized` flag guard keeps the index from reprocessing (or looping on) nodes that are already done. The index is checked in with the rest of the workspace definitions under [`config/code/code-indexes`](/config/code/code-indexes) and imported with the rest of the `config/` bundle.

## Synonyms

The synonym system allows defining equivalent terms to improve search recall. Instead of requiring exact keyword matches, you can configure word mappings so that searches for one term return results for its synonyms. This is useful for handling domain-specific terminology, abbreviations, or common variations.

Synonyms can be managed under `Data → Search`, on the `Synonyms` tab (`Predefined Synonyms`). 

## Useful queries

For searching in the shell and on endpoints, you can use two main entry points to the search engine. One is provided as part of the Query interface and let's you search the full-text search indexes directly, bypassing all the logic of the search engine from the Curiosity workspace. The second is to use the complete search engine (the same as used by the front-end to search), which provides multiple advanced features and also includes the AI search integration for ranking and adding new results (if enabled).

### Full-text search as the starting point of a query

```csharp
return Q().StartSearch(N.SupportCase.Type, N.SupportCase.Content, SearchExpression.For(SearchToken.StartsWith("MacBook Air"), "MacBook Air")).Emit();
```

### Full-text search within a query

```csharp
return Q().StartAt(N.SupportCase.Type).WhereTimestamp(Time.Now().Add(TimeSpan.FromDays(-100)), Time.Now(), insideBoundary: true)
          .Search(N.SupportCase.Type, N.SupportCase.Content, SearchExpression.For(SearchToken.StartsWith("MacBook Air"), "MacBook Air"))
          .Emit();
```

### AI-search using embeddings indexes

```csharp
return Q().StartAtSimilarText("Apple screen issue").EmitWithScores();
```

### AI-search using embeddings indexes on a specific type

```csharp
return Q().StartAtSimilarText("Apple screen issue", nodeTypes:[N.SupportCase.Type]).EmitWithScores();
```

### Search using the full search engine

```csharp
var request = SearchRequest.For("MacBook Air");
request.BeforeTypesFacet = new([N.SupportCase.Type, N.Device.Type, N.Part.Type]);
request.SortMode = SortModeEnum.RecentFirst;

var query = await Graph.CreateSearchAsync(request);

return query.Emit();
```

### Search using the full search engine and using a target query

```csharp
var request = SearchRequest.For("MacBook Air");
request.BeforeTypesFacet = new([N.SupportCase.Type, N.Device.Type, N.Part.Type]);
request.SortMode = SortModeEnum.RecentFirst;
request.TargetUIDs = Q().StartAt(N.Manufacturer.Type, "Apple").Out().AsUIDEnumerable().ToArray();

var query = await Graph.CreateSearchAsync(request);

return query.Emit();
```

### Search using the full search engine as a user

```csharp
var user = Q().StartAt(_User.Type).AsUIDEnumerable().First();

var request = SearchRequest.For("MacBook Air");
request.BeforeTypesFacet = new([N.SupportCase.Type, N.Device.Type, N.Part.Type]);
request.SortMode = SortModeEnum.RecentFirst;

var query = await Graph.CreateSearchAsUserAsync(request, user);

return query.Emit();
```

## Conclusion

Curiosity AI provides a flexible and configurable search engine with support for multiple languages, synonym handling, filtering, embeddings support and access control. Developers can customize search behavior to match their application's requirements and ensure efficient, secure data retrieval.

## Next steps
- Setup your own API endpoints in the [Custom Endpoints Guide](/custom-endpoints/INSTRUCTIONS.md)
- Build a custom user interface in the [User Interface Guide](/custom-front-end/INSTRUCTIONS.md)