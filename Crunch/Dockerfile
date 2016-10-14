FROM microsoft/dotnet:latest
COPY . /app
WORKDIR /app
 
RUN ["dotnet", "restore"]
RUN ["dotnet", "build"]

EXPOSE 5000/tcp
ENV ASPNETCORE_URLS http://*
ENV RUNNING_URL http://0.0.0.0
ENV DOTNET_USE_POLLING_FILE_WATCHER true
 
CMD ["dotnet", "watch", "run"]