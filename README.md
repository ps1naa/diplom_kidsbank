# KidBank - Family Banking API for Kids

Production-ready backend для мобильного банковского приложения для детей с двумя ролями: Parent и Kid.

## Технологический стек

- **Framework**: ASP.NET Core 8.0 Web API
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, API)
- **Database**: PostgreSQL с Entity Framework Core
- **Authentication**: JWT с Refresh Tokens
- **CQRS**: MediatR
- **Validation**: FluentValidation
- **Real-time**: SignalR для чата

## Структура проекта

```
KidBank/
├── KidBank.Domain/           # Доменные сущности, Value Objects, интерфейсы
├── KidBank.Application/      # CQRS (Commands/Queries), Handlers, Validators
├── KidBank.Infrastructure/   # EF Core, Persistence, Identity Services
└── KidBank.API/              # Controllers, Middleware, SignalR Hubs
```

## Основные функции

### Аутентификация
- Регистрация родителя с созданием семьи
- Регистрация ребёнка по приглашению
- JWT-аутентификация с ротацией refresh-токенов
- Отзыв всех сессий

### Финансы (Ledger-based accounting)
- Неизменяемые транзакции (INSERT only)
- Перевод между счетами
- Пополнение счёта ребёнка
- Optimistic concurrency для Account, SpendingLimit, WishlistGoal

### Задания и награды
- Создание заданий родителем
- Выполнение заданий ребёнком
- Автоматический перевод награды при одобрении

### Цели накопления
- Создание целей с дедлайном
- Пополнение целей из основного счёта
- Отслеживание прогресса

### Запросы денег
- Запрос от ребёнка к родителю
- Одобрение/отклонение с автоматическим переводом

### Лимиты расходов
- Дневные, недельные, месячные лимиты
- Автоматический сброс при смене периода
- Валидация при транзакциях

### Образование и геймификация
- Образовательные модули с квизами
- XP и система уровней
- Стрики активности
- Достижения

### Чат
- Семейный чат (SignalR)
- Личные сообщения

## API Endpoints

### Auth
- `POST /api/v1/auth/register/parent` - Регистрация родителя
- `POST /api/v1/auth/register/kid` - Регистрация ребёнка
- `POST /api/v1/auth/login` - Вход
- `POST /api/v1/auth/refresh` - Обновление токена
- `POST /api/v1/auth/logout` - Выход
- `POST /api/v1/auth/revoke-all` - Отзыв всех сессий

### Families
- `POST /api/v1/families` - Создание семьи
- `POST /api/v1/families/invite` - Генерация приглашения
- `GET /api/v1/families/dashboard` - Дашборд семьи
- `GET /api/v1/families/kids` - Список детей

### Users
- `GET /api/v1/users/me` - Текущий пользователь
- `PUT /api/v1/users/me` - Обновление профиля
- `GET /api/v1/users/{id}` - Получение пользователя

### Accounts
- `GET /api/v1/accounts/my` - Мои счета
- `GET /api/v1/accounts/kid/{kidId}` - Счета ребёнка
- `GET /api/v1/accounts/{accountId}/transactions` - История транзакций
- `POST /api/v1/accounts/savings` - Создание накопительного счёта
- `POST /api/v1/accounts/deposit` - Пополнение счёта ребёнка
- `POST /api/v1/accounts/transfer` - Перевод между счетами

### Goals
- `POST /api/v1/goals` - Создание цели
- `PUT /api/v1/goals/{goalId}` - Обновление цели
- `DELETE /api/v1/goals/{goalId}` - Удаление цели
- `GET /api/v1/goals/my` - Мои цели
- `GET /api/v1/goals/kid/{kidId}` - Цели ребёнка
- `POST /api/v1/goals/{goalId}/deposit` - Пополнение цели

### Tasks
- `POST /api/v1/tasks` - Создание задания
- `PUT /api/v1/tasks/{taskId}` - Обновление задания
- `DELETE /api/v1/tasks/{taskId}` - Удаление задания
- `GET /api/v1/tasks/my` - Мои задания
- `POST /api/v1/tasks/{taskId}/complete` - Выполнение задания
- `POST /api/v1/tasks/{taskId}/approve` - Одобрение задания
- `POST /api/v1/tasks/{taskId}/reject` - Отклонение задания

### Money Requests
- `POST /api/v1/money-requests` - Создание запроса
- `GET /api/v1/money-requests/my` - Мои запросы
- `GET /api/v1/money-requests/pending` - Ожидающие запросы
- `POST /api/v1/money-requests/{id}/approve` - Одобрение
- `POST /api/v1/money-requests/{id}/reject` - Отклонение

### Spending Limits
- `POST /api/v1/spending-limits` - Установка лимита
- `PUT /api/v1/spending-limits/{id}` - Обновление лимита
- `GET /api/v1/spending-limits/kid/{kidId}` - Лимиты ребёнка

### Education
- `GET /api/v1/education/lessons` - Список уроков
- `GET /api/v1/education/lessons/{id}` - Детали урока
- `POST /api/v1/education/quiz/submit` - Отправка ответа
- `GET /api/v1/education/progress` - Прогресс обучения

### Analytics
- `GET /api/v1/analytics/kid/{kidId}/summary` - Сводка по ребёнку
- `GET /api/v1/analytics/kid/{kidId}/monthly` - Месячная статистика
- `GET /api/v1/analytics/family/overview` - Обзор семьи

### Chat
- `GET /api/v1/chat/history` - История сообщений
- SignalR Hub: `/hubs/chat`

## Запуск

### С Docker Compose

```bash
docker-compose up -d
```

API будет доступен на `http://localhost:5000`

### Локально

1. Установите PostgreSQL
2. Обновите строку подключения в `appsettings.Development.json`
3. Запустите миграции:

```bash
cd KidBank.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../KidBank.API
dotnet ef database update --startup-project ../KidBank.API
```

4. Запустите приложение:

```bash
cd KidBank.API
dotnet run
```

## Конфигурация

### JWT Settings
```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-at-least-32-characters",
    "Issuer": "KidBank",
    "Audience": "KidBankApp",
    "ExpirationMinutes": 60
  }
}
```

### Database
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=kidbank;Username=postgres;Password=password"
  }
}
```

## Безопасность

- Пароли хэшируются с BCrypt
- JWT токены с коротким сроком жизни
- Refresh токены с ротацией и возможностью отзыва
- Role-based авторизация (Parent/Kid)
- Проверка принадлежности к семье для всех операций
- Optimistic concurrency для финансовых операций

## Особенности реализации

- **Ledger-based accounting**: Все финансовые операции создают immutable Transaction записи
- **Optimistic concurrency**: Account, SpendingLimit, WishlistGoal используют version token
- **No global query filters**: Для избежания проблем с required relationships
- **Feature-first structure**: Команды и запросы организованы по функциональным модулям
- **Result pattern**: Без исключений для бизнес-логики
