using FluentAssertions;
using TimeTracker.Application;
using TimeTracker.Domain;
using TimeTracker.Presentation.ViewModels;
using Xunit;
using DomainTaskStatus = TimeTracker.Domain.TaskStatus;

namespace TimeTracker.UI.Tests;

public sealed class TaskCardViewModelTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CanChangeProject_ForRunningTask_IsFalse()
    {
        var card = new TaskCardViewModel(CreateItem(DomainTaskStatus.Running));

        card.CanChangeProject.Should().BeFalse();
    }

    [Fact]
    public void CanChangeProject_ForPausedTask_IsTrue()
    {
        var card = new TaskCardViewModel(CreateItem(DomainTaskStatus.Paused));

        card.CanChangeProject.Should().BeTrue();
    }

    [Fact]
    public void CanChangeProject_ForNotStartedTask_IsTrue()
    {
        var card = new TaskCardViewModel(CreateItem(DomainTaskStatus.NotStarted));

        card.CanChangeProject.Should().BeTrue();
    }

    [Fact]
    public void CanChangeProject_ForFinishedTask_IsTrue()
    {
        var card = new TaskCardViewModel(CreateItem(DomainTaskStatus.Finished));

        card.CanChangeProject.Should().BeTrue();
    }

    private static TaskListItem CreateItem(DomainTaskStatus status)
    {
        return new TaskListItem(
            Id: Guid.NewGuid(),
            Description: "Работа",
            ProjectId: null,
            ProjectName: string.Empty,
            ProjectColor: string.Empty,
            Status: status,
            Elapsed: Duration.Zero,
            CreatedAt: Start,
            StartedAt: null,
            LastStartedAt: null,
            FinishedAt: null,
            Tags: string.Empty);
    }
}
