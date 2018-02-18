REPO_ROOT=tintoyddr.azurecr.io/glider-gun
REPO_BASE=${REPO_ROOT}/remote

RKE_VERSION=v0.1.1
TERRAFORM_VERSION=0.11.3

IMAGE_TAG_REMOTE_NODE=${REPO_BASE}/node:latest

## Build

build: build.ubuntu

build.ubuntu: build.ubuntu-xenial

build.ubuntu-xenial:
	docker build -t ${IMAGE_TAG_REMOTE_NODE} --build-arg RKE_VERSION=${RKE_VERSION} --build-arg TERRAFORM_VERSION=${TERRAFORM_VERSION} -f remote-node.Dockerfile .

## Push

push: push.ubuntu

push.ubuntu: push.ubuntu-xenial

push.ubuntu-xenial:
	docker push ${IMAGE_TAG_REMOTE_NODE}
