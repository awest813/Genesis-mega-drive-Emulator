# Architecture Overview

This document describes the current architecture of the Genesis/Mega Drive emulator and the planned direction for future development.

## Hardware Being Emulated

The Sega Genesis (Mega Drive) contains the following major components, all of which are emulated:

```
┌─────────────────────────────────────────────────────────┐
│                    System Bus (md_bus)                   │
│                  Bus Arbiter: 315-5308                   │
├─────────┬──────────┬───────┬────────┬──────┬────────────┤
│ MC68000 │   Z80    │  VDP  │ YM2612 │ PSG  │ Controller │
│ (main)  │ (sound)  │(video)│  (FM)  │(SN76 │    I/O     │
│         │          │       │  489)  │      │            │
└─────────┴──────────┴───────┴────────┴──────┴────────────┘
```

## Source File Map

### Emulation Core

| File | Component | Hardware Reference |
|------|-----------|-------------------|
| `md_m68k.cs` | Motorola MC68000 CPU | MC68000 User's Manual |
| `md_m68k_memory.cs` | 68K memory (ROM + 64KB RAM) | — |
| `md_m68k_addressing.cs` | 68K addressing modes | — |
| `md_m68k_sub.cs` | 68K helper functions | — |
| `md_m68k_initialize.cs` | 68K opcode table setup | — |
| `md_m68k_initialize2.cs` | 68K opcode table (generated) | — |
| `md_m68k_state.cs` | 68K state save/restore | — |
| `opc/md_m68k_ope*.cs` | Individual 68K instructions | — |
| `md_z80.cs` | Zilog Z80 CPU | Z80 User Manual |
| `md_z80_operand.cs` | Z80 operand decoding | — |
| `md_z80_operand_2.cs` | Z80 extended operands | — |
| `md_z80_operand_sub.cs` | Z80 operand helpers | — |
| `md_z80_memory.cs` | Z80 memory (8KB RAM) | — |
| `md_z80_initialize.cs` | Z80 opcode table setup | — |
| `md_z80_state.cs` | Z80 state save/restore | — |
| `md_vdp.cs` | Video Display Processor | Genesis Technical Overview |
| `md_vdp_regster.cs` | VDP register handling | — |
| `md_vdp_memory.cs` | VRAM, CRAM, VSRAM | — |
| `md_vdp_dma.cs` | VDP DMA transfers | — |
| `md_vdp_renderer.cs` | VDP rendering coordinator | — |
| `md_vdp_renderer_line.cs` | Scanline rendering | — |
| `md_vdp_renderer_data.cs` | Renderer data structures | — |
| `md_vdp_renderer_snap.cs` | Renderer snapshot | — |
| `md_vdp_renderer_frame_directx.cs` | DirectX 12 frame output | — |
| `md_vdp_renderer_frame_directx_sub.cs` | DirectX helpers | — |
| `md_vdp_initialize.cs` | VDP initialization | — |
| `md_vdp_state.cs` | VDP state save/restore | — |
| `md_music.cs` | Audio coordinator | — |
| `md_music_ym2612_core.cs` | YM2612 FM synthesis | YM2608 OPNA Manual |
| `md_music_ym2612_regster.cs` | YM2612 registers | — |
| `md_music_ym2612_init.cs` | YM2612 initialization | — |
| `md_music_ym2612_document.cs` | YM2612 documentation | — |
| `md_music_sn76489_core.cs` | SN76489 PSG | SN76489 User Manual |
| `md_music_sn76489_register.cs` | PSG registers | — |
| `md_music_state.cs` | Audio state save/restore | — |
| `md_bus.cs` | System bus arbiter (315-5308) | — |
| `md_cartridge.cs` | ROM loading + header parsing | — |
| `md_sram.cs` | Battery-backed cartridge SRAM (`.srm`) | — |
| `md_io.cs` | Controller I/O ports | — |
| `md_io_device.cs` | I/O device abstraction | — |
| `md_io_replay.cs` | Input recording/replay | — |
| `md_control.cs` | System control registers | — |

### System Coordination

| File | Purpose |
|------|---------|
| `md_main.cs` | Main emulation loop and subsystem orchestration |
| `md_main_initialize.cs` | System initialization |
| `md_main_setting.cs` | Settings management |
| `md_main_state_capture.cs` | Save state implementation |
| `md_main_state_capture_request.cs` | Save state request handling |
| `md_main_input_capture.cs` | Input recording storage |
| `md_main_input_capture_request.cs` | Input recording request handling |

### Frontend (WinForms)

| File | Purpose |
|------|---------|
| `Form_Main.cs` | Main window, game screen, menu |
| `Form_Code.cs` | Disassembly/code view |
| `Form_Code_Trace.cs` | Execution tracing |
| `Form_Code_Analyse.cs` | Code analysis |
| `Form_Code_View.cs` | Code display |
| `Form_Code_dock.cs` | Dockable code panels |
| `Form_Registry.cs` | CPU/VDP register display |
| `Form_VDP_Screen.cs` | VDP layer viewer |
| `Form_Pattern.cs` | Pattern/tile viewer |
| `Form_Pallete.cs` | Palette viewer |
| `Form_IO.cs` | I/O state display |
| `Form_IO_Setting.cs` | Input configuration |
| `Form_MUSIC.cs` | Audio channel viewer |
| `Form_Flow.cs` | Call flow diagram |
| `Form_Setting.cs` | Settings dialog |
| `Form_About.cs` | About dialog |
| `Form_Main_AviRecorder.cs` | Video recording |
| `Form_Main_Capture_list.cs` | State/input capture list |

### Development Tools

| File | Purpose |
|------|---------|
| `opcode_make/` | MC68000 opcode lookup table generator (build-time tool) |

## Memory Map

The Genesis memory map as implemented in `md_bus.cs`:

```
0x000000 - 0x3FFFFF  ROM (cartridge, up to 4MB)
                     └─ battery-backed SRAM overlays its declared window
                        (header 'RA' marker), gated by 0xA130F1 bit 0
0xA00000 - 0xA0FFFF  Z80 address space (8KB RAM + bank register)
0xA04000 - 0xA04003  YM2612 FM sound chip
0xA10000 - 0xA10FFF  I/O ports (controllers)
0xA11000 - 0xA1FFFF  System control registers (active)
0xA14000 - 0xA14003  TMSS (Trademark Security System)
0xC00000 - 0xDFFFFF  VDP (data port, control port, HV counter, PSG)
0xFF0000 - 0xFFFFFF  68K RAM (64KB, mirrored)
```

## Frame Execution Model

Each frame follows this sequence (in `md_main.md_run()`):

1. Process hard reset if requested
2. Handle pause / frame advance
3. Update input state
4. Process save state requests
5. For each scanline (262 lines NTSC):
   - Run VDP for the line (rendering + interrupt generation)
   - Run MC68000 for 488 master clocks
   - Run Z80 for 228 clocks
   - Run audio synthesis for 488 master clocks
6. Wait for frame timing (~16.67ms target for 60 FPS)

## Current Limitations

- **Tight UI coupling:** The emulation core (`md_main`, `md_bus`, CPU classes) directly references WinForms UI classes for tracing and debug display
- **Global state:** Most subsystems are accessed via static fields on `md_main`
- **Windows-only:** SharpDX (Direct3D 12) for rendering, DirectInput for gamepads, WinForms for UI
- **Limited automated coverage:** Core CPU/memory/SRAM/mapper behavior has tests, but broad timing and compatibility regression coverage is still in progress

## Phase 2 Milestone Checkpoint (Current Work)

### Locked Scope

1. Memory mapper controller support (`0xA13000+`, including 8/16/32-bit control-path writes)
2. Timing-accuracy baselines for key CPU/frame constants and instruction-cycle checks
3. Compatibility regression tracking via a maintained matrix of known-good titles and known issues

### Acceptance Criteria

- **Mapper behavior complete**
  - Mapper register writes correctly update 512 KB bank mappings for banks 1-7
  - SRAM gate (`0xA130F1`) and mapper bank controls coexist correctly
  - Save states preserve and restore mapper bank register state on new captures
- **Timing baselines stable**
  - Core timing baseline tests pass (per-line CPU/Z80 constants and key instruction timing checks)
  - No accidental drift in established baseline constants without intentional updates
- **No regressions on currently working titles**
  - Compatibility matrix is updated and reviewed after core changes
  - Known-good entries remain bootable/runnable within existing expectations

### Compatibility Tracking Artifact

- Lightweight matrix: [docs/COMPATIBILITY_MATRIX.md](COMPATIBILITY_MATRIX.md)
- Use it as the regression checklist after mapper/timing/core bus changes.

## Phase 3 Preparation — UI Coupling (In Progress)

The following coupling points are being untangled before extracting a standalone core.

### Completed seams

1. **`IM68kTracer` no longer references `Form_Code_Trace`** — `STACK_LIST_TYPE` has been
   moved to `M68kStackEntryType` in `md_m68k_interfaces.cs`. The interface and both null
   and production implementations now have zero UI dependency. `Form_Code_Trace` keeps
   internal constants for its own methods and `STACK_LIST` struct, which still lives there.

2. **`md_bus` no longer calls `md_main.g_form_code` directly** — `IBusMonitor` /
   `NullBusMonitor` introduced in `md_bus_interfaces.cs`. `md_bus.g_monitor` is injected
   at startup (defaults to `NullBusMonitor`); production wires in `Form_Code`.
   Every read/write hook now routes through the interface.

3. **`md_main.cs` emulation loop no longer directly drives `Form_*` instances** —
   `IMainLoopUiHooks` / `NullMainLoopUiHooks` introduced in `md_main_ui_hooks.cs`.
   `md_main` now invokes UI actions through the injected hook, while production wires
   in `WinFormsMainLoopUiHooks` during initialization.

### Remaining coupling hotspots

1. `md_main` still owns WinForms instances as global static state (`g_form_*`) used by
   settings and tool windows outside the core execution loop.

## Development Roadmap

### Phase 1 — Core Stability
- Boot and run commercial ROMs reliably with correct audio, video, and input
- Establish CI pipeline and build verification
- Add initial test coverage for CPU instructions and bus routing

### Phase 2 — Feature Completeness
- SRAM / battery-backed save support — **done** (`md_sram.cs`), included in save states (v5)
- Memory mapper controller (0xA13000+)
- Timing accuracy improvements
- Expanded compatibility testing and regression tracking

### Phase 3 — Architecture Clean-Up (In Progress)
- Extract emulation core into a standalone library (no UI dependencies)
- Define clean interfaces between subsystems (CPU, bus, VDP, audio, I/O)
- Make tracer/debugger/disassembler optional modules that attach to the core
- Create a minimal game-playing frontend separate from the debug tools
- **Done:** `M68kStackEntryType` moved to core interfaces; `IBusMonitor` injected into `md_bus`
- **Done:** `md_main` emulation-loop UI calls moved behind `IMainLoopUiHooks`
- **Remaining:** remove `md_main` global `g_form_*` ownership used by settings/tool windows

### Phase 4 — Platform Expansion
- Platform-independent rendering backend (replacing SharpDX)
- Cross-platform audio output (replacing NAudio/Windows-specific APIs)
- Cross-platform input handling
- Linux and macOS support

### Deferred (Out of Scope for Now)
- Sega 32X
- Sega CD
- Interlace mode
- PAL timing
