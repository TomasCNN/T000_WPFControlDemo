# 004020002_WPF `Calendar` 日历控件官方源代码级逐行深度解析（.NET 8 最终版）

本文严格基于微软官方.NET 8 开源代码，延续之前`ScrollBar`/`Slider`/`ProgressBar`的解析体系，从**类元数据、特性、继承链、静态成员、实例属性、方法、内部机制**七个维度进行 100% 源码级解析，重点突出官方设计意图和工业自动化场景的核心应用要点。

------

## 一、类定义总览与核心元数据

### 1.1 官方完整类签名（带所有特性）

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

### 1.2 核心元数据（官方精确值）

| 项               | 官方值                                                       | 工业场景关键说明                                             |
| :--------------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| **命名空间**     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                                         |
| **程序集**       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                                           |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → Calendar` | **独立复合控件**，不继承自 RangeBase，与之前的三个控件无直接继承关系 |
| **抽象性**       | 非抽象                                                       | 可直接实例化                                                 |
| **可继承性**     | 未密封                                                       | 官方明确支持自定义扩展                                       |
| **模板强制部件** | `PART_Root`（根面板）、`PART_CalendarItem`（核心日历项）     | 缺少任何一个都会导致日历完全失效，但**不会抛出任何异常**（WPF 通用坑） |
| **自动化对等类** | `CalendarAutomationPeer`                                     | 支持屏幕阅读器和 UI 自动化测试                               |
| **线程安全**     | 仅 UI 线程安全                                               | 所有日期操作必须在 Dispatcher 线程执行                       |
| **本地化支持**   | 自动适配系统区域设置                                         | 月份、星期名称会自动显示为系统语言                           |

### 1.3 特性深度解析

1. **`[Localizability(LocalizationCategory.None)]`**
   - 官方含义：`Calendar`控件本身不需要手动本地化
   - 核心行为：自动读取系统的`CultureInfo`，将月份、星期名称转换为对应语言
   - 工业注意：如果需要强制使用特定语言（如中文），需在应用启动时设置全局文化
2. **`[TemplatePart(...)]` 两个强制部件**
   - **`PART_Root`**：类型为`Panel`，是日历所有子元素的根容器
   - **`PART_CalendarItem`**：类型为`CalendarItem`，是日历的核心渲染部件，负责生成月 / 年 / 十年视图
   - **官方强制契约**：任何自定义`Calendar`模板必须包含这两个命名完全匹配的部件
   - **内部实现**：`OnApplyTemplate()`方法会通过`Template.FindName()`查找这两个部件，如果找不到，日历将显示为空白且无任何报错

> ⚠️ 工业开发红线：自定义日历模板时，必须严格保留`PART_Root`和`PART_CalendarItem`的命名，否则会导致生产环境中日历 "假死"。

------

## 二、继承链与设计定位

### 2.1 与其他 RangeBase 控件的区别

`Calendar`是 WPF 标准控件中少数几个直接继承自`Control`的复杂控件，与之前解析的三个 RangeBase 控件定位完全不同：

| 控件          | 继承链    | 核心设计定位     | 交互性 | 工业应用                     |
| :------------ | :-------- | :--------------- | :----- | :--------------------------- |
| `ScrollBar`   | RangeBase | 内容滚动位置调节 | 强交互 | 列表、图像滚动               |
| `Slider`      | RangeBase | 连续参数值调节   | 强交互 | 温度、压力、速度调节         |
| `ProgressBar` | RangeBase | 进度值可视化     | 无交互 | 任务进度、状态显示           |
| `Calendar`    | Control   | 日期选择与展示   | 中交互 | 生产计划、数据查询、维护管理 |

### 2.2 官方设计思想

微软设计`Calendar`的核心思想可以概括为三点：

1. **分层设计**：将复杂的日历逻辑拆分为`Calendar`（顶层 API）、`CalendarItem`（视图渲染）、`CalendarDayButton`（单个日期）三个层次
2. **自动本地化**：完全依赖系统区域设置处理日期格式和语言，无需开发者手动翻译
3. **高度可定制**：通过`CalendarDayButtonStyle`、`CalendarButtonStyle`等样式属性，允许开发者完全自定义每个日期的外观

------

## 三、静态成员逐行解析

### 3.1 静态依赖属性（按官方注册顺序）

所有依赖属性都使用`FrameworkPropertyMetadata`注册，并包含**强制值回调**和**属性变更回调**，确保值的合法性和自动更新。

csharp:

```c#
// 静态依赖属性（按官方注册顺序）
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
```

#### 1. 选择核心属性（工业场景最高频）

表格







| 属性                        | 官方定义           | 默认值                             | 工业最佳实践                                                 |
| :-------------------------- | :----------------- | :--------------------------------- | :----------------------------------------------------------- |
| **`SelectedDateProperty`**  | 当前选中的单个日期 | `null`                             | 单选模式下优先使用，MVVM 绑定首选                            |
| **`SelectedDatesProperty`** | 选中的日期集合     | 空集合                             | 多选 / 范围选择模式下使用，类型为`ObservableCollection<DateTime>` |
| **`SelectionModeProperty`** | 选择模式           | `CalendarSelectionMode.SingleDate` | 工业常用：`SingleDate`（单选）、`SingleRange`（连续范围）    |
| **`BlackoutDatesProperty`** | 不可选择的日期集合 | 空集合                             | 用于禁用周末、节假日、设备停机日、已完成日期                 |

**`SelectionMode`枚举官方精确值**：

csharp:

```c#
public enum CalendarSelectionMode
{
    SingleDate = 0,    // 只能选择单个日期（默认）
    SingleRange = 1,   // 只能选择一个连续的日期范围
    MultipleRange = 2  // 可以选择多个不连续的日期范围
}
```

**`BlackoutDates`官方内部实现**：

- 类型为`CalendarBlackoutDatesCollection`，是一个强类型的集合
- 支持添加单个日期或日期范围：`Add(DateTime)` / `Add(CalendarDateRange)`
- 自动去重和合并重叠的日期范围
- 渲染时会自动将集合中的日期对应的`CalendarDayButton.IsEnabled`设为`false`

#### 2. 显示控制属性

| 属性                             | 官方定义           | 默认值               | 工业最佳实践                                         |
| :------------------------------- | :----------------- | :------------------- | :--------------------------------------------------- |
| **`DisplayDateProperty`**        | 当前显示的基准日期 | `DateTime.Now`       | 决定日历显示哪个月 / 年，程序控制导航的核心属性      |
| **`DisplayDateStartProperty`**   | 可显示的最早日期   | `DateTime.MinValue`  | 限制只能查看未来日期（如生产计划）                   |
| **`DisplayDateEndProperty`**     | 可显示的最晚日期   | `DateTime.MaxValue`  | 限制只能查看过去日期（如历史数据查询）               |
| **`DisplayModeProperty`**        | 显示视图模式       | `CalendarMode.Month` | 工业几乎只用`Month`（月视图），极少用`Year`/`Decade` |
| **`FirstDayOfWeekProperty`**     | 一周的第一天       | 系统区域设置         | **工业强制设为`DayOfWeek.Monday`**，符合生产排班习惯 |
| **`IsTodayHighlightedProperty`** | 是否高亮今天       | `true`               | 保持默认，方便操作人员快速定位当前日期               |

#### 3. 行为属性

无额外行为属性，所有行为都通过上述属性控制。

### 3.2 静态路由事件

csharp:

```c#
// 静态路由事件
public static readonly RoutedEvent DisplayDateChangedEvent;
public static readonly RoutedEvent SelectedDatesChangedEvent;
```

| 事件                            | 官方路由策略 | 触发时机                        | 工业核心应用                                   |
| :------------------------------ | :----------- | :------------------------------ | :--------------------------------------------- |
| **`SelectedDatesChangedEvent`** | 冒泡         | 选中的日期发生变化时            | 查询选中日期的生产数据、生成报表、加载维护计划 |
| **`DisplayDateChangedEvent`**   | 冒泡         | 当前显示的月份 / 年份发生变化时 | 预加载当前月份的生产数据、标记特殊日期         |

> 🔑 官方设计要点：两个事件都是**冒泡路由事件**，可以在视觉树的任何父元素上监听，实现 UI 与逻辑的解耦。

------

## 四、实例成员逐行解析

### 4.1 公共属性

所有公共属性都是对应静态依赖属性的包装，没有额外逻辑：

csharp:

```c#
// 公共属性（按官方定义顺序）
public CalendarBlackoutDatesCollection BlackoutDates { get; }
public DateTime? DisplayDate { get; set; }
public DateTime? DisplayDateEnd { get; set; }
public DateTime? DisplayDateStart { get; set; }
public CalendarMode DisplayMode { get; set; }
public DayOfWeek FirstDayOfWeek { get; set; }
public bool IsTodayHighlighted { get; set; }
public DateTime? SelectedDate { get; set; }
public SelectedDatesCollection SelectedDates { get; }
public CalendarSelectionMode SelectionMode { get; set; }
```

> ⚠️ 关键注意：`BlackoutDates`和`SelectedDates`都是**只读属性**，只能修改集合本身（Add/Remove/Clear），不能重新赋值。

### 4.2 公共方法

#### 导航方法（程序控制日历跳转）

csharp:

```c#
public void DisplayDateNextMonth();    // 跳转到下一个月
public void DisplayDatePreviousMonth(); // 跳转到上一个月
public void DisplayDateNextYear();     // 跳转到下一年
public void DisplayDatePreviousYear(); // 跳转到上一年
```

- **官方实现逻辑**：直接修改`DisplayDate`属性，触发`DisplayDateChanged`事件
- **工业应用**：添加自定义导航按钮，实现快速跳转（如 "本月"、"下月"、"去年"）

#### `OnApplyTemplate()`

csharp:

```c#
public override void OnApplyTemplate();
```

- **官方完整实现逻辑**：
  1. 调用基类`OnApplyTemplate()`
  2. 查找`PART_Root`根面板
  3. 查找`PART_CalendarItem`核心日历项
  4. 初始化`CalendarItem`的属性（如`DisplayMode`、`FirstDayOfWeek`）
  5. 订阅`CalendarItem`的内部事件
  6. 刷新日历显示

### 4.3 受保护虚方法（官方扩展点）

这些方法是`Calendar`提供给子类的核心扩展点，所有事件最终都会调用这些方法：

#### `OnSelectedDatesChanged(SelectionChangedEventArgs e)`

csharp:

```c#
protected virtual void OnSelectedDatesChanged(SelectionChangedEventArgs e);
```

- **触发时机**：选中的日期发生变化时
- **官方默认实现**：触发`SelectedDatesChanged`路由事件
- **工业扩展**：重写此方法可以实现自定义选择逻辑，如限制最多选择 7 天

#### `OnDisplayDateChanged(CalendarDateChangedEventArgs e)`

csharp:

```c#
protected virtual void OnDisplayDateChanged(CalendarDateChangedEventArgs e);
```

- **触发时机**：当前显示的月份 / 年份发生变化时
- **官方默认实现**：触发`DisplayDateChanged`路由事件
- **工业扩展**：重写此方法可以预加载当前月份的数据，提升用户体验

#### `OnCreateAutomationPeer()`

csharp:

```c#
protected override AutomationPeer OnCreateAutomationPeer();
```

- **官方实现**：返回一个`CalendarAutomationPeer`实例
- **作用**：支持屏幕阅读器和 UI 自动化测试

------

## 五、官方内部工作原理

### 5.1 完整控件层次结构

`Calendar`是一个多层复合控件，其内部结构如下：

plaintext:

```tex
Calendar
└── PART_Root (Grid)
    └── PART_CalendarItem (CalendarItem)
        ├── PART_Header (Button)           // 标题栏（显示年月）
        ├── PART_PreviousButton (Button)   // 上一月按钮
        ├── PART_NextButton (Button)       // 下一月按钮
        ├── PART_MonthView (Grid)          // 月视图网格
        │   ├── 星期表头（7个TextBlock）
        │   └── 日期网格（6行×7列=42个CalendarDayButton）
        ├── PART_YearView (Grid)           // 年视图（12个月）
        └── PART_DecadeView (Grid)         // 十年视图（10年）
```

- **`CalendarItem`**：核心渲染部件，根据`DisplayMode`切换显示月 / 年 / 十年视图
- **`CalendarDayButton`**：每个日期对应的按钮，继承自`Button`，负责单个日期的选择和显示
- **官方优化**：月视图只渲染 42 个日期按钮（6 周 ×7 天），通过数据绑定显示不同月份的日期，避免频繁创建销毁控件

### 5.2 日期选择完整流程

从用户点击日期到 UI 更新的完整官方流程：

plaintext:

```tex
用户点击CalendarDayButton
    ↓
CalendarDayButton处理Click事件
    ↓
根据SelectionMode更新SelectedDates集合
    ↓
触发INotifyCollectionChanged.CollectionChanged事件
    ↓
Calendar内部监听CollectionChanged事件
    ↓
更新SelectedDate属性（取集合第一个元素）
    ↓
调用OnSelectedDatesChanged()方法
    ↓
触发SelectedDatesChanged路由事件
    ↓
更新所有CalendarDayButton的IsSelected状态
    ↓
UI重新渲染
```

### 5.3 不可选日期（BlackoutDates）实现机制

1. 当向`BlackoutDates`集合添加日期时，触发`CollectionChanged`事件
2. `Calendar`内部监听此事件，通知所有`CalendarDayButton`刷新状态
3. 每个`CalendarDayButton`在渲染时，会检查自己的日期是否在`BlackoutDates`中
4. 如果存在，则将`IsEnabled`设为`false`，并应用禁用样式
5. 禁用的日期按钮无法被点击，也无法通过键盘选中

------

## 六、官方设计思想与工业开发核心启示

### 6.1 官方设计思想总结

1. **分层解耦**：将顶层 API、视图渲染、单个日期交互拆分为三个独立层次，职责清晰
2. **自动本地化**：完全依赖系统文化处理日期格式，减少开发者工作量
3. **数据驱动**：所有 UI 状态都由依赖属性驱动，支持 MVVM 模式
4. **高度可定制**：通过样式和模板允许完全自定义外观，同时保留核心逻辑不变

### 6.2 工业开发核心启示

1. **强制设置 FirstDayOfWeek 为 Monday**：符合工业生产排班习惯，避免混淆
2. **合理限制日期范围**：使用`DisplayDateStart`和`DisplayDateEnd`限制可查看的日期，提升性能和安全性
3. **善用 BlackoutDates**：禁用所有不需要选择的日期（周末、节假日、停机日），减少操作人员误操作
4. **预加载月份数据**：在`DisplayDateChanged`事件中预加载当前月份的数据，避免用户等待
5. **自定义日期样式标记状态**：通过`CalendarDayButtonStyle`标记不同状态的日期（维护日、生产日、停机日、报警日）
6. **优先使用 SelectedDate 进行 MVVM 绑定**：比`SelectedDates`更简单，适合大多数工业场景
7. **避免使用 MultipleRange 模式**：操作复杂且容易出错，工业场景几乎用不到