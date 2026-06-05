# Contributing to Genesis / Mega Drive Emulator

Thank you for your interest in contributing! This project aims to be a robust, lightweight, and focused Genesis/Mega Drive emulator.

## Getting Started

### Prerequisites

- Windows 10 or later (currently Windows-only due to WinForms/SharpDX dependencies)
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2022 (recommended) or any editor with C# support

### Building

```bash
git clone https://github.com/awest813/Genesis-mega-drive-Emulator.git
cd Genesis-mega-drive-Emulator
dotnet restore
dotnet build
```

### Running Tests

```bash
dotnet test
```

## Project Architecture

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for a detailed overview of the emulator subsystems and how they interact.

## How to Contribute

### Reporting Issues

- Search existing issues before opening a new one
- Include the ROM title (not the ROM itself) and a description of the expected vs. actual behavior
- Screenshots or short videos are very helpful for rendering/audio issues

### Pull Requests

1. Fork the repository and create a feature branch from `main`
2. Make your changes in small, focused commits
3. Ensure the project builds without warnings: `dotnet build`
4. Run any existing tests: `dotnet test`
5. Open a pull request with a clear description of what changed and why

### Code Style

- Follow existing naming conventions in the codebase (e.g., `g_` prefix for member fields, `w_` for local variables, `in_` for parameters)
- Keep emulation core code free of UI dependencies where possible
- Use `[MethodImpl(MethodImplOptions.AggressiveInlining)]` for performance-critical inner loops
- Add comments referencing hardware documentation when implementing chip behavior

### Areas Where Help Is Needed

- **Compatibility:** Testing with additional ROM titles and reporting issues
- **Correctness:** Improving timing accuracy, VDP edge cases, DMA behavior
- **Missing features:** SRAM/battery save, memory mapper controller, interlace mode
- **Testing:** Adding unit tests for CPU instructions, bus routing, VDP register handling
- **Documentation:** Improving hardware behavior documentation and code comments
- **Cross-platform:** Investigating platform-independent rendering/audio backends

## Scope

This project focuses on stock Genesis/Mega Drive emulation. The following are explicitly **out of scope** for now:

- Sega 32X
- Sega CD
- Game Gear / Master System
- PAL-specific timing (deferred, not rejected)

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
