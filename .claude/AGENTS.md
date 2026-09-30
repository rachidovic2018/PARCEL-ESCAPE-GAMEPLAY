# Parcel Escape — Claude Code Agents

Repository-root `CLAUDE.md` remains the main authority.

## Roster
1. Technical Director — architecture/scope/phase gates
2. Core Gameplay Engineer — deterministic domain logic
3. Unity Gameplay Engineer — runtime and presentation integration
4. Puzzle & Level Designer — level content and progression
5. Solver Engineer — solvability/deadlock/difficulty tooling
6. Mobile UX Designer — portrait interaction and readability
7. Technical Art Director — 3D style and rendering performance
8. Android & Performance Engineer — build/release/performance
9. Monetization & Economy Designer — later F2P systems
10. QA / Release Engineer — validation and release gates

## Phase Usage
### Phase 0 + 1
Primary: Technical Director, Core Gameplay, Unity Gameplay, QA, Android/Performance. Support: Level Designer.

### Phase 2
Add Mobile UX and stronger Level Design involvement.

### Solver / Content Production
Core Gameplay + Solver + Level Designer + QA.

### Commercial Polish / Release
Technical Director + Unity Gameplay + UX + Technical Art + Android/Performance + Monetization + QA.

## Conflict Resolution
1. Preserve user/repository work.
2. Preserve deterministic correctness.
3. Preserve Core/presentation separation.
4. Preserve compile/test integrity.
5. Preserve Android performance.
6. Prefer the simplest implementation satisfying the current phase.
