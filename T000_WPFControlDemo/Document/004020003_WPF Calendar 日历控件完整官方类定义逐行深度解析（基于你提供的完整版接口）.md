# 004020003_WPF `Calendar` 日历控件完整官方类定义逐行深度解析（基于你提供的完整版接口）

**源码：**

```c#
public class Calendar : Control
{
    public static readonly RoutedEvent SelectedDatesChangedEvent;
    public static readonly DependencyProperty SelectionModeProperty;
    public static readonly DependencyProperty SelectedDateProperty;
    public static readonly DependencyProperty FirstDayOfWeekProperty;
    public static readonly DependencyProperty DisplayModeProperty;
    public static readonly DependencyProperty DisplayDateStartProperty;
    public static readonly DependencyProperty IsTodayHighlightedProperty;
    public static readonly DependencyProperty DisplayDateProperty;
    public static readonly DependencyProperty CalendarItemStyleProperty;
    public static readonly DependencyProperty CalendarDayButtonStyleProperty;
    public static readonly DependencyProperty CalendarButtonStyleProperty;
    public static readonly DependencyProperty DisplayDateEndProperty;
 
    public Calendar();
 
    public DateTime? DisplayDateStart { get; set; }
    public Style CalendarItemStyle { get; set; }
    public Style CalendarDayButtonStyle { get; set; }
    public Style CalendarButtonStyle { get; set; }
    public CalendarBlackoutDatesCollection BlackoutDates { get; }
    public CalendarMode DisplayMode { get; set; }
    public DateTime? DisplayDateEnd { get; set; }
    public bool IsTodayHighlighted { get; set; }
    public DateTime? SelectedDate { get; set; }
    public SelectedDatesCollection SelectedDates { get; }
    public CalendarSelectionMode SelectionMode { get; set; }
    public DateTime DisplayDate { get; set; }
    public DayOfWeek FirstDayOfWeek { get; set; }
 
    public event EventHandler<SelectionChangedEventArgs> SelectedDatesChanged;
    public event EventHandler<CalendarDateChangedEventArgs> DisplayDateChanged;
    public event EventHandler<EventArgs> SelectionModeChanged;
    public event EventHandler<CalendarModeChangedEventArgs> DisplayModeChanged;
 
    public override void OnApplyTemplate();
    public override string ToString();
    protected override AutomationPeer OnCreateAutomationPeer();
    protected virtual void OnDisplayDateChanged(CalendarDateChangedEventArgs e);
    protected virtual void OnDisplayModeChanged(CalendarModeChangedEventArgs e);
    protected override void OnKeyDown(KeyEventArgs e);
    protected override void OnKeyUp(KeyEventArgs e);
    protected virtual void OnSelectedDatesChanged(SelectionChangedEventArgs e);
    protected virtual void OnSelectionModeChanged(EventArgs e);
 
}
```

你提供的是`Calendar`控件**完整的公开接口定义**，补充了之前未覆盖的**三大样式属性、键盘处理、更多状态变更事件**，这是自定义工业日历的核心扩展点。本文严格对应这份代码，100% 覆盖所有成员，结合 .NET 8 官方源码实现进行深度解析，延续之前的工业自动化场景视角。

------

## 一、类定义总览与核心元数据

### 1.1 类签名与隐含特性

csharp:

```c#
public class Calendar : Control
```

官方完整类上还标注了三个关键特性（代码中未显式写出，但属于官方强制契约）：

csharp:

```c#
[Localizability(LocalizationCategory.None)]
[TemplatePart(Name = "PART_Root", Type = typeof(Panel))]
[TemplatePart(Name = "PART_CalendarItem", Type = typeof(CalendarItem))]
public class Calendar : Control
```

### 1.2 核心元数据总表

| 项           | 官方精确值                                                   | 工业场景关键说明                                             |
| :----------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| 命名空间     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                                         |
| 完整继承链   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → Calendar` | 直接继承自 `Control` 的复合控件，与 RangeBase 系列无继承关系 |
| 模板强制部件 | `PART_Root`（根面板）、`PART_CalendarItem`（核心日历项）     | 缺少则日历静默失效，无异常抛出                               |
| 核心扩展点   | 3 个样式属性 + 4 个状态变更事件 + 键盘重写方法               | 支持深度自定义外观与交互                                     |
| 自动化对等类 | `CalendarAutomationPeer`                                     | 支持 UI 自动化测试与无障碍访问                               |
| 本地化       | 自动适配系统区域文化                                         | 月份、星期名称随系统语言自动切换                             |

------

## 二、静态成员逐行解析

### 2.1 静态路由事件

csharp:

```c#
public static readonly RoutedEvent SelectedDatesChangedEvent;
```

- **官方定义**：这是 `Calendar` 唯一的**冒泡路由事件**，对应 `SelectedDatesChanged` 事件。
- **路由策略**：`RoutingStrategy.Bubble`（冒泡）
- **触发时机**：选中的日期集合发生任何变化时
- **工业价值**：可以在父容器（如窗口、页面）统一监听所有日历的选择事件，实现全局逻辑解耦。

> 注意：代码中另外三个事件（`DisplayDateChanged`/`SelectionModeChanged`/`DisplayModeChanged`）是普通 CLR 事件，不是路由事件，不会沿视觉树冒泡。

### 2.2 静态依赖属性（按官方定义顺序）

所有属性均通过 `DependencyProperty.Register` 注册，支持绑定、样式、动画等 WPF 核心特性。

csharp:

```c#
public static readonly DependencyProperty SelectionModeProperty;
public static readonly DependencyProperty SelectedDateProperty;
public static readonly DependencyProperty FirstDayOfWeekProperty;
public static readonly DependencyProperty DisplayModeProperty;
public static readonly DependencyProperty DisplayDateStartProperty;
public static readonly DependencyProperty IsTodayHighlightedProperty;
public static readonly DependencyProperty DisplayDateProperty;
public static readonly DependencyProperty CalendarItemStyleProperty;
public static readonly DependencyProperty CalendarDayButtonStyleProperty;
public static readonly DependencyProperty CalendarButtonStyleProperty;
public static readonly DependencyProperty DisplayDateEndProperty;
```

#### 核心基础属性（工业高频使用）

| 属性                         | 类型                    | 默认值           | 官方作用                           | 工业最佳实践                                                 |
| :--------------------------- | :---------------------- | :--------------- | :--------------------------------- | :----------------------------------------------------------- |
| `SelectionModeProperty`      | `CalendarSelectionMode` | `SingleDate`     | 控制日期选择模式                   | 优先用 `SingleDate`（单选）或 `SingleRange`（连续范围），避免 `MultipleRange` 增加操作复杂度 |
| `SelectedDateProperty`       | `DateTime?`             | `null`           | 当前选中的单个日期                 | MVVM 绑定首选，单选场景下比 `SelectedDates` 更简洁           |
| `FirstDayOfWeekProperty`     | `DayOfWeek`             | 系统区域默认值   | 定义一周的第一天                   | **工业强制设为 `Monday`**，符合生产排班、周统计的业务习惯    |
| `DisplayModeProperty`        | `CalendarMode`          | `Month`          | 控制日历显示视图（月 / 年 / 十年） | 工业场景几乎只用 `Month` 月视图，可锁定禁止切换到年 / 十年视图 |
| `DisplayDateStartProperty`   | `DateTime?`             | `null`（无下限） | 可显示 / 选择的最早日期            | 限制历史数据查询范围，如只能查近 1 年数据                    |
| `DisplayDateEndProperty`     | `DateTime?`             | `null`（无上限） | 可显示 / 选择的最晚日期            | 限制未来计划范围，如只能排未来 3 个月的生产计划              |
| `IsTodayHighlightedProperty` | `bool`                  | `true`           | 是否高亮标记今日                   | 保持开启，方便操作人员快速定位当前日期                       |
| `DisplayDateProperty`        | `DateTime`              | `DateTime.Now`   | 当前日历显示的基准日期             | 程序控制日历跳转的核心属性，如一键跳转到本月                 |

#### 三大样式属性（自定义外观核心）

这是工业定制日历最关键的三个属性，分别对应日历内部不同层级的部件样式：

| 属性                         | 对应控件            | 作用                                               | 工业典型应用                                       |
| :--------------------------- | :------------------ | :------------------------------------------------- | :------------------------------------------------- |
| **`CalendarItemStyle`**      | `CalendarItem`      | 控制整个日历主体（标题、导航按钮、视图容器）的样式 | 定制工业风格标题栏、修改导航按钮外观、调整整体边距 |
| **`CalendarDayButtonStyle`** | `CalendarDayButton` | 控制**月视图中每个日期按钮**的样式                 | 标记维护日、停机日、生产日、报警日等不同状态的日期 |
| **`CalendarButtonStyle`**    | `CalendarButton`    | 控制**年 / 十年视图中月份 / 年份按钮**的样式       | 定制年视图下的月份按钮外观，工业场景使用频率极低   |

> 🔑 工业开发核心：90% 的日历定制需求都可以通过 `CalendarDayButtonStyle` 实现，无需重写整个控件模板。

------

## 三、实例属性逐行解析

### 3.1 基础状态属性

csharp:

```c#
public DateTime? DisplayDateStart { get; set; }
public DateTime? DisplayDateEnd { get; set; }
public CalendarMode DisplayMode { get; set; }
public bool IsTodayHighlighted { get; set; }
public DateTime? SelectedDate { get; set; }
public SelectedDatesCollection SelectedDates { get; }
public CalendarSelectionMode SelectionMode { get; set; }
public DateTime DisplayDate { get; set; }
public DayOfWeek FirstDayOfWeek { get; set; }
```

- **`SelectedDates`**：只读的 `ObservableCollection<DateTime>`，只能通过 `Add/Remove/Clear` 修改集合，不能重新赋值。在范围选择模式下，通过该集合获取起止日期。
- **`BlackoutDates`**：只读的 `CalendarBlackoutDatesCollection`，用于添加不可选择的日期（周末、节假日、停机日），添加后对应日期按钮自动禁用。

### 3.2 三大样式属性

csharp:

```c#
public Style CalendarItemStyle { get; set; }
public Style CalendarDayButtonStyle { get; set; }
public Style CalendarButtonStyle { get; set; }
```

1. **`CalendarItemStyle`**
   - 应用对象：模板内部的 `PART_CalendarItem`
   - 可定制内容：标题栏字体、导航按钮样式、月份网格间距、边框背景等
   - 工业场景：修改为深色主题、增大按钮尺寸适配触摸屏
2. **`CalendarDayButtonStyle`**（最常用）
   - 应用对象：月视图中 42 个日期按钮（6 行 × 7 列）
   - 可定制内容：背景色、边框、字体、鼠标悬停效果、选中效果、禁用效果
   - 工业场景核心用法：通过数据触发器根据日期状态显示不同颜色
     - 绿色 = 正常生产日
     - 黄色 = 计划维护日
     - 红色 = 设备停机日
     - 蓝色 = 今日
3. **`CalendarButtonStyle`**
   - 应用对象：年视图的月份按钮、十年视图的年份按钮
   - 工业场景使用极少，一般保持默认即可。

------

## 四、事件逐行解析

csharp:

```c#
public event EventHandler<SelectionChangedEventArgs> SelectedDatesChanged;
public event EventHandler<CalendarDateChangedEventArgs> DisplayDateChanged;
public event EventHandler<EventArgs> SelectionModeChanged;
public event EventHandler<CalendarModeChangedEventArgs> DisplayModeChanged;
```

### 4.1 `SelectedDatesChanged`

- **触发时机**：选中的日期集合发生任何变化时（点击日期、范围选择、代码赋值）
- **事件参数**：`SelectionChangedEventArgs`，包含 `AddedItems`（新增选中的日期）和 `RemovedItems`（取消选中的日期）
- **工业核心应用**：
  - 单选场景：获取选中日期，查询对应生产数据、报警记录、批次信息
  - 范围场景：获取起止日期，生成区间报表、统计产量良率

### 4.2 `DisplayDateChanged`

- **触发时机**：当前显示的月份 / 年份发生变化时（点击导航按钮、代码修改 `DisplayDate`）
- **事件参数**：`CalendarDateChangedEventArgs`，包含 `AddedDate`（新显示日期）和 `RemovedDate`（旧显示日期）
- **工业核心应用**：
  - 预加载当前月份的生产计划、维护安排
  - 延迟加载当月的日期标记数据，避免一次性加载全年数据导致卡顿

### 4.3 `SelectionModeChanged`

- **触发时机**：`SelectionMode` 属性值发生变化时
- **事件参数**：普通 `EventArgs`，无额外数据
- **工业应用**：切换选择模式时更新 UI 提示，如从单选切换到范围选择时显示 "请选择起止日期" 提示

### 4.4 `DisplayModeChanged`

- **触发时机**：`DisplayMode` 属性值发生变化时（月 ↔ 年 ↔ 十年）
- **事件参数**：`CalendarModeChangedEventArgs`，包含新旧模式
- **工业应用**：锁定显示模式，禁止用户切换到年 / 十年视图，保持操作一致性

------

## 五、核心方法逐行解析

### 5.1 公共方法

#### 构造函数

csharp:

```c#
public Calendar();
```

- **官方内部实现**：
  1. 初始化所有依赖属性默认值
  2. 初始化 `BlackoutDates` 和 `SelectedDates` 集合并监听集合变更
  3. 绑定默认样式与控件模板
  4. 设置默认焦点行为与键盘导航模式

#### `OnApplyTemplate()`

csharp:

```c#
public override void OnApplyTemplate();
```

- **官方完整执行逻辑**：
  1. 调用基类方法
  2. 查找 `PART_Root` 根面板
  3. 查找 `PART_CalendarItem` 核心日历项
  4. 将 `CalendarItemStyle` 应用到 `PART_CalendarItem`
  5. 订阅日历项内部的日期点击、导航等事件
  6. 根据当前 `DisplayMode` 初始化视图
  7. 刷新所有日期的选中、禁用、高亮状态

#### `ToString()`

csharp:

```c#
public override string ToString();
```

- **官方实现**：输出当前选中日期的文本表示，格式跟随系统文化

  csharp:

  ```c#
  public override string ToString()
  {
      return SelectedDate.HasValue 
          ? SelectedDate.Value.ToString() 
          : base.ToString();
  }
  ```

- **作用**：调试时快速查看选中日期，便于排查问题。

### 5.2 受保护虚方法（官方扩展点）

#### `OnCreateAutomationPeer()`

csharp:

```c#
protected override AutomationPeer OnCreateAutomationPeer();
```

- **官方实现**：返回 `CalendarAutomationPeer` 实例，支持 UI 自动化测试与屏幕阅读器。

#### `OnSelectedDatesChanged(SelectionChangedEventArgs e)`

csharp:

```c#
protected virtual void OnSelectedDatesChanged(SelectionChangedEventArgs e);
```

- **触发时机**：选中日期变化时
- **官方默认实现**：触发 `SelectedDatesChanged` 路由事件
- **工业扩展**：重写此方法可添加自定义选择逻辑，例如：
  - 限制最多选择 7 天
  - 自动跳过禁用日期
  - 选择完成后自动执行查询

#### `OnDisplayDateChanged(CalendarDateChangedEventArgs e)`

csharp:

```c#
protected virtual void OnDisplayDateChanged(CalendarDateChangedEventArgs e);
```

- **触发时机**：显示月份变化时
- **官方默认实现**：触发 `DisplayDateChanged` 事件
- **工业扩展**：重写实现月份数据预加载、日期标记批量更新

#### `OnDisplayModeChanged(CalendarModeChangedEventArgs e)`

csharp:

```c#
protected virtual void OnDisplayModeChanged(CalendarModeChangedEventArgs e);
```

- **触发时机**：显示模式（月 / 年 / 十年）切换时
- **官方默认实现**：触发 `DisplayModeChanged` 事件，更新视图显示
- **工业扩展**：重写可锁定显示模式，禁止用户切换到年 / 十年视图

#### `OnSelectionModeChanged(EventArgs e)`

csharp:

```c#
protected virtual void OnSelectionModeChanged(EventArgs e);
```

- **触发时机**：选择模式变化时
- **官方默认实现**：触发 `SelectionModeChanged` 事件，清空已选日期
- **工业扩展**：切换模式时保留已有选择、或重置为默认日期范围

#### `OnKeyDown(KeyEventArgs e)` / `OnKeyUp(KeyEventArgs e)`

csharp:

```c#
protected override void OnKeyDown(KeyEventArgs e);
protected override void OnKeyUp(KeyEventArgs e);
```

- **官方内置键盘导航逻辑**（工业操作人员必备）：

  | 按键                                | 功能                |
  | :---------------------------------- | :------------------ |
  | 方向键 ↑↓←→                         | 按天移动选中日期    |
  | `PageUp` / `PageDown`               | 切换上一月 / 下一月 |
  | `Ctrl + PageUp` / `Ctrl + PageDown` | 切换上一年 / 下一年 |
  | `Home`                              | 跳转到当月第一天    |
  | `End`                               | 跳转到当月最后一天  |
  | `空格` / `回车`                     | 选中当前焦点日期    |

- **工业价值**：支持全键盘操作，适合无鼠标的工业操作终端，提升操作效率。

------

## 六、官方内部核心机制

### 6.1 样式应用层级

三个样式属性自上而下应用，形成完整的外观定制体系：

plaintext:

```tex
Calendar
└── CalendarItemStyle → 应用到 CalendarItem
    ├── CalendarButtonStyle → 应用到年/十年视图的按钮
    └── CalendarDayButtonStyle → 应用到月视图的日期按钮
```

### 6.2 日期选择完整流程

plaintext:

```tex
用户点击日期按钮 / 按下键盘
    ↓
CalendarDayButton 触发点击事件
    ↓
CalendarItem 接收事件，根据 SelectionMode 更新 SelectedDates 集合
    ↓
集合触发 CollectionChanged 通知
    ↓
Calendar 同步更新 SelectedDate 属性
    ↓
调用 OnSelectedDatesChanged 方法
    ↓
触发 SelectedDatesChanged 路由事件
    ↓
刷新所有日期按钮的选中状态
    ↓
UI 重新渲染
```

### 6.3 禁用日期（BlackoutDates）实现原理

1. 向 `BlackoutDates` 添加日期范围时，触发集合变更通知
2. `Calendar` 内部通知所有 `CalendarDayButton` 刷新状态
3. 每个日期按钮渲染前检查自身日期是否在禁用集合中
4. 若命中则设置 `IsEnabled = false`，并应用禁用样式
5. 禁用的按钮无法通过鼠标或键盘选中

------

## 七、工业开发核心启示与最佳实践

1. **优先通过 `CalendarDayButtonStyle` 定制日期外观**：不需要重写整个模板，通过数据触发器即可实现多状态日期标记，开发成本低、维护性好。
2. **合理限制日期范围**：使用 `DisplayDateStart` / `DisplayDateEnd` 缩小可操作区间，既提升性能也避免操作人员误选。
3. **善用 `DisplayDateChanged` 做延迟加载**：只加载当前月份的业务数据，不要一次性加载全年数据，大幅降低数据库压力。
4. **保留键盘操作支持**：工业终端很多是纯键盘操作，不要重写键盘逻辑破坏默认导航。
5. **统一使用 `FirstDayOfWeek.Monday`**：符合国内生产排班、周统计的业务惯例，避免周维度数据统计出错。
6. **范围选择优先用 `SingleRange`**：`MultipleRange` 操作逻辑复杂且易出错，工业场景几乎没有刚需。
7. **禁用日期使用 `BlackoutDates`**：不要自己通过样式禁用，官方机制会自动处理键盘导航、范围选择时的跳过逻辑。