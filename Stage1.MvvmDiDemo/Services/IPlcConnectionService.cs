namespace Stage1.MvvmDiDemo.Services;

/// <summary>
/// PLC 连接服务契约（工控背景：真实项目里这就是 IPlcService 的极简版）。
/// ViewModel 只依赖这个接口，不关心背后是 Mock 还是西门子 S7。
/// </summary>
public interface IPlcConnectionService
{
    bool IsConnected { get; }

    /// <summary>连接状态变更事件（true=已连接，false=已断开）。</summary>
    event Action<bool>? ConnectionChanged;

    Task<bool> ConnectAsync(string ip, int port);
    Task DisconnectAsync();
}