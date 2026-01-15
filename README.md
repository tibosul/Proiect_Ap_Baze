# VolunteerSystem

## Prerequisites
- **Operating System**: Linux (Debian/Ubuntu or RedHat/Fedora based)
- **.NET SDK**: Version 10.0 (Installed automatically by setup script if missing)
- **Docker**: Required for running the SQL Server database container (Installed automatically by setup script if missing)

## Setup

This project includes a convenient setup script that handles dependency installation and database initialization.

1.  **Run the setup script**:
    ```bash
    sudo ./setup_env.sh
    ```
    This script will:
    - Install .NET SDK 10.0 (if missing)
    - Install Docker (if missing)
    - Start a Docker container for SQL Server
    - Initialize the `VolunteerSystem` database using `VolunteerSystem.sql`
    - Restore project dependencies

## Running the Application

To run the application (Avalonia UI), use the following command:

```bash
dotnet run --project VolunteerSystem.Avalonia
```

## Makefile

A `Makefile` is included for your convenience. You can use the following commands:

-   `make setup`: Runs the setup environment script.
-   `make run`: Runs the Avalonia application.
-   `make clean`: Cleans the build artifacts.
