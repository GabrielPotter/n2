FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY services/common/Common.csproj services/common/
COPY services/core-editor/CoreEditor.csproj services/core-editor/
RUN dotnet restore services/core-editor/CoreEditor.csproj

COPY services/common/ services/common/
COPY services/core-editor/ services/core-editor/
RUN dotnet publish services/core-editor/CoreEditor.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
EXPOSE 5202
ENTRYPOINT ["dotnet", "CoreEditor.dll"]
