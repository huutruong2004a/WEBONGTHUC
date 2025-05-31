# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy C# project file(s) and restore dependencies
# Điều này giả định Dockerfile nằm cùng cấp với file .csproj của bạn.
COPY *.csproj ./
RUN dotnet restore "./WEB_CONG_THUC.csproj" # Hoặc chỉ cần tên file .csproj nếu nó là duy nhất

# Copy the rest of the application code
COPY . .

# Publish the application
# Sử dụng tên file .csproj để publish.
RUN dotnet publish "./WEB_CONG_THUC.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# App Engine Flexible Environment sẽ tự động expose port 8080 và forward traffic.
# ASPNETCORE_URLS sẽ được set bởi app.yaml.
# EXPOSE 8080 # Dòng này có thể có hoặc không, App Engine sẽ tự xử lý.

# Entry point for the application (đảm bảo tên DLL là WEB_CONG_THUC.dll)
ENTRYPOINT ["dotnet", "WEB_CONG_THUC.dll"]