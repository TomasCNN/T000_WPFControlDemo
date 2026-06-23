# 005010005_WPF ContextMenu 官方类定义逐成员完整深度解析

以下解析基于 .NET WPF 官方源码与框架设计规范，从**字段本质、底层实现、设计意图、使用场景、常见坑点**五个维度，完整拆解 `System.Windows.Controls.ContextMenu` 类的每一个成员。

------

## 一、类声明与整体架构定位

csharp:

```c#
public class ContextMenu : MenuBase
```

### 1. 完整继承链与能力分层

`ContextMenu` 的完整继承链为：

```tex
ContextMenu → MenuBase → ItemsControl → Control → FrameworkElement → UIElement → Visual → DependencyObject
```

每一层基类为其注入的核心能力：

| 基类               | 核心能力                                                     |
| :----------------- | :----------------------------------------------------------- |
| `DependencyObject` | 依赖属性系统支持，是数据绑定、样式、动画、属性值继承的底层基础 |
| `Visual`           | 视觉渲染、命中测试、视觉树管理                               |
| `UIElement`        | 路由事件、输入处理、布局参与、焦点管理                       |
| `FrameworkElement` | 样式、数据上下文、资源查找、布局对齐、尺寸计算               |
| `Control`          | 控件模板、字体 / 背景 / 边框等通用外观属性、交互状态         |
| `ItemsControl`     | 项集合管理、数据绑定、项容器生成、UI 虚拟化                  |
| `MenuBase`         | 菜单通用逻辑：菜单项导航、命令路由、子菜单展开收起、键盘交互规范 |
| `ContextMenu`      | 专属扩展：悬浮弹出、右键触发、自动关闭、多模式定位           |

### 2. 核心设计职责

它是 WPF 右键上下文菜单的标准实现，本质是 **「菜单项集合 + Popup 弹出窗口」的外观封装 **：

- 对外暴露简洁的菜单操作接口，对内封装底层 `Popup` 窗口的创建、定位、显隐全部复杂度；
- 严格遵循 Windows 系统交互规范，支持键盘导航、无障碍访问、系统主题适配；
- 完全兼容 WPF 声明式编程模型，支持 XAML 配置、数据绑定、MVVM 模式。

------

## 二、静态字段：依赖属性与路由事件标识符

代码开头的 11 个 `static readonly` 字段，是 WPF 控件最核心的设计特征 ——**所有属性与事件的元数据在类型层面全局注册，所有实例共享唯一标识**，而非每个实例单独存储。

### 2.1 依赖属性标识符（共 9 个）

每个字段对应一个公开属性，在类型静态构造函数中通过 `DependencyProperty.Register()` 注册，包含默认值、变更回调、强制值约束等元数据。

#### 1. `HorizontalOffsetProperty`

csharp:

```c#
public static readonly DependencyProperty HorizontalOffsetProperty;
```

- **底层本质**：对应内部 `Popup` 控件的 `HorizontalOffset` 属性，菜单弹出时的水平偏移量。
- **默认值**：`0.0`（与设备无关的像素单位，1/96 英寸）。
- **设计意图**：配合定位模式微调菜单横向位置，正数向右偏移，负数向左偏移。
- **变更逻辑**：属性值变化时，通过回调通知内部 Popup 实时更新弹出位置，无需关闭重开。

#### 2. `StaysOpenProperty`

csharp:

```c#
public static readonly DependencyProperty StaysOpenProperty;
```

- **底层本质**：透传给内部 Popup 的 `StaysOpen` 属性，控制菜单的自动关闭逻辑。
- **默认值**：`false`。
- **核心逻辑**：
  - `false`（默认）：点击菜单外区域、按下 ESC 键、主窗口失去焦点时，自动将 `IsOpen` 置为 `false` 并关闭菜单；
  - `true`：菜单常驻显示，仅能通过代码修改 `IsOpen` 手动关闭。
- **使用场景**：默认值适配绝大多数右键场景；设为 `true` 适合需要常驻的工具型菜单。

#### 3. `CustomPopupPlacementCallbackProperty`

csharp:

```c#
public static readonly DependencyProperty CustomPopupPlacementCallbackProperty;
```

- **底层本质**：自定义定位计算的委托入口，仅当 `Placement = Custom` 时生效。
- **委托签名**：`CustomPopupPlacement[] CustomPopupPlacementCallback(Size popupSize, Size targetSize, Point offset)`
- **设计意图**：内置定位模式无法满足需求时，允许开发者完全掌控弹出坐标。
- **边界避让机制**：返回值为候选位置数组，WPF 按数组顺序依次尝试，自动选择第一个能完整显示在屏幕工作区内的位置，无需手动处理屏幕边界。

#### 4. `HasDropShadowProperty`

csharp:

```c#
public static readonly DependencyProperty HasDropShadowProperty;
```

- **底层本质**：控制弹出窗口是否启用系统投影效果，透传给 Popup 的窗口样式。
- **默认值**：由系统主题决定，经典主题下默认 `true`。
- **注意事项**：投影效果依赖 Popup 的分层窗口特性（`AllowsTransparency=true`）；若系统禁用窗口动画、或运行在远程桌面 / 安全模式下，该属性设置可能不生效。

#### 5. `PlacementRectangleProperty`

csharp:

```c#
public static readonly DependencyProperty PlacementRectangleProperty;
```

- **底层本质**：在 `PlacementTarget` 的坐标系内，指定一个子矩形作为定位基准。
- **默认值**：`Rect.Empty`（默认使用整个目标元素的边界作为基准）。
- **使用场景**：需要在控件的局部区域弹出菜单时使用，例如仅在按钮的图标区域右键弹出菜单，而非整个按钮范围。

#### 6. `PlacementTargetProperty`

csharp:

```c#
public static readonly DependencyProperty PlacementTargetProperty;
```

- **底层本质**：菜单定位的基准宿主元素，是所有弹出位置计算的参考原点。
- **默认值**：`null`。
- **自动赋值机制**：通过控件的 `ContextMenu` 属性（如 `Button.ContextMenu`）挂载菜单时，WPF 的 `ContextMenuService` 会自动将该属性设置为宿主控件，无需手动赋值。
- **高频坑点**：代码手动实例化 `ContextMenu` 并弹出时，**必须手动指定该属性**，否则菜单会默认定位到屏幕左上角 (0,0)。

#### 7. `IsOpenProperty`

csharp:

```c#
public static readonly DependencyProperty IsOpenProperty;
```

- **底层本质**：菜单显隐的核心状态属性，直接控制内部 Popup 的打开与关闭。
- **默认值**：`false`。
- **变更逻辑**：
  - 置为 `true`：内部创建 / 复用 Popup 窗口，同步所有定位属性，触发弹出动画，动画完成后引发 `Opened` 事件；
  - 置为 `false`：触发关闭动画，动画完成后隐藏 Popup，引发 `Closed` 事件。
- **MVVM 核心**：支持双向绑定，是 ViewModel 代码控制菜单显隐的唯一标准入口。

#### 8. `VerticalOffsetProperty`

csharp:

```c#
public static readonly DependencyProperty VerticalOffsetProperty;
```

- **底层本质**：对应内部 Popup 的 `VerticalOffset` 属性，菜单弹出时的垂直偏移量。
- **默认值**：`0.0`。
- **行为说明**：正数向下偏移，负数向上偏移，与 `HorizontalOffset` 配合完成位置微调。

#### 9. `PlacementProperty`

csharp:

```c#
public static readonly DependencyProperty PlacementProperty;
```

- **底层本质**：指定菜单的定位基准模式，类型为 `PlacementMode` 枚举，透传给内部 Popup。

- **默认值**：`PlacementMode.MousePoint`（鼠标热点位置为原点弹出）。

- **常用枚举值与行为**：

  | 枚举值       | 定位逻辑                                               |
  | :----------- | :----------------------------------------------------- |
  | `MousePoint` | 以鼠标当前坐标为菜单左上角原点，右键场景默认行为       |
  | `Bottom`     | 菜单顶部对齐目标元素底部，类似下拉按钮                 |
  | `Center`     | 菜单在目标元素正中心居中显示                           |
  | `Relative`   | 相对于目标元素左上角，偏移量完全由两个 Offset 属性控制 |
  | `Custom`     | 完全由 `CustomPopupPlacementCallback` 计算位置         |

### 2.2 路由事件标识符（共 2 个）

两个字段均为 `RoutedEvent` 类型，在静态构造中通过 `EventManager.RegisterRoutedEvent()` 注册，采用冒泡路由策略。

#### 1. `OpenedEvent`

csharp:

```c#
public static readonly RoutedEvent OpenedEvent;
```

- **触发时机**：菜单完全展开、弹出动画结束、视觉树挂载完成后触发。
- **设计意图**：提供菜单打开完成后的生命周期通知，此时 `IsOpen` 已为 `true`，UI 元素可正常访问。

#### 2. `ClosedEvent`

csharp:

```c#
public static readonly RoutedEvent ClosedEvent;
```

- **触发时机**：菜单完全收起、关闭动画结束、视觉树卸载完成后触发。
- **设计意图**：提供菜单关闭完成后的生命周期通知，适合执行资源清理、状态同步、数据保存等逻辑。

------

## 三、构造函数

csharp:

```c#
public ContextMenu();
```

### 1. 静态构造（隐式执行）

类型首次加载时执行，完成所有核心注册工作：

1. 注册全部 9 个依赖属性，设置默认值与属性变更回调；
2. 注册 2 个路由事件，指定路由策略与事件参数类型；
3. 重写 `DefaultStyleKeyProperty` 元数据，关联系统默认控件样式。

### 2. 实例构造

创建实例时执行初始化：

1. 初始化内部私有 `Popup` 字段，配置 Popup 的基础行为；
2. 设置默认交互属性，如 `Focusable = true`；
3. 注册内部事件监听，如 Popup 的打开关闭状态同步。

------

## 四、实例属性：CLR 包装器与状态控制

所有公开属性均为对应依赖属性的**强类型语法糖**，内部通过 `GetValue()` / `SetValue()` 操作依赖属性系统，本身不包含业务逻辑。

> ⚠️ 重要设计原则：WPF 的绑定、动画、样式系统会直接绕过 CLR 属性，通过 `DependencyProperty` 标识符直接操作值。**绝对不要在属性的 get/set 中添加自定义业务逻辑**，否则会出现 “代码赋值生效、绑定赋值不生效” 的疑难 bug。

### 1. 公开读写属性

| 属性名                         | 类型                           | 核心作用                               |
| :----------------------------- | :----------------------------- | :------------------------------------- |
| `HorizontalOffset`             | `double`                       | 水平弹出偏移量，单位：与设备无关的像素 |
| `VerticalOffset`               | `double`                       | 垂直弹出偏移量                         |
| `StaysOpen`                    | `bool`                         | 是否禁用自动关闭，保持菜单常驻         |
| `CustomPopupPlacementCallback` | `CustomPopupPlacementCallback` | 自定义定位计算委托                     |
| `HasDropShadow`                | `bool`                         | 弹出窗口是否显示投影效果               |
| `Placement`                    | `PlacementMode`                | 弹出定位模式                           |
| `PlacementRectangle`           | `Rect`                         | 目标元素内的局部定位矩形               |
| `PlacementTarget`              | `UIElement`                    | 定位基准宿主元素                       |
| `IsOpen`                       | `bool`                         | 菜单打开 / 关闭状态，支持双向绑定      |

### 2. 受保护内部属性

csharp:

```c#
protected internal override bool HandlesScrolling { get; }
```

- **访问级别**：`protected internal` 表示仅本程序集和子类可访问，是 WPF 内部滚动体系的约定属性。
- **返回值**：固定返回 `true`。
- **设计含义**：表示 `ContextMenu` 内部已自带 `ScrollViewer` 滚动容器，菜单项溢出时自行处理滚动，不需要父级控件额外提供滚动支持。

------

## 五、事件：生命周期通知

csharp:

```c#
public event RoutedEventHandler Closed;
public event RoutedEventHandler Opened;
```

这两个不是普通 C# 委托事件，而是**路由事件的 CLR 包装器**，底层通过 `AddHandler` / `RemoveHandler` 操作 WPF 路由事件系统。

### 核心特性

1. **冒泡路由**：事件可以沿元素树向上传播，允许在父容器（如窗口、页面）统一处理所有子级菜单的打开 / 关闭，无需逐个订阅。
2. **时机准确**：事件在动画完成、状态稳定后触发，避免在动画过程中访问 UI 导致异常。
3. **标准规范**：遵循 .NET 事件设计模式，对应受保护的 `OnOpened` / `OnClosed` 触发方法。

------

## 六、受保护方法：模板方法扩展点

这 8 个方法是 WPF 控件**模板方法设计模式**的核心体现：基类定义完整的主流程骨架，子类通过重写这些方法注入自定义逻辑，无需修改基类代码，保证控件行为一致性。

### 1. `OnOpened`

csharp:

```c#
protected virtual void OnOpened(RoutedEventArgs e);
```

- **调用时机**：菜单完全打开后，引发 `Opened` 事件前。
- **默认实现**：调用 `RaiseEvent(e)` 触发 `Opened` 路由事件。
- **重写场景**：子类注入打开逻辑，如播放自定义动画、初始化焦点到指定菜单项、埋点统计、动态加载菜单项数据。
- **注意**：重写时必须调用 `base.OnOpened(e)`，否则外部订阅的 `Opened` 事件不会触发。

### 2. `OnClosed`

csharp:

```c#
protected virtual void OnClosed(RoutedEventArgs e);
```

- **调用时机**：菜单完全关闭后，引发 `Closed` 事件前。
- **默认实现**：调用 `RaiseEvent(e)` 触发 `Closed` 路由事件。
- **重写场景**：执行清理逻辑，如释放资源、取消异步任务、保存菜单状态、清理临时数据。

### 3. `OnCreateAutomationPeer`

csharp:

```c#
protected override AutomationPeer OnCreateAutomationPeer();
```

- **重写来源**：`UIElement`。
- **默认实现**：返回 `ContextMenuAutomationPeer` 实例。
- **核心作用**：支持 Windows UI 自动化规范，向屏幕阅读器、自动化测试工具暴露控件类型、状态、可操作菜单项等信息，是无障碍访问的基础。
- **重写场景**：自定义 ContextMenu 新增交互能力时，扩展自动化对等类以暴露更多可访问信息。

### 4. `OnIsKeyboardFocusWithinChanged`

csharp:

```c#
protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e);
```

- **重写来源**：`UIElement`。
- **触发时机**：`IsKeyboardFocusWithin` 属性变化时，即键盘焦点进入 / 离开菜单内部。
- **内置逻辑**：
  - 焦点进入时：确保焦点在菜单项间正确导航，记忆上一次选中项；
  - 焦点离开时：配合 `StaysOpen` 逻辑判断是否自动关闭菜单，关闭时归还焦点到宿主元素。
- **重写场景**：自定义焦点管理，如打开菜单时自动聚焦到第一个可用项、自定义焦点循环逻辑。

### 5. `OnKeyDown`

csharp:

```c#
protected override void OnKeyDown(KeyEventArgs e);
```

- **重写来源**：`UIElement`。
- **触发时机**：键盘按键按下时。
- **内置交互逻辑**：
  - `ESC`：关闭菜单；
  - 上 / 下方向键：在同级菜单项间移动焦点；
  - 左 / 右方向键：收起 / 展开子菜单；
  - 回车 / 空格：执行当前选中菜单项的命令。
- **重写场景**：扩展自定义快捷键，修改默认键盘导航规则。

### 6. `OnKeyUp`

csharp:

```c#
protected override void OnKeyUp(KeyEventArgs e);
```

- **重写来源**：`UIElement`。
- **触发时机**：键盘按键抬起时。
- **作用**：配合 `OnKeyDown` 完成完整按键交互，处理按键释放后的状态同步，避免按键重复触发问题。

### 7. `PrepareContainerForItemOverride`

csharp:

```c#
protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
```

- **重写来源**：`ItemsControl`。
- **触发时机**：每个数据项生成对应 `MenuItem` 容器后调用。
- **内置逻辑**：将数据项与 `MenuItem` 绑定，应用样式、数据模板，同步命令、图标等属性。
- **重写场景**：批量自定义菜单项，如统一添加图标前缀、设置命令参数、根据数据权限控制菜单项可用性、批量注册事件。

### 8. `OnVisualParentChanged`

csharp:

```c#
protected internal override void OnVisualParentChanged(DependencyObject oldParent);
```

- **重写来源**：`Visual`，访问级别为 `protected internal`。
- **触发时机**：菜单的视觉父级发生变化时 —— 弹出时从主视觉树挂载到 Popup 独立视觉树，关闭时从 Popup 视觉树卸载。
- **内置逻辑**：同步弹出状态、更新资源引用、处理事件订阅与取消订阅，避免视觉树切换导致的内存泄漏。
- **重写场景**：自定义资源生命周期管理，如挂载时加载动态资源，卸载时释放大型对象。

------

## 七、整体设计思想总结

1. **职责分离**：菜单内容与交互由 `ContextMenu` 负责，窗口弹出与层级管理委托给内部 `Popup`，符合单一职责原则。
2. **模板方法模式**：通过大量受保护虚方法定义扩展点，基类控制主流程一致性，子类仅扩展局部逻辑。
3. **声明式优先**：全部核心状态均为依赖属性，完美适配 WPF 数据绑定、样式、动画的声明式编程模型。
4. **系统级兼容**：严格遵循 Windows 交互规范、无障碍标准、系统主题适配，保证用户体验一致性。
5. **视觉树隔离设计**：Popup 独立窗口机制保证菜单不受父容器裁剪限制，但也带来了绑定失效的经典坑点，是使用时最需要注意的底层特性。