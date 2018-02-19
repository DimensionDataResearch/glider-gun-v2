#!/bin/bash

set -euo pipefail

SSH_KEY_FILE=/secrets/id_rsa
SSH_SECURE_KEY_FILE=/secrets/ssh_private_key
TF_STATE_FILE=/state/terraform.tfstate
RKE_MANIFEST_FILE=/state/cluster.yml

# Irritatingly, we can't change permissions on a bind-mount
cp $SSH_KEY_FILE $SSH_SECURE_KEY_FILE
chmod 0600 $SSH_SECURE_KEY_FILE

echo "Configuring SSH to use key '$SSH_SECURE_KEY_FILE'..."

eval $(ssh-agent -s)
ssh-add $SSH_SECURE_KEY_FILE

echo 'Creating hosts and installing Docker...'

pushd /deploy

terraform apply -auto-approve -state=$TF_STATE_FILE
terraform refresh -state=$TF_STATE_FILE
generate-rke-config.py --terraform-state-file $TF_STATE_FILE --rke-manifest-file $RKE_MANIFEST_FILE

popd # /deploy

echo 'Creating Kubernetes cluster...'

pushd /state

rke up

popd # /state

echo 'Done.'
