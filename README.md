# System Autentykacji Passkey

Aplikacja demonstracyjna do autentykacji przez passkey (WebAuthn).

## Struktura projektu

```
passkey/
├── src/
│   ├── backend/          # Backend .NET 10
│   │   └── PasskeyAuth.Api/
│   └── frontend/         # Frontend Angular 21
├── infra/                # Infrastruktura Docker
├── docs/                 # Dokumentacja
└── plan/                 # Plan wykonania
```

## Szybki start

### 1. Uruchomienie bazy danych

```powershell
docker-compose -f infra/docker-compose.yml up -d
```

### 2. Backend

```powershell
cd src/backend/PasskeyAuth.Api
dotnet restore
dotnet run
```

Backend będzie dostępny na: `http://localhost:5000`

### 3. Frontend

```powershell
cd src/frontend
npm install
npm start
```

Frontend będzie dostępny na: `http://localhost:4200`

## Funkcjonalności

- ✅ Rejestracja passkey
- ✅ Logowanie passkey
- ✅ Lista passkey użytkownika
- ✅ Usuwanie passkey

## API Endpoints

- `POST /api/v1/auth/passkey/register/start` - Rozpoczęcie rejestracji
- `POST /api/v1/auth/passkey/register/finish` - Zakończenie rejestracji
- `POST /api/v1/auth/passkey/login/start` - Rozpoczęcie logowania
- `POST /api/v1/auth/passkey/login/finish` - Zakończenie logowania
- `GET /api/v1/auth/passkey?userId={id}` - Lista passkey
- `DELETE /api/v1/auth/passkey/{id}?userId={id}` - Usunięcie passkey

## Dokumentacja

- [Przegląd projektu](docs/09-projekty/01-przeglad-projektu.md)
- [Passkey Implementation](docs/09-projekty/02-passkey-implementation.md)
- [Plan wykonania](plan/README.md)


