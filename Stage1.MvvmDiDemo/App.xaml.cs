using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stage1.MvvmDiDemo.Services;
using Stage1.MvvmDiDemo.ViewModels;
using Stage1.MvvmDiDemo.Views;

namespace Stage1.MvvmDiDemo;

/// <summary>
/// 组合根（Composition Root）：
///  - 用 Microsoft.Extensions.Hosting 搭建 DI 容器
///  - 注册服务接口 → 实现类
///  - 注册 ViewModel / Window
///  - 启动时手动从容器取 MainWindow 并 Show
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // 服务契约 → 实现
                // 想切真机？只改这一行：services.AddSingleton<IPlcConnectionService, RealPlcConnectionService>();
                services.AddSingleton<IPlcConnectionService, MockPlcConnectionService>();

                // ViewModel / Window 都用 Transient（每次取都新建）
                services.AddTransient<MainViewModel>();
                services.AddTransient<MainWindow>();
            })
            .Build();

        await _host.StartAsync();

        // 从容器拿到窗口和 VM，手动装配 DataContext
        var window = _host.Services.GetRequiredService<MainWindow>();
        window.DataContext = _host.Services.GetRequiredService<MainViewModel>();
        window.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(2));
            _host.Dispose();
        }
        base.OnExit(e);
    }
}