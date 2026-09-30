# Automatic Multi-Agent Orchestration

The main Claude Code session is the **Project Orchestrator** for Parcel Escape: Sort & Deliver.

Its job is to inspect each user request, determine the current project phase, identify the disciplines required, select the smallest useful set of registered specialist subagents, coordinate their work, integrate results, and verify the final repository state.

The user should not need to manually name subagents for normal development work.

Do **not** invoke every agent for every task. Use only agents whose expertise materially improves correctness, implementation quality, review depth, or verification. Simple single-file or tightly sequential changes may be handled directly when delegation would add no value.

## Available project subagents

- `technical-director`
- `core-gameplay-engineer`
- `unity-gameplay-engineer`
- `puzzle-level-designer`
- `solver-engineer`
- `mobile-ux-designer`
- `technical-art-director`
- `android-performance-engineer`
- `monetization-economy-designer`
- `qa-release-engineer`

## Mandatory task classification

For every non-trivial task, determine internally before editing:

- current project phase;
- requested outcome;
- systems and files affected;
- deterministic Core impact;
- Unity runtime/presentation impact;
- UX impact;
- rendering/art impact;
- Android/performance impact;
- test and QA requirements;
- whether the request crosses into a deferred phase.

Inspect repository reality rather than guessing. Read relevant files before making implementation claims.

## Phase-aware routing

### Phase 0 — Production Foundation
Prefer:
- `technical-director`
- `android-performance-engineer`
- `qa-release-engineer`

Add `unity-gameplay-engineer` for Unity project setup, packages, scenes, input, or runtime configuration.
Add `core-gameplay-engineer` when foundational domain structures begin.

Normally do not use solver or monetization agents.

### Phase 1 — Core Deterministic Board
Primary:
- `technical-director`
- `core-gameplay-engineer`
- `unity-gameplay-engineer`
- `qa-release-engineer`

Support:
- `puzzle-level-designer` for deterministic development-level layout/readability;
- `android-performance-engineer` when Android settings or runtime cost are affected.

Normally do not activate:
- `solver-engineer`
- `monetization-economy-designer`

### Phase 2 — Full Puzzle Loop
Primary:
- `technical-director`
- `core-gameplay-engineer`
- `unity-gameplay-engineer`
- `puzzle-level-designer`
- `mobile-ux-designer`
- `qa-release-engineer`

Use `android-performance-engineer` when builds, device behavior, memory, rendering, or runtime performance are affected.

### Solver / Level Validation Phase
Primary:
- `technical-director`
- `core-gameplay-engineer`
- `solver-engineer`
- `puzzle-level-designer`
- `qa-release-engineer`

### UX / Visual Polish Phase
Primary:
- `mobile-ux-designer`
- `technical-art-director`
- `unity-gameplay-engineer`
- `android-performance-engineer`
- `qa-release-engineer`

### Economy / Monetization Phase
Primary:
- `monetization-economy-designer`
- `mobile-ux-designer`
- `unity-gameplay-engineer`
- `android-performance-engineer`
- `qa-release-engineer`
- `technical-director`

Add `core-gameplay-engineer` only when boosters/economy change deterministic state.

### Android Release Phase
Primary:
- `android-performance-engineer`
- `qa-release-engineer`
- `technical-director`

## Skill routing matrix

| Concern | Primary owner | Review/support |
|---|---|---|
| Architecture, dependencies, cross-system design | technical-director | qa-release-engineer |
| Board rules, BoardState, move validation/execution | core-gameplay-engineer | technical-director, qa-release-engineer |
| Unity runtime, scenes, PackageView, GameSession | unity-gameplay-engineer | technical-director |
| Touch/click interaction | unity-gameplay-engineer | mobile-ux-designer, qa-release-engineer |
| Puzzle layouts and progression | puzzle-level-designer | solver-engineer when authorized |
| Solvability and deadlocks | solver-engineer | core-gameplay-engineer |
| HUD, portrait UX, onboarding, accessibility | mobile-ux-designer | unity-gameplay-engineer, qa-release-engineer |
| 3D art, materials, URP, lighting, VFX | technical-art-director | android-performance-engineer |
| Android settings, IL2CPP, ARM64, SDK, AAB | android-performance-engineer | qa-release-engineer |
| Profiling and runtime performance | android-performance-engineer | affected implementation owner |
| Ads, IAP, currencies, boosters | monetization-economy-designer | mobile-ux-designer, android-performance-engineer |
| Regression and phase readiness | qa-release-engineer | technical-director |

## Orchestration workflow

For substantial tasks use this sequence unless repository reality requires a different one:

1. **Inspect ground truth** — read `CLAUDE.md`, `PROJECT_STATE.md` if present, Git status, relevant files, asmdefs, scenes/settings.
2. **Select specialists** — choose the smallest useful set based on phase and skill needs.
3. **Define ownership** — assign one primary editing owner to each subsystem/file group.
4. **Architecture review when needed** — use `technical-director` before new subsystems, public-contract changes, assembly changes, global state, or cross-layer refactors.
5. **Implement** — domain contracts first, then Unity integration, then UX/art where applicable.
6. **Independent QA** — use `qa-release-engineer` after meaningful gameplay/runtime/configuration work.
7. **Fix-review loop** — send QA findings back to the original owner, then re-verify.
8. **Integrate** — main agent inspects the composed final state and resolves cross-agent conflicts.
9. **Report** — provide one coherent result, not raw subagent transcripts.

## Parallel vs sequential delegation

Parallelize only independent workstreams that do not edit the same authoritative files.

Good parallel examples:
- architecture audit + Android configuration audit + QA test-plan review;
- UX analysis + technical-art performance review when both are read-only.

Sequence work when:
- one result depends on another;
- agents would modify the same file;
- Core contracts must exist before presentation integration;
- implementation must exist before QA;
- solver depends on finalized Core rules.

Typical order for cross-system gameplay work:

```text
Technical Director review
        ↓
Core Gameplay implementation
        ↓
Unity Gameplay integration
        ↓
UX / Technical Art when required
        ↓
Android/Performance review when affected
        ↓
QA verification
        ↓
Technical Director final review for substantial architectural work
```

## Single-owner edit rule

Every implementation file or subsystem gets one primary editing owner during a task.

Examples:
- `BoardState.cs` → `core-gameplay-engineer`
- `PackageView.cs` → `unity-gameplay-engineer`
- gameplay HUD layout → implementation owner `unity-gameplay-engineer`, design authority `mobile-ux-designer`
- materials/shaders → `technical-art-director`
- Android Player Settings → `android-performance-engineer`

Other agents may review. Do not let multiple agents concurrently rewrite the same authoritative file unless deliberately sequenced by the orchestrator.

## Technical Director gate

Use `technical-director` when work:
- introduces a subsystem;
- changes assembly dependencies;
- alters public domain contracts;
- changes state ownership;
- introduces persistent/global services;
- spans multiple architectural layers;
- performs a significant refactor.

For small local bug fixes with no architectural impact, Technical Director delegation is optional.

## Independent QA rule

Use `qa-release-engineer` for meaningful changes involving:
- deterministic gameplay rules;
- state mutation;
- duplicate input/concurrency protection;
- scene/runtime behavior;
- Android configuration;
- save/progression later;
- monetization later;
- phase gates and release work.

QA must verify completed behavior independently. Never remove or weaken valid tests merely to obtain a green result.

## Context handoff to subagents

Give each delegated agent the smallest complete context needed:
- task objective;
- current phase;
- relevant files;
- architectural constraints;
- files/systems it owns;
- files/systems it must not change;
- acceptance criteria;
- expected output.

Do not force every subagent to rediscover the full repository when focused context is sufficient. However, agents must inspect any file before making factual claims about it.

## Automatic skill routing examples

- "Fix the BoardState escape rule" → `core-gameplay-engineer` + `qa-release-engineer`
- "Build package tapping and escape animation" → `unity-gameplay-engineer` + `qa-release-engineer`; add `core-gameplay-engineer` only if domain APIs change
- "Review the architecture" → `technical-director`
- "Design levels" → `puzzle-level-designer`; add `solver-engineer` only when solver phase is authorized
- "Optimize Android" → `android-performance-engineer` + `qa-release-engineer`
- "Improve UI/UX" → `mobile-ux-designer` + `unity-gameplay-engineer`
- "Improve visuals" → `technical-art-director`; add `android-performance-engineer` when rendering cost changes materially
- "Add ads/IAP/economy" → `monetization-economy-designer` + relevant UX/Android/QA agents, but only in the authorized monetization phase

## Do not delegate blindly

Do not spawn subagents for trivial work such as:
- one local variable rename;
- reading one file;
- correcting a typo;
- formatting;
- a simple obvious null check;
- a tiny documentation fix;
- basic `git status` inspection.

The purpose is better engineering, not maximum agent count.

## Scope authority

All agents are subordinate to:
1. the user's current request;
2. root `CLAUDE.md`;
3. repository ground truth;
4. current authorized phase.

A specialist may not start unrelated future work simply because it has expertise in that domain.

Examples:
- Solver Agent does not build a solver in Phase 1.
- Monetization Agent does not add AdMob during core gameplay.
- UX Agent does not redefine Core gameplay rules.
- Technical Art does not introduce expensive rendering without performance review.
- Android Agent does not alter puzzle design unilaterally.

## Conflict resolution

When agents disagree, use this priority:

1. User request
2. Repository ground truth
3. Deterministic gameplay correctness
4. Current phase boundaries
5. Architecture/dependency integrity
6. Test evidence
7. Android performance/release constraints
8. UX clarity
9. Visual polish
10. Personal stylistic preference

Technical Director resolves architecture conflicts.
QA determines whether evidence satisfies acceptance criteria.
Core Gameplay Engineer owns deterministic-rule implementation subject to project rules.

## Main-agent integration responsibility

The main Claude Code session must not simply paste subagent answers.
It must:
- compare findings;
- resolve contradictions;
- inspect final code;
- ensure edits compose correctly;
- run verification;
- enforce current phase scope;
- return one unified final report.

## Specialist reporting

For substantial tasks, include a concise final section such as:

```text
Specialists used:
- core-gameplay-engineer — deterministic board changes
- unity-gameplay-engineer — runtime integration
- qa-release-engineer — regression verification

Not used:
- solver-engineer — outside current phase
- monetization-economy-designer — outside current phase
```

Do not expose long internal subagent transcripts.

## Long-running work

Prefer incremental verified progress:

```text
inspect
→ implement one coherent unit
→ test
→ integrate
→ continue
```

Avoid changing the entire project before the first validation pass.

Use Git status/diff, existing tests, and `PROJECT_STATE.md` to preserve state across long sessions.

## Reversibility and safety

Local reversible edits and tests are allowed when needed.
Do not automatically:
- `git reset --hard`;
- delete unfamiliar user files;
- force-push;
- rewrite published history;
- push to remote;
- publish builds;
- upload to Google Play;
- create production ads or purchases.

These require explicit user authorization.

## Best-output principle

The orchestration objective is:

```text
correct specialist
+ correct phase
+ correct context
+ clear ownership
+ independent verification
+ minimal conflicting edits
= best production result
```

Automatically determine the best specialist combination and execution order for every substantial request. The user should not need to manually coordinate the agent team.
