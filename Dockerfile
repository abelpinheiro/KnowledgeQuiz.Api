# Stage 1 - Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0@sha256:3fcf6f1e809c0553f9feb222369f58749af314af6f063f389cbd2f913b4ad556 AS build
WORKDIR /App

# Copy project files and restore as distinct layers 
# This is improves build performance as restore will only be called if dependencies changed on the csproj. If not, use previous layer from cache.
COPY *.sln ./
COPY src/*/*.csproj ./src/
COPY tests/*/*.csproj ./tests/
RUN dotnet restore KnowledgeQuiz.Api.sln

# Copy the rest of the code
COPY . ./

# Build and publish a release
RUN dotnet publish KnowledgeQuiz.Api.WebApi/KnowledgeQuiz.Api.WebApi.csproj -c Release -o out

# Stage 2 - Set up the production environment building the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0@sha256:b4bea3a52a0a77317fa93c5bbdb076623f81e3e2f201078d89914da71318b5d8
WORKDIR /App

# Copy the built app from the build 
COPY --from=build /App/out .
ENTRYPOINT ["dotnet", "KnowledgeQuiz.Api.WebApi.dll"]