FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY services/common/Common.csproj services/common/
COPY services/core-query/CoreQuery.csproj services/core-query/
RUN dotnet restore services/core-query/CoreQuery.csproj

COPY services/common/ services/common/
COPY services/core-query/ services/core-query/
RUN dotnet publish services/core-query/CoreQuery.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
EXPOSE 5203
ENTRYPOINT ["dotnet", "CoreQuery.dll"]
