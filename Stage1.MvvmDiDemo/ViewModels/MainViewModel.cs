using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Stage1.MvvmDiDemo.Services;

namespace Stage1.MvvmDiDemo.ViewModels;

/// <summary>
/// 主 ViewModel：
///  - 输入属性：Ip / Port（[ObservableProperty] 自动生成带通知的 Ip/Port 属性）
///  - 状态属性：IsConnected / StatusText / LogText（变了 UI 自动刷新）
///  - 命令：ConnectCommand / DisconnectCommand（[RelayCommand] 自动生成 ICommand）
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IPlcConnectionService _plcService;

    [ObservableProperty]
    private string _ip = "127.0.0.1";

    [ObservableProperty]
    private int _port = 502;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _statusText = "未连接";

    [ObservableProperty]
    private string _logText = "";

    // 专门用于禁用按钮的"忙碌"标志
    [ObservableProperty]
    private bool _isBusy;

    public MainViewModel(IPlcConnectionService plcService)
    {
        _plcService = plcService;

        // 订阅服务事件：真机/Mock 掉线时会回调这里
        _plcService.ConnectionChanged += OnConnectionChanged;

        // 初始化时同步一次当前状态
        IsConnected = _plcService.IsConnected;
    }

    private void OnConnectionChanged(bool connected)
    {
        // ⚠️ 阶段②会用 IDispatcherService 切回 UI 线程。
        // 本阶段 Mock 的回调都在 await 后（UI 线程），先不切也没问题。
        IsConnected = connected;
        StatusText = connected ? "已连接" : "未连接";
        AppendLog(connected ? "✅ PLC 连接成功" : "🔌 PLC 已断开");
    }

    /// <summary>
    /// [RelayCommand] 会生成 ConnectCommand（ICommand），XAML 直接 {Binding ConnectCommand}。
    /// async Task 的方法会自动包装成 AsyncRelayCommand，执行期间 IsRunning 可避免重复点击。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync()
    {
        IsBusy = true;
        try
        {
            AppendLog($"⏳ 正在连接 {Ip}:{Port} ...");
            var ok = await _plcService.ConnectAsync(Ip, Port);
            if (!ok)
            {
                StatusText = "连接失败";
                AppendLog("❌ 连接失败");
            }
        }
        catch (Exception ex)
        {
            // 工控软件永远不要让异常冒到 UI 线程去炸掉进程
            StatusText = $"连接异常：{ex.Message}";
            AppendLog($"❌ 异常：{ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanConnect() => !IsBusy && !IsConnected;

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    private async Task DisconnectAsync()
    {
        IsBusy = true;
        try
        {
            AppendLog("⏳ 正在断开 ...");
            await _plcService.DisconnectAsync();
        }
        catch (Exception ex)
        {
            AppendLog($"❌ 断开异常：{ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanDisconnect() => !IsBusy && IsConnected;

    /// <summary>
    /// [ObservableProperty] 生成的属性变更会触发这个 partial 方法。
    /// 属性一变，按钮的可用状态要重算，否则按钮永远灰/永远亮。
    /// 这是 Toolkit 的"手动联动"套路，习惯就好。
    /// </summary>
    partial void OnIsBusyChanged(bool value)
    {
        ConnectCommand.NotifyCanExecuteChanged();
        DisconnectCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsConnectedChanged(bool value)
    {
        ConnectCommand.NotifyCanExecuteChanged();
        DisconnectCommand.NotifyCanExecuteChanged();
    }

    private void AppendLog(string message)
    {
        // 带毫秒时间戳，工控调试必备
        LogText = $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n{LogText}";
    }
}