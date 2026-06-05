# 004008001_WPF **RepeatButton 重复按钮**官方类定义 + 底层原理 + 工业级使用方法 + 完整实例

`RepeatButton` 是工业上位机开发中**最常用的特殊按钮**，按住不放会**连续重复触发 Click 事件**，完美适配参数微调、速度调节、数值加减等工业场景。本文 100% 基于.NET 9 官方源码，从底层到实战全面覆盖。

------

## 一、RepeatButton 官方完整类定义（原汁原味）

csharp:

```c#
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace System.Windows.Controls.Primitives
{
    /// <summary>
    /// 重复按钮：按住鼠标左键不放时，会重复触发Click事件
    /// </summary>
    [Localizability(LocalizationCategory.Button)]
    public class RepeatButton : ButtonBase
    {
        // 静态依赖属性
        public static readonly DependencyProperty DelayProperty;
        public static readonly DependencyProperty IntervalProperty;

        // 静态构造函数
        static RepeatButton();

        // 公共构造函数
        public RepeatButton();

        // 核心属性
        public int Delay { get; set; }
        public int Interval { get; set; }

        // 受保护方法
        protected override void OnClick();
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e);
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e);
        protected override void OnMouseLeave(MouseEventArgs e);
        protected override AutomationPeer OnCreateAutomationPeer();
    }
}
```

------

## 二、逐行深度解析：官方类定义

### 1. 类声明与继承体系

csharp:

```c#
public class RepeatButton : ButtonBase
```

- **命名空间**：`System.Windows.Controls.Primitives`（注意：不在主 Controls 命名空间，很多人容易写错）
- **直接父类**：`ButtonBase`（所有按钮的基类）
- **核心结论**：RepeatButton 99% 的基础功能来自`ButtonBase`，自己只实现了**按住重复触发**的逻辑。

#### 完整继承链

plaintext:

```tex
object
   ↳ DispatcherObject
   ↳ DependencyObject
   ↳ Visual
   ↳ UIElement
   ↳ FrameworkElement
   ↳ Control
   ↳ ContentControl
   ↳ ButtonBase
   ↳ RepeatButton
```

------

### 2. 静态构造函数（官方内部实现）

csharp:

```c#
static RepeatButton()
{
    // 1. 注册Delay依赖属性：按下后延迟多久开始重复
    DelayProperty = DependencyProperty.Register(
        nameof(Delay),
        typeof(int),
        typeof(RepeatButton),
        new FrameworkPropertyMetadata(
            SystemParameters.KeyboardDelay, // 默认值：系统键盘延迟（约500ms）
            FrameworkPropertyMetadataOptions.None,
            null,
            CoerceDelay), // 强制验证：不能小于0

    // 2. 注册Interval依赖属性：重复触发的间隔
    IntervalProperty = DependencyProperty.Register(
        nameof(Interval),
        typeof(int),
        typeof(RepeatButton),
        new FrameworkPropertyMetadata(
            SystemParameters.KeyboardSpeed, // 默认值：系统键盘速度（约33ms）
            FrameworkPropertyMetadataOptions.None,
            null,
            CoerceInterval), // 强制验证：不能小于0

    // 3. 覆盖默认样式：应用系统原生按钮样式
    DefaultStyleKeyProperty.OverrideMetadata(
        typeof(RepeatButton),
        new FrameworkPropertyMetadata(typeof(RepeatButton)));
}
```

------

### 3. 核心依赖属性（RepeatButton 的灵魂）

#### ① `Delay` 属性

csharp:

```c#
public int Delay { get; set; }
```

- **作用**：鼠标按下后，**延迟多少毫秒开始第一次重复触发**
- **单位**：毫秒（ms）
- **默认值**：`SystemParameters.KeyboardDelay`（约 500ms，和系统键盘按住重复的延迟一致）
- **最小值**：0（不能为负数）

#### ② `Interval` 属性

csharp:

```c#
public int Interval { get; set; }
```

- **作用**：第一次触发后，**每隔多少毫秒重复触发一次**
- **单位**：毫秒（ms）
- **默认值**：`SystemParameters.KeyboardSpeed`（约 33ms，每秒触发 30 次）
- **最小值**：0（不能为负数）

> **工业场景最佳值**：
>
> - Delay：200-300ms（按下后稍作等待再开始重复，避免误触）
> - Interval：100-200ms（每秒触发 5-10 次，调节速度适中）

------

### 4. 核心方法与底层原理

RepeatButton 的重复逻辑完全基于**DispatcherTimer**实现，工作流程如下：

#### ① 鼠标按下：启动计时器

csharp:

```c#
protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
{
    base.OnMouseLeftButtonDown(e);
    
    if (IsPressed && IsEnabled)
    {
        // 捕获鼠标，即使移出按钮也能继续触发
        Mouse.Capture(this);
        
        // 启动计时器：先等待Delay毫秒，然后每隔Interval毫秒触发一次
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(Delay);
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }
}
```

#### ② 计时器触发：重复调用 OnClick

csharp:

```c#
private void OnTimerTick(object sender, EventArgs e)
{
    // 第一次触发后，将计时器间隔改为Interval
    if (_timer.Interval.TotalMilliseconds == Delay)
    {
        _timer.Interval = TimeSpan.FromMilliseconds(Interval);
    }
    
    // 触发Click事件
    OnClick();
}
```

#### ③ 鼠标松开 / 离开：停止计时器

csharp:

```c#
protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
{
    base.OnMouseLeftButtonUp(e);
    StopTimer();
    Mouse.Capture(null);
}

protected override void OnMouseLeave(MouseEventArgs e)
{
    base.OnMouseLeave(e);
    StopTimer();
    Mouse.Capture(null);
}

private void StopTimer()
{
    if (_timer != null)
    {
        _timer.Stop();
        _timer.Tick -= OnTimerTick;
        _timer = null;
    }
}
```

#### ④ 重写 OnClick：触发 Click 事件

csharp:

```c#
protected override void OnClick()
{
    // 调用基类方法，触发Click路由事件和命令
    base.OnClick();
}
```

------

## 三、RepeatButton 核心功能详解

### 1. 核心独有功能（区别于普通 Button）

#### ① 按住重复触发

- 按住鼠标左键不放，会连续触发 Click 事件
- 松开鼠标或移出按钮，立即停止触发
- 支持鼠标捕获：即使鼠标移出按钮范围，只要不松开，仍会继续触发

#### ② 可配置的触发节奏

- 通过`Delay`控制开始重复前的延迟
- 通过`Interval`控制重复触发的频率
- 完全自定义调节速度，适配不同场景

#### ③ 与系统行为一致

- 默认值与系统键盘按住重复的参数一致
- 用户体验符合操作系统习惯

### 2. 继承自 ButtonBase 的功能

- `Content`：支持任意内容（文本、图标、布局）
- `Click`事件：点击 / 重复触发时触发
- `Command`/`CommandParameter`：MVVM 命令绑定
- `IsEnabled`：禁用状态下不会触发重复
- `IsPressed`：指示按钮是否处于按下状态
- 样式、模板、数据绑定等所有 WPF 控件通用功能

### 3. RepeatButton vs Button 对比

| 特性     | RepeatButton       | Button               |
| :------- | :----------------- | :------------------- |
| 点击行为 | 按住不放重复触发   | 点击一次触发一次     |
| 核心属性 | Delay、Interval    | 无                   |
| 适用场景 | 参数微调、连续调节 | 单次操作、确认、取消 |
| 继承关系 | ButtonBase         | ButtonBase           |

------

## 四、标准使用方法

### 1. 基础使用（默认参数）

xaml:

```xaml
<RepeatButton Content="按住增加" Click="RepeatButton_Click"/>
```

### 2. 自定义延迟和间隔（工业推荐）

xaml:

```xaml
<RepeatButton Content="按住增加" 
              Delay="200" 
              Interval="100"
              Click="RepeatButton_Click"/>
```

- 按下后等待 200ms 开始重复
- 之后每隔 100ms 触发一次（每秒 10 次）

### 3. MVVM 命令绑定（企业级开发）

xaml:

```xaml
<RepeatButton Content="温度+" 
              Delay="200" 
              Interval="100"
              Command="{Binding IncreaseTemperatureCommand}"/>
```

### 4. 图标式重复按钮（工业界面常用）

xaml:

```xaml
<RepeatButton Width="40" Height="40"
              Delay="200" 
              Interval="100"
              Command="{Binding DecreaseCommand}">
    <Path Data="M0,5 L10,0 L10,10 Z" Fill="White" Width="10" Height="10"/>
</RepeatButton>
```

------

## 五、完整工业级实例

### 实例 1：参数调节加减按钮（最常用）

这是工业上位机中最常见的场景，用于微调温度、压力、速度等参数。

#### XAML

xaml:

```c#
<Grid Margin="20">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="100"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>

    <!-- 减按钮 -->
    <RepeatButton Grid.Column="0" 
                  Content="-" 
                  FontSize="20"
                  Width="40" 
                  Height="40"
                  Delay="200" 
                  Interval="100"
                  Click="Decrease_Click"/>

    <!-- 参数显示 -->
    <TextBox Grid.Column="1" 
             Text="{Binding Temperature, StringFormat='{0:F1}℃'}"
             FontSize="18"
             HorizontalContentAlignment="Center"
             VerticalContentAlignment="Center"
             Margin="5,0"
             IsReadOnly="True"/>

    <!-- 加按钮 -->
    <RepeatButton Grid.Column="2" 
                  Content="+" 
                  FontSize="20"
                  Width="40" 
                  Height="40"
                  Delay="200" 
                  Interval="100"
                  Click="Increase_Click"/>
</Grid>
```

#### C# 后台

csharp:

```c#
private double _temperature = 25.0;
public double Temperature
{
    get => _temperature;
    set
    {
        // 范围限制：0-100℃
        _temperature = Math.Clamp(value, 0.0, 100.0);
        OnPropertyChanged();
    }
}

private void Increase_Click(object sender, RoutedEventArgs e)
{
    // 每次增加0.1℃
    Temperature += 0.1;
}

private void Decrease_Click(object sender, RoutedEventArgs e)
{
    // 每次减少0.1℃
    Temperature -= 0.1;
}
```

------

### 实例 2：速度调节滑块 + 重复按钮

结合 Slider 和 RepeatButton，实现精确的速度调节。

xaml:

```xaml
<Grid Margin="20">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>

    <TextBlock Grid.Row="0" 
               Text="传送带速度：" 
               FontSize="14"
               Margin="0 0 0 5"/>

    <Grid Grid.Row="1">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto"/>
            <ColumnDefinition Width="*"/>
            <ColumnDefinition Width="Auto"/>
        </Grid.ColumnDefinitions>

        <!-- 减速按钮 -->
        <RepeatButton Grid.Column="0" 
                      Content="◄" 
                      Width="40" 
                      Height="30"
                      Delay="200" 
                      Interval="100"
                      Command="{Binding DecreaseSpeedCommand}"/>

        <!-- 滑块 -->
        <Slider Grid.Column="1" 
                Minimum="0" 
                Maximum="100"
                Value="{Binding Speed}"
                Margin="10,0"/>

        <!-- 加速按钮 -->
        <RepeatButton Grid.Column="2" 
                      Content="►" 
                      Width="40" 
                      Height="30"
                      Delay="200" 
                      Interval="100"
                      Command="{Binding IncreaseSpeedCommand}"/>
    </Grid>

    <TextBlock Grid.Row="2" 
               Text="{Binding Speed, StringFormat='速度：{0:F0} m/min'}"
               FontSize="14"
               HorizontalAlignment="Center"
               Margin="0 10 0 0"/>
</Grid>
```

------

### 实例 3：MVVM 完整实现（无后台代码）

#### ViewModel

csharp:

```c#
public class MainViewModel : INotifyPropertyChanged
{
    private double _temperature = 25.0;
    public double Temperature
    {
        get => _temperature;
        set
        {
            _temperature = Math.Clamp(value, 0.0, 100.0);
            OnPropertyChanged();
        }
    }

    // 增加温度命令
    public ICommand IncreaseCommand { get; }
    // 减少温度命令
    public ICommand DecreaseCommand { get; }

    public MainViewModel()
    {
        IncreaseCommand = new DelegateCommand(() => Temperature += 0.1);
        DecreaseCommand = new DelegateCommand(() => Temperature -= 0.1);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string prop = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}

// DelegateCommand 实现
public class DelegateCommand : ICommand
{
    private readonly Action _execute;
    public DelegateCommand(Action execute) => _execute = execute;
    public bool CanExecute(object parameter) => true;
    public void Execute(object parameter) => _execute();
    public event EventHandler CanExecuteChanged;
}
```

#### XAML

xaml:

```xaml
<Window.Resources>
    <local:MainViewModel x:Key="VM"/>
</Window.Resources>

<Grid DataContext="{StaticResource VM}">
    <RepeatButton Content="-" 
                  Width="40" 
                  Height="40"
                  Delay="200" 
                  Interval="100"
                  Command="{Binding DecreaseCommand}"/>

    <TextBox Text="{Binding Temperature, StringFormat='{0:F1}℃'}"
             HorizontalContentAlignment="Center"
             IsReadOnly="True"
             Margin="50,0"/>

    <RepeatButton Content="+" 
                  Width="40" 
                  Height="40"
                  Delay="200" 
                  Interval="100"
                  Command="{Binding IncreaseCommand}"
                  Margin="100,0,0,0"/>
</Grid>
```

------

## 六、工业开发最佳实践

### 1. 参数设置建议

- **Delay**：200-300ms（避免误触，给用户反应时间）
- **Interval**：100-200ms（每秒 5-10 次，调节速度适中）
- 不要设置太小的 Interval（<50ms），否则会导致界面卡顿和数据更新过于频繁

### 2. 范围限制

- 一定要在 ViewModel 或后台代码中对参数进行范围限制，避免超出设备允许的范围
- 使用`Math.Clamp`方法可以简洁地实现范围限制

### 3. 禁用状态处理

- 当设备处于运行状态或参数不可修改时，设置`IsEnabled="False"`
- 禁用状态下 RepeatButton 不会触发任何重复事件

### 4. 输入验证

- 如果允许用户直接输入参数，一定要添加输入验证，防止非法值
- 可以使用`IDataErrorInfo`或`INotifyDataErrorInfo`实现验证

### 5. 样式统一

- 整个应用内的 RepeatButton 样式保持一致
- 工业界面推荐使用简洁、大尺寸的按钮，方便操作

------

## 七、常见问题与解决方案

### 问题 1：按住按钮不触发重复

**可能原因**：

- 处理了`MouseLeftButtonDown`事件并设置了`e.Handled = true`，阻止了 RepeatButton 的内部逻辑
- 按钮被禁用（`IsEnabled="False"`）
- 鼠标捕获被其他控件抢走

**解决方案**：

- 不要在 RepeatButton 上处理`MouseLeftButtonDown`事件，使用`Click`事件或命令
- 确保按钮处于启用状态
- 检查是否有其他控件捕获了鼠标

### 问题 2：重复触发太快导致数据溢出

**原因**：Interval 设置太小，触发频率过高

**解决方案**：

- 将 Interval 设置为 100ms 以上
- 在事件处理中添加节流逻辑，限制更新频率

### 问题 3：鼠标移出后还在触发

**原因**：鼠标捕获异常

**解决方案**：

- 在`MouseLeave`事件中手动停止计时器并释放鼠标捕获
- 确保在按钮的 Unloaded 事件中停止计时器

------

## 八、终极总结

### RepeatButton 的本质

**RepeatButton = ButtonBase + 按住重复触发逻辑**

- 基类：`ButtonBase`（提供所有按钮基础功能）
- 核心：`Delay`（开始延迟）+ `Interval`（重复间隔）+ `DispatcherTimer`（计时器）
- 行为：按住不放连续触发 Click 事件，松开立即停止
- 场景：工业参数微调、速度调节、数值加减等需要连续操作的场景