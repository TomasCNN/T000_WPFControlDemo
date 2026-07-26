# 004021004_WPF `DatePicker` 官方类深度解析（工业场景适配版）

下面提供 7 个工业场景高频使用的 `DatePicker` 完整实例，覆盖 MVVM 绑定、日期范围限制、禁用日期、输入验证、联动逻辑、样式自定义、业务联动等核心用法，可直接复制到项目中复用。

------

## 实例 1：基础 MVVM 双向绑定（报表日期筛选）

**适用场景**：生产报表、产能查询页面，通过 ViewModel 控制选中日期。

### ViewModel 代码

csharp:

```c#
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ReportViewModel : INotifyPropertyChanged
{
    private DateTime? _reportDate = DateTime.Now;

    /// <summary>
    /// 报表查询日期
    /// </summary>
    public DateTime? ReportDate
    {
        get => _reportDate;
        set
        {
            _reportDate = value;
            OnPropertyChanged();
            // 日期变更后自动刷新数据
            LoadCapacityData();
        }
    }

    private void LoadCapacityData()
    {
        // 你的产能数据加载逻辑
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
```

### XAML 绑定

xaml:

```xaml
<Window.DataContext>
    <local:ReportViewModel />
</Window.DataContext>

<DatePicker 
    Width="200"
    SelectedDateFormat="Short"
    FirstDayOfWeek="Monday"
    SelectedDate="{Binding ReportDate, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>
```

**关键点**：`UpdateSourceTrigger=PropertyChanged` 实现选完日期立即同步到 ViewModel。

------

## 实例 2：限制可选日期范围（仅允许当月上报）

**适用场景**：产能上报仅允许选择当月日期，禁止选择未来日期和历史月份。

### 方式 1：XAML 静态设置

xaml:

```xaml
<DatePicker
    DisplayDateStart="2026-06-01"
    DisplayDateEnd="{x:Static sys:DateTime.Now}"
    SelectedDate="{Binding ReportDate}"/>
```

> 需引入命名空间：`xmlns:sys="clr-namespace:System;assembly=mscorlib"`

### 方式 2：后台代码动态设置（按月自动计算）

csharp:

```c#
public MainWindow()
{
    InitializeComponent();
    
    // 当月第一天到今天
    DateTime monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
    datePicker.DisplayDateStart = monthStart;
    datePicker.DisplayDateEnd = DateTime.Now;
}
```

------

## 实例 3：动态禁用指定日期（设备停机日不可选）

**适用场景**：从 MES 接口获取设备停机日、节假日，加入禁用列表，用户无法选中。

csharp:

```c#
private void DatePicker_CalendarOpened(object sender, RoutedEventArgs e)
{
    DatePicker dp = sender as DatePicker;
    
    // 每次打开先清空，防止重复添加报错
    dp.BlackoutDates.Clear();

    // 1. 禁用所有过去日期
    dp.BlackoutDates.AddDatesInPast();

    // 2. 模拟从MES获取的停机日列表
    List<DateTime> stopDates = new List<DateTime>
    {
        new DateTime(2026, 6, 10),
        new DateTime(2026, 6, 15),
        new DateTime(2026, 6, 20)
    };

    // 3. 批量添加禁用日期
    foreach (DateTime dt in stopDates)
    {
        dp.BlackoutDates.Add(new CalendarDateRange(dt));
    }

    // 4. 也可禁用一个区间（比如设备大修3天）
    dp.BlackoutDates.Add(new CalendarDateRange(
        new DateTime(2026, 6, 25), 
        new DateTime(2026, 6, 27)));
}
```

XAML 关联事件：

xaml:

```xaml
<DatePicker CalendarOpened="DatePicker_CalendarOpened"/>
```

------

## 实例 4：日期输入验证与错误拦截

**适用场景**：防止用户手动输入非法日期（如 `2026/13/40`、乱码）导致程序崩溃，统一处理错误。

csharp:

```c#
private void DatePicker_DateValidationError(object sender, DatePickerDateValidationErrorEventArgs e)
{
    // 关键：禁止抛出异常，避免程序崩溃
    e.ThrowException = false;

    DatePicker dp = sender as DatePicker;
    
    // 记录日志
    UpdateProcess($"日期输入非法：{e.Text}，错误原因：{e.Exception.Message}");

    // 自动修正为今日
    dp.SelectedDate = DateTime.Now;
}
```

XAML 关联：

xaml:

```xaml
<DatePicker DateValidationError="DatePicker_DateValidationError"/>
```

------

## 实例 5：开始 / 结束日期联动（时间范围选择）

**适用场景**：产能报表区间查询，结束日期不能早于开始日期，开始日期不能晚于结束日期。

### XAML

xaml:

```xaml
<StackPanel Orientation="Horizontal" Spacing="10">
    <DatePicker x:Name="dpStart" SelectedDateChanged="DpStart_SelectedDateChanged"/>
    <TextBlock Text="至" VerticalAlignment="Center"/>
    <DatePicker x:Name="dpEnd" SelectedDateChanged="DpEnd_SelectedDateChanged"/>
</StackPanel>
```

### 后台联动逻辑

csharp:

```c#
private void DpStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
{
    // 结束日期的最小值 = 开始日期
    if (dpStart.SelectedDate.HasValue)
    {
        dpEnd.DisplayDateStart = dpStart.SelectedDate.Value;
        
        // 如果结束日期早于开始日期，自动修正
        if (dpEnd.SelectedDate < dpStart.SelectedDate)
        {
            dpEnd.SelectedDate = dpStart.SelectedDate.Value;
        }
    }
}

private void DpEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
{
    // 开始日期的最大值 = 结束日期
    if (dpEnd.SelectedDate.HasValue)
    {
        dpStart.DisplayDateEnd = dpEnd.SelectedDate.Value;
    }
}
```

------

## 实例 6：自定义日历样式（工业深色主题）

**适用场景**：工业软件深色界面，自定义日历背景、选中色、今日高亮，匹配系统主题。

xaml:

```xaml
<DatePicker
    FirstDayOfWeek="Monday"
    SelectedDateFormat="Short">
    <DatePicker.CalendarStyle>
        <Style TargetType="Calendar">
            <!-- 日历整体背景 -->
            <Setter Property="Background" Value="#1E1E1E"/>
            <Setter Property="Foreground" Value="White"/>
            <Setter Property="BorderBrush" Value="#333"/>
            <Setter Property="BorderThickness" Value="1"/>
            
            <!-- 选中日期背景色 -->
            <Setter Property="CalendarDayButtonStyle">
                <Setter.Value>
                    <Style TargetType="CalendarDayButton">
                        <Style.Triggers>
                            <Trigger Property="IsSelected" Value="True">
                                <Setter Property="Background" Value="#007ACC"/>
                                <Setter Property="Foreground" Value="White"/>
                            </Trigger>
                            <Trigger Property="IsToday" Value="True">
                                <Setter Property="BorderBrush" Value="#FF6B00"/>
                                <Setter Property="BorderThickness" Value="2"/>
                            </Trigger>
                            <Trigger Property="IsBlackedOut" Value="True">
                                <Setter Property="Foreground" Value="#555"/>
                            </Trigger>
                        </Style.Triggers>
                    </Style>
                </Setter.Value>
            </Setter>
        </Style>
    </DatePicker.CalendarStyle>
</DatePicker>
```

------

## 实例 7：业务联动：选日期自动加载产能数据

**适用场景**：贴合你之前的产能上报逻辑，选择日期后自动读取 PLC 数据并生成上报报文。

csharp:

```c#
private void Dp_ReportDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
{
    DatePicker dp = sender as DatePicker;
    
    if (!dp.SelectedDate.HasValue)
    {
        UpdateProcess("请选择上报日期");
        return;
    }

    DateTime reportDate = dp.SelectedDate.Value;
    UpdateProcess($"已选择上报日期：{reportDate:yyyy-MM-dd}，开始读取PLC时段产能");

    // 调用你的产能上报方法，传入指定日期
    Task.Run(() =>
    {
        try
        {
            SendEQP_CAPACITY_BYHOUR_REPORT(reportDate);
        }
        catch (Exception ex)
        {
            UpdateProcess($"上报异常：{ex.Message}");
        }
    });
}
```

> 建议将原 `SendEQP_CAPACITY_BYHOUR_REPORT` 方法增加 `DateTime reportDate` 参数，支持按指定日期读取对应 PLC 寄存器数据。

------

## 补充：常用小技巧

### 1. 清空选中日期

csharp:

```c#
datePicker.SelectedDate = null;
```

### 2. 代码强制展开下拉面板

csharp:

```c#
datePicker.IsDropDownOpen = true;
```

### 3. 获取当天完整时间区间（查询必备）

csharp:

```c#
if (datePicker.SelectedDate is DateTime date)
{
    DateTime dayStart = date.Date;                  // 当天 00:00:00
    DateTime dayEnd = date.Date.AddDays(1).AddTicks(-1); // 当天 23:59:59.999
}
```