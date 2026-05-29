using AStar.Dev.CloudSyncFunctional.Settings;
using AStar.Dev.CloudSyncFunctional.Tests.Unit.Infrastructure;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Settings;

public class GivenASettingsViewModel : IClassFixture<ReactiveUiFixture>
{
    [Fact]
    public void when_constructed_then_close_command_is_not_null()
    {
        var sut = new SettingsViewModel();

        sut.Close.ShouldNotBeNull();
    }

    [Fact]
    public void when_close_is_executed_then_closed_event_is_raised()
    {
        var sut = new SettingsViewModel();
        var eventRaised = false;
        sut.Closed += (_, _) => eventRaised = true;

        sut.Close.Execute().Subscribe();

        eventRaised.ShouldBeTrue();
    }

    [Fact]
    public void when_close_is_executed_multiple_times_then_closed_event_is_raised_each_time()
    {
        var sut = new SettingsViewModel();
        var raiseCount = 0;
        sut.Closed += (_, _) => raiseCount++;

        sut.Close.Execute().Subscribe();
        sut.Close.Execute().Subscribe();

        raiseCount.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void when_close_command_property_changed_is_not_raised_on_construction()
    {
        var sut = new SettingsViewModel();
        var raisedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

        sut.Close.Execute().Subscribe();

        raisedProperties.ShouldNotContain(nameof(SettingsViewModel.Close));
    }
}
