# Architektura Bazy Danych

> 📖 [Powrót do planu](../README.md)

---

## Spis treści
1. [Przegląd](#przegląd)
2. [Schematy bazy danych](#schematy-bazy-danych)
3. [Tabele](#tabele)
4. [Relacje](#relacje)
5. [Indeksy](#indeksy)

---

## Przegląd

Baza danych PostgreSQL z podziałem na schematy zgodnie z zasadami Domain-Driven Design i separowalności mikroserwisów.

---

## Schematy bazy danych

### auth - Schemat autentykacji

Zawiera tabele związane z autentykacją:
- `sessions` - sesje użytkowników
- `refresh_tokens` - refresh tokens
- `passkey_credentials` - credentiale passkey
- `totp_credentials` - credentiale TOTP
- `sms_credentials` - credentiale SMS 2FA
- `backup_codes` - backup codes dla 2FA

### users - Schemat użytkowników

Zawiera tabele związane z użytkownikami:
- `users` - użytkownicy
- `user_roles` - role użytkowników (many-to-many)
- `oauth_credentials` - credentiale OAuth użytkowników

### security - Schemat bezpieczeństwa

Zawiera tabele związane z bezpieczeństwem:
- `audit_logs` - logi audit
- `security_alerts` - alerty bezpieczeństwa
- `ip_filters` - filtry IP (whitelist/blacklist)

### config - Schemat konfiguracji

Zawiera tabele związane z konfiguracją:
- `oauth_providers` - konfiguracja providerów OAuth
- `ldap_configurations` - konfiguracja LDAP
- `external_api_configurations` - konfiguracja zewnętrznych API

---

## Tabele

### auth.sessions

```sql
CREATE TABLE auth.sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users.users(id) ON DELETE CASCADE,
    refresh_token_id UUID NOT NULL,
    access_token TEXT NOT NULL,
    refresh_token_hash TEXT NOT NULL,
    ip_address INET,
    user_agent TEXT,
    device_fingerprint TEXT,
    expires_at TIMESTAMP NOT NULL,
    refresh_token_expires_at TIMESTAMP NOT NULL,
    is_revoked BOOLEAN NOT NULL DEFAULT FALSE,
    revoked_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_sessions_user_id ON auth.sessions(user_id);
CREATE INDEX idx_sessions_refresh_token_id ON auth.sessions(refresh_token_id);
CREATE INDEX idx_sessions_expires_at ON auth.sessions(expires_at);
```

### auth.passkey_credentials

```sql
CREATE TABLE auth.passkey_credentials (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users.users(id) ON DELETE CASCADE,
    credential_id TEXT NOT NULL UNIQUE,
    public_key TEXT NOT NULL,
    counter INTEGER NOT NULL DEFAULT 0,
    name VARCHAR(255) NOT NULL,
    device_type VARCHAR(50) NOT NULL,
    user_agent TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    last_used_at TIMESTAMP
);

CREATE INDEX idx_passkey_credentials_user_id ON auth.passkey_credentials(user_id);
CREATE INDEX idx_passkey_credentials_credential_id ON auth.passkey_credentials(credential_id);
```

### auth.totp_credentials

```sql
CREATE TABLE auth.totp_credentials (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users.users(id) ON DELETE CASCADE,
    secret_key TEXT NOT NULL,
    is_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    verified_at TIMESTAMP,
    is_recovery_mode BOOLEAN NOT NULL DEFAULT FALSE,
    recovery_started_at TIMESTAMP,
    UNIQUE(user_id)
);

CREATE INDEX idx_totp_credentials_user_id ON auth.totp_credentials(user_id);
```

### auth.sms_credentials

```sql
CREATE TABLE auth.sms_credentials (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users.users(id) ON DELETE CASCADE,
    phone_number TEXT NOT NULL,
    is_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    is_verified BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    verified_at TIMESTAMP,
    UNIQUE(user_id)
);

CREATE INDEX idx_sms_credentials_user_id ON auth.sms_credentials(user_id);
```

### auth.backup_codes

```sql
CREATE TABLE auth.backup_codes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users.users(id) ON DELETE CASCADE,
    code_hash TEXT NOT NULL,
    is_used BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    used_at TIMESTAMP
);

CREATE INDEX idx_backup_codes_user_id ON auth.backup_codes(user_id);
CREATE INDEX idx_backup_codes_code_hash ON auth.backup_codes(code_hash);
```

### users.users

```sql
CREATE TABLE users.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) NOT NULL UNIQUE,
    user_name VARCHAR(100),
    name VARCHAR(255),
    password_hash TEXT,
    is_email_verified BOOLEAN NOT NULL DEFAULT FALSE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    is_locked BOOLEAN NOT NULL DEFAULT FALSE,
    locked_until TIMESTAMP,
    failed_login_attempts INTEGER NOT NULL DEFAULT 0,
    last_login_at TIMESTAMP,
    source VARCHAR(50) NOT NULL DEFAULT 'local',
    external_id TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP
);

CREATE INDEX idx_users_email ON users.users(email);
CREATE INDEX idx_users_user_name ON users.users(user_name);
CREATE INDEX idx_users_external_id ON users.users(external_id);
CREATE INDEX idx_users_is_active ON users.users(is_active);
```

### users.user_roles

```sql
CREATE TABLE users.user_roles (
    user_id UUID NOT NULL REFERENCES users.users(id) ON DELETE CASCADE,
    role_name VARCHAR(50) NOT NULL,
    PRIMARY KEY (user_id, role_name)
);

CREATE INDEX idx_user_roles_user_id ON users.user_roles(user_id);
CREATE INDEX idx_user_roles_role_name ON users.user_roles(role_name);
```

### users.oauth_credentials

```sql
CREATE TABLE users.oauth_credentials (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users.users(id) ON DELETE CASCADE,
    provider_id UUID NOT NULL REFERENCES config.oauth_providers(id) ON DELETE CASCADE,
    external_user_id TEXT NOT NULL,
    email VARCHAR(255),
    access_token TEXT,
    refresh_token TEXT,
    token_expires_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    last_used_at TIMESTAMP,
    UNIQUE(provider_id, external_user_id)
);

CREATE INDEX idx_oauth_credentials_user_id ON users.oauth_credentials(user_id);
CREATE INDEX idx_oauth_credentials_provider_id ON users.oauth_credentials(provider_id);
```

### security.audit_logs

```sql
CREATE TABLE security.audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    event_type VARCHAR(50) NOT NULL,
    event_name VARCHAR(100) NOT NULL,
    severity VARCHAR(20) NOT NULL,
    user_id UUID REFERENCES users.users(id) ON DELETE SET NULL,
    user_email VARCHAR(255),
    user_name VARCHAR(255),
    ip_address INET,
    user_agent TEXT,
    request_id UUID,
    session_id TEXT,
    message TEXT,
    metadata JSONB,
    success BOOLEAN NOT NULL DEFAULT TRUE,
    error_message TEXT,
    timestamp TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_audit_logs_event_type ON security.audit_logs(event_type);
CREATE INDEX idx_audit_logs_event_name ON security.audit_logs(event_name);
CREATE INDEX idx_audit_logs_user_id ON security.audit_logs(user_id);
CREATE INDEX idx_audit_logs_timestamp ON security.audit_logs(timestamp);
CREATE INDEX idx_audit_logs_severity ON security.audit_logs(severity);
CREATE INDEX idx_audit_logs_timestamp_user_id ON security.audit_logs(timestamp, user_id);
```

### security.security_alerts

```sql
CREATE TABLE security.security_alerts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    alert_type VARCHAR(50) NOT NULL,
    title VARCHAR(255) NOT NULL,
    message TEXT NOT NULL,
    severity VARCHAR(20) NOT NULL,
    user_id UUID REFERENCES users.users(id) ON DELETE SET NULL,
    ip_address INET,
    is_resolved BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    resolved_at TIMESTAMP
);

CREATE INDEX idx_security_alerts_alert_type ON security.security_alerts(alert_type);
CREATE INDEX idx_security_alerts_severity ON security.security_alerts(severity);
CREATE INDEX idx_security_alerts_user_id ON security.security_alerts(user_id);
CREATE INDEX idx_security_alerts_is_resolved ON security.security_alerts(is_resolved);
CREATE INDEX idx_security_alerts_created_at ON security.security_alerts(created_at);
```

### security.ip_filters

```sql
CREATE TABLE security.ip_filters (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ip_address INET NOT NULL,
    type VARCHAR(20) NOT NULL,
    reason TEXT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMP
);

CREATE INDEX idx_ip_filters_ip_address ON security.ip_filters(ip_address);
CREATE INDEX idx_ip_filters_type ON security.ip_filters(type);
CREATE INDEX idx_ip_filters_is_active ON security.ip_filters(is_active);
```

### config.oauth_providers

```sql
CREATE TABLE config.oauth_providers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(50) NOT NULL UNIQUE,
    display_name VARCHAR(100) NOT NULL,
    type VARCHAR(20) NOT NULL,
    authorization_endpoint TEXT NOT NULL,
    token_endpoint TEXT NOT NULL,
    user_info_endpoint TEXT,
    client_id TEXT NOT NULL,
    client_secret TEXT NOT NULL,
    scopes TEXT[],
    redirect_uri TEXT NOT NULL,
    is_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    "order" INTEGER NOT NULL DEFAULT 0,
    icon_url TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP
);

CREATE INDEX idx_oauth_providers_name ON config.oauth_providers(name);
CREATE INDEX idx_oauth_providers_is_enabled ON config.oauth_providers(is_enabled);
```

---

## Relacje

```
users.users
    ├── auth.sessions (user_id)
    ├── auth.passkey_credentials (user_id)
    ├── auth.totp_credentials (user_id)
    ├── auth.sms_credentials (user_id)
    ├── auth.backup_codes (user_id)
    ├── users.oauth_credentials (user_id)
    ├── users.user_roles (user_id)
    ├── security.audit_logs (user_id)
    └── security.security_alerts (user_id)

config.oauth_providers
    └── users.oauth_credentials (provider_id)
```

---

## Indeksy

### Indeksy dla wydajności

- **users.users**: email (UNIQUE), user_name, external_id, is_active
- **auth.sessions**: user_id, refresh_token_id, expires_at
- **auth.passkey_credentials**: user_id, credential_id (UNIQUE)
- **security.audit_logs**: timestamp, user_id, event_type, severity
- **security.security_alerts**: created_at, is_resolved, severity

### Indeksy złożone

- **security.audit_logs**: (timestamp, user_id) - dla zapytań po czasie i użytkowniku
- **security.audit_logs**: (event_type, timestamp) - dla zapytań po typie eventu

---

> 📖 [Powrót do planu](../README.md)


