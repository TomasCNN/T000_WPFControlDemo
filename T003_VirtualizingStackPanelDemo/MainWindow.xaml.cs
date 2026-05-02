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

namespace T003_VirtualizingStackPanelDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 生成 100000 条测试数据
            List<string> data = new List<string>();
            for (int i = 0; i < 100000; i++)
            {
                data.Add($"虚拟化列表项 {i}");
            }

            // 绑定数据
            listBox.ItemsSource = data;
        }
    }
}
