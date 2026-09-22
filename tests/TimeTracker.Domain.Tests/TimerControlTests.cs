using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using TimeTracker.Application;
using TimeTracker.Domain;
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

        control.Start();
        time.Advance(TimeSpan.FromSeconds(65));

        control.IsRunning.Should().BeTrue();
        control.GetElapsed().ToClockString().Should().Be("01:05");
    }

    [Fact]
    public void Pause_FreezesElapsed()
    {
        var (control, time, _, _) = CreateControl();
        control.Start();
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
        control.Start();
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
        control.Start();
        time.Advance(TimeSpan.FromSeconds(10));
        control.Pause();
        time.Advance(TimeSpan.FromMinutes(1));

        await control.Stop();

        control.IsFinished.Should().BeTrue();
        repository.Added.Should().HaveCount(1);
        repository.Added[0].StartedAt.Should().Be(Start);
        repository.Added[0].EndedAt.Should().Be(Start.AddSeconds(70));
        repository.Added[0].PausedSeconds.Should().Be(60);
        repository.Added[0].ElapsedAt(Start).ToClockString().Should().Be("00:10");
        unitOfWork.SaveCount.Should().Be(1);
    }

    [Fact]
    public void Pause_WithoutStart_Throws()
    {
        var (control, _, _, _) = CreateControl();

        var act = () => control.Pause();

        act.Should().Throw<InvalidTimerStateException>();
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

    private sealed class FakeRepository : ITimeEntryRepository
    {
        public List<TimeEntry> Added { get; } = new();

        public Task AddAsync(TimeEntry entry, CancellationToken cancellationToken = default)
        {
            Added.Add(entry);
            return Task.CompletedTask;
        }

        public Task<TimeEntry?> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Added.Find(entry => entry.IsOpen));
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
