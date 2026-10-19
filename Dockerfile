FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json ./
COPY HiveContracts/ HiveContracts/
COPY HiveLib/ HiveLib/
COPY HiveClient/ HiveClient/
COPY HiveServer/ HiveServer/
RUN dotnet publish HiveServer/HiveServer.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app .
ENV HIVE_PORT=7777
EXPOSE 7777
ENTRYPOINT ["dotnet", "HiveServer.dll"]
