# Set the base image as the .NET 6.0 SDK (this includes the runtime)
FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env

# Copy everything and publish the release (publish implicitly restores and builds)
COPY . ./
RUN dotnet publish ./JsonFileExtractor.GitHubAction/JsonFileExtractor.GitHubAction.csproj -c Release -o out --no-self-contained

# Label the container
LABEL maintainer="Christophe Ohl <chris.ohl67@gmail.com>"
LABEL repository="https://github.com/chrisfactory/GithubActions.git"
LABEL homepage="https://github.com/chrisfactory/GithubActions.git"

# Label as GitHub action
LABEL com.github.actions.name="JSON properties extractor"
LABEL com.github.actions.description="Extract propeties Json file"
LABEL com.github.actions.icon="sliders"
LABEL com.github.actions.color="purple"

# Relayer the .NET SDK, anew with the build output 
FROM mcr.microsoft.com/dotnet/runtime:6.0
COPY --from=build-env /out .
ENTRYPOINT [ "dotnet", "/JsonFileExtractor.GitHubAction.dll" ]
