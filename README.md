<h1 align="center">
  <p>
    <img width="700" src="https://socialify.git.ci/BedrockSharp/QuantumMC/image?description=1&font=Inter&forks=1&issues=1&logo=https%3A%2F%2Fgithub.com%2FBedrockSharp%2FQuantumMC%2Fblob%2Fmaster%2F.github%2Flogo.png%3Fraw%3Dtrue&name=1&owner=1&pattern=Plus&pulls=1&stargazers=1&theme=Auto" alt="The QuantumMC logo">
</p>
</h1>

<h4 align="center">Highly customized, lightweight and super-fast Minecraft: Bedrock Edition server software written in modern C#.</h4>

<p align="center">
  <a href="#the-vision">The Vision</a> •
  <a href="#how-to-use">How To Use</a> •
  <a href="#credits">Credits</a> •
  <a href="#license">License</a>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Minecraft BE-1.26.10-brightgreen?style=for-the-badge">
  <img src="https://img.shields.io/badge/.NET-9.0-blueviolet?style=for-the-badge&logo=.net">
  <img src="https://img.shields.io/github/actions/workflow/status/BedrockSharp/QuantumMC/build.yml?style=for-the-badge" alt="Build Status">
  <img src="https://img.shields.io/badge/Status-In%20Development-yellow?style=for-the-badge">
</p>

---

## 🚀 The Vision

QuantumMC aims to be the modern C# pioneer for Bedrock server software. 

By leveraging the cutting-edge performance enhancements of **.NET 9**, QuantumMC moves entirely away from archaic procedural code into a beautifully scalable, highly-concurrent Object-Oriented framework.

---

## 💻 How To Use

### Option 1: Downloading a Release (Easiest)

If you just want to run the server without compiling anything, you can grab the latest pre-compiled binaries:

1. Navigate to the [Releases](https://github.com/BedrockSharp/QuantumMC/releases) page.
2. Download the latest `QuantumMC.dll` from the assets.
3. Open a terminal in your download folder and run the server using the .NET 9 runtime:
   ```bash
   dotnet QuantumMC.dll
   ```

### Option 2: Running from Source

To deploy your own bleeding-edge QuantumMC node directly from the repository, ensure you have the [**.NET 9.0 SDK**](https://dotnet.microsoft.com/download/dotnet/9.0) installed.

```bash
# 1. Clone the bleeding edge QuantumMC repository
$ git clone https://github.com/BedrockSharp/QuantumMC.git

# 2. Enter the source directory
$ cd QuantumMC

# 3. Build the optimal release binary
$ dotnet build -c Release

# 4. Spin up the server!
$ dotnet run --project src/QuantumMC/QuantumMC.csproj
```

---

## 🤝 Contributing

We strongly believe an open-source bedrock environment is the ultimate key to a better multiplayer universe.
1. **Fork** the repository and create your custom feature branch (`git checkout -b feature/YourFeature`).
2. Adhere to the C# principles detailed within the undocumented components (`AGENTS.md`).
3. Create a **Pull Request**.

## ⚖️ License

QuantumMC is distributed under the proprietary ecosystem standards allowing completely free community execution while strictly encouraging code-return enhancements. (See the [`LICENSE`](LICENSE) document for further details.)

<p align="center">
  <i>Developed with ❤️ for the Bedrock Community</i>
</p>
