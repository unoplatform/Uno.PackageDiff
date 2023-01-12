# Set the base image as the .NET 5.0 SDK (this includes the runtime)
FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env

# Copy everything and publish the release (publish implicitly restores and builds)
WORKDIR /app
COPY . ./
RUN dotnet publish ./src/Uno.PackageDiff/Uno.PackageDiff.csproj -c Release -o /app/out --no-self-contained


# Label the container
LABEL maintainer="Jérôme Laban <djerome.laban@nventive.com>"
LABEL repository="https://github.com/unoplatform/Uno.PackageDiff"
LABEL homepage="https://github.com/unoplatform/Uno.PackageDiff"

# Label as GitHub action
LABEL com.github.actions.name="NuGet Package Diffing Tool"
# Limit to 160 characters
LABEL com.github.actions.description="NuGet Package Diffing Tool."
# See branding:
# https://docs.github.com/actions/creating-actions/metadata-syntax-for-github-actions#branding
LABEL com.github.actions.icon="award"
LABEL com.github.actions.color="green"

# Relayer the .NET SDK, anew with the build output
FROM mcr.microsoft.com/dotnet/sdk:7.0
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "/Uno.PackageDiff.dll" ]
