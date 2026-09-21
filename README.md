# TimeTracker

Кроссплатформенное настольное приложение для учета рабочего времени (Windows / macOS / Linux).

## Стек

- **.NET 10** (`net10.0`), **Avalonia UI** — кросс-платформенный интерфейс.
- **CommunityToolkit.Mvvm** — MVVM.
- **EF Core + SQLite** — хранение данных.
- **Serilog** — логирование; **`TimeProvider`** — работа со временем.
- **ScottPlot.Avalonia** — диаграммы в отчётах.
- **Velopack** — упаковка и автообновление.

## Требования

- .NET SDK **10.0**
- Git

## Команды

| Действие | Команда |
|:--|:--|
| Сборка решения | `dotnet build TimeTracker.slnx` |
| Запуск тестов | `dotnet test` |
| Запуск приложения | `dotnet run --project src/TimeTracker.Desktop` |
| Восстановить инструменты | `dotnet tool restore` |
| Форматирование кода | `dotnet format` |

## Структура репозитория

```
src/     — код приложения
tests/   — тестовые проекты
docs/    — план и документация (docs/stages/)
```

## Лицензия

Уточняется.
