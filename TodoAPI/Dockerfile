#FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-1809ltsc2022 as base
FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-ltsc2022 as base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

#FROM mcr.microsoft.com/dotnet/sdk:8.0-nanoserver-1809ltsc2022 AS build
FROM mcr.microsoft.com/dotnet/sdk:8.0-nanoserver-ltsc2022 AS build
COPY . /src
WORKDIR /src
#RUN ls
RUN dotnet build "TodoAPI/TodoAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TodoAPI/TodoAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TodoAPI.dll"]
