# Plan Wykonania - System Autentykacji Passkey

> 📖 [Powrót do dokumentacji projektu](../docs/09-projekty/01-przeglad-projektu.md)

---

## Spis treści
1. [Przegląd](#przegląd)
2. [Struktura planu](#struktura-planu)
3. [Modele danych](#modele-danych)
4. [REST API](#rest-api)
5. [Role i uprawnienia](#role-i-uprawnienia)
6. [Architektura bazy danych](#architektura-bazy-danych)

---

## Przegląd

Ten katalog zawiera szczegółowy plan wykonania projektu System Autentykacji Passkey, w tym:

- **Modele danych** - JSON schemas dla wszystkich encji
- **REST API** - definicje wszystkich endpointów
- **Role i uprawnienia** - system ról i kontroli dostępu
- **Architektura bazy danych** - schemat bazy danych

---

## Struktura planu

```
plan/
├── README.md                 # Ten plik
├── models/                   # Modele danych (JSON schemas)
│   ├── user.json
│   ├── passkey-credential.json
│   ├── totp-credential.json
│   ├── session.json
│   ├── audit-log.json
│   └── ...
├── api/                      # Definicje REST API
│   ├── auth-api.json
│   ├── user-api.json
│   ├── passkey-api.json
│   ├── 2fa-api.json
│   └── ...
└── roles/                    # Role i uprawnienia
    ├── roles.json
    ├── permissions.json
    └── role-permissions.json
```

---

## Modele danych

Modele danych znajdują się w katalogu `models/` i są zdefiniowane jako JSON schemas zgodnie z JSON Schema Draft 7.

### Główne encje:

- **User** - użytkownik systemu
- **PasskeyCredential** - credential passkey (WebAuthn)
- **TotpCredential** - credential TOTP 2FA
- **SmsCredential** - credential SMS 2FA
- **BackupCode** - backup codes dla 2FA
- **Session** - sesja użytkownika
- **RefreshToken** - refresh token
- **OAuthProvider** - konfiguracja providera OAuth
- **OAuthCredential** - credential OAuth użytkownika
- **AuditLog** - log audit
- **SecurityAlert** - alert bezpieczeństwa
- **IpFilter** - filtr IP (whitelist/blacklist)

---

## REST API

Definicje REST API znajdują się w katalogu `api/` i są zdefiniowane zgodnie z wytycznymi API Design.

### Główne API:

- **Auth API** - autentykacja (login, register, refresh token)
- **Passkey API** - operacje passkey
- **2FA API** - operacje 2FA (TOTP, SMS, Email)
- **User API** - zarządzanie użytkownikami
- **OAuth API** - integracje OAuth
- **Admin API** - operacje administracyjne
- **Audit API** - przeglądanie logów audit

---

## Role i uprawnienia

System ról i uprawnień znajduje się w katalogu `roles/`.

### Role:

- **User** - zwykły użytkownik
- **Admin** - administrator systemu
- **SecurityAdmin** - administrator bezpieczeństwa
- **Auditor** - audytor (tylko odczyt logów)

---

## Architektura bazy danych

Baza danych PostgreSQL z następującymi schematami:

- **auth** - schemat autentykacji (sessions, tokens, passkeys)
- **users** - schemat użytkowników
- **security** - schemat bezpieczeństwa (audit logs, alerts, ip filters)
- **config** - schemat konfiguracji (oauth providers, ldap configs)

**Dokumentacja:** [Database Schema](database-schema.md)

---

## Pliki w planie

### Modele danych (`models/`)
- `user.json` - model użytkownika
- `passkey-credential.json` - model credential passkey
- `totp-credential.json` - model credential TOTP
- `sms-credential.json` - model credential SMS 2FA
- `backup-code.json` - model backup code
- `session.json` - model sesji
- `audit-log.json` - model logu audit
- `oauth-provider.json` - model providera OAuth
- `oauth-credential.json` - model credential OAuth
- `security-alert.json` - model alertu bezpieczeństwa
- `ip-filter.json` - model filtra IP

### REST API (`api/`)
- `auth-api.json` - API autentykacji (login, register, refresh, logout)
- `passkey-api.json` - API passkey (register, login, manage)
- `2fa-api.json` - API 2FA (TOTP, SMS, Email, Backup Codes)
- `user-api.json` - API użytkowników
- `oauth-api.json` - API OAuth providers
- `admin-api.json` - API administracyjne
- `audit-api.json` - API audit logs

**Podsumowanie:** [API Endpoints Summary](api-endpoints-summary.md)

### Role i uprawnienia (`roles/`)
- `roles.json` - definicja ról
- `permissions.json` - definicja uprawnień
- `role-permissions.json` - mapowanie ról do uprawnień

---

> 📖 [Powrót do dokumentacji projektu](../docs/09-projekty/01-przeglad-projektu.md)

