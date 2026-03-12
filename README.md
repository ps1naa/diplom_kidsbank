# KidBank — Family Banking API for Kids

Бэкенд мобильного банковского приложения для детей с ролями **Parent** и **Kid**: счета, задания, цели, лимиты, образование и геймификация.

## Технологический стек

- **.NET 8** — ASP.NET Core Web API
- **Архитектура** — Clean Architecture (Domain, Application, Infrastructure, API)
- **БД** — PostgreSQL (две базы: основная `kidbank` и настройки `kidbank_settings`)
- **ORM** — Entity Framework Core 8
- **Кэш и рассылка событий** — Redis (Pub/Sub для синхронизации настроек между инстансами)
- **Аутентификация** — JWT + Refresh Tokens, BCrypt для паролей
- **CQRS** — MediatR
- **Валидация** — FluentValidation
- **Шифрование данных** — AES-256-GCM для чувствительных полей в БД
- **Real-time** — SignalR (чат)

## Структура решения

```
KidBank.sln
├── KidBank.Domain/           # Сущности, enum'ы, доменные сервисы (статичные)
├── KidBank.Application/      # CQRS (Commands/Queries), Handlers, Validators, интерфейсы
├── KidBank.Infrastructure/   # EF Core, DbContext'ы, репозитории, Redis, шифрование
├── KidBank.API/              # Контроллеры, Middleware, SignalR Hub
├── KidBank.Domain.Tests/
├── KidBank.Application.Tests/
└── KidBank.Infrastructure.Tests/
```

## Основные возможности

- **Auth** — регистрация родителя/ребёнка по инвайту, логин, refresh, logout, revoke-all
- **Семья** — дашборд, список детей, генерация приглашений
- **Счета** — основной и накопительные счета, пополнение, переводы, депозит родителем ребёнку
- **Виртуальные карты** — создание, заморозка/разморозка
- **Цели** — создание, пополнение, прогресс
- **Задания** — создание родителем, выполнение ребёнком, одобрение/отклонение, награда
- **Запросы денег** — запрос ребёнка, одобрение/отклонение родителем
- **Лимиты расходов** — по периоду (Daily/Weekly/Monthly)
- **Образование** — уроки, квизы, XP, стрики, достижения
- **Аналитика** — сводка по ребёнку, месячная статистика, обзор семьи
- **Чат** — история сообщений (REST), SignalR Hub
- **Настройки** — глобальные в БД (с учётом hostname), клиентские настройки пользователя

## Запуск

### Требования

- **.NET 8 SDK**
- **Docker и Docker Compose** (для PostgreSQL и Redis) **или** установленные локально PostgreSQL 18 и Redis 8

---

### Вариант 1: Всё через Docker Compose

1. В корне проекта:

```bash
docker-compose up -d
```

2. Дождаться готовности контейнеров (healthcheck для `db` и `redis`).

3. Применить миграции к основной БД и к БД настроек (на хосте, с подключением к порту контейнера):

```bash
# Переменная окружения (Windows: set ASPNETCORE_ENVIRONMENT=Development; Linux/macOS: export ASPNETCORE_ENVIRONMENT=Development)
set ASPNETCORE_ENVIRONMENT=Development

# Основная БД (ApplicationDbContext)
dotnet ef database update --project KidBank.Infrastructure --startup-project KidBank.API --context ApplicationDbContext

# БД настроек (SettingsDbContext)
dotnet ef database update --project KidBank.Infrastructure --startup-project KidBank.API --context SettingsDbContext
```

4. API из контейнера будет доступен на **http://localhost:5000** (порт 80 внутри маппится на 5000).  
   Swagger: **http://localhost:5000/swagger**

Пароль PostgreSQL в `docker-compose`: `postgres` (логин `postgres`). В контейнере уже создаётся БД `kidbank_settings` через `init-settings-db.sql`.

---

### Вариант 2: Локальный запуск API (PostgreSQL и Redis в Docker)

1. Запустить только БД и Redis:

```bash
docker-compose up -d db redis
```

2. Создать базу для настроек (если не создаётся скриптом при первом запуске):

```bash
docker exec -it <имя_контейнера_postgres> psql -U postgres -c "CREATE DATABASE kidbank_settings;"
# Имя контейнера: docker ps (например diplomv2-db-1). Ошибка «already exists» не критична.
```

3. В `appsettings.Development.json` (или через переменные окружения) задать:

- `ConnectionStrings:DefaultConnection` — строка к основной БД `kidbank`
- `ConnectionStrings:SettingsConnection` — строка к БД `kidbank_settings`
- `Redis:ConnectionString` — например `localhost:6379`
- `Encryption:Key` — Base64-строка ровно 32 байта (см. ниже)
- `Jwt:SecretKey` — свой секрет для JWT

Пример (подставьте свой пароль и при необходимости хост/порт):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=kidbank;Username=postgres;Password=YOUR_PASSWORD",
    "SettingsConnection": "Host=localhost;Port=5432;Database=kidbank_settings;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Encryption": {
    "Key": "BASE64_STRING_32_BYTES_SEE_BELOW"
  },
  "Jwt": {
    "SecretKey": "YOUR_JWT_SECRET_AT_LEAST_32_CHARS_BASE64"
  }
}
```

4. Сгенерировать ключ шифрования (32 байта в Base64):

```powershell
# PowerShell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))
```

```bash
# Linux/macOS
openssl rand -base64 32
```

Подставить результат в `Encryption:Key`.

5. Применить миграции (из корня решения):

```bash
set ASPNETCORE_ENVIRONMENT=Development

dotnet ef database update --project KidBank.Infrastructure --startup-project KidBank.API --context ApplicationDbContext
dotnet ef database update --project KidBank.Infrastructure --startup-project KidBank.API --context SettingsDbContext
```

6. Запуск API:

```bash
cd KidBank.API
dotnet run
```

По умолчанию приложение слушает порты из launchSettings (часто 5000/5001 или 5100 при переопределении). Swagger в Development: **http://localhost:5000/swagger** (или тот порт, что выводится в консоли).

---

### Вариант 3: API и PostgreSQL локально, только Redis в Docker

Удобно, когда PostgreSQL уже установлен на машине, а Redis поднимается одним контейнером.

1. Запустить только Redis:

```bash
docker-compose up -d redis
```

2. Убедиться, что локальный PostgreSQL запущен. Создать базы (если ещё нет):

```sql
CREATE DATABASE kidbank;
CREATE DATABASE kidbank_settings;
```

3. В `appsettings.Development.json` указать подключение к **локальному** PostgreSQL и к Redis на localhost (порт 6379 проброшен из контейнера):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=kidbank;Username=postgres;Password=ВАШ_ПАРОЛЬ",
    "SettingsConnection": "Host=localhost;Port=5432;Database=kidbank_settings;Username=postgres;Password=ВАШ_ПАРОЛЬ"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Encryption": {
    "Key": "BASE64_КЛЮЧ_32_БАЙТА"
  },
  "Jwt": {
    "SecretKey": "ВАШ_JWT_СЕКРЕТ"
  }
}
```

4. Ключ шифрования и JWT — как в варианте 2 (шаг 4).

5. Из корня проекта применить миграции (подключение к localhost идёт из `appsettings.Development.json`):

```bash
set ASPNETCORE_ENVIRONMENT=Development
dotnet ef database update --project KidBank.Infrastructure --startup-project KidBank.API --context ApplicationDbContext
dotnet ef database update --project KidBank.Infrastructure --startup-project KidBank.API --context SettingsDbContext
```

6. Запустить API локально:

```bash
cd KidBank.API
dotnet run
```

Swagger: **http://localhost:5000/swagger** (или порт из консоли). Redis при этом работает в контейнере на порту 6379.

---

### Вариант 4: Всё локально (без Docker)

1. Установить PostgreSQL 18 и Redis 8.
2. Создать базы:

```sql
CREATE DATABASE kidbank;
CREATE DATABASE kidbank_settings;
```

3. Настроить `appsettings.Development.json` как в варианте 2 (хост, порт, пароль, Redis, Encryption, Jwt).
4. Выполнить шаги 4–6 из варианта 2 (ключ шифрования, миграции, `dotnet run`).

---

## Конфигурация

| Секция | Описание |
|--------|----------|
| `ConnectionStrings:DefaultConnection` | Основная БД приложения (`kidbank`) |
| `ConnectionStrings:SettingsConnection` | БД настроек (`kidbank_settings`) |
| `Redis:ConnectionString` | Подключение к Redis (например `localhost:6379`) |
| `Encryption:Key` | Ключ AES-256 в Base64 (ровно 32 байта) |
| `Jwt:SecretKey` | Ключ для подписи JWT |
| `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpirationMinutes` | Параметры JWT |

Настройки приложения хранятся в БД `kidbank_settings` с поддержкой hostname (конкретный хост или `*`). При изменении настроек через API публикуется событие в Redis — все инстансы API обновляют кэш без перезапуска.

## API (кратко)

Базовый префикс: **`/api/v1`**. Полная документация после запуска: **/swagger**.

- **Auth** — `POST auth/register/parent`, `POST auth/register/kid`, `POST auth/login`, `POST auth/refresh`, `POST auth/logout`, `POST auth/revoke-all`
- **Users** — `GET users/me`, `PUT users/me`, `GET users/{id}`
- **Families** — `GET families/dashboard`, `GET families/kids`, `POST families/invite`
- **Accounts** — `GET accounts/my`, `GET accounts/kid/{kidId}`, `GET accounts/{accountId}/transactions`, `POST accounts/savings`, `POST accounts/deposit`, `POST accounts/transfer`, `POST accounts/topup`
- **Cards** — `GET cards/my`, `POST cards`, `POST cards/{cardId}/freeze`, `POST cards/{cardId}/unfreeze`
- **Goals** — `POST goals`, `PUT goals/{goalId}`, `DELETE goals/{goalId}`, `GET goals/my`, `GET goals/kid/{kidId}`, `POST goals/{goalId}/deposit`
- **Tasks** — `POST tasks`, `PUT tasks/{taskId}`, `DELETE tasks/{taskId}`, `GET tasks/my`, `GET tasks/pending-approval`, `POST tasks/{taskId}/complete`, `POST tasks/{taskId}/approve`, `POST tasks/{taskId}/reject`
- **MoneyRequests** — `POST moneyrequests`, `GET moneyrequests/my`, `GET moneyrequests/pending`, `POST moneyrequests/{id}/approve`, `POST moneyrequests/{id}/reject`
- **Allowances** — `POST allowances`, `GET allowances/kid/{kidId}`
- **SpendingLimits** — `POST spendinglimits`, `PUT spendinglimits/{id}`, `GET spendinglimits/kid/{kidId}`
- **Education** — `GET education/lessons`, `GET education/lessons/{id}`, `POST education/quiz/submit`, `GET education/progress`, `POST education/xp/add`, `POST education/streak/update`, `POST education/achievement/unlock`
- **Achievements** — `GET achievements`, `GET achievements/my`
- **Categories** — `GET categories`, `POST categories/block`
- **Notifications** — `GET notifications`, `POST notifications/{id}/read`
- **Activity** — `GET activity/feed`
- **Chat** — `GET chat/history`; SignalR Hub: `/hubs/chat`
- **Leaderboard** — `GET leaderboard/family`
- **Analytics** — `GET analytics/kid/{kidId}/summary`, `GET analytics/kid/{kidId}/monthly`, `GET analytics/family/overview`
- **Settings** — `GET settings/client`, `PUT settings/client`

## Тесты

Из корня решения:

```bash
dotnet test KidBank.sln
```

Запускаются юнит- и интеграционные тесты (в т.ч. с реальными PostgreSQL и Redis при доступности localhost).

## Безопасность

- Пароли — BCrypt
- JWT с ограниченным временем жизни, refresh с ротацией
- Роли и политики (Parent/Kid), проверка принадлежности к семье
- Чувствительные поля в БД (email, имена, пароль-хеш, данные карт, описание транзакций и т.п.) шифруются AES-256-GCM
- Поиск пользователя по email — по HMAC-SHA256 хешу (`normalized_email_hash`), сам email в БД хранится в зашифрованном виде
- Аудит — логирование запросов в таблицу `audit_logs`

## Особенности реализации

- **Две БД** — основная и отдельная для настроек; настройки с учётом hostname и Redis Pub/Sub для синхронизации между инстансами
- **Ledger** — финансовые операции только через неизменяемые записи транзакций
- **Optimistic concurrency** — для Account, SpendingLimit, WishlistGoal
- **Result-паттерн** — бизнес-ошибки без исключений
- **EF Core Value Converters** — шифрование/дешифрование на уровне чтения/записи; проекции по зашифрованным полям выполняются в памяти после материализации запроса
