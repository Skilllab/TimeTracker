using FluentAssertions;
using TimeTracker.Domain.Common;
using TimeTracker.Domain.Projects;
using TimeTracker.Domain.Tags;
using TimeTracker.Domain.TimeTracking;
using TimeTracker.Domain.TimeTracking.Events;
using Xunit;

namespace TimeTracker.Domain.Tests;

public class TimeEntryTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void StartNew_CreatesRunningEntry()
    {
        // Act
        var entry = TimeEntry.StartNew("Работушка", startedAt: Now);

        // Assert
        entry.Id.Value.Should().NotBeEmpty();
        entry.Description.Should().Be("Работушка");
        entry.Range.IsRunning.Should().BeTrue();
        entry.Range.Start.Should().Be(Now);
        entry.IsBillable.Should().BeTrue();
        entry.Tags.Should().BeEmpty();
    }

    [Fact]
    public void StartNew_NullDescription_BecomesEmpty()
    {
        // Act
        var entry = TimeEntry.StartNew(null, startedAt: Now);

        // Assert
        entry.Description.Should().BeEmpty();
    }

    [Fact]
    public void StartNew_TrimsDescription()
    {
        // Act
        var entry = TimeEntry.StartNew("  Работушка  ", startedAt: Now);

        // Assert
        entry.Description.Should().Be("Работушка");
    }

    [Fact]
    public void StartNew_TooLongDescription_Throws()
    {
        // Arrange
        var long_ = new string('a', 501);

        // Act
        var act = () => TimeEntry.StartNew(long_, startedAt: Now);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void StartNew_RaisesTimerStartedEvent()
    {
        // Act
        var entry = TimeEntry.StartNew("Работушка", startedAt: Now);

        // Assert
        entry.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TimerStartedEvent>()
            .Which.EntryId.Should().Be(entry.Id);
    }

    [Fact]
    public void Stop_OnRunningEntry_SetsEndAndRaisesEvent()
    {
        // Arrange
        var entry = TimeEntry.StartNew("Работушка", startedAt: Now);
        entry.ClearDomainEvents();

        // Act
        entry.Stop(Now.AddMinutes(30));

        // Assert
        entry.Range.IsRunning.Should().BeFalse();
        entry.Range.End.Should().Be(Now.AddMinutes(30));
        entry.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TimerStoppedEvent>()
            .Which.TotalDuration.Value.Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public void Stop_OnStoppedEntry_Throws()
    {
        // Arrange
        var entry = TimeEntry.StartNew("Работушка", startedAt: Now);
        entry.Stop(Now.AddMinutes(30));

        // Act
        var act = () => entry.Stop(Now.AddMinutes(45));

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Stop_BeforeStart_Throws()
    {
        // Arrange
        var entry = TimeEntry.StartNew("Работушка", startedAt: Now);

        // Act
        var act = () => entry.Stop(Now.AddMinutes(-1));

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ChangeDescription_TrimsAndRaisesEvent()
    {
        // Arrange
        var entry = TimeEntry.StartNew("Работушка", startedAt: Now);
        entry.ClearDomainEvents();

        // Act
        entry.ChangeDescription("  дескрипсионе  ");

        // Assert
        entry.Description.Should().Be("дескрипсионе");
        entry.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EntryEditedEvent>();
    }

    [Fact]
    public void ChangeDescription_SameValue_DoesNotRaise()
    {
        // Arrange
        var entry = TimeEntry.StartNew("Работушка", startedAt: Now);
        entry.ClearDomainEvents();

        // Act
        entry.ChangeDescription("Работушка");

        // Assert
        entry.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AssignProject_RaisesEvent()
    {
        // Arrange
        var entry = TimeEntry.StartNew("Работушка", startedAt: Now);
        entry.ClearDomainEvents();
        var projectId = ProjectId.New();

        // Act
        entry.AssignProject(projectId);

        // Assert
        entry.ProjectId.Should().Be(projectId);
        entry.DomainEvents.Should().ContainSingle(e => e is EntryEditedEvent);
    }

    [Fact]
    public void AssignProject_SameValue_DoesNotRaise()
    {
        // Arrange
        var projectId = ProjectId.New();
        var entry = TimeEntry.StartNew("Работушка", projectId, Now);
        entry.ClearDomainEvents();

        // Act
        entry.AssignProject(projectId);

        // Assert
        entry.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AddTag_AddsAndRaisesEvent()
    {
        // Arrange
        var entry = TimeEntry.StartNew("Работушка", startedAt: Now);
        entry.ClearDomainEvents();
        var tag = Tag.Create("ASAP");

        // Act
        entry.AddTag(tag);

        // Assert
        entry.Tags.Should().ContainSingle().Which.Should().Be(tag);
        entry.DomainEvents.Should().ContainSingle(e => e is EntryEditedEvent);
    }

    [Fact]
    public void AddTag_Duplicate_DoesNotRaise()
    {
        // Arrange
        var entry = TimeEntry.StartNew("Работушка", startedAt: Now);
        var tag = Tag.Create("ASAP");
        entry.AddTag(tag);
        entry.ClearDomainEvents();

        // Act
        entry.AddTag(tag);

        // Assert
        entry.Tags.Should().HaveCount(1);
        entry.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void RemoveTag_RemovesAndRaises()
    {
        // Arrange
        var entry = TimeEntry.StartNew("Работушка", startedAt: Now);
        var tag = Tag.Create("ASAP");
        entry.AddTag(tag);
        entry.ClearDomainEvents();

        // Act
        entry.RemoveTag(tag);

        // Assert
        entry.Tags.Should().BeEmpty();
        entry.DomainEvents.Should().ContainSingle(e => e is EntryEditedEvent);
    }

    [Fact]
    public void RemoveTag_NotPresent_DoesNotRaise()
    {
        // Arrange
        var entry = TimeEntry.StartNew("Работушка", startedAt: Now);
        entry.ClearDomainEvents();

        // Act
        entry.RemoveTag(Tag.Create("потрачено"));

        // Assert
        entry.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Restore_CreatesEntryWithoutEvents()
    {
        // Arrange
        var id = TimeEntryId.New();
        var range = new TimeRange(Now.AddMinutes(-30), Now);

        // Act
        var entry = TimeEntry.Restore(id, "восстановлен", range, null, isBillable: true);

        // Assert
        entry.Id.Should().Be(id);
        entry.DomainEvents.Should().BeEmpty();
        entry.Range.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void Restore_WithTags_RestoresThem()
    {
        // Arrange
        var id = TimeEntryId.New();
        var range = new TimeRange(Now.AddMinutes(-30), Now);
        var tags = new[] { Tag.Create("ASAP"), Tag.Create("уплочено") };

        // Act
        var entry = TimeEntry.Restore(id, "восстановлен", range, null, isBillable: true, tags);

        // Assert
        entry.Tags.Should().HaveCount(2);
        entry.Tags.Should().Contain(Tag.Create("ASAP"));
        entry.Tags.Should().Contain(Tag.Create("уплочено"));
    }

    [Fact]
    public void Tags_ReturnsSameViewInstance()
    {
        // Arrange
        var entry = TimeEntry.StartNew("работушка", startedAt: Now);

        // Act
        var view1 = entry.Tags;
        var view2 = entry.Tags;

        // Assert: кэшированная обёртка, один и тот же объект
        view1.Should().BeSameAs(view2);
    }

    [Fact]
    public void Tags_ReflectsMutations()
    {
        // Arrange
        var entry = TimeEntry.StartNew("работушка", startedAt: Now);
        var view = entry.Tags;   // держим обёртку

        // Act: мутируем агрегат
        entry.AddTag(Tag.Create("urgent"));

        // Assert: обёртка видит изменение, потому что держит ссылку
        // на тот же List<Tag>
        view.Should().ContainSingle()
            .Which.Name.Should().Be("urgent");
    }

    [Fact]
    public void Tags_IsReadOnlyOutside()
    {
        // Arrange
        var entry = TimeEntry.StartNew("работушка", startedAt: Now);
        entry.AddTag(Tag.Create("urgent"));

        // Act
        var view = entry.Tags;

        // Assert: view — ReadOnlyCollection, не List
        view.Should().NotBeAssignableTo<List<Tag>>();

        // Проверить, что добавить нельзя — через explicit cast к ICollection
        var act = () => ((ICollection<Tag>)view).Add(Tag.Create("hacked"));
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void Equality_SameId_AreEqual()
    {
        // Arrange
        var id = TimeEntryId.New();
        var range = new TimeRange(Now, Now.AddMinutes(30));

        // Act
        var a = TimeEntry.Restore(id, "a", range, null, true);
        var b = TimeEntry.Restore(id, "b", range, null, true);

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentId_AreNotEqual()
    {
        // Arrange
        var range = new TimeRange(Now, Now.AddMinutes(30));

        // Act
        var a = TimeEntry.Restore(TimeEntryId.New(), "x", range, null, true);
        var b = TimeEntry.Restore(TimeEntryId.New(), "x", range, null, true);

        // Assert
        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_Null_IsFalse()
    {
        // Arrange
        var entry = TimeEntry.StartNew("Работушка", startedAt: Now);

        // Act
        var result = entry.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equality_DifferentType_AreNotEqual()
    {
        // Arrange
        var entry = TimeEntry.StartNew("Работушка", startedAt: Now);
        var project = Project.Create("Прожект", ProjectColor.FromRgb(0, 0, 0));

        // Act
        var result = entry.Equals(project);

        // Assert
        result.Should().BeFalse();
    }
}
