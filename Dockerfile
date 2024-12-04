FROM alpine:latest

RUN apk update
# I'm missing some packages
RUN apk add dotnet8-runtime dotnet8-sdk aws-cli vim aws-cli-doc mandoc bash curl

RUN dotnet add package Pulumi Pulumi.Aws # Pulumi.Serialization

RUN curl -fsSL https://get.pulumi.com | sh
ENV PATH=$PATH:/root/.pulumi/bin

RUN echo "alias ll='ls -l'" >> /root/.bashrc

RUN mkdir /app
WORKDIR /app
ENTRYPOINT bash
