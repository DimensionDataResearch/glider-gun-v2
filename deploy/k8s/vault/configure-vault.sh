#!/bin/bash

# Enable CA
vault mount -path=/glidergun/pki pki
vault mount-tune -max-lease-ttl=87600h /glidergun/pki

# Generate CA certificate
vault write /glidergun/pki/root/generate/internal common_name=vault.glidergun.tintoy.io ttl=87600h
