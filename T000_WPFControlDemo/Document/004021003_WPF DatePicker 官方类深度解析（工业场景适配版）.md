# 004021003_WPF `DatePicker` 官方类深度解析（工业场景适配版）

**源码：**

```c#
public class DatePicker : Control
{
    public static readonly RoutedEvent SelectedDateChangedEvent;
    public static readonly DependencyProperty TextProperty;
    public static readonly DependencyProperty SelectedDateFormatProperty;
    public static readonly DependencyProperty IsTodayHighlightedProperty;
    public static readonly DependencyProperty IsDropDownOpenProperty;
    public static readonly DependencyProperty SelectedDateProperty;
    public static readonly DependencyProperty DisplayDateStartProperty;
    public static readonly DependencyProperty DisplayDateEndProperty;
    public static readonly DependencyProperty DisplayDateProperty;
    public static readonly DependencyProperty CalendarStyleProperty;
    public static readonly DependencyProperty FirstDayOfWeekProperty;
 
    public DatePicker();
 
    public CalendarBlackoutDatesCollection BlackoutDates { get; }
    public DateTime? DisplayDateStart { get; set; }
    public DateTime? DisplayDateEnd { get; set; }
    public DateTime DisplayDate { get; set; }
    public Style CalendarStyle { get; set; }
    public bool IsTodayHighlighted { get; set; }
    public bool IsDropDownOpen { get; set; }
    public DatePickerFormat SelectedDateFormat { get; set; }
    public string Text { get; set; }
    public DayOfWeek FirstDayOfWeek { get; set; }
    public DateTime? SelectedDate { get; set; }
    protected internal override bool HasEffectiveKeyboardFocus { get; }
 
    public event RoutedEventHandler CalendarClosed;
    public event RoutedEventHandler CalendarOpened;
    public event EventHandler<SelectionChangedEventArgs> SelectedDateChanged;
    public event EventHandler<DatePickerDateValidationErrorEventArgs> DateValidationError;
 
    public override void OnApplyTemplate();
    public override string ToString();
    protected virtual void OnCalendarClosed(RoutedEventArgs e);
    protected virtual void OnCalendarOpened(RoutedEventArgs e);
    protected override AutomationPeer OnCreateAutomationPeer();
    protected virtual void OnDateValidationError(DatePickerDateValidationErrorEventArgs e);
    protected virtual void OnSelectedDateChanged(SelectionChangedEventArgs e);
 
}
```

这是 WPF 框架原生 `System.Windows.Controls.DatePicker` 控件的类定义签名，定义于程序集 `PresentationFramework.dll`，是带下拉日历面板的标准日期输入控件，继承自 `Control` 基类，完整支持依赖属性绑定、路由事件、控件模板重定义等 WPF 核心特性。以下从继承体系、静态成员、实例属性、事件体系、可扩展方法、工业场景实践六个维度做逐字段解析。

------

## 一、继承体系与类定位

### 完整继承链

plaintext：

```tex
DispatcherObject
  → DependencyObject
    → Visual
      → UIElement
        → FrameworkElement
          → Control
            → DatePicker
```

- 核心定位：同时支持**文本手动输入**和**可视化日历选择**的日期输入控件，是表单录入、数据筛选、报表查询、产能上报等场景的标准输入组件。
- 设计特点：由「文本输入框 + 下拉按钮 + Popup 内嵌 Calendar 控件」三部分组成，通过控件模板可完全自定义外观。

------

## 二、静态成员：依赖属性与路由事件

所有 `static readonly` 字段均为 WPF 属性系统的标识符，是数据绑定、样式、动画、模板绑定的底层基础。

### 1. 核心业务依赖属性

| 依赖属性标识符             | 对应 CLR 属性      | 类型        | 默认值         | 核心说明                                                     |
| :------------------------- | :----------------- | :---------- | :------------- | :----------------------------------------------------------- |
| `SelectedDateProperty`     | `SelectedDate`     | `DateTime?` | `null`         | **控件主值属性**，双向绑定。可空类型，未选择时为 `null`；选中后仅保留年月日，时分秒默认为 `00:00:00`。 |
| `DisplayDateProperty`      | `DisplayDate`      | `DateTime`  | `DateTime.Now` | 日历面板当前展示的年月，仅控制视觉显示，不影响选中值；打开下拉时自动定位到该日期所在月份。 |
| `DisplayDateStartProperty` | `DisplayDateStart` | `DateTime?` | `null`         | 可选日期范围下限，早于该值的日期灰化不可选，用于限制上报 / 查询的最早日期。 |
| `DisplayDateEndProperty`   | `DisplayDateEnd`   | `DateTime?` | `null`         | 可选日期范围上限，晚于该值的日期灰化不可选，用于禁止选择未来日期。 |

### 2. 交互与样式依赖属性

| 依赖属性标识符               | 对应 CLR 属性        | 类型               | 默认值         | 核心说明                                                     |
| :--------------------------- | :------------------- | :----------------- | :------------- | :----------------------------------------------------------- |
| `TextProperty`               | `Text`               | `string`           | `string.Empty` | 输入框显示的原始文本，与 `SelectedDate` 双向同步；手动输入非法文本时会触发验证错误。 |
| `SelectedDateFormatProperty` | `SelectedDateFormat` | `DatePickerFormat` | `Short`        | 日期显示格式枚举：- `Short`：短日期（如 `2026/6/16`）- `Long`：长日期（如 `2026年6月16日`） |
| `IsDropDownOpenProperty`     | `IsDropDownOpen`     | `bool`             | `false`        | 控制日历下拉面板的展开 / 收起状态，支持双向绑定，可通过代码强制弹出面板。 |
| `IsTodayHighlightedProperty` | `IsTodayHighlighted` | `bool`             | `true`         | 是否在日历中高亮标注今日日期。                               |
| `FirstDayOfWeekProperty`     | `FirstDayOfWeek`     | `DayOfWeek`        | 系统区域默认值 | 日历每周的起始列，国内工业系统通常设置为 `Monday` 以匹配生产周统计规则。 |
| `CalendarStyleProperty`      | `CalendarStyle`      | `Style`            | `null`         | 内部下拉 `Calendar` 控件的样式，用于自定义单元格、标题、选中态外观，可实现「有产能日期标红、停机日标灰」等业务视觉。 |

### 3. 路由事件标识符

`SelectedDateChangedEvent`：对应 `SelectedDateChanged` 事件的路由事件标识符，路由策略为**冒泡**，支持在父容器统一监听日期变更。

------

## 三、实例属性详解

### 1. 核心集合属性

csharp:

```c#
public CalendarBlackoutDatesCollection BlackoutDates { get; }
```

- 只读集合，用于添加**禁用日期区间**，添加后的日期会灰化且无法被选中。

- 与 `DisplayDateStart/End` 的区别：后者是连续的范围限制，前者支持离散日期、多个区间组合禁用。

- 工业场景典型用法：

  csharp:

  ```c#
  // 禁用所有过去日期
  datePicker.BlackoutDates.AddDatesInPast();
  // 禁用指定区间（设备停机日）
  datePicker.BlackoutDates.Add(new CalendarDateRange(
      new DateTime(2026, 6, 10), 
      new DateTime(2026, 6, 12)));
  ```

### 2. 其他实例属性

- `SelectedDate`、`DisplayDate` 等均为对应依赖属性的 CLR 包装器，内部通过 `GetValue/SetValue` 操作 WPF 属性系统。
- `HasEffectiveKeyboardFocus`（`protected internal`）：内部焦点判断属性，标识键盘焦点是否位于控件内部（包括下拉日历面板），用于键盘导航逻辑。

------

## 四、事件体系与触发逻辑

### 1. `SelectedDateChanged`

csharp:

```c#
public event EventHandler<SelectionChangedEventArgs> SelectedDateChanged;
```

- **触发时机**：选中日期发生变更时（鼠标选择、代码赋值、绑定更新）。

- 事件参数 `SelectionChangedEventArgs` 包含 `AddedItems`（新选中日期）和 `RemovedItems`（原选中日期）。

- 工业场景典型用法：选完日期自动加载对应数据

  csharp:

  ```c#
  private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
  {
      if (datePicker.SelectedDate is DateTime date)
      {
          // 自动加载当日24小时产能数据
          LoadCapacityReport(date);
      }
  }
  ```

### 2. `DateValidationError`

csharp:

```c#
public event EventHandler<DatePickerDateValidationErrorEventArgs> DateValidationError;
```

- **触发时机**：用户手动输入非法日期文本（如 `2026/13/40`、非日期字符串）时触发。

- 事件参数包含 `Text`（输入的原始文本）、`Exception`（解析异常）、`ThrowException`（是否抛出异常）。

- 最佳实践：拦截异常，自动修正或提示，避免程序崩溃

  csharp:

  ```c#
  private void DatePicker_DateValidationError(object sender, DatePickerDateValidationErrorEventArgs e)
  {
      e.ThrowException = false; // 禁止抛出异常
      UpdateProcess($"日期输入非法：{e.Text}，已重置为当日");
      datePicker.SelectedDate = shturl.cc/VDmw;
  }
  ```

### 3. `CalendarOpened` / `CalendarClosed`

csharp:

```c#
public event RoutedEventHandler CalendarOpened;
public event RoutedEventHandler CalendarClosed;
```

- 路由事件，分别在日历下拉面板展开、收起时触发。
- 典型用法：展开时动态拉取 MES 接口返回的当月停机日，动态更新 `BlackoutDates`。

------

## 五、可重写方法（自定义控件扩展）

继承 `DatePicker` 实现工业专用日期控件时，核心重写点：

### 1. `OnApplyTemplate()`

控件模板加载完成后调用，是自定义控件的入口。可通过 `GetTemplateChild` 获取内部命名元素：

- `PART_TextBox`：内部输入框
- `PART_Popup`：下拉弹出面板
- `PART_Calendar`：内嵌日历控件

常用于扩展功能：增加「今日」「清除」快捷按钮、限制输入字符、强制日期格式等。

### 2. `OnSelectedDateChanged(SelectionChangedEventArgs e)`

触发 `SelectedDateChanged` 事件的受保护虚方法。子类重写可在事件触发前插入统一业务逻辑，比如全局日期范围校验、自动补全时分秒等。

### 3. `OnCalendarOpened(RoutedEventArgs e)` / `OnCalendarClosed(RoutedEventArgs e)`

下拉面板开闭的虚方法，可重写实现面板展开前的数据预加载、关闭后的最终校验。

### 4. `OnDateValidationError(DatePickerDateValidationErrorEventArgs e)`

验证错误的虚方法，可重写实现全局统一的错误处理策略。

### 5. 其他

- `OnCreateAutomationPeer()`：创建自动化对等类，用于 UI 自动化测试、无障碍访问。
- `ToString()`：重写自 `Object`，返回选中日期的字符串表示，未选中时返回空字符串。

------

## 六、工业 MES / 产能系统最佳实践与避坑

### 1. 标准 MVVM 绑定写法

xaml:

```xaml
<DatePicker
    SelectedDate="{Binding ReportDate, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
    SelectedDateFormat="Short"
    FirstDayOfWeek="Monday"
    DisplayDateEnd="{x:Static sys:shturl.cc/VDmw}"
    DateValidationError="DatePicker_DateValidationError"/>
```

### 2. 高频坑点规避

1. **时分秒丢失问题**：`SelectedDate` 仅包含日期，时间固定为 `00:00:00`。查询当日全量数据时，结束时间需手动补全：

   csharp:

   ```c#
   DateTime start = datePicker.SelectedDate.Value;
   DateTime end = start.AddDays(1).AddTicks(-1); // 当天 23:59:59.999
   ```

2. **空值安全**：`SelectedDate` 为可空类型，必须先判空再访问 `.Value`，禁止直接调用抛空引用异常。

3. **BlackoutDates 重复添加**：动态更新禁用日期前必须先调用 `Clear()`，否则会抛出「日期范围重叠」异常。

4. **数据上报格式**：对接 MES 接口时，需按接口要求格式化日期字符串（如 `yyyy-MM-dd HH:mm:ss`），不要直接依赖 `Text` 属性。