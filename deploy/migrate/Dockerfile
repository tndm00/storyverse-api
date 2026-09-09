# syntax=docker/dockerfile:1
# One-shot database migrator. Runs `dotnet ef database update` for a single
# service, then exits 0. Deploy as a Cloud Run Job (recommended) or `docker run`
# it manually before rolling out a service that has schema changes.
#
#   build ARG  INFRA_PROJECT   e.g. src/Services/Contents/Content.Infrastructure
#   run   ENV  DB_ENV_VAR      the design-time factory's env var, e.g. CONTENT_DB_CONNECTION
#   run   ENV  DB_CONNECTION   the target Postgres connection string
#
# Only Authentication + Content have migrations today; the other services get a
# design-time factory + migrations when they are built.

FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /src

ENV PATH="$PATH:/root/.dotnet/tools" \
    DOTNET_CLI_TELEMETRY_OPTOUT=1
RUN dotnet tool install --global dotnet-ef --version 8.0.*

ARG INFRA_PROJECT
ENV INFRA_PROJECT=${INFRA_PROJECT}

COPY . .
RUN dotnet restore "${INFRA_PROJECT}"

# The factory reads $DB_ENV_VAR for the connection string; export it from the
# generic DB_CONNECTION passed by the job.
ENTRYPOINT ["/bin/sh", "-c", "export \"${DB_ENV_VAR:?set DB_ENV_VAR}=${DB_CONNECTION:?set DB_CONNECTION}\" && exec dotnet ef database update --project \"$INFRA_PROJECT\" --startup-project \"$INFRA_PROJECT\""]
