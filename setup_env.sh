#!/bin/bash
set -e

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}Starting VolunteerSystem Environment Setup...${NC}"

# Check for sudo/root
if [ "$EUID" -ne 0 ] && ! command -v sudo &> /dev/null; then
  echo -e "${RED}Error: This script requires root privileges or sudo access to install dependencies.${NC}"
  exit 1
fi

# 1. Install .NET SDK 8.0
if ! command -v dotnet &> /dev/null; then
    echo "Installing .NET SDK 8.0..."
    # Update and install pre-requisites
    if [ -f /etc/debian_version ]; then
        sudo apt-get update
        sudo apt-get install -y dotnet-sdk-8.0
    elif [ -f /etc/redhat-release ]; then
        sudo dnf install -y dotnet-sdk-8.0
    else 
        echo -e "${RED}Unsupported OS for automatic .NET installation. Please install .NET SDK 8.0 manually.${NC}"
    fi
else
    echo -e "${GREEN}.NET SDK is already installed: $(dotnet --version)${NC}"
fi

# 2. Install Docker
if ! command -v docker &> /dev/null; then
    echo "Installing Docker..."
    if [ -f /etc/debian_version ]; then
        sudo apt-get update
        sudo apt-get install -y docker.io
        sudo usermod -aG docker $USER || true
    else
        echo -e "${RED}Unsupported OS for automatic Docker installation. Please install Docker manually.${NC}"
    fi
    echo "Docker installed. Note: You may need to log out and back in for group changes to take effect."
else
    echo -e "${GREEN}Docker is already installed.${NC}"
fi

# 3. Setup SQL Server
CONTAINER_NAME="sql_server_volunteer"
# Check for SA_PASSWORD env var or prompt user
if [ -z "$SA_PASSWORD" ]; then
    echo "SA_PASSWORD not set."
    read -s -p "Please enter the SQL Server Password (will be hidden): " SA_PASSWORD
    echo ""
    if [ -z "$SA_PASSWORD" ]; then
        echo -e "${RED}Error: Password cannot be empty.${NC}"
        exit 1
    fi
fi
DB_PASSWORD="$SA_PASSWORD"

if [ ! "$(sudo docker ps -q -f name=$CONTAINER_NAME)" ]; then
    if [ "$(sudo docker ps -aq -f name=$CONTAINER_NAME)" ]; then
        echo "Restarting existing container..."
        sudo docker start $CONTAINER_NAME
        echo "Waiting for SQL Server to start (5s)..."
        sleep 5
    else
        echo "Starting SQL Server container (persistence enabled)..."
        sudo docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=$DB_PASSWORD" \
           -p 1433:1433 --name $CONTAINER_NAME --hostname sql_server \
           -v volunteer_mssql_data:/var/opt/mssql \
           -d mcr.microsoft.com/mssql/server:2022-latest
        echo "Waiting for SQL Server to start (30s)..."
        sleep 30
    fi
else
    echo -e "${GREEN}SQL Server container is already running.${NC}"
fi

# 4. Initialize Database
echo "Initializing Database..."
# Copy SQL file to container (assumes file is in same directory as this script)
SCRIPT_DIR=$(dirname "$(realpath "$0")")
SQL_FILE="$SCRIPT_DIR/VolunteerSystem.sql"

if [ -f "$SQL_FILE" ]; then
    # Check if DB exists to prevent overwriting
    # Fail-safe check in case server is slow to wake up
    DB_EXISTS_CHECK=$(sudo docker exec $CONTAINER_NAME /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$DB_PASSWORD" \
        -Q "IF DB_ID('VolunteerSystem') IS NOT NULL PRINT 'DB_FOUND'" -C -N 2>&1)

    if [[ "$DB_EXISTS_CHECK" == *"DB_FOUND"* ]]; then
        echo -e "${GREEN}Database 'VolunteerSystem' already exists. Skipping re-initialization.${NC}"
        echo "To force a reset, run: sudo docker exec $CONTAINER_NAME /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P \"$DB_PASSWORD\" -Q \"DROP DATABASE VolunteerSystem\" -C -N"
    else
        echo "Initializing Database..."
        sudo docker cp "$SQL_FILE" $CONTAINER_NAME:/tmp/VolunteerSystem.sql

        # Execute SQL file
        # Retry logic if SQL Server is not quite ready
        MAX_RETRIES=10
        COUNT=0
        until sudo docker exec $CONTAINER_NAME /opt/mssql-tools18/bin/sqlcmd \
           -S localhost -U sa -P "$DB_PASSWORD" \
           -i /tmp/VolunteerSystem.sql \
           -C -N -o /tmp/sql_output.txt 2>&1; do
           
           COUNT=$((COUNT+1))
           if [ $COUNT -ge $MAX_RETRIES ]; then
               echo -e "${RED}Failed to execute SQL script after $MAX_RETRIES attempts.${NC}"
               echo "Last error from sqlcmd:"
               cat /tmp/sql_output.txt
               echo "Check container logs: sudo docker logs $CONTAINER_NAME"
               exit 1
           fi
           echo "Waiting for SQL Server to accept connections... ($COUNT/$MAX_RETRIES)"
           echo "Last error: $(head -n 1 /tmp/sql_output.txt)"
           sleep 10
        done
        
        echo -e "${GREEN}Database initialized!${NC}"
    fi
else
    echo -e "${RED}VolunteerSystem.sql not found at $SQL_FILE${NC}"
fi

# 5. Restore Dependencies
echo "Restoring project dependencies (skipping Windows-only UI)..."
cd "$SCRIPT_DIR"
dotnet restore VolunteerSystem.Avalonia/VolunteerSystem.Avalonia.csproj

echo -e "${GREEN}Setup Complete!${NC}"
echo "You can now build the project with: dotnet build VolunteerSystem.Core"
echo "Note: The WPF UI cannot run on Linux."
