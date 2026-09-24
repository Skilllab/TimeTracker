using FluentAssertions;
using TimeTracker.Application;
using Xunit;

namespace TimeTracker.Domain.Tests;


public sealed class TimerControlTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static (ITimerControl Control, FakeTimeProvider Time, FakeRepository Repository, FakeUnitOfWork UnitOfWork) CreateControl()
    {
        var time = new FakeTimeProvider(Start);
        var repository = new FakeRepository();
        var unitOfWork = new FakeUnitOfWork();
        ITimerControl control = new TimerControl(new TimerSession(), time, repository, unitOfWork);

        return (control, time, repository, unitOfWork);
    }

    [Fact]
    public async Task Start_WithProject_SavesProjectId()
    {
        var (control, _, repository, _) = CreateControl();
        var projectId = Guid.NewGuid();

        await control.Start(projectId);

        repository.Added.Should().HaveCount(1);
        repository.Added[0].ProjectId.Should().Be(projectId);
    }

    [Fact]
    public async Task Start_WithoutProject_SavesEmptyLink()
    {
        var (control, _, repository, _) = CreateControl();

        await control.Start(projectId: null);

        repository.Added.Should().HaveCount(1);
        repository.Added[0].ProjectId.Should().BeNull();
    }


    [Fact]
    public void BeforeStart_ElapsedIsZero()
    {
        var (control, _, _, _) = CreateControl();

        control.IsRunning.Should().BeFalse();
        control.IsPaused.Should().BeFalse();
        control.IsFinished.Should().BeFalse();
        control.GetElapsed().Should().Be(Duration.Zero);
    }

    [Fact]
    public void Start_ThenTimeAdvance_CountsUp()
    {
        var (control, time, _, _) = CreateControl();

        control.Start(projectId: null);
        time.Advance(TimeSpan.FromSeconds(65));

        control.IsRunning.Should().BeTrue();
        control.GetElapsed().ToClockString().Should().Be("01:05");
    }

    [Fact]
    public void Pause_FreezesElapsed()
    {
        var (control, time, _, _) = CreateControl();
        control.Start(projectId: null);
        time.Advance(TimeSpan.FromSeconds(10));

        control.Pause();
        time.Advance(TimeSpan.FromMinutes(5));

        control.IsPaused.Should().BeTrue();
        control.GetElapsed().ToClockString().Should().Be("00:10");
    }

    [Fact]
    public void Resume_ContinuesFromSameValue()
    {
        var (control, time, _, _) = CreateControl();
        control.Start(projectId: null);
        time.Advance(TimeSpan.FromSeconds(10));
        control.Pause();
        time.Advance(TimeSpan.FromMinutes(1));

        control.Resume();
        time.Advance(TimeSpan.FromSeconds(5));

        control.IsRunning.Should().BeTrue();
        control.GetElapsed().ToClockString().Should().Be("00:15");
    }

    [Fact]
    public async Task Stop_SavesEntryWithPauseSeconds()
    {
        var (control, time, repository, unitOfWork) = CreateControl();
        await control.Start(projectId: null);
        time.Advance(TimeSpan.FromSeconds(10));
        await control.Pause();
        time.Advance(TimeSpan.FromMinutes(1));

        await control.Stop();

        control.IsFinished.Should().BeTrue();
        repository.Added.Should().HaveCount(1);
        repository.Updated.Should().HaveCount(2);

        var closed = repository.Updated[^1];
        closed.StartedAt.Should().Be(Start);
        closed.EndedAt.Should().Be(Start.AddSeconds(70));
        closed.PausedSeconds.Should().Be(60);
        closed.ElapsedAt(Start).ToClockString().Should().Be("00:10");
        unitOfWork.SaveCount.Should().Be(3);
    }

    [Fact]
    public async Task Pause_WithoutStart_Throws()
    {
        var (control, _, _, _) = CreateControl();

        var act = async () => await control.Pause();

        await act.Should().ThrowAsync<InvalidTimerStateException>();
    }

    [Fact]
    public async Task Stop_WithoutStart_Throws()
    {
        var (control, _, repository, unitOfWork) = CreateControl();

        var act = async () => await control.Stop();

        await act.Should().ThrowAsync<InvalidTimerStateException>();
        repository.Added.Should().BeEmpty();
        unitOfWork.SaveCount.Should().Be(0);
    }

    [Fact]
    public async Task Start_SavesOpenEntry()
    {
        var (control, _, repository, unitOfWork) = CreateControl();

        await control.Start(projectId: null);

        repository.Added.Should().HaveCount(1);
        repository.Added[0].IsOpen.Should().BeTrue();
        repository.Added[0].StartedAt.Should().Be(Start);
        unitOfWork.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task Pause_SavesPauseMoment()
    {
        var (control, time, repository, _) = CreateControl();
        await control.Start(projectId: null);
        time.Advance(TimeSpan.FromSeconds(10));

        await control.Pause();

        repository.Updated.Should().HaveCount(1);
        repository.Updated[0].PausedAt.Should().Be(Start.AddSeconds(10));
    }

    [Fact]
    public async Task Stop_ClosesSavedEntry()
    {
        var (control, time, repository, _) = CreateControl();
        await control.Start(projectId: null);
        time.Advance(TimeSpan.FromSeconds(10));

        await control.Stop();

        repository.Updated.Should().HaveCount(1);
        repository.Updated[0].EndedAt.Should().Be(Start.AddSeconds(10));
        repository.Updated[0].IsOpen.Should().BeFalse();
    }

    [Fact]
    public async Task RestoreAsync_WithSavedEntry_StartsPaused()
    {
        var (control, _, repository, _) = CreateControl();
        repository.Active = new TimeEntry(
            id: Guid.NewGuid(),
            description: string.Empty,
            startedAt: Start.AddMinutes(-10),
            endedAt: null,
            pausedSeconds: 60,
            pausedAt: null,
            isBillable: false,
            projectId: null);

        await control.RestoreAsync(TestContext.Current.CancellationToken);

        control.IsPaused.Should().BeTrue();
        control.IsRunning.Should().BeFalse();
        control.GetElapsed().Should().Be(Duration.From(TimeSpan.FromMinutes(9)));
    }

    [Fact]
    public async Task RestoreAsync_WithoutSavedEntry_StaysIdle()
    {
        var (control, _, _, _) = CreateControl();

        await control.RestoreAsync(TestContext.Current.CancellationToken);

        control.IsRunning.Should().BeFalse();
        control.IsPaused.Should().BeFalse();
        control.IsFinished.Should().BeFalse();
    }

    private sealed class FakeRepository : ITimeEntryRepository
    {
        public List<TimeEntry> Added { get; } = new();

        public List<TimeEntry> Updated { get; } = new();

        public TimeEntry? Active { get; set; }

        public Task AddAsync(TimeEntry entry, CancellationToken cancellationToken = default)
        {
            Added.Add(entry);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(TimeEntry entry, CancellationToken cancellationToken = default)
        {
            Updated.Add(entry);
            return Task.CompletedTask;
        }

        public Task<TimeEntry?> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Active);
        }

        public Task<IReadOnlyList<TimeEntry>> GetRangeAsync(
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<TimeEntry> entries = Added;
            return Task.FromResult(entries);
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
