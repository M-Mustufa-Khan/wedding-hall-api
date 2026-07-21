FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY WeddingHallAPI/*.csproj WeddingHallAPI/
RUN dotnet restore WeddingHallAPI/WeddingHallAPI.csproj
COPY . .
RUN dotnet publish WeddingHallAPI/WeddingHallAPI.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "WeddingHallAPI.dll"]