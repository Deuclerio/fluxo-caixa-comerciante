FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY FluxoCaixa.slnx ./
COPY src/Shared.Contracts/Shared.Contracts.csproj src/Shared.Contracts/
COPY src/Consolidacao.Domain/Consolidacao.Domain.csproj src/Consolidacao.Domain/
COPY src/Consolidacao.Application/Consolidacao.Application.csproj src/Consolidacao.Application/
COPY src/Consolidacao.Infrastructure/Consolidacao.Infrastructure.csproj src/Consolidacao.Infrastructure/
COPY src/Consolidacao.Api/Consolidacao.Api.csproj src/Consolidacao.Api/
RUN dotnet restore src/Consolidacao.Api/Consolidacao.Api.csproj
COPY src/Shared.Contracts/ src/Shared.Contracts/
COPY src/Consolidacao.Domain/ src/Consolidacao.Domain/
COPY src/Consolidacao.Application/ src/Consolidacao.Application/
COPY src/Consolidacao.Infrastructure/ src/Consolidacao.Infrastructure/
COPY src/Consolidacao.Api/ src/Consolidacao.Api/
RUN dotnet publish src/Consolidacao.Api/Consolidacao.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Consolidacao.Api.dll"]
