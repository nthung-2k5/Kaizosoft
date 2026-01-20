# Release Process

This document describes how to create a new release for Kaizosoft.

## Automated Releases via GitHub Actions

The project uses GitHub Actions to automatically build and release executables for multiple platforms.

### Creating a New Release

1. **Update the version** in `Kaizosoft.csproj`:
   ```xml
   <Version>1.0.0</Version>
   ```

2. **Commit your changes**:
   ```powershell
   git add .
   git commit -m "Prepare release v1.0.0"
   ```

3. **Create and push a version tag**:
   ```powershell
   git tag v1.0.0
   git push origin main
   git push origin v1.0.0
   ```

4. **GitHub Actions will automatically**:
   - Build single-file executables for Windows, Linux, and macOS
   - Package them with required files (`keys.csv`, `classdata.tpk`)
   - Create a GitHub Release with all artifacts
   - Generate release notes from commits

5. **The release will appear** at: `https://github.com/YOUR_USERNAME/Kaizosoft/releases`

### Manual Trigger

You can also trigger the workflow manually from the Actions tab on GitHub:

1. Go to **Actions** → **Build and Release**
2. Click **Run workflow**
3. Select the branch and click **Run workflow**

This will build the executables but won't create a release (since it's not triggered by a tag).

## Manual Release (Local Build)

If you prefer to build locally:

### Windows
```powershell
dotnet publish -c Release -r win-x64 --self-contained
```

### Linux
```powershell
dotnet publish -c Release -r linux-x64 --self-contained
```

### macOS
```powershell
dotnet publish -c Release -r osx-x64 --self-contained
```

The output will be in `bin/Release/net10.0/<runtime>/publish/`

### Package for Distribution

After building, copy the required files:
```powershell
Copy-Item keys.csv bin/Release/net10.0/win-x64/publish/
Copy-Item classdata.tpk bin/Release/net10.0/win-x64/publish/
```

Then create an archive:
```powershell
Compress-Archive -Path bin/Release/net10.0/win-x64/publish/* -DestinationPath Kaizosoft-win-x64.zip
```

## Version Numbering

Follow [Semantic Versioning](https://semver.org/):
- **Major** (1.x.x): Breaking changes
- **Minor** (x.1.x): New features, backwards compatible
- **Patch** (x.x.1): Bug fixes, backwards compatible

## Release Checklist

Before creating a release:

- [ ] Update version in `Kaizosoft.csproj`
- [ ] Update `README.md` if there are new features or changes
- [ ] Test the application with at least one game
- [ ] Ensure `keys.csv` is up to date
- [ ] Review and commit all changes
- [ ] Create and push version tag
- [ ] Verify GitHub Actions workflow completes successfully
- [ ] Test downloaded release artifacts
- [ ] Update release notes if needed

## Troubleshooting

### Workflow Fails

Check the Actions tab for error logs. Common issues:
- .NET SDK version mismatch (update workflow to use correct version)
- Missing dependencies
- Build errors in code

### Release Not Created

Make sure:
- You pushed a tag starting with `v` (e.g., `v1.0.0`)
- The workflow has permissions to create releases
- The build jobs completed successfully
