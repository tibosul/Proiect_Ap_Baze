# VolunteerSystem

**Aplicație Desktop pentru Gestionarea Voluntarilor și Oportunităților de Voluntariat**

O platformă completă care conectează voluntarii cu organizatorii de evenimente, oferind:
- 🎯 Sistem de aplicare și aprobare pentru evenimente
- 🏆 Gamification prin puncte și clasament
- 💬 Chat între utilizatori
- 📊 Rapoarte și statistici detaliate
- ⭐ Feedback și review-uri pentru evenimente

> **📖 Pentru documentație completă în română despre cum funcționează aplicația, arhitectură, baza de date și toate funcționalitățile, vezi [DOCUMENTATIE_PROIECT.md](DOCUMENTATIE_PROIECT.md)**

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

## Default Test Users

After setup, you can login with these test accounts (password: `Password123!`):
- **Admin**: admin@test.com
- **Volunteer**: volunteer@test.com
- **Organizer**: organizer@test.com

## Documentation

For complete documentation about the application flow, features, database structure, and use cases (in Romanian), see:
- **[DOCUMENTATIE_PROIECT.md](DOCUMENTATIE_PROIECT.md)** - Comprehensive project documentation

## Architecture

- **VolunteerSystem.Core** - Business entities and service interfaces
- **VolunteerSystem.Data** - Data access layer with Entity Framework Core
- **VolunteerSystem.Avalonia** - Cross-platform desktop UI with Avalonia (MVVM)
- **VolunteerSystem.UI** - Alternative WPF implementation (Windows only)

## Key Features by Role

### Volunteer
- Browse and search volunteering opportunities
- Apply to events
- View personalized recommendations
- Track points and leaderboard
- Submit feedback after events
- Chat with organizers

### Organizer
- Create and manage opportunities with multiple events
- Review and approve/reject applications
- Mark attendance after events
- Award points to volunteers
- Generate activity reports
- View feedback from volunteers

### Administrator
- Monitor system-wide statistics
- Manage all users
- Handle reports and moderation
- View audit trails
