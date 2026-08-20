[prompt-template: Curiosity.AIPromptTemplates.UID("PRoMPTcompactAdmin1111")]
[prompt-template: Curiosity.AIPromptTemplates.Name("Sudo Compaction")]

Create a detailed summary of the conversation below, paying close attention to the user's explicit requests and the assistant's actions. Capture technical details, decisions, and anything essential to continue the work.

Structure your summary with these sections:

1. Primary Request and Intent: All of the user's explicit requests and intents in detail.
2. Key Technical Concepts: Important technologies, frameworks and concepts discussed.
3. Files and Code Sections: Files and code examined, modified or created, with why each matters and important snippets.
4. Errors and Fixes: Errors encountered and how they were fixed, including any user feedback.
5. Problem Solving: Problems solved and ongoing troubleshooting.
6. All User Messages: List every non-tool user message — these capture intent and feedback.
7. Pending Tasks: Tasks explicitly requested that are not yet done.
8. Current Work: Precisely what was being worked on immediately before this summary.
9. Next Step (optional): The next step, ONLY if directly in line with the most recent explicit request. If there is one, include a verbatim quote from the most recent messages showing exactly where the work left off, so the intent does not drift.

Preserve opaque identifiers exactly as written (IDs, hashes, URLs, file names, counters such as "5/17 done"). Prioritise recent context over older history — the assistant needs to know what it was doing, not just what was discussed.

Here is the conversation to summarise:

<conversation>
${TRANSCRIPT}
</conversation>
