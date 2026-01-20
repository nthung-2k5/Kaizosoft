# GitHub Actions Workflow Documentation

## Overview

The `release.yml` workflow automates building and releasing Kaizosoft executables for multiple platforms.

## Workflow Structure

### Triggers

```yaml
on:
  push:
    tags:
      - 'v*'           # Automatic: When you push a tag starting with 'v'
  workflow_dispatch:   # Manual: From GitHub Actions UI
```

### Build Job

Builds single-file executables for three platforms in parallel:
- **Windows** (`win-x64`)
- **Linux** (`linux-x64`)
- **macOS** (`osx-x64`)

#### Build Features

- **Single-file executable**: All dependencies bundled into one file
- **Self-contained**: No .NET runtime required on target machine
- **Trimmed**: Unused code removed to reduce file size
- **Native AOT**: Ahead-of-time compilation for better performance
- **Required files included**: `keys.csv` and `classdata.tpk` packaged with executable

### Release Job

Automatically creates a GitHub Release when triggered by a version tag:
- Downloads all platform artifacts
- Creates a release with auto-generated release notes
- Attaches all platform archives to the release

## Usage

### Automatic Release (Recommended)

```powershell
# 1. Update version in Kaizosoft.csproj
# 2. Commit your changes
git add .
git commit -m "Release v1.0.0"

# 3. Create and push tag
git tag v1.0.0
git push origin main
git push origin v1.0.0
```

The workflow will:
1. ✅ Build for all platforms
2. ✅ Create ZIP archives with required files
3. ✅ Create GitHub Release
4. ✅ Upload all artifacts

### Manual Build (Testing)

From GitHub:
1. Go to **Actions** tab
2. Select **Build and Release** workflow
3. Click **Run workflow**
4. Select branch and click **Run workflow**

This builds the executables but doesn't create a release (no tag).

## Output

Each platform archive contains:
```
Kaizosoft-{platform}.zip
├── Kaizosoft.exe (or Kaizosoft on Linux/macOS)
├── keys.csv
└── classdata.tpk
```

## Workflow Configuration

### Changing Target Platforms

Edit the matrix in `.github/workflows/release.yml`:

```yaml
strategy:
  matrix:
    runtime: [win-x64, linux-x64, osx-x64, osx-arm64]  # Add more runtimes
```

Available runtimes:
- `win-x64`, `win-x86`, `win-arm64`
- `linux-x64`, `linux-arm64`, `linux-musl-x64`
- `osx-x64`, `osx-arm64`

### Changing .NET Version

Update the setup-dotnet step:

```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '10.0.x'  # Change version here
```

### Additional Build Options

Add to the `dotnet publish` command:

```yaml
-p:EnableCompressionInSingleFile=true  # Compress embedded assemblies
-p:TrimMode=link                       # More aggressive trimming
```

## Troubleshooting

### Build Fails

**Check the Actions log** for specific errors:

1. Go to Actions tab
2. Click on the failed workflow run
3. Expand the failing step

Common issues:
- **Dependencies not restored**: Check NuGet packages
- **Compilation errors**: Fix code issues
- **.NET version mismatch**: Update workflow YAML

### Release Not Created

Verify:
- ✅ Tag starts with `v` (e.g., `v1.0.0`, not `1.0.0`)
- ✅ Tag was pushed to GitHub
- ✅ Build jobs completed successfully
- ✅ Repository has permissions for releases

### Artifacts Not Uploaded

Check:
- ✅ File paths in workflow are correct
- ✅ `keys.csv` and `classdata.tpk` exist in repository
- ✅ Archive creation step succeeded

## Permissions

The workflow requires:
- **Read** access to repository contents
- **Write** access to releases (automatic with `GITHUB_TOKEN`)

These are granted automatically by GitHub Actions.

## Cost

GitHub Actions usage:
- **Public repositories**: Free unlimited minutes
- **Private repositories**: Free tier includes 2,000 minutes/month

This workflow typically uses ~15-20 minutes per run (all 3 platforms).

## Best Practices

1. **Test locally first**: Build and test before creating a tag
2. **Semantic versioning**: Use `vMAJOR.MINOR.PATCH` format
3. **Update changelog**: Document changes in commits for auto-generated notes
4. **Clean builds**: Actions always build from scratch (no cached artifacts)
5. **Security**: Never commit sensitive keys or credentials

## Customization Ideas

### Add Code Signing (Windows)

```yaml
- name: Sign executable
  run: |
    signtool sign /f certificate.pfx /p ${{ secrets.CERT_PASSWORD }} ./publish/win-x64/Kaizosoft.exe
```

### Add Checksums

```yaml
- name: Generate checksums
  run: |
    Get-FileHash ./Kaizosoft-win-x64.zip | Out-File checksums.txt
```

### Deploy to Multiple Locations

```yaml
- name: Upload to S3
  uses: aws-actions/configure-aws-credentials@v1
  # ... configuration
```

## Resources

- [GitHub Actions Documentation](https://docs.github.com/actions)
- [.NET Publish Options](https://learn.microsoft.com/dotnet/core/deploying/)
- [action-gh-release](https://github.com/softprops/action-gh-release)
