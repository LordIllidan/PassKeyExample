# Prezentacja biznesowa (15 min), demo aplikacji i maszyna firmowa

Dokument łączy: plan slajdów ze skryptem minutowym, ocenę gotowości demo oraz wytyczne przeniesienia repozytorium na laptop stacjonarny w firmie.

---

## Passkey i WebAuthn — rozszerzony opis

**Passkey** to w praktyce credential FIDO2 / **WebAuthn**: para kluczy kryptograficznych powiązana z kontem i z **domeną usługi** (relying party, `rpId`). Użytkownik potwierdza operację na **authenticatorze** — zwykle biometria lub PIN urządzenia, czasem osobny klucz sprzętowy (NFC/USB).

- **Klucz prywatny** pozostaje na urządzeniu (lub w bezpiecznym magazynie platformy, np. menedżer haseł systemu / przeglądarki) i **nie jest przesyłany** do serwera.
- **Klucz publiczny** (oraz identyfikator credentialu) trafia do serwera — logowanie to **odpowiedź kryptograficzna** na jednorazowe **challenge** wystawione przez backend, a nie przesłanie współdzielonego sekretu typu hasło.
- **Discoverable credentials (resident keys):** credential może być „pamiętany” przez urządzenie, co umożliwia logowanie **bez wpisywania identyfikatora** (w zależności od konfiguracji i UX przeglądarki).
- **Zsynchronizowane passkey (platform sync):** ten sam credential może być dostępny na wielu urządzeniach użytkownika w ekosystemie (np. Apple iCloud Keychain, Google Password Manager). **Passkey „przywiązany” do urządzenia** nie synchronizuje się w ten sposób — inny profil ryzyka i odzyskiwania.
- **Phishing:** przeglądarka i protokół wiążą operację z **konkretnym originem** i `rpId`; fałszywa strona na innej domenie nie przekaże poprawnego asercji dla Waszej aplikacji (w przeciwieństwie do wielu scenariuszy z kradzieżą hasła lub OTP).

Na prezentacji dla biznesu: passkey to **standard branżowy** (W3C WebAuthn, FIDO Alliance), wspierany przez duże platformy i przeglądarki — nie własny, zamknięty mechanizm jednej firmy.

---

## Korzyści biznesowe — szczegółowy opis

| Obszar | Korzyść | Uwaga do powiedzenia |
|--------|---------|----------------------|
| **Doświadczenie klienta i pracownika** | Krótsza ścieżka logowania, mniej pól do wypełniania, mniej „zapomniałem hasła”. | Szczególnie widoczne na mobile i w aplikacjach często używanych. |
| **Koszty operacyjne (IT / helpdesk)** | Mniej ticketów z resetami hasła i blokadami kont; mniej czasu na procedury odzyskiwania. | Efekt zależy od skali i od tego, czy passkey **zastępuje** hasło, czy jest **dodatkiem** — warto to rozróżnić w planie wdrożenia. |
| **Ryzyko incydentów** | Osłabienie klasycznych ataków na hasło: stuffing, wycieki haseł z innych serwisów (reuse), część scenariuszy phishingowych względem samego hasła. | Passkey nie usuwa potrzeby ochrony sesji, tokenów API, urządzeń końcowych ani procesów odzyskiwania konta. |
| **Regulacje i audyty** | Silniejsze uwierzytelnianie wspiera podejście wieloskładnikowe; decyzje compliance zawsze z prawnikiem / audytorem. | Nie obiecywać „zgodności z PCI / DORA / NIS2” wyłącznie przez passkey — to element szerszej architektury. |
| **Konkurencyjność i wizerunek** | Usługi finansowe, Big Tech i wiele aplikacji B2C już pokazuje logowanie bez hasła — użytkownicy to kojarzą z nowoczesnością i bezpieczeństwem. | W B2B często passkey **uzupełnia** SSO (SAML/OIDC), zamiast go zastępować. |
| **Skalowanie produktu** | Ten sam wzorzec API (rejestracja / asercja) dla web i wielu platform, zamiast wielu odrębnych, niestandardowych hacków. | Koszt wdrożenia to integracja, testy, UX odzyskiwania i wsparcie użytkowników. |

**Jedno zdanie na zakończenie slajdu korzyści:** inwestycja w passkey to inwestycja w **standard**, **UX** i **redukcję najczęstszych klas problemów z hasłami**, przy świadomym zaplanowaniu odzyskiwania konta i polityki urządzeń.

---

## Inne techniki uwierzytelniania — kontekst i porównanie

Krótki przegląd technik, które audytorium (biznes + security) zwykle zestawia z passkey:

| Technika | Idea | Mocne strony | Ograniczenia względem passkey |
|----------|------|--------------|--------------------------------|
| **Hasło** | Współdzielony sekret pamiętany przez użytkownika; na serwerze zwykle hash. | Proste do zrozumienia, wszechobecne. | Phishing, reuse, słabe hasła, koszty resetów; serwer musi chronić bazy sekretów (lub ich pochodnych). |
| **Hasło + TOTP (aplikacja authenticator)** | Drugi składnik oparty o czasowy kod z sekretu. | Bez SMS; powszechne w firmach. | Phishing w czasie rzeczywistym (proxy), utrata telefonu, koszt onboardingu; użytkownik musi wpisywać kody. |
| **SMS / e-mail OTP** | Jednorazowy kod w kanale. | Znane użytkownikom, łatwe wdrożyć. | SIM swap, opóźnienia, koszty, słabsza odporność na phishing niż WebAuthn; regulatory często ograniczają SMS jako jedyny czynnik. |
| **Magic link (e-mail)** | Logowanie przez jednorazowy link. | Bez hasła z perspektywy UX. | Zależność od skrzynki; link może być przechwycony; nie zastępuje silnego powiązania z urządzeniem jak passkey. |
| **OAuth 2.0 / OpenID Connect (np. „Zaloguj przez …”)** | Zaufanie zewnętrznemu IdP; tokeny i sesja u Was. | Szybki start, SSO, mniej haseł u Was — jeśli użytkownik i tak ma konto u IdP. | Zależność od dostawcy; polityka MFA po stronie IdP; to **federacja tożsamości**, nie to samo co lokalny passkey u Waszej aplikacji (choć IdP może oferować passkey po swojej stronie). |
| **Klucz sprzętowy FIDO (U2F/FIDO2)** | Fizyczny token USB/NFC/BLE. | Bardzo silny czynnik, popularny w adminach i wysokim ryzyku. | Koszt, logistyka, gubienie tokena; passkey na telefonie często wygodniejszy dla masowego B2C. |
| **Certyfikaty klienta (mTLS)** | TLS z certyfikatem po stronie klienta. | Silne w machine-to-machine i niektórych sieciach firmowych. | Trudniejsze dla typowego użytkownika konsumenckiego niż WebAuthn w przeglądarce. |
| **Biometria „sama” (bez WebAuthn)** | Odcisk twarzy na urządzeniu odblokowuje coś lokalnie. | Wygoda. | Jeśli nie jest powiązana ze standardem FIDO/WebAuthn i z serwerem przez asercje, **nie** jest tym samym co passkey w sensie protokołu internetowego. |

**Passkey** najczęściej **współistnieje** z innymi metodami: np. SSO dla pracowników, passkey lub hasło+MFA dla partnerów, stopniowe włączanie passkey dla klientów z jasnym procesem backupu i wsparcia.

---

## 1. Slajdy (tytuły)

1. Tytuł — Passkey / WebAuthn w praktyce (demo: PassKeyExample).
2. Problem — hasła, phishing, koszty wsparcia (krótko).
3. Czym jest passkey — FIDO2 / WebAuthn; klucz prywatny na urządzeniu, publiczny u usługi; sync vs device-bound; szczegóły w sekcji „Passkey i WebAuthn — rozszerzony opis”.
4. Passkey a inne metody — hasło, TOTP, SMS, magic link, OAuth/OIDC, klucz sprzętowy, mTLS (skrót: sekcja „Inne techniki uwierzytelniania”).
5. Korzyści biznesowe — UX, koszty helpdesku, ryzyko, konkurencyjność; szczegóły w sekcji „Korzyści biznesowe — szczegółowy opis” (bez obietnic compliance bez prawnika).
6. Co dostaje security — wiązanie z domeną (rpId), brak „sekretu hasła” w stylu współdzielonego sekretu serwer–użytkownik (model kryptograficzny inny niż hasło); uczciwie: odzysk konta, polityka urządzeń, passkey zsynchronizowane w chmurze platformy.
7. Architektura demo — Angular + API .NET, PostgreSQL, opcjonalnie RabbitMQ; przepływ register/start → finish, login/start → finish.
8. Live demo — pełny stack Docker (zalecane): utworzenie użytkownika (Swagger/curl), rejestracja passkey, logowanie.
9. Podsumowanie — jeden standard, ścieżka do produkcji wymaga twardej weryfikacji kryptograficznej (w demo uproszczenia).
10. Pytania.

---

## 2. Skrypt minutowy (~15 min)

| Czas | Treść |
|------|--------|
| 0:00–0:45 | Powitanie, kim jesteście, że to aplikacja demonstracyjna, nie produkcja. Agenda. |
| 0:45–3:00 | Problem + czym jest passkey (FIDO2/WebAuthn, prywatny vs publiczny klucz, sync vs urządzenie). |
| 3:00–4:00 | Opcjonalnie slajd „inne metody”: jedna tabela lub 3 przykłady (hasło+TOTP, OAuth, klucz sprzętowy) — passkey jako uzupełnienie, nie zawsze zamiennik SSO. |
| 4:00–6:30 | Korzyści biznesowe: tabela z dokumentu — UX, helpdesk, ryzyko, rynek; bez obietnic compliance bez prawnika. |
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

- [Przegląd projektu](docs/09-projekty/01-przeglad-projektu.md)
- [Passkey Implementation](docs/09-projekty/02-passkey-implementation.md)
- [Quick Start](QUICK_START.md) (uwaga: ścieżka developerska vs Docker — patrz sekcja 4 powyżej)

