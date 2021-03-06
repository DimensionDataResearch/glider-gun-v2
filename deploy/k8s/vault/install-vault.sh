#!/bin/bash

# You must already have installed Consul (see ../consul/install-consul.sh)

helm install incubator/vault --name=glidergun-vault --set vault.dev=false --set vault.config.storage.consul.address="glidergun-consul-consul.default.svc.cluster.local:8500" --set vault.config.storage.consul.path="vault"
