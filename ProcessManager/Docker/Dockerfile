FROM microsoft/aspnetcore:2.0

EXPOSE 80 22

RUN apt-get update && apt-get install -y openssh-server supervisor wget iputils-ping net-tools nano ffmpeg
RUN mkdir /var/run/sshd && mkdir -p logs
RUN useradd vuta && echo 'vuta:Echo@1927' | chpasswd
RUN echo adduser vuta sudo
RUN echo 'root:Echo@1927' | chpasswd
RUN sed -i 's/PermitRootLogin prohibit-password/PermitRootLogin yes/' /etc/ssh/sshd_config

# SSH login fix. Otherwise user is kicked off after login
RUN sed 's@session\s*required\s*pam_loginuid.so@session optional pam_loginuid.so@g' -i /etc/pam.d/sshd

ENV NOTVISIBLE "in users profile"
RUN echo "export VISIBLE=now" >> /etc/profile

COPY supervisord.conf /etc/supervisor/conf.d/supervisord.conf

WORKDIR /app
COPY publish/. .
ENTRYPOINT ["supervisord", "-c","/etc/supervisor/supervisord.conf"]
