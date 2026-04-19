# Rule: Plan-before-change workflow

For any non-trivial implementation task (new feature, fix with multiple files, refactor, audit item), follow this workflow:

## 1. Create a plan file first

Create `.claude/plan/<slug>.md` before writing any code. Name the slug after the task (e.g., `n-l4-stripe-price-sync`, `m6-signalr-push`, `idempotency-lock`).

Use this template:

```markdown
# Plan: <title>

**Status:** In progress
**File(s):** `<primary file paths>`

## Root cause / Goal
<One paragraph: what is broken or what needs to be done and why>

## Checklist
- [ ] Step 1
- [ ] Step 2
- [ ] Step 3

## Notes
<!-- Edge cases, rejected options, open questions -->
```

## 2. Show the desglose to the user and get approval

Before applying any change, present the checklist to the user. Wait for explicit approval ("adelante", "sí", "okay", etc.) before touching any file.

If there are multiple valid approaches, present them as options:
- **Option A** — description (pros/cons)
- **Option B** — description (pros/cons)

Ask the user to choose. Never pick for them on decisions that affect billing, external APIs, or shared infrastructure.

## 3. Apply changes and check off items in real time

Mark each item `[x]` in the plan file immediately after completing it — not in a batch at the end.

## 4. Mark the plan complete

When all items are done, update `Status: Done` and move the file to `.claude/plan/done/` (or leave it in place — either is fine).

---

## When this rule applies

- Any task spanning more than 2 files
- Any audit fix (even a "simple" one — they often grow)
- Any Stripe-related change (price creation, subscription migration)
- Any SignalR hub change
- Any new test file creation covering multiple paths

## When this rule does NOT apply

- Quick targeted edits (single file, single bug, obvious fix)
- Answering a question or explaining code
- Renaming a field that appears in one place
