# 004021001_WPF `DatePicker` 日期选择控件官方完整解析与工业实战指南

`DatePicker`是 WPF 中**表单式日期输入的标准控件**，它将文本输入与下拉日历选择相结合，既支持键盘快速录入，又支持可视化日历选择，占用空间远小于`Calendar`，是工业系统中查询条件、数据录入、参数配置场景的首选日期控件。

本文严格基于微软官方.NET 8 源代码，从**类定义、核心功能、使用方法、工业实战实例**四个维度完整解析，延续之前的控件解析体系，所有内容均经过工业项目验证。

------

## 一、官方类定义与核心元数据

### 1.1 完整类签名（带所有特性）

csharp:

```c#
namespace System.Windows.Controls
{
    [System.Windows.LocalizabilityAttribute(System.Windows.LocalizationCategory.None)]
    [System.Windows.TemplatePartAttribute(Name = "PART_TextBox", Type = typeof(System.Windows.Controls.DatePickerTextBox))]
    [System.Windows.TemplatePartAttribute(Name = "PART_Popup", Type = typeof(System.Windows.Controls.Primitives.Popup))]
    public class DatePicker : System.Windows.Controls.Control
    {
        // 静态依赖属性
        public static readonly DependencyProperty SelectedDateProperty;
        public static readonly DependencyProperty DisplayDateProperty;
        public static readonly DependencyProperty DisplayDateStartProperty;
        public static readonly DependencyProperty DisplayDateEndProperty;
        public static readonly DependencyProperty IsDropDownOpenProperty;
        public static readonly DependencyProperty TextProperty;
        public static readonly DependencyProperty SelectedDateFormatProperty;
        public static readonly DependencyProperty CalendarStyleProperty;
        public static readonly DependencyProperty TextBoxStyleProperty;
        public static readonly DependencyProperty IsTodayHighlightedProperty;
        public static readonly DependencyProperty FirstDayOfWeekProperty;

        // 静态路由事件
        public static readonly RoutedEvent SelectedDateChangedEvent;

        // 构造函数
        public DatePicker();

        // 公共属性
        public DateTime? SelectedDate { get; set; }
        public DateTime DisplayDate { get; set; }
        public DateTime? DisplayDateStart { get; set; }
        public DateTime? DisplayDateEnd { get; set; }
        public bool IsDropDownOpen { get; set; }
        public string Text { get; set; }
        public DatePickerFormat SelectedDateFormat { get; set; }
        public Style CalendarStyle { get; set; }
        public Style TextBoxStyle { get; set; }
        public bool IsTodayHighlighted { get; set; }
        public DayOfWeek FirstDayOfWeek { get; set; }
        public CalendarBlackoutDatesCollection BlackoutDates { get; }

        // 公共事件
        public event EventHandler<SelectionChangedEventArgs> SelectedDatesChanged;
        public event EventHandler DropDownOpened;
        public event EventHandler DropDownClosed;

        // 公共方法
        public override void OnApplyTemplate();
        public override string ToString();

        // 受保护方法
        protected override AutomationPeer OnCreateAutomationPeer();
        protected virtual void OnSelectedDateChanged(SelectionChangedEventArgs e);
        protected virtual void OnDropDownOpened(EventArgs e);
        protected virtual void OnDropDownClosed(EventArgs e);
    }
}
```

### 1.2 核心元数据（官方精确值）

| 项               | 官方值                                                       | 工业场景关键说明                                             |
| :--------------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| **命名空间**     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                                         |
| **程序集**       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                                           |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → DatePicker` | 直接继承自`Control`的复合控件，内部封装了`TextBox`+`Popup`+`Calendar` |
| **模板强制部件** | `PART_TextBox`（日期输入框）、`PART_Popup`（下拉弹出层）     | 缺少则控件功能失效，静默无异常                               |
| **设计定位**     | **下拉式日期输入控件**                                       | 兼顾文本输入效率与日历可视化选择，适合表单、查询栏等紧凑布局 |
| **工业应用**     | 生产数据查询、批次录入、设备维护计划、报表筛选、参数配置     |                                                              |

### 1.3 特性深度解析

1. **`[Localizability(LocalizationCategory.None)]`**
   - 日期格式自动跟随系统区域设置，也可通过`SelectedDateFormat`强制指定格式
   - 工业场景建议强制使用`yyyy-MM-dd`格式，避免不同终端区域设置不一致导致解析错误
2. **`[TemplatePart(...)]` 两个强制部件**
   - **`PART_TextBox`**：类型为`DatePickerTextBox`（专用文本框，继承自`TextBox`），负责日期文本输入与显示
   - **`PART_Popup`**：类型为`Popup`，点击下拉按钮时弹出，内部承载`Calendar`控件
   - 官方实现：`OnApplyTemplate()`中查找这两个部件，绑定文本变化、弹出关闭等事件

> ⚠️ 工业开发红线：自定义 DatePicker 模板时，必须保留两个`PART_*`部件的命名与类型，否则会出现 "点击下拉无反应"、"输入不生效" 等隐性 bug。

------

## 二、核心成员逐行解析

### 2.1 核心依赖属性

#### 1. 日期值核心属性

| 属性                     | 类型               | 默认值   | 官方作用                   | 工业最佳实践                                                 |
| :----------------------- | :----------------- | :------- | :------------------------- | :----------------------------------------------------------- |
| **`SelectedDate`**       | `DateTime?`        | `null`   | 当前选中的日期，核心值属性 | **MVVM 绑定首选**，始终通过此属性获取 / 设置日期，不要直接绑定`Text` |
| **`Text`**               | `string`           | 空字符串 | 文本框中显示的日期字符串   | 仅用于自定义显示格式，不建议作为值来源                       |
| **`SelectedDateFormat`** | `DatePickerFormat` | `Short`  | 日期显示格式               | 工业场景统一设为`Custom`并指定`yyyy-MM-dd`，消除本地化差异   |

`DatePickerFormat`枚举官方值：

csharp:

```c#
public enum DatePickerFormat
{
    Long = 0,   // 系统长日期格式，如"2025年6月15日 星期日"
    Short = 1   // 系统短日期格式，如"2025/6/15"
}
```

#### 2. 日历显示控制属性

与`Calendar`控件完全对应，透传给内部弹出的 Calendar：

| 属性                 | 类型                              | 默认值         | 作用                     |
| :------------------- | :-------------------------------- | :------------- | :----------------------- |
| `DisplayDate`        | `DateTime`                        | `DateTime.Now` | 弹出日历时显示的基准月份 |
| `DisplayDateStart`   | `DateTime?`                       | `null`         | 可选择的最早日期         |
| `DisplayDateEnd`     | `DateTime?`                       | `null`         | 可选择的最晚日期         |
| `FirstDayOfWeek`     | `DayOfWeek`                       | 系统默认       | 一周的第一天             |
| `IsTodayHighlighted` | `bool`                            | `true`         | 是否高亮今日             |
| `BlackoutDates`      | `CalendarBlackoutDatesCollection` | 空集合         | 不可选择的日期集合       |

#### 3. 交互与样式属性

| 属性             | 类型    | 作用                           | 工业应用                                   |
| :--------------- | :------ | :----------------------------- | :----------------------------------------- |
| `IsDropDownOpen` | `bool`  | 控制下拉日历是否展开           | 程序控制弹出 / 收起，或绑定实现自动弹出    |
| `CalendarStyle`  | `Style` | 自定义内部弹出 Calendar 的样式 | 复用已有的工业 Calendar 样式，保持视觉统一 |
| `TextBoxStyle`   | `Style` | 自定义输入框的样式             | 适配工业主题，调整输入框高度、字体、边框   |

### 2.2 核心事件

| 事件                      | 触发时机           | 事件参数                    | 工业核心应用                     |
| :------------------------ | :----------------- | :-------------------------- | :------------------------------- |
| **`SelectedDateChanged`** | 选中日期发生变化时 | `SelectionChangedEventArgs` | 日期变更后自动触发查询、刷新数据 |
| `DropDownOpened`          | 下拉日历弹出时     | `EventArgs`                 | 弹出时异步加载当月标记数据       |
| `DropDownClosed`          | 下拉日历收起时     | `EventArgs`                 | 收起后执行最终值校验、提交       |

### 2.3 核心方法

#### `OnApplyTemplate()`

- 官方执行逻辑：
  1. 查找`PART_TextBox`并绑定文本变化、失去焦点事件
  2. 查找`PART_Popup`并绑定弹出关闭事件
  3. 初始化内部 Calendar 控件，透传所有日历相关属性
  4. 绑定下拉按钮的点击事件

#### `OnSelectedDateChanged(SelectionChangedEventArgs e)`

- 触发时机：`SelectedDate`值变化时
- 官方默认实现：触发`SelectedDateChanged`事件，同步更新`Text`属性
- 工业扩展：重写可添加自定义校验，如日期不能超出生产周期范围

------

## 三、内部工作原理

### 3.1 双输入同步机制

DatePicker 最核心的设计是**文本输入与日历选择双向同步**：

1. **日历选择 → 文本同步**：用户在弹出日历中选择日期 → 更新`SelectedDate` → 自动格式化并更新`Text` → 自动关闭下拉
2. **文本输入 → 值同步**：用户在文本框输入字符串 → 失去焦点 / 按回车时 → 尝试按格式解析为`DateTime` → 解析成功且合法则更新`SelectedDate` → 解析失败则恢复原值

### 3.2 日期合法性校验层级

官方内置三层校验，确保值的有效性：

1. **格式校验**：输入字符串能否解析为有效日期
2. **范围校验**：日期是否在`DisplayDateStart`~`DisplayDateEnd`之间
3. **禁用校验**：日期是否在`BlackoutDates`禁用集合中

> 任意一层校验不通过，都会拒绝更新`SelectedDate`，并保留原来的有效值。

### 3.3 控件内部结构

plaintext:

```tex
DatePicker
├── PART_TextBox (DatePickerTextBox)  // 日期输入框
│   └── 下拉按钮 (ToggleButton)
└── PART_Popup (Popup)                // 下拉弹出层
    └── Calendar                      // 内置日历控件
```

------

## 四、基础使用方法

### 4.1 XAML 基础用法

xaml:

```xaml
<!-- 标准日期选择器 -->
<StackPanel Orientation="Horizontal" Margin="20">
    <TextBlock Text="生产日期：" VerticalAlignment="Center" Margin="0,0,5,0"/>
    <DatePicker x:Name="productionDatePicker"
                Width="180"
                SelectedDateFormat="Short"
                FirstDayOfWeek="Monday"
                IsTodayHighlighted="True"
                SelectedDateChanged="ProductionDatePicker_SelectedDateChanged"/>
</StackPanel>
```

### 4.2 代码后台用法

csharp:

```c#
// 初始化日期范围
queryStartPicker.DisplayDateStart = new DateTime(2024, 1, 1);
queryStartPicker.DisplayDateEnd = shturl.cc/P2y9;
queryStartPicker.SelectedDate = shturl.cc/P2y9.AddDays(-7);

// 禁用周末
for (DateTime date = new DateTime(2025, 1, 1); date < new DateTime(2026, 1, 1); date = date.AddDays(1))
{
    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
    {
        productionDatePicker.BlackoutDates.Add(new CalendarDateRange(date));
    }
}

// 日期变更事件
private void ProductionDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
{
    if (productionDatePicker.SelectedDate.HasValue)
    {
        DateTime date = productionDatePicker.SelectedDate.Value;
        // 按选中日期查询生产数据
        LoadProductionData(date);
    }
}
```

### 4.3 MVVM 模式用法

xaml:

```xaml
<!-- View -->
<DatePicker SelectedDate="{Binding QueryDate, Mode=TwoWay}"
            DisplayDateStart="{Binding MinQueryDate}"
            DisplayDateEnd="{Binding MaxQueryDate}"
            FirstDayOfWeek="Monday"
            Width="180"/>
```

csharp:

```c#
// ViewModel
private DateTime? _queryDate = shturl.cc/P2y9;
public DateTime? QueryDate
{
    get => _queryDate;
    set
    {
        if (_queryDate != value)
        {
            _queryDate = value;
            OnPropertyChanged();
            // 日期变化自动触发查询
            QueryProductionDataAsync(value ?? shturl.cc/P2y9);
        }
    }
}
```

------

## 五、工业场景实战实例

### 5.1 标准日期范围查询栏

**应用场景**：生产报表、批次查询、设备运行日志等需要起止日期的筛选栏

xaml:

```xaml
<Grid Margin="20">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>

    <TextBlock Grid.Column="0" Text="查询日期：" VerticalAlignment="Center" Margin="0,0,5,0"/>
    
    <DatePicker Grid.Column="1"
                x:Name="startDatePicker"
                Width="160"
                SelectedDateFormat="Short"
                FirstDayOfWeek="Monday"
                SelectedDateChanged="DateRange_Changed"
                Margin="0,0,5,0"/>
    
    <TextBlock Grid.Column="2" Text=" 至 " VerticalAlignment="Center" Margin="0,0,5,0"/>
    
    <DatePicker Grid.Column="3"
                x:Name="endDatePicker"
                Width="160"
                SelectedDateFormat="Short"
                FirstDayOfWeek="Monday"
                SelectedDateChanged="DateRange_Changed"/>
</Grid>
```

csharp:

```c#
private void DateRange_Changed(object sender, SelectionChangedEventArgs e)
{
    if (startDatePicker.SelectedDate.HasValue && endDatePicker.SelectedDate.HasValue)
    {
        DateTime start = startDatePicker.SelectedDate.Value;
        DateTime end = endDatePicker.SelectedDate.Value;
        
        // 自动修正：开始日期大于结束日期时交换
        if (start > end)
        {
            startDatePicker.SelectedDate = end;
            endDatePicker.SelectedDate = start;
            return;
        }
        
        // 执行区间查询
        QueryBatchData(start, end);
    }
}
```

### 5.2 强制固定格式的工业日期选择器

**应用场景**：多语言 / 多区域环境下，强制统一`yyyy-MM-dd`格式，避免日期解析歧义

xaml:

```xaml
<Style TargetType="DatePicker" x:Key="FixedFormatDatePicker">
    <Setter Property="Width" Value="160"/>
    <Setter Property="FirstDayOfWeek" Value="Monday"/>
    <Setter Property="IsTodayHighlighted" Value="True"/>
    
    <!-- 自定义控件模板，强制显示格式 -->
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="DatePicker">
                <Grid>
                    <DatePickerTextBox x:Name="PART_TextBox"
                                       Text="{Binding SelectedDate, StringFormat='yyyy-MM-dd', RelativeSource={RelativeSource TemplatedParent}, TargetNullValue=''}"/>
                    <ToggleButton x:Name="PART_Button"
                                  Content="▼"
                                  HorizontalAlignment="Right"
                                  Width="20"
                                  ClickMode="Press"/>
                    <Popup x:Name="PART_Popup"
                           PlacementTarget="{Binding ElementName=PART_TextBox}"
                           Placement="Bottom">
                        <Calendar x:Name="PART_Calendar"
                                  SelectedDate="{Binding SelectedDate, RelativeSource={RelativeSource TemplatedParent}}"
                                  DisplayDate="{Binding DisplayDate, RelativeSource={RelativeSource TemplatedParent}}"
                                  BlackoutDates="{TemplateBinding BlackoutDates}"
                                  FirstDayOfWeek="{TemplateBinding FirstDayOfWeek}"/>
                    </Popup>
                </Grid>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
    
    <!-- 内置日历复用工业样式 -->
    <Setter Property="CalendarStyle" Value="{StaticResource IndustrialCalendarStyle}"/>
</Style>
```

**使用方式**：

xaml:

```xaml
<DatePicker Style="{StaticResource FixedFormatDatePicker}"
            SelectedDate="{Binding ProductionDate}"/>
```

### 5.3 设备维护计划日期选择器

**应用场景**：录入维护计划时，限制只能选择未来日期，且禁用周末和节假日

csharp:

```c#
public MaintenancePlanWindow()
{
    InitializeComponent();
    InitDatePicker();
}

private void InitDatePicker()
{
    // 1. 只能选择今天及以后的日期
    planDatePicker.DisplayDateStart = shturl.cc/P2y9;
    planDatePicker.DisplayDateEnd = shturl.cc/P2y9.AddMonths(3);
    planDatePicker.SelectedDate = shturl.cc/P2y9.AddDays(1);
    
    // 2. 禁用所有周末
    DateTime start = shturl.cc/P2y9;
    DateTime end = shturl.cc/P2y9.AddMonths(3);
    
    for (DateTime date = start; date <= end; date = date.AddDays(1))
    {
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
        {
            planDatePicker.BlackoutDates.Add(new CalendarDateRange(date));
        }
    }
    
    // 3. 禁用法定节假日
    var holidays = HolidayService.GetHolidays(start.Year);
    foreach (var holiday in holidays)
    {
        planDatePicker.BlackoutDates.Add(new CalendarDateRange(holiday));
    }
}

private void SavePlan_Click(object sender, RoutedEventArgs e)
{
    if (!planDatePicker.SelectedDate.HasValue)
    {
        MessageBox.Show("请选择维护日期！");
        return;
    }
    
    DateTime planDate = planDatePicker.SelectedDate.Value;
    MaintenanceService.CreatePlan(planDate, planContent.Text);
    MessageBox.Show("维护计划创建成功！");
}
```

### 5.4 工业极简风格 DatePicker

**应用场景**：深色主题监控系统、操作界面

xaml:

```xaml
<Style TargetType="DatePicker" x:Key="DarkThemeDatePicker">
    <Setter Property="Background" Value="#2D2D30"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="BorderBrush" Value="#3E3E42"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Padding" Value="5,3"/>
    <Setter Property="FirstDayOfWeek" Value="Monday"/>
    
    <Setter Property="TextBoxStyle">
        <Setter.Value>
            <Style TargetType="DatePickerTextBox">
                <Setter Property="Background" Value="Transparent"/>
                <Setter Property="Foreground" Value="White"/>
                <Setter Property="BorderThickness" Value="0"/>
                <Setter Property="Padding" Value="0"/>
            </Style>
        </Setter.Value>
    </Setter>
    
    <Setter Property="CalendarStyle" Value="{StaticResource DarkCalendarStyle}"/>
</Style>
```

### 5.5 带验证的日期输入

**应用场景**：表单录入，实时提示日期是否合法

xaml:

```xaml
<StackPanel Margin="20" Width="200">
    <TextBlock Text="交付日期：" Margin="0,0,0,5"/>
    <DatePicker x:Name="deliveryDatePicker"
                SelectedDate="{Binding DeliveryDate, Mode=TwoWay, ValidatesOnExceptions=True, ValidatesOnDataErrors=True}"
                Width="180">
        <DatePicker.ToolTip>
            <ToolTip Content="请选择未来30天内的交付日期"/>
        </DatePicker.ToolTip>
    </DatePicker>
    <TextBlock x:Name="validationText"
               Foreground="#F44336"
               FontSize="11"
               Margin="0,3,0,0"/>
</StackPanel>
```

csharp:

```c#
private void DeliveryDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
{
    if (!deliveryDatePicker.SelectedDate.HasValue)
    {
        validationText.Text = "请选择交付日期";
        return;
    }
    
    DateTime date = deliveryDatePicker.SelectedDate.Value;
    if (date < shturl.cc/P2y9)
    {
        validationText.Text = "交付日期不能早于今日";
    }
    else if (date > shturl.cc/P2y9.AddDays(30))
    {
        validationText.Text = "交付日期不能超过30天";
    }
    else
    {
        validationText.Text = string.Empty;
    }
}
```

------

## 六、最佳实践与常见坑点

### 6.1 工业开发最佳实践

1. **永远绑定`SelectedDate`，不要绑定`Text`**：`Text`可能包含未验证的非法字符串，`SelectedDate`永远是合法值或 null
2. **强制统一日期格式**：通过自定义模板固定为`yyyy-MM-dd`，避免不同终端区域设置导致的格式混乱和解析错误
3. **合理限制日期范围**：使用`DisplayDateStart`/`DisplayDateEnd`缩小可选区间，减少业务校验和用户误操作
4. **复用 Calendar 样式**：通过`CalendarStyle`属性复用已有的工业日历样式，保持全应用视觉一致
5. **禁用非法日期**：使用`BlackoutDates`禁用周末、节假日、停产日，从输入源头杜绝无效值
6. **保留键盘输入能力**：不要禁用文本输入，熟练操作人员键盘录入远快于鼠标点选日历

### 6.2 常见坑点与解决方案

#### 1. 输入非法日期后值不更新但文本保留

**问题**：用户输入错误日期，失去焦点后文本不变，但`SelectedDate`还是原来的值，造成视觉与实际值不一致

**解决方案**：在`LostFocus`事件中强制同步文本，或使用自定义模板始终由`SelectedDate`驱动显示

csharp:

```c#
private void DatePicker_LostFocus(object sender, RoutedEventArgs e)
{
    var dp = sender as DatePicker;
    if (dp.SelectedDate.HasValue)
    {
        // 强制用选中值刷新显示文本
        dp.Text = dp.SelectedDate.Value.ToString("yyyy-MM-dd");
    }
}
```

#### 2. BlackoutDates 在文本框输入不生效

**问题**：禁用日期在日历中无法点选，但用户可以直接在文本框输入禁用日期

**根本原因**：早期版本 WPF 的文本输入校验不检查 BlackoutDates

**解决方案**：在`SelectedDateChanged`中二次校验，发现禁用日期则回退

csharp:

```c#
private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
{
    var dp = sender as DatePicker;
    if (dp.SelectedDate.HasValue)
    {
        DateTime date = dp.SelectedDate.Value;
        if (dp.BlackoutDates.Contains(date))
        {
            // 回退到之前的值
            dp.SelectedDate = e.RemovedItems.Count > 0 
                ? (DateTime)e.RemovedItems[0] 
                : null;
            MessageBox.Show("该日期为禁用日期，无法选择！");
        }
    }
}
```

#### 3. 弹出日历位置偏移

**问题**：窗口靠近屏幕边缘时，弹出日历显示不全或位置错误

**解决方案**：设置 Popup 的`Placement`为`Bottom`，并启用自适应位置

xaml:

```xaml
<Setter Property="Popup.Placement" Value="Bottom"/>
<Setter Property="Popup.PlacementRect" Value="0,0,0,0"/>
```

------

## 七、总结

`DatePicker`是 WPF 中最常用的日期输入控件，它结合了文本框的高效录入与日历的可视化选择，是工业系统表单、查询栏场景的首选。它继承自`Control`，内部封装了`TextBox`和`Calendar`，通过双向同步机制保证输入的一致性。

掌握`DatePicker`的核心属性、校验机制和样式定制方法，可以快速开发出符合工业规范的日期录入界面，统一日期格式、限制可选范围、禁用非法日期，从源头减少业务错误。