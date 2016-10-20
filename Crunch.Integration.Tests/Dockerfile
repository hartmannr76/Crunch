FROM microsoft/dotnet:latest
COPY . /app
WORKDIR /app
 
RUN ["dotnet", "restore"]
RUN ["dotnet", "build"]

ENV DOTNET_USE_POLLING_FILE_WATCHER true
 
CMD ["dotnet", "watch", "test"]