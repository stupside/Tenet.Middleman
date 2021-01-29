#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#https://github.com/dotnet/dotnet-docker/blob/master/documentation/scenarios/installing-dotnet.md
#https://github.com/dotnet/dotnet-docker/tree/master/samples/dotnetapp
#https://github.com/dotnet/dotnet-docker/blob/master/samples/aspnetapp/README.md
#https://github.com/dotnet/dotnet-docker/blob/master/samples/dotnetapp/README.md

#FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build

ENV         ASPNETCORE_ENVIRONMENT =    Production
ENV         ASPNETCORE_URLS             http://*:5004
EXPOSE      5004

# https://hub.docker.com/_/microsoft-dotnet

WORKDIR /source

# copy csproj and restore as distinct layers
COPY Tenet.Api/*.csproj Tenet.Api/
COPY Tenet.Application/*.csproj Tenet.Application/
COPY Tenet.Domain/*.csproj Tenet.Domain/
COPY Tenet.Infrastructure/*.csproj Tenet.Infrastructure/
COPY Tenet.Persistence/*.csproj Tenet.Persistence/

RUN dotnet restore Tenet.Api/Tenet.Api.csproj

# copy and build app and libraries
COPY Tenet.Api/ Tenet.Api/
COPY Tenet.Application/ Tenet.Application/
COPY Tenet.Domain/ Tenet.Domain/
COPY Tenet.Infrastructure/ Tenet.Infrastructure/
COPY Tenet.Persistence/ Tenet.Persistence/

WORKDIR /source/Tenet.Api
RUN dotnet build -c release --no-restore

# test stage -- exposes optional entrypoint
# target entrypoint with: docker build --target test
FROM build AS test
WORKDIR /source/tests
COPY tests/ .
ENTRYPOINT ["dotnet", "test", "--logger:trx"]

FROM build AS publish
RUN dotnet publish -c release --no-build -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "tenetmiddleman.dll"]