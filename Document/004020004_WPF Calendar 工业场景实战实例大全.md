# 004020004_WPF `Calendar` 工业场景实战实例大全

以下实例全部面向**工业自动化生产环境**设计，覆盖基础查询、样式定制、业务场景三大类需求，所有代码均经过生产项目验证，可直接复制复用。每个实例均标注应用场景，并对应官方类定义中的核心属性 / 事件。

------

## 一、基础功能实例

### 1.1 单日期生产数据查询

**应用场景**：查询指定日期的产量、良率、设备状态等日报数据

xml:

```xaml
<Grid Margin="20">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>

    <!-- 日历选择区 -->
    <Calendar x:Name="dailyCalendar"
              Grid.Column="0"
              Width="280"
              Height="240"
              SelectionMode="SingleDate"
              FirstDayOfWeek="Monday"
              IsTodayHighlighted="True"
              SelectedDatesChanged="DailyCalendar_SelectedDatesChanged"
              Margin="0,0,20,0"/>

    <!-- 数据展示区 -->
    <Border Grid.Column="1"
            Background="White"
            BorderBrush="#DDDDDD"
            BorderThickness="1"
            CornerRadius="3">
        <StackPanel Margin="15">
            <TextBlock x:Name="dateTitle"
                       Text="请选择日期查看生产数据"
                       FontSize="16"
                       FontWeight="Bold"
                       Margin="0,0,0,15"/>
            
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                
                <StackPanel Grid.Column="0">
                    <TextBlock Text="计划产量" Foreground="#666666"/>
                    <TextBlock x:Name="planQtyText" Text="-" FontSize="20" FontWeight="Bold"/>
                </StackPanel>
                
                <StackPanel Grid.Column="1">
                    <TextBlock Text="实际产量" Foreground="#666666"/>
                    <TextBlock x:Name="actualQtyText" Text="-" FontSize="20" FontWeight="Bold" Foreground="#2196F3"/>
                </StackPanel>
                
                <StackPanel Grid.Column="2">
                    <TextBlock Text="良率" Foreground="#666666"/>
                    <TextBlock x:Name="yieldText" Text="-" FontSize="20" FontWeight="Bold" Foreground="#4CAF50"/>
                </StackPanel>
            </Grid>
        </StackPanel>
    </Border>
</Grid>
```

csharp:

```c#
private void DailyCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
{
    if (dailyCalendar.SelectedDate.HasValue)
    {
        DateTime date = dailyCalendar.SelectedDate.Value;
        dateTitle.Text = $"生产日报 - {date:yyyy年MM月dd日}";
        
        // 从业务服务获取当日生产数据
        var data = ProductionService.GetDailyReport(date);
        planQtyText.Text = data.PlanQuantity.ToString();
        actualQtyText.Text = data.ActualQuantity.ToString();
        yieldText.Text = $"{data.YieldRate:F2}%";
    }
}
```

### 1.2 日期范围批次查询

**应用场景**：查询指定时间段内的批次记录、质检报告、设备运行日志

xaml:

```xaml
<Grid Margin="20">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>

    <!-- 快捷选择栏 -->
    <StackPanel Orientation="Horizontal" Margin="0,0,0,10">
        <Button Content="近7天" Click="QuickRange_Click" Tag="7" Style="{StaticResource IndustrialButtonStyle}" Width="60" Margin="0,0,5,0"/>
        <Button Content="近30天" Click="QuickRange_Click" Tag="30" Style="{StaticResource IndustrialButtonStyle}" Width="60" Margin="0,0,5,0"/>
        <Button Content="本月" Click="QuickRange_Click" Tag="Month" Style="{StaticResource IndustrialButtonStyle}" Width="60" Margin="0,0,20,0"/>
        <TextBlock Text="已选范围：" VerticalAlignment="Center"/>
        <TextBlock x:Name="rangeText" Text="未选择" VerticalAlignment="Center" FontWeight="Bold"/>
    </StackPanel>

    <!-- 范围选择日历 -->
    <Calendar x:Name="rangeCalendar"
              Grid.Row="1"
              SelectionMode="SingleRange"
              FirstDayOfWeek="Monday"
              SelectedDatesChanged="RangeCalendar_SelectedDatesChanged"
              Margin="0,0,0,10"/>

    <!-- 批次数据表格 -->
    <DataGrid Grid.Row="2"
              x:Name="batchGrid"
              AutoGenerateColumns="False"
              IsReadOnly="True"
              GridLinesVisibility="Horizontal">
        <DataGrid.Columns>
            <DataGridTextColumn Header="批次号" Binding="{Binding BatchNo}" Width="150"/>
            <DataGridTextColumn Header="生产日期" Binding="{Binding ProductionDate, StringFormat={}{0:yyyy-MM-dd}}" Width="120"/>
            <DataGridTextColumn Header="产品型号" Binding="{Binding ProductModel}" Width="150"/>
            <DataGridTextColumn Header="数量" Binding="{Binding Quantity}" Width="100"/>
            <DataGridTextColumn Header="状态" Binding="{Binding Status}" Width="100"/>
        </DataGrid.Columns>
    </DataGrid>
</Grid>
```

csharp:

```c#
private void RangeCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
{
    if (rangeCalendar.SelectedDates.Count == 0) return;
    
    DateTime start = rangeCalendar.SelectedDates[0];
    DateTime end = rangeCalendar.SelectedDates[rangeCalendar.SelectedDates.Count - 1];
    rangeText.Text = $"{start:yyyy-MM-dd} 至 {end:yyyy-MM-dd}";
    
    // 查询区间批次数据
    batchGrid.ItemsSource = BatchService.QueryBatchList(start, end);
}

private void QuickRange_Click(object sender, RoutedEventArgs e)
{
    string tag = (sender as Button).Tag.ToString();
    DateTime end = DateTime.Today;
    DateTime start;
    
    switch (tag)
    {
        case "7": start = end.AddDays(-6); break;
        case "30": start = end.AddDays(-29); break;
        case "Month": start = new DateTime(end.Year, end.Month, 1); break;
        default: start = end; break;
    }
    
    // 选中日期范围
    rangeCalendar.SelectedDates.Clear();
    rangeCalendar.SelectedDates.AddRange(start, end);
}
```

### 1.3 禁用不可选日期（BlackoutDates）

**应用场景**：禁用周末、节假日、已停产日期，避免操作人员误选

csharp:

```c#
public MainWindow()
{
    InitializeComponent();
    InitBlackoutDates();
}

private void InitBlackoutDates()
{
    // 1. 禁用所有周末
    DateTime start = new DateTime(2025, 1, 1);
    DateTime end = new DateTime(2025, 12, 31);
    
    for (DateTime date = start; date <= end; date = date.AddDays(1))
    {
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
        {
            productionCalendar.BlackoutDates.Add(new CalendarDateRange(date));
        }
    }
    
    // 2. 禁用法定节假日
    var holidays = new List<DateTime>
    {
        new DateTime(2025, 1, 1),   // 元旦
        new DateTime(2025, 5, 1),   // 劳动节
        new DateTime(2025, 10, 1),  // 国庆节
    };
    
    foreach (var holiday in holidays)
    {
        productionCalendar.BlackoutDates.Add(new CalendarDateRange(holiday));
    }
    
    // 3. 限制只能选择近1年的日期
    productionCalendar.DisplayDateStart = DateTime.Today.AddYears(-1);
    productionCalendar.DisplayDateEnd = DateTime.Today;
}
```

------

## 二、样式定制实例

### 2.1 工业极简风格日历

**应用场景**：工业监控界面、深色主题系统，去除多余装饰，突出核心信息

xaml:

```xaml
<Style TargetType="Calendar" x:Key="IndustrialCalendarStyle">
    <Setter Property="Background" Value="White"/>
    <Setter Property="BorderBrush" Value="#E0E0E0"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Padding" Value="10"/>
    <Setter Property="FontSize" Value="12"/>
    <Setter Property="FirstDayOfWeek" Value="Monday"/>
    
    <!-- 日期按钮样式 -->
    <Setter Property="CalendarDayButtonStyle">
        <Setter.Value>
            <Style TargetType="CalendarDayButton">
                <Setter Property="Background" Value="Transparent"/>
                <Setter Property="BorderThickness" Value="0"/>
                <Setter Property="Padding" Value="6"/>
                <Setter Property="HorizontalContentAlignment" Value="Center"/>
                <Setter Property="VerticalContentAlignment" Value="Center"/>
                <Setter Property="FontSize" Value="12"/>
                
                <Style.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter Property="Background" Value="#E3F2FD"/>
                    </Trigger>
                    <Trigger Property="IsSelected" Value="True">
                        <Setter Property="Background" Value="#2196F3"/>
                        <Setter Property="Foreground" Value="White"/>
                    </Trigger>
                    <Trigger Property="IsToday" Value="True">
                        <Setter Property="BorderBrush" Value="#2196F3"/>
                        <Setter Property="BorderThickness" Value="1"/>
                        <Setter Property="FontWeight" Value="Bold"/>
                    </Trigger>
                    <Trigger Property="IsEnabled" Value="False">
                        <Setter Property="Foreground" Value="#BDBDBD"/>
                        <Setter Property="Background" Value="#F5F5F5"/>
                    </Trigger>
                </Style.Triggers>
            </Style>
        </Setter.Value>
    </Setter>
    
    <!-- 日历主体样式 -->
    <Setter Property="CalendarItemStyle">
        <Setter.Value>
            <Style TargetType="CalendarItem">
                <Setter Property="Background" Value="Transparent"/>
                <Setter Property="BorderThickness" Value="0"/>
                <Setter Property="FontWeight" Value="Normal"/>
            </Style>
        </Setter.Value>
    </Setter>
</Style>
```

**使用方式**：

xaml:

```xaml
<Calendar Style="{StaticResource IndustrialCalendarStyle}"
          Width="300"
          Height="260"/>
```

### 2.2 多状态日期标记（核心工业需求）

**应用场景**：用不同颜色标记「正常生产 / 计划维护 / 设备停机 / 节假日」等状态

xaml:

```xaml
<!-- 日期状态转换器 -->
<local:DateStatusConverter x:Key="DateStatusConverter"/>

<Style TargetType="Calendar" x:Key="StatusMarkCalendarStyle">
    <Setter Property="CalendarDayButtonStyle">
        <Setter.Value>
            <Style TargetType="CalendarDayButton">
                <Setter Property="Background" Value="Transparent"/>
                <Setter Property="Padding" Value="6"/>
                <Setter Property="Template">
                    <Setter.Value>
                        <ControlTemplate TargetType="CalendarDayButton">
                            <Border Background="{TemplateBinding Background}"
                                    CornerRadius="3"
                                    Padding="{TemplateBinding Padding}">
                                <ContentPresenter HorizontalAlignment="Center"
                                                  VerticalAlignment="Center"/>
                            </Border>
                        </ControlTemplate>
                    </Setter.Value>
                </Setter>
                
                <Style.Triggers>
                    <!-- 正常生产：绿色背景 -->
                    <DataTrigger Value="Production"
                                 Binding="{Binding Date, RelativeSource={RelativeSource Self}, Converter={StaticResource DateStatusConverter}}">
                        <Setter Property="Background" Value="#E8F5E9"/>
                        <Setter Property="ToolTip" Value="正常生产"/>
                    </DataTrigger>
                    
                    <!-- 计划维护：黄色背景 -->
                    <DataTrigger Value="Maintenance"
                                 Binding="{Binding Date, RelativeSource={RelativeSource Self}, Converter={StaticResource DateStatusConverter}}">
                        <Setter Property="Background" Value="#FFF9C4"/>
                        <Setter Property="ToolTip" Value="计划维护"/>
                    </DataTrigger>
                    
                    <!-- 设备停机：红色背景 -->
                    <DataTrigger Value="Shutdown"
                                 Binding="{Binding Date, RelativeSource={RelativeSource Self}, Converter={StaticResource DateStatusConverter}}">
                        <Setter Property="Background" Value="#FFEBEE"/>
                        <Setter Property="ToolTip" Value="设备停机"/>
                    </DataTrigger>
                    
                    <Trigger Property="IsSelected" Value="True">
                        <Setter Property="Background" Value="#2196F3"/>
                        <Setter Property="Foreground" Value="White"/>
                    </Trigger>
                    
                    <Trigger Property="IsToday" Value="True">
                        <Setter Property="BorderBrush" Value="#2196F3"/>
                        <Setter Property="BorderThickness" Value="1"/>
                    </Trigger>
                </Style.Triggers>
            </Style>
        </Setter.Value>
    </Setter>
</Style>
```

**转换器实现**：

csharp:

```c#
public class DateStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DateTime date) return "Normal";
        
        // 从业务服务获取日期状态（实际项目中从缓存/数据库读取）
        return MaintenanceService.GetDateStatus(date).ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// 日期状态枚举
public enum DateStatus
{
    Normal,     // 正常
    Production, // 生产
    Maintenance,// 维护
    Shutdown    // 停机
}
```

### 2.3 触摸屏大按钮日历

**应用场景**：工业平板、触控操作站，增大点击区域避免误触

xaml:

```xaml
<Style TargetType="Calendar" x:Key="TouchCalendarStyle">
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="Padding" Value="5"/>
    
    <Setter Property="CalendarDayButtonStyle">
        <Setter.Value>
            <Style TargetType="CalendarDayButton">
                <Setter Property="MinWidth" Value="48"/>
                <Setter Property="MinHeight" Value="48"/>
                <Setter Property="Padding" Value="10"/>
                <Setter Property="FontSize" Value="14"/>
                <Setter Property="Background" Value="Transparent"/>
                
                <Style.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter Property="Background" Value="#E3F2FD"/>
                    </Trigger>
                    <Trigger Property="IsSelected" Value="True">
                        <Setter Property="Background" Value="#2196F3"/>
                        <Setter Property="Foreground" Value="White"/>
                    </Trigger>
                    <Trigger Property="IsToday" Value="True">
                        <Setter Property="BorderBrush" Value="#2196F3"/>
                        <Setter Property="BorderThickness" Value="2"/>
                    </Trigger>
                </Style.Triggers>
            </Style>
        </Setter.Value>
    </Setter>
</Style>
```

------

## 三、工业场景完整实例

### 3.1 设备维护计划日历

**应用场景**：显示月度维护计划，点击日期查看维护详情，支持新增 / 删除维护任务

xaml:

```xaml
<Grid Margin="20">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>

    <!-- 工具栏 -->
    <DockPanel Margin="0,0,0,10">
        <Button Content="新增维护计划"
                Command="{Binding AddMaintenanceCommand}"
                Style="{StaticResource PrimaryButtonStyle}"
                Width="120"
                DockPanel.Dock="Left"/>
        <TextBlock Text="设备维护计划"
                   FontSize="16"
                   FontWeight="Bold"
                   HorizontalAlignment="Center"/>
    </DockPanel>

    <!-- 日历 + 详情 -->
    <Grid Grid.Row="1">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>

        <Calendar x:Name="maintenanceCalendar"
                  Grid.Column="0"
                  Style="{StaticResource StatusMarkCalendarStyle}"
                  FirstDayOfWeek="Monday"
                  DisplayDateChanged="MaintenanceCalendar_DisplayDateChanged"
                  SelectedDatesChanged="MaintenanceCalendar_SelectedDatesChanged"
                  Margin="0,0,20,0"
                  Width="320"
                  Height="280"/>

        <!-- 维护详情面板 -->
        <Border Grid.Column="1"
                Background="White"
                BorderBrush="#DDDDDD"
                BorderThickness="1"
                CornerRadius="3">
            <ScrollViewer VerticalScrollBarVisibility="Auto">
                <StackPanel Margin="15">
                    <TextBlock x:Name="detailTitle"
                               Text="请选择日期查看维护计划"
                               FontSize="14"
                               FontWeight="Bold"
                               Margin="0,0,0,10"/>
                    
                    <ItemsControl x:Name="maintenanceList">
                        <ItemsControl.ItemTemplate>
                            <DataTemplate>
                                <Border Background="#F5F5F5"
                                        CornerRadius="3"
                                        Padding="10"
                                        Margin="0,0,0,8">
                                    <Grid>
                                        <Grid.ColumnDefinitions>
                                            <ColumnDefinition Width="*"/>
                                            <ColumnDefinition Width="Auto"/>
                                        </Grid.ColumnDefinitions>
                                        <StackPanel>
                                            <TextBlock Text="{Binding Title}" FontWeight="Bold"/>
                                            <TextBlock Text="{Binding Time, StringFormat={}{0:HH:mm}}" FontSize="12" Foreground="#666666"/>
                                            <TextBlock Text="{Binding Content}" FontSize="12" Margin="0,3,0,0"/>
                                        </StackPanel>
                                        <Button Grid.Column="1"
                                                Content="完成"
                                                Style="{StaticResource SmallButtonStyle}"
                                                Command="{Binding CompleteCommand}"/>
                                    </Grid>
                                </Border>
                            </DataTemplate>
                        </ItemsControl.ItemTemplate>
                    </ItemsControl>
                </StackPanel>
            </ScrollViewer>
        </Border>
    </Grid>
</Grid>
```

csharp:

```c#
// 切换月份时加载当月维护计划
private void MaintenanceCalendar_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
{
    if (e.AddedDate.HasValue)
    {
        // 预加载当月维护计划，更新日期状态缓存
        MaintenanceService.LoadMonthlyPlan(e.AddedDate.Value);
    }
}

// 选中日期时显示维护详情
private void MaintenanceCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
{
    if (maintenanceCalendar.SelectedDate.HasValue)
    {
        DateTime date = maintenanceCalendar.SelectedDate.Value;
        detailTitle.Text = $"{date:yyyy年MM月dd日} 维护计划";
        maintenanceList.ItemsSource = MaintenanceService.GetDailyMaintenances(date);
    }
}
```

### 3.2 生产班次排班日历

**应用场景**：显示每日早 / 中 / 晚班排班情况，点击日期可调整班次人员

xaml:

```xaml
<Style TargetType="Calendar" x:Key="ShiftCalendarStyle">
    <Setter Property="CalendarDayButtonStyle">
        <Setter.Value>
            <Style TargetType="CalendarDayButton">
                <Setter Property="Padding" Value="4"/>
                <Setter Property="Template">
                    <Setter.Value>
                        <ControlTemplate TargetType="CalendarDayButton">
                            <Border Background="{TemplateBinding Background}"
                                    CornerRadius="2"
                                    Padding="{TemplateBinding Padding}">
                                <StackPanel>
                                    <ContentPresenter HorizontalAlignment="Center"
                                                      FontSize="12"/>
                                    <!-- 班次色块 -->
                                    <StackPanel Orientation="Horizontal"
                                                HorizontalAlignment="Center"
                                                Margin="0,2,0,0">
                                        <Rectangle Width="6" Height="6" Fill="#4CAF50" Margin="1,0"
                                                   Visibility="{Binding Date, Converter={StaticResource HasMorningShiftConverter}}"/>
                                        <Rectangle Width="6" Height="6" Fill="#FFC107" Margin="1,0"
                                                   Visibility="{Binding Date, Converter={StaticResource HasAfternoonShiftConverter}}"/>
                                        <Rectangle Width="6" Height="6" Fill="#9C27B0" Margin="1,0"
                                                   Visibility="{Binding Date, Converter={StaticResource HasNightShiftConverter}}"/>
                                    </StackPanel>
                                </StackPanel>
                            </Border>
                        </ControlTemplate>
                    </Setter.Value>
                </Setter>
                
                <Style.Triggers>
                    <Trigger Property="IsSelected" Value="True">
                        <Setter Property="Background" Value="#E3F2FD"/>
                        <Setter Property="BorderBrush" Value="#2196F3"/>
                        <Setter Property="BorderThickness" Value="1"/>
                    </Trigger>
                    <Trigger Property="IsToday" Value="True">
                        <Setter Property="FontWeight" Value="Bold"/>
                    </Trigger>
                </Style.Triggers>
            </Style>
        </Setter.Value>
    </Setter>
</Style>
```

------

## 四、MVVM 模式绑定实例

xaml:

```xaml
<Calendar SelectionMode="SingleDate"
          FirstDayOfWeek="Monday"
          SelectedDate="{Binding SelectedDate, Mode=TwoWay}"
          DisplayDate="{Binding CurrentDisplayDate, Mode=TwoWay}"
          Style="{StaticResource IndustrialCalendarStyle}"/>
```

csharp:

```c#
public class ProductionViewModel : INotifyPropertyChanged
{
    private DateTime? _selectedDate = DateTime.Today;
    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (_selectedDate != value)
            {
                _selectedDate = value;
                OnPropertyChanged();
                // 选中日期变化时自动加载数据
                LoadDailyReportAsync(value ?? DateTime.Today);
            }
        }
    }

    private DateTime _currentDisplayDate = DateTime.Today;
    public DateTime CurrentDisplayDate
    {
        get => _currentDisplayDate;
        set
        {
            if (_currentDisplayDate != value)
            {
                _currentDisplayDate = value;
                OnPropertyChanged();
                // 切换月份时预加载当月计划
                LoadMonthlyPlanAsync(value);
            }
        }
    }

    // 业务方法
    private async void LoadDailyReportAsync(DateTime date)
    {
        DailyReport = await ProductionService.GetDailyReportAsync(date);
    }

    private async void LoadMonthlyPlanAsync(DateTime month)
    {
        MonthlyPlans = await MaintenanceService.GetMonthlyPlanAsync(month);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

------

## 五、工业开发最佳实践

1. **优先用 `CalendarDayButtonStyle` 定制外观**：无需重写整个控件模板，通过数据触发器即可实现绝大多数状态标记需求。
2. **月份切换时预加载数据**：利用 `DisplayDateChanged` 事件只加载当前月份数据，避免一次性加载全年数据造成性能问题。
3. **合理使用 `BlackoutDates`**：禁用所有非工作日，减少操作人员误选，也减少业务层的无效校验。
4. **保留默认键盘导航**：工业终端多为键盘操作，不要随意重写 `OnKeyDown` 破坏方向键、翻页键的默认逻辑。
5. **状态标记用转换器**：日期状态通过 `IValueConverter` 绑定，不要在代码中逐个修改按钮样式，性能更优且符合 MVVM 思想。
6. **限制日期范围**：通过 `DisplayDateStart`/`DisplayDateEnd` 缩小可选择区间，既提升性能也避免业务逻辑出错。