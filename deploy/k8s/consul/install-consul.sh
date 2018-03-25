#!/bin/bash

helm install --name glidergun-consul stable/consul --set ui.enabled=true,uiService.enabled=true,Storage=500Mi,StorageClass=rook-block,DisableHostNodeId=true
