# SCPSL Template Generator

A .NET CLI tool for quickly generating SCP:SL LabAPI plugin templates with automatic dependency management.

## Features

✨ **Quick Plugin Generation** - Create a complete plugin structure with one command  
🔧 **Automatic Dependencies** - Integrated dependency management via `LABAPI_REFERENCES`  
📦 **LabAPI Integration** - Pre-configured with Northwood.LabAPI and CustomEventHandler  
🎯 **Customizable** - Plugin name, author, description, and version  
📁 **Complete Structure** - Includes example commands, events, and config files

## Installation

Install globally using .NET CLI:

```bash
dotnet tool install --global SCPSLTemplateGenerator
```

## Quick Start

Generate a new plugin:

```bash
scpsl-template new MyPlugin --author "YourName" --description "My awesome plugin"
```

### Options

- `--author` / `-a` : Plugin author name (default: "YourName")
- `--description` / `-d` : Plugin description (default: "A SCP:SL plugin")
- `--version` / `-v` : Plugin version (default: "1.0.0")
- `--output` / `-o` : Output directory (default: current directory)
- `--force` / `-f` : Overwrite existing files

## Generated Structure

```
MyPlugin/
├── MyPlugin.sln
├── MyPlugin.csproj
├── Plugin.cs          # Main plugin class
├── Config.cs          # Plugin configuration
├── LICENSE            # MIT License
├── .gitignore
├── Commands/
│   └── ExampleCommand.cs
└── Events/
    └── PlayerEvent.cs
```

## Requirements

- .NET 8.0 SDK or later
- Environment variable `LABAPI_REFERENCES` pointing to your SCP:SL dependencies folder

The tool will guide you through setting up `LABAPI_REFERENCES` on first run if not configured.

## Environment Setup

On first use, the tool will help you set up the `LABAPI_REFERENCES` environment variable, which should point to a folder containing your SCP:SL game dependencies (Assembly-CSharp.dll, UnityEngine.dll, etc.).

## Building from Source

```bash
git clone <your-repo-url>
cd SCPSLTemplateGenerator
dotnet build -c Release
dotnet pack -c Release
dotnet tool install --global --add-source ./nupkg SCPSLTemplateGenerator
```

## License

MIT License - See LICENSE file for details

## Support

For issues, feature requests, or contributions, please visit the [GitHub repository](https://github.com/your-repo/scpsl-template-generator).
