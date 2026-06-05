# 004007003_RadioButton 枚举绑定 + 完整参数配置面板模板

这是**工业上位机开发中最常用、最优雅的 RadioButton 用法**，完全遵循 MVVM 架构，可直接复制到生产项目中使用。

------

## 一、RadioButton 与枚举类型的 MVVM 绑定最佳实践

### 为什么要绑定枚举？

工业场景中，**运行模式、通信速率、校验位、报警级别**等选项本质上都是枚举类型。传统的 "每个选项对应一个 bool 属性" 的写法：

- 代码冗余，扩展性差
- 新增选项需要修改多处代码
- 容易出现逻辑错误

**最佳方案：使用 `IValueConverter` 将枚举值直接与 RadioButton 的 `IsChecked` 绑定**

- 一个属性搞定所有选项
- 新增枚举值无需修改 ViewModel
- 类型安全，编译时检查

------

### 1. 第一步：定义枚举类型

csharp:

```c#
/// <summary>
/// 设备运行模式（工业场景典型枚举）
/// </summary>
public enum RunMode
{
    /// <summary>
    /// 手动模式
    /// </summary>
    Manual,
    
    /// <summary>
    /// 自动模式
    /// </summary>
    Auto,
    
    /// <summary>
    /// 调试模式
    /// </summary>
    Debug,
    
    /// <summary>
    /// 维护模式
    /// </summary>
    Maintenance
}

/// <summary>
/// 通信校验位
/// </summary>
public enum Parity
{
    None,
    Odd,
    Even
}
```

------

### 2. 第二步：实现通用枚举转换器（核心）

这是**可复用的通用转换器**，适用于所有枚举类型与 RadioButton 的绑定：

csharp:

```c#
using System;
using System.Globalization;
using System.Windows.Data;

/// <summary>
/// 通用枚举与 RadioButton IsChecked 转换器
/// 工业级标准实现，支持所有枚举类型
/// </summary>
[ValueConversion(typeof(Enum), typeof(bool))]
public class EnumToBooleanConverter : IValueConverter
{
    /// <summary>
    /// 枚举值 → IsChecked
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        // 将参数转换为枚举类型
        if (!Enum.TryParse(value.GetType(), parameter.ToString(), out object parameterValue))
            return false;

        // 比较当前值与参数值，相等则返回 true（选中）
        return value.Equals(parameterValue);
    }

    /// <summary>
    /// IsChecked → 枚举值
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null || (bool)value == false)
            return Binding.DoNothing;

        // 将参数转换为目标枚举类型并返回
        return Enum.Parse(targetType, parameter.ToString());
    }
}
```

------

### 3. 第三步：ViewModel 实现（极简）

**只需要一个枚举属性**，无需多个 bool 属性：

csharp:

```c#
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class DeviceConfigViewModel : INotifyPropertyChanged
{
    // 运行模式（一个属性搞定4个选项）
    private RunMode _currentRunMode = RunMode.Auto;
    public RunMode CurrentRunMode
    {
        get => _currentRunMode;
        set
        {
            if (_currentRunMode != value)
            {
                _currentRunMode = value;
                OnPropertyChanged();
                OnRunModeChanged(value); // 模式切换业务逻辑
            }
        }
    }

    // 校验位
    private Parity _currentParity = Parity.None;
    public Parity CurrentParity
    {
        get => _currentParity;
        set
        {
            if (_currentParity != value)
            {
                _currentParity = value;
                OnPropertyChanged();
                OnParityChanged(value);
            }
        }
    }

    // 通信速率
    private int _baudRate = 9600;
    public int BaudRate
    {
        get => _baudRate;
        set { _baudRate = value; OnPropertyChanged(); }
    }

    // 数据位
    private int _dataBits = 8;
    public int DataBits
    {
        get => _dataBits;
        set { _dataBits = value; OnPropertyChanged(); }
    }

    // 模式切换业务逻辑
    private void OnRunModeChanged(RunMode newMode)
    {
        switch (newMode)
        {
            case RunMode.Manual:
                ProductionSystem.StartManualMode();
                break;
            case RunMode.Auto:
                ProductionSystem.StartAutoMode();
                break;
            case RunMode.Debug:
                ProductionSystem.StartDebugMode();
                break;
            case RunMode.Maintenance:
                ProductionSystem.StartMaintenanceMode();
                break;
        }
    }

    private void OnParityChanged(Parity newParity)
    {
        // 更新串口配置
        SerialPortManager.UpdateParity(newParity);
    }

    // INotifyPropertyChanged 实现
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string prop = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
```

------

### 4. 第四步：XAML 绑定（一行代码一个选项）

xaml:

```xaml
<Window.Resources>
    <!-- 注册转换器 -->
    <local:EnumToBooleanConverter x:Key="EnumToBooleanConverter"/>
    <local:DeviceConfigViewModel x:Key="VM"/>
</Window.Resources>

<StackPanel DataContext="{StaticResource VM}" Margin="20">
    <TextBlock Text="运行模式:" FontWeight="Bold" Margin="0 0 0 5"/>
    
    <!-- 4个RadioButton绑定同一个枚举属性，通过Converter参数区分 -->
    <RadioButton Content="手动模式" 
                 IsChecked="{Binding CurrentRunMode, Converter={StaticResource EnumToBooleanConverter}, ConverterParameter=Manual}"/>
    
    <RadioButton Content="自动模式" 
                 IsChecked="{Binding CurrentRunMode, Converter={StaticResource EnumToBooleanConverter}, ConverterParameter=Auto}"/>
    
    <RadioButton Content="调试模式" 
                 IsChecked="{Binding CurrentRunMode, Converter={StaticResource EnumToBooleanConverter}, ConverterParameter=Debug}"/>
    
    <RadioButton Content="维护模式" 
                 IsChecked="{Binding CurrentRunMode, Converter={StaticResource EnumToBooleanConverter}, ConverterParameter=Maintenance}"/>
</StackPanel>
```

------

### 5. 优势总结

✅ **代码极简**：一个枚举属性对应 N 个选项

✅ **扩展性强**：新增枚举值只需加一行 XAML，无需修改 ViewModel

✅ **类型安全**：编译时检查，避免字符串错误

✅ **业务逻辑集中**：所有模式切换逻辑都在 ViewModel 的属性 setter 中

✅ **完全符合 MVVM**：无后台代码，纯绑定驱动

------

## 二、完整工业级参数配置面板模板

这是**可直接用于生产项目**的参数配置面板，包含：

- 运行模式单选组（枚举绑定）
- 通信参数配置（波特率、数据位、校验位）
- 报警参数配置
- 操作按钮区
- 工业风格样式
- 响应式布局

------

### 1. 完整 XAML 代码

xaml:

```xaml
<Window x:Class="IndustrialConfigPanel.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:IndustrialConfigPanel"
        Title="设备参数配置" Height="500" Width="600"
        WindowStartupLocation="CenterScreen">

    <Window.Resources>
        <local:EnumToBooleanConverter x:Key="EnumToBooleanConverter"/>
        <local:DeviceConfigViewModel x:Key="VM"/>

        <!-- 工业风格样式 -->
        <Style TargetType="GroupBox">
            <Setter Property="Margin" Value="10"/>
            <Setter Property="Padding" Value="10"/>
            <Setter Property="FontWeight" Value="Bold"/>
        </Style>

        <Style TargetType="RadioButton">
            <Setter Property="Margin" Value="0 5"/>
            <Setter Property="FontWeight" Value="Normal"/>
        </Style>

        <Style TargetType="TextBox">
            <Setter Property="Height" Value="25"/>
            <Setter Property="Margin" Value="0 5"/>
        </Style>

        <Style TargetType="Button">
            <Setter Property="Width" Value="80"/>
            <Setter Property="Height" Value="30"/>
            <Setter Property="Margin" Value="5"/>
        </Style>
    </Window.Resources>

    <Grid DataContext="{StaticResource VM}">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- 1. 运行模式配置 -->
        <GroupBox Grid.Row="0" Header="运行模式">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>

                <StackPanel Grid.Column="0">
                    <RadioButton Content="手动模式" 
                                 IsChecked="{Binding CurrentRunMode, Converter={StaticResource EnumToBooleanConverter}, ConverterParameter=Manual}"/>
                    <RadioButton Content="自动模式" 
                                 IsChecked="{Binding CurrentRunMode, Converter={StaticResource EnumToBooleanConverter}, ConverterParameter=Auto}"/>
                </StackPanel>

                <StackPanel Grid.Column="1">
                    <RadioButton Content="调试模式" 
                                 IsChecked="{Binding CurrentRunMode, Converter={StaticResource EnumToBooleanConverter}, ConverterParameter=Debug}"/>
                    <RadioButton Content="维护模式" 
                                 IsChecked="{Binding CurrentRunMode, Converter={StaticResource EnumToBooleanConverter}, ConverterParameter=Maintenance}"/>
                </StackPanel>
            </Grid>
        </GroupBox>

        <!-- 2. 通信参数配置 -->
        <GroupBox Grid.Row="1" Header="通信参数">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto" SharedSizeGroup="Label"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto" SharedSizeGroup="Label"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>

                <TextBlock Grid.Column="0" Text="波特率:" VerticalAlignment="Center"/>
                <TextBox Grid.Column="1" Text="{Binding BaudRate}"/>

                <TextBlock Grid.Column="2" Text="数据位:" VerticalAlignment="Center" Margin="10 0 0 0"/>
                <TextBox Grid.Column="3" Text="{Binding DataBits}"/>

                <TextBlock Grid.Row="1" Grid.Column="0" Text="校验位:" VerticalAlignment="Center" Margin="0 10 0 0"/>
                <StackPanel Grid.Row="1" Grid.Column="1" Orientation="Horizontal" Margin="0 10 0 0">
                    <RadioButton Content="无" 
                                 IsChecked="{Binding CurrentParity, Converter={StaticResource EnumToBooleanConverter}, ConverterParameter=None}"
                                 Margin="0 0 10 0"/>
                    <RadioButton Content="奇" 
                                 IsChecked="{Binding CurrentParity, Converter={StaticResource EnumToBooleanConverter}, ConverterParameter=Odd}"
                                 Margin="0 0 10 0"/>
                    <RadioButton Content="偶" 
                                 IsChecked="{Binding CurrentParity, Converter={StaticResource EnumToBooleanConverter}, ConverterParameter=Even}"/>
                </StackPanel>
            </Grid>
        </GroupBox>

        <!-- 3. 报警参数配置 -->
        <GroupBox Grid.Row="2" Header="报警参数">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto" SharedSizeGroup="Label"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto" SharedSizeGroup="Label"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>

                <TextBlock Grid.Column="0" Text="温度上限(℃):" VerticalAlignment="Center"/>
                <TextBox Grid.Column="1" Text="{Binding TemperatureUpper}"/>

                <TextBlock Grid.Column="2" Text="温度下限(℃):" VerticalAlignment="Center" Margin="10 0 0 0"/>
                <TextBox Grid.Column="3" Text="{Binding TemperatureLower}"/>

                <TextBlock Grid.Row="1" Grid.Column="0" Text="压力上限(MPa):" VerticalAlignment="Center" Margin="0 10 0 0"/>
                <TextBox Grid.Row="1" Grid.Column="1" Text="{Binding PressureUpper}" Margin="0 10 0 0"/>

                <TextBlock Grid.Row="1" Grid.Column="2" Text="压力下限(MPa):" VerticalAlignment="Center" Margin="10 10 0 0"/>
                <TextBox Grid.Row="1" Grid.Column="3" Text="{Binding PressureLower}" Margin="0 10 0 0"/>
            </Grid>
        </GroupBox>

        <!-- 4. 操作按钮区 -->
        <DockPanel Grid.Row="4" LastChildFill="False" HorizontalAlignment="Right" Margin="0 10">
            <Button Content="保存配置" Command="{Binding SaveConfigCommand}" Background="#2ECC71" Foreground="White"/>
            <Button Content="加载配置" Command="{Binding LoadConfigCommand}"/>
            <Button Content="恢复默认" Command="{Binding RestoreDefaultCommand}"/>
            <Button Content="应用" Command="{Binding ApplyCommand}" Background="#3498DB" Foreground="White"/>
        </DockPanel>
    </Grid>
</Window>
```

------

### 2. 完整 ViewModel 补充

csharp:

```c#
public class DeviceConfigViewModel : INotifyPropertyChanged
{
    // 运行模式
    private RunMode _currentRunMode = RunMode.Auto;
    public RunMode CurrentRunMode
    {
        get => _currentRunMode;
        set { _currentRunMode = value; OnPropertyChanged(); OnRunModeChanged(value); }
    }

    // 通信参数
    private int _baudRate = 9600;
    public int BaudRate
    {
        get => _baudRate;
        set { _baudRate = value; OnPropertyChanged(); }
    }

    private int _dataBits = 8;
    public int DataBits
    {
        get => _dataBits;
        set { _dataBits = value; OnPropertyChanged(); }
    }

    private Parity _currentParity = Parity.None;
    public Parity CurrentParity
    {
        get => _currentParity;
        set { _currentParity = value; OnPropertyChanged(); }
    }

    // 报警参数
    private double _temperatureUpper = 80.0;
    public double TemperatureUpper
    {
        get => _temperatureUpper;
        set { _temperatureUpper = value; OnPropertyChanged(); }
    }

    private double _temperatureLower = 20.0;
    public double TemperatureLower
    {
        get => _temperatureLower;
        set { _temperatureLower = value; OnPropertyChanged(); }
    }

    private double _pressureUpper = 1.0;
    public double PressureUpper
    {
        get => _pressureUpper;
        set { _pressureUpper = value; OnPropertyChanged(); }
    }

    private double _pressureLower = 0.1;
    public double PressureLower
    {
        get => _pressureLower;
        set { _pressureLower = value; OnPropertyChanged(); }
    }

    // 命令（使用DelegateCommand）
    public ICommand SaveConfigCommand { get; }
    public ICommand LoadConfigCommand { get; }
    public ICommand RestoreDefaultCommand { get; }
    public ICommand ApplyCommand { get; }

    public DeviceConfigViewModel()
    {
        // 初始化命令
        SaveConfigCommand = new DelegateCommand(SaveConfig);
        LoadConfigCommand = new DelegateCommand(LoadConfig);
        RestoreDefaultCommand = new DelegateCommand(RestoreDefault);
        ApplyCommand = new DelegateCommand(Apply);
    }

    // 命令实现
    private void SaveConfig()
    {
        // 保存配置到文件或数据库
        ConfigManager.Save(this);
        MessageBox.Show("配置保存成功");
    }

    private void LoadConfig()
    {
        // 从文件或数据库加载配置
        var config = ConfigManager.Load();
        if (config != null)
        {
            CurrentRunMode = config.CurrentRunMode;
            BaudRate = config.BaudRate;
            DataBits = config.DataBits;
            CurrentParity = config.CurrentParity;
            TemperatureUpper = config.TemperatureUpper;
            TemperatureLower = config.TemperatureLower;
            PressureUpper = config.PressureUpper;
            PressureLower = config.PressureLower;
        }
    }

    private void RestoreDefault()
    {
        // 恢复默认配置
        CurrentRunMode = RunMode.Auto;
        BaudRate = 9600;
        DataBits = 8;
        CurrentParity = Parity.None;
        TemperatureUpper = 80.0;
        TemperatureLower = 20.0;
        PressureUpper = 1.0;
        PressureLower = 0.1;
    }

    private void Apply()
    {
        // 应用配置到设备
        SerialPortManager.UpdateConfig(this);
        MessageBox.Show("配置已应用到设备");
    }

    // 业务逻辑
    private void OnRunModeChanged(RunMode newMode)
    {
        // 模式切换逻辑
    }

    // INotifyPropertyChanged
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
    private readonly Func<bool> _canExecute;

    public DelegateCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object parameter) => _execute();

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}
```

------

## 三、工业开发最佳实践总结

1. **永远使用枚举绑定 RadioButton**：不要用多个 bool 属性，扩展性和可维护性差
2. **使用通用转换器**：一个转换器适用于所有枚举类型，避免重复代码
3. **业务逻辑集中在 ViewModel**：所有模式切换、参数更新逻辑都放在属性 setter 或命令中
4. **使用共享尺寸组**：让标签列宽度一致，界面更整齐
5. **统一控件样式**：整个应用内的 RadioButton、TextBox、Button 样式保持一致
6. **添加输入验证**：对数值输入进行范围验证，避免非法值
7. **支持配置保存 / 加载**：工业设备必须支持配置的持久化
8. **使用命令代替事件**：完全遵循 MVVM 架构，提高代码可测试性

------

## 四、扩展建议

- 可以将 `EnumToBooleanConverter` 放到通用类库中，供所有项目复用
- 对于复杂的参数配置，可以使用 `TabControl` 分标签页显示
- 添加输入验证功能，使用 `IDataErrorInfo` 或 `INotifyDataErrorInfo`
- 实现配置的版本管理，支持回滚到历史版本
- 添加权限控制，不同用户可以修改不同的参数