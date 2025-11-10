# 阶段 1: 最终运行环境的基础镜像
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root
ENV TZ=Asia/Shanghai
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
RUN mkdir -p /app/certs
RUN mkdir -p /app/wwwroot/uploads && chmod -R 777 /app/wwwroot/uploads
WORKDIR /app
EXPOSE 8082
EXPOSE 8083

# ------------------------------------------------------------------------------------

# 阶段 2: 构建和发布阶段 (合并为一步)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Packages.props", "./"]
COPY ["FollowUp/NuGet.Config", "./"]
COPY ["FollowUp/FollowUp.csproj", "FollowUp/"]
COPY ["Bio.Migration/Bio.Migration.csproj", "Bio.Migration/"]
COPY ["Bio.FormSetLib/Bio.FormSetLib.csproj", "Bio.FormSetLib/"]
RUN dotnet restore "FollowUp/FollowUp.csproj"
COPY . .
WORKDIR "/src/FollowUp"
RUN dotnet publish "FollowUp.csproj" -c $BUILD_CONFIGURATION -f net9.0 -o /app/publish /p:UseAppHost=false --disable-parallel

# ------------------------------------------------------------------------------------

# 阶段 3: 创建最终的生产镜像
FROM base AS final
WORKDIR /app
ENV ASPNETCORE_URLS="https://+:8083;http://+:8082"
ENV ASPNETCORE_ENVIRONMENT="Production"

# 直接从 'build' 阶段复制已发布的应用文件和证书，这里不再有歧义
COPY --from=build /app/publish .
COPY --from=build /src/FollowUp/certs/wxdebug.bio-ocean.com.pfx /app/certs/

ENTRYPOINT ["dotnet", "FollowUp.dll"]