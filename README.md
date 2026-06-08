# Genesis / Mega Drive Emulator

A lightweight, focused Sega Genesis (Mega Drive) emulator written entirely in C# and .NET. Originally based on [MDTracer](https://github.com/sasayaki-japan/MDTracer) by sasayaki-japan, this project is being developed into a robust, standalone open-source emulator.

> **Note:** This program is not intended for playing games illegally. Its purpose is to help users understand and appreciate the ingenuity of the engineers who created this remarkable technology.

## Current Status

The emulator can boot and run a number of commercial Genesis/Mega Drive titles. The emulation core (`GenesisEmu.Core`) is a portable .NET 9 class library. Windows frontends use WinForms with optional Direct3D 12 GPU compositing via `GenesisEmu.Platform.Windows`:

- **`GenesisEmu.Game`** — minimal game-playing app (no debug tools)
- **`MDTracer`** — full developer frontend with tracer, disassembler, and VDP viewers

**Emulated Hardware:**
- Motorola MC68000 main CPU
- Zilog Z80 secondary CPU
- VDP (Video Display Processor) — scrolling planes, sprites, palette, DMA
- YM2612 FM synthesis sound chip
- SN76489 PSG (Programmable Sound Generator)
- Controller I/O (keyboard and gamepad input)

## Getting Started

### Prerequisites

- Windows 10 or later
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (or Visual Studio 2022 17.8+)

### Building

**Windows (game-only app):**

```bash
dotnet build GenesisEmu.Game/GenesisEmu.Game.csproj
```

**Windows (full app + debug tools):**

```bash
dotnet build MDTracer/MDTracer.csproj
```

**Portable core + tests (Linux/macOS/Windows):**

```bash
dotnet build GenesisEmu.Core/GenesisEmu.Core.csproj
dotnet test tests/GenesisEmu.Core.Tests/GenesisEmu.Core.Tests.csproj
```

Or open `MDTracer.sln` in Visual Studio 2022 and build from the IDE.

### Running

**Game-only (recommended for play):**

```bash
dotnet run --project GenesisEmu.Game
```

**Developer tools (tracer, VDP viewers, settings):**

```bash
dotnet run --project MDTracer
```

Press **Ctrl+O** or use the menu to open a ROM file (.bin / .md / .gen / .smd). You can also pass a ROM path on the command line with `GenesisEmu.Game`.

### Default Controls (Player 1)

| Button | Key |
|--------|-----|
| Start  | Space |
| A      | N |
| B      | M |
| C      | , |
| Up     | W |
| Down   | S |
| Left   | A |
| Right  | D |
| X      | H |
| Y      | J |
| Z      | K |
| Mode   | Return |

Remap keys and gamepad buttons (including player 2) via **Options → Controller Settings** in `GenesisEmu.Game`.

### Keyboard Shortcuts — GenesisEmu.Game

| Key | Action |
|-----|--------|
| Ctrl+O | Open ROM |
| Esc | Pause / Resume (exit fullscreen first if active) |
| F1 | Save State |
| F4 | Load Latest State |
| Ctrl+F4 | Save State List |
| F5 | Frame Advance |
| F10 | Screenshot |
| F11 | Fullscreen |
| Alt+Enter | Fullscreen |
| F12 | Reset |

### Keyboard Shortcuts — MDTracer (developer frontend)

Includes all game shortcuts above, plus:

| Key | Action |
|-----|--------|
| F2 | Input Recording Start/Stop |
| F3 | Save State + Input Recording Start/Stop |
| F4 | Restore Latest State + matching Input replay |
| Ctrl+F4 | Capture History (state + input) |
| F9 | Settings |
| F11 | Video Recording Start/Stop |

## Compatibility

The following commercially released titles have been verified to run:

<details>
<summary>Verified Titles (click to expand)</summary>

**Japan**
- Space Harrier II, Super Thunder Blade, Altered Beast, Phantasy Star
- Thunder Force II / III / IV, Ghouls'n Ghosts, Super Hang-On
- Super Shinobi, Tatsujin, Vermilion, Golden Axe III, Sorcerian
- After Burner II, Phantasy Star III, Phelios, Super Monaco GP
- Hellfire, Strider Hiryuu, FZ Senki Axis, Burning Force, Granada
- DARIUS (Sagaia), Musha Aleste, Sonic The Hedgehog, Bare Knuckle
- Kuuga, Castlevania - Bloodlines, Super Fantasy Zone
- Galaxy Force II, Panorama Cotton, Ex-Ranza, Dynamite Headdy
- Battle Mania Daiginjou, OutRun, Gunstar Heroes

**USA**
- Vectorman

</details>

## Unimplemented Features

- Interlace mode
- Interlace rendering (HV counter paths exist; full double-field output is still deferred)
- Sega 32X
- Sega CD

**Recently implemented:** SRAM / battery-backed save (`.srm` persistence + periodic autosave), Sega mapper controller (`0xA13000+`, SSF2-style bank switching), VDP DMA timing and bus-routing fixes, PAL video timing (50 Hz / 313-line frame loop), SMD ROM deinterleaving, DMA 128 KB source-window wrap

## Roadmap

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the technical architecture and development roadmap.

**Phase 1 — Core Stability:** Boot and run commercial ROMs reliably with correct audio, video, and input.

**Phase 2 — Feature Completeness:** Save states, SRAM, mapper support, timing accuracy, expanded compatibility.

**Phase 3 — Developer Tools:** Optional tracer, debugger, disassembler, and VDP inspection as separate modules.

**Phase 4 — Platform Expansion:** Cross-platform rendering/audio backends, broader hardware support.

## Project Structure

```
MDTracer.sln                    # Solution file
GenesisEmu.Core/                # Portable emulation core (net9.0)
  md_m68k.cs, md_z80.cs, md_vdp.cs, md_music.cs, md_bus.cs, ...
  opc/                          # MC68000 opcode implementations
GenesisEmu.Platform.Windows/    # Windows D3D12 GPU, NAudio, DirectInput
GenesisEmu.Frontend.Windows/    # Shared WinForms display helpers
GenesisEmu.Game/                # Minimal game-playing WinExe
MDTracer/                       # Full WinForms frontend + debug tools
  WinFormsDebugTools.cs         # Debug-tool registry and display wiring
  WinFormsVdpDebugBitmap.cs     # ARGB buffer → Bitmap for layer viewers
  Form_*.cs                     # UI windows
opcode_make/                    # M68K opcode table generator (build-time)
tests/GenesisEmu.Core.Tests/    # Core unit tests (net9.0)
docs/                           # Architecture and compatibility docs
```

## References

- Sega Genesis Manual: Genesis Technical Overview v1.00 (1991, Sega, US)
- YM2608 OPNA Application Manual
- SN76489 User Manual (Texas Instruments)
- MC68000 16-Bit Microprocessor User's Manual (Motorola, 1981)
- Z80 CPU User Manual (Zilog)

### Acknowledgments

The following open-source projects and documentation were referenced during development:

- [Gens](http://www.gens.me/) — Genesis emulator
- [BlastEm](https://www.retrodev.com/blastem/) — Genesis emulator
- [Sandopolis](https://github.com/pixel-clover/sandopolis) — MIT-licensed Genesis emulator (PAL timing, DMA source wrap, SMD loader, mapper page mask)
- MDSound — Sound emulation reference
- Sega Genesis VDP Documentation by Charles MacDonald

## License

This project is licensed under the **MIT License** — see [LICENSE](LICENSE) for details.

The file [license.txt](license.txt) contains the license for referenced code from the Gens emulator (MIT, Copyright © 2019 Stephane Dallongeville).

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on how to contribute.
