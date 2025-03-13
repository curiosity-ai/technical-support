# Setting Up NLP on a Curiosity Workspace Application

## Table of Contents
1. [Introduction](#introduction)
2. [Configuring NLP Pipelines](#configuring-nlp-pipelines)
3. [Creating Spotter Models](#creating-spotter-models)
4. [Creating Pattern Spotter Models](#creating-pattern-spotter-models)
5. [Running Entity Capture Experiments](#running-entity-capture-experiments)
6. [Setting Up Automatic Entity Linking on the Graph](#setting-up-automatic-entity-linking-on-the-graph)
7. [Reparse all the data with the new settings](#reparse-all-the-data-with-the-new-settings)
8. [Conclusion](#conclusion)

---

## Introduction
Curiosity Workspace offers a powerful NLP framework to extract and organize information from your data automatically. This guide walks through configuring NLP pipelines, creating models, linking entities, and experimenting with entity capture. By the end, you’ll have a fully operational NLP setup tailored to your data needs.

## Configuring NLP Pipelines
NLP pipelines define the steps for processing unstructured text data. In a Curiosity Workspace, these pipelines can be used to transform raw text into structured data. 

For setting up a pipeline, start by navigating to the `Management` interface, and navigating to `Languages`. Click on the English language to configure it. This will automatically download the required NLP models from nuget.org for you. 

Now proceed to the `NLP` settings page, and select `Pipelines`. Click on the option to create a new pipeline. Choose `English` for the pipeline language (which will affect the components like tokenization, part-of-speech tagging and sentence detection that are automatically added to the pipeline), select `data parsing`, give your pipeline a name and continue. Now open the pipeline, click on the tab `Used for`, and select the node type and field `SupportCase > Summary`. If not visible, you can click on `Show all node types` to show all types.

## Creating Spotter Models
Spotter models, Curiosity’s version of gazetteer models, are used to identify entities based on predefined lists of terms. 

You can create a Spotter model from a given node type, so that it will be automatically up-to-date on data changes from a data connector. You can also manually create a Spotter model by providing a list of known entities and their variations. These models are ideal for recognizing fixed, well-defined categories like product names, locations, or company titles.

For this dataset, let's create a Spotter model for the `Device` node type. Start by navigating to the `Data` management interface. Select the `Device` node type in the list, click on `Capture` and then `New Spotter`. As this node type only has one field (`Name`), you can continue. Finally, set a minumum size of 4 characters and click on `Save`. Navigate to the `Pipelines using this model` and add it to the pipeline you created above.


## Creating Pattern Spotter Models
Pattern Spotter models function like advanced regex, capturing entities based on complex text patterns. You can define patterns using Curiosity’s pattern editor, specifying rules for word sequences, token types, and contextual relationships. Use this for detecting structured data like dates, IDs, or custom text formats.

Let's start by creating a simple pattern to capture potential identifiers in the data. Navigate to the `Data` management interface, and click on `Experiment and Capture`. Click on `Capture using patterns`. In the definition, add the following patterns.

- Shape = `XX999`
- Shape = `X9999`
- Shape = `XX9999`

You can use the text area on the right side to test your pattern.  Now let's continue and run an experiment to see what this pattern is capturing.

## Running Entity Capture Experiments

Before deploying models, it is useful to run entity capture experiments to validate what is being captured. This is useful to identify potential exceptions to be added to Spotter models, to iterate on Patterns, and to understand if it would be a useful model or not (for example, models capturing too much information or too little information might not be relevant to a use-case). 

As you're in the Pattern Spotter setup flow by now, click on the `Experiments` tab and click on `Run new experiment`. You can then click on the experiment on the list and you should see any entities being captured next to it.

If you found the patterns being captured useful, you can proceed to create a new schema for this pattern (let's call it for example `PotentialIdentifiers`). Select where to capture it (select `Support Case > Summary`), and proceed until the end to create the pattern spotter and node type in the system. *Don't forget to add the model to your pipeline.*

## Setting Up Automatic Entity Linking on the Graph

Once entities are captured, Curiosity can also link them automatically to the knowledge graph. You can configure entity linking rules to match extracted entities with existing graph nodes, or create new nodes if they don't exist yet.

Start by navigating to the `Data` management interface, select the `Device` node type, then open `Linking`. Click on the toggles to enable the linking for `Support Case > Summary`. You can also customize the edge type used for this linking, but you can also leave the default values (i.e. `_AppearsIn` and `_Mentions`).

Repeat the same for the node type you created for the pattern spotter above, and also enable the option `If missing node, Create new node`.

## Reparse all the data with the new settings

Finally, you need to trigger the reparsing of all the `SupportCase` nodes by navigating to the node schema page under `Data`, selecting `Reparse this data` and triggering the reparsing task. The application will automatically start parsing the data for you in the background. 

You can use the `Overview` page to see that there are new edges added to the `SupportCase` nodes (i.e. `_Mentions -> PotentialIdentifiers` and `_Mentions -> Device`. 

## Conclusion
Curiosity Workspace’s NLP capabilities offer robust tools for structuring and linking textual data. By setting up pipelines, models, and entity linking, you create a scalable system for information extraction and knowledge graph enrichment. Iterate on experiments to keep your system precise and effective.

## Useful queries

### List all NLP pipelines:
```csharp
return Q().StartAt("_NlpPipeline").Emit("N", includeHidden:true);
```

### List all Spotter Models:
```csharp
return Q().StartAt("_NlpSpotterFromNode").Emit("N", includeHidden:true);
```

### List all Pattern Spotter Models:
```csharp
return Q().StartAt("_NlpPatternSpotter").Emit("N", includeHidden:true);
```

### Get a pipeline by UID and use it to parse a document
```csharp
var (exists, pipeline) = await Graph.Internals.GetPipelineAsync(UID128.Parse("CyW7HoQdgY8YLE4g95gcA2"));
if (!exists) throw new Exception("Pipeline not found");

var doc = new Catalyst.Document("The big brown fox jumps over the lazy dog", Language.English);
pipeline.ProcessSingle(doc);
return doc.ToJson();
```

### Return the documents created for a given
```csharp
return Q().StartAt(N.SupportCase.Type).Take(1).Emit("Node").Out("_Document").Emit("Doc", includeHidden:true);
```

## Next steps
- Configure search on the data in the [Search Configuration Guide](/search-configuration/INSTRUCTIONS.md)
- Setup your own API endpoints in the [Custom Endpoints Guide](/custom-endpoints/INSTRUCTIONS.md)
