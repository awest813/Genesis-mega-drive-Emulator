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
| Super Street Fighter II | Sega mapper banks (`0xA130F3-0xA130FF`) | pending | Pending manual | Unit/integration tests cover bank switching; needs ROM boot verification |
| Any SRAM-enabled title | SRAM gate (`0xA130F1`) + persistence | 2026-06-08 | Automated | `SramTests`, `BusRoutingTests`, `DmaTests`; periodic `.srm` autosave |

## Known Issues

| Area | Issue | Status |
|---|---|---|
| Mapper coverage | Mapper bank switching covered by unit/bus tests; ROM boot matrix still pending for SSF2 | Open |
| Save states | v6 round-trip covered by `SaveStateTests`; manual in-game verify still recommended | Partial |
| Timing | PAL 50 Hz frame loop and 313-line scanline count wired to VDP TV-mode setting; deeper per-subsystem timing conformance remains in progress | Partial |
| SMD ROM format | `.smd` interleaved images deinterleaved on load (Sandopolis-derived) | Automated (`CartridgeTests`) |
