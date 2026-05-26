using AStar.Dev.CloudSyncFunctional.Controls;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Controls;

public sealed class GivenAThroughputCard
{
    [Fact]
    public void when_max_value_is_zero_then_compute_bar_height_returns_min_bar_height() =>
        ThroughputCard.ComputeBarHeight(0, 0, 28.0, 2.0).ShouldBe(2.0);

    [Fact]
    public void when_max_value_is_zero_and_bucket_value_is_nonzero_then_compute_bar_height_returns_min_bar_height() =>
        ThroughputCard.ComputeBarHeight(100, 0, 28.0, 2.0).ShouldBe(2.0);

    [Fact]
    public void when_bucket_value_equals_max_value_then_compute_bar_height_returns_histogram_height()
    {
        var result = ThroughputCard.ComputeBarHeight(50, 50, 28.0, 2.0);

        result.ShouldBe(28.0);
    }

    [Fact]
    public void when_bucket_value_is_half_of_max_value_then_compute_bar_height_returns_half_histogram_height()
    {
        var result = ThroughputCard.ComputeBarHeight(25, 50, 28.0, 2.0);

        result.ShouldBe(14.0);
    }

    [Fact]
    public void when_calculated_height_falls_below_min_bar_height_then_compute_bar_height_returns_min_bar_height()
    {
        var result = ThroughputCard.ComputeBarHeight(1, 1000, 28.0, 2.0);

        result.ShouldBe(2.0);
    }

    [Fact]
    public void when_bucket_value_is_zero_and_max_value_is_positive_then_compute_bar_height_returns_min_bar_height() =>
        ThroughputCard.ComputeBarHeight(0, 100, 28.0, 2.0).ShouldBe(2.0);

    [Fact]
    public void when_bar_index_is_zero_then_is_history_bar_returns_true() =>
        ThroughputCard.IsHistoryBar(0).ShouldBeTrue();

    [Fact]
    public void when_bar_index_is_nineteen_then_is_history_bar_returns_true() =>
        ThroughputCard.IsHistoryBar(19).ShouldBeTrue();

    [Fact]
    public void when_bar_index_is_twenty_then_is_history_bar_returns_false() =>
        ThroughputCard.IsHistoryBar(20).ShouldBeFalse();

    [Fact]
    public void when_bar_index_is_twenty_three_then_is_history_bar_returns_false() =>
        ThroughputCard.IsHistoryBar(23).ShouldBeFalse();
}
