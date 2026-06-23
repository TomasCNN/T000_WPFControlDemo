# 005011001_WPF StatusBar 状态栏官方类定义深度解析 + 工业场景实战

`StatusBar` 是 WPF 框架内置的标准状态栏控件，通常部署在窗口底部，用于展示应用运行状态、系统参数、进度信息、提示消息等，是工业上位机、MES、SCADA 等软件的标配 UI 元素。它继承自 `ItemsControl`，天然支持项集合管理、数据绑定、自定义模板，配合 `StatusBarItem` 容器可实现灵活的多分栏布局。

------

## 一、类的整体定位与继承体系

### 1. 基本信息

- **命名空间**：`System.Windows.Controls`
- **程序集**：`PresentationFramework.dll`
- **核心设计**：基于项控件模型，默认使用 `StatusBarPanel`（DockPanel 子类）作为布局面板，支持左右停靠、剩余空间自动填充，每个子项自动包装为 `StatusBarItem` 容器。

### 2. 完整继承链与能力分层

plaintext:

```tex
StatusBar → ItemsControl → Control → FrameworkElement → UIElement → Visual → DependencyObject
```

| 基类               | 注入的核心能力                                               |
| :----------------- | :----------------------------------------------------------- |
| `DependencyObject` | 依赖属性系统，支持绑定、样式、动画                           |
| `Visual`           | 视觉渲染、命中测试、视觉树管理                               |
| `UIElement`        | 路由事件、输入处理、布局参与                                 |
| `FrameworkElement` | 样式、数据上下文、资源查找、布局对齐                         |
| `Control`          | 控件模板、字体 / 背景 / 边框等通用外观属性                   |
| `ItemsControl`     | 项集合管理、数据绑定、容器自动生成、UI 虚拟化                |
| `StatusBar`        | 专属扩展：状态栏默认样式、`StatusBarItem` 容器、停靠式分栏布局 |

------

## 二、官方类定义逐成员深度解析

### 2.1 StatusBar 主类官方定义

以下是 `StatusBar` 的元数据签名，对应 WPF 官方源码的公开接口：

csharp:

```c#
public class StatusBar : ItemsControl
{
    // 构造函数
    public StatusBar();

    // 项容器生成核心重写（继承自 ItemsControl）
    protected override DependencyObject GetContainerForItemOverride();
    protected override bool IsItemItsOwnContainerOverride(object item);
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
    
    // 无障碍自动化
    protected override AutomationPeer OnCreateAutomationPeer();
}
```

#### 逐成员深度解析

##### 1. 构造函数 `public StatusBar()`

- 实例化控件时执行初始化：
  1. 重写默认样式键，关联系统内置的状态栏主题样式；
  2. 设置默认 `ItemsPanel` 为 `StatusBarPanel`（停靠式布局面板）；
  3. 初始化默认外观属性，如垂直对齐、字体大小等，适配状态栏紧凑显示场景。

##### 2. `GetContainerForItemOverride()`

- **作用**：ItemsControl 容器生成机制的核心重写，返回一个新的 `StatusBarItem` 实例作为每个数据项的 UI 容器。
- **设计意义**：实现「数据 → UI 容器」的自动映射，开发者添加数据项时无需手动创建 `StatusBarItem`。

##### 3. `IsItemItsOwnContainerOverride(object item)`

- **作用**：判断子元素本身是否已经是 `StatusBarItem` 类型。
- **优化逻辑**：如果 XAML 中直接写了 `<StatusBarItem>`，则跳过容器包装步骤，直接使用该元素，提升渲染性能。

##### 4. `PrepareContainerForItemOverride(DependencyObject element, object item)`

- **作用**：容器生成后执行初始化，将数据上下文、样式、数据模板应用到 `StatusBarItem` 上。
- **扩展点**：子类可重写此方法，在项生成时注入自定义逻辑（如权限控制、状态染色）。

##### 5. `OnCreateAutomationPeer()`

- **作用**：创建 `StatusBarAutomationPeer` 自动化对等类，支持屏幕阅读器、UI 自动化测试，符合 Windows 无障碍规范。

#### 核心继承属性（高频使用）

`StatusBar` 本身的专属公开属性很少，绝大多数能力来自继承，以下是开发中最常用的属性：

| 属性                              | 来源           | 作用                                                        |
| :-------------------------------- | :------------- | :---------------------------------------------------------- |
| `ItemsSource`                     | `ItemsControl` | 绑定数据源，动态生成状态栏项，MVVM 模式核心                 |
| `ItemTemplate`                    | `ItemsControl` | 数据模板，定义每个数据项的 UI 外观                          |
| `ItemContainerStyle`              | `ItemsControl` | 自定义 `StatusBarItem` 容器的样式                           |
| `ItemsPanel`                      | `ItemsControl` | 布局面板，默认 `StatusBarPanel`，可替换为 Grid 等自定义布局 |
| `Background` / `Foreground`       | `Control`      | 状态栏背景、文字颜色                                        |
| `BorderBrush` / `BorderThickness` | `Control`      | 边框样式                                                    |
| `FontSize` / `FontFamily`         | `Control`      | 全局字体设置                                                |

------

### 2.2 StatusBarItem 项容器定义

`StatusBarItem` 是状态栏每一栏的容器，位于 `System.Windows.Controls.Primitives` 命名空间，继承自 `ContentControl`：

csharp:

```c#
namespace System.Windows.Controls.Primitives
{
    public class StatusBarItem : ContentControl
    {
        public StatusBarItem();
        
        protected override AutomationPeer OnCreateAutomationPeer();
    }
}
```

#### 核心特性

1. **单内容容器**：继承自 `ContentControl`，可承载任意 UI 元素（文本、图标、进度条、按钮等），扩展性极强。
2. **停靠布局支持**：配合父级 `StatusBarPanel`（DockPanel 子类），可通过 `DockPanel.Dock` 附加属性控制停靠位置（左 / 右）。
3. **紧凑样式**：默认样式针对状态栏做了内边距、对齐优化，适合小空间密集信息展示。

------

### 2.3 StatusBarPanel 布局面板

`StatusBarPanel` 是状态栏的默认布局面板，位于 `System.Windows.Controls.Primitives` 命名空间，继承自 `DockPanel`，是状态栏分栏布局的核心：

csharp:

```c#
public class StatusBarPanel : DockPanel
{
    public StatusBarPanel();
}
```

#### 布局规则

1. **默认停靠**：子元素默认 `Dock.Left`，从左到右依次排列；
2. **尾部填充**：最后一个子元素默认填充剩余所有空间（DockPanel 标准行为）；
3. **自定义停靠**：可通过 `DockPanel.Dock="Right"` 将项停靠到右侧，实现左右分栏；
4. **自动紧凑**：子元素按内容自适应宽度，避免空间浪费。

------

## 三、核心功能与设计机制

### 1. 项容器自动生成机制

作为 `ItemsControl` 的子类，StatusBar 遵循 WPF 标准项控件逻辑：

- 直接在 XAML 中添加子元素 → 自动包装为 `StatusBarItem`；
- 绑定 `ItemsSource` 集合 → 为每个数据项自动生成 `StatusBarItem` 容器；
- 通过 `ItemTemplate` 统一控制数据项的渲染外观。

### 2. 多分栏停靠布局机制

基于 `StatusBarPanel`（DockPanel）的停靠特性，无需手动写 Grid 列宽，即可快速实现「左侧状态 + 右侧信息」的经典状态栏布局，是工业软件最常用的布局方式。

### 3. 内容无限扩展能力

得益于 `StatusBarItem` 的 `ContentControl` 特性，状态栏不仅能放文本，还可嵌入：

- 状态指示灯（边框 + 填充颜色）
- 进度条（`ProgressBar`）
- 图标按钮（`Button` + 图标）
- 下拉菜单
- 实时时间、计数等动态数据

### 4. 系统主题适配

默认样式跟随 Windows 系统主题自动切换，支持浅色 / 深色模式；也可通过重写模板完全自定义外观，适配工业深色护眼主题。

------

## 四、标准使用方法

### 4.1 基础静态状态栏

最简用法，直接添加文本项，默认从左到右排列。

xaml:

```xaml
<Window ...>
    <DockPanel>
        <!-- 状态栏停靠到底部 -->
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="系统就绪"/>
            <StatusBarItem Content="当前用户：Admin"/>
            <StatusBarItem Content="版本：V1.2.0"/>
        </StatusBar>

        <!-- 主内容区域 -->
        <Grid Background="#f5f5f5"/>
    </DockPanel>
</Window>
```

### 4.2 左右分栏布局

通过 `DockPanel.Dock` 附加属性实现左右分布，是最经典的状态栏布局。

xaml:

```xaml
<StatusBar DockPanel.Dock="Bottom">
    <!-- 左侧区域 -->
    <StatusBarItem Content="通讯状态：正常"/>
    <StatusBarItem Content="工单：WO-202606001"/>

    <!-- 右侧区域 -->
    <StatusBarItem DockPanel.Dock="Right" Content="2026-06-18 14:30:00"/>
    <StatusBarItem DockPanel.Dock="Right" Content="操作员：张三"/>
</StatusBar>
```

> 注意：右侧项需要按「从右往左」的顺序写，先写的靠最右。

### 4.3 嵌入复杂控件

`StatusBarItem` 内可放置任意控件，实现图标 + 文字、进度条、按钮等复杂效果：

xaml:

```xaml
<StatusBar DockPanel.Dock="Bottom">
    <!-- 带指示灯的状态 -->
    <StatusBarItem>
        <StackPanel Orientation="Horizontal" Margin="0,0,12,0">
            <Ellipse Width="8" Height="8" Fill="Green" VerticalAlignment="Center" Margin="0,0,6,0"/>
            <TextBlock Text="PLC 通讯正常"/>
        </StackPanel>
    </StatusBarItem>

    <!-- 进度条 -->
    <StatusBarItem Width="180" Margin="0,0,12,0">
        <ProgressBar Value="65" Height="12" Minimum="0" Maximum="100"/>
    </StatusBarItem>

    <!-- 右侧报警计数 -->
    <StatusBarItem DockPanel.Dock="Right">
        <Button Content="报警：3" Background="Transparent" BorderThickness="0" Foreground="Red"/>
    </StatusBarItem>

    <!-- 右侧时间 -->
    <StatusBarItem DockPanel.Dock="Right" Content="2026-06-18 14:30:00" Margin="12,0,0,0"/>
</StatusBar>
```

### 4.4 MVVM 动态绑定

通过 `ItemsSource` 绑定集合，适合状态栏项不固定、需动态增减的场景。

#### ViewModel 定义

csharp:

```c#
public class StatusItemModel
{
    public string Text { get; set; }
    public Brush StateColor { get; set; }
    public bool IsRight { get; set; }
}

public class MainViewModel
{
    public ObservableCollection<StatusItemModel> StatusItems { get; set; } = new();

    public MainViewModel()
    {
        StatusItems.Add(new StatusItemModel { Text = "PLC 正常", StateColor = Brushes.Green });
        StatusItems.Add(new StatusItemModel { Text = "工单：WO-001", StateColor = Brushes.White });
        StatusItems.Add(new StatusItemModel { Text = "Admin", IsRight = true, StateColor = Brushes.White });
    }
}
```

#### XAML 绑定

xaml:

```xaml
<StatusBar ItemsSource="{Binding StatusItems}">
    <StatusBar.ItemContainerStyle>
        <Style TargetType="StatusBarItem">
            <Style.Triggers>
                <DataTrigger Binding="{Binding IsRight}" Value="True">
                    <Setter Property="DockPanel.Dock" Value="Right"/>
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </StatusBar.ItemContainerStyle>
    <StatusBar.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal">
                <Ellipse Width="8" Height="8" Fill="{Binding StateColor}" VerticalAlignment="Center" Margin="0,0,6,0"/>
                <TextBlock Text="{Binding Text}"/>
            </StackPanel>
        </DataTemplate>
    </StatusBar.ItemTemplate>
</StatusBar>
```

### 4.5 自定义 Grid 布局面板

如果需要精确的列宽控制（如左中右三等分），可替换默认的 `StatusBarPanel` 为 `Grid`：

xaml:

```xaml
<StatusBar>
    <StatusBar.ItemsPanel>
        <ItemsPanelTemplate>
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
            </Grid>
        </ItemsPanelTemplate>
    </StatusBar.ItemsPanel>

    <StatusBarItem Grid.Column="0" Content="左侧信息" HorizontalAlignment="Left"/>
    <StatusBarItem Grid.Column="1" Content="中间提示" HorizontalAlignment="Center"/>
    <StatusBarItem Grid.Column="2" Content="右侧时间" HorizontalAlignment="Right"/>
</StatusBar>
```

------

## 五、工业场景完整实战实例

### 场景：SCADA 上位机底部状态栏

工业软件标准状态栏，包含：

- **左侧**：PLC 通讯状态指示灯、当前工单编号、设备运行状态
- **中间**：生产进度条 + 进度百分比
- **右侧**：报警数量、登录用户、实时系统时间、软件版本
- 深色工业护眼主题，适配车间长时间作业

### 1. ViewModel 实现

csharp:

```c#
using System;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Media;
using WpfIndustrialDemo.Common;

namespace WpfIndustrialDemo.ViewModels
{
    public class IndustrialStatusBarViewModel : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _timeTimer;
        private double _productionProgress;
        private int _alarmCount;
        private bool _isPlcConnected;

        // 通讯状态
        public bool IsPlcConnected
        {
            get => _isPlcConnected;
            set { _isPlcConnected = value; OnPropertyChanged(nameof(IsPlcConnected)); OnPropertyChanged(nameof(PlcStatusColor)); }
        }

        public Brush PlcStatusColor => IsPlcConnected ? Brushes.LimeGreen : Brushes.Red;
        public string PlcStatusText => IsPlcConnected ? "PLC 通讯正常" : "PLC 通讯中断";

        // 当前工单
        public string CurrentWorkOrder { get; set; } = "WO-20260618-001";

        // 生产进度
        public double ProductionProgress
        {
            get => _productionProgress;
            set { _productionProgress = value; OnPropertyChanged(nameof(ProductionProgress)); OnPropertyChanged(nameof(ProgressText)); }
        }
        public string ProgressText => $"生产进度：{ProductionProgress:F0}%";

        // 报警数量
        public int AlarmCount
        {
            get => _alarmCount;
            set { _alarmCount = value; OnPropertyChanged(nameof(AlarmCount)); OnPropertyChanged(nameof(AlarmForeground)); }
        }
        public Brush AlarmForeground => AlarmCount > 0 ? Brushes.Red : Brushes.LightGray;

        // 登录用户
        public string LoginUser { get; set; } = "操作员：张三";

        // 系统时间
        private string _systemTime;
        public string SystemTime
        {
            get => _systemTime;
            set { _systemTime = value; OnPropertyChanged(nameof(SystemTime)); }
        }

        // 版本号
        public string AppVersion { get; set; } = "V1.2.3 Build 20260618";

        public IndustrialStatusBarViewModel()
        {
            // 初始化模拟数据
            IsPlcConnected = true;
            ProductionProgress = 68.5;
            AlarmCount = 2;

            // 实时时间定时器
            _timeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timeTimer.Tick += (s, e) => SystemTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _timeTimer.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

### 2. 工业风样式 + XAML 界面

xaml:

```xaml
<Window x:Class="WpfIndustrialDemo.StatusBarWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:WpfIndustrialDemo.ViewModels"
        Title="工业上位机监控系统" Height="600" Width="1000" Background="#1e1e1e">
    <Window.DataContext>
        <vm:IndustrialStatusBarViewModel/>
    </Window.DataContext>

    <Window.Resources>
        <!-- 工业风 StatusBar 全局样式 -->
        <Style TargetType="StatusBar">
            <Setter Property="Background" Value="#252526"/>
            <Setter Property="Foreground" Value="#d4d4d4"/>
            <Setter Property="BorderBrush" Value="#3e3e42"/>
            <Setter Property="BorderThickness" Value="0,1,0,0"/>
            <Setter Property="Padding" Value="8,4"/>
            <Setter Property="FontSize" Value="12"/>
        </Style>

        <Style TargetType="StatusBarItem">
            <Setter Property="Padding" Value="12,2"/>
            <Setter Property="VerticalAlignment" Value="Center"/>
        </Style>

        <!-- 垂直分隔线样式 -->
        <Style x:Key="StatusSeparator" TargetType="Separator">
            <Setter Property="Width" Value="1"/>
            <Setter Property="Height" Value="16"/>
            <Setter Property="Background" Value="#3e3e42"/>
            <Setter Property="Margin" Value="4,0"/>
        </Style>
    </Window.Resources>

    <DockPanel>
        <!-- 底部状态栏 -->
        <StatusBar DockPanel.Dock="Bottom">
            <!-- 左侧：PLC 通讯状态 -->
            <StatusBarItem>
                <StackPanel Orientation="Horizontal">
                    <Ellipse Width="10" Height="10" Fill="{Binding PlcStatusColor}" VerticalAlignment="Center" Margin="0,0,6,0">
                        <Ellipse.Style>
                            <Style TargetType="Ellipse">
                                <Style.Triggers>
                                    <DataTrigger Binding="{Binding IsPlcConnected}" Value="True">
                                        <DataTrigger.EnterActions>
                                            <BeginStoryboard>
                                                <Storyboard AutoReverse="True" RepeatBehavior="Forever">
                                                    <DoubleAnimation Storyboard.TargetProperty="Opacity" From="1" To="0.6" Duration="0:0:1.5"/>
                                                </Storyboard>
                                            </BeginStoryboard>
                                        </DataTrigger.EnterActions>
                                    </DataTrigger>
                                </Style.Triggers>
                            </Style>
                        </Ellipse.Style>
                    </Ellipse>
                    <TextBlock Text="{Binding PlcStatusText}" VerticalAlignment="Center"/>
                </StackPanel>
            </StatusBarItem>

            <Separator Style="{StaticResource StatusSeparator}"/>

            <!-- 左侧：当前工单 -->
            <StatusBarItem Content="{Binding CurrentWorkOrder, StringFormat=工单：{0}}"/>

            <Separator Style="{StaticResource StatusSeparator}"/>

            <!-- 中间：生产进度 -->
            <StatusBarItem Width="260">
                <Grid>
                    <ProgressBar Value="{Binding ProductionProgress}" Minimum="0" Maximum="100" Height="14" Background="#3c3c3c" Foreground="#007acc"/>
                    <TextBlock Text="{Binding ProgressText}" HorizontalAlignment="Center" VerticalAlignment="Center" FontSize="11" Foreground="White"/>
                </Grid>
            </StatusBarItem>

            <!-- 右侧：版本号 -->
            <StatusBarItem DockPanel.Dock="Right" Content="{Binding AppVersion}" Foreground="#888"/>

            <!-- 右侧：系统时间 -->
            <StatusBarItem DockPanel.Dock="Right" Content="{Binding SystemTime}"/>

            <Separator DockPanel.Dock="Right" Style="{StaticResource StatusSeparator}"/>

            <!-- 右侧：登录用户 -->
            <StatusBarItem DockPanel.Dock="Right" Content="{Binding LoginUser}"/>

            <Separator DockPanel.Dock="Right" Style="{StaticResource StatusSeparator}"/>

            <!-- 右侧：报警数量 -->
            <StatusBarItem DockPanel.Dock="Right">
                <TextBlock Text="{Binding AlarmCount, StringFormat=报警：{0} 条}" Foreground="{Binding AlarmForeground}" FontWeight="SemiBold"/>
            </StatusBarItem>
        </StatusBar>

        <!-- 主内容区域（模拟组态画面） -->
        <Grid Background="#1e1e1e">
            <TextBlock Text="主监控画面区域" Foreground="#666" FontSize="18" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Grid>
    </DockPanel>
</Window>
```

### 实例亮点

1. **呼吸灯效果**：通讯正常时指示灯呼吸闪烁，直观展示通讯心跳状态；
2. **进度条文字叠加**：进度条上叠加百分比文字，信息密度更高；
3. **状态联动染色**：报警数 > 0 自动变红，通讯中断自动变红，符合工业视觉规范；
4. **实时时间更新**：每秒自动刷新系统时间，无需后台手动操作 UI；
5. **深色护眼主题**：低对比度深灰底色，降低车间长时间作业的视觉疲劳。

------

## 六、常见坑点与最佳实践

1. **右侧项顺序颠倒**：`DockPanel.Dock="Right"` 的项，先写的靠最右，书写顺序与视觉顺序相反。
2. **最后一项填充问题**：默认最后一个左侧项会占满剩余空间，若不想拉伸，可在最后加一个空的 `StatusBarItem` 占位。
3. **动态更新不触发**：时间、计数等动态数据，数据源必须实现 `INotifyPropertyChanged`，否则界面不刷新。
4. **工业场景高度建议**：状态栏高度控制在 24~28px，字体 12px，信息紧凑不占用过多主界面空间。
5. **性能优化**：状态栏不要放过多复杂控件，避免频繁刷新导致主界面卡顿。