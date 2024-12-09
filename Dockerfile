FROM alpine:latest

RUN apk update
# I'm missing some packages
RUN apk add dotnet8-runtime dotnet8-sdk aws-cli vim aws-cli-doc mandoc bash curl

COPY app/ /app/
RUN find /app/

WORKDIR /app

RUN dotnet add package Pulumi
RUN dotnet add package Pulumi.Aws
# RUN dotnet add package Pulumi.Serialization

RUN curl -fsSL https://get.pulumi.com | sh
ENV PATH=$PATH:/root/.pulumi/bin

ARG BUILD_PULUMI_ACCESS_TOKEN
ENV PULUMI_ACCESS_TOKEN=${BUILD_PULUMI_ACCESS_TOKEN}
RUN pulumi stack select dev

RUN pulumi plugin install resource aws-apigateway 2.6.1
RUN pulumi plugin install resource aws 6.61.0
RUN pulumi plugin install resource aws 6.64.0

RUN echo "alias ll='ls -l'" >> /root/.bashrc

CMD pulumi stack select dev && bash