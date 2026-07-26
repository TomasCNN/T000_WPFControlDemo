# 005010003_WPF `ContextMenu` 上下文菜单官方类定义深度解析

**源码：**

```c#
public class ContextMenu : MenuBase
{
    public static readonly DependencyProperty HorizontalOffsetProperty;
    public static readonly RoutedEvent OpenedEvent;
    public static readonly DependencyProperty StaysOpenProperty;
    public static readonly DependencyProperty CustomPopupPlacementCallbackProperty;
    public static readonly DependencyProperty HasDropShadowProperty;
    public static readonly RoutedEvent ClosedEvent;
    public static readonly DependencyProperty PlacementRectangleProperty;
    public static readonly DependencyProperty PlacementTargetProperty;
    public static readonly DependencyProperty IsOpenProperty;
    public static readonly DependencyProperty VerticalOffsetProperty;
    public static readonly DependencyProperty PlacementProperty;
 
    public ContextMenu();
 
    public double HorizontalOffset { get; set; }
    public bool StaysOpen { get; set; }
    public CustomPopupPlacementCallback CustomPopupPlacementCallback { get; set; }
    public bool HasDropShadow { get; set; }
    public PlacementMode Placement { get; set; }
    public Rect PlacementRectangle { get; set; }
    public UIElement PlacementTarget { get; set; }
    public bool IsOpen { get; set; }
    public double VerticalOffset { get; set; }
    protected internal override bool HandlesScrolling { get; }
 
    public event RoutedEventHandler Closed;
    public event RoutedEventHandler Opened;
 
    protected virtual void OnClosed(RoutedEventArgs e);
    protected override AutomationPeer OnCreateAutomationPeer();
    protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e);
    protected override void OnKeyDown(KeyEventArgs e);
    protected override void OnKeyUp(KeyEventArgs e);
    protected virtual void OnOpened(RoutedEventArgs e);
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
    protected internal override void OnVisualParentChanged(DependencyObject oldParent);
 
}
```

这段代码是 **WPF（Windows Presentation Foundation）框架中 `System.Windows.Controls.ContextMenu` 控件的公开接口元数据定义**，只展示了类的成员签名，不包含方法内部实现。它完整体现了 WPF 控件的标准设计范式、继承体系与扩展机制，下面从**整体定位、成员分类、设计模式、底层逻辑**四个维度做深度分析。

------

## 一、类的整体定位与继承体系

csharp:

```c#
public class ContextMenu : MenuBase
```

### 1. 继承链与能力来源

`ContextMenu` 的完整继承链为：

```tex
ContextMenu → MenuBase → Control → FrameworkElement → UIElement → Visual → DependencyObject
```

每一层都为它注入了核心能力：

- `DependencyObject`：支持依赖属性系统，是 WPF 数据绑定、样式、动画的基础
- `UIElement`：支持输入事件、路由事件、命中测试、布局参与
- `FrameworkElement`：支持样式、数据上下文、资源查找、布局对齐
- `Control`：支持控件模板、字体、背景、边框等通用控件属性
- `MenuBase`：封装所有菜单的通用逻辑 —— 菜单项容器生成、命令路由、键盘导航、项集合管理
- `ContextMenu`：在菜单基础上扩展 **「悬浮弹出、右键触发、自动关闭」** 的专属交互特性

### 2. 设计职责

它是 WPF 中右键上下文菜单的标准实现，核心职责是：

- 以悬浮窗口形式展示菜单项集合
- 提供丰富的弹出定位能力
- 处理鼠标 / 键盘的菜单交互逻辑
- 封装底层 `Popup` 窗口的复杂度，对外暴露简洁的调用接口

------

## 二、静态字段：WPF 核心系统的标识符

代码开头的 9 个 `DependencyProperty` 和 2 个 `RoutedEvent` 静态只读字段，是 WPF 控件最标志性的设计。

csharp:

```c#
public static readonly DependencyProperty HorizontalOffsetProperty;
public static readonly DependencyProperty StaysOpenProperty;
// ... 其余依赖属性字段
public static readonly RoutedEvent OpenedEvent;
public static readonly RoutedEvent ClosedEvent;
```

### 1. 为什么必须是 `static readonly`？

- **类型级共享**：依赖属性、路由事件的元数据（默认值、回调、路由策略等）属于**类**而非实例，所有 `ContextMenu` 对象共用同一个标识，大幅节省内存。
- **全局唯一标识**：WPF 的属性系统、事件系统是全局维护的，静态字段作为唯一 Key，用于注册、查找、计算属性有效值和事件路由。
- **不可修改**：`readonly` 保证注册后标识符不会被篡改，确保系统稳定性。

### 2. 字段分类与设计意图

| 分类           | 包含字段                                                     | 核心作用                                                     |
| :------------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| 弹出定位组     | `PlacementProperty`、`PlacementTargetProperty`、`PlacementRectangleProperty`、`HorizontalOffsetProperty`、`VerticalOffsetProperty`、`CustomPopupPlacementCallbackProperty` | 控制菜单弹出的位置基准、偏移量与自定义定位逻辑，全部透传给内部的 `Popup` 控件实现 |
| 状态控制组     | `IsOpenProperty`、`StaysOpenProperty`                        | 控制菜单的打开 / 关闭状态，以及是否保持常开不自动关闭        |
| 视觉效果组     | `HasDropShadowProperty`                                      | 控制弹出窗口是否带投影效果                                   |
| 生命周期事件组 | `OpenedEvent`、`ClosedEvent`                                 | 菜单打开 / 关闭的通知事件，采用路由事件模型支持元素树传播    |

------

## 三、构造函数与属性包装器

### 1. 构造函数

csharp:

```c#
public ContextMenu();
```

- 无参公共构造是 WPF 控件的硬性要求，用于 XAML 解析器的实例化。
- 内部实际执行：设置默认控件样式、创建内部 `Popup` 实例、注册属性变更回调、初始化默认交互逻辑、设置属性默认值（如 `Placement=MousePoint`、`StaysOpen=false`）。

### 2. CLR 属性包装器

所有公开属性都是对应依赖属性的**强类型语法糖**，内部通过 `GetValue` / `SetValue` 操作依赖属性系统：

csharp:

```c#
public double HorizontalOffset { get; set; }
public bool IsOpen { get; set; }
// ... 其余属性
```

#### 设计要点：

1. **仅做包装，不写业务逻辑**：WPF 系统（绑定、动画、样式）会直接绕过 CLR 属性，通过 `DependencyProperty` 操作值。如果在 `get/set` 里加自定义逻辑，会出现 “代码赋值生效、绑定赋值不生效” 的诡异 bug。
2. **强类型便捷访问**：给开发者提供编译时类型检查，避免直接操作 `GetValue` 的装箱拆箱开销与类型错误。
3. **XAML 友好**：XAML 解析器通过属性名匹配对应依赖属性，实现声明式赋值。

### 3. 特殊的受保护属性

csharp:

```c#
protected internal override bool HandlesScrolling { get; }
```

- `protected internal`：仅本程序集和子类可访问，是 WPF 内部滚动体系的约定属性。
- 返回 `true`，表示 `ContextMenu` 内部自带滚动容器，自行处理菜单项溢出滚动，不需要父级 `ScrollViewer` 为它提供滚动支持。

------

## 四、事件包装器：路由事件的对外入口

csharp:

```c#
public event RoutedEventHandler Closed;
public event RoutedEventHandler Opened;
```

这两个事件不是普通 C# 委托事件，而是**路由事件的包装器**，底层通过 `AddHandler` / `RemoveHandler` 操作 WPF 路由事件系统。

### 设计优势：

- **支持路由传播**：事件可以在元素树中冒泡 / 隧道，允许在父容器（如窗口、页面）统一处理所有子菜单的打开关闭，不用逐个订阅。
- **与 WPF 事件体系统一**：和鼠标、键盘事件共用一套路由机制，符合框架整体设计。
- **对应引发方法**：下方的 `OnOpened` / `OnClosed` 是事件的触发入口，遵循.NET 事件设计规范。

------

## 五、受保护虚方法：模板方法模式的扩展点

这 8 个 `protected virtual/override` 方法，是 WPF 控件的核心扩展机制，完美体现**模板方法设计模式**：基类定义整体流程骨架，子类通过重写步骤方法扩展功能，无需修改基类逻辑。

### 1. 生命周期扩展点

csharp:

```c#
protected virtual void OnOpened(RoutedEventArgs e);
protected virtual void OnClosed(RoutedEventArgs e);
```

- 分别在菜单完全打开、完全关闭时调用，默认实现是触发对应路由事件。
- 子类重写可以在不订阅事件的情况下注入自定义逻辑（如播放动画、埋点、状态清理），优先级高于外部事件订阅。

### 2. 无障碍与自动化扩展点

csharp:

```c#
protected override AutomationPeer OnCreateAutomationPeer();
```

- 创建 UI 自动化对等类，向屏幕阅读器、自动化测试工具暴露控件的类型、状态、可操作项。
- 是 Windows 无障碍规范的要求，所有标准 WPF 控件都必须实现。

### 3. 焦点与输入扩展点

csharp:

```c#
protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e);
protected override void OnKeyDown(KeyEventArgs e);
protected override void OnKeyUp(KeyEventArgs e);
```

- `OnIsKeyboardFocusWithinChanged`：菜单内部焦点变化时触发，负责打开菜单时移入焦点、关闭时归还焦点给宿主。
- `OnKeyDown/OnKeyUp`：实现标准菜单键盘交互 —— 上下方向键切换菜单项、ESC 关闭菜单、回车 / 空格触发命令，保证控件符合 Windows 交互规范。

### 4. 项容器生成扩展点

csharp:

```c#
protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
```

- 继承自 `ItemsControl`，每生成一个菜单项容器（`MenuItem`）时调用一次。
- 用于初始化菜单项：应用样式、绑定命令、设置数据上下文、关联图标等，是实现 “数据驱动菜单” 的核心扩展点。

### 5. 视觉树变更扩展点

csharp:

```c#
protected internal override void OnVisualParentChanged(DependencyObject oldParent);
```

- 菜单弹出 / 关闭时，会在主视觉树和 Popup 独立视觉树之间挂载 / 卸载，触发此方法。
- 内部用于同步弹出状态、清理事件订阅、释放资源，是避免内存泄漏的关键钩子。

------

## 六、整体设计模式与架构思想

1. **模板方法模式**：通过大量受保护虚方法定义扩展点，基类控制主流程，子类只扩展局部逻辑，保证控件行为一致性。
2. **外观模式**：封装了内部 `Popup` 窗口、菜单项管理、键盘导航等复杂逻辑，对外只暴露简洁的属性和事件，降低使用门槛。
3. **依赖属性系统**：用类型级属性存储替代实例字段，天然支持数据绑定、样式、动画、属性值继承，是 WPF 声明式编程的基石。
4. **路由事件系统**：事件沿元素树传播，解耦事件源和处理者，适合复杂 UI 层级下的事件处理。
5. **单一职责原则**：`MenuBase` 负责菜单通用逻辑，`ContextMenu` 只负责弹出交互，`Popup` 只负责窗口悬浮，层级清晰、复用性强。

------

## 总结

这段代码虽然只有签名，但完整勾勒出了 WPF 标准控件的设计全貌：**以依赖属性和路由事件为核心，通过继承体系复用通用能力，通过虚方法提供扩展点，对外提供一致、简洁的编程模型**。理解这套设计范式，不仅能正确使用 `ContextMenu`，也能举一反三掌握所有 WPF 控件的开发与自定义方法。