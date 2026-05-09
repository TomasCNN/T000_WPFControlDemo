using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace T005_WPFCanvasDemo
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private Random _random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            InitTimer();
        }

        // 初始化定时器：模拟实时数据更新
        private void InitTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        // 定时器回调：更新温度、转速
        private void Timer_Tick(object sender, EventArgs e)
        {
            double temperature = 25 + _random.NextDouble() * 10;
            int speed = 1500 + _random.Next(-100, 100);

            txtTemperature.Text = $"{temperature:F1}℃";
            txtSpeed.Text = $"{speed} rpm";
        }

        // 启动按钮点击
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            ellipseStatus.Fill = Brushes.Green;
            MessageBox.Show("设备已启动");
        }

        // 停止按钮点击
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            ellipseStatus.Fill = Brushes.Red;
            MessageBox.Show("设备已停止");
        }

        // 复位按钮点击
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            txtTemperature.Text = "25.0℃";
            txtSpeed.Text = "1500 rpm";
            MessageBox.Show("设备已复位");
        }
    }
}
