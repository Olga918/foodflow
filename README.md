# FoodFlow

FoodFlow — веб-приложение на ASP.NET Core MVC для управления рестораном и онлайн-заказами.  
Проект объединяет клиентские заказы, работу кухни, склад/закупки и админ-отчеты в одной системе.

## Возможности

- Меню, корзина, checkout и страница успешного заказа для клиента
- Личная страница **My Orders** с таймлайном статусов
- Панель кухни и подготовка порций на линии
- Управление складом и списками закупок для кладовщика
- Админ-панель с отчетами
- Страница Contact и единый UI-стиль в формате cafe template

## Роли

- **Client**: просмотр меню, оформление и отслеживание своих заказов
- **Cook**: обработка заказов и подготовка порций на кухне
- **Storekeeper**: управление остатками и ингредиентами рецептов
- **Admin**: полный доступ, аналитика и закупки

## Технологии

- **.NET 10**, ASP.NET Core MVC
- **Entity Framework Core**
- **ASP.NET Core Identity** (авторизация и роли)
- **Bootstrap 5** + кастомные стили
- **MySQL** (текущая конфигурация запуска)

## Локальный запуск

### 1) Восстановление пакетов и сборка

```bash
dotnet restore
dotnet build
```

### 2) Настройка базы данных

Укажи строку подключения в `FoodFlow/appsettings.json` или через переменную окружения:

- `ConnectionStrings__DefaultConnection`

Пример формата:

```text
Server=YOUR_HOST;Port=3306;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASSWORD;SslMode=Preferred
```

### 3) Запуск

```bash
dotnet run --project FoodFlow/FoodFlow.csproj
```

## Деплой (Linux / Pterodactyl)

Команда запуска:

```bash
dotnet FoodFlow.dll --urls http://0.0.0.0:${PORT}
```

Если деплоишь через publish:

```bash
dotnet publish FoodFlow/FoodFlow.csproj -c Release -o publish
```

Загрузи содержимое папки `publish` на сервер и запускай `FoodFlow.dll`.

## Примечания

- При первом запуске приложение заполняет базовые роли/пользователей/данные меню через `DbSeeder`.
- Секреты (реальные пароли и строки подключения) не храни в git; для продакшена используй переменные окружения.
