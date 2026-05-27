using FpUnit = AStar.Dev.FunctionalParadigm.Unit;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.FunctionalParadigm;

public class GivenAUnit
{
    [Fact]
    public void when_default_is_accessed_then_returns_non_null_instance()
    {
        FpUnit.Default.ShouldNotBeNull();
    }

    [Fact]
    public void when_default_is_accessed_twice_then_same_reference_is_returned()
    {
        FpUnit.Default.ShouldBeSameAs(FpUnit.Default);
    }

    [Fact]
    public void when_two_unit_instances_are_compared_then_they_are_equal()
    {
        var a = new FpUnit();
        var b = new FpUnit();

        a.ShouldBe(b);
    }
}
