# todo:
# install openssh dependencies

FROM ubuntu:latest AS build-openssh

RUN apt update
RUN apt install clang libcrypto git

RUN git clone https://github.com/openssh/openssh-portable
RUN cd openssh-portable

# build openssh
RUN autoreconf
RUN ./configure

RUN make

# build container
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["KeyGenerationApi.csproj", "."]
RUN dotnet restore "./KeyGenerationApi.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "KeyGenerationApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KeyGenerationApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "KeyGenerationApiAPI.dll"]