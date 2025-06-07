FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY GerenciamentoBiblioteca.API/*.csproj ./GerenciamentoBiblioteca.API/
COPY GerenciamentoBiblioteca.Application/*.csproj ./GerenciamentoBiblioteca.Application/
COPY GerenciamentoBiblioteca.Domain/*.csproj ./GerenciamentoBiblioteca.Domain/
COPY GerenciamentoBiblioteca.Infrastructure/*.csproj ./GerenciamentoBiblioteca.Infrastructure/

RUN dotnet restore GerenciamentoBiblioteca.API/GerenciamentoBiblioteca.API.csproj

COPY . .

RUN dotnet build GerenciamentoBiblioteca.API/GerenciamentoBiblioteca.API.csproj -c Release --no-restore

RUN dotnet publish GerenciamentoBiblioteca.API/GerenciamentoBiblioteca.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS runtime
WORKDIR /app

RUN dotnet tool install --global dotnet-ef --version 9.0.5
ENV PATH="${PATH}:/root/.dotnet/tools"

RUN dotnet ef --version

COPY --from=build /app/publish .

COPY --from=build /app /app/source

RUN echo '#!/bin/bash\n\
set -e\n\
\n\
echo "Waiting for SQL Server to be ready..."\n\
\n\
# Wait for SQL Server to be available\n\
until /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P "YourStrong!Passw0rd" -Q "SELECT 1" > /dev/null 2>&1; do\n\
  echo "SQL Server is unavailable - sleeping for 2 seconds"\n\
  sleep 2\n\
done\n\
\n\
echo "SQL Server is up - running migrations"\n\
\n\
cd /app/source\n\
\n\
if [ ! -d "GerenciamentoBiblioteca.Infrastructure/Migrations" ] || [ -z "$(ls -A GerenciamentoBiblioteca.Infrastructure/Migrations 2>/dev/null)" ]; then\n\
  echo "No migrations found, creating initial migration..."\n\
  dotnet ef migrations add InitialCreate --project GerenciamentoBiblioteca.Infrastructure --startup-project GerenciamentoBiblioteca.API\n\
fi\n\
\n\
echo "Checking for pending migrations..."\n\
\n\
set +e  # Temporarily disable exit on error\n\
\n\
MIGRATION_FILES=$(find GerenciamentoBiblioteca.Infrastructure/Migrations -name "*.cs" -not -name "*Designer.cs" -not -name "*ModelSnapshot.cs" 2>/dev/null | wc -l)\n\
echo "Migration files found: $MIGRATION_FILES"\n\
\n\
DB_CHECK_RESULT=$(dotnet ef migrations list --project GerenciamentoBiblioteca.Infrastructure --startup-project GerenciamentoBiblioteca.API --no-build 2>&1)\n\
DB_CHECK_EXIT_CODE=$?\n\
\n\
if [ $DB_CHECK_EXIT_CODE -eq 0 ]; then\n\
  APPLIED_COUNT=$(echo "$DB_CHECK_RESULT" | grep "^[[:space:]]*\\*" | wc -l)\n\
  TOTAL_COUNT=$(echo "$DB_CHECK_RESULT" | grep -E "^[[:space:]]*(\\*|[^[:space:]])" | grep -v "Build succeeded" | grep -v "warnings" | wc -l)\n\
  \n\
  echo "Applied migrations in database: $APPLIED_COUNT"\n\
  echo "Total migrations available: $TOTAL_COUNT"\n\
  \n\
  if [ "$APPLIED_COUNT" -lt "$TOTAL_COUNT" ] || [ "$MIGRATION_FILES" -gt "$APPLIED_COUNT" ]; then\n\
    echo "Pending migrations detected. Updating database..."\n\
    dotnet ef database update --project GerenciamentoBiblioteca.Infrastructure --startup-project GerenciamentoBiblioteca.API --verbose\n\
    echo "Database updated successfully"\n\
  else\n\
    echo "Database is up to date. No migrations needed."\n\
  fi\n\
else\n\
  # Database might not exist or there\'s an error, apply migrations\n\
  echo "Database not found or error checking migrations. Applying all migrations..."\n\
  dotnet ef database update --project GerenciamentoBiblioteca.Infrastructure --startup-project GerenciamentoBiblioteca.API --verbose\n\
  echo "Database created/updated successfully"\n\
fi\n\
\n\
set -e  # Re-enable exit on error\n\
\n\
echo "Starting the application..."\n\
cd /app\n\
exec dotnet GerenciamentoBiblioteca.API.dll' > /app/entrypoint.sh

RUN apt-get update && apt-get install -y curl gnupg2 software-properties-common && \
    curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - && \
    curl https://packages.microsoft.com/config/ubuntu/20.04/prod.list > /etc/apt/sources.list.d/mssql-release.list && \
    apt-get update && \
    ACCEPT_EULA=Y apt-get install -y mssql-tools unixodbc-dev && \
    echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> ~/.bashrc

RUN chmod +x /app/entrypoint.sh

EXPOSE 5000

ENTRYPOINT ["/app/entrypoint.sh"]
