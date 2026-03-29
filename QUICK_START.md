# Quick Start - Passkey Authentication

## Wymagania

- .NET 10 SDK
- Node.js 18+
- Docker Desktop

## Krok 1: Uruchomienie bazy danych

```powershell
docker-compose -f infra/docker-compose.yml up -d
```

Sprawdź czy baza działa:
```powershell
docker ps
```

## Krok 2: Backend

```powershell
cd src/backend/PasskeyAuth.Api
dotnet restore
dotnet run
```

Backend będzie dostępny na: `http://localhost:5000`

Swagger UI: `http://localhost:5000/swagger`

## Krok 3: Frontend

W nowym terminalu:

```powershell
cd src/frontend
npm install
npm start
```

Frontend będzie dostępny na: `http://localhost:4200`

## Krok 4: Testowanie

### 1. Utwórz użytkownika

Przez Swagger UI lub curl:

```powershell
curl -X POST http://localhost:5000/api/v1/users `
  -H "Content-Type: application/json" `
  -d '{\"email\":\"test@example.com\",\"name\":\"Test User\"}'
```

Zapisz zwrócony `id` użytkownika.

### 2. Zarejestruj passkey

1. Otwórz `http://localhost:4200`
2. Wprowadź User ID z kroku 1
3. Kliknij "Register Passkey"
4. Użyj biometrii/PIN na urządzeniu

### 3. Zaloguj się passkey

1. Wprowadź email (opcjonalnie)
2. Kliknij "Login with Passkey"
3. Użyj biometrii/PIN na urządzeniu

## Rozwiązywanie problemów

### Baza danych nie startuje

```powershell
docker-compose -f infra/docker-compose.yml down
docker-compose -f infra/docker-compose.yml up -d
```

### Backend nie łączy się z bazą

Sprawdź connection string w `appsettings.json` i czy PostgreSQL działa:
```powershell
docker ps | findstr postgres
```

### WebAuthn nie działa

- Upewnij się, że używasz HTTPS lub localhost
- Sprawdź czy przeglądarka wspiera WebAuthn (Chrome, Edge, Firefox)
- Sprawdź konfigurację `WebAuthn:RpId` w `appsettings.json`


