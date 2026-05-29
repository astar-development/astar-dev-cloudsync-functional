using System.Reactive.Subjects;
using AStar.Dev.CloudSyncFunctional.LogViewer;
using AStar.Dev.CloudSyncFunctional.Tests.Unit.Infrastructure;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Serilog.Parsing;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.LogViewer;

public class GivenAnInMemoryLogSink : IClassFixture<ReactiveUiFixture>
{
    private static LogEvent CreateLogEvent(string message, LogEventLevel level = LogEventLevel.Information)
    {
        var template = new MessageTemplate(message, [new TextToken(message)]);

        return new LogEvent(DateTimeOffset.UtcNow, level, null, template, []);
    }

    [Fact]
    public void when_constructed_then_snapshot_is_empty()
    {
        var sut = new InMemoryLogSink();

        sut.GetSnapshot().ShouldBeEmpty();
    }

    [Fact]
    public void when_entry_emitted_then_snapshot_contains_it()
    {
        var sut = new InMemoryLogSink();

        sut.Emit(CreateLogEvent("hello"));

        sut.GetSnapshot().Count.ShouldBe(1);
    }

    [Fact]
    public void when_500_entries_emitted_then_snapshot_count_is_500()
    {
        var sut = new InMemoryLogSink();

        for (var i = 0; i < 500; i++)
            sut.Emit(CreateLogEvent($"entry {i}"));

        sut.GetSnapshot().Count.ShouldBe(500);
    }

    [Fact]
    public void when_501_entries_emitted_then_oldest_is_evicted()
    {
        var sut = new InMemoryLogSink();
        sut.Emit(CreateLogEvent("first entry"));

        for (var i = 0; i < 500; i++)
            sut.Emit(CreateLogEvent($"entry {i}"));

        var snapshot = sut.GetSnapshot();
        snapshot.Count.ShouldBe(500);
        snapshot.Any(e => e.Message == "first entry").ShouldBeFalse();
    }

    [Fact]
    public void when_entry_with_email_is_emitted_then_email_is_scrubbed()
    {
        var sut = new InMemoryLogSink();

        sut.Emit(CreateLogEvent("user email@example.com logged in"));

        var snapshot = sut.GetSnapshot();
        snapshot[0].Message.ShouldContain("[email]");
        snapshot[0].Message.ShouldNotContain("email@example.com");
    }

    [Fact]
    public void when_entry_with_username_is_emitted_then_username_is_scrubbed()
    {
        var sut = new InMemoryLogSink();
        var username = Environment.UserName;

        sut.Emit(CreateLogEvent($"running as {username} on this machine"));

        sut.GetSnapshot()[0].Message.ShouldNotContain(username);
    }

    [Fact]
    public void when_entry_emitted_then_entry_added_observable_pushes_it()
    {
        var sut = new InMemoryLogSink();
        LogEntry? received = null;
        sut.EntryAdded.Subscribe(entry => received = entry);

        sut.Emit(CreateLogEvent("observable test"));

        received.ShouldNotBeNull();
        received.Message.ShouldContain("observable test");
    }

    [Fact]
    public void when_disposed_then_get_snapshot_returns_last_known_entries()
    {
        var sut = new InMemoryLogSink();
        sut.Emit(CreateLogEvent("entry one"));
        sut.Emit(CreateLogEvent("entry two"));

        sut.Dispose();

        sut.GetSnapshot().Count.ShouldBe(2);
    }
}
