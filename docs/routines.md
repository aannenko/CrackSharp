### Actions to perform after a .NET version update
- update `src/**/*.csproj` files to target the required .NET version
- update .NET version in `.github/workflows/*.yml`
- update Azure Functions package versions in `src/CrackSharp.Api/CrackSharp.Api.csproj`
- update `azure.yaml` and `infra/` Bicep files if the runtime version changed
- update `.vscode/settings.json` and `.vscode/tasks.json` paths if the target framework moniker changed
- update all `README.md`
