FROM mcr.microsoft.com/dotnet/sdk:10.0 as build

# Install node
RUN curl -sL https://deb.nodesource.com/setup_22.x | bash -
RUN apt-get install -y nodejs
RUN npm install --global yarn

WORKDIR /workspace
COPY . .
RUN dotnet tool restore

RUN dotnet run -- Bundle

FROM mcr.microsoft.com/dotnet/aspnet:10.0
COPY --from=build /workspace/deploy /app
WORKDIR /app
EXPOSE 8085
ENTRYPOINT [ "dotnet", "Server.dll" ]
