#!/bin/bash

set -euo pipefail

SSH_KEY_FILE=/secrets/id_rsa
SSH_SECURE_KEY_FILE=/secrets/ssh_private_key
TF_STATE_FILE=/state/terraform.tfstate
RKE_MANIFEST_FILE=/state/cluster.yml

COMMAND=${1:-}

alias gtfo=/dev/null

deploy() {
    echo 'Creating hosts and installing Docker...'

    pushd /deploy > gtfo

    terraform apply -auto-approve -state=$TF_STATE_FILE
    terraform refresh -state=$TF_STATE_FILE
    generate-rke-config.py --terraform-state-file $TF_STATE_FILE --ssh-key-file=$SSH_SECURE_KEY_FILE --cluster-manifest-file=$RKE_MANIFEST_FILE

    popd > gtfo # /deploy

    echo 'Creating Kubernetes cluster...'

    pushd /state > gtfo

    rke up

    popd > gtfo # /state
}

destroy() {
    echo 'Destroying hosts...'

    pushd /deploy > gtfo

    terraform destroy -force -state=$TF_STATE_FILE

    popd > gtfo # /deploy
}

# Irritatingly, we can't change permissions on a bind-mount
cp $SSH_KEY_FILE $SSH_SECURE_KEY_FILE
chmod 0600 $SSH_SECURE_KEY_FILE

echo "Configuring SSH to use key '$SSH_SECURE_KEY_FILE'..."

eval $(ssh-agent -s)
ssh-add $SSH_SECURE_KEY_FILE

case $COMMAND in
    deploy)
    
    deploy
    exit 0
    
    ;;

    destroy)
    
    destroy
    exit 0

    ;;

    *)

    echo "Invalid command '$COMMAND' (must be 'deploy' or 'destroy')."
    exit 1

    ;;
esac

echo 'Done.'
