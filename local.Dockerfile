FROM mcr.microsoft.com/dotnet/sdk:10.0 as build

# Install node
RUN curl -sL https://deb.nodesource.com/setup_22.x | bash -
RUN apt-get install -y nodejs
RUN npm install --global yarn

WORKDIR /workspace
COPY .config/ .config/
RUN dotnet tool restore