FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/EmployeeManagement.API/EmployeeManagement.API.csproj", "src/EmployeeManagement.API/"]
RUN dotnet restore "src/EmployeeManagement.API/EmployeeManagement.API.csproj"
COPY . .
RUN dotnet publish "src/EmployeeManagement.API/EmployeeManagement.API.csproj" -c Release -o /publish --no-restore

FROM runtime AS final
WORKDIR /app
COPY --from=build /publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "EmployeeManagement.API.dll"]
