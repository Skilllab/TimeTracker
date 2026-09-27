using FluentAssertions;
using Xunit;

namespace TimeTracker.Domain.Tests;

public sealed class TaskTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithoutName_Throws()
    {
        var act = () => WorkTask.Create(Guid.NewGuid(), "   ", null, Start);

        act.Should().Throw<InvalidTaskStateException>()
            .WithMessage("Наименование задачи не может быть пустым.");
    }

    [Fact]
    public void Create_WithName_IsNotStarted()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "  Работа над отчетом  ", null, Start);

        task.Description.Should().Be("Работа над отчетом");
        task.Status.Should().Be(TaskStatus.NotStarted);
        task.CreatedAt.Should().Be(Start);
        task.StartedAt.Should().BeNull();
        task.LastStartedAt.Should().BeNull();
        task.Tags.Should().BeEmpty();
        task.ElapsedAt(Start.AddHours(3)).Should().Be(Duration.Zero);
    }

    [Fact]
    public void Start_FromNotStarted_SetsRunning()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", null, Start);

        var started = task.Start(Start.AddMinutes(5));

        started.Status.Should().Be(TaskStatus.Running);
        started.StartedAt.Should().Be(Start.AddMinutes(5));
        started.LastStartedAt.Should().Be(Start.AddMinutes(5));
        started.CreatedAt.Should().Be(Start);
    }

    [Fact]
    public void Start_Twice_Throws()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", null, Start).Start(Start.AddMinutes(5));

        var act = () => task.Start(Start.AddMinutes(10));

        act.Should().Throw<InvalidTaskStateException>()
            .WithMessage("Начать можно только задачу, которая еще не начиналась.");
    }

    [Fact]
    public void Pause_FromRunning_SetsPaused()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", null, Start).Start(Start.AddMinutes(5));

        var paused = task.Pause(Start.AddMinutes(15));

        paused.Status.Should().Be(TaskStatus.Paused);
        paused.PausedAt.Should().Be(Start.AddMinutes(15));
    }

    [Fact]
    public void Pause_WithoutStart_Throws()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", null, Start);

        var act = () => task.Pause(Start.AddMinutes(5));

        act.Should().Throw<InvalidTaskStateException>()
            .WithMessage("Приостановить можно только выполняемую задачу.");
    }

    [Fact]
    public void Resume_FromPaused_UpdatesLastStartedAndCountsPause()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", null, Start)
            .Start(Start)
            .Pause(Start.AddMinutes(10));

        var resumed = task.Resume(Start.AddMinutes(40));

        resumed.Status.Should().Be(TaskStatus.Running);
        resumed.LastStartedAt.Should().Be(Start.AddMinutes(40));
        resumed.StartedAt.Should().Be(Start);
        resumed.PausedSeconds.Should().Be(1800);
        resumed.ElapsedAt(Start.AddMinutes(40)).Should().Be(Duration.From(TimeSpan.FromMinutes(10)));
    }

    [Fact]
    public void Resume_WithoutPause_Throws()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", null, Start).Start(Start);

        var act = () => task.Resume(Start.AddMinutes(5));

        act.Should().Throw<InvalidTaskStateException>()
            .WithMessage("Продолжить можно только приостановленную задачу.");
    }

    [Fact]
    public void ElapsedAt_WhilePaused_DoesNotGrow()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", null, Start)
            .Start(Start)
            .Pause(Start.AddMinutes(10));

        task.ElapsedAt(Start.AddHours(2)).Should().Be(Duration.From(TimeSpan.FromMinutes(10)));
    }

    [Fact]
    public void Finish_FromRunning_SetsFinished()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", null, Start).Start(Start);

        var finished = task.Finish(Start.AddMinutes(30));

        finished.Status.Should().Be(TaskStatus.Finished);
        finished.FinishedAt.Should().Be(Start.AddMinutes(30));
        finished.ElapsedAt(Start.AddHours(5)).Should().Be(Duration.From(TimeSpan.FromMinutes(30)));
    }

    [Fact]
    public void Finish_FromPaused_ExcludesPauseTime()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", null, Start)
            .Start(Start)
            .Pause(Start.AddMinutes(10));

        var finished = task.Finish(Start.AddMinutes(40));

        finished.PausedSeconds.Should().Be(1800);
        finished.ElapsedAt(Start.AddMinutes(40)).Should().Be(Duration.From(TimeSpan.FromMinutes(10)));
    }

    [Fact]
    public void Finish_WithoutStart_Throws()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", null, Start);

        var act = () => task.Finish(Start.AddMinutes(5));

        act.Should().Throw<InvalidTaskStateException>()
            .WithMessage("Завершить можно только выполняемую или приостановленную задачу.");
    }

    [Fact]
    public void Reopen_FromFinished_ReturnsToPaused()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", null, Start)
            .Start(Start)
            .Finish(Start.AddMinutes(30));

        var reopened = task.Reopen(Start.AddHours(1));

        reopened.Status.Should().Be(TaskStatus.Paused);
        reopened.FinishedAt.Should().BeNull();
        reopened.PausedAt.Should().Be(Start.AddHours(1));
        reopened.ElapsedAt(Start.AddHours(4)).Should().Be(Duration.From(TimeSpan.FromMinutes(30)));
    }

    [Fact]
    public void Reopen_NotFinished_Throws()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", null, Start).Start(Start);

        var act = () => task.Reopen(Start.AddMinutes(5));

        act.Should().Throw<InvalidTaskStateException>()
            .WithMessage("Вернуть в работу можно только завершенную задачу.");
    }

    [Fact]
    public void Rename_KeepsStateAndMoments()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", Guid.NewGuid(), Start).Start(Start);

        var renamed = task.Rename("  Другая работа  ");

        renamed.Description.Should().Be("Другая работа");
        renamed.Id.Should().Be(task.Id);
        renamed.ProjectId.Should().Be(task.ProjectId);
        renamed.Status.Should().Be(TaskStatus.Running);
        renamed.StartedAt.Should().Be(Start);
    }

    [Fact]
    public void Rename_WithoutName_Throws()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", null, Start);

        var act = () => task.Rename("   ");

        act.Should().Throw<InvalidTaskStateException>()
            .WithMessage("Наименование задачи не может быть пустым.");
    }

    [Fact]
    public void AddTag_ThenRemoveTag_ChangesString()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Задача", null, Start);

        var tagged = task.AddTag("Работа");
        var cleaned = tagged.RemoveTag("Работа");

        task.Tags.Should().BeEmpty();
        tagged.Tags.Should().Be("Работа");
        cleaned.Tags.Should().BeEmpty();
    }

    [Fact]
    public void AddTag_SameNameTwice_KeepsSingleEntry()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Задача", null, Start)
            .AddTag("Работа")
            .AddTag("работа");

        task.TagNames.Should().ContainSingle().Which.Should().Be("Работа");
    }

    [Fact]
    public void RemoveTag_UnknownName_KeepsTags()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Задача", null, Start).AddTag("Работа");

        var cleaned = task.RemoveTag("Учеба");

        cleaned.TagNames.Should().ContainSingle().Which.Should().Be("Работа");
    }

    [Fact]
    public void AddTag_WithInvalidSymbol_Throws()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Задача", null, Start);

        var act = () => task.AddTag("Работа!");

        act.Should().Throw<InvalidTaskStateException>();
    }

    [Fact]
    public void Tags_AfterFinishAndReopen_AreKept()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Задача", null, Start)
            .AddTag("Работа")
            .Start(Start)
            .Finish(Start.AddMinutes(10));

        var reopened = task.Reopen(Start.AddHours(1));

        reopened.TagNames.Should().ContainSingle().Which.Should().Be("Работа");
        reopened.ElapsedAt(Start.AddHours(1)).Should().Be(Duration.From(TimeSpan.FromMinutes(10)));
    }
}
