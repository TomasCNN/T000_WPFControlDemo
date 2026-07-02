# 008006002_WPF `DataTrigger` 数据触发器基础案例合集

以下 5 个案例覆盖 DataTrigger 最核心的基础用法，全部贴合工业上位机的设备状态、缺陷列表、权限控制等真实场景，代码可直接复制运行，遵循 MVVM 架构，纯声明式实现数据驱动 UI。

> 前置约定：所有数据源均实现 `INotifyPropertyChanged` 接口，保证数据变化时 UI 自动同步；默认值写在静态 `Setter` 中，触发器负责状态切换。

------

## 案例 1：布尔值驱动设备状态指示灯（入门必学）

### 应用场景

最基础的入门用法：根据 ViewModel 中的布尔属性 `IsRunning`，自动切换指示灯的灰 / 绿状态，对应设备离线 / 运行两种基础状态。

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

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

#### 2. XAML 样式与界面

xaml:

```xaml
<Window x:Class="DataTriggerDemo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:DataTriggerDemo"
        Title="案例1：布尔值状态灯" Height="250" Width="400">

    <Window.Resources>
        <!-- 指示灯样式：默认灰色（离线） -->
        <Style x:Key="StatusLightStyle" TargetType="Ellipse">
            <Setter Property="Width" Value="24"/>
            <Setter Property="Height" Value="24"/>
            <Setter Property="Fill" Value="Gray"/>

            <Style.Triggers>
                <!-- 数据触发器：IsRunning=True 时变绿 -->
                <DataTrigger Binding="{Binding IsRunning}" Value="True">
                    <Setter Property="Fill" Value="LimeGreen"/>
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Window.Resources>

    <Window.DataContext>
        <local:DeviceViewModel/>
    </Window.DataContext>

    <StackPanel Margin="40" Spacing="20">
        <!-- 状态指示灯 -->
        <StackPanel Orientation="Horizontal" Spacing="12">
            <Ellipse Style="{StaticResource StatusLightStyle}"/>
            <TextBlock Text="设备运行状态" FontSize="16" VerticalAlignment="Center"/>
        </StackPanel>

        <!-- 切换开关，验证数据驱动UI -->
        <CheckBox Content="切换设备运行状态" 
                  IsChecked="{Binding IsRunning, Mode=TwoWay}" 
                  FontSize="14"/>
    </StackPanel>
</Window>
```

### 核心要点

1. **数据驱动**：修改 ViewModel 的 `IsRunning` 属性，指示灯自动变色，全程不操作控件；
2. **自动恢复**：条件不满足时自动回到 Setter 的默认灰色，不需要写反向触发器；
3. **基础前提**：数据源必须实现 `INotifyPropertyChanged`，否则数据变化 UI 不会更新。

------

## 案例 2：枚举状态多分支映射（工业标准用法）

### 应用场景

工业设备标准的多状态场景：离线、待机、运行、告警、故障 5 种状态对应不同颜色，通过枚举值触发不同视觉效果，是状态指示灯的完整实现方案。

### 完整代码

#### 1. 枚举与视图模型

csharp:

```c#
/// <summary> 设备状态枚举 </summary>
public enum DeviceStatus
{
    Offline,   // 离线
    Standby,   // 待机
    Running,   // 运行
    Alarm,     // 告警
    Fault      // 故障
}

public class DeviceViewModel : INotifyPropertyChanged
{
    private DeviceStatus _status = DeviceStatus.Standby;
    public DeviceStatus Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }
}
```

#### 2. XAML 样式

xaml:

```xaml
<Window.Resources>
    <Style x:Key="StatusLightStyle" TargetType="Ellipse">
        <Setter Property="Width" Value="24"/>
        <Setter Property="Height" Value="24"/>
        <Setter Property="Fill" Value="Gray"/> <!-- 默认：离线灰色 -->

        <Style.Triggers>
            <!-- 待机：黄色 -->
            <DataTrigger Binding="{Binding Status}" 
                         Value="{x:Static local:DeviceStatus.Standby}">
                <Setter Property="Fill" Value="Yellow"/>
            </DataTrigger>

            <!-- 运行：绿色（优先级高于待机） -->
            <DataTrigger Binding="{Binding Status}" 
                         Value="{x:Static local:DeviceStatus.Running}">
                <Setter Property="Fill" Value="LimeGreen"/>
            </DataTrigger>

            <!-- 告警：橙色 -->
            <DataTrigger Binding="{Binding Status}" 
                         Value="{x:Static local:DeviceStatus.Alarm}">
                <Setter Property="Fill" Value="Orange"/>
            </DataTrigger>

            <!-- 故障：红色（最高优先级，放最后） -->
            <DataTrigger Binding="{Binding Status}" 
                         Value="{x:Static local:DeviceStatus.Fault}">
                <Setter Property="Fill" Value="Red"/>
                <Setter Property="Stroke" Value="DarkRed"/>
                <Setter Property="StrokeThickness" Value="1"/>
            </DataTrigger>
        </Style.Triggers>
    </Style>
</Window.Resources>
```

### 核心要点

1. **枚举推荐用 `x:Static`**：显式指定类型，避免字符串隐式转换失败导致触发器不生效；
2. **顺序决定优先级**：多个触发器同时满足时，写在后面的覆盖前面的，因此高优先级状态（故障 > 告警 > 运行）要放在最后；
3. **默认值兜底**：所有状态都不匹配时，显示静态 Setter 的默认值。

------

## 案例 3：列表行数据状态标记（列表高频用法）

### 应用场景

缺陷记录、生产记录等数据列表中，根据每条数据的严重等级、处理状态，自动改变行的背景、文字颜色，是工业数据列表最常用的写法。

### 完整代码

#### 1. 数据实体类

csharp:

```xaml
public class DefectRecord : INotifyPropertyChanged
{
    private string _defectName;
    public string DefectName
    {
        get => _defectName;
        set { _defectName = value; OnPropertyChanged(); }
    }

    private bool _isCritical;
    /// <summary> 是否为严重缺陷 </summary>
    public bool IsCritical
    {
        get => _isCritical;
        set { _isCritical = value; OnPropertyChanged(); }
    }

    private bool _isFinished;
    /// <summary> 是否已处理完成 </summary>
    public bool IsFinished
    {
        get => _isFinished;
        set { _isFinished = value; OnPropertyChanged(); }
    }
}
```

#### 2. 列表与数据模板

xaml:

```xaml
<Window.Resources>
    <!-- 列表数据模板：触发器写在模板内 -->
    <DataTemplate x:Key="DefectItemTemplate">
        <Border x:Name="ItemBorder" Padding="8 4" Background="Transparent">
            <TextBlock x:Name="ItemText" Text="{Binding DefectName}" Foreground="#333"/>
        </Border>

        <DataTemplate.Triggers>
            <!-- 严重缺陷：红底红字 -->
            <DataTrigger Binding="{Binding IsCritical}" Value="True">
                <Setter TargetName="ItemBorder" Property="Background" Value="#FFF1F0"/>
                <Setter TargetName="ItemText" Property="Foreground" Value="Red"/>
            </DataTrigger>

            <!-- 已处理：灰色删除线 -->
            <DataTrigger Binding="{Binding IsFinished}" Value="True">
                <Setter TargetName="ItemText" Property="Foreground" Value="#999"/>
                <Setter TargetName="ItemText" Property="TextDecorations" Value="Strikethrough"/>
            </DataTrigger>
        </DataTemplate.Triggers>
    </DataTemplate>
</Window.Resources>

<!-- 列表绑定数据，使用数据模板 -->
<ListBox Width="300" Height="180" Margin="40"
         ItemsSource="{Binding DefectList}"
         ItemTemplate="{StaticResource DefectItemTemplate}"/>
```

### 核心要点

1. **`TargetName` 指定目标元素**：数据模板内的触发器通过 `TargetName` 修改模板内部已命名的元素，不能直接设置控件属性；
2. **数据上下文自动切换**：列表每行的 DataContext 是单条 `DefectRecord`，绑定直接写属性名即可；
3. **多状态叠加**：严重且已完成的记录，会同时应用两个触发器的效果，后写的优先级更高。

------

## 案例 4：权限控制控件显隐

### 应用场景

根据当前用户权限，自动控制高级配置、参数修改按钮的显示 / 隐藏，权限逻辑完全放在 ViewModel 中，UI 只做呈现，符合 MVVM 分层。

### 完整代码

#### 1. 视图模型权限属性

csharp:

```c#
public class MainViewModel : INotifyPropertyChanged
{
    private bool _isAdmin = false;
    /// <summary> 当前用户是否为管理员 </summary>
    public bool IsAdmin
    {
        get => _isAdmin;
        set { _isAdmin = value; OnPropertyChanged(); }
    }
}
```

#### 2. 按钮样式与界面

xaml:

```xaml
<Window.Resources>
    <Style x:Key="AdminOnlyButtonStyle" TargetType="Button">
        <Setter Property="Width" Value="120"/>
        <Setter Property="Height" Value="32"/>
        <Setter Property="Background" Value="#2E7DFF"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Cursor" Value="Hand"/>
        <!-- 默认显示 -->
        <Setter Property="Visibility" Value="Visible"/>

        <Style.Triggers>
            <!-- 非管理员：隐藏 -->
            <DataTrigger Binding="{Binding IsAdmin}" Value="False">
                <Setter Property="Visibility" Value="Collapsed"/>
            </DataTrigger>
        </Style.Triggers>
    </Style>
</Window.Resources>

<StackPanel Margin="40" Spacing="15">
    <CheckBox Content="管理员模式" IsChecked="{Binding IsAdmin, Mode=TwoWay}"/>
    <Button Content="高级参数配置" Style="{StaticResource AdminOnlyButtonStyle}"/>
</StackPanel>
```

### 核心要点

1. **纯声明式控制**：不需要后台代码手动设置 `Visibility`，数据变化自动切换显隐；
2. **可复用性强**：样式可以复用到所有需要权限控制的按钮、菜单项、面板上；
3. 扩展：也可以绑定权限枚举，通过多个 DataTrigger 实现不同等级的权限控制。

------

## 案例 5：空值占位提示

### 应用场景

设备名称、参数值为空时，自动显示灰色占位提示文字，优化表单、详情页的空数据体验。

### 完整代码

xaml:

```xaml
<Window.Resources>
    <Style x:Key="EmptyPlaceholderStyle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="Foreground" Value="#333"/>
        <Setter Property="Text" Value="{Binding DeviceName}"/>

        <Style.Triggers>
            <!-- 绑定值为空时，显示占位提示 -->
            <DataTrigger Binding="{Binding DeviceName}" Value="{x:Null}">
                <Setter Property="Text" Value="未设置设备名称"/>
                <Setter Property="Foreground" Value="#AAAAAA"/>
            </DataTrigger>
        </Style.Triggers>
    </Style>
</Window.Resources>

<StackPanel Margin="40" Spacing="10">
    <TextBlock Text="设备名称：" FontWeight="Bold"/>
    <TextBlock Style="{StaticResource EmptyPlaceholderStyle}"/>
</StackPanel>
```

### 核心要点

1. **空值判断用 `{x:Null}`**：专门用于判断绑定值是否为 null；
2. **适用场景**：表单详情页、设备信息卡片、参数展示区的空数据友好提示；
3. 扩展：也可以结合字符串为空的判断，需要配合 `String.IsNullOrEmpty` 转换器使用。

------

## 通用避坑总结

1. **必须实现属性变更通知**：数据源（ViewModel / 实体类）必须实现 `INotifyPropertyChanged`，否则数据修改后触发器感知不到，UI 不会更新。
2. **本地值覆盖触发器**：如果控件上直接写了同名属性（比如 `<Ellipse Fill="Red"/>`），本地值优先级高于触发器，会导致触发器完全失效。
3. **类型严格匹配**：`Value` 的类型要和绑定属性一致，枚举、布尔值推荐用 `{x:Static}` 显式指定，避免字符串隐式转换失败。
4. **仅支持相等比较**：DataTrigger 只能判断「等于」，无法直接实现大于、小于、区间判断；需要范围逻辑时，推荐在 ViewModel 中新增布尔属性（如 `IsOverThreshold`）再绑定。
5. **顺序决定优先级**：多个触发器同时满足时，后定义的覆盖先定义的，高优先级状态（故障、严重缺陷）一定要放在最后。