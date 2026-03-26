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

namespace T002_Application的生命周期
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.SourceInitialized += (s, e) => Console.WriteLine("1.MainWindow的SourceInitialized(创建窗体源)被执行");

            this.Activated += (s, e) => Console.WriteLine("2.MainWindow的Activated（设置当前窗体成为前台窗体）被执行");

            this.Loaded += (s, e) => Console.WriteLine("3.MainWindow的Loaded（当前窗体内部所有元素完成布局和呈现）被执行");

            this.ContentRendered += (s, e) => Console.WriteLine("4.MainWindow的ContentRendered（当前窗体的内容呈现之后）被执行");

            this.Deactivated += (s, e) => Console.WriteLine("5.MainWindow的Deactivated（当前窗体成为后台窗体）被执行");

            this.Closing += (s, e) => Console.WriteLine("6.MainWindow的Closing（当前窗体关闭之前）被执行");

            this.Closed += (s, e) => Console.WriteLine("7.MainWindow的Closed（当前窗体关闭之后）被执行");

            this.Unloaded += (s, e) => Console.WriteLine("8.MainWindow的Unloaded（当前窗体从元素树中删除）被执行");
        }
    }
}
