# Build from the repository root: docker build -t splitwiseclone-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SplitwiseClone.slnx ./
COPY Api/SplitwiseClone.Api.csproj Api/
COPY Application/SplitwiseClone.Application.csproj Application/
COPY Domain/SplitwiseClone.Domain.csproj Domain/
COPY Infrastructure/SplitwiseClone.Infrastructure.csproj Infrastructure/
RUN dotnet restore Api/SplitwiseClone.Api.csproj

COPY Api/ Api/
COPY Application/ Application/
COPY Domain/ Domain/
COPY Infrastructure/ Infrastructure/
WORKDIR /src/Api
RUN dotnet publish SplitwiseClone.Api.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
# Npgsql's GSSAPI support dynamically loads this library on Linux.
RUN apt-get update \
    && apt-get install --yes --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SplitwiseClone.Api.dll"]
