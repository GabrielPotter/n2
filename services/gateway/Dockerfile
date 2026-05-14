FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY services/common/Common.csproj services/common/
COPY services/gateway/Gateway.csproj services/gateway/
RUN dotnet restore services/gateway/Gateway.csproj

COPY services/common/ services/common/
COPY services/gateway/ services/gateway/
RUN dotnet publish services/gateway/Gateway.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
EXPOSE 5100
ENTRYPOINT ["dotnet", "Gateway.dll"]
