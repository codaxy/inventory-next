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
COPY server/Codaxy.Inventory/*.csproj Codaxy.Inventory/
COPY server/Codaxy.Inventory.Tests.Unit/*.csproj Codaxy.Inventory.Tests.Unit/
COPY server/Codaxy.Inventory.Tests.Integration/*.csproj Codaxy.Inventory.Tests.Integration/
RUN dotnet restore Codaxy.Inventory/Codaxy.Inventory.csproj
COPY server/ ./
COPY --from=client /src/client/dist/ Codaxy.Inventory/wwwroot/
RUN dotnet publish Codaxy.Inventory/Codaxy.Inventory.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=server /app ./

# A fresh named volume takes its ownership from the image, so the directory is created and handed to
# the application's user before the volume is mounted over it — otherwise a non-root process cannot
# write its keys.
RUN mkdir -p /var/lib/inventory/keys && chown -R $APP_UID /var/lib/inventory

# Not root. The image serves static files and talks to Postgres; it needs nothing it owns.
USER $APP_UID

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

ENTRYPOINT ["dotnet", "Codaxy.Inventory.dll"]
