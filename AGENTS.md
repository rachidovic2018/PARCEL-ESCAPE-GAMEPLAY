# AGENTS.md — Parcel Escape: Sort & Deliver

## Project Identity

**Game:** Parcel Escape: Sort & Deliver  
**Engine:** Unity 6 LTS  
**Language:** C#  
**Render Pipeline:** URP  
**Primary Platform:** Android / Google Play  
**Orientation:** Portrait  
**Presentation:** 2.5D / lightweight 3D  
**Current Authorized Scope:** Phase 0 — Production Foundation + Phase 1 — Core Deterministic Board

This repository is a production mobile game project, not a prototype. Preserve architecture, determinism, Android performance, maintainability, and the ability to ship.

---

# 1. OPERATING RULES

## 1.1 Repository Reality Is Ground Truth

Before changing anything, inspect the repository.

Any existing:

- code
- folder structure
- class name
- namespace
- scene
- assembly definition
- Unity setting
- package
- level rule
- prefab
- ScriptableObject
- asset
- Git state
- project documentation

is authoritative unless explicitly changed by the user.

Never invent an existing implementation.

When repository reality conflicts with an older plan, repository reality wins unless the user explicitly approves a refactor.

If implementation details required for a safe change are missing, inspect the relevant files before modifying architecture.

---

## 1.2 Do Not Overbuild

Implement only what is required for the current authorized phase.

Current scope:

- Phase 0 — Production Foundation
- Phase 1 — Core Deterministic Board

Do not begin Phase 2 or later systems automatically.

Do not create speculative services, managers, interfaces, factories, repositories, SDK wrappers, event buses, or placeholder systems merely because they may be useful later.

Create an architectural seam only when the current implementation genuinely requires one.

---

## 1.3 Preserve User Work

Before editing:

1. run `git status`;
2. record the current branch;
3. record the current HEAD SHA;
4. identify staged, modified, deleted, and untracked files.

Never automatically:

- reset
- clean
- checkout over user changes
- revert user work
- delete unrelated files
- rewrite Git history
- force push
- push to a remote repository

At completion, report all files created and modified.

---

# 2. CURRENT GAME RULES

Parcel Escape is a one-tap deterministic logistics puzzle.

The board contains packages positioned on an integer grid.

Each package has:

- deterministic unique ID
- grid position
- fixed direction
- package color/type

Directions:

- Up
- Down
- Left
- Right

A package may escape only when every logical grid cell between its current position and the board boundary in its fixed direction is clear.

Packages:

- never turn;
- never change direction;
- never move one grid cell at a time;
- either escape completely or remain blocked.

Blocking may come from:

- another package;
- a fixed blocker.

Puzzle legality must never depend on:

- Unity physics;
- colliders;
- animation completion;
- Transform positions;
- renderer state;
- random behavior;
- frame timing.

The logical simulation is authoritative.

---

# 3. COORDINATE CONVENTION

The logical board coordinate system is fixed:

- origin `(0,0)` = bottom-left;
- X increases to the right;
- Y increases upward.

Direction deltas:

```text
Up    = ( 0, +1)
Down  = ( 0, -1)
Left  = (-1,  0)
Right = (+1,  0)
```

Unity world-space mapping may differ.

A typical presentation mapping may be:

```text
Grid X -> World X
Grid Y -> World Z
```

but world coordinates must never redefine logical rules.

---

# 4. ARCHITECTURAL PRINCIPLES

Use three conceptual layers.

## 4.1 Domain / Core

Owns deterministic puzzle state and rules.

Examples:

- GridPosition
- PackageDirection
- PackageColor
- PackageState
- BlockerState
- CellOccupancy
- BoardState
- MoveCommand
- MoveValidator
- MoveValidationResult
- MoveExecutor
- MoveExecutionResult
- pure runtime level data

Core logic must remain independent from presentation.

`ParcelEscape.Core` must not reference:

- UnityEngine
- MonoBehaviour
- GameObject
- Transform
- Collider
- Physics
- UI
- scene objects
- animation
- input systems

The goal is for the deterministic puzzle simulation to compile as ordinary C# domain code.

---

## 4.2 Runtime / Gameplay Orchestration

Coordinates application flow.

Examples:

- GameSession
- level conversion/loading
- move request orchestration
- interaction state

Runtime orchestration may depend on Core.

It must not become a global god object.

Do not create:

- `GameSession.Instance`
- `GameManager.Instance`
- static mutable gameplay state

Dependencies should be composed explicitly by scene/bootstrap code.

---

## 4.3 Presentation

Owns Unity-facing behavior.

Examples:

- PackageView
- BlockerView
- BoardPresenter
- input picking
- temporary move animation
- camera setup
- scene composition

Presentation reads results from gameplay/domain systems.

Presentation must never determine puzzle legality.

---

# 5. ASSEMBLY DEPENDENCIES

Use only useful assembly boundaries.

Target minimum:

```text
ParcelEscape.Core
ParcelEscape.Gameplay
ParcelEscape.Tests
```

Dependency direction:

```text
ParcelEscape.Core
        ↑
ParcelEscape.Gameplay

ParcelEscape.Tests -> ParcelEscape.Core
```

Rules:

- `ParcelEscape.Core` must not reference `ParcelEscape.Gameplay`.
- `ParcelEscape.Core` must remain Unity-presentation independent.
- `ParcelEscape.Tests` should test Core directly.
- Do not create dozens of assemblies.
- Test assemblies must be configured correctly for Unity Test Framework.
- Prefer Edit Mode tests for pure logic.

If existing repository architecture already uses different assembly names or boundaries, preserve it unless a change is necessary and justified.

---

# 6. PROJECT AUDIT BEFORE MODIFICATION

Before implementing Phase 0/1, inspect and report:

- Unity Editor version;
- render pipeline;
- scenes;
- packages;
- folder structure;
- scripts;
- namespaces;
- assembly definitions;
- Input System status;
- Unity Test Framework availability;
- Android build support/configuration;
- package identifier;
- minimum Android API;
- target Android API;
- locally installed Android platforms if detectable;
- scripting backend;
- CPU architectures;
- orientation;
- build format configuration if detectable;
- Git repository status;
- existing gameplay code/assets.

Do not assume the project is empty.

Do not overwrite working configuration merely to match this document.

---

# 7. PRODUCTION TARGET

Intended production target:

- Unity 6 LTS-compatible;
- URP;
- Android;
- portrait orientation;
- Android App Bundle for store release;
- IL2CPP;
- ARM64;
- Google Play target API 36 or newer;
- mobile-first performance.

Only make safe configuration changes that can be verified.

Report separately:

1. configured Android target SDK;
2. highest locally installed Android platform if detectable;
3. whether the current environment can actually build against the configured target.

If API 36 or required Android tooling is unavailable locally, report exactly what is missing.

Never fabricate a successful Android configuration or build.

---

# 8. PROJECT STRUCTURE

Create missing directories under:

```text
Assets/_Game/
├── Art/
├── Audio/
├── Materials/
├── Prefabs/
├── Scenes/
├── ScriptableObjects/
├── Resources/
├── Tests/
└── Scripts/
    ├── Core/
    ├── Grid/
    ├── Gameplay/
    ├── Levels/
    ├── Solver/
    ├── Economy/
    ├── Progression/
    ├── Save/
    ├── UI/
    ├── Ads/
    ├── IAP/
    ├── Analytics/
    ├── Audio/
    └── Utilities/
```

Do not populate every folder with boilerplate.

The directory structure reserves organization space only.

Do not use `Resources.Load` merely because a `Resources` directory exists. Prefer serialized references unless there is a real Phase 1 need.

---

# 9. CORE DOMAIN TYPES

Implement the minimum required deterministic types.

## GridPosition

Integer coordinate value object.

Requirements:

- X;
- Y;
- value semantics;
- equality;
- hashing;
- readable `ToString`;
- no floating-point coordinates.

---

## PackageDirection

Enum:

```text
Up
Down
Left
Right
```

---

## PackageColor

Initial values:

```text
Red
Blue
Green
Yellow
Purple
Orange
```

Logical package identity must not depend on Unity Material or renderer color.

---

## CellOccupancy

Represent whether a logical cell contains:

```text
Empty
Package
Blocker
```

Choose a simple deterministic representation.

---

## PackageState

Minimum:

- unique deterministic package ID;
- GridPosition;
- PackageDirection;
- PackageColor.

Do not use runtime random GUID generation as core level identity.

---

## BlockerState

Minimum:

- deterministic ID if useful;
- GridPosition.

---

# 10. BOARDSTATE

Implement a pure logical `BoardState`.

It owns:

- Width;
- Height;
- Packages;
- Blockers.

It must provide deterministic queries such as:

- `IsInside(GridPosition)`;
- `IsOccupied(GridPosition)`;
- `IsBlocked(GridPosition)`;
- `TryGetPackageAt(GridPosition)`;
- lookup by package ID;
- path-clear/escape query.

Collections exposed by `BoardState` must not permit uncontrolled external mutation.

State mutation must occur through explicit domain methods.

---

## 10.1 Board Invariants

A level/board must reject invalid data rather than silently repairing it.

Validate:

- width > 0;
- height > 0;
- every package is inside bounds;
- every blocker is inside bounds;
- package IDs are unique;
- blocker IDs are unique if IDs exist;
- package positions are unique;
- blocker positions are unique;
- no package shares a cell with a blocker.

Malformed authored content should fail clearly during development.

---

# 11. CORE ESCAPE RULE

For a package to escape, inspect only the cells strictly between the package and the corresponding board edge.

Example:

Package at `(1,2)`, facing Right, board width = 5.

Inspect:

```text
(2,2)
(3,2)
(4,2)
```

If all are empty, the package can escape.

If any contains a package or blocker, it cannot escape.

The package's own cell is never treated as an obstruction.

An obstruction behind a package must not block escape.

Example:

```text
Package facing Right at (2,2)
Blocker at (1,2)
```

The blocker is behind the package and therefore irrelevant.

A package already adjacent to its exit edge has zero cells to inspect and is valid if no other rule forbids it.

---

# 12. MOVE MODEL

Create a minimal command/result model.

## MoveCommand

Contains:

- `PackageId`

---

## MoveValidationStatus

Use only statuses currently required.

Recommended:

```text
Valid
PackageNotFound
Blocked
```

Do not add future game-state statuses unless current implementation requires them.

---

## MoveValidationResult

Must clearly expose whether validation succeeded and why it failed.

Avoid string-only result contracts.

---

## MoveExecutionResult

A successful execution should preserve the escaped package's logical data.

Minimum useful information:

- execution status;
- escaped `PackageState` when successful.

Do not reduce move execution to a simple `bool`.

The escaped package result will become the clean integration seam for Phase 2 delivery routing.

---

# 13. MOVE VALIDATOR

Implement pure logical `MoveValidator`.

Responsibilities:

1. find package by ID;
2. inspect only cells in the package's escape ray;
3. return deterministic validation result.

It must not:

- mutate board state;
- animate;
- play sound;
- access GameObjects;
- inspect scene objects;
- access UI;
- access physics.

---

# 14. MOVE EXECUTOR

For Phase 1, a valid move must:

1. validate the requested package;
2. explicitly remove it from BoardState;
3. return a result containing the escaped package.

A blocked/invalid move must leave BoardState unchanged.

A package may be removed exactly once.

Do not implement:

- delivery trucks;
- holding queue;
- scoring;
- coins;
- victory;
- failure;
- progression.

---

# 15. AUTHORITATIVE-STATE RULE

`BoardState` is the authority for occupancy and move legality.

Never derive logical state from:

- PackageView existence;
- Transform position;
- collider overlap;
- animation state;
- scene hierarchy.

For a valid move:

```text
Validate logical state
        ↓
Mutate logical state
        ↓
Return/publish MoveExecutionResult
        ↓
Animate the already-resolved outcome
```

If an animation:

- fails;
- is skipped;
- is interrupted;
- is disabled;
- ends early;

the logical move remains completed.

Presentation reconciles with domain state. Presentation must not roll domain state backward.

---

# 16. GAME SESSION

Create the minimum runtime orchestration needed.

A `GameSession`-style object may own current board state.

Possible responsibilities:

- LoadLevel;
- TryMovePackage;
- Restart.

Keep responsibilities narrow.

Do not turn it into:

- service locator;
- scene manager;
- audio manager;
- ad manager;
- economy manager;
- save manager;
- UI manager.

No global singleton.

---

# 17. LEVEL DEFINITION

Introduce only the minimum level authoring abstraction required for the Phase 1 development level.

It should support:

- width;
- height;
- package definitions;
- blocker definitions.

Do not implement:

- trucks;
- holding slots;
- difficulty metadata;
- procedural generation;
- solver metadata;
- monetization metadata.

If a ScriptableObject is used for authoring, convert authored Unity content into pure runtime data before simulation begins.

The Core simulation must not depend on the ScriptableObject.

---

# 18. DEVELOPMENT LEVEL

Create one deterministic 5 × 5 development level.

Use these logical semantics unless repository constraints justify a small adjustment.

Coordinate system uses bottom-left `(0,0)`.

Recommended arrangement:

```text
Board: 5 × 5

Blue
Position: (2,3)
Direction: Up
Expected: valid escape because (2,4) is clear.

Red
Position: (0,2)
Direction: Right

Green
Position: (2,2)
Direction: Left

Red and Green create package-against-package blocking.

Yellow
Position: (1,3)
Direction: Down

Blocker
Position: (1,1)

Yellow is blocked by the fixed blocker.
```

This level must demonstrate:

1. valid escape;
2. blocked package;
3. package-against-package blocking;
4. fixed blocker blocking.

Initialization must always be identical.

No runtime random level generation.

---

# 19. 3D BOARD PRESENTATION

Create a lightweight development presentation.

Target:

- 5 × 5 warehouse-style board;
- visible cells;
- top-down/isometric 2.5D composition;
- portrait-friendly layout;
- simple package objects;
- clearly visible direction arrows;
- blocker object.

Placeholder primitives are acceptable.

Do not make final art.

Avoid expensive graphics.

---

# 20. CAMERA

Create a fixed mobile puzzle camera.

Requirements:

- portrait composition;
- elevated top-down/isometric angle;
- entire board visible;
- no player camera control;
- no pinch zoom;
- no rotation;
- no dragging.

Leave reasonable future composition space below the board for:

- truck area;
- holding queue;
- boosters.

Do not implement those systems yet.

---

# 21. PACKAGE VIEW

Create a lightweight `PackageView`.

Responsibilities:

- represent one logical package;
- show package color;
- show fixed direction;
- accept tap/click;
- forward package ID to gameplay/session.

`PackageView` must never decide whether it can move.

---

# 22. INPUT

Touch is the production interaction.

Mouse may work in Editor for convenience.

Acceptable flow:

```text
Touch / Mouse
      ↓
Presentation input picking
      ↓
PackageView
      ↓
PackageId
      ↓
GameSession
      ↓
Core validation/execution
```

Physics/colliders may be used only as a presentation-layer input-picking mechanism.

Physics must never determine:

- occupancy;
- blocking;
- escape path;
- logical move validity;
- game outcome.

Avoid putting gameplay rules directly inside input callbacks.

---

# 23. INTERACTION STATE

Prevent accidental duplicate move execution.

Minimal runtime state is sufficient:

```text
Ready
ResolvingMove
```

During a valid escape presentation:

- temporarily reject duplicate move requests if necessary;
- return to `Ready` after presentation resolution.

A blocked tap should require little or no global lock.

Do not build the complete future game state machine.

---

# 24. MOVE FEEDBACK

## Valid Move

Logical resolution happens first.

Then presentation may perform a temporary animation such as:

1. slight squash/compression;
2. translate package in its fixed direction;
3. move fully off board;
4. hide/recycle the visual.

Do not let animation determine success.

---

## Blocked Move

Do not mutate BoardState.

Temporary feedback may include:

- small shake;
- bump;
- brief highlight.

No modal popup.

Repeated blocked taps must remain safe.

---

# 25. OBJECT LIFECYCLE

Avoid unnecessary Instantiate/Destroy churn.

Phase 1 may use a simple implementation, but `PackageView` lifecycle should not prevent future pooling.

Do not build a large pooling framework yet.

---

# 26. SCENES

## Gameplay Scene

Required if no equivalent scene already exists.

Expected runtime result:

- 5 × 5 board visible;
- packages in authored cells;
- blockers visible;
- package directions readable;
- valid package escapes;
- blocked package stays;
- repeated tapping does not corrupt logical state.

---

## Bootstrap Scene

Do not create a Bootstrap scene solely because it is a common architecture convention.

Create or preserve one only if the existing repository/application lifecycle genuinely needs it.

Phase 1 Gameplay may run directly if safe.

Do not initialize:

- ads;
- IAP;
- analytics;
- Firebase;
- cloud services.

---

## Main Menu

Do not build the final menu.

If navigation absolutely requires a placeholder, keep it minimal.

Do not add:

- shop;
- daily rewards;
- profile;
- settings architecture;
- level map.

---

# 27. UNITY SERIALIZATION SAFETY

Do not blindly hand-author complex Unity serialization.

Avoid manually fabricating complex:

- `.unity`;
- `.prefab`;
- `.asset`;
- `.meta`

YAML unless there is no safer mechanism and the serialized format is verified.

Prefer:

- Unity Editor operations;
- existing project editor tooling;
- Unity-generated serialization;
- small safe code-based setup where appropriate.

Never guess Unity GUID relationships.

Never replace an existing `.meta` file casually.

---

# 28. AUTOMATED TESTS

Pure simulation tests should normally be Edit Mode tests.

Do not test deterministic board rules through scene objects.

Minimum test coverage:

## Grid

- inside board;
- outside board;
- negative coordinates;
- upper boundary.

## Occupancy

- package occupies cell;
- blocker occupies cell;
- empty cell is empty.

## Escape Up

- clear path = valid;
- blocked by package = invalid;
- blocked by blocker = invalid.

## Escape Down

- same cases.

## Escape Left

- same cases.

## Escape Right

- same cases.

## Edge Package

Package adjacent to boundary with zero cells ahead is valid.

## Invalid Package ID

Fails safely.

## State Mutation

Blocked move:

- package remains;
- board state unchanged.

Valid move:

- package removed exactly once.

## Directional-Ray Correctness

An obstruction behind a package must not block it.

An obstruction on the same row/column but outside the forward escape ray must not block it.

## Determinism

Given identical initial BoardState and identical MoveCommand:

- validation result is identical;
- execution result is identical;
- resulting logical board is identical.

---

# 29. EDIT MODE VS PLAY MODE

Prefer Edit Mode tests for:

- GridPosition;
- BoardState;
- MoveValidator;
- MoveExecutor;
- runtime level conversion where pure.

Use Play Mode tests only for Unity runtime behavior that genuinely requires Unity objects.

Examples where Play Mode may be justified:

- PackageView forwarding package ID;
- scene initialization;
- animation/input integration.

Do not add Play Mode tests merely for coverage numbers.

---

# 30. PERFORMANCE RULES

The board is small. Architecture should remain simple.

Do not introduce:

- `Update()` on every package without necessity;
- runtime scene-wide `Find*` calls in gameplay;
- physics-based puzzle logic;
- expensive realtime lighting;
- excessive transparent materials;
- per-frame allocations;
- LINQ in hot gameplay paths;
- unnecessary coroutines;
- unnecessary Instantiate/Destroy loops;
- avoidable garbage generation from repeated input.

Optimize for stable Android frame pacing and low complexity.

---

# 31. CODE QUALITY

Requirements:

- namespaces;
- clear names;
- focused classes;
- explicit responsibilities;
- no unexplained magic values;
- no giant MonoBehaviour controller;
- no direct UI dependency in Core;
- no unnecessary third-party framework;
- error-safe handling;
- comments that explain architectural intent rather than restating code.

Do not create abstraction for abstraction's sake.

Prefer boring, deterministic, testable code.

---

# 32. ANDROID CONFIGURATION CHECK

Inspect actual Player Settings and build configuration.

Production objective:

```text
Orientation: Portrait
Scripting Backend: IL2CPP
Architecture: ARM64
Target API: 36+
Build Format: Android App Bundle for store release
```

Do not break local development just to force a production setting when required Android components are missing.

If a setting cannot safely be changed from project files, document the exact Unity Editor action required.

Never claim a build was successful unless it was actually executed successfully.

---

# 33. VERIFICATION EVIDENCE

Verification claims must be evidence-based.

If Unity Editor or Unity CLI is unavailable:

- do not claim Unity compilation succeeded;
- do not claim Edit Mode tests passed;
- do not claim Play Mode verification passed;
- separate static/code inspection from runtime verification;
- report the exact remaining Editor checks.

The final phase gate must reflect actual verified state, not implementation intent.

---

# 34. PROJECT_STATE.md

Create or update repository-root:

```text
PROJECT_STATE.md
```

Required sections:

```text
# Project
Parcel Escape: Sort & Deliver

# Current Phase
Phase 0 + Phase 1

# Baseline
- original Git SHA
- branch
- Unity version
- render pipeline
- relevant packages

# Architecture
- domain/runtime/presentation separation
- assembly dependency direction

# Scenes
- scenes and purposes

# Logical Systems
- completed systems

# Tests
- test categories
- execution results

# Android Configuration
- package identifier
- orientation
- minimum SDK
- target SDK
- locally available SDK status if detectable
- scripting backend
- architectures
- build format

# Known Issues
Only real unresolved issues.

# Deferred Systems
- trucks
- holding queue
- victory/failure
- solver
- generator
- economy
- ads
- IAP
- analytics
- final visual art

# Next Phase
Phase 2 — Full Puzzle Loop
```

Keep this file factual.

---

# 35. DO NOT IMPLEMENT YET

Do not implement during Phase 0/1:

- truck gameplay;
- active truck logic;
- holding queue;
- auto-loading;
- win condition;
- fail condition;
- solver;
- procedural generator;
- 120 levels;
- coins;
- stars;
- boosters;
- daily rewards;
- shop;
- cosmetics;
- ads;
- AdMob;
- purchases;
- Google Play Billing;
- analytics;
- Firebase;
- remote config;
- cloud save;
- accounts;
- profiles;
- final graphics;
- live ops.

Do not create fake placeholder services for these systems.

---

# 36. PHASE 0 + PHASE 1 ACCEPTANCE CRITERIA

The phase is complete only if:

1. repository audit completed;
2. project architecture documented;
3. Unity project compiles without new errors;
4. deterministic 5 × 5 development board loads;
5. packages render in authored cells;
6. arrows/directions are clearly visible;
7. valid package escapes in correct direction;
8. blocked package remains;
9. fixed blockers are respected;
10. package-to-package blocking works;
11. blocked taps do not mutate state;
12. repeated interaction does not create duplicate moves;
13. Core puzzle logic has no physics dependency;
14. `ParcelEscape.Core` has no Unity presentation dependency;
15. all four directions are unit-tested;
16. state mutation tests pass;
17. determinism tests pass;
18. Android configuration is audited;
19. `PROJECT_STATE.md` is current;
20. no later-phase feature was prematurely implemented;
21. Git diff contains no accidental unrelated changes.

---

# 37. FINAL VERIFICATION PROCEDURE

Before declaring completion:

1. inspect `git status`;
2. compile Unity scripts;
3. run Edit Mode tests;
4. run relevant Play Mode tests if any;
5. inspect Unity Console for errors;
6. verify the development level manually where tooling allows;
7. confirm valid escape;
8. confirm blocked-by-package behavior;
9. confirm blocked-by-blocker behavior;
10. confirm repeated blocked taps are safe;
11. inspect final Git diff;
12. confirm no unrelated files changed;
13. confirm no generated cache/temp directories are tracked.

Do not silently skip failed verification steps.

---

# 38. PHASE GATE

Declare:

```text
PHASE 0 + PHASE 1: READY
```

only when the required runtime verification has actually succeeded.

At minimum, READY requires:

- Unity compilation verified;
- Edit Mode tests executed and passing;
- development gameplay runtime verified;
- Git diff inspected;
- no acceptance-blocking issue remains.

If implementation is complete but Unity runtime verification cannot be performed, declare:

```text
PHASE 0 + PHASE 1: NOT READY
```

and state exactly which checks remain.

Never use READY as a synonym for "code written."

---

# 39. FINAL REPORT FORMAT

At completion provide:

## Baseline

- branch;
- starting Git SHA;
- Unity version.

## Audit

- render pipeline;
- packages;
- scenes;
- input;
- tests;
- Android configuration.

## Created

List files.

## Modified

List files.

## Architecture

Explain the implemented separation between:

- Core/domain;
- runtime orchestration;
- Unity presentation.

## Gameplay Result

State exactly what works.

## Tests

Report:

- executed;
- passed;
- failed;
- skipped/not executable.

## Android

Report actual detected configuration.

## Issues

List only real unresolved issues.

## Git Status

Report changed/untracked files.

## Phase Gate

Use exactly one:

```text
PHASE 0 + PHASE 1: READY
```

or:

```text
PHASE 0 + PHASE 1: NOT READY
```

with concrete reasons.

## Next Recommended Work

Describe **Phase 2 — Full Puzzle Loop** only.

Do not start Phase 2 automatically.

---

# 40. EXPECTED PHASE 1 DATA FLOW

Preferred conceptual flow:

```text
LevelDefinitionAsset
        ↓
LevelDefinitionConverter
        ↓
Pure Runtime Level Data
        ↓
BoardState
        ↓
GameSession
        ↓
MoveCommand
        ↓
MoveValidator
        ↓
MoveExecutor
        ↓
MoveExecutionResult
        ↓
BoardPresenter / PackageView animation
```

This is conceptual guidance, not permission to overwrite an existing equivalent architecture.

The key invariant is:

**domain state resolves gameplay first; Unity presents the result second.**

---

# 41. FUTURE COMPATIBILITY

Phase 1 architecture should naturally allow Phase 2 to extend this flow:

```text
BoardState
    ↓
Escaped Package
    ↓
Delivery Resolver
    ↓
Active Truck / Holding System
```

Do not implement that flow yet.

The only Phase 1 responsibility is to preserve enough information in `MoveExecutionResult` that the next phase does not require rewriting the escape system.

---

# 42. PRIORITY ORDER WHEN MAKING DECISIONS

When requirements compete, use this order:

1. Preserve repository/user work.
2. Preserve deterministic puzzle correctness.
3. Preserve domain/presentation separation.
4. Preserve compile/test integrity.
5. Preserve Android/mobile performance.
6. Preserve implementation simplicity.
7. Preserve future extensibility where it costs little today.
8. Visual polish last during Phase 1.

Do not sacrifice deterministic correctness for visual convenience.


# 43. AUTOMATIC MULTI-AGENT ORCHESTRATION

# Automatic Multi-Agent Orchestration

The main Codex session is the **Project Orchestrator** for Parcel Escape: Sort & Deliver.

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

1. **Inspect ground truth** — read `AGENTS.md`, `PROJECT_STATE.md` if present, Git status, relevant files, asmdefs, scenes/settings.
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
2. root `AGENTS.md`;
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

The main Codex session must not simply paste subagent answers.
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
