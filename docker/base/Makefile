REPO_ROOT=tintoy-ddr.azurecr.io/glider-gun
REPO_BASE=${REPO_ROOT}/base

VAULT2ENV_VERSION=v0.0.2

## Build

build: build.ubuntu

build.ubuntu: build.ubuntu-xenial

build.ubuntu-xenial:
	docker build -t ${REPO_BASE}/ubuntu:xenial --build-arg VAULT2ENV_VERSION=${VAULT2ENV_VERSION} -f ubuntu-xenial.Dockerfile .

## Push

push: push.ubuntu

push.ubuntu: push.ubuntu-xenial

push.ubuntu-xenial:
	docker push ${REPO_BASE}/ubuntu:xenial
