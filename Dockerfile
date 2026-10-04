# The client builds first: its output is what the server stage publishes as static content, so the
# two cannot be assembled independently.
FROM node:22-alpine AS client
WORKDIR /src/client
COPY client/package.json client/package-lock.json ./
RUN npm ci
COPY client/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS server
# No repository here for the pre-commit hook to install into.
ENV HUSKY=0
WORKDIR /src
COPY server/*.slnx ./
COPY server/Codaxy.Inventory.App/*.csproj Codaxy.Inventory.App/
COPY server/Codaxy.Inventory.Web/*.csproj Codaxy.Inventory.Web/
COPY server/Codaxy.Inventory.Tests.Unit/*.csproj Codaxy.Inventory.Tests.Unit/
COPY server/Codaxy.Inventory.Tests.Integration/*.csproj Codaxy.Inventory.Tests.Integration/
RUN dotnet restore Codaxy.Inventory.Web/Codaxy.Inventory.Web.csproj
COPY server/ ./
COPY --from=client /src/client/dist/ Codaxy.Inventory.Web/wwwroot/
# The version CI stamps, from the commit: `26.9.29+1222.eaea98a`. Its part before `+` is the
# assembly's version as well; `dev`, the default, and a test build's `dev.…` are not numbers and stay
# the informational version only. Not `VERSION`: MSBuild reads the environment as properties, ignoring
# case, so an argument of that name becomes `$(Version)` in every project and fails to parse.
ARG APP_VERSION=dev
RUN core="${APP_VERSION%%+*}"; \
    case "$core" in [0-9]*) number="-p:Version=$core" ;; *) number="" ;; esac; \
    dotnet publish Codaxy.Inventory.Web/Codaxy.Inventory.Web.csproj -c Release -o /app --no-restore \
        $number -p:InformationalVersion="$APP_VERSION" -p:IncludeSourceRevisionInInformationalVersion=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# wget only for compose's health check, which runs inside the container; the base image has no HTTP
# client. A stopgap: the lasting fix is the application probing itself (`--healthcheck`), which needs
# nothing installed and survives a move to a chiseled image.
RUN apt-get update \
    && apt-get install -y --no-install-recommends wget \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=server /app ./

# A fresh named volume takes its ownership from the image, so the directories are created and handed to
# the application's user before the volumes are mounted over them — otherwise a non-root process cannot
# write its keys or its log.
RUN mkdir -p /var/lib/inventory/keys /var/lib/inventory/logs && chown -R $APP_UID /var/lib/inventory

# Not root. The image serves static files and talks to Postgres; it needs nothing it owns.
USER $APP_UID

# The base image binds 8080 through ASPNETCORE_HTTP_PORTS; setting ASPNETCORE_URLS as well
# overrides it and logs a warning at every start.
EXPOSE 8080
# The server log in the folder the image made writable: the default, `logs` under /app, is root's,
# and the file sink fails without a word.
ENV ASPNETCORE_ENVIRONMENT=Production \
    ServerLog__Path=/var/lib/inventory/logs

ENTRYPOINT ["dotnet", "Codaxy.Inventory.Web.dll"]
