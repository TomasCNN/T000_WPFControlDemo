# 004001001_WPF Control 基类 完整深度解析

`Control`是**所有 WPF 标准控件的共同基类**，也是 WPF 控件体系的核心基石。它定义了所有可视化控件共有的属性、方法、事件和行为，是你开发自定义控件、统一界面风格、实现复杂交互的基础。

结合你正在开发的**工业上位机项目**，我会从**类定义→核心特性→工业场景应用→避坑指南→实战代码**全方位讲解，所有实例都可直接用于你的 IO 监控、状态显示、参数配置等模块。

------

## 一、Control 基类的本质与类定义

### 1.1 在 WPF 类层次结构中的位置

`Control`位于 WPF 类层次结构的核心位置，继承链如下：

plaintext

```tex
System.Object
  ↳ System.Windows.Threading.DispatcherObject
    ↳ System.Windows.DependencyObject （依赖属性系统）
      ↳ System.Windows.Media.Visual （可视化渲染）
        ↳ System.Windows.UIElement （输入事件、布局）
          ↳ System.Windows.FrameworkElement （样式、数据绑定、布局）
            ↳ System.Windows.Controls.Control （所有控件的基类）
              ↳ System.Windows.Controls.ContentControl （单内容控件：Button、Label）
              ↳ System.Windows.Controls.ItemsControl （多内容控件：ListBox、DataGrid）
              ↳ System.Windows.Controls.TextBoxBase （文本控件：TextBox）
              ↳ System.Windows.Controls.Primitives.ButtonBase （按钮基类）
              ↳ ... 所有其他WPF标准控件
```

### 1.2 官方类定义

csharp:

```c#
namespace System.Windows.Controls
{
    /// <summary>
    /// 表示用户界面元素的基类，这些元素使用ControlTemplate来定义其外观
    /// </summary>
    [DefaultProperty("Content")]
    [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
    public class Control : FrameworkElement
    {
        // 核心依赖属性
        public static readonly DependencyProperty TemplateProperty;
        public static readonly DependencyProperty StyleProperty;
        public static readonly DependencyProperty BackgroundProperty;
        public static readonly DependencyProperty ForegroundProperty;
        public static readonly DependencyProperty FontFamilyProperty;
        public static readonly DependencyProperty FontSizeProperty;
        public static readonly DependencyProperty FontWeightProperty;
        public static readonly DependencyProperty IsEnabledProperty;
        public static readonly DependencyProperty VisibilityProperty;
        public static readonly DependencyProperty BorderBrushProperty;
        public static readonly DependencyProperty BorderThicknessProperty;
        public static readonly DependencyProperty PaddingProperty;
        public static readonly DependencyProperty HorizontalContentAlignmentProperty;
        public static readonly DependencyProperty VerticalContentAlignmentProperty;

        // 构造函数
        public Control();

        // 核心属性
        public ControlTemplate Template { get; set; }
        public Style Style { get; set; }
        public Brush Background { get; set; }
        public Brush Foreground { get; set; }
        public FontFamily FontFamily { get; set; }
        public double FontSize { get; set; }
        public FontWeight FontWeight { get; set; }
        public bool IsEnabled { get; set; }
        public Visibility Visibility { get; set; }
        public Brush BorderBrush { get; set; }
        public Thickness BorderThickness { get; set; }
        public Thickness Padding { get; set; }
        public HorizontalAlignment HorizontalContentAlignment { get; set; }
        public VerticalAlignment VerticalContentAlignment { get; set; }

        // 核心方法
        public override void OnApplyTemplate();
        protected override Size MeasureOverride(Size constraint);
        protected override Size ArrangeOverride(Size arrangeBounds);
        public virtual void BeginInit();
        public virtual void EndInit();
        public bool Focus();
        public void UpdateLayout();

        // 核心事件
        public event RoutedEventHandler Loaded;
        public event RoutedEventHandler Unloaded;
        public event DependencyPropertyChangedEventHandler IsEnabledChanged;
        public event DependencyPropertyChangedEventHandler VisibilityChanged;
        public event MouseButtonEventHandler MouseDown;
        public event MouseButtonEventHandler MouseUp;
        public event MouseEventHandler MouseMove;
    }
}
```

### 1.3 Control 类的核心设计思想

WPF 控件的核心设计原则是 **"逻辑与外观分离"**：

- `Control`类只负责**控件的逻辑行为**（如点击、状态变化、数据处理）
- 控件的**外观完全由`ControlTemplate`定义**，可以完全自定义
- 逻辑与外观通过`TemplateBinding`进行绑定

这是 WPF 与 WinForms 最大的区别之一，也是 WPF 能够实现高度自定义界面的基础。

------

## 二、Control 基类核心特性详解

### 2.1 最核心的属性：Template（控件模板）

`Template`是`Control`类最重要的属性，没有之一。它决定了控件在界面上的样子。

#### 什么是 ControlTemplate？

`ControlTemplate`是一个 XAML 代码块，定义了控件的可视化树结构。你可以完全替换一个控件的模板，而不改变它的逻辑行为。

**示例：将 Button 变成一个圆形按钮**

xaml:

```xaml
<Button Content="圆形按钮" Width="100" Height="100">
    <Button.Template>
        <ControlTemplate TargetType="Button">
            <Ellipse Fill="{TemplateBinding Background}" 
                     Stroke="{TemplateBinding BorderBrush}"
                     StrokeThickness="2">
                <ContentPresenter HorizontalAlignment="Center" 
                                  VerticalAlignment="Center"/>
            </Ellipse>
        </ControlTemplate>
    </Button.Template>
</Button>
```

这个按钮的逻辑行为（点击事件、命令绑定）完全不变，但外观变成了圆形。

#### TemplateBinding

`TemplateBinding`是专门用于控件模板的绑定，它将模板中的元素属性绑定到控件的依赖属性上。

- `{TemplateBinding Background}`：绑定到控件的 Background 属性
- `{TemplateBinding IsEnabled}`：绑定到控件的 IsEnabled 属性

### 2.2 样式与主题：Style 属性

`Style`属性允许你将一组属性值应用到多个控件上，实现样式的复用。

**示例：统一所有按钮的样式**

xaml:

```xaml
<Window.Resources>
    <Style TargetType="Button">
        <Setter Property="Background" Value="#2196F3"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="Padding" Value="10,5"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border Background="{TemplateBinding Background}"
                            CornerRadius="4">
                        <ContentPresenter HorizontalAlignment="Center"
                                          VerticalAlignment="Center"/>
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</Window.Resources>

<!-- 所有按钮都会自动应用上面的样式 -->
<Button Content="启动"/>
<Button Content="停止"/>
<Button Content="复位"/>
```

### 2.3 通用外观属性

所有继承自`Control`的控件都拥有以下通用外观属性：

| 属性              | 作用       | 工业场景常用值                          |
| :---------------- | :--------- | :-------------------------------------- |
| `Background`      | 控件背景色 | 绿色 (运行)、红色 (停机)、黄色 (警告)   |
| `Foreground`      | 文本颜色   | 白色、黑色                              |
| `BorderBrush`     | 边框颜色   | 灰色、红色                              |
| `BorderThickness` | 边框厚度   | 1、2                                    |
| `Padding`         | 内边距     | 5,5                                     |
| `FontFamily`      | 字体       | "Microsoft YaHei"                       |
| `FontSize`        | 字号       | 12、14、16                              |
| `FontWeight`      | 字重       | FontWeights.Bold(正常/斜体/粗体/细体等) |

### 2.4 状态属性

| 属性         | 作用             | 工业场景应用               |
| :----------- | :--------------- | :------------------------- |
| `IsEnabled`  | 控件是否启用     | 设备运行时禁用参数修改按钮 |
| `Visibility` | 控件是否可见     | 隐藏未使用的功能模块       |
| `IsFocused`  | 控件是否获得焦点 | 输入框自动聚焦             |

### 2.5 核心方法

#### 1. OnApplyTemplate()

当控件的模板被应用时调用，这是你访问模板内部元素的唯一时机。

csharp:

```c#
public override void OnApplyTemplate()
{
    base.OnApplyTemplate();
    
    // 获取模板中的元素
    var indicator = GetTemplateChild("PART_Indicator") as Ellipse;
    if (indicator != null)
    {
        // 初始化元素
        indicator.Fill = Brushes.Gray;
    }
}
```

**命名规范**：模板中需要被后台代码访问的元素，命名必须以`PART_`开头。

#### 2. MeasureOverride () 和 ArrangeOverride ()

用于实现自定义布局：

- `MeasureOverride(Size constraint)`：计算控件需要的大小
- `ArrangeOverride(Size arrangeBounds)`：排列控件内部的子元素

### 2.6 核心事件

| 事件               | 触发时机             | 工业场景应用                       |
| :----------------- | :------------------- | :--------------------------------- |
| `Loaded`           | 控件加载到可视化树时 | 初始化控件数据、启动定时器         |
| `Unloaded`         | 控件从可视化树移除时 | 释放资源、停止定时器、取消事件订阅 |
| `IsEnabledChanged` | IsEnabled 属性变化时 | 禁用时改变控件外观                 |
| `MouseDown`        | 鼠标按下时           | 自定义按钮点击逻辑                 |

------

## 三、工业上位机典型应用场景

### 场景 1：自定义工业专用控件

这是`Control`类在工业项目中最常用的场景。WPF 标准控件无法满足工业场景的特殊需求（如 IO 指示灯、阀门状态、仪表等），需要继承`Control`开发自定义控件。

#### 实战实例：IO 状态指示灯控件

**需求**：显示 PLC 输入输出点的状态，绿色表示 ON，红色表示 OFF，支持闪烁报警。

##### 第一步：定义自定义控件类

csharp:

```c#
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace IndustrialVisionTemplate.Controls
{
    /// <summary>
    /// IO状态指示灯控件
    /// </summary>
    public class IoIndicator : Control
    {
        // 静态构造函数：指定默认样式
        static IoIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(IoIndicator), 
                new FrameworkPropertyMetadata(typeof(IoIndicator)));
        }

        // 依赖属性：IO状态（true=ON，false=OFF）
        public static readonly DependencyProperty IsOnProperty = DependencyProperty.Register(
            nameof(IsOn), 
            typeof(bool), 
            typeof(IoIndicator), 
            new PropertyMetadata(false, OnIsOnChanged));

        public bool IsOn
        {
            get => (bool)GetValue(IsOnProperty);
            set => SetValue(IsOnProperty, value);
        }

        // 依赖属性：是否闪烁（报警状态）
        public static readonly DependencyProperty IsBlinkingProperty = DependencyProperty.Register(
            nameof(IsBlinking), 
            typeof(bool), 
            typeof(IoIndicator), 
            new PropertyMetadata(false, OnIsBlinkingChanged));

        public bool IsBlinking
        {
            get => (bool)GetValue(IsBlinkingProperty);
            set => SetValue(IsBlinkingProperty, value);
        }

        // 闪烁定时器
        private DispatcherTimer _blinkTimer;
        private bool _isBlinkOn;

        // 状态变化回调
        private static void OnIsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (IoIndicator)d;
            control.UpdateIndicatorColor();
        }

        private static void OnIsBlinkingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (IoIndicator)d;
            if ((bool)e.NewValue)
            {
                control.StartBlinking();
            }
            else
            {
                control.StopBlinking();
            }
        }

        // 启动闪烁
        private void StartBlinking()
        {
            if (_blinkTimer == null)
            {
                _blinkTimer = new DispatcherTimer();
                _blinkTimer.Interval = TimeSpan.FromMilliseconds(500);
                _blinkTimer.Tick += (s, e) =>
                {
                    _isBlinkOn = !_isBlinkOn;
                    UpdateIndicatorColor();
                };
            }
            _blinkTimer.Start();
        }

        // 停止闪烁
        private void StopBlinking()
        {
            _blinkTimer?.Stop();
            _isBlinkOn = false;
            UpdateIndicatorColor();
        }

        // 更新指示灯颜色
        private void UpdateIndicatorColor()
        {
            if (IsBlinking && !_isBlinkOn)
            {
                // 闪烁时熄灭
                SetValue(IndicatorColorPropertyKey, Brushes.Gray);
            }
            else if (IsOn)
            {
                // ON状态绿色
                SetValue(IndicatorColorPropertyKey, Brushes.LimeGreen);
            }
            else
            {
                // OFF状态红色
                SetValue(IndicatorColorPropertyKey, Brushes.Red);
            }
        }

        // 只读依赖属性：指示灯颜色
        private static readonly DependencyPropertyKey IndicatorColorPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(IndicatorColor), 
            typeof(Brush), 
            typeof(IoIndicator), 
            new PropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty IndicatorColorProperty = IndicatorColorPropertyKey.DependencyProperty;

        public Brush IndicatorColor
        {
            get => (Brush)GetValue(IndicatorColorProperty);
        }

        // 控件卸载时释放资源
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Unloaded += (s, e) =>
            {
                _blinkTimer?.Stop();
                _blinkTimer = null;
            };
        }
    }
}
```

##### 第二步：定义默认样式（Themes/Generic.xaml）

xaml:

```xaml
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:controls="clr-namespace:IndustrialVisionTemplate.Controls">

    <Style TargetType="controls:IoIndicator">
        <Setter Property="Width" Value="20"/>
        <Setter Property="Height" Value="20"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="controls:IoIndicator">
                    <Ellipse 
                        x:Name="PART_Indicator"
                        Fill="{TemplateBinding IndicatorColor}"
                        Stroke="Black"
                        StrokeThickness="1"/>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</ResourceDictionary>
```

##### 第三步：在界面中使用

xaml:

```xaml
<Window xmlns:controls="clr-namespace:IndustrialVisionTemplate.Controls">
    <StackPanel Orientation="Horizontal" Margin="10">
        <TextBlock Text="输入1：" VerticalAlignment="Center"/>
        <controls:IoIndicator IsOn="{Binding Input1State}" Margin="5,0"/>
        
        <TextBlock Text="输入2：" VerticalAlignment="Center" Margin="20,0,0,0"/>
        <controls:IoIndicator IsOn="{Binding Input2State}" IsBlinking="{Binding Input2Alarm}" Margin="5,0"/>
        
        <TextBlock Text="输出1：" VerticalAlignment="Center" Margin="20,0,0,0"/>
        <controls:IoIndicator IsOn="{Binding Output1State}" Margin="5,0"/>
    </StackPanel>
</Window>
```

### 场景 2：统一界面风格

通过继承`Control`的样式，实现整个项目界面风格的统一，这是工业上位机项目的必备需求。

**示例：工业项目全局控件样式**

xaml:

```xaml
<!-- App.xaml -->
<Application.Resources>
    <!-- 全局Control样式，所有控件继承 -->
    <Style TargetType="Control">
        <Setter Property="FontFamily" Value="Microsoft YaHei"/>
        <Setter Property="FontSize" Value="12"/>
        <Setter Property="Foreground" Value="#333333"/>
    </Style>

    <!-- 按钮样式 -->
    <Style TargetType="Button" BasedOn="{StaticResource {x:Type Control}}">
        <Setter Property="Background" Value="#2196F3"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="Padding" Value="15,8"/>
        <Setter Property="MinWidth" Value="80"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border 
                        Background="{TemplateBinding Background}"
                        CornerRadius="4"
                        BorderThickness="0">
                        <ContentPresenter 
                            HorizontalAlignment="Center"
                            VerticalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter Property="Background" Value="#1976D2"/>
                        </Trigger>
                        <Trigger Property="IsPressed" Value="True">
                            <Setter Property="Background" Value="#0D47A1"/>
                        </Trigger>
                        <Trigger Property="IsEnabled" Value="False">
                            <Setter Property="Background" Value="#BDBDBD"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- 危险按钮样式 -->
    <Style x:Key="DangerButtonStyle" TargetType="Button" BasedOn="{StaticResource {x:Type Button}}">
        <Setter Property="Background" Value="#F44336"/>
        <Style.Triggers>
            <Trigger Property="IsMouseOver" Value="True">
                <Setter Property="Background" Value="#D32F2F"/>
            </Trigger>
        </Style.Triggers>
    </Style>
</Application.Resources>
```

### 场景 3：动态创建控件

在工业项目中，经常需要根据 PLC 的 IO 点数动态创建控件。

**示例：动态创建 IO 监控界面**

csharp:

```c#
private void CreateIoControls()
{
    // 从PLC获取IO点列表
    var ioPoints = Device.PLC.GetIoPoints();
    
    foreach (var ioPoint in ioPoints)
    {
        // 创建IO指示灯控件
        var indicator = new IoIndicator();
        indicator.IsOn = ioPoint.State;
        indicator.IsBlinking = ioPoint.IsAlarm;
        
        // 创建标签
        var label = new TextBlock();
        label.Text = $"{ioPoint.Name}：";
        label.VerticalAlignment = VerticalAlignment.Center;
        
        // 添加到布局
        var stackPanel = new StackPanel();
        stackPanel.Orientation = Orientation.Horizontal;
        stackPanel.Margin = new Thickness(5);
        stackPanel.Children.Add(label);
        stackPanel.Children.Add(indicator);
        
        // 绑定数据
        indicator.SetBinding(IoIndicator.IsOnProperty, 
            new Binding($"IoPoints[{ioPoint.Id}].State"));
        indicator.SetBinding(IoIndicator.IsBlinkingProperty, 
            new Binding($"IoPoints[{ioPoint.Id}].IsAlarm"));
        
        IoPanel.Children.Add(stackPanel);
    }
}
```

### 场景 4：控件状态管理

通过`Control`的`IsEnabled`和`Visibility`属性，实现设备不同状态下的界面控制。

**示例：设备运行时禁用参数修改**

csharp:

```c#
private void DeviceStatusChanged(object sender, DeviceStatusChangedEventArgs e)
{
    if (e.NewStatus == DeviceStatus.RUNNING)
    {
        // 设备运行时禁用所有参数输入控件
        ParameterPanel.IsEnabled = false;
        // 隐藏调试按钮
        DebugButton.Visibility = Visibility.Collapsed;
        // 启用停止按钮
        StopButton.IsEnabled = true;
        // 禁用启动按钮
        StartButton.IsEnabled = false;
    }
    else if (e.NewStatus == DeviceStatus.IDLE)
    {
        // 设备空闲时启用参数输入
        ParameterPanel.IsEnabled = true;
        // 显示调试按钮
        DebugButton.Visibility = Visibility.Visible;
        // 禁用停止按钮
        StopButton.IsEnabled = false;
        // 启用启动按钮
        StartButton.IsEnabled = true;
    }
}
```

------

## 四、应用注意点与避坑指南

### 1. 绝对不要在构造函数中访问模板元素

模板是在`OnApplyTemplate()`方法中才被应用的，构造函数执行时模板还没有加载，此时调用`GetTemplateChild()`会返回 null。

❌ 错误写法：

csharp:

```c#
public IoIndicator()
{
    // 错误！构造函数中模板还没应用
    var indicator = GetTemplateChild("PART_Indicator") as Ellipse;
}
```

✅ 正确写法：

csharp:

```c#
public override void OnApplyTemplate()
{
    base.OnApplyTemplate();
    // 正确！此时模板已经应用
    var indicator = GetTemplateChild("PART_Indicator") as Ellipse;
}
```

### 2. 不要直接修改模板内部元素的属性

应该通过依赖属性和`TemplateBinding`来控制模板元素的外观，不要在后台代码中直接修改模板元素的属性。

❌ 错误写法：

csharp:

```c#
public void SetColor(Brush color)
{
    var indicator = GetTemplateChild("PART_Indicator") as Ellipse;
    if (indicator != null)
    {
        indicator.Fill = color; // 错误！直接修改模板元素
    }
}
```

✅ 正确写法：

csharp:

```c#
// 定义依赖属性
public static readonly DependencyProperty IndicatorColorProperty = ...;

// 在模板中绑定
<Ellipse Fill="{TemplateBinding IndicatorColor}"/>

// 修改依赖属性
public void SetColor(Brush color)
{
    IndicatorColor = color;
}
```

### 3. 必须在 Unloaded 事件中释放资源

自定义控件中使用的定时器、事件订阅、非托管资源，必须在`Unloaded`事件中释放，否则会导致内存泄漏。

✅ 正确写法：

csharp:

```c#
public override void OnApplyTemplate()
{
    base.OnApplyTemplate();
    
    // 订阅Unloaded事件
    Unloaded += (s, e) =>
    {
        // 停止定时器
        _blinkTimer?.Stop();
        _blinkTimer = null;
        
        // 取消事件订阅
        Device.PLC.IoStateChanged -= OnIoStateChanged;
    };
}
```

### 4. 不要频繁修改 Template 属性

每次修改`Template`属性都会导致控件重新生成可视化树，性能开销很大。如果需要改变控件外观，应该通过修改依赖属性来实现。

### 5. 正确使用 MeasureOverride 和 ArrangeOverride

如果需要实现自定义布局，必须正确实现`MeasureOverride`和`ArrangeOverride`方法：

- 在`MeasureOverride`中，必须调用每个子元素的`Measure`方法
- 返回的大小不能超过传入的 constraint
- 在`ArrangeOverride`中，必须调用每个子元素的`Arrange`方法

### 6. 避免内存泄漏

- 不要在静态变量中引用`Control`实例
- 及时取消事件订阅
- 定时器不再使用时必须停止
- 非托管资源必须实现`IDisposable`接口

------

## 五、总结

`Control`基类是 WPF 控件体系的核心，它提供了所有可视化控件共有的基础功能。在工业上位机项目中，你主要会在以下场景使用它：

1. **开发自定义工业控件**（IO 指示灯、阀门、仪表等）
2. **统一项目界面风格**（通过 Style 和 Template）
3. **动态创建界面元素**（根据 PLC 配置生成 IO 监控界面）
4. **管理控件状态**（设备不同状态下的界面控制）

掌握`Control`基类的核心特性，特别是**控件模板和依赖属性**，是开发高质量 WPF 工业上位机的关键。