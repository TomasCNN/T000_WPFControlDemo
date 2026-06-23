# 005011004_工业场景下 WPF `StatusBar` 实战用法全指南

在工业上位机、产线监控、设备管理系统中，`StatusBar` 是窗口底部的**常驻全局信息面板**，核心价值是让操作人员扫一眼就能掌握系统整体运行状态，无需切换页面。它基于 `ItemsControl` 体系，默认搭载 `StatusBarPanel` 横向布局面板，天然支持左右分段停靠，非常适合承载「运行状态、通讯状态、报警计数、生产信息、用户身份、系统时间」这类轻量、高频的全局信息。

工业场景使用 StatusBar 的核心设计原则：**信息精简、状态直观、重点突出、交互克制**—— 底部栏是信息区，不是操作区，只放最核心的全局状态，避免堆砌冗余信息干扰操作人员。

------

## 一、典型用法 1：标准工业分段式状态栏（最常用）

### 场景说明

产线监控系统的底部状态栏，左侧展示系统运行状态、PLC 通讯状态、设备在线数、未处理报警；右侧展示当前用户、运行模式、系统时间。不同类别信息用垂直分隔线划分，关键异常状态用颜色高亮，是工业软件最经典的状态栏布局。

### 核心特性

- 利用 `StatusBarPanel.Dock` 附加属性实现左右分段停靠
- 状态指示灯（圆形色块）替代纯文字，辨识度更高
- 报警、异常信息红色高亮，第一时间吸引注意力
- 垂直分隔线分组，信息结构清晰

### 完整实现代码

xaml:

```xaml
<Window x:Class="IndustrialDemo.MainWindow"
        Title="产线监控系统" Height="600" Width="1000">
    <DockPanel>
        <!-- 主内容区 -->
        <Grid Background="#F0F2F5">
            <TextBlock Text="生产监控主界面" HorizontalAlignment="Center" VerticalAlignment="Center" Foreground="#999"/>
        </Grid>

        <!-- 底部状态栏 -->
        <StatusBar DockPanel.Dock="Bottom"
                   Height="28"
                   Background="#F8F9FA"
                   BorderBrush="#E0E0E0"
                   BorderThickness="0 1 0 0"
                   FontSize="12"
                   Foreground="#333">

            <!-- ==================== 左侧状态区 ==================== -->
            <!-- 1. 系统运行状态 -->
            <StatusBarItem>
                <StackPanel Orientation="Horizontal">
                    <Ellipse Width="8" Height="8" Fill="#52C41A" VerticalAlignment="Center" Margin="0 0 6 0"/>
                    <TextBlock Text="系统运行正常"/>
                </StackPanel>
            </StatusBarItem>

            <Separator/>

            <!-- 2. PLC通讯状态 -->
            <StatusBarItem>
                <StackPanel Orientation="Horizontal">
                    <Ellipse Width="8" Height="8" Fill="#52C41A" VerticalAlignment="Center" Margin="0 0 6 0"/>
                    <TextBlock Text="PLC通讯正常"/>
                </StackPanel>
            </StatusBarItem>

            <Separator/>

            <!-- 3. 设备在线数 -->
            <StatusBarItem>
                <TextBlock Text="设备在线：12 / 15"/>
            </StatusBarItem>

            <Separator/>

            <!-- 4. 未处理报警（异常红色高亮） -->
            <StatusBarItem>
                <TextBlock Text="未处理报警：3" Foreground="#F5222D" FontWeight="Bold"/>
            </StatusBarItem>

            <Separator/>

            <!-- 5. 当前工单 -->
            <StatusBarItem>
                <TextBlock Text="当前工单：WO20240618001"/>
            </StatusBarItem>

            <!-- ==================== 右侧信息区 ==================== -->
            <!-- 系统时间 -->
            <StatusBarItem StatusBarPanel.Dock="Right" Text="{Binding CurrentTime, StringFormat=系统时间：{0}}"/>

            <Separator StatusBarPanel.Dock="Right"/>

            <!-- 运行模式 -->
            <StatusBarItem StatusBarPanel.Dock="Right">
                <TextBlock Text="运行模式：自动" Foreground="#1890FF"/>
            </StatusBarItem>

            <Separator StatusBarPanel.Dock="Right"/>

            <!-- 当前用户 -->
            <StatusBarItem StatusBarPanel.Dock="Right" Text="当前用户：张工(管理员)"/>
        </StatusBar>
    </DockPanel>
</Window>
```

### 配套系统时间更新逻辑

工业场景要求状态栏时间实时刷新，使用 `DispatcherTimer` 每秒更新：

csharp:

```c#
public partial class MainWindow : Window
{
    public string CurrentTime { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        // 每秒更新系统时间
        var timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += (s, e) =>
        {
            CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            // 通知界面更新
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTime)));
        };
        timer.Start();
    }

    public event PropertyChangedEventHandler PropertyChanged;
}
```

### 要点解析

1. **左右停靠规则**：设置 `StatusBarPanel.Dock="Right"` 的条目会自动贴靠右侧，且**从右往左依次排列**—— 先写的条目在最右边，后写的在左边，编写时注意顺序。
2. **状态可视化**：用绿 / 黄 / 红三色圆形指示灯替代纯文字，操作人员远距离就能快速识别状态，符合工控操作习惯。
3. **异常高亮**：报警、故障、离线等异常信息用红色 / 橙色加粗显示，优先级远高于普通信息，确保第一时间被发现。
4. **信息分组**：不同类别信息之间用 `Separator` 垂直分隔线隔开，避免信息密集导致阅读困难。

------

## 二、典型用法 2：MVVM 数据驱动动态状态栏

### 场景说明

设备数量多、状态项动态变化的场景，比如全厂设备监控系统，状态栏展示各车间的运行状态、报警数，状态项由 ViewModel 动态生成，支持新增、删除、状态变更，完全遵循 MVVM 架构。

### 核心特性

- `ItemsSource` 绑定状态集合，纯数据驱动
- `ItemTemplate` 自定义状态项外观（图标 + 文本）
- 状态变更自动同步 UI，无需操作控件

### 1. 状态项数据模型

csharp:

```c#
public class StatusItem : INotifyPropertyChanged
{
    private string _statusText;
    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    private string _statusColor;
    public string StatusColor
    {
        get => _statusColor;
        set { _statusColor = value; OnPropertyChanged(); }
    }

    private int _order;
    public int Order // 排序号，控制显示顺序
    {
        get => _order;
        set { _order = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### 2. ViewModel 定义

csharp:

```c#
public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<StatusItem> LeftStatusItems { get; set; }
    public ObservableCollection<StatusItem> RightStatusItems { get; set; }

    public MainViewModel()
    {
        // 左侧动态状态项
        LeftStatusItems = new ObservableCollection<StatusItem>
        {
            new() { StatusText = "系统运行正常", StatusColor = "#52C41A", Order = 1 },
            new() { StatusText = "PLC通讯正常", StatusColor = "#52C41A", Order = 2 },
            new() { StatusText = "未处理报警：3", StatusColor = "#F5222D", Order = 3 }
        };

        // 右侧动态状态项
        RightStatusItems = new ObservableCollection<StatusItem>
        {
            new() { StatusText = "管理员", StatusColor = "#333", Order = 1 },
            new() { StatusText = DateTime.Now.ToString("HH:mm:ss"), StatusColor = "#333", Order = 2 }
        };
    }

    // 示例：报警数变化时动态更新
    public void UpdateAlarmCount(int count)
    {
        var alarmItem = LeftStatusItems.First(i => i.StatusText.Contains("报警"));
        alarmItem.StatusText = $"未处理报警：{count}";
        alarmItem.StatusColor = count > 0 ? "#F5222D"c : "#52C41A";
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### 3. XAML 绑定实现

xaml:

```xaml
<StatusBar DockPanel.Dock="Bottom" Height="28" Background="#F8F9FA">
    <!-- 左侧动态状态区 -->
    <ItemsControl ItemsSource="{Binding LeftStatusItems}">
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <StackPanel Orientation="Horizontal" Margin="12 0">
                    <Ellipse Width="8" Height="8" Fill="{Binding StatusColor}" VerticalAlignment="Center" Margin="0 0 6 0"/>
                    <TextBlock Text="{Binding StatusText}" VerticalAlignment="Center"/>
                </StackPanel>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>

    <!-- 右侧动态状态区 -->
    <ItemsControl StatusBarPanel.Dock="Right" ItemsSource="{Binding RightStatusItems}">
        <ItemsControl.ItemsPanel>
            <ItemsPanelTemplate>
                <StackPanel Orientation="Horizontal"/>
            </ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <TextBlock Text="{Binding StatusText}" Margin="12 0" VerticalAlignment="Center"/>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</StatusBar>
```

### 适用场景

- 状态项数量不固定，需要根据权限、场景动态增减；
- 状态值频繁变化，需要通过数据绑定自动更新；
- 严格遵循 MVVM 架构，禁止后台代码直接操作 UI。

------

## 三、典型用法 3：带交互与进度的状态栏

### 场景说明

工业软件常见的「数据导出、配方下载、固件升级」等耗时操作，在状态栏显示实时进度；同时支持点击报警数快速跳转到报警窗口，兼顾信息展示与轻量交互。

### 核心特性

- 内嵌 `ProgressBar` 显示任务进度
- 可点击文本跳转对应功能页面
- 操作完成后自动隐藏进度条

### 实现代码

xaml:

```xaml
<StatusBar DockPanel.Dock="Bottom" Height="28">
    <!-- 左侧状态 -->
    <StatusBarItem>
        <Ellipse Width="8" Height="8" Fill="#52C41A" VerticalAlignment="Center"/>
    </StatusBarItem>

    <!-- 进度条区域（默认隐藏，任务执行时显示） -->
    <StatusBarItem x:Name="ProgressItem" Visibility="Collapsed">
        <StackPanel Orientation="Horizontal">
            <TextBlock Text="配方下载中：" VerticalAlignment="Center" Margin="0 0 8 0"/>
            <ProgressBar Width="120" Height="16" Value="{Binding DownloadProgress}" VerticalAlignment="Center"/>
            <TextBlock Text="{Binding DownloadProgress, StringFormat={}{0}%}" VerticalAlignment="Center" Margin="8 0 0 0"/>
        </StackPanel>
    </StatusBarItem>

    <!-- 右侧可点击报警数 -->
    <StatusBarItem StatusBarPanel.Dock="Right" Cursor="Hand">
        <TextBlock Text="未处理报警：3" Foreground="#F5222D"
                   MouseLeftButtonUp="AlarmText_Click"/>
    </StatusBarItem>

    <Separator StatusBarPanel.Dock="Right"/>

    <StatusBarItem StatusBarPanel.Dock="Right" Text="{Binding CurrentTime}"/>
</StatusBar>
```

### 后台交互逻辑

csharp:

```c#
// 点击报警数跳转到报警窗口
private void AlarmText_Click(object sender, MouseButtonEventArgs e)
{
    var alarmWindow = new AlarmListWindow();
    alarmWindow.Show();
}

// 模拟下载进度更新
private void StartDownload()
{
    ProgressItem.Visibility = Visibility.Visible;
    
    Task.Run(() =>
    {
        for (int i = 0; i <= 100; i++)
        {
            Thread.Sleep(50);
            Dispatcher.Invoke(() => DownloadProgress = i);
        }
        Dispatcher.Invoke(() => ProgressItem.Visibility = Visibility.Collapsed);
    });
}
```

### 工业场景设计建议

1. **进度条只放全局任务**：只有影响全系统的耗时操作（如全线配方下发、全量数据导出）才放在状态栏，单设备操作不要占用全局状态栏；
2. **交互要克制**：状态栏只放 1-2 个高频点击入口，不要堆砌按钮，底部栏核心是信息展示，不是操作区；
3. **操作完成自动隐藏**：进度条、提示信息在任务结束后自动消失，避免长期占用空间。

------

## 四、典型用法 4：深色工控主题状态栏适配

工业现场很多上位机使用深色主题，降低视觉疲劳、减少屏幕反光，需要对 StatusBar 做深色样式适配。

### 深色主题样式示例

xaml:

```xaml
<Window.Resources>
    <Style TargetType="StatusBar">
        <Setter Property="Height" Value="28"/>
        <Setter Property="Background" Value="#1F1F1F"/>
        <Setter Property="Foreground" Value="#E0E0E0"/>
        <Setter Property="BorderBrush" Value="#333"/>
        <Setter Property="BorderThickness" Value="0 1 0 0"/>
        <Setter Property="FontSize" Value="12"/>
    </Style>

    <Style TargetType="StatusBarItem">
        <Setter Property="Padding" Value="12 0"/>
        <Setter Property="VerticalContentAlignment" Value="Center"/>
    </Style>

    <Style TargetType="Separator">
        <Setter Property="Background" Value="#333"/>
        <Setter Property="Width" Value="1"/>
        <Setter Property="Margin" Value="0 4"/>
    </Style>
</Window.Resources>
```

### 适配要点

- 背景用深灰（#1F1F1F）而非纯黑，避免强光对比刺眼；
- 文字用浅灰（#E0E0E0）而非纯白，降低视觉疲劳；
- 分隔线用深灰色，弱化边界感；
- 状态指示灯亮度适当降低，避免过于刺眼。

------

## 五、工业场景最佳实践

### 1. 状态优先，颜色说话

核心状态（运行、报警、离线）必须用颜色可视化，不要只放纯文字。工业规范：

- 绿色：正常、运行、在线
- 黄色：警告、待机、手动模式
- 红色：报警、故障、离线
- 灰色：未启用、断开

### 2. 信息分级，重点突出

- 第一优先级：报警、故障、异常，红色加粗，最醒目；
- 第二优先级：运行状态、通讯状态，常规显示；
- 第三优先级：用户、时间、版本号，靠右放置，弱化显示。

### 3. 数量克制，避免堆砌

状态栏只放**全局级别的核心信息**，建议条目控制在 5-8 个以内，不要把所有状态都塞到底部栏，导致信息过载，操作人员反而找不到重点。

### 4. 交互极简，点到为止

状态栏最多放 1-2 个可点击入口（如报警数跳转），不要放大量按钮。操作功能应该放在工具栏、右键菜单或主界面，底部栏以展示为主。

### 5. 实时刷新，数据准确

时间、计数、状态等动态数据要保证实时性，时间建议每秒刷新，状态变化要即时同步，避免状态栏信息滞后误导操作人员。

------

## 六、常见坑点与避坑指南

### 1. 右停靠条目顺序颠倒

- **现象**：多个设置了 `Dock="Right"` 的条目，显示顺序和 XAML 编写顺序相反；
- **原因**：右停靠条目是「从右往左」依次排列，先写的在最右侧，后写的在左侧；
- **解决**：靠右的条目按「从右到左」的顺序编写，或者反向绑定集合。

### 2. 内容不垂直居中

- **现象**：自定义的图标、文本偏上或偏下；
- **解决**：给 `StatusBarItem` 设置 `VerticalContentAlignment="Center"`，内部元素设置 `VerticalAlignment="Center"`。

### 3. 背景色设置不生效

- **现象**：修改 `Background` 后，还是显示默认灰色；
- **原因**：默认控件模板有内置背景，直接设置属性会被模板覆盖；
- **解决**：重写 `StatusBar` 的样式与控件模板，或通过 `Window.Resources` 全局覆盖默认样式。

### 4. 动态项过多导致拥挤

- **现象**：窗口变窄时，状态栏内容挤压重叠；
- **解决**：优先保证左侧核心状态可见，右侧次要信息可裁剪；重要系统建议设置窗口最小宽度，避免布局错乱。

------

## 总结

工业场景下的 `StatusBar` 不是简单的底部文字栏，而是系统全局状态的「仪表盘」。它的核心价值是用最少的空间、最直观的方式，让操作人员快速掌握系统整体健康度。用好左右分段布局、颜色状态指示、异常高亮三个核心手段，就能打造出符合工业操作习惯的专业状态栏。