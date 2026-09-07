FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY publish/ .

ENTRYPOINT ["dotnet", "Label.Mcp.Regulatory.dll"]