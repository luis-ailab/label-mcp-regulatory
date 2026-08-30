FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Label.Mcp.Regulatory.csproj", "./"]
RUN dotnet restore "Label.Mcp.Regulatory.csproj"

COPY . .
RUN dotnet publish "Label.Mcp.Regulatory.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .
	
ENTRYPOINT ["dotnet", "Label.Mcp.Regulatory.dll"]