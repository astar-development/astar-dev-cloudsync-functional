using ReactiveUI;
using ReactiveUI.Builder;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Infrastructure;

public sealed class ReactiveUiFixture
{
    static ReactiveUiFixture()
    {
        RxAppBuilder.CreateReactiveUIBuilder()
            .WithCoreServices()
            .BuildApp();
    }
}
