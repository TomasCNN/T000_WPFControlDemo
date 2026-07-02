# 008004002_WPF Trigger 触发器基础案例合集

以下 6 个案例覆盖 WPF 最常用的 4 类触发器（属性触发器、数据触发器、多数据触发器、事件触发器），全部贴合工业上位机的按钮交互、设备状态、参数校验、列表展示等真实场景，代码可直接复制运行。

> 前置约定：所有触发器都定义在样式 `Style` 中，遵循「默认值写 Setter，状态变化写 Trigger」的规范。

------

## 案例 1：属性触发器 - 按钮交互三态

### 场景

最基础的入门案例：给按钮添加「鼠标悬停、鼠标按下、控件禁用」三种交互状态，纯 XAML 实现，无需后台事件代码。

### 知识点

- 掌握 `Trigger` 基础语法：监听控件自身依赖属性；
- 理解「条件满足生效，条件消失自动恢复」的特性。

### 完整代码

xaml:

```xaml
<Window x:Class="TriggerDemo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="属性触发器-按钮三态" Height="250" Width="400">

    <Window.Resources>
        <!-- 按钮样式：默认值 + 触发器 -->
        <Style x:Key="OperateButtonStyle" TargetType="Button">
            <!-- 默认状态：静态 Setter -->
            <Setter Property="Width" Value="120"/>
            <Setter Property="Height" Value="36"/>
            <Setter Property="Background" Value="#2E7DFF"/>
            <Setter Property="Foreground" Value="White"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="FontSize" Value="14"/>

            <!-- 触发器集合 -->
            <Style.Triggers>
                <!-- 1. 鼠标悬停状态 -->
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#5597FF"/>
                </Trigger>

                <!-- 2. 鼠标按下状态 -->
                <Trigger Property="IsPressed" Value="True">
                    <Setter Property="Background" Value="#1A66E0"/>
                </Trigger>

                <!-- 3. 控件禁用状态 -->
                <Trigger Property="IsEnabled" Value="False">
                    <Setter Property="Background" Value="#CCCCCC"/>
                    <Setter Property="Foreground" Value="#999999"/>
                    <Setter Property="Cursor" Value="No"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </Window.Resources>

    <StackPanel Orientation="Horizontal" Margin="50" Spacing="20">
        <!-- 正常按钮 -->
        <Button Content="启动设备" Style="{StaticResource OperateButtonStyle}"/>
        <!-- 禁用按钮 -->
        <Button Content="禁用状态" Style="{StaticResource OperateButtonStyle}" IsEnabled="False"/>
    </StackPanel>
</Window>
```

### 核心要点

1. **自动恢复**：只需要写满足条件的触发器，鼠标移开、按钮松开后，属性会自动恢复到 Setter 的默认值，不需要写反向触发器；
2. **避坑提醒**：不要在按钮上直接写 `Background="Red"` 这类本地值，本地值优先级高于触发器，会导致触发器失效。

------

## 案例 2：数据触发器 - 设备状态指示灯

### 场景

工业场景最经典用法：根据 ViewModel 中的设备运行、告警属性，自动切换指示灯颜色，完全符合 MVVM 架构，数据驱动 UI。

### 知识点

- 掌握 `DataTrigger` 语法：绑定业务数据源属性；
- 理解触发器顺序与优先级：后写的优先级更高。

### 完整代码

#### 1. 视图模型（提供数据）

csharp:

```c#
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class DeviceViewModel : INotifyPropertyChanged
{
    private bool _isRunning = true;
    /// <summary> 设备是否运行 </summary>
    public bool IsRunning
    {
        get => _isRunning;
        set { _isRunning = value; OnPropertyChanged(); }
    }

    private bool _isAlarming;
    /// <summary> 设备是否告警 </summary>
    public bool IsAlarming
    {
        get => _isAlarming;
        set { _isAlarming = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

#### 2. XAML 界面

xaml:

```xaml
<Window x:Class="TriggerDemo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:TriggerDemo"
        Title="数据触发器-状态指示灯" Height="250" Width="450">

    <Window.Resources>
        <!-- 指示灯样式：数据驱动颜色 -->
        <Style x:Key="StatusLightStyle" TargetType="Ellipse">
            <!-- 默认：离线灰色 -->
            <Setter Property="Width" Value="24"/>
            <Setter Property="Height" Value="24"/>
            <Setter Property="Fill" Value="Gray"/>

            <Style.Triggers>
                <!-- 运行中 → 绿色 -->
                <DataTrigger Binding="{Binding IsRunning}" Value="True">
                    <Setter Property="Fill" Value="LimeGreen"/>
                </DataTrigger>

                <!-- 告警中 → 橙色（写在后面，优先级高于运行状态） -->
                <DataTrigger Binding="{Binding IsAlarming}" Value="True">
                    <Setter Property="Fill" Value="Orange"/>
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Window.Resources>

    <Window.DataContext>
        <local:DeviceViewModel/>
    </Window.DataContext>

    <StackPanel Margin="40" Spacing="20">
        <!-- 状态指示灯 + 文本 -->
        <StackPanel Orientation="Horizontal" Spacing="12">
            <Ellipse Style="{StaticResource StatusLightStyle}"/>
            <TextBlock Text="设备运行状态" FontSize="16" VerticalAlignment="Center"/>
        </StackPanel>

        <!-- 切换开关，验证数据驱动UI -->
        <CheckBox Content="切换运行状态" IsChecked="{Binding IsRunning, Mode=TwoWay}" FontSize="14"/>
        <CheckBox Content="切换告警状态" IsChecked="{Binding IsAlarming, Mode=TwoWay}" FontSize="14"/>
    </StackPanel>
</Window>
```

### 核心要点

1. **数据驱动**：修改 ViewModel 的属性，UI 自动变色，全程不操作控件；
2. **优先级规则**：多个触发器同时满足时，写在后面的覆盖前面的。所以高优先级的告警状态放在最后；
3. **默认值兜底**：所有状态都不满足时，显示 Setter 定义的灰色（离线）。

------

## 案例 3：多数据触发器 - 组合状态判断

### 场景

只有「设备运行中 + 存在告警」两个条件同时满足时，才显示红色紧急状态；单独运行、单独告警都不触发，用于复杂业务状态组合。

### 知识点

- 掌握 `MultiDataTrigger` 语法：多条件「与逻辑」同时满足才触发。

### 完整代码

xaml:

```xaml
<Window.Resources>
    <Style x:Key="AlarmLightStyle" TargetType="Ellipse">
        <Setter Property="Width" Value="24"/>
        <Setter Property="Height" Value="24"/>
        <Setter Property="Fill" Value="Gray"/>

        <Style.Triggers>
            <!-- 单条件：仅运行 → 绿色 -->
            <DataTrigger Binding="{Binding IsRunning}" Value="True">
                <Setter Property="Fill" Value="LimeGreen"/>
            </DataTrigger>

            <!-- 多条件：运行 + 告警，同时满足才显示红色 -->
            <MultiDataTrigger>
                <MultiDataTrigger.Conditions>
                    <Condition Binding="{Binding IsRunning}" Value="True"/>
                    <Condition Binding="{Binding IsAlarming}" Value="True"/>
                </MultiDataTrigger.Conditions>
                <Setter Property="Fill" Value="Red"/>
                <Setter Property="Stroke" Value="DarkRed"/>
                <Setter Property="StrokeThickness" Value="1"/>
            </MultiDataTrigger>
        </Style.Triggers>
    </Style>
</Window.Resources>
```

### 核心要点

1. **与逻辑**：所有 Condition 全部满足，触发器才会生效；
2. **适用场景**：权限校验、状态组合、多参数联动等复杂 UI 逻辑。

------

## 案例 4：属性触发器 - 输入框校验状态

### 场景

结合 WPF 校验机制，当输入框存在校验错误时，自动显示红色边框和错误提示，统一所有输入框的校验样式。

### 知识点

- 监听附加属性 `Validation.HasError`；
- 掌握触发器中使用相对源绑定。

### 完整代码

xaml:

```xaml
<Window.Resources>
    <!-- 全局文本框样式：校验状态自动变色 -->
    <Style TargetType="TextBox">
        <Setter Property="Width" Value="250"/>
        <Setter Property="Height" Value="32"/>
        <Setter Property="Padding" Value="6 4"/>
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="BorderBrush" Value="#CCCCCC"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="VerticalContentAlignment" Value="Center"/>

        <Style.Triggers>
            <!-- 获得焦点：边框变主题色 -->
            <Trigger Property="IsFocused" Value="True">
                <Setter Property="BorderBrush" Value="#2E7DFF"/>
            </Trigger>

            <!-- 校验错误：边框变红，Tooltip显示错误信息 -->
            <Trigger Property="Validation.HasError" Value="True">
                <Setter Property="BorderBrush" Value="Red"/>
                <Setter Property="ToolTip"
                        Value="{Binding RelativeSource={RelativeSource Self},
                                Path=(Validation.Errors)[0].ErrorContent}"/>
            </Trigger>
        </Style.Triggers>
    </Style>
</Window.Resources>

<StackPanel Margin="40" Spacing="15">
    <TextBlock Text="曝光时间（100~10000μs）："/>
    <TextBox Text="{Binding ExposureTime, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}">
        <TextBox.Text>
            <Binding Path="ExposureTime" Mode="TwoWay" UpdateSourceTrigger="PropertyChanged">
                <Binding.ValidationRules>
                    <local:DoubleRangeValidationRule Min="100" Max="10000"/>
                </Binding.ValidationRules>
            </Binding>
        </TextBox.Text>
    </TextBox>
</StackPanel>
```

### 核心要点

1. **附加属性监听**：触发器不仅能监听控件自身属性，也能监听 `Validation.HasError` 这类附加属性；
2. **相对源绑定**：通过 `RelativeSource Self` 绑定控件自身的错误信息，实现通用样式。

------

## 案例 5：列表容器触发器 - 行状态高亮

### 场景

自定义列表行的悬停、选中效果，以及根据行数据状态自动标色，是缺陷列表、设备列表等工业场景的高频用法。

### 知识点

- `ItemContainerStyle` 中使用触发器；
- 列表行的数据触发器，绑定单条数据的属性。

### 完整代码

xaml:

```xaml
<Window.Resources>
    <!-- 列表项容器样式：控制每一行的交互+数据状态 -->
    <Style x:Key="DefectListItemStyle" TargetType="ListBoxItem">
        <Setter Property="Padding" Value="10 6"/>
        <Setter Property="Foreground" Value="#333"/>
        <Setter Property="Background" Value="Transparent"/>

        <Style.Triggers>
            <!-- 1. 属性触发器：鼠标悬停 -->
            <Trigger Property="IsMouseOver" Value="True">
                <Setter Property="Background" Value="#F0F7FF"/>
            </Trigger>

            <!-- 2. 属性触发器：选中状态 -->
            <Trigger Property="IsSelected" Value="True">
                <Setter Property="Background" Value="#E6F0FF"/>
                <Setter Property="Foreground" Value="#2E7DFF"/>
                <Setter Property="FontWeight" Value="Bold"/>
            </Trigger>

            <!-- 3. 数据触发器：严重缺陷自动标红 -->
            <DataTrigger Binding="{Binding IsCritical}" Value="True">
                <Setter Property="Foreground" Value="Red"/>
                <Setter Property="Background" Value="#FFF1F0"/>
            </DataTrigger>
        </Style.Triggers>
    </Style>
</Window.Resources>

<ListBox Width="300" Height="180" Margin="40"
         ItemsSource="{Binding DefectList}"
         ItemContainerStyle="{StaticResource DefectListItemStyle}"
         DisplayMemberPath="DefectName"/>
```

### 配套数据类

csharp:

```c#
public class DefectInfo
{
    public string DefectName { get; set; }
    public bool IsCritical { get; set; }
}
```

### 核心要点

1. **容器样式**：列表行的触发器要写在 `ItemContainerStyle` 中，作用于行容器 `ListBoxItem`；
2. **混合使用**：属性触发器处理交互状态，数据触发器处理业务状态，各司其职。

------

## 案例 6：事件触发器 - 悬停放大动效

### 场景

监听控件的路由事件，触发平滑动画，实现按钮悬停放大的过渡效果，提升界面交互质感。

### 知识点

- 掌握 `EventTrigger` 基础用法；
- 理解事件触发器只能执行动画，不能直接设置属性。

### 完整代码

xaml:

```xaml
<Window.Resources>
    <Style x:Key="ScaleButtonStyle" TargetType="Button">
        <Setter Property="Width" Value="120"/>
        <Setter Property="Height" Value="36"/>
        <Setter Property="Background" Value="#2E7DFF"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Cursor" Value="Hand"/>
        <!-- 开启缩放变换，设置变换原点为中心 -->
        <Setter Property="RenderTransformOrigin" Value="0.5 0.5"/>
        <Setter Property="RenderTransform">
            <Setter.Value>
                <ScaleTransform ScaleX="1" ScaleY="1"/>
            </Setter.Value>
        </Setter>

        <Style.Triggers>
            <!-- 事件触发器：鼠标进入，放大到1.05倍 -->
            <EventTrigger RoutedEvent="MouseEnter">
                <BeginStoryboard>
                    <Storyboard Duration="0:0:0.2">
                        <DoubleAnimation To="1.05" 
                                         Storyboard.TargetProperty="RenderTransform.ScaleX"/>
                        <DoubleAnimation To="1.05" 
                                         Storyboard.TargetProperty="RenderTransform.ScaleY"/>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>

            <!-- 事件触发器：鼠标离开，恢复原大小 -->
            <EventTrigger RoutedEvent="MouseLeave">
                <BeginStoryboard>
                    <Storyboard Duration="0:0:0.2">
                        <DoubleAnimation To="1" 
                                         Storyboard.TargetProperty="RenderTransform.ScaleX"/>
                        <DoubleAnimation To="1" 
                                         Storyboard.TargetProperty="RenderTransform.ScaleY"/>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Style.Triggers>
    </Style>
</Window.Resources>

<Button Content="悬停放大" Style="{StaticResource ScaleButtonStyle}" Margin="50"/>
```

### 核心要点

1. **无自动恢复**：和属性触发器不同，事件触发器不会自动恢复，需要分别写进入和离开两个事件的动画；
2. **职责单一**：事件触发器专门负责动画、过渡效果，属性修改优先用属性 / 数据触发器。

------

## 基础总结与选型

| 触发器类型         | 触发源       | 适用场景                          |
| :----------------- | :----------- | :-------------------------------- |
| `Trigger`          | 控件自身属性 | 悬停、按下、禁用、聚焦等交互状态  |
| `DataTrigger`      | 业务数据属性 | 设备状态、权限、等级等数据驱动 UI |
| `MultiDataTrigger` | 多个业务属性 | 多条件同时满足的组合状态          |
| `EventTrigger`     | 路由事件     | 动画、过渡动效                    |

### 通用避坑提醒

1. **本地值覆盖**：控件上直接写的属性优先级高于触发器，会导致触发器失效；
2. **自动恢复**：属性 / 数据触发器条件消失自动恢复，不要写反向逻辑；
3. **顺序优先级**：后定义的触发器优先级更高，高优先级状态放最后。