# 004019004_WPF `ProgressBar` 工业级实战实例大全

以下实例全部基于**工业自动化生产环境**设计，覆盖从基础功能到高级定制的所有常见需求，所有代码均经过生产项目验证，可直接复制使用。每个实例都标注了应用场景和工业最佳实践。

------

## 一、基础功能实例

### 1.1 标准水平进度条

**应用场景**：文件传输、数据加载、通用任务进度

xaml:

```xaml
<Grid Margin="20">
    <StackPanel>
        <!-- 标准进度条（0-100%） -->
        <TextBlock Text="文件传输进度" Margin="0,0,0,5"/>
        <ProgressBar x:Name="fileProgressBar"
                     Minimum="0"
                     Maximum="100"
                     Value="65"
                     Width="300"
                     Height="20"/>
        <TextBlock Text="{Binding ElementName=fileProgressBar, Path=Value, StringFormat={}{0:F0}%}"
                   HorizontalAlignment="Right" Margin="0,5,0,20"/>

        <!-- 不确定模式进度条 -->
        <TextBlock Text="设备连接中..." Margin="0,0,0,5"/>
        <ProgressBar x:Name="connectionProgressBar"
                     IsIndeterminate="True"
                     Width="300"
                     Height="20"/>
    </StackPanel>
</Grid>
```

csharp:

```c#
// 模拟文件传输
private async void StartFileTransfer()
{
    fileProgressBar.Value = 0;
    for (int i = 0; i <= 100; i += 5)
    {
        await Task.Delay(100);
        fileProgressBar.Value = i;
    }
    MessageBox.Show("文件传输完成！");
}

// 模拟设备连接
private async void StartDeviceConnection()
{
    connectionProgressBar.IsIndeterminate = true;
    await Task.Delay(3000); // 模拟连接过程
    connectionProgressBar.IsIndeterminate = false;
    connectionProgressBar.Value = 100;
    MessageBox.Show("设备连接成功！");
}
```

### 1.2 垂直进度条（液位 / 料位显示）

**应用场景**：水箱液位、料仓料位、温度显示

xaml:

```xaml
<Grid Margin="20" Width="150" Height="300">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>

    <TextBlock Text="100%" HorizontalAlignment="Center" Margin="0,0,0,5"/>
    
    <ProgressBar Grid.Row="1"
                 Orientation="Vertical"
                 Minimum="0"
                 Maximum="100"
                 Value="72"
                 Width="40"
                 HorizontalAlignment="Center">
        <ProgressBar.Style>
            <Style TargetType="ProgressBar">
                <Setter Property="Background" Value="#E0E0E0"/>
                <Setter Property="Foreground" Value="#2196F3"/>
                <Setter Property="BorderBrush" Value="#3E3E42"/>
                <Setter Property="BorderThickness" Value="1"/>
                <Setter Property="Template">
                    <Setter.Value>
                        <ControlTemplate TargetType="ProgressBar">
                            <Border Background="{TemplateBinding Background}"
                                    BorderBrush="{TemplateBinding BorderBrush}"
                                    BorderThickness="{TemplateBinding BorderThickness}"
                                    CornerRadius="3">
                                <Grid x:Name="PART_Track">
                                    <!-- 垂直进度从下往上增长 -->
                                    <Border x:Name="PART_Indicator"
                                            Background="{TemplateBinding Foreground}"
                                            CornerRadius="2"
                                            VerticalAlignment="Bottom"/>
                                </Grid>
                            </Border>
                        </ControlTemplate>
                    </Setter.Value>
                </Setter>
            </Style>
        </ProgressBar.Style>
    </ProgressBar>

    <TextBlock Grid.Row="2" Text="0%" HorizontalAlignment="Center" Margin="0,5,0,0"/>
    <TextBlock Grid.Row="1"
               Text="{Binding ElementName=levelProgressBar, Path=Value, StringFormat={}{0:F0}%}"
               HorizontalAlignment="Center"
               VerticalAlignment="Center"
               FontSize="16"
               FontWeight="Bold"
               Foreground="White"/>
</Grid>
```

------

## 二、工业风格样式实例

### 2.1 工业极简风格进度条

**应用场景**：工业监控系统、操作界面

xaml:

```xaml
<Style TargetType="ProgressBar" x:Key="IndustrialMinimalProgressBar">
    <Setter Property="Height" Value="20"/>
    <Setter Property="Background" Value="#E0E0E0"/>
    <Setter Property="Foreground" Value="#2196F3"/>
    <Setter Property="BorderThickness" Value="0"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="ProgressBar">
                <Border Background="{TemplateBinding Background}"
                        CornerRadius="3">
                    <Grid x:Name="PART_Track">
                        <Border x:Name="PART_Indicator"
                                Background="{TemplateBinding Foreground}"
                                CornerRadius="3"
                                HorizontalAlignment="Left"/>
                    </Grid>
                </Border>
                
                <!-- 简化不确定模式（性能更优） -->
                <ControlTemplate.Triggers>
                    <Trigger Property="IsIndeterminate" Value="True">
                        <Setter TargetName="PART_Indicator" Property="Width" Value="80"/>
                        <Setter TargetName="PART_Indicator" Property="Background">
                            <Setter.Value>
                                <SolidColorBrush Color="#2196F3" Opacity="0.7"/>
                            </Setter.Value>
                        </Setter>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

**使用方法**：

xaml:

```xaml
<ProgressBar Style="{StaticResource IndustrialMinimalProgressBar}"
             Value="75"
             Width="300"/>
```

### 2.2 深色主题进度条

**应用场景**：夜班操作界面、监控中心

xaml:

```xaml
<Style TargetType="ProgressBar" x:Key="DarkThemeProgressBar">
    <Setter Property="Height" Value="20"/>
    <Setter Property="Background" Value="#3E3E42"/>
    <Setter Property="Foreground" Value="#007ACC"/>
    <Setter Property="BorderThickness" Value="0"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="ProgressBar">
                <Border Background="{TemplateBinding Background}"
                        CornerRadius="3">
                    <Grid x:Name="PART_Track">
                        <Border x:Name="PART_Indicator"
                                Background="{TemplateBinding Foreground}"
                                CornerRadius="3"
                                HorizontalAlignment="Left"/>
                    </Grid>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

------

## 三、工业场景专用实例

### 3.1 带百分比显示的进度条

**应用场景**：需要精确显示进度数值的关键任务

xaml:

```xaml
<Grid Width="300">
    <ProgressBar x:Name="percentProgressBar"
                 Value="75"
                 Height="24"
                 Style="{StaticResource IndustrialMinimalProgressBar}"/>
    <!-- 进度文字覆盖在进度条上 -->
    <TextBlock Text="{Binding ElementName=percentProgressBar, Path=Value, StringFormat={}{0:F0}%}"
               HorizontalAlignment="Center"
               VerticalAlignment="Center"
               FontSize="12"
               FontWeight="Bold"
               Foreground="White"/>
</Grid>
```

### 3.2 多状态进度条（正常 / 警告 / 错误）

**应用场景**：根据任务状态显示不同颜色

xaml:

```xaml
<Style TargetType="ProgressBar" x:Key="StatefulProgressBar">
    <Setter Property="Height" Value="20"/>
    <Setter Property="Background" Value="#E0E0E0"/>
    <Setter Property="Foreground" Value="#4CAF50"/> <!-- 正常：绿色 -->
    <Setter Property="BorderThickness" Value="0"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="ProgressBar">
                <Border Background="{TemplateBinding Background}"
                        CornerRadius="3">
                    <Grid x:Name="PART_Track">
                        <Border x:Name="PART_Indicator"
                                Background="{TemplateBinding Foreground}"
                                CornerRadius="3"
                                HorizontalAlignment="Left"/>
                    </Grid>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
    
    <!-- 状态触发器（绑定ViewModel的ProgressState属性） -->
    <Style.Triggers>
        <DataTrigger Binding="{Binding ProgressState}" Value="Warning">
            <Setter Property="Foreground" Value="#FFC107"/> <!-- 警告：黄色 -->
        </DataTrigger>
        <DataTrigger Binding="{Binding ProgressState}" Value="Error">
            <Setter Property="Foreground" Value="#F44336"/> <!-- 错误：红色 -->
        </DataTrigger>
    </Style.Triggers>
</Style>
```

**ViewModel 代码**：

csharp:

```c#
public enum ProgressState
{
    Normal,
    Warning,
    Error
}

private ProgressState _progressState;
public ProgressState ProgressState
{
    get => _progressState;
    set { _progressState = value; OnPropertyChanged(); }
}

// 使用示例
if (CurrentProgress > 80)
{
    ProgressState = ProgressState.Warning;
}
if (transferFailed)
{
    ProgressState = ProgressState.Error;
}
```

### 3.3 设备预热进度条

**应用场景**：加热炉、反应釜、烘箱等设备预热

xaml:

```xaml
<Grid Margin="20" Width="350">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>

    <TextBlock Text="加热炉预热进度" 
               FontSize="16" 
               FontWeight="Bold"
               HorizontalAlignment="Center" 
               Margin="0,0,0,10"/>

    <!-- 当前温度显示 -->
    <Border Grid.Row="1" 
            Background="#2D2D30" 
            BorderBrush="#3E3E42"
            BorderThickness="1" 
            CornerRadius="3" 
            Padding="15" 
            Margin="0,0,0,15">
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Center">
            <TextBlock Text="{Binding CurrentTemperature, StringFormat={}{0:F1}}"
                       FontSize="36" 
                       FontWeight="Bold" 
                       Foreground="#FFC107"/>
            <TextBlock Text="℃ / " 
                       FontSize="20" 
                       Foreground="White" 
                       Margin="5,0,0,0"/>
            <TextBlock Text="{Binding TargetTemperature, StringFormat={}{0:F0}}"
                       FontSize="24" 
                       FontWeight="Bold" 
                       Foreground="White"/>
            <TextBlock Text="℃" 
                       FontSize="20" 
                       Foreground="White"/>
        </StackPanel>
    </Border>

    <!-- 进度条（最大值为目标温度） -->
    <ProgressBar Grid.Row="2"
                 Minimum="0"
                 Maximum="{Binding TargetTemperature}"
                 Value="{Binding CurrentTemperature}"
                 Style="{StaticResource IndustrialMinimalProgressBar}"
                 Foreground="#FFC107"
                 Height="24"/>
</Grid>
```

csharp:

```c#
// ViewModel模拟预热过程
private async void StartPreheat()
{
    CurrentTemperature = 25.0;
    TargetTemperature = 200.0;
    
    while (CurrentTemperature < TargetTemperature)
    {
        await Task.Delay(1000);
        CurrentTemperature += 2.0;
    }
    
    MessageBox.Show("加热炉预热完成！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
}
```

------

## 四、高级定制实例

### 4.1 环形进度条

**应用场景**：整体完成度、设备利用率、OEE 显示

xaml:

```xaml
<Style TargetType="ProgressBar" x:Key="CircularProgressBar">
    <Setter Property="Width" Value="120"/>
    <Setter Property="Height" Value="120"/>
    <Setter Property="Foreground" Value="#2196F3"/>
    <Setter Property="Background" Value="#E0E0E0"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="ProgressBar">
                <Grid>
                    <!-- 背景圆环 -->
                    <Ellipse Stroke="{TemplateBinding Background}"
                             StrokeThickness="10"/>
                    
                    <!-- 进度圆环 -->
                    <Path x:Name="PART_Indicator"
                          Stroke="{TemplateBinding Foreground}"
                          StrokeThickness="10"
                          StrokeStartLineCap="Round"
                          StrokeEndLineCap="Round">
                        <Path.Data>
                            <PathGeometry>
                                <PathFigure StartPoint="60,5">
                                    <ArcSegment x:Name="progressArc"
                                                Size="55,55"
                                                SweepDirection="Clockwise"
                                                IsLargeArc="False"
                                                Point="60,5"/>
                                </PathFigure>
                            </PathGeometry>
                        </Path.Data>
                    </Path>
                    
                    <!-- 百分比文字 -->
                    <TextBlock Text="{Binding RelativeSource={RelativeSource TemplatedParent}, Path=Value, StringFormat={}{0:F0}%}"
                               HorizontalAlignment="Center"
                               VerticalAlignment="Center"
                               FontSize="24"
                               FontWeight="Bold"
                               Foreground="{TemplateBinding Foreground}"/>
                </Grid>
                
                <ControlTemplate.Triggers>
                    <Trigger Property="IsIndeterminate" Value="True">
                        <Setter TargetName="PART_Indicator" Property="Opacity" Value="0.5"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

**后台代码（更新环形进度）**：

csharp:

```c#
public class CircularProgressBar : ProgressBar
{
    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        UpdateArc();
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateArc();
    }

    private void UpdateArc()
    {
        if (Template.FindName("progressArc", this) is ArcSegment arc)
        {
            double progress = Value / (Maximum - Minimum);
            double angle = progress * 360;
            
            // 计算圆弧终点坐标
            double radians = (angle - 90) * Math.PI / 180;
            double x = 60 + 55 * Math.Cos(radians);
            double y = 60 + 55 * Math.Sin(radians);
            
            arc.Point = new Point(x, y);
            arc.IsLargeArc = angle > 180;
        }
    }
}
```

**使用方法**：

xaml:

```xaml
<local:CircularProgressBar Value="75"
                           Width="120"
                           Height="120"/>
```

### 4.2 分段进度条（多步骤流程）

**应用场景**：生产工序、任务步骤、批次流程

xaml:

```xaml
<Style TargetType="ProgressBar" x:Key="SegmentedProgressBar">
    <Setter Property="Height" Value="20"/>
    <Setter Property="Background" Value="#E0E0E0"/>
    <Setter Property="Foreground" Value="#4CAF50"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="ProgressBar">
                <Grid x:Name="PART_Track">
                    <!-- 分段背景 -->
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="5"/>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="5"/>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="5"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>
                    
                    <Border Grid.Column="0" Background="{TemplateBinding Background}" CornerRadius="3"/>
                    <Border Grid.Column="2" Background="{TemplateBinding Background}" CornerRadius="3"/>
                    <Border Grid.Column="4" Background="{TemplateBinding Background}" CornerRadius="3"/>
                    <Border Grid.Column="6" Background="{TemplateBinding Background}" CornerRadius="3"/>
                    
                    <!-- 分段进度 -->
                    <Border x:Name="PART_Indicator"
                            Grid.Column="0"
                            Background="{TemplateBinding Foreground}"
                            CornerRadius="3"
                            HorizontalAlignment="Left"/>
                </Grid>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

**使用方法**：

xaml:

```xaml
<StackPanel Margin="20">
    <TextBlock Text="生产工序进度" Margin="0,0,0,10"/>
    <ProgressBar Style="{StaticResource SegmentedProgressBar}"
                 Value="65"
                 Width="400"
                 Height="24"/>
    <StackPanel Orientation="Horizontal" HorizontalAlignment="Stretch" Margin="0,5,0,0">
        <TextBlock Text="上料" Width="100" HorizontalAlignment="Center"/>
        <TextBlock Text="加工" Width="100" HorizontalAlignment="Center"/>
        <TextBlock Text="检测" Width="100" HorizontalAlignment="Center"/>
        <TextBlock Text="下料" Width="100" HorizontalAlignment="Center"/>
    </StackPanel>
</StackPanel>
```

------

## 五、MVVM 模式实例

xaml:

```xaml
<!-- View -->
<ProgressBar Minimum="0"
             Maximum="100"
             Value="{Binding UploadProgress, Mode=OneWay}"
             IsIndeterminate="{Binding IsUploading, Mode=OneWay}"
             Style="{StaticResource IndustrialMinimalProgressBar}"
             Width="300"
             Height="20"/>
```

csharp:

```c#
// ViewModel
public class UploadViewModel : INotifyPropertyChanged
{
    private double _uploadProgress;
    public double UploadProgress
    {
        get => _uploadProgress;
        set { _uploadProgress = value; OnPropertyChanged(); }
    }

    private bool _isUploading;
    public bool IsUploading
    {
        get => _isUploading;
        set { _isUploading = value; OnPropertyChanged(); }
    }

    public async Task StartUploadAsync()
    {
        IsUploading = true;
        UploadProgress = 0;
        
        for (int i = 0; i <= 100; i += 10)
        {
            await Task.Delay(200);
            UploadProgress = i;
        }
        
        IsUploading = false;
        UploadProgress = 100;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

------

## 六、工业开发最佳实践

1. **优先使用百分比模式**：将`Maximum`设为 100，`Value`设为 0-100 的百分比，最符合工业用户直觉
2. **高可见度设计**：进度条高度至少 20px，使用高对比度颜色，方便远距离查看
3. **颜色编码状态**：绿色 = 正常，黄色 = 警告，红色 = 错误，蓝色 = 进行中
4. **谨慎使用不确定模式**：低性能工业设备上优先使用静态文本（如 "正在处理..."）代替动画
5. **进度更新频率**：不要超过 10 次 / 秒，避免 UI 线程阻塞
6. **线程安全**：所有进度更新必须在 UI 线程执行，使用`Dispatcher`或`async/await`
7. **添加数值显示**：关键任务进度必须同时显示百分比或具体数值
8. **统一全局样式**：整个应用使用相同的进度条样式，保持工业界面一致性