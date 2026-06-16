# 004022003_WPF Expander 基类体系官方类定义逐层解析

**源码：**

```c#
public class Expander : HeaderedContentControl
{
    public static readonly DependencyProperty ExpandDirectionProperty;
    public static readonly DependencyProperty IsExpandedProperty;
    public static readonly RoutedEvent ExpandedEvent;
    public static readonly RoutedEvent CollapsedEvent;
 
    public Expander();
 
    public ExpandDirection ExpandDirection { get; set; }
    public bool IsExpanded { get; set; }
 
    public event RoutedEventHandler Expanded;
    public event RoutedEventHandler Collapsed;
 
    public override void OnApplyTemplate();
    protected virtual void OnCollapsed();
    protected override AutomationPeer OnCreateAutomationPeer();
    protected virtual void OnExpanded();
 
}
```

这是 WPF 原生 `System.Windows.Controls.Expander` 控件的**核心类定义签名**，定义于 `PresentationFramework.dll`，继承自 `HeaderedContentControl`，是「标题 + 可折叠内容」结构的逻辑核心。整个类严格遵循 WPF「逻辑与外观分离」的设计原则：自身仅负责状态管理与事件通知，所有视觉渲染、动画、布局全部交由控件模板实现。

------

## 一、静态成员：依赖属性与路由事件

所有 `static readonly` 字段均为 WPF 底层系统的唯一标识符，是数据绑定、样式、动画、路由事件的基础设施。

### 1. 依赖属性标识符

#### ExpandDirectionProperty

csharp:

```c#
public static readonly DependencyProperty ExpandDirectionProperty;
```

- **对应 CLR 属性**：`ExpandDirection`
- **属性类型**：`ExpandDirection` 枚举
- **默认值**：`ExpandDirection.Down`
- **元数据特性**：支持绑定、样式继承、动画，值变更时自动触发视觉状态刷新
- **核心作用**：控制内容区域的展开方向，枚举值共 4 种：
  - `Down`：向下展开（默认，最常用，适用于页面垂直参数分组）
  - `Up`：向上展开（适用于底部悬浮面板场景）
  - `Left`：向左展开（适用于右侧属性栏、详情面板）
  - `Right`：向右展开（适用于左侧导航栏、功能菜单）
- **底层逻辑**：值变更时自动切换控件模板中的视觉状态，同步更新标题箭头方向与内容布局流向。

#### IsExpandedProperty

csharp:

```c#
public static readonly DependencyProperty IsExpandedProperty;
```

- **对应 CLR 属性**：`IsExpanded`
- **属性类型**：`bool`
- **默认值**：`false`（默认折叠状态）
- **元数据特性**：**默认双向绑定（BindsTwoWayByDefault）**，支持动画、样式触发器，值变更时触发完整的状态流转
- **核心作用**：控件的核心状态属性，控制内容区域的展开 / 折叠。MVVM 场景下无需显式指定 `Mode=TwoWay`，直接绑定即可双向同步。
- **完整状态链路**：属性值变更 → 触发属性变更回调 → 切换视觉状态（执行展开 / 折叠动画）→ 调用 `OnExpanded/OnCollapsed` 虚方法 → 冒泡触发路由事件。

### 2. 路由事件标识符

#### ExpandedEvent / CollapsedEvent

csharp:

```c#
public static readonly RoutedEvent ExpandedEvent;
public static readonly RoutedEvent CollapsedEvent;
```

- **路由策略**：**冒泡（Bubble）**，事件会从当前 Expander 向上遍历可视化树，直到窗口根容器
- **委托类型**：`RoutedEventHandler`
- **触发时机**：展开 / 折叠动画完成、状态稳定后触发（并非属性赋值瞬间触发）
- **设计价值**：支持父容器统一监听多个 Expander 的状态变化，无需为每个控件单独订阅事件，非常适合批量参数分组的工业配置页面。

------

## 二、构造函数

csharp:

```c#
public Expander();
```

构造函数内部完成三项核心初始化：

1. 设置 `DefaultStyleKey` 为 `typeof(Expander)`，自动关联系统主题对应的默认控件模板
2. 注册 `IsExpanded`、`ExpandDirection` 依赖属性的变更回调
3. 初始化交互配置：默认 `Focusable = false`，标题区域的点击与焦点由内部模板的 ToggleButton 承接

------

## 三、实例属性（CLR 包装器）

csharp:

```c#
public ExpandDirection ExpandDirection { get; set; }
public bool IsExpanded { get; set; }
```

这两个属性是**依赖属性的 CLR 语法糖包装**，本身不存储值：

- `get` 内部调用 `GetValue(依赖属性标识符)`，从 WPF 全局属性系统读取值
- `set` 内部调用 `SetValue(依赖属性标识符, value)`，写入属性系统并触发所有关联逻辑（绑定更新、样式刷新、动画、事件）

> 重要注意：禁止在 CLR 属性中添加自定义业务逻辑。WPF 属性系统（如样式、动画、绑定）会绕开 CLR 包装直接操作底层值，自定义逻辑应通过属性变更回调、重写虚方法或订阅事件实现。

------

## 四、事件（CLR 包装器）

csharp:

```c#
public event RoutedEventHandler Expanded;
public event RoutedEventHandler Collapsed;
```

这两个事件是**路由事件的 CLR 包装器**：

- `add` 内部调用 `AddHandler(ExpandedEvent, value)` 注册路由事件处理器
- `remove` 内部调用 `RemoveHandler(ExpandedEvent, value)` 移除处理器
- 工业场景典型用法：展开时异步加载对应分组的 PLC 实时数据，折叠时释放非必要的通信资源

------

## 五、核心方法详解

### 1. OnApplyTemplate()

csharp:

```c#
public override void OnApplyTemplate();
```

- **重写来源**：`FrameworkElement`
- **触发时机**：控件模板加载完成、所有模板子元素创建完毕后调用，是自定义控件的生命周期入口
- **Expander 内部实现**：
  1. 通过 `GetTemplateChild` 获取模板内的命名部件（如标题区的 ToggleButton、内容容器）
  2. 绑定标题按钮的点击状态与 `IsExpanded` 属性同步
  3. 初始化当前展开状态对应的视觉状态
- **扩展场景**：自定义 Expander 样式时，重写此方法获取自定义模板中的子元素，附加额外交互逻辑。

### 2. OnExpanded() / OnCollapsed()

csharp:

```c#
protected virtual void OnExpanded();
protected virtual void OnCollapsed();
```

- **访问级别**：受保护虚方法，是对应路由事件的**官方触发入口**

- **执行顺序**：属性变更 → 视觉状态切换完成 → 调用此方法 → 方法内部调用 `RaiseEvent` 触发冒泡路由事件

- **扩展最佳实践**：

  

  继承 Expander 实现自定义工业控件时，优先重写这两个方法插入业务逻辑，比订阅事件性能更好、封装性更强。

  

  典型场景：重写 `OnExpanded`实现「展开时才读取对应分组的 PLC 寄存器数据」，降低界面启动开销。

### 3. OnCreateAutomationPeer()

csharp:

```c#
protected override AutomationPeer OnCreateAutomationPeer();
```

- **重写来源**：`UIElement`
- **作用**：创建 `ExpanderAutomationPeer` 自动化对等类，为屏幕阅读器、UI 自动化测试、无障碍访问提供支持，对外暴露「展开 / 折叠」的控件状态与操作接口。

------

## 六、设计思想与补充说明

1. **极简核心逻辑**：Expander 自身仅定义 2 个依赖属性、2 个路由事件、4 个核心方法，所有外观、动画、布局全部在控件模板中实现，完全遵循 WPF 「逻辑与视觉分离」的架构原则。
2. **能力全部来自基类复用**：标题（`Header`）、内容（`Content`）、样式、背景边框、布局、数据绑定等所有能力，全部继承自 `HeaderedContentControl` → `ContentControl` → `Control` 等基类，自身不重复实现。
3. **工业场景最佳实践**：
   - MVVM 模式下直接绑定 `IsExpanded` 到 ViewModel，无需后台代码
   - 多分组参数配置页，通过父容器监听冒泡事件统一管理加载逻辑
   - 大数量配置项分组，使用 `OnExpanded` 懒加载数据，显著提升界面启动速度