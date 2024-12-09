name=pulumi-cs-dev
tag=latest
credentials_file=credentials
config_file=config
project_name=app
count:=$(shell sudo docker ps | grep $(name) | wc -l)
pulumi_access_token=$(shell cat token.txt)

.PHONY: all
all: build run

build:
	sudo docker build \
		--build-arg BUILD_PULUMI_ACCESS_TOKEN=$(pulumi_access_token) \
		-t $(name):$(tag) .
	touch build

.PHONY: run
run:
	sudo docker run --rm -ti \
		--name $(name)-$(count) \
		-v $(PWD)/$(project_name):/$(project_name) \
		-v $(PWD)/$(credentials_file):/root/.aws/credentials \
		-v $(PWD)/$(config_file):/root/.aws/config \
		$(name):$(tag)
