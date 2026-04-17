# 🤖 AI Agent Guidelines (AGENTS.md)

Welcome, fellow agent! This document provides the essential context, architectural principles, and technical constraints required to maintain and evolve **QuantumMC**.

## 🚀 Project Mission
QuantumMC is a high-performance, modular Minecraft Bedrock Edition server implementation focusing on protocol accuracy and modern .NET 9 features.

## 🛠️ Developer Workflows

### Versioning
- Versioning is managed through [Utils/Version.cs](src/QuantumMC/Utils/Version.cs).
- Use `SemanticVersion.cs` for parsing logic.
- Manual version bumps in `Version.cs` automatically sync to the `.csproj` via GitHub Actions.

### Automation
- **CI**: Build checks on every PR.
- **Nightly**: Builds and pushes a `nightly` pre-release on every commit to `master`.
- **Release**: Manual trigger via GitHub Actions with version bumping.

## 📜 Coding Standards for Agents
1. **Zero-Buffering**: Never use buffered streams for encryption; Bedrock expects immediate byte-for-byte stream processing.
2. **Endianness**: Use natural .NET byte order for the ECDH Shared Secret (no reversal required after latest fixes).
3. **Async/Sync**: Prefer synchronous packet processing unless dealing with I/O (like DB/Files) to minimize jitter in the RakNet loop.
4. **Logging**: Use `Serilog`. Always include the `ThreadName` and `Username` in network-related logs where possible.

---
*Maintained by the QuantumMC Dev Team*