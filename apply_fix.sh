#!/bin/bash
echo "Applying database fix for Points mechanism..."
sudo docker cp ApplyPointsSchema.sql sql_server_volunteer:/tmp/ApplyPointsSchema.sql
sudo docker exec sql_server_volunteer /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "VolunteerSystem_2024!" -C -i /tmp/ApplyPointsSchema.sql
echo "Fix applied! You can now run the application."
