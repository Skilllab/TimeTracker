<p align="center">
  <img src="resources/logo.png" alt="TimeTracker" width="180">
</p>

<p align="center">
  <a href="https://github.com/Skilllab/TimeTracker/actions/workflows/ci.yml">
    <img src="https://img.shields.io/github/actions/workflow/status/Skilllab/TimeTracker/ci.yml?label=Build&branch=main" alt="Build">
  </a>
  <img src="https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white" alt=".NET 10">
  <img src="https://img.shields.io/badge/Avalonia-12.1.2-8B44AC?logo=avalonia&logoColor=white" alt="Avalonia">
  <img src="https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-2F6FED" alt="Platform">
  <a href="https://github.com/Skilllab/TimeTracker/commits/main">
    <img src="https://img.shields.io/github/last-commit/Skilllab/TimeTracker" alt="Last commit">
  </a>
  <img src="https://img.shields.io/badge/architecture-Clean%20Architecture-2E7D32" alt="Clean Architecture">
</p>

# TimeTracker

Кроссплатформенное настольное приложение для учета рабочего времени (Windows / macOS / Linux).

## Стек

- **.NET 10** (`net10.0`), **Avalonia UI** — кросс-платформенный интерфейс.
- **CommunityToolkit.Mvvm** — MVVM.
- **EF Core + SQLite** — хранение данных.
- **Serilog** — логирование; **`TimeProvider`** — работа со временем.
- **ScottPlot.Avalonia** — диаграммы в отчетах.
- **Velopack** — установщик и автообновление (артефакты Windows).
- **Clean Architecture** — обязательное требование к структуре решения (см. ниже).

## Архитектура

Проект строится по принципам **Clean Architecture** — это **обязательное требование**
(см. `docs/stages/tech-decisions.md`, ADR-002). Слои разделены **отдельными проектами**, правило
направления зависимостей проверяется компилятором.

## Требования

- .NET SDK **10.0**
- Git
- Для отладки UI (только Debug): инструмент **AvaloniaUI.DeveloperTools** — устанавливается отдельно
  (`dotnet tool install --global AvaloniaUI.DeveloperTools`, команда `avdt`). Без него вызов
  `AttachDeveloperTools()` не сможет подключиться к инспектору.
- Для локальной сборки установщика нужен инструмент **`vpk`** — он восстанавливается командой
  `dotnet tool restore` (объявлен в `.config/dotnet-tools.json`).

## Команды

| Действие | Команда |
|:--|:--|
| Сборка решения | `dotnet build TimeTracker.slnx` |
| Запуск тестов | `dotnet test` |
| Запуск приложения | `dotnet run --project src/TimeTracker.Desktop` |
| Восстановить инструменты | `dotnet tool restore` |
| Сборка установщика (Windows) | `dotnet publish src/TimeTracker.Desktop -c Release -r win-x64 --self-contained true -o publish`, затем `dotnet vpk pack --packId TimeTracker --packVersion 1.0.0 --packDir publish --mainExe TimeTracker.exe --outputDir releases` |
| Выпуск релиза | создать и отправить тег: `git tag v1.0.0` и `git push origin v1.0.0` |
| Форматирование кода | `dotnet format` |

## Релиз и обновление

- **Версия хранится в одном месте.** Номер задается свойством `VersionPrefix` в `Directory.Build.props` и вручную не правится.
- **Релиз запускается тегом.** Тег вида `v1.2.3` запускает конвейер `.github/workflows/release.yml`: из имени тега извлекается номер `1.2.3`, записывается в файл сборки, после чего выполняется сборка, упаковка и публикация.
- **Артефакты.** Собирается самодостаточная сборка приложения под Windows; из нее формируются установщик и файлы обновления (Velopack). Артефакты прикладываются к релизу GitHub и доступны как артефакт сборки.
- **Обновление.** Установленное приложение проверяет обновления при запуске, до показа окна, и устанавливает найденную версию. Для запуска из исходников проверка пропускается: канала обновления у такой сборки нет.
- **Данные не теряются.** База и настройки лежат в папке данных вне области установки (Windows: `%LocalAppData%\TimeTracker`), поэтому обновление их не затрагивает.
- **Платформы.** В этом этапе артефакты формируются только под Windows; установщики Linux и macOS — позже.
- Подробности: `docs/adr/ADR-013-versioning-and-release.md`, `docs/stages/stage-19-packaging-release.md`.

## Структура репозитория

```
src/
  TimeTracker.Domain/          — сущности, VO, инварианты (ни от чего не зависит)
  TimeTracker.Application/     — сценарии, порты (интерфейсы репозиториев, Unit of Work)
  TimeTracker.Infrastructure/  — EF Core + SQLite, реализации портов, платформенные реализации
  TimeTracker.Presentation/    — ViewModels + Views (Avalonia)
  TimeTracker.Desktop/         — тонкая точка входа: Program, App, DI

tests/   — тестовые проекты
docs/    — план и документация (docs/stages/, docs/adr/)
```

**Правило направления зависимостей (обязательное):** `Desktop` → `Presentation` → `Application` → `Domain`;
`Infrastructure` → `Application` → `Domain`. `Domain` не ссылается ни на что; код слоя запрещено размещать
в UI-проекте.

## Лицензия

Уточняется.
