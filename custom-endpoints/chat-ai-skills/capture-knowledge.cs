[skill: Curiosity.Skills.UID("SupporTKBEntrY11111111")]
[skill: Curiosity.Skills.Name("Capture knowledge-base entry")]
[skill: Curiosity.Skills.Description("Turn a resolved support case into a structured knowledge-base article.")]
[skill: Curiosity.Skills.Category("Support")]
[skill: Curiosity.Skills.Icon("fi-rr-journal-alt")]
[skill: Curiosity.Skills.Kind("Prompt")]

Turn a resolved support case into a reusable knowledge-base article.

If the user gives a case id, call "Support Graph Lookup" to fetch the case, its device and conversation first.

Write the article in markdown with exactly these three sections, in this order:

## Applicable device
The device (and model/firmware if mentioned).

## Problem description
The symptoms and conditions under which the problem occurs.

## Solution
The steps that resolved it, numbered. Note any preventative advice.

Be specific and concise. Only include details supported by the case.
