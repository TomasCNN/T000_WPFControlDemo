# 004001003_IoIndicator 控件完整配套代码

包含**默认样式配置、完整使用示例、进阶自定义、常见问题排查**四部分，所有代码可直接复制到你的项目中，开箱即用。

------

## 一、第一步：配置默认样式（必须完成）

### 1.1 项目结构要求

在你的项目**根目录**下创建`Themes`文件夹，然后在其中添加`Generic.xaml`文件，结构如下：

plaintext：

```tex
IndustrialVisionTemplate/
├── Controls/
│   └── IoIndicator.cs          ← 你之前写的控件类
├── Themes/
│   └── Generic.xaml            ← 新增：默认样式文件
├── Properties/
│   └── AssemblyInfo.cs         ← 需要修改
└── IndustrialVisionTemplate.csproj
```

### 1.2 修改 Generic.xaml 的生成操作

**这是 90% 的人控件不显示的原因**：

1. 右键点击`Generic.xaml` → 属性
2. 将**生成操作**设置为`Page`
3. 将**复制到输出目录**设置为`不复制`

### 1.3 配置 AssemblyInfo.cs

打开`Properties/AssemblyInfo.cs`，添加以下代码（如果没有的话）：

csharp：

```c#
using System.Windows;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, // 主题特定资源字典的位置
    ResourceDictionaryLocation.SourceAssembly // 通用资源字典的位置
)]
```

### 1.4 编写默认样式代码

将以下代码复制到`Themes/Generic.xaml`中：

xaml:

```xaml
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:controls="clr-namespace:IndustrialVisionTemplate.Controls">

    <!-- IoIndicator 工业级默认样式 -->
    <Style TargetType="{x:Type controls:IoIndicator}">
        <!-- 默认大小：20x20，工业标准尺寸 -->
        <Setter Property="Width" Value="20"/>
        <Setter Property="Height" Value="20"/>
        <Setter Property="HorizontalAlignment" Value="Center"/>
        <Setter Property="VerticalAlignment" Value="Center"/>
        
        <!-- 控件模板：定义指示灯的外观 -->
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type controls:IoIndicator}">
                    <Grid>
                        <!-- 外阴影：增加立体感 -->
                        <Ellipse 
                            Width="{TemplateBinding Width}"
                            Height="{TemplateBinding Height}"
                            Fill="#22000000"
                            Margin="1,1,0,0"/>
                        
                        <!-- 主指示灯 -->
                        <Ellipse 
                            x:Name="PART_Indicator"
                            Width="{TemplateBinding Width}"
                            Height="{TemplateBinding Height}"
                            Fill="{TemplateBinding IndicatorColor}"
                            Stroke="Black"
                            StrokeThickness="1">
                            <Ellipse.Effect>
                                <DropShadowEffect 
                                    Color="{TemplateBinding IndicatorColor}"
                                    ShadowDepth="0"
                                    BlurRadius="4"
                                    Opacity="0.7"/>
                            </Ellipse.Effect>
                        </Ellipse>
                        
                        <!-- 高光效果：模拟玻璃质感 -->
                        <Ellipse 
                            Width="{TemplateBinding Width * 0.6}"
                            Height="{TemplateBinding Height * 0.3}"
                            Fill="#66FFFFFF"
                            VerticalAlignment="Top"
                            Margin="0,2,0,0"/>
                    </Grid>
                    
                    <!-- 控件状态触发器 -->
                    <ControlTemplate.Triggers>
                        <!-- 禁用状态：变灰 -->
                        <Trigger Property="IsEnabled" Value="False">
                            <Setter TargetName="PART_Indicator" Property="Fill" Value="#9E9E9E"/>
                            <Setter TargetName="PART_Indicator" Property="Effect" Value="{x:Null}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</ResourceDictionary>
```

------

## 二、第二步：完整使用示例

### 2.1 在 XAML 中引用控件

首先在窗口或页面的顶部添加命名空间引用：

xaml:

```xaml
<Window x:Class="IndustrialVisionTemplate.Views.IoMonitorView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:controls="clr-namespace:IndustrialVisionTemplate.Controls">
```

### 2.2 静态使用示例

xaml:

```xaml
<StackPanel Orientation="Horizontal" Margin="10">
    <!-- 输入点1：ON状态，绿色 -->
    <TextBlock Text="输入1：" VerticalAlignment="Center" Margin="0,0,5,0"/>
    <controls:IoIndicator IsOn="True" Margin="0,0,20,0"/>
    
    <!-- 输入点2：OFF状态，红色 -->
    <TextBlock Text="输入2：" VerticalAlignment="Center" Margin="0,0,5,0"/>
    <controls:IoIndicator IsOn="False" Margin="0,0,20,0"/>
    
    <!-- 输入点3：报警状态，闪烁红色 -->
    <TextBlock Text="输入3：" VerticalAlignment="Center" Margin="0,0,5,0"/>
    <controls:IoIndicator IsOn="True" IsBlinking="True" Margin="0,0,20,0"/>
    
    <!-- 自定义大小：30x30 -->
    <TextBlock Text="输出1：" VerticalAlignment="Center" Margin="0,0,5,0"/>
    <controls:IoIndicator IsOn="True" Width="30" Height="30"/>
</StackPanel>
```

### 2.3 MVVM 数据绑定示例（工业项目标准用法）

#### 第一步：编写 ViewModel

csharp:

```c#
using IndustrialVisionTemplate.Controls;
using Prism.Mvvm;
using System;
using System.Timers;

namespace IndustrialVisionTemplate.ViewModels
{
    public class IoMonitorViewModel : BindableBase
    {
        // 输入点状态
        private bool _input1State;
        public bool Input1State
        {
            get => _input1State;
            set => SetProperty(ref _input1State, value);
        }

        private bool _input2State;
        public bool Input2State
        {
            get => _input2State;
            set => SetProperty(ref _input2State, value);
        }

        private bool _input2Alarm;
        public bool Input2Alarm
        {
            get => _input2Alarm;
            set => SetProperty(ref _input2Alarm, value);
        }

        // 输出点状态
        private bool _output1State;
        public bool Output1State
        {
            get => _output1State;
            set => SetProperty(ref _output1State, value);
        }

        public IoMonitorViewModel()
        {
            // 模拟PLC实时更新IO状态（实际项目中替换为真实PLC读取）
            var timer = new Timer(1000);
            timer.Elapsed += (s, e) =>
            {
                // 随机更新状态，模拟PLC数据变化
                Input1State = new Random().Next(2) == 0;
                Input2State = new Random().Next(2) == 0;
                Input2Alarm = Input2State && new Random().Next(10) == 0; // 10%概率报警
                Output1State = new Random().Next(2) == 0;
            };
            timer.Start();
        }
    }
}
```

#### 第二步：XAML 绑定

xaml:

```xaml
<Window.DataContext>
    <viewModels:IoMonitorViewModel/>
</Window.DataContext>

<Grid Margin="20">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>

    <!-- 输入点区域 -->
    <TextBlock Text="输入点" Grid.Row="0" Grid.Column="0" FontWeight="Bold" Margin="0,0,10,10"/>
    
    <TextBlock Text="输入1：" Grid.Row="1" Grid.Column="0" VerticalAlignment="Center" Margin="0,0,5,10"/>
    <controls:IoIndicator 
        Grid.Row="1" Grid.Column="1" 
        IsOn="{Binding Input1State}" 
        Margin="0,0,20,10"/>
    
    <TextBlock Text="输入2：" Grid.Row="1" Grid.Column="2" VerticalAlignment="Center" Margin="0,0,5,10"/>
    <controls:IoIndicator 
        Grid.Row="1" Grid.Column="3" 
        IsOn="{Binding Input2State}" 
        IsBlinking="{Binding Input2Alarm}"
        Margin="0,0,0,10"/>

    <!-- 输出点区域 -->
    <TextBlock Text="输出点" Grid.Row="2" Grid.Column="0" FontWeight="Bold" Margin="0,0,10,0"/>
    
    <TextBlock Text="输出1：" Grid.Row="3" Grid.Column="0" VerticalAlignment="Center" Margin="0,0,5,0"/>
    <controls:IoIndicator 
        Grid.Row="3" Grid.Column="1" 
        IsOn="{Binding Output1State}"
        Margin="0,0,20,0"/>
</Grid>
```

### 2.4 动态创建控件示例

如果需要根据 PLC 的 IO 点数动态创建控件：

csharp:

```c#
private void CreateIoControls()
{
    // 从PLC获取IO点列表
    var ioPoints = Device.PLC.GetIoPoints();
    
    foreach (var ioPoint in ioPoints)
    {
        // 创建IO指示灯
        var indicator = new IoIndicator();
        indicator.Width = 20;
        indicator.Height = 20;
        
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
        
        // 数据绑定
        indicator.SetBinding(
            IoIndicator.IsOnProperty, 
            new Binding($"IoPoints[{ioPoint.Id}].State"));
        indicator.SetBinding(
            IoIndicator.IsBlinkingProperty, 
            new Binding($"IoPoints[{ioPoint.Id}].IsAlarm"));
        
        IoPanel.Children.Add(stackPanel);
    }
}
```

------

## 三、进阶自定义用法

### 3.1 自定义颜色

如果需要修改默认的颜色方案，可以在 App.xaml 中重写样式：

xaml:

```xaml
<Application.Resources>
    <Style TargetType="{x:Type controls:IoIndicator}" BasedOn="{StaticResource {x:Type controls:IoIndicator}}">
        <!-- 自定义ON状态颜色：蓝色 -->
        <Setter Property="OnColor" Value="#2196F3"/>
        <!-- 自定义OFF状态颜色：橙色 -->
        <Setter Property="OffColor" Value="#FF9800"/>
        <!-- 自定义闪烁颜色：黄色 -->
        <Setter Property="BlinkOffColor" Value="#FFEB3B"/>
    </Style>
</Application.Resources>
```

### 3.2 区分输入输出指示灯

可以创建两个派生类，分别用于输入和输出：

csharp:

```c#
/// <summary>
/// 输入点指示灯（绿色=ON，红色=OFF）
/// </summary>
public class InputIndicator : IoIndicator
{
    static InputIndicator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(InputIndicator), 
            new FrameworkPropertyMetadata(typeof(IoIndicator)));
    }
}

/// <summary>
/// 输出点指示灯（蓝色=ON，灰色=OFF）
/// </summary>
public class OutputIndicator : IoIndicator
{
    static OutputIndicator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(OutputIndicator), 
            new FrameworkPropertyMetadata(typeof(IoIndicator)));
    }

    public OutputIndicator()
    {
        // 重写颜色逻辑
        if (IsOn)
            IndicatorColor = Brushes.Blue;
        else
            IndicatorColor = Brushes.Gray;
    }
}
```

### 3.3 自定义闪烁频率

修改`IoIndicator.cs`中的闪烁间隔：

csharp:

```c#
// 闪烁间隔：300ms（更快的闪烁）
_blinkTimer.Interval = TimeSpan.FromMilliseconds(300);
```

------

## 四、常见问题排查

### 问题 1：控件显示不出来（最常见）

**排查步骤**：

1. ✅ 确认`Themes/Generic.xaml`的生成操作是`Page`
2. ✅ 确认`AssemblyInfo.cs`中添加了`ThemeInfo`特性
3. ✅ 确认控件类是`public`的
4. ✅ 确认静态构造函数中写了`DefaultStyleKeyProperty.OverrideMetadata`
5. ✅ 确认 XAML 中的命名空间与控件类的命名空间完全一致（包括大小写）

### 问题 2：颜色不更新

**排查步骤**：

1. ✅ 确认依赖属性的变更回调正确调用了`UpdateIndicatorColor()`
2. ✅ 确认`IndicatorColor`是依赖属性，而不是普通属性
3. ✅ 确认模板中使用了`{TemplateBinding IndicatorColor}`
4. ✅ 确认没有在后台代码中直接修改 Ellipse 的 Fill 属性

### 问题 3：内存泄漏

**排查步骤**：

1. ✅ 确认在`Unloaded`事件中停止了定时器并释放了引用
2. ✅ 确认没有在静态变量中引用控件实例
3. ✅ 确认及时取消了事件订阅
4. ✅ 确认没有在定时器回调中持有强引用

### 问题 4：闪烁不流畅

**解决方案**：

- 使用`DispatcherTimer`而不是`System.Timers.Timer`
- 闪烁间隔不要小于 200ms，人眼无法区分更快的闪烁
- 避免在闪烁回调中执行耗时操作

------

## 五、工业现场优化建议

1. **颜色标准化**：严格遵循工业现场颜色规范

   

   - 绿色：运行、导通、正常
   - 红色：停机、断开、故障
   - 黄色：警告、待机
   - 蓝色：输出、动作

   

2. **大小标准化**：

   

   - 普通 IO 点：20x20
   - 重要 IO 点：30x30
   - 报警指示灯：40x40

   

3. **性能优化**：

   

   - 同一页面不要超过 100 个指示灯
   - 状态更新频率不要超过 10Hz
   - 批量更新时使用`BeginInvoke`批量处理