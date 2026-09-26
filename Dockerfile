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
RUN dotnet publish Codaxy.Inventory.Web/Codaxy.Inventory.Web.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=server /app ./

# A fresh named volume takes its ownership from the image, so the directories are created and handed to
# the application's user before the volumes are mounted over them — otherwise a non-root process cannot
# write its keys or its log.
RUN mkdir -p /var/lib/inventory/keys /var/lib/inventory/logs && chown -R $APP_UID /var/lib/inventory

# Not root. The image serves static files and talks to Postgres; it needs nothing it owns.
USER $APP_UID

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

ENTRYPOINT ["dotnet", "Codaxy.Inventory.Web.dll"]
