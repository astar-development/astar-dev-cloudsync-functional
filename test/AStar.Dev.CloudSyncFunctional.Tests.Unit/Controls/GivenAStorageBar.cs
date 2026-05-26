using AStar.Dev.CloudSyncFunctional.Controls;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Controls;

public class GivenAStorageBar
{
    [Fact]
    public void when_used_is_zero_then_fraction_is_zero()
    {
        var fraction = StorageBar.ComputeFraction(0, 100);

        fraction.ShouldBe(0.0);
    }

    [Fact]
    public void when_used_equals_total_then_fraction_is_one()
    {
        var fraction = StorageBar.ComputeFraction(100, 100);

        fraction.ShouldBe(1.0);
    }

    [Fact]
    public void when_total_is_zero_then_fraction_is_zero()
    {
        var fraction = StorageBar.ComputeFraction(50, 0);

        fraction.ShouldBe(0.0);
    }

    [Fact]
    public void when_used_exceeds_total_then_fraction_is_clamped_to_one()
    {
        var fraction = StorageBar.ComputeFraction(150, 100);

        fraction.ShouldBe(1.0);
    }

    [Fact]
    public void when_used_is_half_of_total_then_fraction_is_point_five()
    {
        var fraction = StorageBar.ComputeFraction(50, 100);

        fraction.ShouldBe(0.5);
    }
}
