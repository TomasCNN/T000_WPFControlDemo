# 004021002_WPF `DatePicker` 日期选择控件官方源代码级逐行深度解析（.NET 8 最终版）

结合微软官方.NET 8 开源源码，延续此前`Slider`/`ProgressBar`/`Calendar`的解析体系，从**类元数据、特性契约、静态成员、实例属性、事件、方法、内部运行机制**七个维度 100% 覆盖所有公开接口，重点补充官方内部实现细节与工业自动化场景的核心注意事项。

------

## 一、类定义总览与核心元数据

### 1.1 官方完整类签名（含所有隐含特性）

你提供的是公开接口定义，官方完整类还标注了三个关键特性（模板强制契约）：

csharp：

```c#
namespace System.Windows.Controls
{
    [System.Windows.LocalizabilityAttribute(System.Windows.LocalizationCategory.None)]
    [System.Windows.TemplatePartAttribute(Name = "PART_TextBox", Type = typeof(System.Windows.Controls.DatePickerTextBox))]
    [System.Windows.TemplatePartAttribute(Name = "PART_Popup", Type = typeof(System.Windows.Controls.Primitives.Popup))]
    public class DatePicker : System.Windows.Controls.Control
    {
        // 静态路由事件
        public static readonly RoutedEvent SelectedDatesChangedEvent;

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

| 项               | 官方精确值                                                   | 工业场景关键说明                                             |
| :--------------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| **命名空间**     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                                         |
| **程序集**       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                                           |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → DatePicker` | 直接继承自`Control`的复合控件，**内部封装了`Calendar`+`TextBox`+`Popup`** |
| **模板强制部件** | `PART_TextBox`（专用输入框）、`PART_Popup`（下拉弹出层）     | 命名与类型必须完全匹配，缺失则功能静默失效，无异常抛出       |
| **抽象性**       | 非抽象                                                       | 可直接实例化                                                 |
| **可继承性**     | 未密封                                                       | 官方支持子类扩展                                             |
| **自动化对等类** | `DatePickerAutomationPeer`                                   | 支持 UI 自动化测试与无障碍访问                               |
| **设计定位**     | **下拉式日期输入控件**                                       | 兼顾键盘录入效率与可视化日历选择，占用空间小，适合表单、查询栏等紧凑布局 |
| **工业核心应用** | 生产数据查询、批次录入、设备维护计划、报表筛选、参数配置     |                                                              |

### 1.3 特性深度解析

1. **`[Localizability(LocalizationCategory.None)]`**
   - 官方含义：控件本身无需手动本地化，日期格式默认跟随系统区域设置
   - ⚠️ **工业红线**：多区域部署场景下，**绝对不要依赖系统默认格式**，必须强制固定为`yyyy-MM-dd`，否则不同终端的日期解析逻辑会出现不可预期的错误。
2. **`[TemplatePart(...)]` 两个强制部件契约**
   - **`PART_TextBox`**：类型为`DatePickerTextBox`（WPF 专用派生类，继承自`TextBox`），负责日期的文本显示与键盘输入
   - **`PART_Popup`**：类型为`Popup`，点击下拉按钮时弹出，内部承载完整的`Calendar`控件
   - 官方实现逻辑：`OnApplyTemplate()`中通过`Template.FindName()`查找这两个部件，绑定文本变更、弹出关闭、日历选择等事件；若查找失败，控件会出现 "点击下拉无反应"" 输入不生效 " 等隐性 bug，且不抛出任何异常。

> 🔑 工业开发核心提示：自定义 DatePicker 模板时，必须严格保留两个`PART_*`部件的命名与类型，这是官方强制的 UI - 逻辑分离契约。

------

## 二、继承链与设计定位

### 2.1 与 Calendar 控件的关系

`DatePicker`不是`Calendar`的子类，而是**组合封装**关系：

- `DatePicker`对外暴露精简的日期输入 API
- 内部持有一个完整的`Calendar`实例，所有日历相关的属性（`DisplayDate`/`BlackoutDates`/`FirstDayOfWeek`等）全部透传给内部 Calendar
- 额外增加了文本输入、下拉弹出、日期格式化三大能力

### 2.2 官方设计思想

微软设计`DatePicker`的核心逻辑是**场景互补**：

| 控件         | 占用空间           | 输入方式            | 适用场景                             |
| :----------- | :----------------- | :------------------ | :----------------------------------- |
| `Calendar`   | 大（固定月历视图） | 鼠标点选为主        | 排班、计划等需要持续查看日历的场景   |
| `DatePicker` | 小（单行输入框）   | 键盘输入 + 下拉点选 | 表单、查询栏等紧凑布局的单次日期选择 |

------

## 三、静态成员逐行解析

### 3.1 静态路由事件

csharp:

```c#
public static readonly RoutedEvent SelectedDatesChangedEvent;
```

- **官方定义**：对应`SelectedDatesChanged`事件的路由事件字段
- **路由策略**：`RoutingStrategy.Bubble`（冒泡）
- **触发时机**：选中的日期值发生任何变化时（日历点选、文本输入、代码赋值）
- **工业价值**：支持在父容器统一监听所有日期选择器的变更事件，实现全局查询联动、数据校验等逻辑。

> 注：官方实际命名为`SelectedDateChangedEvent`（单数），因 DatePicker 仅支持单选；你提供的定义中为复数形式，属于命名差异，功能完全一致。

### 3.2 静态依赖属性（按官方定义顺序）

所有属性均通过`DependencyProperty.Register`注册，完整支持数据绑定、样式、动画、继承等 WPF 核心特性。

csharp:

```c#
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
```

#### 核心值属性（工业开发最高频）

表格







| 属性字段                     | 包装属性             | 类型               | 默认值         | 官方作用                     | 工业最佳实践                                                 |
| :--------------------------- | :------------------- | :----------------- | :------------- | :--------------------------- | :----------------------------------------------------------- |
| `SelectedDateProperty`       | `SelectedDate`       | `DateTime?`        | `null`         | 当前选中的日期，控件的核心值 | **MVVM 绑定唯一首选**，始终通过此属性获取 / 设置日期，永远不要直接绑定`Text` |
| `TextProperty`               | `Text`               | `string`           | `string.Empty` | 输入框中显示的日期文本       | 仅用于自定义显示格式，不可作为值来源；输入非法值时会出现文本与实际值不一致 |
| `SelectedDateFormatProperty` | `SelectedDateFormat` | `DatePickerFormat` | `Short`        | 控制日期文本的显示格式       | 工业场景**禁止使用默认值**，必须通过自定义模板强制固定为`yyyy-MM-dd`，消除本地化差异 |

`DatePickerFormat`官方枚举定义：

csharp:

```c#
public enum DatePickerFormat
{
    Long = 0,   // 系统长日期格式，如"2025年6月15日 星期日"
    Short = 1   // 系统短日期格式，如"2025/6/15"（随区域设置变化）
}
```

#### 日历透传属性（完全等价于 Calendar 同名属性）

这些属性全部透传给内部的`Calendar`控件，行为与 Calendar 完全一致：

| 属性字段                     | 包装属性             | 类型        | 默认值           | 作用                         |
| :--------------------------- | :------------------- | :---------- | :--------------- | :--------------------------- |
| `DisplayDateProperty`        | `DisplayDate`        | `DateTime`  | `DateTime.Now`   | 弹出日历时默认显示的基准月份 |
| `DisplayDateStartProperty`   | `DisplayDateStart`   | `DateTime?` | `null`（无下限） | 可选择的最早日期             |
| `DisplayDateEndProperty`     | `DisplayDateEnd`     | `DateTime?` | `null`（无上限） | 可选择的最晚日期             |
| `FirstDayOfWeekProperty`     | `FirstDayOfWeek`     | `DayOfWeek` | 系统区域默认值   | 一周的第一天                 |
| `IsTodayHighlightedProperty` | `IsTodayHighlighted` | `bool`      | `true`           | 是否高亮标记今日             |

#### 交互与样式属性

| 属性字段                 | 包装属性         | 类型    | 默认值  | 作用                           | 工业应用                                            |
| :----------------------- | :--------------- | :------ | :------ | :----------------------------- | :-------------------------------------------------- |
| `IsDropDownOpenProperty` | `IsDropDownOpen` | `bool`  | `false` | 控制下拉日历的展开 / 收起状态  | 程序控制自动弹出，或实现获取焦点自动展开的交互      |
| `CalendarStyleProperty`  | `CalendarStyle`  | `Style` | `null`  | 自定义内部弹出 Calendar 的样式 | 复用全局工业日历样式，保持视觉一致性                |
| `TextBoxStyleProperty`   | `TextBoxStyle`   | `Style` | `null`  | 自定义输入框的样式             | 适配深色 / 浅色工业主题，调整输入框高度、字体、边框 |

------

## 四、实例属性逐行解析

### 4.1 核心值属性

csharp:

```c#
public DateTime? SelectedDate { get; set; }
public string Text { get; set; }
public DatePickerFormat SelectedDateFormat { get; set; }
```

1. **`SelectedDate`**
   - 可空`DateTime`类型，`null`表示未选择任何日期
   - 官方保证：所有赋值都会经过范围、格式、禁用三层校验，非法值会被自动拒绝
   - 工业开发铁则：所有业务逻辑只认`SelectedDate`，永远不信任`Text`属性。
2. **`Text`**
   - 输入框的显示文本，支持用户手动输入
   - 核心坑点：用户输入非法日期后，失去焦点时`Text`可能保留错误字符串，但`SelectedDate`仍是之前的有效值，出现**视觉与实际值不一致**的问题
   - 官方同步机制：仅在失去焦点、按下回车、日历选择时才会尝试同步`Text`到`SelectedDate`。
3. **`SelectedDateFormat`**
   - 仅控制显示格式，不影响内部值的存储
   - 仅支持`Long`/`Short`两种系统格式，无法直接指定自定义格式；需要固定格式必须通过自定义模板实现。

### 4.2 日历控制属性（透传）

csharp:

```c#
public DateTime DisplayDate { get; set; }
public DateTime? DisplayDateStart { get; set; }
public DateTime? DisplayDateEnd { get; set; }
public bool IsTodayHighlighted { get; set; }
public DayOfWeek FirstDayOfWeek { get; set; }
public CalendarBlackoutDatesCollection BlackoutDates { get; }
```

- 所有属性与`Calendar`控件行为完全一致，内部直接绑定到内嵌 Calendar 的对应属性
- **`BlackoutDates`**：只读集合属性，类型为`CalendarBlackoutDatesCollection`，用于添加不可选择的日期（周末、节假日、停产日）；添加后日历中对应日期会自动禁用。
  - ⚠️ 已知官方缺陷：早期 WPF 版本中，**文本输入不会校验 BlackoutDates**，用户可通过键盘输入禁用日期；工业场景必须在`SelectedDateChanged`中二次校验。

### 4.3 交互与样式属性

csharp:

```c#
public bool IsDropDownOpen { get; set; }
public Style CalendarStyle { get; set; }
public Style TextBoxStyle { get; set; }
```

1. **`IsDropDownOpen`**
   - 可读写，设置为`true`可程序控制弹出下拉日历
   - 用户点击下拉按钮、按 F4 键都会自动修改此属性
2. **`CalendarStyle` / `TextBoxStyle`**
   - 官方样式透传机制，应用到内部对应的子控件上
   - 工业开发最佳实践：全局统一定义工业风格 Calendar 样式，通过此属性复用到所有 DatePicker，避免重复代码。

------

## 五、事件逐行解析

csharp:

```c#
public event EventHandler<SelectionChangedEventArgs> SelectedDatesChanged;
public event EventHandler DropDownOpened;
public event EventHandler DropDownClosed;
```

### 5.1 `SelectedDatesChanged`

- **触发时机**：`SelectedDate`值发生任何变化时（鼠标点选、键盘输入、代码赋值）
- **事件参数**：`SelectionChangedEventArgs`，包含`AddedItems`（新选中的日期）和`RemovedItems`（取消选中的日期）
- **工业核心应用**：
  - 日期变更后自动触发数据查询、报表刷新
  - 二次校验日期合法性（如 BlackoutDates 补漏、业务规则校验）
  - 联动其他控件（如结束日期不能早于开始日期）

### 5.2 `DropDownOpened`

- **触发时机**：下拉日历弹出展开时
- **事件参数**：普通`EventArgs`
- **工业应用**：弹出时异步加载当月的日期标记数据、维护计划、停产信息，延迟加载提升性能。

### 5.3 `DropDownClosed`

- **触发时机**：下拉日历收起关闭时
- **事件参数**：普通`EventArgs`
- **工业应用**：关闭后执行最终值校验、提交查询、保存输入结果；也可用于修正非法输入的显示文本。

------

## 六、核心方法逐行解析

### 6.1 公共方法

#### 构造函数

csharp:

```c#
public DatePicker();
```

- **官方内部实现逻辑**：
  1. 初始化所有依赖属性的默认元数据
  2. 初始化`BlackoutDates`集合并监听集合变更
  3. 绑定默认样式与控件模板
  4. 注册默认的命令与键盘快捷键（F4 展开下拉、Esc 收起等）

#### `OnApplyTemplate()`

csharp:

```c#
public override void OnApplyTemplate();
```

- **官方完整执行流程**：
  1. 调用基类`OnApplyTemplate()`
  2. 查找`PART_TextBox`部件，注册`TextChanged`、`LostFocus`、`PreviewKeyDown`事件
  3. 查找`PART_Popup`部件，注册`Opened`、`Closed`事件
  4. 初始化内部`Calendar`控件，将所有日历相关属性双向绑定到 Calendar
  5. 绑定下拉按钮的点击事件，控制`IsDropDownOpen`切换
  6. 根据当前`SelectedDate`初始化文本显示

#### `ToString()`

csharp:

```c#
public override string ToString();
```

- **官方实现**：返回`SelectedDate`的文本表示，格式由`SelectedDateFormat`决定；未选择日期时返回空字符串。
- **作用**：调试时快速查看当前选中值。

### 6.2 受保护虚方法（官方扩展点）

这些方法是子类扩展的核心入口，所有事件最终都会调用对应虚方法。

#### `OnCreateAutomationPeer()`

csharp:

```c#
protected override AutomationPeer OnCreateAutomationPeer();
```

- 官方实现：返回`DatePickerAutomationPeer`实例，支持 UI 自动化测试与屏幕阅读器。

#### `OnSelectedDateChanged(SelectionChangedEventArgs e)`

csharp:

```c#
protected virtual void OnSelectedDateChanged(SelectionChangedEventArgs e);
```

- **触发时机**：选中日期变化时
- **官方默认实现**：触发`SelectedDatesChanged`路由事件，同步更新`Text`属性
- **工业扩展场景**：
  - 添加自定义业务校验规则
  - 实现日期范围联动（结束日期自动≥开始日期）
  - 自动触发数据查询逻辑

#### `OnDropDownOpened(EventArgs e)`

csharp:

```c#
protected virtual void OnDropDownOpened(EventArgs e);
```

- **触发时机**：下拉弹出时
- **官方默认实现**：触发`DropDownOpened`事件，将内部 Calendar 的`DisplayDate`同步到当前选中日期
- **工业扩展**：弹出时异步加载当月业务数据、更新日期状态标记。

#### `OnDropDownClosed(EventArgs e)`

csharp:

```c#
protected virtual void OnDropDownClosed(EventArgs e);
```

- **触发时机**：下拉关闭时
- **官方默认实现**：触发`DropDownClosed`事件，将焦点移回输入框
- **工业扩展**：关闭时执行最终值校验，修正非法输入的显示文本。

------

## 七、官方内部核心工作机制

### 7.1 内部控件层级结构

plaintext:

```tex
DatePicker
├── PART_TextBox (DatePickerTextBox)  // 日期输入框
│   └── 下拉按钮 (ToggleButton)       // 触发展开/收起
└── PART_Popup (Popup)                // 下拉弹出层
    └── Calendar                      // 内嵌完整日历控件
```

### 7.2 双向同步核心机制

DatePicker 最核心的设计是**文本输入与日历选择的双向值同步**，官方内置两套同步逻辑：

1. **日历选择 → 文本同步**

   plaintext:

   ```tex
   用户点击日历日期
       ↓
   内部Calendar更新SelectedDate
       ↓
   DatePicker同步更新自身SelectedDate
       ↓
   按SelectedDateFormat格式化日期为字符串
       ↓
   更新Text属性与输入框显示
       ↓
   自动关闭Popup下拉
   ```

2. **文本输入 → 值同步**

   plaintext:

   ```tex
   用户在文本框输入字符串
       ↓
   失去焦点 / 按下回车键
       ↓
   尝试按系统格式解析字符串为DateTime
       ↓
   校验日期是否在DisplayDate范围内
       ↓
   校验日期是否在BlackoutDates中（部分版本生效）
       ↓
   校验通过 → 更新SelectedDate → 同步Calendar显示
   校验失败 → 恢复Text为原值，SelectedDate保持不变
   ```

### 7.3 三层合法性校验

官方内置三层校验，确保`SelectedDate`永远是合法值：

1. **格式校验**：输入字符串能否解析为有效日期
2. **范围校验**：日期是否在`DisplayDateStart`~`DisplayDateEnd`区间内
3. **禁用校验**：日期是否属于`BlackoutDates`禁用集合

> 任意一层校验不通过，都会拒绝更新`SelectedDate`，并保留原有有效值。

------

## 八、官方设计思想与工业开发核心启示

### 8.1 官方设计思想总结

1. **组合优于继承**：不继承 Calendar，而是内部封装 Calendar，对外暴露更精简的输入型 API
2. **双输入模式**：兼顾键盘录入效率与鼠标可视化选择，适配不同操作习惯的用户
3. **值安全优先**：所有输入都经过多层校验，确保`SelectedDate`属性永远合法，业务层无需处理非法值
4. **样式可扩展**：通过`CalendarStyle`/`TextBoxStyle`透传样式，支持深度定制外观且不破坏核心逻辑

### 8.2 工业开发核心启示

1. **只信任 SelectedDate**：永远不要用`Text`做业务逻辑，`SelectedDate`是唯一可靠的值来源
2. **强制固定日期格式**：不要依赖系统区域格式，通过自定义模板固定为`yyyy-MM-dd`，杜绝多环境下的格式混乱
3. **二次校验 BlackoutDates**：弥补官方文本输入不校验禁用日期的缺陷，在`SelectedDateChanged`中补校验
4. **限制日期范围**：用`DisplayDateStart`/`DisplayDateEnd`从输入源头缩小可选区间，减少业务层校验成本
5. **复用全局 Calendar 样式**：通过`CalendarStyle`属性统一样式，避免重复开发
6. **保留键盘输入能力**：不要禁用文本框，熟练操作人员的键盘录入效率远高于点选日历
7. **修正非法输入显示**：在`LostFocus`或`DropDownClosed`中强制用`SelectedDate`刷新`Text`，避免视觉与实际值不一致的问题