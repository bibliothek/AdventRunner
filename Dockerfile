FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build

# Install node
RUN curl -sL https://deb.nodesource.com/setup_14.x | bash
RUN apt-get update && apt-get install -y nodejs

WORKDIR /workspace
COPY . .
RUN dotnet tool restore

RUN dotnet fake build -t Bundle


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
COPY --from=build /workspace/deploy /app
WORKDIR /app
EXPOSE 8085
ENTRYPOINT [ "dotnet", "Server.dll" ]
