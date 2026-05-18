FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY services/common/Common.csproj services/common/
COPY services/system/System.csproj services/system/
RUN dotnet restore services/system/System.csproj

COPY services/common/ services/common/
COPY services/system/ services/system/
RUN dotnet publish services/system/System.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5204
ENTRYPOINT ["dotnet", "System.dll"]
