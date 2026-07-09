# Розгортання SmartTable на Railway (railway.app)

## Крок 0 — обов'язкові зміни в коді (зробіть це ДО пушу на GitHub)

1. Замініть `Program.cs` у `CourseManagementApi` вмістом з `Program.Api.cs`.
2. Замініть `Program.cs` у `SmartTable.Client` вмістом з `Program.Client.cs`.
3. Покладіть `Dockerfile` у **корінь репозиторію** (там, де лежать папки `CourseManagementApi` і `SmartTable.Client`, а не всередині однієї з них).
4. Видаліть (або ігноруйте) старі `docker-compose.yml`, `nginx.conf`, окремі `Dockerfile`-и для VPS — вони більше не потрібні для Railway, все тепер в одному контейнері.

## Крок 1 — викласти код на GitHub

Якщо ще не викладено:
```powershell
cd C:\Users\ypkab\Documents\SmartTable
git init
git add .
git commit -m "Ready for Railway"
```
Створіть новий приватний репозиторій на github.com і запуште його туди (GitHub Desktop — найпростіше, якщо не звикли до команд git).

## Крок 2 — створити проєкт на Railway

1. Зареєструйтесь на [railway.app](https://railway.app) (можна через GitHub-акаунт).
2. **New Project → Deploy from GitHub repo** → оберіть ваш репозиторій `SmartTable`.
3. Railway побачить `Dockerfile` в корені й запропонує білдити саме так — погоджуйтесь.

## Крок 3 — додати базу даних

1. У тому ж проєкті: **+ New → Database → Add PostgreSQL**.
2. Railway сам створить базу і згенерує змінну `DATABASE_URL`.

## Крок 4 — налаштувати змінні середовища для сервісу застосунку

Відкрийте ваш сервіс (не базу) → вкладка **Variables** → додайте:

| Змінна | Значення |
|---|---|
| `ConnectionStrings__DefaultConnection` | див. нижче — треба зібрати з даних Postgres-плагіна |
| `Jwt__Key` | новий згенерований ключ (нижче) |
| `Jwt__Issuer` | `SmartTableApi` |
| `Jwt__Audience` | `SmartTableClient` |
| `Jwt__ExpiresHours` | `12` |
| `SEED_ADMIN1_PASSWORD` | ваш новий пароль для admin1 |
| `SEED_ADMIN2_PASSWORD` | ваш новий пароль для admin2 |
| `SEED_SUPERADMIN_PASSWORD` | ваш новий пароль для superadmin |

**Новий JWT-ключ (для продакшена, відмінний від того, що вже засвітився в цьому чаті):**
```
usqZ9H5W0TCWL-TJxJbQdwW-HMSXQcUuMGspte8pPBeyLTauarBTcbknIhvBz3_p
```

**Рядок підключення до бази** — Railway дає окремі змінні (`PGHOST`, `PGPORT`, `PGDATABASE`, `PGUSER`, `PGPASSWORD`) від Postgres-плагіна. Натисніть на плагін бази → вкладка **Variables**, скопіюйте значення і зберіть рядок так:
```
Host=<PGHOST>;Port=<PGPORT>;Database=<PGDATABASE>;Username=<PGUSER>;Password=<PGPASSWORD>
```
Або простіше: у Railway можна послатись на змінну іншого сервісу напряму — у полі значення для `ConnectionStrings__DefaultConnection` введіть:
```
Host=${{Postgres.PGHOST}};Port=${{Postgres.PGPORT}};Database=${{Postgres.PGDATABASE}};Username=${{Postgres.PGUSER}};Password=${{Postgres.PGPASSWORD}}
```
(назва `Postgres` має збігатися з назвою вашого сервісу бази в Railway — подивіться зліва в списку сервісів проєкту).

## Крок 5 — деплой

Після збереження змінних Railway сам перезапустить білд. Проґрес видно у вкладці **Deployments**. Перше застосування міграцій і засіювання адмінів (`admin1`, `admin2`, `superadmin`) відбудеться автоматично при старті — так само, як і локально.

## Крок 6 — домен

У вкладці **Settings → Networking** натисніть **Generate Domain** — отримаєте щось на кшталт `smarttable-production.up.railway.app`, вже з готовим HTTPS. Свій домен (якщо є) можна прив'язати там же через **Custom Domain**.

## Готово

Відкрийте виданий домен — має одразу показати `/login`. Заходьте вже нOVYMI паролями, які задали у змінних `SEED_*`.

## Оновлення після змін коду

Просто `git push` — Railway сам перезбере і передеплоїть.
