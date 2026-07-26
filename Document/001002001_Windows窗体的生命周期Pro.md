# 001002001_Windows窗体的生命周期

​	Window 窗体，其实也是一个控件，一个 Application 应用实例可能会有多个窗体，这些窗体随着用户的操作被创建于内存，最后被销毁于内存。大多数情况下，销毁的请求虽然由用户发起，但最终回收内存则是由 GC 垃圾回收器在干活儿。

​	我们以 HelloWorld 应用为例，打开 MainWindow 窗体的源代码，切换至后端代码，可以发现 MainWindow 继承于 Window 类。

```
namespace HelloWorld
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
    }
}
```

​	将鼠标的光标放至“Window”字符串上面，按下 F12，就可以导航到 Window 类的定义页面，我们会发现 Window 类又继承于 ContentControl 类，下面是 MainWindow 主窗体的整个继承路线。

​	MainWindow -> Window -> ContentControl -> Control -> FrameworkElement -> UIElement -> Visual -> DependencyObject -> DispatcherObject

​	在这里，我们并不打算将 MainWindow 的所有知识详尽，因为这必须要将它一路继承下来的所有父类都要交代清楚，我们只关注 MainWindow 的生命周期。所以我们在 MainWindow 的构造函数中写下如下代码：

```c#
namespace HelloWorld
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
 
            this.SourceInitialized += (s, e) => Console.WriteLine("1.MainWindow的SourceInitialized被执行");
 
            this.Activated += (s, e) => Console.WriteLine("2.MainWindow的Activated被执行");
 
            this.Loaded += (s, e) => Console.WriteLine("3.MainWindow的Loaded被执行");
 
            this.ContentRendered += (s, e) => Console.WriteLine("4.MainWindow的ContentRendered被执行");
 
            this.Deactivated += (s, e) => Console.WriteLine("5.MainWindow的Deactivated被执行");
 
            this.Closing += (s, e) => Console.WriteLine("6.MainWindow的Closing被执行");
 
            this.Closed += (s, e) => Console.WriteLine("7.MainWindow的Closed被执行");
 
            this.Unloaded += (s, e) => Console.WriteLine("8.MainWindow的Unloaded被执行");
 
        }
    }
}
```

然后我们直接F5调试，待主窗体显示后，直接关闭主窗体，观察输出（Ctrl+Alt+Q）结果。Application的生命周期和主窗体的生命周期是充满交织的，首先是Application的OnStartup，然后是主窗体的SourceInitialized，然后依次执行了Application的OnActivated和MainWindow的Activated，最后直到主窗体Closed，才轮到Application的OnExit。

```c#
1.OnStartup被触发
“HelloWorld.exe”(CLR v4.0.30319: HelloWorld.exe): 已加载“C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\PresentationFramework.Aero2\v4.0_4.0.0.0__31bf3856ad364e35\PresentationFramework.Aero2.dll”。已跳过加载符号。模块进行了优化，并且调试器选项“仅我的代码”已启用。
“HelloWorld.exe”(CLR v4.0.30319: HelloWorld.exe): 已加载“C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\PresentationCore.resources\v4.0_4.0.0.0_zh-Hans_31bf3856ad364e35\PresentationCore.resources.dll”。模块已生成，不包含符号。
1.MainWindow的SourceInitialized被执行
“HelloWorld.exe”(CLR v4.0.30319: HelloWorld.exe): 已加载“c:\program files\microsoft visual studio\2022\community\common7\ide\commonextensions\microsoft\xamldiagnostics\Framework\x86\Microsoft.VisualStudio.DesignTools.WpfTap.dll”。已跳过加载符号。模块进行了优化，并且调试器选项“仅我的代码”已启用。
“HelloWorld.exe”(CLR v4.0.30319: HelloWorld.exe): 已加载“C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\System.Runtime.Serialization\v4.0_4.0.0.0__b77a5c561934e089\System.Runtime.Serialization.dll”。已跳过加载符号。模块进行了优化，并且调试器选项“仅我的代码”已启用。
“HelloWorld.exe”(CLR v4.0.30319: HelloWorld.exe): 已加载“C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\SMDiagnostics\v4.0_4.0.0.0__b77a5c561934e089\SMDiagnostics.dll”。已跳过加载符号。模块进行了优化，并且调试器选项“仅我的代码”已启用。
“HelloWorld.exe”(CLR v4.0.30319: HelloWorld.exe): 已加载“C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\System.ServiceModel.Internals\v4.0_4.0.0.0__31bf3856ad364e35\System.ServiceModel.Internals.dll”。已跳过加载符号。模块进行了优化，并且调试器选项“仅我的代码”已启用。
2.OnActivated被触发
2.MainWindow的Activated被执行
“HelloWorld.exe”(CLR v4.0.30319: HelloWorld.exe): 已加载“C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\System.Runtime.Serialization.resources\v4.0_4.0.0.0_zh-Hans_b77a5c561934e089\System.Runtime.Serialization.resources.dll”。模块已生成，不包含符号。
“HelloWorld.exe”(CLR v4.0.30319: HelloWorld.exe): 已加载“C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\UIAutomationTypes\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationTypes.dll”。已跳过加载符号。模块进行了优化，并且调试器选项“仅我的代码”已启用。
“HelloWorld.exe”(CLR v4.0.30319: HelloWorld.exe): 已加载“C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\UIAutomationProvider\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationProvider.dll”。已跳过加载符号。模块进行了优化，并且调试器选项“仅我的代码”已启用。
3.MainWindow的Loaded被执行
4.MainWindow的ContentRendered被执行
5.MainWindow的Closing被执行
6.MainWindow的Deactivated被执行
3.OnDeactivated被触发
7.MainWindow的Closed被执行
4.OnExit被触发
程序“[4808] HelloWorld.exe”已退出，返回值为 0 (0x0)。
```

​	我们来单独看看主窗体的生命周期，在上述输出结果中寻找带“MainWindow”的字符串，可以发现如下的输出结果。

- 1.MainWindow的SourceInitialized被执行
- 2.MainWindow的Activated被执行
- 3.MainWindow的Loaded被执行
- 4.MainWindow的ContentRendered被执行
- 5.MainWindow的Closing被执行
- 6.MainWindow的Deactivated被执行
- 7.MainWindow的Closed被执行

​	观察这些输出结果，与我们订阅事件的代码顺序一致，唯独少了Unloaded的结果输出。因为Unloaded事件没有被触发。下面我们将分析一下这些事件分别代表什么含义。

| SourceInitialized | 创建窗体源时引发此事件                         |
| ----------------- | ---------------------------------------------- |
| Activated         | 当前窗体成为前台窗体时引发此事件               |
| Loaded            | 当前窗体内部所有元素完成布局和呈现时引发此事件 |
| ContentRendered   | 当前窗体的内容呈现之后引发此事件               |
| Closing           | 当前窗体关闭之前引发此事件                     |
| Deactivated       | 当前窗体成为后台窗体时引发此事件               |
| Closed            | 当前窗体关闭之后引发此事件                     |
| Unloaded          | 当前窗体从元素树中删除时引发此事件             |

​	由此我们可以得出结论，Window窗体的生命周期应如下图所示：

![img](https://i-blog.csdnimg.cn/img_convert/50afc73e0c26144cb2fc7c474b9e8321.jpeg)

结合上图，Window 窗体的生命周期可以划分为四个主要阶段：

1. **初始化阶段**：当窗体被创建时，首先触发 `SourceInitialized` 事件，标志着窗体源（窗口句柄）已建立。此时窗体尚未显示，适合进行一些底层的初始化配置。
2. **激活与加载阶段**：`SourceInitialized` 之后，窗体成为前台窗口时触发 `Activated` 事件；随后 WPF 布局系统完成所有子元素的测量与排列，触发 `Loaded` 事件；当窗体内容完全渲染到屏幕后，触发 `ContentRendered` 事件。`Loaded` 和 `ContentRendered` 之间是执行“首屏显示后的一次性操作”的最佳时机。
3. **运行交互阶段**：用户正常使用窗体期间，`Activated` 和 `Deactivated` 事件会在窗口切换时交替触发——用户切换到其他窗口时触发 `Deactivated`（变为后台窗体），再次切回时重新触发 `Activated`。这一对事件可重复发生多次。
4. **关闭阶段**：用户点击关闭按钮时，首先触发 `Closing` 事件（可在此取消关闭操作）；紧接着窗体变为后台触发 `Deactivated`；窗体真正关闭后触发 `Closed` 事件，适合做资源释放或保存工作。需要注意的是，`Unloaded` 事件在常规关闭流程中并不一定会触发——实验中未见输出正是因为主窗体关闭时并未从元素树中逻辑移除，而是直接销毁。

整体来看，事件执行顺序严格遵循 **SourceInitialized → Activated → Loaded → ContentRendered → …（运行与切换）… → Closing → Deactivated → Closed** 的时序链路。理解这一生命周期顺序后，开发者就能在合适的“钩子”中注入业务逻辑，例如在 `Loaded` 中初始化数据、在 `Closing` 中弹出保存提示、在 `Closed` 中释放非托管资源等。


窗体从元素树中删除时引发此事件             

​	在了解窗体的生命周期之后，我们就可以在它不同的生命周期处理一些不同的业务。例如在Application或Window的创建时加载一些本地设置，在窗体关闭或应用程序退出时保存一些本地设置。

下面我们通过一个更具体的实战示例，展示如何结合生命周期事件实现异步数据加载、关闭确认与资源清理：

```csharp
// 引入必要的命名空间
using System.Data.SqlClient;
using System.Windows;

namespace HelloWorld
{
    public partial class MainWindow : Window
    {
        // 数据库连接对象，使用字段保存以便在多个事件间共享
        private SqlConnection _connection;
        // 标记窗体中的数据是否被修改过（用于退出时判断是否需要保存）
        private bool _isDirty = false;

        public MainWindow()
        {
            InitializeComponent();
            // 注册各个生命周期事件的处理逻辑

            // 1. Loaded 事件：窗体布局完成后，异步加载初始数据
            this.Loaded += async (s, e) =>
            {
                // 在实际项目中，连接字符串建议写在配置文件中
                _connection = new SqlConnection("Data Source=.;Initial Catalog=MyDb;Integrated Security=True");
                // 异步打开数据库连接，避免阻塞UI线程
                await _connection.OpenAsync();

                // 模拟从数据库异步查询数据，并填充到界面控件（这里以 ListBox 为例）
                using (var cmd = new SqlCommand("SELECT Name, Price FROM Products", _connection))
                {
                    var reader = await cmd.ExecuteReaderAsync();
                    // 将 DataReader 中的数据转换为对象集合，并绑定给前端控件
                    var products = new List<Product>();
                    while (await reader.ReadAsync())
                    {
                        products.Add(new Product
                        {
                            Name = reader["Name"].ToString(),
                            Price = Convert.ToDecimal(reader["Price"])
                        });
                    }
                    ListBoxProducts.ItemsSource = products;
                }
            };

            // 2. Closing 事件：窗体关闭前，检查是否有未保存的更改，并给予用户选择
            this.Closing += (s, e) =>
            {
                if (_isDirty)
                {
                    // 弹出确认对话框，三个选项：是（保存并关闭）、否（不保存直接关闭）、取消（不关闭）
                    var result = MessageBox.Show("当前文档已修改，是否保存更改？", "保存确认",
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        // 执行实际的保存逻辑（例如将 _isDirty 标记的数据写回数据库）
                        SaveChanges();
                    }
                    else if (result == MessageBoxResult.Cancel)
                    {
                        // 取消关闭操作，窗体继续保持打开状态
                        e.Cancel = true;
                    }
                    // 若用户选择“否”，则直接关闭，不执行额外操作
                }
            };

            // 3. Closed 事件：窗体已经关闭，彻底释放非托管资源（如数据库连接、文件句柄等）
            this.Closed += (s, e) =>
            {
                if (_connection != null)
                {
                    // 释放数据库连接，Dispose 内部会关闭连接并释放相关资源
                    _connection.Dispose();
                    _connection = null;
                }
                // 如果有其他需要手动释放的资源（例如文件流、非托管句柄等），也应在这里统一处理
            };
        }

        // 保存更改的示例方法
        private void SaveChanges()
        {
            // 在这里编写将界面修改写回数据库的实际逻辑
            // 保存成功后，重置脏标记
            _isDirty = false;
        }
    }

    // 简单的数据模型，用于绑定商品信息
    public class Product
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
```

在上述代码中，`Loaded` 事件内使用 `async/await` 异步加载数据库数据，确保界面不会因为等待数据库操作而失去响应；`Closing` 事件通过 `e.Cancel = true` 实现了关闭前的拦截与二次确认；`Closed` 事件则在窗口销毁后彻底释放 `SqlConnection` 等非托管资源。这正好印证了我们前面所说的“在不同的生命周期处理不同业务”的设计思路。

接下来，我们再回过头仔细看看文章前面提到的那张生命周期示意图。图中以横向时间轴的方式串联了窗体从创建到销毁的完整事件序列：

- 示意图最左侧标出了 **初始化阶段**，这里只包含 `SourceInitialized` 一个事件。箭头从 `SourceInitialized` 引出，指向右侧的激活与加载阶段，对应着窗体从“底层的窗口句柄已建立”到“即将成为前台窗口”的过渡。

- 进入 **激活与加载阶段** 后，依次排列着 `Activated` → `Loaded` → `ContentRendered`，箭头从左向右依次连接。在图中，`Loaded` 和 `ContentRendered` 通常被框在一起，表示它们都属于“首屏呈现”的核心事件；而 `Activated` 虽然是激活事件，但也放在了该阶段的起始位置，强调“前台激活后才开始执行加载和渲染”。

- 示意图中间部分标明了 **运行交互阶段**。这一阶段里，`Activated` 和 `Deactivated` 之间画出了双向箭头或环形箭头，形象地表达了用户在窗体与后台之间反复切换时，这两个事件会交替触发的过程。图中可能会用一个虚线框将这一部分圈起来，并标注“可重复触发”，帮助读者理解在同一窗体生命周期中，`Deactivated` 和 `Activated` 可以出现多次，而不是仅仅执行一次。

- 示意图最右侧为 **关闭阶段**，事件顺序为 `Closing` → `Deactivated` → `Closed`，箭头依然从左向右递进。图中通常会把 `Closing` 放在最前面，因为这是关闭流程的入口；紧接着的 `Deactivated` 表示窗体失去焦点变成后台；最后才是 `Closed`，代表窗体已彻底销毁。这张图也同时说明了为什么在实验输出中 `Unloaded` 没有出现——因为它不在常规关闭路径的主序列中，属于当元素从逻辑树移除时才会触发的“特殊事件”，因此生命周期主示意图一般不会包含它。

通过对图中箭头和阶段分区的梳理，我们可以更直观地将前文的文字解释与这个示意图对应起来：每一条箭头都对应着一次“即将发生状态变化”的通知，而每一个阶段的边界，则帮我们明确了应该在何时处理哪些业务逻辑。

