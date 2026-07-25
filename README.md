# Credit Engine

## Setup

### Create Project

```bash
dotnet new webapi --framework net8.0
```

### Create specs structure

```bash
specify init . --integration codex
```

### Git Ignore

```bash
dotnet new gitignore
```

### Create src folder and move source code
```bash
mkdir src
Move-Item CreditEngine.csproj, Program.cs, appsettings.json, appsettings.Development.json, Properties, CreditEngine.http, bin, obj -Destination src\
dotnet new sln -n CreditEngine
dotnet sln add src\CreditEngine.csproj
```

## Run
```bash
dotnet run --project src\CreditEngine.csproj
```