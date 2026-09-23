# Build with this directory as the context and --platform linux/amd64.
# The pinned Alpine AOT SDK includes clang and the musl static libraries.
FROM mcr.microsoft.com/dotnet/sdk:10.0.401-alpine3.23-aot-amd64@sha256:240a20b94625153877c8ec4ed0394d3396f243a5663fe74b8f601c31f1dac3fc AS build

WORKDIR /src
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1
ARG PROJECT=ImdsGuest.csproj
COPY . .

# Blink's qualified guest profile is static Linux x64 ELF, without an ELF
# interpreter or shared-library dependencies. PIE is disabled intentionally.
RUN dotnet publish "$PROJECT" -c Release -r linux-musl-x64 --self-contained true \
    -o /out \
    -p:RuntimeFrameworkVersion=10.0.12 \
    -p:PublishAot=true \
    -p:StaticExecutable=true \
    -p:PositionIndependentExecutable=false \
    -p:InvariantGlobalization=true

# Export the publish directory with --output; this stage is an artifact, not
# a container that launches the application. Blink executes the exported ELF.
FROM scratch AS export
COPY --from=build /out/ /
