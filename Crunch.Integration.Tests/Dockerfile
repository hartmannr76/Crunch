FROM microsoft/dotnet:1.0-sdk-projectjson
COPY . /app
WORKDIR /app
 
RUN ["dotnet", "restore"]
RUN ["dotnet", "build"]

ADD wait-for-it.sh /wait-for-it.sh

ENV DOTNET_USE_POLLING_FILE_WATCHER true
 
CMD ["dotnet", "watch", "test"]