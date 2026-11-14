using System.Windows;

namespace Stage1.MvvmDiDemo.Views;

/// <summary>
/// 纯壳：无业务逻辑，只 InitializeComponent。
/// DataContext 由 App.OnStartup 注入，窗口不 new ViewModel。
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();
}