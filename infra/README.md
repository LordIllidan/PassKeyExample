# Docker Setup - Passkey Auth

## Porty (+200 względem standardowych)

- **Frontend (nginx)**: 
  - HTTP: `4400` (zamiast 4200)
  - HTTPS: `4443` (zamiast 443)
- **Backend API (.NET)**: 
  - HTTP: `5200` (zamiast 5000)
  - HTTPS: `5201` (zamiast 5001)
- **PostgreSQL**: `5632` (zamiast 5432)

## Uruchomienie

### 1. Generowanie certyfikatów SSL (opcjonalne)

Certyfikaty są automatycznie generowane w kontenerze nginx podczas budowania. Jeśli chcesz wygenerować je lokalnie:

**Windows (PowerShell):**
```powershell
.\generate-certs.ps1
```

**Linux/Mac:**
```bash
chmod +x generate-certs.sh
./generate-certs.sh
```

### 2. Uruchomienie wszystkich serwisów

```bash
docker-compose up -d
```

### 3. Sprawdzenie statusu

```bash
docker-compose ps
```

### 4. Logi

```bash
# Wszystkie serwisy
docker-compose logs -f

# Konkretny serwis
docker-compose logs -f frontend
docker-compose logs -f backend
docker-compose logs -f postgres
```

### 5. Zatrzymanie

```bash
docker-compose down
```

### 6. Zatrzymanie z usunięciem wolumenów

```bash
docker-compose down -v
```

## Dostęp do aplikacji

- **Frontend (HTTPS)**: https://localhost:4443
- **Frontend (HTTP - przekierowanie)**: http://localhost:4400
- **Backend API**: http://localhost:5200
- **Swagger UI**: http://localhost:5200/swagger
- **PostgreSQL**: localhost:5632

## Konfiguracja

### Zmienne środowiskowe

Backend używa zmiennych środowiskowych zdefiniowanych w `docker-compose.yml`:

- `ASPNETCORE_ENVIRONMENT`: Development
- `ASPNETCORE_URLS`: http://+:5200;https://+:5201
- `ConnectionStrings__DefaultConnection`: Host=postgres;Port=5432;Database=passkey_db;Username=passkey_user;Password=passkey_password
- `WebAuthn__RpId`: localhost
- `WebAuthn__Origin`: https://localhost:4443

### Baza danych

PostgreSQL jest dostępna na porcie `5632` z następującymi danymi:

- **Database**: passkey_db
- **User**: passkey_user
- **Password**: passkey_password

## Troubleshooting

### Problem z certyfikatami SSL

Jeśli masz problemy z certyfikatami, przeglądarka może wyświetlać ostrzeżenie o niebezpiecznym połączeniu. To normalne dla self-signed certificates. Zaakceptuj certyfikat w przeglądarce.

### Problem z połączeniem do bazy danych

Upewnij się, że kontener PostgreSQL jest uruchomiony:

```bash
docker-compose ps postgres
```

Sprawdź logi:

```bash
docker-compose logs postgres
```

### Problem z CORS

Backend jest skonfigurowany do akceptowania żądań z:
- http://localhost:4400
- https://localhost:4443
- http://frontend
- https://frontend

Jeśli masz problemy z CORS, sprawdź konfigurację w `Program.cs`.

## Rebuild kontenerów

Jeśli zmieniłeś kod i chcesz przebudować kontenery:

```bash
docker-compose up -d --build
```

## Czyszczenie

Usunięcie wszystkich kontenerów, obrazów i wolumenów:

```bash
docker-compose down -v --rmi all
```
