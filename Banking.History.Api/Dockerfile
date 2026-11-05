
# Etapa 1 - Imagem base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

USER $APP_UID
WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

# Etapa 2 - Compilação
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copia o arquivo do projeto
COPY ["Banking.History.Api/Banking.History.Api.csproj", "Banking.History.Api/"]

# Restaura as dependências
RUN dotnet restore "Banking.History.Api/Banking.History.Api.csproj"

# Copia os demais arquivos
COPY . .

WORKDIR "/src/Banking.History.Api"

# Compila a aplicação
RUN dotnet build "Banking.History.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build \
    --no-restore

# Etapa 3 - Publicação
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "Banking.History.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Etapa 4 - Imagem final
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Banking.History.Api.dll"]
