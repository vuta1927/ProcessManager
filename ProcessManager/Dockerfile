FROM microsoft/aspnetcore:2.0 AS base
RUN apt-get update && apt-get install -y openssh-server wget iputils-ping net-tools nano ffmpeg
RUN mkdir /var/run/sshd && mkdir -p logs
RUN useradd vuta && echo 'vuta:Echo@1927' | chpasswd
RUN echo adduser vuta sudo
RUN echo 'root:Echo@1927' | chpasswd
RUN sed -i 's/PermitRootLogin prohibit-password/PermitRootLogin yes/' /etc/ssh/sshd_config
RUN sed 's@session\s*required\s*pam_loginuid.so@session optional pam_loginuid.so@g' -i /etc/pam.d/sshd
ENV NOTVISIBLE "in users profile"
RUN echo "export VISIBLE=now" >> /etc/profile
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ProcessManager/ProcessManager.csproj ProcessManager/
RUN dotnet restore ProcessManager/ProcessManager.csproj
COPY . .
WORKDIR /src/ProcessManager
RUN dotnet build ProcessManager.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ProcessManager.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ProcessManager.dll"]
