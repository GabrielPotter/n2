FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY services/common/Common.csproj services/common/
COPY services/catalog/Catalog.csproj services/catalog/
RUN dotnet restore services/catalog/Catalog.csproj

COPY services/common/ services/common/
COPY services/catalog/ services/catalog/
RUN dotnet publish services/catalog/Catalog.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
EXPOSE 5201
ENTRYPOINT ["dotnet", "Catalog.dll"]
