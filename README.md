# TimeTracker

Кроссплатформенный... шутка, WPF. PET-проект — трекер времени, вдохновленный Toggl Track.

## Стек

- .NET 10
- WPF + CommunityToolkit.Mvvm
- EF Core 10 + SQLite
- Serilog
- LiveCharts2 (позже)
- H.NotifyIcon (позже)

## Требования

- .NET SDK 10.0.100+
- Windows 10 1903+ / Windows 11

## Команды

```bash
# Восстановить зависимости
dotnet restore

# Собрать
dotnet build -c Release

# Тесты
dotnet test

# Запустить WPF
dotnet run --project src/TimeTracker.Wpf

# Новая миграция EF Core
dotnet ef migrations add <Name> --project src/TimeTracker.Infrastructure --startup-project src/TimeTracker.Wpf

# Применить миграции
dotnet ef database update --project src/TimeTracker.Infrastructure --startup-project src/TimeTracker.Wpf
```

## Структура

```
src/
  TimeTracker.Domain/          — сущности, VO, доменные события
  TimeTracker.Application/     — use cases, интерфейсы, DTO
  TimeTracker.Infrastructure/  — EF Core, SQLite, файлы
  TimeTracker.Wpf/             — UI, ViewModels, DI
tests/
  TimeTracker.Domain.Tests/
  TimeTracker.Application.Tests/
  TimeTracker.Integration.Tests/
```

## Лицензия

MIT