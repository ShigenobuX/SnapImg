using Application = System.Windows.Application;
using StartupEventArgs = System.Windows.StartupEventArgs;

namespace ImgConv;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        if (CommandLine.Run(e.Args)) Shutdown();
        else new MainWindow().Show();
    }
}
