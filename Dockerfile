FROM mcr.microsoft.com/dotnet/sdk:8.0 as build

# Install node
RUN apt-get update && apt-get install -y nodejs npm
RUN npm install --global yarn

WORKDIR /workspace
COPY . .
RUN dotnet tool restore

RUN dotnet run -- Bundle


FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY --from=build /workspace/deploy /app
WORKDIR /app
EXPOSE 8085
ENTRYPOINT [ "dotnet", "Server.dll" ]
