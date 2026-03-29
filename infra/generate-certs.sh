#!/bin/bash
# Generate SSL certificates for nginx

CERT_DIR="certs"
CERT_PATH="../src/frontend/$CERT_DIR"

# Create certs directory if it doesn't exist
mkdir -p "$CERT_PATH"

# Generate private key
openssl genrsa -out "$CERT_PATH/key.pem" 2048

# Generate certificate
openssl req -new -x509 -key "$CERT_PATH/key.pem" -out "$CERT_PATH/cert.pem" -days 365 -subj "/CN=localhost"

echo "Certificates generated in $CERT_PATH"
