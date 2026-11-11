FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish AdmissionSystem.Api/AdmissionSystem.Api.csproj -c Release -o /app

FROM build AS test
ENTRYPOINT ["dotnet", "test", "AdmissionSystem.Tests/AdmissionSystem.Tests.csproj"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AdmissionSystem.Api.dll"]
