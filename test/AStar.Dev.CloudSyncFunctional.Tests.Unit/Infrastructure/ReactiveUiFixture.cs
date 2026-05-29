using ReactiveUI;
using ReactiveUI.Builder;
using System.Reactive.Concurrency;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Infrastructure;

public sealed class ReactiveUiFixture
{
    static ReactiveUiFixture()
    {
        RxAppBuilder.CreateReactiveUIBuilder()
            .WithCoreServices()
            .BuildApp();

        RxSchedulers.MainThreadScheduler = ImmediateScheduler.Instance;
    }
}
