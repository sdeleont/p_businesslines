FROM mcr.microsoft.com/dotnet/core/aspnet:2.1 AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS build
WORKDIR /src
COPY LineaNegociosBackend.sln ./
COPY NuGet.Config ./
COPY LineaNegociosBackend/*.csproj ./LineaNegociosBackend/
COPY Core/*.csproj ./Core/


RUN dotnet restore LineaNegociosBackend/LineaNegociosBackend.csproj 

COPY . .

WORKDIR /src/LineaNegociosBackend
RUN dotnet build -c Release -o /app-bin

FROM build AS publish
RUN dotnet publish -c Release -o /app
RUN ls /app-bin


FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENV ASPNETCORE_URLS http://*:8080
ENTRYPOINT ["dotnet", "LineaNegociosBackend.dll"]
EXPOSE 8080/tcp