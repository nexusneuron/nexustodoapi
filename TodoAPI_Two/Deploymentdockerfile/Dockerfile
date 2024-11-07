ARG git_url_csproj=https://github.com/nexusneuron/nexustodoapi.git#master:

FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-ltsc2022 as base
#FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-1809 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0-nanoserver-ltsc2022 AS build
#FROM mcr.microsoft.com/dotnet/sdk:8.0-nanoserver-1809 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
#COPY ["TodoAPI_Two/TodoAPI_Two.csproj" , "TodoAPI_Two/"]
COPY [ "%git_url_csproj%TodoAPI_Two/TodoAPI_Two.csproj" , "TodoAPI_Two/"]
#RUN dotnet restore "./TodoAPI_Two/TodoAPI_Two.csproj"
RUN dotnet restore "%git_url_csproj%TodoAPI_Two/TodoAPI_Two.csproj"
COPY . .
WORKDIR "/src/TodoAPI_Two"
#RUN dotnet build "./TodoAPI_Two.csproj" -c %BUILD_CONFIGURATION% -o /app/build
RUN dotnet build "%git_url_csproj%TodoAPI_Two/TodoAPI_Two.csproj" -c %BUILD_CONFIGURATION% -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./TodoAPI_Two.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=false
RUN dotnet publish "%git_url_csproj%TodoAPI_Two/TodoAPI_Two.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TodoAPI_Two.dll"]