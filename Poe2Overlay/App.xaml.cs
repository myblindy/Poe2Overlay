using System.Windows;

namespace Poe2Overlay;

public partial class App : Application
{
    readonly CancellationTokenSource cts = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        _ = ImageListener.StartAsync(cts.Token);
    }
}
