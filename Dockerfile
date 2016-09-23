FROM microsoft/dotnet:latest
COPY . /app
WORKDIR /app
 
RUN ["dotnet", "restore"]
RUN ["dotnet", "build"]
 
EXPOSE 5000/tcp
ENV ASPNETCORE_URLS http://*:PORT
ENV DOTNET_USE_POLLING_FILE_WATCHER true
 
ENTRYPOINT ["dotnet", "watch", "run"]