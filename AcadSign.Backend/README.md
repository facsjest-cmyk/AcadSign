# AcadSign.Backend

The project was generated using the [Clean.Architecture.Solution.Template](https://github.com/jasontaylordev/CleanArchitecture) version 10.3.0.

## Dev Containers Setup (Recommended for macOS)

This project supports development using VS Code Dev Containers, providing a consistent Linux environment with .NET 10 SDK.

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop) installed and running
- [VS Code](https://code.visualstudio.com/) installed
- [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers) installed

### First Time Setup

1. **Clone the repository** (if not already done)

2. **Configure environment variables**
   ```bash
   cd /path/to/e-sign
   cp .env.example .env
   # Edit .env and fill in the required values
   ```

3. **Open in Dev Container**
   - Open VS Code in the `AcadSign.Backend/` folder
   - VS Code will detect the `.devcontainer/devcontainer.json` file
   - Click "Reopen in Container" when prompted
   - Or use: `Cmd+Shift+P` → "Dev Containers: Reopen in Container"

4. **Wait for setup**
   - First time: Downloads .NET 10 image (~2-3 minutes)
   - Installs VS Code extensions automatically
   - Runs `dotnet restore`

### Daily Development

Once the Dev Container is set up, VS Code will automatically reconnect to it when you open the project.

**Terminal commands run inside the Linux container:**
```bash
# Build the solution
dotnet build

# Run the API (with hot reload)
dotnet run --project src/Web

# Run tests
dotnet test
```

### Accessing Services

**From your browser (macOS host):**
- API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger
- Scalar UI: http://localhost:5000/scalar
- Seq Logs: http://localhost:5341
- MinIO Console: http://localhost:9001
- PostgreSQL: localhost:5432

**From code (inside container):**
- PostgreSQL: `Host=postgres;Port=5432;Database=acadsign;Username=acadsign_user`
- MinIO: `http://minio:9000`
- Seq: `http://seq:80`

### Troubleshooting

**Container won't start:**
- Ensure Docker Desktop is running
- Check that ports 5000, 5432, 9000, 9001, 5341 are not in use
- Run `docker-compose up -d` from project root to start infrastructure services

**Extensions not installed:**
- Reopen the container: `Cmd+Shift+P` → "Dev Containers: Rebuild Container"

**Database connection fails:**
- Verify infrastructure services are running: `docker-compose ps`
- Check `.env` file has correct `POSTGRES_PASSWORD`

## Build

Run `dotnet build -tl` to build the solution.

## Run

To run the web application:

```bash
cd .\src\Web\
dotnet watch run
```

Navigate to https://localhost:5001. The application will automatically reload if you change any of the source files.

## Code Styles & Formatting

The template includes [EditorConfig](https://editorconfig.org/) support to help maintain consistent coding styles for multiple developers working on the same project across various editors and IDEs. The **.editorconfig** file defines the coding styles applicable to this solution.

## Code Scaffolding

The template includes support to scaffold new commands and queries.

Start in the `.\src\Application\` folder.

Create a new command:

```
dotnet new ca-usecase --name CreateTodoList --feature-name TodoLists --usecase-type command --return-type int
```

Create a new query:

```
dotnet new ca-usecase -n GetTodos -fn TodoLists -ut query -rt TodosVm
```

If you encounter the error *"No templates or subcommands found matching: 'ca-usecase'."*, install the template and try again:

```bash
dotnet new install Clean.Architecture.Solution.Template::10.3.0
```

## Test

The solution contains unit, integration, and functional tests.

To run the tests:
```bash
dotnet test
```

## Help
To learn more about the template go to the [project website](https://github.com/jasontaylordev/CleanArchitecture). Here you can find additional guidance, request new features, report a bug, and discuss the template with other users.