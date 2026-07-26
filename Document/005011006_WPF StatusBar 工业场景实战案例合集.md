# 005011006_WPF StatusBar 工业场景实战案例合集

以下案例全部面向工业上位机、产线监控、设备管理等真实业务场景，覆盖从基础分段布局、进度反馈、动态数据绑定，到深色主题、报警闪烁、多工位聚合等高频需求，均可直接复用到项目中。

------

## 案例 1：标准工业分段式状态栏（最常用）

### 场景说明

产线监控系统底部标准状态栏，左侧承载**系统运行状态、PLC 通讯状态、设备在线数、未处理报警、当前工单**核心信息；右侧承载**运行模式、当前用户、系统时间、软件版本**辅助信息。是工业软件最经典的状态栏布局。

### 完整实现代码

xaml:

```xaml
<Window x:Class="StatusBarDemo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
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

            <!-- 左侧：系统运行状态 -->
            <StatusBarItem>
                <StackPanel Orientation="Horizontal">
                    <Ellipse Width="8" Height="8" Fill="#52C41A" VerticalAlignment="Center" Margin="0 0 6 0"/>
                    <TextBlock Text="系统运行正常"/>
                </StackPanel>
            </StatusBarItem>

            <Separator/>

            <!-- PLC通讯状态 -->
            <StatusBarItem>
                <StackPanel Orientation="Horizontal">
                    <Ellipse Width="8" Height="8" Fill="#52C41A" VerticalAlignment="Center" Margin="0 0 6 0"/>
                    <TextBlock Text="PLC通讯正常"/>
                </StackPanel>
            </StatusBarItem>

            <Separator/>

            <!-- 设备在线数 -->
            <StatusBarItem>
                <TextBlock Text="设备在线：12 / 15"/>
            </StatusBarItem>

            <Separator/>

            <!-- 未处理报警（异常高亮） -->
            <StatusBarItem>
                <TextBlock Text="未处理报警：3" Foreground="#F5222D" FontWeight="Bold"/>
            </StatusBarItem>

            <Separator/>

            <!-- 当前工单 -->
            <StatusBarItem>
                <TextBlock Text="工单：WO20240621001"/>
            </StatusBarItem>

            <!-- 右侧：软件版本 -->
            <StatusBarItem StatusBarPanel.Dock="Right" Text="版本：V2.1.3" Foreground="#999"/>

            <Separator StatusBarPanel.Dock="Right"/>

            <!-- 当前用户 -->
            <StatusBarItem StatusBarPanel.Dock="Right" Text="用户：张工(管理员)"/>

            <Separator StatusBarPanel.Dock="Right"/>

            <!-- 运行模式 -->
            <StatusBarItem StatusBarPanel.Dock="Right">
                <TextBlock Text="模式：自动" Foreground="#1890FF"/>
            </StatusBarItem>

            <Separator StatusBarPanel.Dock="Right"/>

            <!-- 系统时间 -->
            <StatusBarItem StatusBarPanel.Dock="Right">
                <TextBlock Text="{Binding CurrentTime}"/>
            </StatusBarItem>
        </StatusBar>
    </DockPanel>
</Window>
```

#### 配套后台代码（系统时间实时更新）

csharp:

```c#
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace StatusBarDemo
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _currentTime;
        public string CurrentTime
        {
            get => _currentTime;
            set { _currentTime = value; OnPropertyChanged(); }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // 每秒刷新系统时间
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            };
            timer.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
```

### 核心要点

1. **左右分段停靠**：通过 `StatusBarPanel.Dock="Right"` 实现右对齐，右侧条目按「从右往左」的顺序编写。
2. **状态可视化**：用绿 / 红圆形指示灯替代纯文字，操作人员远距离即可快速识别状态。
3. **信息分级**：报警信息红色加粗，优先级最高；版本号灰色弱化，优先级最低。
4. **分组清晰**：不同类别信息之间用 `Separator` 垂直分隔线区隔，避免信息密集。

------

## 案例 2：带任务进度的状态栏

### 场景说明

配方下发、固件升级、全量数据导出等全局耗时操作时，在状态栏显示实时进度与百分比，任务完成后自动隐藏。既不占用主界面空间，又能让用户全局感知任务进度。

### 完整实现代码

xaml:

```xaml
<StatusBar DockPanel.Dock="Bottom" Height="28" Background="#F8F9FA">
    <!-- 左侧基础状态 -->
    <StatusBarItem>
        <Ellipse Width="8" Height="8" Fill="#52C41A" VerticalAlignment="Center"/>
    </StatusBarItem>
    <Separator/>
    <StatusBarItem Content="设备在线：12/15"/>

    <!-- 进度条区域：默认隐藏，任务执行时显示 -->
    <StatusBarItem Visibility="{Binding TaskProgressVisible, Converter={StaticResource BoolToVis}}">
        <StackPanel Orientation="Horizontal">
            <TextBlock Text="{Binding TaskName}" VerticalAlignment="Center" Margin="0 0 10 0"/>
            <ProgressBar Width="150" Height="16" 
                         Value="{Binding TaskProgress}" 
                         VerticalAlignment="Center"/>
            <TextBlock Text="{Binding TaskProgress, StringFormat={}{0}%}" 
                       VerticalAlignment="Center" Margin="8 0 0 0"
                       Foreground="#666"/>
        </StackPanel>
    </StatusBarItem>

    <!-- 右侧时间 -->
    <StatusBarItem StatusBarPanel.Dock="Right" Text="{Binding CurrentTime}"/>
</StatusBar>
```

#### 配套 ViewModel 核心逻辑

csharp:

```c#
private bool _taskProgressVisible;
public bool TaskProgressVisible
{
    get => _taskProgressVisible;
    set { _taskProgressVisible = value; OnPropertyChanged(); }
}

private double _taskProgress;
public double TaskProgress
{
    get => _taskProgress;
    set { _taskProgress = value; OnPropertyChanged(); }
}

private string _taskName;
public string TaskName
{
    get => _taskName;
    set { _taskName = value; OnPropertyChanged(); }
}

// 模拟配方下发任务
public async void StartRecipeDownload()
{
    TaskName = "配方下发中";
    TaskProgressVisible = true;
    TaskProgress = 0;

    for (int i = 0; i <= 100; i++)
    {
        await Task.Delay(30);
        TaskProgress = i;
    }

    // 完成后延迟1秒隐藏
    await Task.Delay(1000);
    TaskProgressVisible = false;
}
```

### 核心要点

1. **全局任务才放状态栏**：仅全线配方下发、全量导出等影响全局的操作显示；单设备操作不要占用全局状态栏。
2. **自动隐藏**：任务完成后延迟隐藏，避免长期占用状态栏空间。
3. **进度 + 文本双展示**：同时显示进度条和百分比数字，兼顾直观性和精确性。

------

## 案例 3：MVVM 数据驱动动态状态栏

### 场景说明

多角色、多场景的系统中，状态栏条目根据权限、场景动态增减（如管理员可见运维状态，操作员不可见）。纯数据驱动，完全遵循 MVVM 架构，无需后台操作 UI。

### 完整实现代码

#### 1. 状态项数据模型

csharp:

```c#
public class StatusItem : INotifyPropertyChanged
{
    private string _text;
    public string Text
    {
        get => _text;
        set { _text = value; OnPropertyChanged(); }
    }

    private string _color;
    public string Color
    {
        get => _color;
        set { _color = value; OnPropertyChanged(); }
    }

    private int _order;
    public int Order
    {
        get => _order;
        set { _order = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

#### 2. ViewModel 定义

csharp:

```c#
public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<StatusItem> LeftStatusItems { get; set; }
    public ObservableCollection<StatusItem> RightStatusItems { get; set; }

    public MainViewModel(string userRole)
    {
        LeftStatusItems = new ObservableCollection<StatusItem>
        {
            new() { Text = "系统运行正常", Color = "#52C41A", Order = 1 },
            new() { Text = "PLC通讯正常", Color = "#52C41A", Order = 2 },
            new() { Text = "未处理报警：3", Color = "#F5222D", Order = 3 }
        };

        RightStatusItems = new ObservableCollection<StatusItem>
        {
            new() { Text = DateTime.Now.ToString("HH:mm:ss"), Color = "#333", Order = 1 },
            new() { Text = "当前用户：" + userRole, Color = "#333", Order = 2 }
        };

        // 管理员额外增加运维状态项
        if (userRole == "管理员")
        {
            LeftStatusItems.Add(new StatusItem 
            { 
                Text = "服务连接正常", 
                Color = "#52C41A", 
                Order = 4 
            });
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

#### 3. XAML 绑定

xaml:

```xaml
<StatusBar DockPanel.Dock="Bottom" Height="28" Background="#F8F9FA">
    <!-- 左侧动态状态区 -->
    <ItemsControl ItemsSource="{Binding LeftStatusItems}">
        <ItemsControl.ItemsPanel>
            <ItemsPanelTemplate>
                <StackPanel Orientation="Horizontal"/>
            </ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <StackPanel Orientation="Horizontal" Margin="12 0">
                    <Ellipse Width="8" Height="8" Fill="{Binding Color}" VerticalAlignment="Center" Margin="0 0 6 0"/>
                    <TextBlock Text="{Binding Text}" VerticalAlignment="Center"/>
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
                <TextBlock Text="{Binding Text}" Margin="12 0" VerticalAlignment="Center" Foreground="{Binding Color}"/>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</StatusBar>
```

### 核心要点

1. **纯数据驱动**：增删状态项只需操作集合，UI 自动同步，符合 MVVM 规范。
2. **权限动态适配**：不同角色看到不同的状态信息，权限逻辑集中在 ViewModel 中管理。
3. **可扩展性强**：新增状态类型只需扩展模型属性，无需修改 UI 结构。

------

## 案例 4：深色工控主题状态栏

### 场景说明

工业现场车间环境中，深色主题可降低视觉疲劳、减少屏幕反光，是高端上位机的标配。本案例实现完整的深色状态栏样式，适配工业深色界面。

### 完整样式代码

xaml:

```xaml
<Window.Resources>
    <!-- 状态栏整体样式 -->
    <Style TargetType="StatusBar">
        <Setter Property="Height" Value="28"/>
        <Setter Property="Background" Value="#1E1E1E"/>
        <Setter Property="Foreground" Value="#CCCCCC"/>
        <Setter Property="BorderBrush" Value="#333333"/>
        <Setter Property="BorderThickness" Value="0 1 0 0"/>
        <Setter Property="FontSize" Value="12"/>
        <Setter Property="FontFamily" Value="Microsoft YaHei"/>
    </Style>

    <!-- 状态栏条目样式 -->
    <Style TargetType="StatusBarItem">
        <Setter Property="Padding" Value="12 0"/>
        <Setter Property="VerticalContentAlignment" Value="Center"/>
    </Style>

    <!-- 分隔线样式 -->
    <Style TargetType="Separator">
        <Setter Property="Background" Value="#3A3A3A"/>
        <Setter Property="Width" Value="1"/>
        <Setter Property="Margin" Value="0 5"/>
    </Style>
</Window.Resources>

<!-- 使用示例 -->
<StatusBar DockPanel.Dock="Bottom">
    <StatusBarItem>
        <StackPanel Orientation="Horizontal">
            <Ellipse Width="8" Height="8" Fill="#67C23A" VerticalAlignment="Center" Margin="0 0 6 0"/>
            <TextBlock Text="系统运行正常"/>
        </StackPanel>
    </StatusBarItem>
    <Separator/>
    <StatusBarItem Content="设备在线：12/15"/>
    
    <StatusBarItem StatusBarPanel.Dock="Right" Text="2024-06-21 14:30:00"/>
</StatusBar>
```

### 核心要点

1. **低对比度设计**：背景用深灰 `#1E1E1E` 而非纯黑，文字用浅灰 `#CCCCCC` 而非纯白，避免强光对比刺眼。
2. **分隔线弱化**：深灰色分隔线，弱化边界感，减少视觉干扰。
3. **状态色降饱和**：绿色、红色等状态色适当降低饱和度，避免深色背景下过于刺眼。

------

## 案例 5：报警闪烁提醒状态栏

### 场景说明

存在未处理报警时，状态栏报警条目红色闪烁，强提醒操作人员注意异常。无需后台定时器，纯 XAML 动画实现，性能优异。

### 完整实现代码

xaml:

```xaml
<Window.Resources>
    <!-- 闪烁动画 -->
    <Storyboard x:Key="AlarmBlinkStoryboard" RepeatBehavior="Forever">
        <ColorAnimationUsingKeyFrames Storyboard.TargetProperty="(TextBlock.Foreground).(SolidColorBrush.Color)">
            <DiscreteColorKeyFrame KeyTime="0:0:0" Value="#F5222D"/>
            <DiscreteColorKeyFrame KeyTime="0:0:0.5" Value="#FFCCC7"/>
            <DiscreteColorKeyFrame KeyTime="0:0:1" Value="#F5222D"/>
        </ColorAnimationUsingKeyFrames>
    </Storyboard>
</Window.Resources>

<!-- 状态栏中报警条目 -->
<StatusBarItem>
    <TextBlock x:Name="AlarmText" Text="未处理报警：3" FontWeight="Bold">
        <TextBlock.Style>
            <Style TargetType="TextBlock">
                <Setter Property="Foreground" Value="#F5222D"/>
                <Style.Triggers>
                    <!-- 报警数>0时启动闪烁动画 -->
                    <DataTrigger Binding="{Binding AlarmCount}" Value="0">
                        <Setter Property="Foreground" Value="#52C41A"/>
                    </DataTrigger>
                    <DataTrigger Binding="{Binding HasAlarm}" Value="True">
                        <DataTrigger.EnterActions>
                            <BeginStoryboard Storyboard="{StaticResource AlarmBlinkStoryboard}"/>
                        </DataTrigger.EnterActions>
                        <DataTrigger.ExitActions>
                            <StopStoryboard BeginStoryboardName="AlarmBlink"/>
                        </DataTrigger.ExitActions>
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </TextBlock.Style>
    </TextBlock>
</StatusBarItem>
```

### 核心要点

1. **纯 XAML 动画**：无需后台线程控制闪烁，无线程安全问题，性能更好。
2. **状态联动**：报警数大于 0 时自动闪烁，报警清零后自动停止，完全数据驱动。
3. **低频闪烁**：1 秒一个周期，既起到提醒作用，又不会过度闪烁造成视觉干扰。

------

## 案例 6：多工位状态聚合状态栏

### 场景说明

单条产线多个工位的设备，在状态栏汇总显示每个工位的运行状态，操作人员扫一眼就能掌握整条产线的健康度，适合多工位流水线监控。

### 实现效果

底部状态栏右侧显示一排小型状态指示灯，对应 1~8 号工位，绿色运行、黄色待机、红色故障。

### 完整代码

xaml:

```xaml
<StatusBar DockPanel.Dock="Bottom" Height="28">
    <StatusBarItem Content="产线运行中"/>
    <Separator/>
    <StatusBarItem Content="产量：1256 / 1500"/>

    <!-- 右侧：工位状态聚合 -->
    <StatusBarItem StatusBarPanel.Dock="Right">
        <StackPanel Orientation="Horizontal">
            <TextBlock Text="工位状态：" VerticalAlignment="Center" Margin="0 0 8 0" Foreground="#666"/>
            <ItemsControl ItemsSource="{Binding StationStatusList}">
                <ItemsControl.ItemsPanel>
                    <ItemsPanelTemplate>
                        <WrapPanel Orientation="Horizontal"/>
                    </ItemsPanelTemplate>
                </ItemsControl.ItemsPanel>
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <ToolTipService.ToolTip>
                            <TextBlock Text="{Binding StationName + ： + StatusText}"/>
                        </ToolTipService.ToolTip>
                        <Ellipse Width="10" Height="10" 
                                 Fill="{Binding StatusColor}" 
                                 Margin="2 0"
                                 VerticalAlignment="Center"/>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </StackPanel>
    </StatusBarItem>
</StatusBar>
```

#### 工位状态模型

csharp:

```c#
public class StationStatus : INotifyPropertyChanged
{
    public string StationName { get; set; }
    public string StatusText { get; set; }
    public string StatusColor { get; set; } // 运行绿/待机黄/故障红
}
```

### 核心要点

1. **信息聚合**：用最小的空间展示最多的设备状态，符合状态栏「信息浓缩」的定位。
2. **Tooltip 详情**：鼠标悬停显示工位名称和详细状态，兼顾简洁性和信息完整性。
3. **全局概览**：操作人员无需切换页面，就能快速发现哪个工位出现异常。

------

## 工业状态栏设计最佳实践

1. **状态优先，颜色说话**：核心状态必须用颜色可视化，工业三色标准：绿正常、黄警告、红故障。
2. **信息分级，重点突出**：报警 > 运行状态 > 辅助信息，优先级从左到右、从高到低。
3. **数量克制，避免堆砌**：单条状态栏条目控制在 5~8 个，信息过载反而失去提醒意义。
4. **交互极简，点到为止**：最多保留 1~2 个可点击入口（如报警跳转），不要堆砌按钮。
5. **实时刷新，数据准确**：时间、计数、状态保证秒级更新，避免信息滞后误导操作。