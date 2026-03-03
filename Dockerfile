FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["KidBank.API/KidBank.API.csproj", "KidBank.API/"]
COPY ["KidBank.Application/KidBank.Application.csproj", "KidBank.Application/"]
COPY ["KidBank.Domain/KidBank.Domain.csproj", "KidBank.Domain/"]
COPY ["KidBank.Infrastructure/KidBank.Infrastructure.csproj", "KidBank.Infrastructure/"]
RUN dotnet restore "KidBank.API/KidBank.API.csproj"
COPY . .
WORKDIR "/src/KidBank.API"
RUN dotnet build "KidBank.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KidBank.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KidBank.API.dll"]
