using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace T007003_WPFLayoutDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private Random _random = new Random();


        public MainWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // 模拟实时数据更新
            TxtTemperature.Text = $"{25 + _random.NextDouble() * 10:F1}℃";
            TxtSpeed.Text = $"{1500 + _random.Next(-100, 100)} rpm";
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            RunStatusLight.Fill = Brushes.Green;
            MessageBox.Show("设备已启动");
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            RunStatusLight.Fill = Brushes.Red;
            MessageBox.Show("设备已停止");
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            RunStatusLight.Fill = Brushes.Gray;
            AlarmStatusLight.Fill = Brushes.Gray;
            TxtOutput.Text = "0 件";
            MessageBox.Show("设备已复位");
        }

        private void BtnAlarm_Click(object sender, RoutedEventArgs e)
        {
            AlarmStatusLight.Fill = Brushes.Red;
            MessageBox.Show("模拟报警触发");
        }
    }
}
