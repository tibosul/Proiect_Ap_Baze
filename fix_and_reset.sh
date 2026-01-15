#!/bin/bash
set -e

# Password
SA_PASSWORD="VolunteerSystem_2024!"
CONTAINER_NAME="sql_server_volunteer"

echo "Stopping existing SQL Server container..."
sudo docker rm -f $CONTAINER_NAME || true

# Note: We do NOT remove the volume here to allow persistence.
# If you want a fresh start, run: sudo docker volume rm volunteer_mssql_data
# We check if volume exists, if not, we might need a reset? No, SQL Server handles existing volume.

echo "Starting new SQL Server container..."
sudo docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=$SA_PASSWORD" \
   -p 1433:1433 --name $CONTAINER_NAME \
   -v volunteer_mssql_data:/var/opt/mssql \
   -d mcr.microsoft.com/mssql/server:2022-latest

echo "Waiting for SQL Server to accept connections..."
# Loop wait up to 60 seconds
for i in {1..60}; do
    if sudo docker exec $CONTAINER_NAME /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" > /dev/null 2>&1; then
        echo "SQL Server is ready."
        break
    fi
    echo "Waiting... ($i/60)"
    sleep 2
done

echo "Initializing Database..."
# This might fail if DB already exists (valid persistence), so we allow failure or check first.
# But for now, let's just attempt to run it. If DB exists, CREATE DATABASE will fail, which is fine?
# Actually, if we want persistence, we shouldn't re-seed every time unless we handle "IF NOT EXISTS".
# But the user complains "cannot open database", meaning it's missing.
# So we run the SQL. It drops and recreated DB? Let's check SQL file.
# The SQL file typically drops DB if exists. So running this WILL RESET DATA.
# To support persistence, we should only run SQL if DB doesn't exist.

# Check if DB exists
# We use a temporary variable to capture output and exit code to be safe
echo "Checking for existing database..."
set +e # Temporarily disable exit-on-error to handle sqlcmd failure manually

# Check if DB exists using sqlcmd. We look for 'YES' in the output.
# We also check the exit code. If sqlcmd fails (connection error), we MUST NOT assume DB is missing.
CHECK_OUTPUT=$(sudo docker exec $CONTAINER_NAME /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" -C -Q "IF DB_ID('VolunteerSystem') IS NOT NULL PRINT 'YES'" 2>&1)
CHECK_EXIT_CODE=$?
set -e # Re-enable exit-on-error

if [ $CHECK_EXIT_CODE -ne 0 ]; then
    echo "ERROR: Failed to connect to SQL Server to check database existence."
    echo "Output: $CHECK_OUTPUT"
    echo "Aborting to prevent accidental data loss."
    exit 1
fi

if echo "$CHECK_OUTPUT" | grep -q "YES"; then
    echo "Database 'VolunteerSystem' already exists. Skipping initialization to preserve data."
else
    echo "Database missing. Running initialization script..."
    sudo docker cp VolunteerSystem.sql $CONTAINER_NAME:/tmp/VolunteerSystem.sql
    sudo docker exec $CONTAINER_NAME /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$SA_PASSWORD" -C -i /tmp/VolunteerSystem.sql
fi

echo "Setup complete. User: sa , Password: $SA_PASSWORD"
