FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SmartResumeAnalyzer.API/SmartResumeAnalyzer.API.csproj", "SmartResumeAnalyzer.API/"]
COPY ["SmartResumeAnalyzer.Core/SmartResumeAnalyzer.Core.csproj", "SmartResumeAnalyzer.Core/"]
COPY ["SmartResumeAnalyzer.Infrastructure/SmartResumeAnalyzer.Infrastructure.csproj", "SmartResumeAnalyzer.Infrastructure/"]
RUN dotnet restore "SmartResumeAnalyzer.API/SmartResumeAnalyzer.API.csproj"
COPY . .
WORKDIR "/src/SmartResumeAnalyzer.API"
RUN dotnet build "SmartResumeAnalyzer.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmartResumeAnalyzer.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p uploads/cvs
ENTRYPOINT ["dotnet", "SmartResumeAnalyzer.API.dll"]