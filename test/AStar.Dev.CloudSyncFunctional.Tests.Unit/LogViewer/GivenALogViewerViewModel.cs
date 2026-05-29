using System.Collections.Generic;
using System.Reactive.Subjects;
using AStar.Dev.CloudSyncFunctional.LogViewer;
using AStar.Dev.CloudSyncFunctional.Tests.Unit.Infrastructure;
using Microsoft.Extensions.Logging;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.LogViewer;

public class GivenALogViewerViewModel : IClassFixture<ReactiveUiFixture>
{
    private static ILogEntryProvider CreateProvider(IReadOnlyList<LogEntry>? snapshot = null, IObservable<LogEntry>? entryAdded = null)
    {
        var provider = Substitute.For<ILogEntryProvider>();
        provider.GetSnapshot().Returns(snapshot ?? []);
        provider.EntryAdded.Returns(entryAdded ?? new Subject<LogEntry>());

        return provider;
    }

    private static LogEntry CreateEntry(LogLevel level = LogLevel.Information, string message = "test message")
        => new(DateTimeOffset.UtcNow, level, message, null);

    [Fact]
    public void when_constructed_then_entries_is_empty_when_provider_has_no_snapshot()
    {
        var sut = new LogViewerViewModel(CreateProvider());

        sut.Entries.ShouldBeEmpty();
    }

    [Fact]
    public void when_constructed_then_entries_loaded_from_snapshot()
    {
        IReadOnlyList<LogEntry> snapshot = [CreateEntry(), CreateEntry(), CreateEntry()];
        var sut = new LogViewerViewModel(CreateProvider(snapshot: snapshot));

        sut.Entries.Count.ShouldBe(3);
    }

    [Fact]
    public void when_entry_added_observable_fires_then_entries_grows()
    {
        var subject = new Subject<LogEntry>();
        var sut = new LogViewerViewModel(CreateProvider(entryAdded: subject));

        subject.OnNext(CreateEntry());

        sut.Entries.Count.ShouldBe(1);
    }

    [Fact]
    public void when_minimum_level_is_warning_then_entries_below_warning_are_not_shown()
    {
        var subject = new Subject<LogEntry>();
        var sut = new LogViewerViewModel(CreateProvider(entryAdded: subject));

        sut.MinimumLevel = LogLevel.Warning;
        subject.OnNext(CreateEntry(LogLevel.Debug, "debug message"));
        subject.OnNext(CreateEntry(LogLevel.Warning, "warning message"));

        sut.Entries.Count.ShouldBe(1);
        sut.Entries[0].Message.ShouldBe("warning message");
    }

    [Fact]
    public void when_autoscroll_is_toggled_then_property_change_is_raised()
    {
        var sut = new LogViewerViewModel(CreateProvider());
        var raisedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

        sut.AutoScroll = !sut.AutoScroll;

        raisedProperties.ShouldContain(nameof(LogViewerViewModel.AutoScroll));
    }

    [Fact]
    public void when_minimum_level_changed_then_property_change_is_raised()
    {
        var sut = new LogViewerViewModel(CreateProvider());
        var raisedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

        sut.MinimumLevel = LogLevel.Error;

        raisedProperties.ShouldContain(nameof(LogViewerViewModel.MinimumLevel));
    }

    [Fact]
    public void when_disposed_then_new_entries_are_not_added_to_entries()
    {
        var subject = new Subject<LogEntry>();
        var sut = new LogViewerViewModel(CreateProvider(entryAdded: subject));

        sut.Dispose();
        subject.OnNext(CreateEntry());

        sut.Entries.ShouldBeEmpty();
    }
}
