FROM verwhelmingnull/aspnet:6.0-ffmpeg-h265
EXPOSE 80
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "Leon.Live.API.dll","--urls=http://0.0.0.0:80"]