using System.Diagnostics;

namespace Stage1.MvvmDiDemo.Services;

/// <summary>
/// 模拟 PLC 服务：无设备、无网络也能开发调 UI。
/// 真实项目里 RealPlcService 用 S7.Net 实现同一个接口，一行 DI 配置就能切换。
/// </summary>
public class MockPlcConnectionService : IPlcConnectionService
{
    // volatile 保证多线程可见性（连接动作可能在后台线程）
    private volatile bool _isConnected;

    public bool IsConnected => _isConnected;

    public event Action<bool>? ConnectionChanged;

    public async Task<bool> ConnectAsync(string ip, int port)
    {
        // 模拟握手耗时
        await Task.Delay(500);

        if (!_isConnected)
        {
            _isConnected = true;
            Debug.WriteLine($"[MOCK PLC] connected to {ip}:{port}");
            ConnectionChanged?.Invoke(true);
        }
        return true;
    }

    public async Task DisconnectAsync()
    {
        await Task.Delay(200);

        if (_isConnected)
        {
            _isConnected = false;
            Debug.WriteLine("[MOCK PLC] disconnected");
            ConnectionChanged?.Invoke(false);
        }
    }
}