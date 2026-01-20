# Kaizosoft

A command-line tool for extracting and importing language files from Kairosoft games built with Unity. This tool allows you to create translation files and modify game assets with custom translations.

## Features

- **Extract Language Files**: Generate CSV translation templates from Kairosoft game assets
- **Import Translations**: Import modified language files back into game assets
- **Multiple Game Support**: Works with various Kairosoft titles through a configurable key database
- **Unity Integration**: Uses AssetsTools.NET to work with Unity asset files

## Prerequisites

- .NET 10.0 or higher
- A supported Kairosoft game installed
- `keys.csv` file with game encryption keys
- `classdata.tpk` file for Unity asset parsing

## Installation

### Building from Source

1. Clone or download the repository
2. Ensure you have the .NET 10.0 SDK installed
3. Build the project:
   ```powershell
   dotnet build -c Release
   ```
4. The executable will be in `bin/Release/net10.0/`

### Publishing

To create a standalone executable:
```powershell
dotnet publish -c Release -r win-x64 --self-contained
```

## Configuration

### keys.csv Format

The `keys.csv` file must contain the encryption keys for supported games:
```csv
name_en,name_jp,key
Game Dev Story,ゲーム発展国++,ABCDEF0123456789...
Dungeon Village,ダンジョンビレッジ,1234567890ABCDEF...
```

### Required Files

Place these files in the same directory as the executable:
- `classdata.tpk` - Unity class database
- `keys.csv` - Game encryption keys

## Usage

### Generate Command

Extract a language template from game assets:

```powershell
Kaizosoft generate <assetsPath> --title <title> --language <lang> [--output <outputPath>]
```

**Arguments:**
- `<assetsPath>` - Path to the game's data folder (containing `globalgamemanagers`)

**Options:**
- `-t, --title <title>` - Title for the language file (required)
- `-l, --lang, --language <lang>` - Language code, e.g., `en`, `vi`, `ko` (required)
- `-o, --output <path>` - Output directory (default: current directory)

**Example:**
```powershell
Kaizosoft generate "C:\Games\GameDevStory\Data" -t "Tiếng Việt" -l vi -o ".\translations"
```

This will create a file like `GameDevStory_vi.csv` with all translatable strings.

### Import Command

Import a translated CSV file back into game assets:

```powershell
Kaizosoft import <languageFile> <assetsPath> [--output <outputPath>]
```

**Arguments:**
- `<languageFile>` - Path to your translated CSV file
- `<assetsPath>` - Path to the game's data folder

**Options:**
- `--output <path>` - Output directory for modified assets (default: same as assetsPath)

**Example:**
```powershell
Kaizosoft import "GameDevStory_vi.csv" "C:\Games\GameDevStory\Data" --output ".\modded"
```

This creates a modified `globalgamemanagers.mod` file that can be used to replace the original.

## CSV Format

The generated CSV files have the following structure:

```csv
@title,Tiếng Việt
@language,vi
@scale,auto
@appli,40 - Do not change
@version,1.00 - Do not change
@author,Kairosoft
#00001,Game Title
#00002,Start Game
#00003,Options
...
```

- Lines starting with `@` are metadata
- Lines starting with `#` are translation keys with 5-digit numbers
- Each row has: `key,translation`

## Project Structure

```
Kaizosoft/
├── Program.cs                          # Main entry point and CLI commands
├── KairosoftArchive.cs                 # Archive format handler
├── KairosoftKey.cs                     # Game key model
├── BinaryReaderBigEndianExtensions.cs  # Big-endian reading utilities
├── BinaryWriterBigEndianExtensions.cs  # Big-endian writing utilities
├── Kaizosoft.csproj                    # Project file
├── keys.csv                            # Game encryption keys (required)
└── classdata.tpk                       # Unity class database (required)
```

## Dependencies

- [AssetsTools.NET](https://github.com/nesrak1/AssetsTools.NET) - Unity asset file manipulation
- [Sep](https://github.com/nietras/Sep) - High-performance CSV parsing
- [System.CommandLine](https://github.com/dotnet/command-line-api) - Command-line interface

## How It Works

1. **Generate**: 
   - Loads Unity asset files from the game directory
   - Identifies the game using PlayerSettings
   - Extracts the language archive using the appropriate encryption key
   - Reads the English template and existing language data
   - Outputs a CSV file with all translatable strings

2. **Import**:
   - Reads your translated CSV file
   - Loads the original game assets
   - Decrypts the language archive
   - Adds/replaces the language file in the archive
   - Re-encrypts with the game-specific key
   - Writes the modified asset file

## Supported Games

Games are supported if their encryption keys are present in `keys.csv`. Common Kairosoft titles include:
- Game Dev Story
- Dungeon Village / Dungeon Village 2
- Dream House Days
- World Cruise Story
- Café Master Story
- Pocket Harvest
- 8-Bit Farm
- And many more!

## Troubleshooting

### "Game name not found in keys.csv"
Make sure the game name in the assets matches an entry in your `keys.csv` file.

### "classdata.tpk not found"
Ensure `classdata.tpk` is in the same directory as the executable.

### File Access Errors
- Make sure the game is not running when importing modified assets
- Run with administrator privileges if necessary
- Check that file paths are correct and accessible

## License

This is a community tool for educational and modding purposes. Respect Kairosoft's intellectual property and game licenses.

## Disclaimer

This tool is not affiliated with or endorsed by Kairosoft. Use at your own risk. Always backup your game files before modifying them.

## Contributing

Contributions are welcome! Areas for improvement:
- Add support for more Kairosoft games
- Improve error handling and validation
- Create a GUI interface
- Add language file validation

## Credits

- Built with [AssetsTools.NET](https://github.com/nesrak1/AssetsTools.NET)
- CSV parsing by [Sep](https://github.com/nietras/Sep)
- Command-line interface by [System.CommandLine](https://github.com/dotnet/command-line-api)
