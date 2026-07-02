# 007004002_WPF `INotifyPropertyChanged` 基础案例合集

以下案例均围绕工业上位机常见的设备参数、状态、相机配置等场景设计，从**最简入门**到**嵌套层级**循序渐进，所有代码可直接复制运行，核心验证「数据修改 → 自动通知 → UI 刷新」的完整链路。

------

## 前置：通用 ViewModel 基类（所有案例复用）

先封装一个标准的属性通知基类，封装通用逻辑，子类只需关注业务属性。这是工业项目的标准写法。

csharp:

```c#
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

/// <summary>
/// 属性变更通知基类
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    /// <summary> 属性变更事件（接口定义） </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// 触发属性变更通知
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// 简化属性赋值：值变化时自动触发通知
    /// </summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        // 值相同则不处理，避免无效通知
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
```

------

## 案例 1：最简单向通知（设备温度实时显示）

### 场景

后台模拟温度采集，修改温度数值后，界面文本自动刷新，无需手动给控件赋值。

这是 `INotifyPropertyChanged` 最基础、最核心的用法。

### 1. 视图模型

csharp:

```c#
public class DeviceViewModel : ViewModelBase
{
    // 后台字段
    private double _temperature = 25.6;

    // 公开属性：赋值时自动触发变更通知
    public double Temperature
    {
        get => _temperature;
        set => SetProperty(ref _temperature, value);
    }

    private string _deviceName = "激光焊接工位A";
    public string DeviceName
    {
        get => _deviceName;
        set => SetProperty(ref _deviceName, value);
    }

    /// <summary>
    /// 模拟温度上升
    /// </summary>
    public void AddTemperature()
    {
        Temperature += 0.5;
    }
}
```

### 2. 窗口后台（设置数据上下文）

csharp:

```c#
public partial class MainWindow : Window
{
    public DeviceViewModel Vm { get; } = new DeviceViewModel();

    public MainWindow()
    {
        InitializeComponent();
        // 窗口设置数据源，子控件自动继承
        this.DataContext = Vm;
    }

    private void BtnAddTemp_Click(object sender, RoutedEventArgs e)
    {
        // 只修改数据，不操作UI控件
        Vm.AddTemperature();
    }
}
```

### 3. XAML 界面绑定

xaml:

```xaml
<StackPanel Margin="30" Spacing="15">
    <TextBlock FontSize="18" FontWeight="Bold" Text="{Binding DeviceName}"/>
    <TextBlock FontSize="16" Text="{Binding Temperature, StringFormat=当前温度：{0:F1} ℃}"/>
    <Button Content="温度 +0.5" Click="BtnAddTemp_Click" Width="120" Height="30"/>
</StackPanel>
```

### 效果与要点

- **效果**：点击按钮，温度数值自动在界面上更新，全程没有 `textBlock.Text = xxx` 这类代码。
- **核心验证**：只有实现了 `INotifyPropertyChanged`，属性变化时 UI 才会自动刷新；如果去掉接口，界面永远只显示初始值。
- **坑点提醒**：如果直接修改后台字段 `_temperature = 30;`，不会走属性的 `set` 逻辑，不会触发通知，UI 不会更新。

------

## 案例 2：双向绑定通知（相机曝光参数调节）

### 场景

用户在输入框修改相机曝光时间，数据源实时同步；数据源修改时，输入框也自动更新，验证双向同步下的通知机制。

工业场景中参数配置界面大量使用这种模式。

### 1. 视图模型新增属性

csharp:

```c#
public class CameraViewModel : ViewModelBase
{
    private double _exposure = 1500;
    /// <summary> 曝光时间（单位：μs） </summary>
    public double Exposure
    {
        get => _exposure;
        set => SetProperty(ref _exposure, value);
    }

    private double _gain = 1.2;
    /// <summary> 增益值 </summary>
    public double Gain
    {
        get => _gain;
        set => SetProperty(ref _gain, value);
    }

    /// <summary> 一键设置默认参数 </summary>
    public void ResetParams()
    {
        Exposure = 1000;
        Gain = 1.0;
    }
}
```

### 2. XAML 双向绑定

xaml:

```xaml
<StackPanel Margin="30" Spacing="12">
    <!-- 曝光时间输入：双向绑定，输入实时回写 -->
    <StackPanel Orientation="Horizontal">
        <TextBlock Width="100" VerticalAlignment="Center">曝光时间：</TextBlock>
        <TextBox Width="200" 
                 Text="{Binding Exposure, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>
        <TextBlock Margin="10 0" VerticalAlignment="Center" Text="μs"/>
    </StackPanel>

    <!-- 增益输入 -->
    <StackPanel Orientation="Horizontal">
        <TextBlock Width="100" VerticalAlignment="Center">增益值：</TextBlock>
        <TextBox Width="200" 
                 Text="{Binding Gain, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>
        <TextBlock Margin="10 0" VerticalAlignment="Center" Text="dB"/>
    </StackPanel>

    <!-- 实时显示当前值，验证数据源同步效果 -->
    <TextBlock Text="{Binding Exposure, StringFormat=当前曝光：{0}μs}" Foreground="Gray"/>
    <TextBlock Text="{Binding Gain, StringFormat=当前增益：{0:F1}dB}" Foreground="Gray"/>

    <Button Content="恢复默认参数" Click="BtnReset_Click" Width="120" Margin="0 10 0 0"/>
</StackPanel>
```

### 3. 按钮事件

csharp:

```c#
private void BtnReset_Click(object sender, RoutedEventArgs e)
{
    Vm.ResetParams();
}
```

### 效果与要点

1. **双向同步**：在输入框修改数值，下方文本实时变化，说明 UI 修改已经回写到数据源；
2. **数据驱动 UI**：点击「恢复默认」，修改数据源属性，输入框自动更新数值；
3. **关键参数**：`UpdateSourceTrigger=PropertyChanged` 让输入实时回写，默认是失焦才回写。

------

## 案例 3：嵌套对象属性通知（工位 - 相机层级）

### 场景

工业项目中 ViewModel 通常是多层嵌套的（工位 → 相机 → 参数），验证嵌套对象内部属性变化时，UI 能否正常刷新。

**核心注意**：嵌套类自身也必须实现 `INotifyPropertyChanged`，只在外层实现是无效的。

### 1. 嵌套类（相机参数）

csharp:

```c#
/// <summary>
/// 相机参数类：必须自己实现通知接口
/// </summary>
public class CameraParams : ViewModelBase
{
    private double _exposure = 1500;
    public double Exposure
    {
        get => _exposure;
        set => SetProperty(ref _exposure, value);
    }

    private double _gain = 1.2;
    public double Gain
    {
        get => _gain;
        set => SetProperty(ref _gain, value);
    }
}
```

### 2. 外层工位视图模型

csharp:

```c#
public class StationViewModel : ViewModelBase
{
    private CameraParams _guideCamera;
    /// <summary> 引导相机（嵌套对象） </summary>
    public CameraParams GuideCamera
    {
        get => _guideCamera;
        set => SetProperty(ref _guideCamera, value);
    }

    private string _stationName;
    public string StationName
    {
        get => _stationName;
        set => SetProperty(ref _stationName, value);
    }

    public StationViewModel()
    {
        StationName = "对位焊接工位";
        GuideCamera = new CameraParams();
    }

    /// <summary> 修改相机曝光（修改嵌套对象的内部属性） </summary>
    public void ChangeCameraExposure(double value)
    {
        GuideCamera.Exposure = value;
    }

    /// <summary> 替换整个相机对象 </summary>
    public void ReplaceCamera()
    {
        GuideCamera = new CameraParams { Exposure = 2000, Gain = 1.5 };
    }
}
```

### 3. XAML 多级路径绑定

xaml:

```xaml
<StackPanel Margin="30" Spacing="12">
    <TextBlock FontSize="16" FontWeight="Bold" Text="{Binding StationName}"/>
    
    <!-- 多级路径：用点号访问嵌套对象的属性 -->
    <TextBlock Text="{Binding GuideCamera.Exposure, StringFormat=曝光时间：{0} μs}"/>
    <TextBlock Text="{Binding GuideCamera.Gain, StringFormat=增益值：{0:F1} dB}"/>

    <Button Content="修改曝光为3000" Click="BtnChangeExposure_Click" Width="150"/>
    <Button Content="替换整个相机对象" Click="BtnReplaceCamera_Click" Width="150"/>
</StackPanel>
```

### 效果与要点

- **修改内部属性**：调用 `ChangeCameraExposure`，界面正常刷新，因为 `CameraParams` 自己实现了通知接口；
- **替换整个对象**：调用 `ReplaceCamera`，界面也会刷新，因为外层 `GuideCamera` 属性触发了通知；
- **高频坑点**：如果 `CameraParams` 不实现 `INotifyPropertyChanged`，修改 `Exposure` 时界面完全没反应，只有替换整个对象时才会刷新。

------

## 案例 4：只读计算属性通知（状态描述合成）

### 场景

一个属性的值由其他多个属性计算得出（只读，没有 set），比如根据「运行状态 + 告警状态」合成状态描述文本。

这类属性不会自动触发通知，需要在依赖属性变化时手动触发。

### 1. 视图模型

csharp:

```c#
public class StatusViewModel : ViewModelBase
{
    private bool _isRunning = true;
    /// <summary> 是否运行中 </summary>
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (SetProperty(ref _isRunning, value))
            {
                // 运行状态变化时，手动通知计算属性也变了
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(StatusColor));
            }
        }
    }

    private bool _isAlarming;
    /// <summary> 是否告警中 </summary>
    public bool IsAlarming
    {
        get => _isAlarming;
        set
        {
            if (SetProperty(ref _isAlarming, value))
            {
                // 告警状态变化时，同样手动通知
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(StatusColor));
            }
        }
    }

    // ---------- 以下是只读计算属性 ----------

    /// <summary> 状态描述文本（由两个属性合成） </summary>
    public string StatusText
    {
        get
        {
            if (!IsRunning) return "设备停机";
            return IsAlarming ? "运行中 · 告警" : "运行中 · 正常";
        }
    }

    /// <summary> 状态颜色（字符串形式，供绑定） </summary>
    public string StatusColor
    {
        get
        {
            if (!IsRunning) return "Gray";
            return IsAlarming ? "Orange" : "LimeGreen";
        }
    }
}
```

### 2. XAML 绑定

xaml:

```c#
<StackPanel Margin="30" Spacing="12">
    <CheckBox Content="设备运行" IsChecked="{Binding IsRunning, Mode=TwoWay}"/>
    <CheckBox Content="触发告警" IsChecked="{Binding IsAlarming, Mode=TwoWay}"/>

    <TextBlock FontSize="16" Text="{Binding StatusText}" Foreground="{Binding StatusColor}"/>
</StackPanel>
```

### 效果与要点

- **效果**：勾选 / 取消两个复选框，下方的状态文本和颜色会自动同步变化；
- **核心原理**：计算属性本身没有 `set`，不会自动发通知，必须在它依赖的属性变化时，手动调用 `OnPropertyChanged` 触发刷新；
- **适用场景**：状态合成、面积计算、全路径拼接、格式转换等由多个字段组合得出的只读属性。

------

## 案例 5：批量属性统一通知（一键复位）

### 场景

一次操作修改多个属性，不需要每个属性都触发一次通知，可以全部修改完成后，统一触发一次刷新，减少 UI 重绘次数，优化性能。

### 1. 视图模型

csharp:

```c#
public class ParamsViewModel : ViewModelBase
{
    public double Exposure { get; set; }
    public double Gain { get; set; }
    public double TriggerDelay { get; set; }

    /// <summary>
    /// 一键复位所有参数
    /// </summary>
    public void ResetAll()
    {
        // 1. 批量修改所有字段（不走属性set，不触发单次通知）
        Exposure = 1000;
        Gain = 1.0;
        TriggerDelay = 0;

        // 2. 统一通知三个属性都变更了，只触发3次UI更新
        OnPropertyChanged(nameof(Exposure));
        OnPropertyChanged(nameof(Gain));
        OnPropertyChanged(nameof(TriggerDelay));
    }
}
```

### 补充进阶：空字符串通知全部刷新

如果属性非常多，可以传 `null` 或空字符串，通知 WPF 刷新该对象的所有绑定属性：

csharp

```c#
// 通知：这个对象所有属性都变了，全部刷新
OnPropertyChanged(null);
```

> 注意：这种方式会刷新所有绑定，简单粗暴，适合全量重置场景；正常业务不建议滥用，会造成不必要的性能开销。

------

## 常见反例验证（必踩坑）

### 错误写法 1：直接修改后台字段

csharp:

```c#
// ❌ 错误：直接改字段，不走set，不会触发通知，UI不更新
_temperature = 30;

// ✅ 正确：走属性赋值，自动触发通知
Temperature = 30;
```

### 错误写法 2：嵌套类不实现接口

csharp:

```c#
// ❌ 错误：CameraParams 没实现 INotifyPropertyChanged
// 修改内部属性 UI 毫无反应，只有替换整个对象才刷新
public CameraParams GuideCamera { get; set; }
```

### 错误写法 3：属性名不匹配

csharp:

```c#
// ❌ 错误：属性名写错，通知和绑定对不上，UI不刷新
OnPropertyChanged("Temperture"); // 拼写错误

// ✅ 正确：用 nameof 或 CallerMemberName 自动获取
OnPropertyChanged(nameof(Temperature));
```

------

## 案例选型总结

| 场景             | 实现方式               | 核心要点                       |
| :--------------- | :--------------------- | :----------------------------- |
| 单个属性展示     | 基础属性 + SetProperty | 走属性赋值，不要直接改字段     |
| 参数输入双向同步 | TwoWay 绑定 + 属性通知 | 按需设置 UpdateSourceTrigger   |
| 多层嵌套对象     | 每层都实现通知接口     | 嵌套类必须自己实现，外层不兜底 |
| 组合计算属性     | 依赖属性变化时手动通知 | 只读属性不会自动发通知         |
| 批量修改参数     | 改完统一触发通知       | 减少 UI 刷新次数，提升性能     |