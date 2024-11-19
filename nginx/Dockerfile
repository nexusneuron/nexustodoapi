FROM mcr.microsoft.com/powershell:lts-windowsservercore-ltsc2022 as build-stage

ENV NGINX_VERSION 1.27.2

SHELL ["pwsh", "-command"]
RUN Invoke-WebRequest "http://nginx.org/download/nginx-$($env:NGINX_VERSION).zip" -OutFile C:\nginx.zip
RUN Expand-Archive C:\nginx.zip C:\nginx
RUN Remove-Item "C:\nginx\nginx-$($env:NGINX_VERSION)\conf\*.conf" -Verbose
RUN New-Item -type directory "c:\\nginx\\enabled-sites\\html"
RUN Remove-Item C:\nginx.zip

WORKDIR c:\\nginx\\nginx-$NGINX_VERSION
COPY ./index.html c:/nginx/enabled-sites/html
COPY ./nginx.conf c:/nginx/nginx-$NGINX_VERSION/conf/nginx.conf

EXPOSE 80
ENTRYPOINT ["nginx.exe"]