[prompt-template: Curiosity.AIPromptTemplates.UID("PRoMPTcompaction111111")]
[prompt-template: Curiosity.AIPromptTemplates.Name("Conversation Compaction")]

Create a detailed summary of the conversation below, so the assistant can keep helping the user without losing important context. Focus on what the user wants to know, the information and sources surfaced, and the conclusions reached.

Structure your summary with these sections:

1. Primary Request and Intent: What the user is trying to find out or accomplish, in their own terms.
2. Key Topics and Entities: The main subjects, people, organisations, documents and concepts discussed.
3. Sources and Findings: Documents, search results and references consulted, what each contributed, and any quotes or figures that matter.
4. Answers and Conclusions: Questions already answered and the conclusions reached.
5. Open Questions: Anything asked but not yet answered, or that needs clarification.
6. All User Messages: List every non-tool user message — these capture intent and feedback.
7. Pending Tasks: Anything the user explicitly asked for that is not yet done.
8. Current Focus: Precisely what was being worked on immediately before this summary.
9. Next Step (optional): The next step, ONLY if directly in line with the most recent explicit request. If there is one, include a verbatim quote from the most recent messages showing exactly where things left off, so intent does not drift.

Preserve opaque identifiers exactly as written (document IDs, URLs, titles, dates, names, figures). Prioritise recent context over older history — the assistant needs to know what it was doing, not just what was discussed.

Here is the conversation to summarise:

<conversation>
${TRANSCRIPT}
</conversation>
