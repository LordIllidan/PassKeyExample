# Podsumowanie Endpointów REST API

> 📖 [Powrót do planu](../README.md)

---

## Spis treści
1. [Auth API](#auth-api)
2. [Passkey API](#passkey-api)
3. [2FA API](#2fa-api)
4. [User API](#user-api)
5. [OAuth API](#oauth-api)
6. [Admin API](#admin-api)
7. [Audit API](#audit-api)

---

## Auth API

### Autentykacja

| Method | Endpoint | Opis | Auth |
|-------|----------|------|------|
| POST | `/api/v1/auth/register` | Rejestracja użytkownika | - |
| POST | `/api/v1/auth/login` | Logowanie hasłem | - |
| POST | `/api/v1/auth/refresh` | Odświeżenie tokena | - |
| POST | `/api/v1/auth/logout` | Wylogowanie | Bearer |
| GET | `/api/v1/auth/me` | Informacje o zalogowanym użytkowniku | Bearer |

---

## Passkey API

### Passkey Operations

| Method | Endpoint | Opis | Auth |
|-------|----------|------|------|
| POST | `/api/v1/auth/passkey/register/start` | Rozpoczęcie rejestracji passkey | Bearer |
| POST | `/api/v1/auth/passkey/register/finish` | Zakończenie rejestracji passkey | Bearer |
| POST | `/api/v1/auth/passkey/login/start` | Rozpoczęcie logowania passkey | - |
| POST | `/api/v1/auth/passkey/login/finish` | Zakończenie logowania passkey | - |
| GET | `/api/v1/auth/passkey` | Lista passkey użytkownika | Bearer |
| DELETE | `/api/v1/auth/passkey/{id}` | Usunięcie passkey | Bearer |
| PATCH | `/api/v1/auth/passkey/{id}` | Aktualizacja nazwy passkey | Bearer |

---

## 2FA API

### TOTP 2FA

| Method | Endpoint | Opis | Auth |
|-------|----------|------|------|
| POST | `/api/v1/auth/2fa/totp/setup` | Konfiguracja TOTP | Bearer |
| POST | `/api/v1/auth/2fa/totp/verify` | Weryfikacja kodu TOTP | Bearer |
| POST | `/api/v1/auth/2fa/totp/login` | Logowanie z kodem TOTP | - |

### SMS 2FA

| Method | Endpoint | Opis | Auth |
|-------|----------|------|------|
| POST | `/api/v1/auth/2fa/sms/setup` | Konfiguracja SMS 2FA | Bearer |
| POST | `/api/v1/auth/2fa/sms/verify` | Weryfikacja kodu SMS | Bearer |

### Backup Codes

| Method | Endpoint | Opis | Auth |
|-------|----------|------|------|
| POST | `/api/v1/auth/2fa/backup-codes/generate` | Generowanie backup codes | Bearer |
| POST | `/api/v1/auth/2fa/backup-codes/verify` | Weryfikacja backup code | - |

---

## User API

### User Management

| Method | Endpoint | Opis | Auth |
|-------|----------|------|------|
| GET | `/api/v1/users` | Lista użytkowników | Bearer |
| GET | `/api/v1/users/{id}` | Pobranie użytkownika | Bearer |
| PATCH | `/api/v1/users/{id}` | Aktualizacja użytkownika | Bearer |

---

## OAuth API

### OAuth Providers

| Method | Endpoint | Opis | Auth |
|-------|----------|------|------|
| GET | `/api/v1/auth/oauth/{provider}/authorize` | Autoryzacja przez providera | - |
| GET | `/api/v1/auth/oauth/{provider}/callback` | Callback od providera | - |
| GET | `/api/v1/auth/oauth/providers` | Lista dostępnych providerów | - |

---

## Admin API

### User Management (Admin)

| Method | Endpoint | Opis | Auth | Role |
|-------|----------|------|------|------|
| GET | `/api/v1/admin/users` | Lista wszystkich użytkowników | Bearer | Admin |
| POST | `/api/v1/admin/users/{id}/lock` | Zablokowanie użytkownika | Bearer | Admin, SecurityAdmin |
| POST | `/api/v1/admin/users/{id}/unlock` | Odblokowanie użytkownika | Bearer | Admin, SecurityAdmin |

### OAuth Providers (Admin)

| Method | Endpoint | Opis | Auth | Role |
|-------|----------|------|------|------|
| GET | `/api/v1/admin/oauth-providers` | Lista providerów OAuth | Bearer | Admin |
| POST | `/api/v1/admin/oauth-providers` | Dodanie providera OAuth | Bearer | Admin |

### IP Filters (Admin)

| Method | Endpoint | Opis | Auth | Role |
|-------|----------|------|------|------|
| GET | `/api/v1/admin/ip-whitelist` | Lista IP whitelist | Bearer | Admin, SecurityAdmin |
| POST | `/api/v1/admin/ip-whitelist` | Dodanie IP do whitelist | Bearer | Admin, SecurityAdmin |
| GET | `/api/v1/admin/ip-blacklist` | Lista IP blacklist | Bearer | Admin, SecurityAdmin |
| POST | `/api/v1/admin/ip-blacklist` | Dodanie IP do blacklist | Bearer | Admin, SecurityAdmin |

---

## Audit API

### Audit Logs

| Method | Endpoint | Opis | Auth | Role |
|-------|----------|------|------|------|
| GET | `/api/v1/audit/logs` | Lista logów audit | Bearer | Admin, SecurityAdmin, Auditor |
| GET | `/api/v1/audit/alerts` | Lista alertów bezpieczeństwa | Bearer | Admin, SecurityAdmin, Auditor |

---

## Status Codes

### Success
- `200 OK` - Sukces
- `201 Created` - Zasób utworzony
- `204 No Content` - Sukces bez zawartości

### Client Errors
- `400 Bad Request` - Nieprawidłowe żądanie
- `401 Unauthorized` - Brak autoryzacji
- `403 Forbidden` - Brak uprawnień
- `404 Not Found` - Zasób nie znaleziony
- `409 Conflict` - Konflikt (np. duplikat)
- `422 Unprocessable Entity` - Walidacja nie przeszła
- `429 Too Many Requests` - Zbyt wiele żądań

### Server Errors
- `500 Internal Server Error` - Błąd serwera
- `503 Service Unavailable` - Serwis niedostępny

---

## Authentication

Wszystkie endpointy wymagające autoryzacji używają Bearer Token w headerze:

```
Authorization: Bearer {access_token}
```

---

## Rate Limiting

- **Login/Register**: 5 prób na minutę
- **Passkey**: 10 prób na minutę
- **2FA**: 5 prób na minutę
- **Default**: 100 żądań na minutę

---

> 📖 [Powrót do planu](../README.md)



