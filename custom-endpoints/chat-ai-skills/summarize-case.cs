[skills: Curiosity.ChatAISkills.UID("SupporTSummarYCase1111")]
[skills: Curiosity.ChatAISkills.Name("Summarize a support case")]
[skills: Curiosity.ChatAISkills.Description("Produce a tight summary of a support case: problem, what was tried, and the outcome.")]
[skills: Curiosity.ChatAISkills.Category("Support")]
[skills: Curiosity.ChatAISkills.Icon("fi-rr-document")]
[skills: Curiosity.ChatAISkills.Kind("Prompt")]

Summarize a support case for a support worker.

If the user gives a case id (e.g. "SC-36556"), call "Support Graph Lookup" to fetch the case, its device and conversation. Otherwise summarize the case the user pasted.

Produce exactly:
- **Problem:** one line describing the customer's issue and the device.
- **Tried:** what the customer and support already attempted.
- **Outcome:** the resolution, or the current status and the recommended next step if still open.

Use short bullets. Do not add information that is not in the case.
