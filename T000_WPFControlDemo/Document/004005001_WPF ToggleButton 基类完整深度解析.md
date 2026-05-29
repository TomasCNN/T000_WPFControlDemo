# 004005001_WPF ToggleButton 基类完整深度解析

`ToggleButton`是**开关式按钮的共同基类**，继承自`ButtonBase`，在普通按钮的基础上增加了**状态保持能力**—— 点击后不会自动弹起，会保持选中 / 未选中状态，还支持第三种不确定状态。你每天都在使用的`CheckBox`、`RadioButton`全部继承自`ToggleButton`。

------

## 一、ToggleButton 在 WPF 类层次结构中的位置

plaintext

```tex
System.Object
  ↳ System.Windows.Threading.DispatcherObject
    ↳ System.Windows.DependencyObject
      ↳ System.Windows.Media.Visual
        ↳ System.Windows.UIElement
          ↳ System.Windows.FrameworkElement
            ↳ System.Windows.Controls.Control
              ↳ System.Windows.Controls.ContentControl
                ↳ System.Windows.Controls.Primitives.ButtonBase
                  ↳ System.Windows.Controls.Primitives.ToggleButton  ← 我们今天的主角
                    ↳ System.Windows.Controls.CheckBox
                    ↳ System.Windows.Controls.RadioButton
```

**核心区别**：

- `Button`：一次性触发按钮，点击后立即弹起，无状态保持
- `ToggleButton`：状态保持按钮，点击后切换状态并保持，支持双状态 / 三状态
- `CheckBox`：复选框，继承自 ToggleButton，支持多选
- `RadioButton`：单选框，继承自 ToggleButton，支持互斥单选

------

## 二、完整官方类定义（.NET 8）

csharp:

```c#
using System.Windows.Automation.Peers;
using System.Windows.Input;

namespace System.Windows.Controls.Primitives
{
    /// <summary>
    /// 表示可以切换选中/未选中状态的按钮基类
    /// </summary>
    /// <remarks>
    /// ToggleButton 是所有开关式控件的基类，支持双状态（选中/未选中）和三状态（增加不确定状态）。
    /// 每次点击会切换 IsChecked 属性的值，并触发对应的路由事件。
    /// </remarks>
    [DefaultEvent("Checked")]
    [Localizability(LocalizationCategory.Button)]
    public class ToggleButton : ButtonBase
    {
        // ==============================================
        // 核心依赖属性（ToggleButton特有）
        // ==============================================
        public static readonly DependencyProperty IsCheckedProperty;
        public static readonly DependencyProperty IsThreeStateProperty;

        // ==============================================
        // 构造函数
        // ==============================================
        public ToggleButton();

        // ==============================================
        // 公共属性
        // ==============================================
        [Bindable(true)]
        [Category("Appearance")]
        [TypeConverter(typeof(NullableBoolConverter))]
        public bool? IsChecked { get; set; }

        [Bindable(true)]
        [Category("Behavior")]
        public bool IsThreeState { get; set; }

        // ==============================================
        // 核心路由事件
        // ==============================================
        public static readonly RoutedEvent CheckedEvent;
        public static readonly RoutedEvent UncheckedEvent;
        public static readonly RoutedEvent IndeterminateEvent;

        public event RoutedEventHandler Checked
        {
            add => AddHandler(CheckedEvent, value);
            remove => RemoveHandler(CheckedEvent, value);
        }

        public event RoutedEventHandler Unchecked
        {
            add => AddHandler(UncheckedEvent, value);
            remove => RemoveHandler(UncheckedEvent, value);
        }

        public event RoutedEventHandler Indeterminate
        {
            add => AddHandler(IndeterminateEvent, value);
            remove => RemoveHandler(IndeterminateEvent, value);
        }

        // ==============================================
        // 受保护方法（派生类可重写）
        // ==============================================
        protected override AutomationPeer OnCreateAutomationPeer();
        protected override void OnClick();
        protected virtual void OnChecked(RoutedEventArgs e);
        protected virtual void OnUnchecked(RoutedEventArgs e);
        protected virtual void OnIndeterminate(RoutedEventArgs e);
        protected internal override void OnToggle();
    }
}
```

------

## 三、核心特性逐行解析

### 3.1 类级特性

csharp:

```c#
[DefaultEvent("Checked")]
[Localizability(LocalizationCategory.Button)]
```

- **`[DefaultEvent("Checked")]`**：指定`Checked`为默认事件，在 Visual Studio 设计器中双击 ToggleButton 会自动生成`Checked`事件处理方法
- **`[Localizability(LocalizationCategory.Button)]`**：本地化特性，告诉本地化工具该类属于按钮类别

### 3.2 ToggleButton 特有依赖属性

这两个属性是 ToggleButton 的核心，也是它区别于普通 Button 的关键。

#### 1. `IsCheckedProperty`（选中状态）

csharp:

```c#
public static readonly DependencyProperty IsCheckedProperty;
public bool? IsChecked { get; set; }
```

- **类型**：`bool?`（可空布尔值），这是 ToggleButton 最特殊的设计

- **可选值**：

  - `true`：选中状态
  - `false`：未选中状态（默认）
  - `null`：不确定状态（仅当`IsThreeState="True"`时有效）

  

- **工业场景应用**：

  - 设备启停开关：true = 运行，false = 停止
  - 自动 / 手动模式：true = 自动，false = 手动
  - 报警静音开关：true = 静音，false = 正常
  - 设备状态：null = 部分运行 / 不确定

  

#### 2. `IsThreeStateProperty`（三状态开关）

csharp:

```c#
public static readonly DependencyProperty IsThreeStateProperty;
public bool IsThreeState { get; set; }
```

- **类型**：`bool`

- **作用**：控制是否启用第三种不确定状态

- **默认值**：`false`（仅支持双状态）

- **工业场景应用**：

  - 多设备总开关：true = 全部运行，false = 全部停止，null = 部分运行
  - 批量选择：true = 全选，false = 全不选，null = 部分选
  - 设备健康状态：true = 正常，false = 故障，null = 未知

  

### 3.3 核心路由事件

ToggleButton 有三个专属路由事件，分别对应三种状态变化：

| 事件            | 触发时机                       | 对应状态   |
| :-------------- | :----------------------------- | :--------- |
| `Checked`       | 当`IsChecked`变为`true`时触发  | 选中状态   |
| `Unchecked`     | 当`IsChecked`变为`false`时触发 | 未选中状态 |
| `Indeterminate` | 当`IsChecked`变为`null`时触发  | 不确定状态 |

**重要区别**：

- 继承自`ButtonBase`的`Click`事件：**每次点击都会触发**，无论状态是否变化
- `Checked/Unchecked/Indeterminate`事件：**只有当状态发生变化时才会触发**

### 3.4 核心受保护方法

#### 1. `OnClick()`

csharp:

```c#
protected override void OnClick();
```

- 重写了`ButtonBase`的`OnClick`方法
- 点击时自动调用`OnToggle()`方法切换状态
- 自定义 ToggleButton 时，重写此方法必须调用`base.OnClick()`

#### 2. `OnToggle()`

csharp:

```c#
protected internal virtual void OnToggle();
```

- ToggleButton 的核心方法，负责切换`IsChecked`属性的值
- 双状态下切换顺序：`false` → `true` → `false` → ...
- 三状态下切换顺序：`false` → `true` → `null` → `false` → ...
- 派生类可以重写此方法自定义切换逻辑

#### 3. 状态变化通知方法

csharp:

```c#
protected virtual void OnChecked(RoutedEventArgs e);
protected virtual void OnUnchecked(RoutedEventArgs e);
protected virtual void OnIndeterminate(RoutedEventArgs e);
```

- 当对应状态变化时调用，负责触发对应的路由事件
- 派生类可以重写这些方法，在状态变化时执行自定义逻辑

------

## 四、ToggleButton 的核心工作原理

当用户点击 ToggleButton 时，WPF 会按照以下流程执行：

1. 触发`MouseLeftButtonDown`事件
2. 设置`IsPressed`为`true`
3. 捕获鼠标
4. 触发`MouseLeftButtonUp`事件
5. 根据`ClickMode`设置触发`Click`事件
6. 调用`OnToggle()`方法切换`IsChecked`属性的值
7. 根据新的`IsChecked`值触发对应的`Checked/Unchecked/Indeterminate`事件
8. 执行`Command`命令（如果设置了）
9. 释放鼠标捕获
10. 设置`IsPressed`为`false`

------

## 五、工业上位机典型使用方法与实例

### 5.1 基础双状态用法（最常用）

工业场景中 90% 的情况都使用双状态 ToggleButton，用于表示两种互斥的状态。

#### 实例 1：自动 / 手动模式切换开关

##### XAML 代码

xaml:

```xaml
<StackPanel Orientation="Horizontal" VerticalAlignment="Center">
    <TextBlock Text="运行模式：" Margin="0,0,10,0"/>
    <ToggleButton x:Name="ModeToggleButton" 
                  Content="手动"
                  Width="100"
                  Height="32"
                  Checked="ModeToggleButton_Checked"
                  Unchecked="ModeToggleButton_Unchecked"/>
</StackPanel>
```

##### 后台代码

csharp:

```c#
private void ModeToggleButton_Checked(object sender, RoutedEventArgs e)
{
    // 切换到自动模式
    ModeToggleButton.Content = "自动";
    Device.PLC.SetOperationMode(OperationMode.Auto);
    Logger.LogCommucation.Info("切换到自动运行模式");
}

private void ModeToggleButton_Unchecked(object sender, RoutedEventArgs e)
{
    // 切换到手动模式
    ModeToggleButton.Content = "手动";
    Device.PLC.SetOperationMode(OperationMode.Manual);
    Logger.LogCommucation.Info("切换到手动运行模式");
}
```

#### 实例 2：报警静音开关

xaml:

```xaml
<ToggleButton x:Name="AlarmMuteToggleButton"
              Content="报警静音"
              Width="100"
              Height="32"
              Checked="AlarmMuteToggleButton_Checked"
              Unchecked="AlarmMuteToggleButton_Unchecked"/>
```

csharp:

```c#
private void AlarmMuteToggleButton_Checked(object sender, RoutedEventArgs e)
{
    AlarmManager.MuteAllAlarms();
    AlarmMuteToggleButton.Content = "取消静音";
}

private void AlarmMuteToggleButton_Unchecked(object sender, RoutedEventArgs e)
{
    AlarmManager.UnmuteAllAlarms();
    AlarmMuteToggleButton.Content = "报警静音";
}
```

### 5.2 MVVM 命令绑定（工业项目标准用法）

工业项目中强烈推荐使用 MVVM 模式，将状态与 UI 分离。

#### ViewModel 代码

csharp:

```c#
using Prism.Commands;
using Prism.Mvvm;

public class MainViewModel : BindableBase
{
    // 运行模式状态
    private bool _isAutoMode = false;
    public bool IsAutoMode
    {
        get => _isAutoMode;
        set
        {
            if (SetProperty(ref _isAutoMode, value))
            {
                // 状态变化时执行逻辑
                if (value)
                {
                    Device.PLC.SetOperationMode(OperationMode.Auto);
                    Logger.LogCommucation.Info("切换到自动运行模式");
                }
                else
                {
                    Device.PLC.SetOperationMode(OperationMode.Manual);
                    Logger.LogCommucation.Info("切换到手动运行模式");
                }
            }
        }
    }

    // 报警静音状态
    private bool _isAlarmMuted = false;
    public bool IsAlarmMuted
    {
        get => _isAlarmMuted;
        set
        {
            if (SetProperty(ref _isAlarmMuted, value))
            {
                if (value)
                {
                    AlarmManager.MuteAllAlarms();
                }
                else
                {
                    AlarmManager.UnmuteAllAlarms();
                }
            }
        }
    }
}
```

#### XAML 代码

xaml:

```xaml
<StackPanel Orientation="Horizontal" VerticalAlignment="Center">
    <TextBlock Text="运行模式：" Margin="0,0,10,0"/>
    <ToggleButton Width="100"
                  Height="32"
                  IsChecked="{Binding IsAutoMode}"
                  Content="{Binding IsAutoMode, Converter={StaticResource BoolToModeConverter}}"/>
</StackPanel>

<StackPanel Orientation="Horizontal" VerticalAlignment="Center" Margin="20,0,0,0">
    <ToggleButton Width="100"
                  Height="32"
                  IsChecked="{Binding IsAlarmMuted}"
                  Content="{Binding IsAlarmMuted, Converter={StaticResource BoolToMuteConverter}}"/>
</StackPanel>
```

#### 值转换器（BoolToModeConverter.cs）

csharp:

```c#
using System;
using System.Globalization;
using System.Windows.Data;

namespace IndustrialVisionTemplate.Converters
{
    public class BoolToModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "自动" : "手动";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
```

### 5.3 三状态用法（高级）

当需要表示 "全部 / 部分 / 都不" 三种状态时，使用三状态 ToggleButton。

#### 实例：多相机总开关

##### XAML 代码

xaml:

```xaml
<ToggleButton x:Name="AllCamerasToggleButton"
              Content="全部相机"
              Width="120"
              Height="32"
              IsThreeState="True"
              Checked="AllCamerasToggleButton_Checked"
              Unchecked="AllCamerasToggleButton_Unchecked"
              Indeterminate="AllCamerasToggleButton_Indeterminate"/>
```

##### 后台代码

csharp:

```c#
private void AllCamerasToggleButton_Checked(object sender, RoutedEventArgs e)
{
    // 启动所有相机
    foreach (var camera in CameraManager.Cameras)
    {
        camera.Start();
    }
    AllCamerasToggleButton.Content = "全部运行";
}

private void AllCamerasToggleButton_Unchecked(object sender, RoutedEventArgs e)
{
    // 停止所有相机
    foreach (var camera in CameraManager.Cameras)
    {
        camera.Stop();
    }
    AllCamerasToggleButton.Content = "全部停止";
}

private void AllCamerasToggleButton_Indeterminate(object sender, RoutedEventArgs e)
{
    // 部分运行状态，不执行操作
    AllCamerasToggleButton.Content = "部分运行";
}

// 当单个相机状态变化时更新总开关状态
private void Camera_StateChanged(object sender, CameraStateEventArgs e)
{
    int runningCount = CameraManager.Cameras.Count(c => c.IsRunning);
    int totalCount = CameraManager.Cameras.Count;

    if (runningCount == totalCount)
    {
        AllCamerasToggleButton.IsChecked = true;
    }
    else if (runningCount == 0)
    {
        AllCamerasToggleButton.IsChecked = false;
    }
    else
    {
        AllCamerasToggleButton.IsChecked = null;
    }
}
```

### 5.4 自定义样式 ToggleButton（工业开关样式）

工业场景中经常需要将 ToggleButton 做成开关样式，更直观地表示状态。

#### XAML 样式代码

xaml:

```xaml
<Style x:Key="ToggleSwitchStyle" TargetType="ToggleButton">
    <Setter Property="Width" Value="60"/>
    <Setter Property="Height" Value="30"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="ToggleButton">
                <Grid>
                    <!-- 背景 -->
                    <Border x:Name="BackgroundBorder"
                            Width="60"
                            Height="30"
                            Background="#BDBDBD"
                            CornerRadius="15"/>
                    
                    <!-- 滑块 -->
                    <Ellipse x:Name="Slider"
                             Width="26"
                             Height="26"
                             Fill="White"
                             HorizontalAlignment="Left"
                             Margin="2,2,0,2">
                        <Ellipse.Effect>
                            <DropShadowEffect ShadowDepth="1" BlurRadius="2" Opacity="0.3"/>
                        </Ellipse.Effect>
                    </Ellipse>
                </Grid>
                
                <ControlTemplate.Triggers>
                    <Trigger Property="IsChecked" Value="True">
                        <Setter TargetName="BackgroundBorder" Property="Background" Value="#4CAF50"/>
                        <Setter TargetName="Slider" Property="HorizontalAlignment" Value="Right"/>
                    </Trigger>
                    
                    <Trigger Property="IsChecked" Value="{x:Null}">
                        <Setter TargetName="BackgroundBorder" Property="Background" Value="#FF9800"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

#### 使用方法

xaml:

```xaml
<StackPanel Orientation="Horizontal" VerticalAlignment="Center">
    <TextBlock Text="自动模式：" Margin="0,0,10,0"/>
    <ToggleButton Style="{StaticResource ToggleSwitchStyle}"
                  IsChecked="{Binding IsAutoMode}"/>
</StackPanel>
```

### 5.5 派生类用法（CheckBox/RadioButton）

`CheckBox`和`RadioButton`是 ToggleButton 最常用的两个派生类，在工业项目中广泛使用。

#### CheckBox（复选框）

用于多选场景：

xaml:

```xaml
<GroupBox Header="报警类型">
    <StackPanel>
        <CheckBox Content="设备故障报警" IsChecked="{Binding IsDeviceAlarmEnabled}"/>
        <CheckBox Content="参数超限报警" IsChecked="{Binding IsParameterAlarmEnabled}"/>
        <CheckBox Content="通讯异常报警" IsChecked="{Binding IsCommunicationAlarmEnabled}"/>
    </StackPanel>
</GroupBox>
```

#### RadioButton（单选框）

用于互斥单选场景：

xaml:

```xaml
<GroupBox Header="触发模式">
    <StackPanel>
        <RadioButton Content="软件触发" IsChecked="{Binding TriggerMode, Converter={StaticResource EnumToBoolConverter}, ConverterParameter=Software}"/>
        <RadioButton Content="硬件触发" IsChecked="{Binding TriggerMode, Converter={StaticResource EnumToBoolConverter}, ConverterParameter=Hardware}"/>
        <RadioButton Content="连续触发" IsChecked="{Binding TriggerMode, Converter={StaticResource EnumToBoolConverter}, ConverterParameter=Continuous}"/>
    </StackPanel>
</GroupBox>
```

------

## 六、与 Button 的区别与选型建议

| 特性     | Button                             | ToggleButton                                |
| :------- | :--------------------------------- | :------------------------------------------ |
| 状态保持 | 无，点击后立即弹起                 | 有，保持选中 / 未选中状态                   |
| 触发方式 | 一次性触发                         | 状态切换触发                                |
| 核心事件 | Click                              | Checked/Unchecked/Indeterminate             |
| 核心属性 | Content/Command                    | IsChecked/IsThreeState                      |
| 适用场景 | 一次性操作：启动、停止、复位、确认 | 状态切换：自动 / 手动、开 / 关、静音 / 正常 |

**工业场景选型原则**：

- ✅ 用 Button：所有一次性操作，点击后执行一个动作
- ✅ 用 ToggleButton：所有需要保持状态的开关操作
- ❌ 不要用 ToggleButton 做危险操作：急停、删除、重置等，这些操作必须用带二次确认的 Button

------

## 七、最佳实践与常见问题

### 7.1 最佳实践

1. **优先使用 MVVM 绑定**：将`IsChecked`绑定到 ViewModel 的属性，不要在后台代码中直接操作控件
2. **明确状态含义**：每个状态的含义必须清晰，特别是三状态的不确定状态
3. **记录状态变化日志**：所有 ToggleButton 的状态变化都必须记录日志，便于问题追溯
4. **使用合适的样式**：工业场景推荐使用开关样式或明确的文字提示，避免用户混淆
5. **危险操作禁用 ToggleButton**：急停、删除等危险操作必须使用带二次确认的 Button

### 7.2 常见问题与解决方案

#### 问题 1：点击 ToggleButton 没有反应

**排查步骤**：

1. 检查是否设置了`IsEnabled="False"`
2. 检查是否有其他控件覆盖了 ToggleButton
3. 检查是否订阅了`PreviewMouseDown`事件并设置了`e.Handled = true`
4. 检查 MVVM 绑定是否正确，DataContext 是否设置

#### 问题 2：三状态切换不正常

**排查步骤**：

1. 确认`IsThreeState`属性设置为`true`
2. 确认`IsChecked`属性的类型是`bool?`而不是`bool`
3. 检查是否在代码中手动修改了`IsChecked`的值，覆盖了自动切换逻辑

#### 问题 3：Checked 事件触发多次

**排查步骤**：

1. 检查是否同时订阅了`Click`和`Checked`事件
2. 检查是否在`Checked`事件处理程序中修改了`IsChecked`的值，导致循环触发
3. 检查是否有多个地方绑定了同一个属性

------

## 八、官方学习资源

### 微软官方文档

- [ToggleButton 类官方参考](https://learn.microsoft.com/zh-cn/dotnet/api/system.windows.controls.primitives.togglebutton)
- [CheckBox 类官方参考](https://learn.microsoft.com/zh-cn/dotnet/api/system.windows.controls.checkbox)
- [RadioButton 类官方参考](https://learn.microsoft.com/zh-cn/dotnet/api/system.windows.controls.radiobutton)

### 中文精品教程

- [WPF 中文网：ToggleButton 控件详解](https://www.wpfsoft.com/2023/08/23/1201.html)
- [CSDN：WPF ToggleButton 用法大全](https://blog.csdn.net/qq_38225558/article/details/121678934)

------

## 总结

`ToggleButton`是 WPF 中所有开关式控件的基类，它在普通按钮的基础上增加了状态保持能力，支持双状态和三状态切换。在工业上位机项目中，你会在以下场景频繁使用它：

1. 运行模式切换（自动 / 手动）
2. 功能开关（报警静音、日志记录）
3. 批量操作（全选 / 全不选）
4. 设备状态显示（运行 / 停止 / 未知）
5. 派生类 CheckBox 和 RadioButton 用于多选和单选

掌握`ToggleButton`的核心特性，特别是`IsChecked`属性和状态事件的使用，是开发高质量工业界面的关键。