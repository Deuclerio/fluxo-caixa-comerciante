FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY FluxoCaixa.slnx ./
COPY src/Shared.Contracts/Shared.Contracts.csproj src/Shared.Contracts/
COPY src/Lancamentos.Domain/Lancamentos.Domain.csproj src/Lancamentos.Domain/
COPY src/Lancamentos.Application/Lancamentos.Application.csproj src/Lancamentos.Application/
COPY src/Lancamentos.Infrastructure/Lancamentos.Infrastructure.csproj src/Lancamentos.Infrastructure/
COPY src/Lancamentos.Api/Lancamentos.Api.csproj src/Lancamentos.Api/
RUN dotnet restore src/Lancamentos.Api/Lancamentos.Api.csproj
COPY src/Shared.Contracts/ src/Shared.Contracts/
COPY src/Lancamentos.Domain/ src/Lancamentos.Domain/
COPY src/Lancamentos.Application/ src/Lancamentos.Application/
COPY src/Lancamentos.Infrastructure/ src/Lancamentos.Infrastructure/
COPY src/Lancamentos.Api/ src/Lancamentos.Api/
RUN dotnet publish src/Lancamentos.Api/Lancamentos.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Lancamentos.Api.dll"]
