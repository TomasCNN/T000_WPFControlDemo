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

namespace T007002_WPFLayoutDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public class DeviceInfo
        {
            public string DeviceId { get; set; }
            public string DeviceName { get; set; }
            public SolidColorBrush StatusColor { get; set; }
            public string RunTime { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            LoadDeviceData();
        }

        private void LoadDeviceData()
        {
            var devices = new List<DeviceInfo>
            {
                new DeviceInfo { DeviceId = "CNC_001", DeviceName = "数控车床1号", StatusColor = Brushes.Green, RunTime = "123小时" },
                new DeviceInfo { DeviceId = "AOI_002", DeviceName = "光学检测2号", StatusColor = Brushes.Red, RunTime = "89小时" },
                new DeviceInfo { DeviceId = "CNC_003", DeviceName = "数控车床3号", StatusColor = Brushes.Yellow, RunTime = "256小时" }
            };
            DeviceListView.ItemsSource = devices;
        }
    }
}
