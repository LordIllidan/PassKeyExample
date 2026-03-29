# Prezentacja biznesowa (15 min), demo aplikacji i maszyna firmowa

Dokument łączy: plan slajdów ze skryptem minutowym, ocenę gotowości demo oraz wytyczne przeniesienia repozytorium na laptop stacjonarny w firmie.

---

## 1. Slajdy (tytuły)

1. Tytuł — Passkey / WebAuthn w praktyce (demo: PassKeyExample).
2. Problem — hasła, phishing, koszty wsparcia (krótko).
3. Czym jest passkey — FIDO2 / WebAuthn; klucz prywatny na urządzeniu, publiczny u dostawcy usługi.
4. Korzyści biznesowe — UX, mniej resetów, mniej klasycznych wektorów na hasło, oczekiwania rynku (bez obietnic compliance).
5. Co dostaje security — wiązanie z domeną (rpId), brak „sekretu hasła” w stylu współdzielonego sekretu serwer–użytkownik (model kryptograficzny inny niż hasło); uczciwie: odzysk konta, polityka urządzeń, passkey zsynchronizowane w chmurze platformy.
6. Architektura demo — Angular + API .NET, PostgreSQL, opcjonalnie RabbitMQ; przepływ register/start → finish, login/start → finish.
7. Live demo — pełny stack Docker (zalecane): utworzenie użytkownika (Swagger/curl), rejestracja passkey, logowanie.
8. Podsumowanie — jeden standard, ścieżka do produkcji wymaga twardej weryfikacji kryptograficznej (w demo uproszczenia).
9. Pytania.

---

## 2. Skrypt minutowy (~15 min)

| Czas | Treść |
|------|--------|
| 0:00–0:45 | Powitanie, kim jesteście, że to aplikacja demonstracyjna, nie produkcja. Agenda. |
| 0:45–3:30 | Slajd „problem + czym jest passkey”: hasła jako koszt; passkey jako logowanie kryptograficzne z potwierdzeniem na urządzeniu. |
| 3:30–6:30 | Korzyści biznesowe: szybsze logowanie, mniej ticketów, mniej credential stuffing względem samego hasła; ostrożnie o RODO/ISO — kierunek, nie obietnica. |
| 6:30–9:30 | Dla security: phishing i origin, challenge po stronie serwera, co jest w bazie (w demo uproszczone przechowywanie/weryfikacja — zaznaczyć). Otwarcie na pytania o attestation, recovery, bound vs synced. |
| 9:30–12:30 | Demo: Docker, `https://localhost:4443`, utworzenie użytkownika, rejestracja passkey, logowanie. |
| 12:30–14:00 | Podsumowanie trzech zdań + przygotowane zrzuty ekranu na wypadek sieci lub USB. |
| 14:00–15:00 | Q&A. |

---

## 3. Weryfikacja buildów (stan z weryfikacji w repozytorium)

Wykonane polecenia i wyniki:

- **Backend API:** `dotnet build -c Release` w `src/backend/PasskeyAuth.Api` — **kompilacja powiodła się**.
- **Testy:** `dotnet test` w `src/backend/PasskeyAuth.Api.Tests` — **73 testy zakończone powodzeniem**.
- **Frontend:** `npm ci` oraz `npm run build` w `src/frontend` — **build produkcyjny zakończony powodzeniem** (wyjście: `dist/passkey-auth-frontend`).

**Ostrzeżenia (warto wspomnieć przy audytorium security, bez paniki):**

- `dotnet restore` zgłasza znane podatności w zależnościach pośrednich (m.in. pakiety JWT/CBOR powiązane z łańcuchem `Fido2NetLib`) — to temat na hardening przed produkcją, nie na blokadę prezentacji demo.
- `npm audit` zgłasza liczne ostrzeżenia w grafie zależności — typowe dla ekosystemu npm; dla produkcji warto politykę aktualizacji i skanowanie.

Równoległy build testów i API mógł dać błąd blokady pliku DLL (`VBCSCompiler`) — na maszynie firmowej: zamknąć inne instancje Visual Studio / `dotnet run`, powtórzyć build.

---

## 4. Czy aplikacja jest OK na pokaz?

**Tak, pod warunkiem właściwej ścieżki uruchomienia.**

| Ścieżka | Ocena |
|---------|--------|
| **Docker Compose z `infra/docker-compose.yml`** | **Zalecana na demo.** Frontend z Nginx i certyfikatem, proxy `/api` do backendu, `WebAuthn:Origin` ustawione na `https://localhost:4443` — spójne z WebAuthn (HTTPS + localhost). Adres dla widzów: **`https://localhost:4443`** (port **4443**). |
| **README: `dotnet run` + `npm start` (port 4200)** | **Ryzykowna / prawdopodobnie niepełna.** Frontend woła względne ścieżki `/api/v1` — przy samym `ng serve` bez proxy do API żądania nie trafiają do backendu. CORS w `Program.cs` obejmuje m.in. `http://localhost:4400` i `https://localhost:4443`, **nie** `http://localhost:4200`. Na pokaz **nie polegaj** na samym opisie z README bez Docker albo bez ręcznej konfiguracji proxy i CORS. |

**Scenariusz demo (Docker):**

1. `docker compose -f infra/docker-compose.yml up -d --build`
2. Przeglądarka: `https://localhost:4443` (zaakceptuj certyfikat zaufania dla localhost, jeśli system pyta).
3. Użytkownika utwórz przez Swagger backendu (`http://localhost:5200/swagger`) lub curl do API — zgodnie z `QUICK_START.md`.
4. Rejestracja passkey z UI (User ID), potem logowanie passkey.

**Uwaga merytoryczna dla security:** W `PasskeyController` rejestracja/logowanie używa uproszczonej walidacji (komentarze w kodzie wskazują, że pełna weryfikacja attestation/assertion pod produkcję jest do dopracowania). Na prezentacji biznesowej można powiedzieć: „to ilustracja przepływu; produkcja wymaga pełnej weryfikacji FIDO2”.

---

## 5. Wytyczne: przeniesienie na maszynę firmową

### 5.1 Repozytorium

- Sklonuj to samo repo (Git SSH/HTTPS zgodnie z polityką firmy) lub skopiuj katalog projektu (bez `node_modules` i `bin`/`obj` — odtworzysz lokalnie).
- Upewnij się, że `.gitignore` nie blokuje potrzebnych plików źródłowych (standardowo nie).

### 5.2 Oprogramowanie

- **Docker Desktop** (lub inny runtime zgodny z Compose v2) — jeśli wybierasz ścieżkę demo przez Compose.
- **.NET 10 SDK** — jeśli chcesz budować backend poza kontenerem.
- **Node.js 18+** — jeśli budujesz frontend poza kontenerem (`npm ci`, `npm run build`).

Wersje sprawdź zgodnie z `README.md` i `QUICK_START.md`.

### 5.3 Sieć i bezpieczeństwo firmowe

- Zezwól na **pull obrazów** z Docker Hub (`postgres`, `rabbitmq`, build context lokalny dla frontend/backend).
- Porty używane przez Compose (domyślnie w pliku): **4400**, **4443**, **5200**, **5632** (Postgres na hoście), **5672**, **15672** (RabbitMQ). Jeśli coś jest zajęte, zmień mapowanie w `infra/docker-compose.yml` i **dostosuj** connection string / adresy w dokumentacji wewnętrznej.
- **Proxy HTTP/HTTPS:** jeśli firma wymusza proxy, skonfiguruj je dla Dockera i ewentualnie `npm` (`npm config set proxy` / `https-proxy`) oraz NuGet — inaczej `npm ci` i `dotnet restore` mogą się nie udać.
- **TLS localhost:** przeglądarka może ostrzegać przed certyfikatem z Dockerfile frontendu — dla demo zaakceptuj wyjątek lub zaimportuj certyfikat zgodnie z procedurą IT.

### 5.4 Po przeniesieniu — szybki smoke test

```powershell
cd <ścieżka-do-repo>
docker compose -f infra/docker-compose.yml up -d --build
```

Następnie: health frontendu, Swagger backendu, jedna pełna ścieżka passkey.

Opcjonalnie poza Dockerem:

```powershell
cd src\backend\PasskeyAuth.Api; dotnet build -c Release
cd ..\..\frontend; npm ci; npm run build
```

### 5.5 Zasady na sali prezentacji

- Jedna sieć, jedna maszyna — unikasz problemów z `localhost` i certyfikatami.
- Zapas: nagranie ekranu 60–90 s z działającym passkey lub zrzuty Swagger + UI.
- Nie używaj na pokazie haseł ani kont produkcyjnych — wyłącznie konta testowe.

---

## 6. Powiązane dokumenty

- [Przegląd projektu](01-przeglad-projektu.md)
- [Passkey Implementation](02-passkey-implementation.md)
- [Quick Start](../../QUICK_START.md) (uwaga: ścieżka developerska vs Docker — patrz sekcja 4 powyżej)

