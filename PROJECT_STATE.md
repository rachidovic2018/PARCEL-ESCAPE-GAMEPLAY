# Project
Parcel Escape: Sort & Deliver

# Current Phase
Phase 0 + Phase 1

# Baseline
- Branch: `main`
- Current checkpoint: `fb60caefcc390a2ba6c1dca7ac9f3245fd7d6a1f` (`Add touch-first package input`)
- Unity: `6000.3.24f1`
- Universal Render Pipeline: resolved `17.3.0`
- Input System: `1.11.2`

# Rendering
- `ParcelEscapeMobileURP` is assigned in Graphics Settings and all six Quality levels.
- The mobile profile uses render scale 1, 2x MSAA, no HDR, no depth/opaque textures, no additional lights, and a single shadow cascade.
- Gameplay board cells use `BoardCell_URP`; runtime package, arrow, and blocker materials use URP Simple Lit.

# Architecture
- `ParcelEscape.Core` owns deterministic board state, validation, and execution without Unity presentation dependencies.
- `GameSession` owns gameplay interaction eligibility and delegates legal moves to Core.
- Gameplay presentation performs input picking, forwards only `PackageId`, and animates resolved results.

# Scenes
- `Assets/_Game/Scenes/Gameplay.unity` is enabled in Build Settings.
- The scene presents a fixed portrait-friendly 5x5 board with 25 cells, four packages, readable direction arrows, and one blocker.

# Logical Systems
- `DevLevel_5x5` is deterministic and demonstrates a valid escape, package-to-package blocking, and fixed-blocker blocking.
- Touch-first Input System picking is active with an Editor mouse fallback.
- Valid moves resolve once; blocked and duplicate requests do not corrupt logical state.

# Tests
- Edit Mode: 49 passed, 0 failed.
- Play Mode: 3 passed, 0 failed.
- Play Mode covers scene rendering, URP-compatible materials, valid/blocked moves, duplicate protection, virtual mouse input, and virtual touch input.
- Physical Android-device input has not been verified.

# Android Configuration
- Package identifier: `com.parcelescape.sortanddeliver`
- Orientation: Portrait only
- Minimum SDK: API 25 (preserved)
- Target SDK: API 36
- Scripting backend: IL2CPP
- Architectures: ARM64 only
- Android App Bundle: enabled in the current Editor build state
- Active build target: StandaloneOSX; Android target switching is unavailable without Android Build Support.
- Highest detected external Android platform: API 37
- Unity Android Build Support, SDK/NDK Tools, and OpenJDK modules are not installed for `6000.3.24f1`; no Android build was attempted.

# Known Issues
- Android build and physical-device verification remain blocked until the matching Unity Android modules are installed.

# Deferred Systems
- Trucks and holding queue
- Victory/failure flow
- Solver and level generator
- Economy, ads, IAP, and analytics
- Final visual art

# Next Phase
Phase 2 — Full Puzzle Loop
