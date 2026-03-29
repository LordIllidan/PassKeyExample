# Generate SSL certificates for nginx
$certDir = "certs"
$certPath = Join-Path $PSScriptRoot "..\src\frontend\$certDir"

# Create certs directory if it doesn't exist
if (-not (Test-Path $certPath)) {
    New-Item -ItemType Directory -Path $certPath -Force | Out-Null
}

# Generate private key
openssl genrsa -out "$certPath\key.pem" 2048 2>&1 | Out-Null

# Generate certificate (skip config file check)
$env:OPENSSL_CONF = ""
openssl req -new -x509 -key "$certPath\key.pem" -out "$certPath\cert.pem" -days 365 -subj "/CN=localhost" -config NUL 2>&1 | Out-Null

if (Test-Path "$certPath\key.pem" -and Test-Path "$certPath\cert.pem") {
    Write-Host "Certificates generated successfully in $certPath" -ForegroundColor Green
} else {
    Write-Host "Error generating certificates. Please ensure OpenSSL is installed." -ForegroundColor Red
    Write-Host "Alternative: Use 'docker-compose up' and certificates will be generated in container" -ForegroundColor Yellow
}
