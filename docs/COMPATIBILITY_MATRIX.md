# Compatibility Matrix (Phase 2 Regression Tracking)

Use this matrix after core changes (mapper, bus routing, timing, CPU/VDP/audio synchronization) to quickly spot regressions.

## Known-Good Baseline Titles

| Region | Title | Last Check | Status | Notes |
|---|---|---|---|---|
| Japan | Space Harrier II | 2026-06-06 | Baseline | Boots and enters gameplay |
| Japan | Super Thunder Blade | 2026-06-06 | Baseline | Boots and playable |
| Japan | Sonic The Hedgehog | 2026-06-06 | Baseline | Boots and gameplay stable |
| Japan | Gunstar Heroes | 2026-06-06 | Baseline | Boots and gameplay stable |
| USA | Vectorman | 2026-06-06 | Baseline | Boots and gameplay stable |

## Mapper / SRAM Focus Cases

| Title | Feature Focus | Last Check | Status | Notes |
|---|---|---|---|---|
| Super Street Fighter II | Sega mapper banks (`0xA130F3-0xA130FF`) | pending | Pending | Validate bank switching paths after mapper changes |
| Any SRAM-enabled title | SRAM gate (`0xA130F1`) + persistence | pending | Pending | Confirm SRAM/ROM window toggling and `.srm` persistence |

## Known Issues

| Area | Issue | Status |
|---|---|---|
| Mapper coverage | Mapper-heavy titles are not yet fully tracked in automated ROM-based regression runs | Open |
| Timing | Baseline constants are covered, but deeper per-subsystem timing conformance remains in progress | Open |
