# Genesis / Mega Drive Emulator

A lightweight, focused Sega Genesis (Mega Drive) emulator written entirely in C# and .NET. Originally based on [MDTracer](https://github.com/sasayaki-japan/MDTracer) by sasayaki-japan, this project is being developed into a robust, standalone open-source emulator.

> **Note:** This program is not intended for playing games illegally. Its purpose is to help users understand and appreciate the ingenuity of the engineers who created this remarkable technology.

## Current Status

The emulator can boot and run a number of commercial Genesis/Mega Drive titles. It currently runs as a Windows desktop application using WinForms + SharpDX (Direct3D 12).

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

```bash
dotnet build MDTracer.sln
```

Or open `MDTracer.sln` in Visual Studio 2022 and build from the IDE.

### Running

```bash
dotnet run --project MDTracer
```

Press **Ctrl+O** or use the menu to open a ROM file (.bin / .md / .zip).

### Default Controls

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

### Keyboard Shortcuts

| Key | Action |
|-----|--------|
| Esc | Pause / Resume |
| F1  | Save State |
| F2  | Input Recording Start/Stop |
| F3  | Save State + Input Recording Start/Stop |
| F4  | Restore Latest State + Input |
| Ctrl+F4 | Capture History |
| F5  | Frame Advance |
| F9  | Settings |
| F10 | Screenshot |
| F11 | Video Recording Start/Stop |
| F12 | Reset |

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

- Memory mapper controller (address 0xA13000+)
- Interlace mode
- Sega 32X
- Sega CD
- PAL timing
- SRAM / battery-backed save

## Roadmap

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the technical architecture and development roadmap.

**Phase 1 — Core Stability:** Boot and run commercial ROMs reliably with correct audio, video, and input.

**Phase 2 — Feature Completeness:** Save states, SRAM, mapper support, timing accuracy, expanded compatibility.

**Phase 3 — Developer Tools:** Optional tracer, debugger, disassembler, and VDP inspection as separate modules.

**Phase 4 — Platform Expansion:** Cross-platform rendering/audio backends, broader hardware support.

## Project Structure

```
MDTracer.sln              # Solution file
GenesisEmu.Core/              # Emulation core class library
GenesisEmu.Platform.Windows/  # Windows audio/input backends
  md_m68k.cs              # MC68000 CPU emulation
  md_z80.cs               # Z80 CPU emulation
  md_vdp.cs               # Video Display Processor
  md_music.cs             # Audio (YM2612 + SN76489)
  md_bus.cs               # System bus arbiter
  md_cartridge.cs         # ROM loading and header parsing
  md_io.cs                # Controller I/O
  md_control.cs           # System control registers
  md_main.cs              # Emulation coordinator / main loop
  opc/                    # MC68000 opcode implementations
MDTracer/                 # WinForms frontend (references GenesisEmu.Core)
  Form_*.cs               # UI and debug tools
opcode_make/              # Development tool: M68K opcode table generator
tests/                    # Unit and integration tests
docs/                     # Architecture and contributor documentation
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
- MDSound — Sound emulation reference
- Sega Genesis VDP Documentation by Charles MacDonald

## License

This project is licensed under the **MIT License** — see [LICENSE](LICENSE) for details.

The file [license.txt](license.txt) contains the license for referenced code from the Gens emulator (MIT, Copyright © 2019 Stephane Dallongeville).

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on how to contribute.
