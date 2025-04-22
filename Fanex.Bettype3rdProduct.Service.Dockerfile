FROM hub.nexdev.net/base/dotnet/core/sdk:9.0-alpine AS builder
WORKDIR /src

COPY ./Fanex.Bettype3rdProduct.Service/Fanex.Bettype3rdProduct.Business ./Fanex.Bettype3rdProduct.Business
COPY ./Fanex.Bettype3rdProduct.Service/Fanex.Bettype3rdProduct.Service/Fanex.Bettype3rdProduct.Service.csproj ./Fanex.Bettype3rdProduct.Service/Fanex.Bettype3rdProduct.Service.csproj

WORKDIR /src/Fanex.Bettype3rdProduct.Service
RUN dotnet nuget add source https://nuget.nexdev.net/api/v2
RUN --mount=type=cache,target=/root/.nuget/packages \
	dotnet restore -v n -r linux-x64

COPY ./Fanex.Bettype3rdProduct.Service/Fanex.Bettype3rdProduct.Service .

RUN --mount=type=cache,target=/root/.nuget/packages \
	dotnet publish -r linux-x64 --self-contained false -c Release -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:9.0-nap-mail-core

# Don’t run as root
RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app
USER dotnetuser
#

ENV TZ="America/La_Paz" 

WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "Fanex.Bettype3rdProduct.Service.dll"]
