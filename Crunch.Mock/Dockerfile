FROM microsoft/dotnet:latest
COPY . /app
WORKDIR /app
 
RUN ["dotnet", "restore"]
RUN ["dotnet", "build"]

EXPOSE 5000/tcp
ENV ASPNETCORE_URLS http://*
ENV RUNNING_URL http://0.0.0.0
ENV ASPNETCORE_ENVIRONMENT Development
 
CMD ["dotnet", "run"]