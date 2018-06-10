FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ProcessManager/ProcessManager.csproj ProcessManager/
RUN dotnet restore ProcessManager/ProcessManager.csproj
COPY . .
WORKDIR /src/ProcessManager
RUN dotnet build ProcessManager.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ProcessManager.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ProcessManager.dll"]
