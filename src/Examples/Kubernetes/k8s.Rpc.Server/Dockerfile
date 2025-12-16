#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /
COPY . .
RUN dotnet restore "/Examples/Kubernetes/k8s.Rpc.Server/k8s.Rpc.Server.csproj"
WORKDIR "/Examples/Kubernetes/k8s.Rpc.Server"
RUN dotnet build "k8s.Rpc.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "k8s.Rpc.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "k8s.Rpc.Server.dll"]