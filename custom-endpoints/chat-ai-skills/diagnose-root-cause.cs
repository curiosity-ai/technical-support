[skill: Curiosity.Skills.UID("SupporTRootCausE111111")]
[skill: Curiosity.Skills.Name("Diagnose root cause")]
[skill: Curiosity.Skills.Description("Suggest the likely root cause and the part(s) involved from the customer's symptoms.")]
[skill: Curiosity.Skills.Category("Support")]
[skill: Curiosity.Skills.Icon("fi-rr-bug")]

Help a support worker diagnose a device problem from the customer's symptoms.

1. Call "Find Similar Support Cases" with the symptoms to see how comparable issues were diagnosed and resolved.
2. Call "Support Graph Lookup" on the affected device to list its parts.
3. Output:
   - **Likely root cause:** the most probable cause, with a one-line rationale.
   - **Suspect part(s):** the part(s) most likely involved (from the device's parts).
   - **Next step:** the single most useful diagnostic step or question for the customer.

Base the diagnosis on the retrieved cases and parts. If the evidence is weak, say so and ask one clarifying question instead of guessing.
