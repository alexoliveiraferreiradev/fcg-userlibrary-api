FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["src/Fcg.UserLibrary.API/Fcg.UserLibrary.API.csproj", "Fcg.UserLibrary.API/"]
COPY ["src/Fcg.UserLibrary.Application/Fcg.UserLibrary.Application.csproj", "Fcg.UserLibrary.Application/"]
COPY ["src/Fcg.UserLibrary.Domain/Fcg.UserLibrary.Domain.csproj", "Fcg.UserLibrary.Domain/"]
COPY ["src/Fcg.UserLibrary.Infrastructure/Fcg.UserLibrary.Infrastructure.csproj", "Fcg.UserLibrary.Infrastructure/"]

RUN dotnet restore "./Fcg.UserLibrary.API/Fcg.UserLibrary.API.csproj"

COPY src/ .
WORKDIR "/src/Fcg.UserLibrary.API"
RUN dotnet build "./Fcg.UserLibrary.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

RUN dotnet ef migrations bundle --self-contained -r linux-x64 -o /app/build/efbundle

FROM build AS publish
RUN dotnet publish "./Fcg.UserLibrary.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root

COPY --from=build /app/build/efbundle ./efbundle
RUN chmod +x ./efbundle
USER app

ENTRYPOINT []
CMD ["dotnet", "Fcg.UserLibrary.API.dll"]