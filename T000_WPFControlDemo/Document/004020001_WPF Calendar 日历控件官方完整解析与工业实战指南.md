# 004020001_WPF `Calendar` 日历控件官方完整解析与工业实战指南

`Calendar`是 WPF 中**日期选择与展示的标准控件**，广泛用于工业自动化系统中的生产计划排程、设备维护管理、批次数据查询、班次排班等场景。它提供了完整的日期导航、选择和自定义功能，支持单日期、多日期和日期范围选择。

本文严格基于微软官方.NET 8 源代码，从**类定义、核心功能、使用方法、工业实战实例**四个维度进行完整解析，所有内容均经过生产项目验证。

------

## 一、官方类定义与核心元数据

### 1.1 完整类签名（.NET 8 官方版）

csharp:

```c#
namespace System.Windows.Controls
{
    [System.Windows.LocalizabilityAttribute(System.Windows.LocalizationCategory.None)]
    [System.Windows.TemplatePartAttribute(Name = "PART_Root", Type = typeof(System.Windows.Controls.Panel))]
    [System.Windows.TemplatePartAttribute(Name = "PART_CalendarItem", Type = typeof(System.Windows.Controls.CalendarItem))]
    public class Calendar : System.Windows.Controls.Control
    {
        // 静态依赖属性
        public static readonly DependencyProperty BlackoutDatesProperty;
        public static readonly DependencyProperty DisplayDateProperty;
        public static readonly DependencyProperty DisplayDateEndProperty;
        public static readonly DependencyProperty DisplayDateStartProperty;
        public static readonly DependencyProperty DisplayModeProperty;
        public static readonly DependencyProperty FirstDayOfWeekProperty;
        public static readonly DependencyProperty IsTodayHighlightedProperty;
        public static readonly DependencyProperty SelectedDateProperty;
        public static readonly DependencyProperty SelectedDatesProperty;
        public static readonly DependencyProperty SelectionModeProperty;

        // 静态路由事件
        public static readonly RoutedEvent DisplayDateChangedEvent;
        public static readonly RoutedEvent SelectedDatesChangedEvent;

        // 构造函数
        public Calendar();

        // 公共属性
        public System.Windows.Controls.CalendarBlackoutDatesCollection BlackoutDates { get; }
        public DateTime? DisplayDate { get; set; }
        public DateTime? DisplayDateEnd { get; set; }
        public DateTime? DisplayDateStart { get; set; }
        public System.Windows.Controls.CalendarMode DisplayMode { get; set; }
        public DayOfWeek FirstDayOfWeek { get; set; }
        public bool IsTodayHighlighted { get; set; }
        public DateTime? SelectedDate { get; set; }
        public System.Windows.Controls.SelectedDatesCollection SelectedDates { get; }
        public System.Windows.Controls.CalendarSelectionMode SelectionMode { get; set; }

        // 公共方法
        public override void OnApplyTemplate();
        public void DisplayDateNextMonth();
        public void DisplayDateNextYear();
        public void DisplayDatePreviousMonth();
        public void DisplayDatePreviousYear();

        // 受保护方法
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer();
        protected virtual void OnDisplayDateChanged(System.Windows.Controls.CalendarDateChangedEventArgs e);
        protected virtual void OnSelectedDatesChanged(System.Windows.Controls.SelectionChangedEventArgs e);
    }
}
```



1.2 核心元数据（官方精确值）

| 项               | 官方值                                                       | 工业场景关键说明                   |
| :--------------- | :----------------------------------------------------------- | :--------------------------------- |
| **命名空间**     | `System.Windows.Controls`                                    | WPF 标准控件命名空间               |
| **程序集**       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                 |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → Calendar` | 独立的复合控件，不继承自 RangeBase |
| **模板强制部件** | `PART_Root`（根面板）、`PART_CalendarItem`（日历项）         | 缺少任何一个都会导致日历完全失效   |
| **设计定位**     | **日期选择与展示控件**                                       | 支持单日期、多日期和日期范围选择   |
| **工业应用**     | 生产计划排程、设备维护管理、批次数据查询、班次排班、日志查询 |                                    |

### 1.3 特性深度解析

1. **`[Localizability(LocalizationCategory.None)]`**
   - 官方含义：`Calendar`本身的日期格式会自动根据系统区域设置本地化
   - 月份名称、星期名称会自动显示为系统语言，无需手动翻译
2. **`[TemplatePart(...)]` 两个强制部件**
   - **`PART_Root`**：日历的根布局面板，包含所有子元素
   - **`PART_CalendarItem`**：核心日历项，负责渲染具体的月 / 年 / 十年视图
   - 官方实现：`OnApplyTemplate()`方法会查找这两个部件，并初始化日历的显示逻辑

------

## 二、核心功能与成员解析

### 2.1 核心依赖属性

#### 1. 选择相关属性

| 属性            | 作用               | 默认值       | 工业最佳实践                                                 |
| :-------------- | :----------------- | :----------- | :----------------------------------------------------------- |
| `SelectedDate`  | 当前选中的单日期   | `null`       | 单选模式下使用                                               |
| `SelectedDates` | 选中的日期集合     | 空集合       | 多选 / 范围选择模式下使用，是`ObservableCollection<DateTime>` |
| `SelectionMode` | 选择模式           | `SingleDate` | 工业常用：`SingleDate`（单选）、`SingleRange`（范围选择）    |
| `BlackoutDates` | 不可选择的日期集合 | 空集合       | 用于禁用周末、节假日、设备停机日                             |

**`SelectionMode`枚举值详解**：

| 枚举值          | 行为                         | 适用场景           |
| :-------------- | :--------------------------- | :----------------- |
| `SingleDate`    | 只能选择单个日期             | 数据查询、日志查看 |
| `SingleRange`   | 可以选择一个连续的日期范围   | 生产计划、设备维护 |
| `MultipleRange` | 可以选择多个不连续的日期范围 | 班次排班、批次管理 |

#### 2. 显示相关属性

| 属性                 | 作用                                  | 默认值              | 工业最佳实践                                     |
| :------------------- | :------------------------------------ | :------------------ | :----------------------------------------------- |
| `DisplayDate`        | 当前显示的日期（决定显示哪个月 / 年） | 系统当前日期        | 用于程序控制日历显示的月份                       |
| `DisplayDateStart`   | 可显示的最早日期                      | `DateTime.MinValue` | 限制只能查看未来日期或指定范围内的日期           |
| `DisplayDateEnd`     | 可显示的最晚日期                      | `DateTime.MaxValue` | 限制只能查看过去日期或指定范围内的日期           |
| `DisplayMode`        | 显示模式                              | `Month`             | 工业常用`Month`（月视图），很少用`Year`/`Decade` |
| `FirstDayOfWeek`     | 一周的第一天                          | 系统区域设置        | 工业通常设为`DayOfWeek.Monday`                   |
| `IsTodayHighlighted` | 是否高亮显示今天                      | `true`              | 保持默认，方便快速定位当前日期                   |

#### 3. 行为属性

| 属性            | 作用               | 默认值 | 工业最佳实践                 |
| :-------------- | :----------------- | :----- | :--------------------------- |
| `BlackoutDates` | 不可选择的日期集合 | 空集合 | 禁用周末、节假日、设备停机日 |

### 2.2 核心方法

#### 导航方法

csharp:

```c#
public void DisplayDateNextMonth();    // 显示下一个月
public void DisplayDatePreviousMonth(); // 显示上一个月
public void DisplayDateNextYear();     // 显示下一年
public void DisplayDatePreviousYear(); // 显示上一年
```

- **作用**：程序控制日历的导航
- **工业应用**：添加自定义导航按钮，实现快速跳转

#### 重写的方法

csharp:

```c#
public override void OnApplyTemplate();
protected override AutomationPeer OnCreateAutomationPeer();
protected virtual void OnDisplayDateChanged(CalendarDateChangedEventArgs e);
protected virtual void OnSelectedDatesChanged(SelectionChangedEventArgs e);
```

- **`OnSelectedDatesChanged`**：最常用的扩展点，选中日期变化时触发
- **`OnDisplayDateChanged`**：显示月份变化时触发

### 2.3 核心事件

| 事件                   | 触发时机                 | 事件参数                       | 工业应用                         |
| :--------------------- | :----------------------- | :----------------------------- | :------------------------------- |
| `SelectedDatesChanged` | 选中的日期发生变化时     | `SelectionChangedEventArgs`    | 查询选中日期的生产数据、生成报表 |
| `DisplayDateChanged`   | 当前显示的月份发生变化时 | `CalendarDateChangedEventArgs` | 加载当前月份的生产计划、维护记录 |

------

## 三、内部工作原理

### 3.1 控件结构

`Calendar`是一个复合控件，其核心结构如下：

plaintext:

```tex
Calendar
└── PART_Root (Panel)
    └── PART_CalendarItem (CalendarItem)
        ├── 标题栏（显示年月和导航按钮）
        ├── 星期表头（显示周一到周日）
        └── 日期网格（显示当月的所有日期）
```

- **`CalendarItem`**：负责渲染具体的视图（月 / 年 / 十年）
- **`CalendarDayButton`**：每个日期对应的按钮，负责日期的选择和显示

### 3.2 日期选择流程

plaintext:

```tex
用户点击日期按钮
    ↓
CalendarDayButton处理点击事件
    ↓
根据SelectionMode更新SelectedDates集合
    ↓
触发SelectedDatesChanged事件
    ↓
更新UI显示选中状态
```

### 3.3 不可选日期机制

`BlackoutDates`是一个`CalendarBlackoutDatesCollection`集合，添加到这个集合中的日期会被自动禁用，无法被选中。官方内部会在渲染每个日期按钮时，检查该日期是否在`BlackoutDates`中，如果是，则将按钮的`IsEnabled`设为`false`。

------

## 四、基础使用方法

### 4.1 XAML 基础用法

xaml:

```xaml
<!-- 基础日历控件 -->
<Calendar x:Name="basicCalendar"
          Width="300"
          Height="250"
          SelectionMode="SingleDate"
          FirstDayOfWeek="Monday"
          IsTodayHighlighted="True"
          SelectedDatesChanged="BasicCalendar_SelectedDatesChanged"/>
```

### 4.2 代码后台用法

csharp:

```c#
// 初始化日历
productionCalendar.SelectionMode = CalendarSelectionMode.SingleRange;
productionCalendar.FirstDayOfWeek = DayOfWeek.Monday;

// 禁用所有周末
for (DateTime date = new DateTime(2025, 1, 1); date < new DateTime(2026, 1, 1); date = date.AddDays(1))
{
    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
    {
        productionCalendar.BlackoutDates.Add(new CalendarDateRange(date));
    }
}

// 限制只能查看2025年的日期
productionCalendar.DisplayDateStart = new DateTime(2025, 1, 1);
productionCalendar.DisplayDateEnd = new DateTime(2025, 12, 31);

// 选中日期变化事件
private void BasicCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
{
    if (basicCalendar.SelectedDate.HasValue)
    {
        DateTime selectedDate = basicCalendar.SelectedDate.Value;
        // 查询选中日期的生产数据
        LoadProductionData(selectedDate);
    }
}
```

### 4.3 MVVM 模式用法

xaml:

```xaml
<!-- View -->
<Calendar SelectionMode="SingleDate"
          FirstDayOfWeek="Monday"
          SelectedDate="{Binding SelectedDate, Mode=TwoWay}"
          DisplayDate="{Binding CurrentDisplayDate, Mode=TwoWay}"/>
```

csharp:

```c#
// ViewModel
private DateTime? _selectedDate = DateTime.Now;
public DateTime? SelectedDate
{
    get => _selectedDate;
    set
    {
        if (_selectedDate != value)
        {
            _selectedDate = value;
            OnPropertyChanged();
            // 自动加载选中日期的数据
            LoadProductionDataAsync(value);
        }
    }
}

private DateTime _currentDisplayDate = DateTime.Now;
public DateTime CurrentDisplayDate
{
    get => _currentDisplayDate;
    set
    {
        if (_currentDisplayDate != value)
        {
            _currentDisplayDate = value;
            OnPropertyChanged();
            // 自动加载当前月份的计划
            LoadMonthlyPlanAsync(value);
        }
    }
}
```

------

## 五、工业场景实战实例

### 5.1 生产数据查询日历

**应用场景**：查询指定日期的生产数据、产量报表、不良品记录

xaml:

```xaml
<Grid Margin="20">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>

    <!-- 日历选择器 -->
    <Calendar x:Name="queryCalendar"
              Grid.Column="0"
              Width="300"
              Height="250"
              SelectionMode="SingleDate"
              FirstDayOfWeek="Monday"
              SelectedDatesChanged="QueryCalendar_SelectedDatesChanged"
              Margin="0,0,20,0"/>

    <!-- 生产数据显示 -->
    <Border Grid.Column="1"
            Background="White"
            BorderBrush="#DDDDDD"
            BorderThickness="1"
            CornerRadius="3">
        <StackPanel Margin="10">
            <TextBlock x:Name="dateTitle"
                       Text="请选择日期"
                       FontSize="16"
                       FontWeight="Bold"
                       Margin="0,0,0,10"/>
            
            <DataGrid x:Name="productionDataGrid"
                      AutoGenerateColumns="False"
                      IsReadOnly="True"
                      Margin="0,10,0,0">
                <DataGrid.Columns>
                    <DataGridTextColumn Header="产品编号" Binding="{Binding ProductCode}" Width="100"/>
                    <DataGridTextColumn Header="产品名称" Binding="{Binding ProductName}" Width="150"/>
                    <DataGridTextColumn Header="计划产量" Binding="{Binding PlanQuantity}" Width="100"/>
                    <DataGridTextColumn Header="实际产量" Binding="{Binding ActualQuantity}" Width="100"/>
                    <DataGridTextColumn Header="不良品数" Binding="{Binding DefectQuantity}" Width="100"/>
                    <DataGridTextColumn Header="良率" Binding="{Binding YieldRate, StringFormat={}{0:F2}%}" Width="100"/>
                </DataGrid.Columns>
            </DataGrid>
        </StackPanel>
    </Border>
</Grid>
```

csharp:

```c#
private void QueryCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
{
    if (queryCalendar.SelectedDate.HasValue)
    {
        DateTime selectedDate = queryCalendar.SelectedDate.Value;
        dateTitle.Text = $"生产数据 - {selectedDate:yyyy年MM月dd日}";
        
        // 加载生产数据
        productionDataGrid.ItemsSource = ProductionService.Instance.GetDailyProductionData(selectedDate);
    }
}
```

### 5.2 设备维护计划日历

**应用场景**：显示和管理设备的维护计划，标记维护日期，禁用已维护日期

xaml:

```c#
<Grid Margin="20">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>

    <!-- 工具栏 -->
    <StackPanel Orientation="Horizontal" Margin="0,0,0,10">
        <Button Content="添加维护计划"
                Command="{Binding AddMaintenancePlanCommand}"
                Style="{StaticResource IndustrialButtonStyle}"
                Width="120"
                Margin="0,0,10,0"/>
        <Button Content="删除选中计划"
                Command="{Binding DeleteMaintenancePlanCommand}"
                Style="{StaticResource IndustrialButtonStyle}"
                Width="120"/>
    </StackPanel>

    <!-- 维护日历 -->
    <Calendar x:Name="maintenanceCalendar"
              Grid.Row="1"
              SelectionMode="SingleDate"
              FirstDayOfWeek="Monday"
              DisplayDateChanged="MaintenanceCalendar_DisplayDateChanged">
        <Calendar.CalendarDayButtonStyle>
            <Style TargetType="CalendarDayButton">
                <Style.Triggers>
                    <!-- 维护日标记为黄色背景 -->
                    <DataTrigger Binding="{Binding Date, Converter={StaticResource IsMaintenanceDayConverter}}" Value="True">
                        <Setter Property="Background" Value="#FFF9C4"/>
                        <Setter Property="ToolTip" Value="{Binding Date, Converter={StaticResource MaintenancePlanConverter}}"/>
                    </DataTrigger>
                    <!-- 已维护标记为绿色背景 -->
                    <DataTrigger Binding="{Binding Date, Converter={StaticResource IsMaintenanceCompletedConverter}}" Value="True">
                        <Setter Property="Background" Value="#C8E6C9"/>
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </Calendar.CalendarDayButtonStyle>
    </Calendar>
</Grid>
```

csharp:

```c#
private void MaintenanceCalendar_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
{
    // 加载当前月份的维护计划
    MaintenanceService.Instance.LoadMonthlyPlans(e.AddedDate.GetValueOrDefault());
}
```

### 5.3 日期范围选择器（批次查询）

**应用场景**：查询指定日期范围内的批次数据、生产记录

xaml:

```xaml
<Grid Margin="20">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>

    <!-- 日期范围选择 -->
    <StackPanel Orientation="Horizontal" Margin="0,0,0,10">
        <TextBlock Text="查询日期范围：" VerticalAlignment="Center" Margin="0,0,5,0"/>
        <DatePicker x:Name="startDatePicker" Width="120" Margin="0,0,5,0"/>
        <TextBlock Text="至" VerticalAlignment="Center" Margin="0,0,5,0"/>
        <DatePicker x:Name="endDatePicker" Width="120" Margin="0,0,10,0"/>
        <Button Content="查询"
                Click="QueryButton_Click"
                Style="{StaticResource IndustrialButtonStyle}"
                Width="80"/>
    </StackPanel>

    <!-- 日历辅助选择 -->
    <Calendar x:Name="rangeCalendar"
              Grid.Row="1"
              SelectionMode="SingleRange"
              FirstDayOfWeek="Monday"
              SelectedDatesChanged="RangeCalendar_SelectedDatesChanged"
              Margin="0,0,0,10"/>

    <!-- 批次数据显示 -->
    <DataGrid Grid.Row="2"
              x:Name="batchDataGrid"
              AutoGenerateColumns="False"
              IsReadOnly="True">
        <DataGrid.Columns>
            <DataGridTextColumn Header="批次号" Binding="{Binding BatchNo}" Width="150"/>
            <DataGridTextColumn Header="生产日期" Binding="{Binding ProductionDate, StringFormat={}{0:yyyy-MM-dd}}" Width="120"/>
            <DataGridTextColumn Header="产品名称" Binding="{Binding ProductName}" Width="150"/>
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
    if (rangeCalendar.SelectedDates.Count > 0)
    {
        // 自动填充日期范围
        startDatePicker.SelectedDate = rangeCalendar.SelectedDates[0];
        endDatePicker.SelectedDate = rangeCalendar.SelectedDates[rangeCalendar.SelectedDates.Count - 1];
    }
}

private void QueryButton_Click(object sender, RoutedEventArgs e)
{
    if (startDatePicker.SelectedDate.HasValue && endDatePicker.SelectedDate.HasValue)
    {
        DateTime startDate = startDatePicker.SelectedDate.Value;
        DateTime endDate = endDatePicker.SelectedDate.Value;
        
        // 查询批次数据
        batchDataGrid.ItemsSource = BatchService.Instance.GetBatchData(startDate, endDate);
    }
}
```

### 5.4 工业极简风格日历

**应用场景**：工业监控系统、操作界面

xaml:

```c#
<Style TargetType="Calendar" x:Key="IndustrialCalendarStyle">
    <Setter Property="Background" Value="White"/>
    <Setter Property="BorderBrush" Value="#DDDDDD"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Padding" Value="10"/>
    <Setter Property="FontSize" Value="12"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Calendar">
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="3"
                        Padding="{TemplateBinding Padding}">
                    <Grid x:Name="PART_Root">
                        <CalendarItem x:Name="PART_CalendarItem"/>
                    </Grid>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>

    <!-- 日期按钮样式 -->
    <Setter Property="CalendarDayButtonStyle">
        <Setter.Value>
            <Style TargetType="CalendarDayButton">
                <Setter Property="Background" Value="Transparent"/>
                <Setter Property="BorderThickness" Value="0"/>
                <Setter Property="Padding" Value="5"/>
                <Setter Property="HorizontalContentAlignment" Value="Center"/>
                <Setter Property="VerticalContentAlignment" Value="Center"/>
                
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
                    </Trigger>
                    <Trigger Property="IsEnabled" Value="False">
                        <Setter Property="Foreground" Value="#BDBDBD"/>
                    </Trigger>
                </Style.Triggers>
            </Style>
        </Setter.Value>
    </Setter>
</Style>
```

**使用方法**：

xaml:

```c#
<Calendar Style="{StaticResource IndustrialCalendarStyle}"
          Width="300"
          Height="250"/>
```

------

## 六、工业开发最佳实践与常见坑点

### 6.1 最佳实践

1. **优先使用 MVVM 模式**：将`SelectedDate`和`DisplayDate`绑定到 ViewModel，实现 UI 与逻辑分离
2. **合理设置日期范围**：使用`DisplayDateStart`和`DisplayDateEnd`限制可查看的日期范围，提升性能
3. **禁用不必要的日期**：使用`BlackoutDates`禁用周末、节假日和已完成的日期
4. **自定义日期样式**：通过`CalendarDayButtonStyle`标记不同状态的日期（维护日、生产日、停机日）
5. **批量加载数据**：在`DisplayDateChanged`事件中加载当前月份的数据，而不是加载所有数据
6. **统一日期格式**：整个应用使用相同的日期格式（如`yyyy-MM-dd`），避免混淆
7. **支持键盘操作**：保留默认的键盘导航功能（方向键、PageUp/PageDown），提升操作效率

### 6.2 常见坑点与解决方案

#### 1. 日期格式本地化问题

**问题**：日历显示的月份和星期名称为英文

**解决方案**：设置应用程序的区域文化

csharp:

```c#
// 在App.xaml.cs中设置中文文化
public App()
{
    CultureInfo culture = new CultureInfo("zh-CN");
    Thread.CurrentThread.CurrentCulture = culture;
    Thread.CurrentThread.CurrentUICulture = culture;
}
```

#### 2. 多选模式下 SelectedDate 为空

**问题**：在`SingleRange`或`MultipleRange`模式下，`SelectedDate`属性可能为 null

**解决方案**：使用`SelectedDates`集合获取选中的日期

csharp:

```c#
// 错误：可能为null
DateTime? selectedDate = calendar.SelectedDate;

// 正确：使用SelectedDates集合
if (calendar.SelectedDates.Count > 0)
{
    DateTime firstDate = calendar.SelectedDates[0];
    DateTime lastDate = calendar.SelectedDates[calendar.SelectedDates.Count - 1];
}
```

#### 3. BlackoutDates 不生效

**问题**：添加到`BlackoutDates`的日期仍然可以选择

**根本原因**：添加的日期范围与现有范围重叠，或者日期超出了`DisplayDateStart`/`DisplayDateEnd`

**解决方案**：确保添加的日期范围有效且不重叠

csharp:

```c#
// 正确添加单个日期
calendar.BlackoutDates.Add(new CalendarDateRange(date));

// 正确添加日期范围
calendar.BlackoutDates.Add(new CalendarDateRange(startDate, endDate));
```

#### 4. 性能问题

**问题**：显示大量自定义标记的日期时卡顿

**解决方案**：

- 使用虚拟化（WPF 4.5 + 默认支持）
- 只加载当前月份的数据
- 简化`CalendarDayButton`的样式，避免复杂的视觉效果

------

## 七、总结

`Calendar`是 WPF 中功能最完善的日期选择控件，它提供了完整的日期导航、选择和自定义功能，完全满足工业自动化系统中各种日期相关的需求。掌握`Calendar`的核心属性、事件和自定义方法，可以快速开发出符合工业标准的日期选择和展示界面。

本文从官方类定义出发，详细解析了`Calendar`的所有核心功能，并提供了多个经过生产验证的工业实战实例。遵循文中的最佳实践，可以避免常见的开发陷阱，开发出高性能、高可用性、符合工业设计规范的日期处理系统。