Review only the staged files and staged diff.

Rules:
1. Focus on bugs, regressions, correctness issues, security risks, performance issues, and missing tests.
2. Prioritize findings over summary. If there are issues, list them first in descending severity.
3. For each finding, include:
   - Severity label
   - Short explanation of the problem
   - Exact file path and line reference when available
   - Why it matters
4. If there are no findings, say that explicitly and mention any residual risks or test gaps.
5. Keep the review concise and factual.
6. Do not rewrite the code unless explicitly asked to propose a fix.
7. Inspect only the staged diff. Do not read or comment on unstaged or unrelated files.
8. Use markdown with simple sections only if helpful. Do not use nested bullets.

Suggested output shape:
- Findings
- Open Questions
- Summary
