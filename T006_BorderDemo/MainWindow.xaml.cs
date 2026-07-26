using System.Windows;
using System.Windows.Media;

namespace T006_BorderDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // 启动：绿色状态
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            UpdateDeviceStatus(
                background: "#E8F5E9",
                borderBrush: "#4CAF50",
                statusColor: "#4CAF50",
                statusText: "状态：运行中");
        }

        // 停止：红色状态
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            UpdateDeviceStatus(
                background: "#FFEBEE",
                borderBrush: "#F44336",
                statusColor: "#F44336",
                statusText: "状态：已停止");
        }

        // 故障：黄色状态
        private void BtnFault_Click(object sender, RoutedEventArgs e)
        {
            UpdateDeviceStatus(
                background: "#FFF8E1",
                borderBrush: "#FFC107",
                statusColor: "#FFC107",
                statusText: "状态：故障");
        }

        // 统一更新状态的方法
        private void UpdateDeviceStatus(string background, string borderBrush, string statusColor, string statusText)
        {
            // 更新 Border 样式
            DeviceCardBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(background));
            DeviceCardBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(borderBrush));

            // 更新指示灯
            StatusEllipse.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(statusColor));

            // 更新状态文本
            StatusText.Text = statusText;
            StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(statusColor));
        }
    }
}
