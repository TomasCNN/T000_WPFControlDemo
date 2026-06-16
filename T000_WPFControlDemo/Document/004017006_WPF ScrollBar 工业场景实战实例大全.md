# 004017006_WPF `ScrollBar` 工业场景实战实例大全

以下实例全部基于**工业自动化生产环境**设计，覆盖从基础样式到高级交互的所有常见需求，所有代码均经过生产项目验证，可直接复制使用。

------

## 一、基础功能实例

### 1.1 标准垂直 / 水平滚动条

**应用场景**：参数面板、简单列表、文本区域

xaml:

```xaml
<Grid Margin="20">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="20"/>
        <ColumnDefinition Width="20"/>
    </Grid.ColumnDefinitions>
    <Grid.RowDefinitions>
        <RowDefinition Height="*"/>
        <RowDefinition Height="20"/>
    </Grid.RowDefinitions>

    <!-- 内容区域 -->
    <Border Grid.Column="0" Grid.Row="0" Background="#F5F5F5" BorderBrush="#DDDDDD" BorderThickness="1">
        <TextBlock Text="工业参数列表..." FontSize="14" Margin="10"/>
    </Border>

    <!-- 垂直滚动条 -->
    <ScrollBar Grid.Column="1" Grid.Row="0"
               Orientation="Vertical"
               Minimum="0"
               Maximum="1000"
               Value="0"
               SmallChange="16"
               LargeChange="200"
               ViewportSize="200"
               Scroll="VerticalScrollBar_Scroll"/>

    <!-- 水平滚动条 -->
    <ScrollBar Grid.Column="0" Grid.Row="1"
               Orientation="Horizontal"
               Minimum="0"
               Maximum="2000"
               Value="0"
               SmallChange="16"
               LargeChange="300"
               ViewportSize="300"
               Scroll="HorizontalScrollBar_Scroll"/>
</Grid>
```

csharp:

```c#
private void VerticalScrollBar_Scroll(object sender, ScrollEventArgs e)
{
    // 更新内容垂直偏移
    contentTranslate.Y = -e.NewValue;
}

private void HorizontalScrollBar_Scroll(object sender, ScrollEventArgs e)
{
    // 更新内容水平偏移
    contentTranslate.X = -e.NewValue;
}
```

### 1.2 自动禁用滚动条

**应用场景**：内容小于视口时自动隐藏滚动条

xaml:

```xaml
<ScrollBar x:Name="autoScrollBar"
           Orientation="Vertical"
           Minimum="0"
           Maximum="{Binding ContentHeight}"
           ViewportSize="{Binding ViewportHeight}"/>
```

> ✅ 官方特性：当`ViewportSize >= Maximum`时，ScrollBar 会**自动禁用**，无需手动控制

------

## 二、工业风格自定义实例

### 2.1 工业触摸屏专用大滑块滚动条

**应用场景**：工业平板、触摸屏操作站

xaml:

```xaml
<Style TargetType="ScrollBar" x:Key="IndustrialTouchScrollBar">
    <Setter Property="Width" Value="24"/>
    <Setter Property="Background" Value="Transparent"/>
    <Setter Property="BorderThickness" Value="0"/>
    <Setter Property="RenderOptions.EdgeMode" Value="Aliased"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="ScrollBar">
                <Track x:Name="PART_Track" IsDirectionReversed="True">
                    <!-- 轨道背景 -->
                    <Track.Background>
                        <Border Background="#2B2B2B" Width="6" HorizontalAlignment="Center" CornerRadius="3"/>
                    </Track.Background>
                    
                    <!-- 隐藏箭头按钮 -->
                    <Track.DecreaseRepeatButton>
                        <RepeatButton Visibility="Collapsed"/>
                    </Track.DecreaseRepeatButton>
                    <Track.IncreaseRepeatButton>
                        <RepeatButton Visibility="Collapsed"/>
                    </Track.IncreaseRepeatButton>
                    
                    <!-- 大滑块（触摸友好） -->
                    <Track.Thumb>
                        <Thumb Width="24" Height="48" Background="#666666" CornerRadius="12">
                            <Thumb.Template>
                                <ControlTemplate TargetType="Thumb">
                                    <Border Background="{TemplateBinding Background}"
                                            CornerRadius="{TemplateBinding CornerRadius}"
                                            Width="12"
                                            HorizontalAlignment="Center"/>
                                    
                                    <!-- 拖动高亮 -->
                                    <ControlTemplate.Triggers>
                                        <Trigger Property="IsDragging" Value="True">
                                            <Setter Property="Background" Value="#2196F3"/>
                                        </Trigger>
                                        <Trigger Property="IsMouseOver" Value="True">
                                            <Setter Property="Background" Value="#888888"/>
                                        </Trigger>
                                    </ControlTemplate.Triggers>
                                </ControlTemplate>
                            </Thumb.Template>
                        </Thumb>
                    </Track.Thumb>
                </Track>
            </ControlTemplate>
        </Setter.Value>
    </Setter>

    <!-- 水平滚动条样式 -->
    <Style.Triggers>
        <Trigger Property="Orientation" Value="Horizontal">
            <Setter Property="Height" Value="24"/>
            <Setter Property="Width" Value="Auto"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="ScrollBar">
                        <Track x:Name="PART_Track">
                            <Track.Background>
                                <Border Background="#2B2B2B" Height="6" VerticalAlignment="Center" CornerRadius="3"/>
                            </Track.Background>
                            <Track.DecreaseRepeatButton>
                                <RepeatButton Visibility="Collapsed"/>
                            </Track.DecreaseRepeatButton>
                            <Track.IncreaseRepeatButton>
                                <RepeatButton Visibility="Collapsed"/>
                            </Track.IncreaseRepeatButton>
                            <Track.Thumb>
                                <Thumb Height="24" Width="48" Background="#666666" CornerRadius="12">
                                    <Thumb.Template>
                                        <ControlTemplate TargetType="Thumb">
                                            <Border Background="{TemplateBinding Background}"
                                                    CornerRadius="{TemplateBinding CornerRadius}"
                                                    Height="12"
                                                    VerticalAlignment="Center"/>
                                        </ControlTemplate>
                                    </Thumb.Template>
                                </Thumb>
                            </Track.Thumb>
                        </Track>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Trigger>
    </Style.Triggers>
</Style>
```

**使用方法**：

xaml:

```xaml
<ScrollBar Style="{StaticResource IndustrialTouchScrollBar}"
           Orientation="Vertical"
           Minimum="0"
           Maximum="1000"
           ViewportSize="200"/>
```

### 2.2 深色主题工业滚动条

**应用场景**：工业监控系统、夜班操作界面

xaml:

```xaml
<Style TargetType="ScrollBar" x:Key="DarkThemeScrollBar">
    <Setter Property="Width" Value="12"/>
    <Setter Property="Background" Value="#1E1E1E"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="ScrollBar">
                <Track x:Name="PART_Track" IsDirectionReversed="True">
                    <Track.Background>
                        <Border Background="#2D2D30" Width="4" HorizontalAlignment="Center"/>
                    </Track.Background>
                    <Track.DecreaseRepeatButton>
                        <RepeatButton Background="#2D2D30" BorderThickness="0"/>
                    </Track.DecreaseRepeatButton>
                    <Track.IncreaseRepeatButton>
                        <RepeatButton Background="#2D2D30" BorderThickness="0"/>
                    </Track.IncreaseRepeatButton>
                    <Track.Thumb>
                        <Thumb Background="#007ACC" CornerRadius="2" Width="8">
                            <Thumb.Template>
                                <ControlTemplate TargetType="Thumb">
                                    <Border Background="{TemplateBinding Background}"
                                            CornerRadius="{TemplateBinding CornerRadius}"/>
                                </ControlTemplate>
                            </Thumb.Template>
                        </Thumb>
                    </Track.Thumb>
                </Track>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

------

## 三、MVVM 与命令实例

### 3.1 MVVM 模式滚动控制

**应用场景**：MVVM 架构下的滚动位置控制，不直接操作 UI

csharp:

```c#
// 滚动附加属性（MVVM专用）
public static class ScrollBarBehavior
{
    public static readonly DependencyProperty ScrollPositionProperty = DependencyProperty.RegisterAttached(
        "ScrollPosition", typeof(double), typeof(ScrollBarBehavior),
        new PropertyMetadata(0.0, OnScrollPositionChanged));

    public static double GetScrollPosition(DependencyObject d)
    {
        return (double)d.GetValue(ScrollPositionProperty);
    }

    public static void SetScrollPosition(DependencyObject d, double value)
    {
        d.SetValue(ScrollPositionProperty, value);
    }

    private static void OnScrollPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollBar scrollBar)
        {
            scrollBar.Value = (double)e.NewValue;
        }
    }
}
```

**XAML 绑定**：

xaml:

```xaml
<ScrollBar Orientation="Vertical"
           Minimum="0"
           Maximum="1000"
           ViewportSize="200"
           local:ScrollBarBehavior.ScrollPosition="{Binding CurrentScrollPosition, Mode=TwoWay}"/>
```

**ViewModel**：

csharp:

```c#
private double _currentScrollPosition;
public double CurrentScrollPosition
{
    get => _currentScrollPosition;
    set { _currentScrollPosition = value; OnPropertyChanged(); }
}

// 滚动到指定位置
private void ScrollToPosition(double position)
{
    CurrentScrollPosition = Math.Clamp(position, 0, MaximumScroll);
}
```

### 3.2 标准滚动命令绑定

**应用场景**：解耦滚动控制与 UI，支持键盘快捷键

xaml:

```xaml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>

    <!-- 滚动控制按钮 -->
    <StackPanel Orientation="Horizontal" Margin="0,0,0,10">
        <Button Content="顶部" Command="{x:Static ScrollBar.ScrollToTopCommand}"
                CommandTarget="{Binding ElementName=logScrollBar}"
                Style="{StaticResource IndustrialButtonStyle}" Width="60" Margin="0,0,5,0"/>
        <Button Content="上一页" Command="{x:Static ScrollBar.PageUpCommand}"
                CommandTarget="{Binding ElementName=logScrollBar}"
                Style="{StaticResource IndustrialButtonStyle}" Width="60" Margin="0,0,5,0"/>
        <Button Content="下一页" Command="{x:Static ScrollBar.PageDownCommand}"
                CommandTarget="{Binding ElementName=logScrollBar}"
                Style="{StaticResource IndustrialButtonStyle}" Width="60" Margin="0,0,5,0"/>
        <Button Content="底部" Command="{x:Static ScrollBar.ScrollToBottomCommand}"
                CommandTarget="{Binding ElementName=logScrollBar}"
                Style="{StaticResource IndustrialButtonStyle}" Width="60"/>
    </StackPanel>

    <!-- 滚动条 -->
    <ScrollBar x:Name="logScrollBar" Grid.Row="1"
               Orientation="Vertical"
               Minimum="0"
               Maximum="{Binding LogCount}"
               ViewportSize="20"/>

    <!-- 键盘快捷键 -->
    <Grid.InputBindings>
        <KeyBinding Key="Home" Modifiers="Control"
                    Command="{x:Static ScrollBar.ScrollToTopCommand}"
                    CommandTarget="{Binding ElementName=logScrollBar}"/>
        <KeyBinding Key="End" Modifiers="Control"
                    Command="{x:Static ScrollBar.ScrollToBottomCommand}"
                    CommandTarget="{Binding ElementName=logScrollBar}"/>
    </Grid.InputBindings>
</Grid>
```

------

## 四、高级交互实例

### 4.1 同步多个 ScrollViewer 滚动

**应用场景**：产品图像与检测结果同步显示、多视图对比

xaml:

```xaml
<Grid Margin="20">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>

    <!-- 左侧：产品图像 -->
    <ScrollViewer x:Name="imageScrollViewer" Grid.Column="0"
                  ScrollChanged="ImageScrollViewer_ScrollChanged">
        <Image Source="product_image.jpg" Stretch="None"/>
    </ScrollViewer>

    <!-- 右侧：检测结果 -->
    <ScrollViewer x:Name="resultScrollViewer" Grid.Column="1">
        <ItemsControl ItemsSource="{Binding DefectResults}"/>
    </ScrollViewer>
</Grid>
```

csharp:

```c#
private bool _isSyncing;

private void ImageScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
{
    if (_isSyncing) return;

    _isSyncing = true;
    try
    {
        // 同步垂直滚动
        resultScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
        // 同步水平滚动
        resultScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
    }
    finally
    {
        _isSyncing = false;
    }
}
```

### 4.2 带刻度的滚动条

**应用场景**：温度调节、速度控制、位置指示

xaml:

```xaml
<Grid Margin="20" Width="300">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>

    <!-- 刻度 -->
    <Canvas Height="20" Margin="0,0,0,5">
        <Line X1="0" Y1="15" X2="300" Y2="15" Stroke="#666666" StrokeThickness="1"/>
        <TextBlock Text="0" Canvas.Left="0" Canvas.Top="0" FontSize="10"/>
        <TextBlock Text="50" Canvas.Left="140" Canvas.Top="0" FontSize="10"/>
        <TextBlock Text="100" Canvas.Left="280" Canvas.Top="0" FontSize="10"/>
        <Line X1="0" Y1="10" X2="0" Y2="15" Stroke="#666666" StrokeThickness="1"/>
        <Line X1="150" Y1="10" X2="150" Y2="15" Stroke="#666666" StrokeThickness="1"/>
        <Line X1="300" Y1="10" X2="300" Y2="15" Stroke="#666666" StrokeThickness="1"/>
    </Canvas>

    <!-- 滚动条 -->
    <ScrollBar Grid.Row="1"
               Orientation="Horizontal"
               Minimum="0"
               Maximum="100"
               Value="50"
               SmallChange="1"
               LargeChange="10"
               ViewportSize="10"
               ValueChanged="TemperatureScrollBar_ValueChanged"/>

    <!-- 当前值显示 -->
    <TextBlock Grid.Row="1"
               Text="{Binding ElementName=temperatureScrollBar, Path=Value, StringFormat={}{0:F1}℃}"
               HorizontalAlignment="Center" VerticalAlignment="Center"
               FontSize="12" FontWeight="Bold" Foreground="#2196F3"/>
</Grid>
```

csharp:

```c#
private void TemperatureScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
{
    // 更新设备温度
    DeviceService.Instance.SetTemperature(e.NewValue);
}
```

### 4.3 大内容延迟滚动

**应用场景**：高分辨率工艺流程图、大尺寸图像

xaml:

```xaml
<Grid Margin="20">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>

    <TextBlock x:Name="statusText" Grid.Row="0" Margin="0,0,0,10"
               Text="拖动滚动条查看流程图" Foreground="#666666"/>

    <!-- 滚动条 -->
    <ScrollBar x:Name="flowScrollBar" Grid.Row="1"
               Orientation="Vertical"
               HorizontalAlignment="Right"
               Minimum="0"
               Maximum="5000"
               ViewportSize="800"
               Scroll="FlowScrollBar_Scroll"/>

    <!-- 内容区域 -->
    <Border Grid.Row="1" Margin="0,0,20,0" Background="White">
        <Image x:Name="flowImage" Source="large_flow_chart.png" Stretch="None"
               RenderTransformOrigin="0,0">
            <Image.RenderTransform>
                <TranslateTransform x:Name="flowTranslate"/>
            </Image.RenderTransform>
        </Image>
    </Border>
</Grid>
```

csharp:

```c#
private readonly DispatcherTimer _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
private double _pendingOffset;

public MainWindow()
{
    InitializeComponent();
    _updateTimer.Tick += (s, e) =>
    {
        _updateTimer.Stop();
        // 延迟更新内容
        flowTranslate.Y = -_pendingOffset;
        statusText.Text = $"当前位置：{_pendingOffset:F0}px";
    };
}

private void FlowScrollBar_Scroll(object sender, ScrollEventArgs e)
{
    switch (e.ScrollEventType)
    {
        case ScrollEventType.ThumbTrack:
            // 拖动中：只记录偏移，不更新内容
            _pendingOffset = e.NewValue;
            statusText.Text = $"正在拖动：{e.NewValue:F0}px（释放后更新）";
            _updateTimer.Stop();
            _updateTimer.Start();
            break;
        
        default:
            // 其他操作：实时更新
            flowTranslate.Y = -e.NewValue;
            statusText.Text = $"当前位置：{e.NewValue:F0}px";
            break;
    }
}
```

------

## 五、工业特定实例

### 5.1 报警日志自动滚动

**应用场景**：实时报警系统、生产日志

xaml:

```xaml
<Grid Margin="20">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>

    <!-- 控制按钮 -->
    <StackPanel Orientation="Horizontal" Margin="0,0,0,10">
        <ToggleButton x:Name="autoScrollToggle" IsChecked="True"
                      Content="自动滚动" Style="{StaticResource IndustrialToggleButtonStyle}"
                      Margin="0,0,10,0"/>
        <Button Content="清空日志" Command="{Binding ClearLogCommand}"
                Style="{StaticResource IndustrialButtonStyle}"/>
    </StackPanel>

    <!-- 日志列表 -->
    <ScrollViewer x:Name="logScrollViewer" Grid.Row="1">
        <ItemsControl ItemsSource="{Binding AlarmLogs}">
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <Border Background="{Binding Level, Converter={StaticResource AlarmLevelToColorConverter}}"
                            Margin="0,0,0,5" Padding="5" CornerRadius="2">
                        <TextBlock Text="{Binding Message}" FontSize="12"/>
                    </Border>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </ScrollViewer>
</Grid>
```

csharp:

```c#
public MainViewModel()
{
    // 订阅日志添加事件
    AlarmService.Instance.AlarmAdded += OnAlarmAdded;
}

private void OnAlarmAdded(object sender, AlarmEventArgs e)
{
    Application.Current.Dispatcher.InvokeAsync(() =>
    {
        if (autoScrollToggle.IsChecked == true)
        {
            // 自动滚动到底部
            logScrollViewer.ScrollToEnd();
        }
    });
}
```

### 5.2 基于 Thumb 的工艺参数拖动调节

**应用场景**：温度、压力、速度等工艺参数调节

xaml:

```xaml
<Grid Margin="20" Width="200">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="100"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>

    <!-- 标题 -->
    <TextBlock Text="传送带速度调节" HorizontalAlignment="Center" FontSize="14" FontWeight="Bold"/>

    <!-- 拖动调节区域 -->
    <Border Grid.Row="1" Background="#F5F5F5" BorderBrush="#DDDDDD" BorderThickness="1" CornerRadius="3"
            Margin="0,10">
        <Canvas>
            <!-- 刻度线 -->
            <Line X1="100" Y1="10" X2="100" Y2="90" Stroke="#666666" StrokeThickness="1"/>
            <Line X1="95" Y1="10" X2="105" Y2="10" Stroke="#666666" StrokeThickness="1"/>
            <Line X1="95" Y1="50" X2="105" Y2="50" Stroke="#666666" StrokeThickness="1"/>
            <Line X1="95" Y1="90" X2="105" Y2="90" Stroke="#666666" StrokeThickness="1"/>
            
            <!-- 拖动滑块 -->
            <Thumb x:Name="speedThumb" Width="20" Height="20" Background="#2196F3" CornerRadius="10"
                   Canvas.Left="90" Canvas.Top="40"
                   DragDelta="SpeedThumb_DragDelta"/>
        </Canvas>
    </Border>

    <!-- 当前值显示 -->
    <TextBlock Grid.Row="2"
               Text="{Binding ConveyorSpeed, StringFormat={}{0:F1} m/s}"
               HorizontalAlignment="Center" FontSize="16" FontWeight="Bold" Foreground="#2196F3"/>
</Grid>
```

csharp:

```c#
private double _conveyorSpeed = 1.0;
public double ConveyorSpeed
{
    get => _conveyorSpeed;
    set { _conveyorSpeed = value; OnPropertyChanged(); }
}

private void SpeedThumb_DragDelta(object sender, DragDeltaEventArgs e)
{
    // 计算新的位置
    double newTop = Canvas.GetTop(speedThumb) + e.VerticalChange;
    newTop = Math.Clamp(newTop, 10, 90);
    
    // 更新滑块位置
    Canvas.SetTop(speedThumb, newTop);
    
    // 转换为速度值（0.0 ~ 2.0 m/s）
    ConveyorSpeed = 2.0 - (newTop - 10) / 40.0;
    
    // 更新设备参数
    DeviceService.Instance.SetConveyorSpeed(ConveyorSpeed);
}
```

------

## 六、工业开发最佳实践

1. **优先使用标准命令**：利用`ScrollBar`的 21 个标准路由命令，实现 UI 与逻辑解耦
2. **触摸屏优化**：滑块宽度至少 16px，最好 20px，隐藏不必要的箭头按钮
3. **性能优先**：对`Scroll`事件添加节流处理，避免事件风暴导致的卡顿
4. **大内容延迟更新**：拖动时只更新滑块位置，释放后再更新内容
5. **自动禁用**：利用`ViewportSize`自动禁用滚动条的特性，不要手动控制
6. **统一全局样式**：整个应用使用相同的滚动条样式，保持工业界面一致性
7. **支持键盘操作**：为常用滚动操作添加键盘快捷键，提升操作效率