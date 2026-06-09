# 🎮 GenesisEmu (and MDTracer)

A high-performance, modular, and portable Sega Genesis (Mega Drive) emulator written entirely in C# and .NET 9.

Originally based on [MDTracer](https://github.com/sasayaki-japan/MDTracer) by sasayaki-japan, this project has been heavily re-architected into a decoupled, robust, open-source emulator. It separates a platform-independent emulation core from multiple tailored frontends, serving both general players and retro-engineering developers.

> ⚠️ **Disclaimer:** This emulator is developed for educational, historical preservation, and development purposes. It is not intended to promote or facilitate copyright infringement.

---

## 🏗️ System Architecture & Frontends

GenesisEmu features a clean architectural separation between the core emulation logic and the audio/video/input presentation layer. This decoupling allows us to target multiple platforms and run different frontends seamlessly:

### 1. ⚡ Emulation Core (`GenesisEmu.Core`)
A fully portable .NET 9 class library containing the system-bus coordinator (`md_bus`) and all core chip-level emulators (MC68000, Z80, VDP, YM2612, SN76489). It is entirely self-contained and cross-platform.

### 2. 📺 `GenesisEmu.Game` (Windows Frontend)
A lightweight, fast, player-focused desktop app written with WinForms, targeting Windows. It supports Direct3D 12 GPU compositing, low-latency audio via NAudio, custom controller mapping, and fullscreen play.

### 3. 🗺️ `MDTracer` (Windows Developer Frontend)
An advanced, diagnostic-rich desktop environment for developers and retro-engineering enthusiasts. It extends the Windows frontend with powerful debug views:
- **Instruction Tracer & Disassembler**: Real-time assembly execution log and program counter tracking.
- **VDP Layer Viewers**: Visual inspectors for VRAM tiles, active color palettes, sprites, and background scroll planes.
- **History Capture**: Combined save states and input recording/replay for deterministic debugging.

### 4. 🐧 `GenesisEmu.Game.Portable` (Cross-Platform SDL2 Frontend)
A fully portable game shell designed for **Linux, macOS, and Windows**. It leverages cross-platform backends in `GenesisEmu.Platform.Portable`:
- **Audio**: OpenAL Soft integration via Silk.NET.
- **Video**: CPU rendering, Vulkan, and Apple Metal VDP compositing via Silk.NET.
- **Input**: Unified SDL2 controller and keyboard bindings.

---

## 🚀 Key Features & Emulated Hardware

- **Motorola MC68000**: Main system CPU, fully emulated with custom instruction decoder tables.
- **Zilog Z80**: Secondary CPU managing audio co-processing and sub-bus communications.
- **VDP (Video Display Processor)**: High-fidelity graphics engine supporting scrolling background planes (A & B), Window layer, sprite rendering (with priorities and collisions), palette management (CRAM), vertical/horizontal scroll RAM (VSRAM), and high-speed DMA transfers.
- **VDP DMA Mechanics**: Includes correct 128 KB source-window wrapping and timing characteristics.
- **YM2612 & SN76489**: Complete FM and PSG sound generators with stereo panning, multi-channel sound mixing, and DAC support.
- **SRAM Persistence**: Battery-backed save file (`.srm`) support featuring periodic auto-saves for non-volatile cartridge memory.
- **Bank-Switching Mappers**: Sega mapper support (e.g. `0xA13000+` address space, supporting massive ROMs like *Super Street Fighter II*).
- **PAL/NTSC Support**: Dynamic video timing supporting 60 Hz NTSC and 50 Hz PAL frame loops.
- **SMD Support**: In-memory deinterleaving loader for standard interleaved ROMs.

---

## 🛠️ Getting Started

### Prerequisites
- **All Platforms**: [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (or Visual Studio 2022 17.8+)
- **Windows Frontends**: Windows 10/11
- **Portable Frontend (Linux/macOS)**: Native SDL2 and OpenAL Soft libraries (often pre-installed, or installable via package managers like `apt` or `brew`).

### 📦 Building the Solution

You can build the entire solution using the root solution file:
```bash
dotnet build MDTracer.sln
```

Or build specific components:

* **Portable Game Shell (Linux, macOS, Windows)**:
  ```bash
  dotnet build GenesisEmu.Game.Portable/GenesisEmu.Game.Portable.csproj
  ```

* **Standard Game Client (Windows Only)**:
  ```bash
  dotnet build GenesisEmu.Game/GenesisEmu.Game.csproj
  ```

* **Developer & Diagnostics Tool (Windows Only)**:
  ```bash
  dotnet build MDTracer/MDTracer.csproj
  ```

* **Core Unit Tests**:
  ```bash
  dotnet test tests/GenesisEmu.Core.Tests/GenesisEmu.Core.Tests.csproj
  ```

---

## 🎮 Running the Emulator

### 1. Portable SDL2 Shell (Recommended for Linux & macOS)
```bash
dotnet run --project GenesisEmu.Game.Portable -- [rom-path]
```
You can also drag and drop ROM files (.bin, .md, .gen, .smd) directly onto the running window.

### 2. Standard Game App (Recommended for Windows Play)
```bash
dotnet run --project GenesisEmu.Game
```

### 3. Developer App (Tracer & Diagnostics for Windows)
```bash
dotnet run --project MDTracer
```

---

## ⌨️ Controls & Shortcuts

### Default Gameplay Controls (Player 1)

| Genesis Button | Keyboard Key |
|:---|:---|
| **Up / Down / Left / Right** | `W` / `S` / `A` / `D` |
| **A / B / C** | `N` / `M` / `,` (Comma) |
| **X / Y / Z** | `H` / `J` / `K` |
| **Start** | `Space` |
| **Mode** | `Return` (Enter) |

*For `GenesisEmu.Game` (Windows), you can remap both Player 1 and Player 2 keyboard/gamepad keys via **Options ➔ Controller Settings**.*

---

### Shortcuts: `GenesisEmu.Game.Portable` (SDL2 Frontend)

| Shortcut | Action |
|:---|:---|
| **Esc** | Pause / Resume gameplay |
| **F1** | Save state |
| **F4** | Load latest state |
| **Ctrl + F4** | Save state list |
| **F5** | Frame advance (while paused) |
| **F12** | Hard Reset |
| **Ctrl + I** | Toggle integer-fit scaling |
| **Ctrl + H** | Toggle on-screen help |
| **Ctrl + G** | Open Gamepad picker |
| **Ctrl + Q** | Quit |

---

### Shortcuts: `GenesisEmu.Game` (Windows Frontend)

| Shortcut | Action |
|:---|:---|
| **Ctrl + O** | Open ROM selection dialog |
| **Esc** | Pause / Resume gameplay (exit fullscreen first) |
| **F1** | Quick Save State |
| **F4** | Load Latest State |
| **Ctrl + F4** | Save State List |
| **F5** | Frame Advance |
| **F10** | Take Screenshot |
| **F11** or **Alt + Enter** | Toggle Fullscreen |
| **F12** | Hard Reset |

---

### Shortcuts: `MDTracer` (Developer Diagnostics)

Includes all of the `GenesisEmu.Game` shortcuts above, plus diagnostic utilities:

| Shortcut | Action |
|:---|:---|
| **F2** | Start / Stop Input Recording |
| **F3** | Save State + Start/Stop Input Recording |
| **F4** | Restore Latest State + replay recorded input |
| **Ctrl + F4** | Capture execution history (state + input) |
| **F9** | Open Settings Manager |
| **F11** | Start / Stop Video Recording |

---

## 📂 Project Structure

```text
MDTracer.sln                    # Root MSBuild Solution
├── docs/                       # Technical architecture & compatibility logs
├── tests/
│   └── GenesisEmu.Core.Tests/  # Extensive unit testing suite (xUnit)
│
├── GenesisEmu.Core/            # 📦 Portable Emulation Core
│   ├── opc/                    # MC68000 instruction-set micro-opcodes
│   ├── md_bus.cs               # System bus coordinator
│   ├── md_m68k.cs              # Motorola 68000 CPU emulator
│   ├── md_z80.cs               # Zilog Z80 audio-co-processor
│   ├── md_vdp.cs               # Video Display Processor (VRAM/rendering)
│   ├── md_music.cs             # Audio coordinator
│   └── ...                     # Cartridge, IO, SRAM, and save state APIs
│
├── GenesisEmu.Platform.Portable/ # 🌀 Cross-Platform Backends
│   ├── CpuVdpGpuRenderer.cs    # Software fall-back rasterizer
│   ├── VulkanVdpGpuRenderer.cs # Vulkan GPU compositor (Silk.NET)
│   ├── MetalVdpGpuRenderer.cs  # Metal GPU compositor (Silk.NET)
│   ├── OpenAlAudioOutputBackend.cs # OpenAL-Soft native sound driver
│   └── ...
│
├── GenesisEmu.Game.Portable/   # 🐧 Cross-Platform SDL2 Shell App
│
├── GenesisEmu.Platform.Windows/ # 🖥️ Windows-Specific Backends (DirectX 12 / NAudio)
├── GenesisEmu.Frontend.Windows/ # Shared WinForms drawing & scaling mechanics
├── GenesisEmu.Game/            # 🎮 Standard WinForms Desktop Player App
├── MDTracer/                   # 🗺️ Dev Debugger App (Trace/Disassembler/VDP Inspectors)
└── opcode_make/                # MC68000 Build-time instruction map generator
```

---

## 📈 Compatibility Status

The emulation core runs many commercial classic titles. For continuous regression checks and detailed performance tracking, consult the **[Compatibility Matrix](docs/COMPATIBILITY_MATRIX.md)**.

### Emulated Hardware Coverage:
- [x] Motorola MC68000 / Zilog Z80
- [x] Full VDP Plane & Sprite Rendering
- [x] VDP DMA (including wrapping and timing constraints)
- [x] High-Fidelity YM2612 FM / SN76489 PSG Synthesis
- [x] Battery SRAM / Save Files (.srm) with periodic autosave
- [x] Sega Mapper Support (Super Street Fighter II, etc.)
- [x] SMD ROM Deinterleaving Loader
- [x] Decoupled Front-end UI interfaces
- [ ] Interlace double-field rendering (partially implemented; full output deferred)
- [ ] Sega CD & Sega 32X expansion support (planned roadmap)

---

## 📜 References & Acknowledgments

This emulator was made possible thanks to the documentation and work of the retro-computing community:

### Reference Specifications:
- *Sega Genesis Technical Overview v1.00* (1991, Sega US)
- *YM2608 OPNA Application Manual*
- *SN76489 User Manual* (Texas Instruments)
- *MC68000 16-Bit Microprocessor User's Manual* (Motorola)
- *Z80 CPU User Manual* (Zilog)
- *Sega Genesis VDP Documentation* by Charles MacDonald

### Open-Source Inspiration:
- [Gens](http://www.gens.me/) by Stephane Dallongeville (referenced code licensed in [license.txt](license.txt))
- [BlastEm](https://www.retrodev.com/blastem/) - High accuracy emulator
- [Sandopolis](https://github.com/pixel-clover/sandopolis) - Helpful reference for PAL loop timing, DMA source wraps, SMD loaders, and mapper masking.

---

## ⚖️ License

This project is open-source software licensed under the **[MIT License](LICENSE)**.

Portions of code referenced from the *Gens* emulator are licensed under the MIT License by Stephane Dallongeville (see [license.txt](license.txt) for full credit).

---

## 🤝 Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on how to contribute.
