using FluentAssertions;
using TimeTracker.Application;
using TimeTracker.Domain;
using Xunit;

namespace TimeTracker.Domain.Tests;

public sealed class TimeEntryEditorTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task RenameAsync_ClosedEntry_SavesNewName()
    {
        var entry = TimeEntry.Start(Guid.NewGuid(), "Работа", Start, null).Close(Start.AddMinutes(30));
        var repository = new FakeEntryRepository(entry);
        var editor = new TimeEntryEditor(repository, new FakeUnitOfWork());

        await editor.RenameAsync(entry.Id, "Работа над отчетом", TestContext.Current.CancellationToken);

        repository.Updated.Should().HaveCount(1);
        repository.Updated[0].Description.Should().Be("Работа над отчетом");
        repository.Updated[0].Id.Should().Be(entry.Id);
    }

    [Fact]
    public async Task RenameAsync_OpenEntry_Throws()
    {
        var entry = TimeEntry.Start(Guid.NewGuid(), "Работа", Start, null);
        var repository = new FakeEntryRepository(entry);
        var editor = new TimeEntryEditor(repository, new FakeUnitOfWork());

        var act = async () => await editor.RenameAsync(entry.Id, "Другое", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidTimeEntryException>()
            .WithMessage("Идущая запись переименовывается на экране таймера.");
        repository.Updated.Should().BeEmpty();
    }

    [Fact]
    public async Task RenameAsync_WithoutName_Throws()
    {
        var entry = TimeEntry.Start(Guid.NewGuid(), "Работа", Start, null).Close(Start.AddMinutes(30));
        var repository = new FakeEntryRepository(entry);
        var editor = new TimeEntryEditor(repository, new FakeUnitOfWork());

        var act = async () => await editor.RenameAsync(entry.Id, "   ", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidTimeEntryException>();
        repository.Updated.Should().BeEmpty();
    }

    [Fact]
    public async Task RenameAsync_UnknownEntry_Throws()
    {
        var repository = new FakeEntryRepository();
        var editor = new TimeEntryEditor(repository, new FakeUnitOfWork());

        var act = async () => await editor.RenameAsync(Guid.NewGuid(), "Работа", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidTimeEntryException>()
            .WithMessage("Запись не найдена.");
    }

    private sealed class FakeEntryRepository : ITimeEntryRepository
    {
        private readonly List<TimeEntry> _entries;

        public FakeEntryRepository(params TimeEntry[] entries)
        {
            _entries = entries.ToList();
        }

        public List<TimeEntry> Updated { get; } = new();

        public Task AddAsync(TimeEntry entry, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<TimeEntry?> GetActiveAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_entries.FirstOrDefault(entry => entry.IsOpen));

        public Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_entries.FirstOrDefault(entry => entry.Id == id));

        public Task<IReadOnlyList<TimeEntry>> GetRangeAsync(
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<TimeEntry>>(Array.Empty<TimeEntry>());

        public Task UpdateAsync(TimeEntry entry, CancellationToken cancellationToken = default)
        {
            Updated.Add(entry);

            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;

            return Task.CompletedTask;
        }
    }
}
