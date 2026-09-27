using FluentAssertions;
using TimeTracker.Domain;
using Xunit;
using DomainTaskStatus = TimeTracker.Domain.TaskStatus;

namespace TimeTracker.Domain.Tests;

public sealed class WorkTaskTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid ProjectId = Guid.NewGuid();

    [Fact]
    public void Delete_ForRunningTask_Throws()
    {
        var task = CreateTask().Start(Start.AddMinutes(1));

        var act = () => task.Delete();

        act.Should().Throw<InvalidTaskStateException>();
    }

    [Fact]
    public void Delete_ForPausedTask_KeepsProjectAndTags()
    {
        var task = CreateTask()
            .Start(Start.AddMinutes(1))
            .Pause(Start.AddMinutes(2))
            .AddTag("Отчеты");

        var deleted = task.Delete();

        deleted.IsDeleted.Should().BeTrue();
        deleted.ProjectId.Should().Be(ProjectId);
        deleted.Tags.Should().Be("Отчеты");
        deleted.Status.Should().Be(DomainTaskStatus.Paused);
    }

    [Fact]
    public void Restore_AfterDelete_ClearsFlag()
    {
        var deleted = CreateTask().Delete();

        var restored = deleted.Restore();

        restored.IsDeleted.Should().BeFalse();
        restored.ProjectId.Should().Be(ProjectId);
    }

    [Fact]
    public void Finish_AfterDelete_KeepsDeletedFlag()
    {
        var task = CreateTask()
            .Start(Start.AddMinutes(1))
            .Pause(Start.AddMinutes(2))
            .Delete()
            .Finish(Start.AddMinutes(3));

        task.IsDeleted.Should().BeTrue();
        task.Status.Should().Be(DomainTaskStatus.Finished);
    }

    private static WorkTask CreateTask()
    {
        return new WorkTask(
            Guid.NewGuid(),
            "Работа",
            ProjectId,
            Start,
            null,
            null,
            null,
            null,
            0,
            false,
            DomainTaskStatus.NotStarted);
    }
}
