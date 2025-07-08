# Dekauto: 🟢 Сервис Экспорта (Dekauto.Export.Service)
### Сервис выгрузки данных в виде Excel-файла (или архива) ответом на запрос из сервиса [Студенты](https://github.com/TOXYGENCY/Dekauto.Students.Service). Автономен, пассивен и не взаимодействует с базой данных.

### 🔸 Функции
- Принятие данных для заполнения Excel-файлов
- Заполнение Excel-файлов данными (+ формирование ZIP-архива)
- Отправка результата ответом на запрос

### 🛠 Технологии
- .NET 8 (ASP.NET Core 8)
- OpenAPI Swagger
- Git
- Docker
- CI (GitHub Actions)
- PostgreSQL 17 (+ Entity Framework Core)
- Grafana Loki + Promtail + Prometheus (логирование и метрики)

## ❇ API-справка
>#### Расположен на портах `5505 (HTTP)` и `5506 (HTTPS)`
#### Контроллер: StudentCards (требует авторизацию)
- `POST   api/studentcards/student` - **ExportStudentAsync** - Экспорт информации о студенте в Excel
- `POST   api/studentcards/students` - **ExportStudentsAsync** - Экспорт списка студентов в ZIP-архив (содержащий Excel-файлы)

_Контроллер: Metrics (DEPRECATED - будет удален)_
- `GET    api/metrics/healthcheck` - **HealthCheckAsync** - Проверка состояния сервиса (health check)
- `GET    api/metrics/requests` - **RequestsPerPeriodAsync** - Получение метрик запросов за период (требует авторизацию)

---
># ℹ О Dekauto
>### Что такое Dekauto?
>Dekauto - это web-сервис, направленный на автоматизацию некоторых процессов работы деканата высшего учебного заведения. На данный момент система специализирована для работы в определенном ВУЗе и исполняет функции хранения, агрегации и вывода данных студентов. Ввод осуществляется через Excel-файлы определенного формата. Выводом является Excel-файл карточки студента с заполненными данными. 
>
>### Общая структура Dekauto
>* ⚪ [Dekauto.Auth.Service](https://github.com/TOXYGENCY/Dekauto.Auth.Service) - Сервис управления аккаунтами и входом. 
>    * DockerHub-образ: `toxygency/dekauto_auth_service:release`
>* 🔵 [Dekauto.Students.Service](https://github.com/TOXYGENCY/Dekauto.Students.Service) - Сервис управления Студентами.
>    * DockerHub-образ: `toxygency/dekauto_students_service:release`
>* 🟣 [Dekauto.Import.Service](https://github.com/TOXYGENCY/Dekauto.Import.Service) - Сервис парсинга Excel-файлов для импорта.
>    * DockerHub-образ: `toxygency/dekauto_import_service:release`
>* 🟢 [Dekauto.Export.Service](https://github.com/TOXYGENCY/Dekauto.Export.Service) - Сервис формирования выходного Excel-файла. _(Вы здесь)_
>    * DockerHub-образ: `toxygency/dekauto_export_service:release`
>* 🟠 [Dekauto.Angular.Frontend](https://github.com/TOXYGENCY/Dekauto.Angular.Frontend) - Фронтенд: Web-приложение на Angular v19 + NGINX.
>    * DockerHub-образ: `toxygency/dekauto_frontend_nginx:release`
