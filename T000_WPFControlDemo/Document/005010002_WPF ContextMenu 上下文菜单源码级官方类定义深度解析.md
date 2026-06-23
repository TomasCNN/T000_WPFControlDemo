# 005010002_WPF `ContextMenu` 上下文菜单源码级官方类定义深度解析

`ContextMenu` 是 WPF 右键弹出式菜单的核心控件，与 `Menu` 平级，共同继承自 `MenuBase` 抽象基类，完整复用 `MenuItem` 菜单项体系，所有主菜单支持的图标、复选、命令、多级子菜单能力，在右键菜单中 100% 兼容。两者的本质差异在于承载形式：`ContextMenu` 寄宿在**独立 `Popup` 悬浮窗口**中，不在主界面视觉树内，右键点击目标控件时在鼠标位置弹出，是工业软件快捷操作的核心交互载体。

其最核心的底层特性是**视觉树隔离**—— 这也是 MVVM 绑定时最容易踩坑的根源，所有数据上下文的传递都必须通过 `PlacementTarget` 作为桥梁。

------

## 一、官方类定义总览与核心元数据

### 1.1 完整类签名（原生源码对应）

csharp:

```c#
namespace System.Windows.Controls
{
    public class ContextMenu : MenuBase
    {
        // 特有依赖属性静态字段
        public static readonly DependencyProperty IsOpenProperty;
        public static readonly DependencyProperty PlacementProperty;
        public static readonly DependencyProperty PlacementTargetProperty;
        public static readonly DependencyProperty PlacementRectangleProperty;
        public static readonly DependencyProperty HorizontalOffsetProperty;
        public static readonly DependencyProperty VerticalOffsetProperty;
        public static readonly DependencyProperty StaysOpenProperty;
        public static readonly DependencyProperty HasDropShadowProperty;

        // 构造函数
        public ContextMenu();

        // 公共属性
        public bool IsOpen { get; set; }
        public PlacementMode Placement { get; set; }
        public UIElement PlacementTarget { get; set; }
        public Rect PlacementRectangle { get; set; }
        public double HorizontalOffset { get; set; }
        public double VerticalOffset { get; set; }
        public bool StaysOpen { get; set; }
        public bool HasDropShadow { get; set; }

        // 公共事件
        public event RoutedEventHandler Opened;
        public event RoutedEventHandler Closed;

        // 受保护重写方法
        protected override DependencyObject GetContainerForItemOverride();
        protected override bool IsItemItsOwnContainerOverride(object item);
        protected virtual void OnOpened(RoutedEventArgs e);
        protected virtual void OnClosed(RoutedEventArgs e);
        protected override AutomationPeer OnCreateAutomationPeer();
    }
}
```

### 1.2 核心元数据（官方精确值）

| 项               | 官方精确值                                                   | 工业场景关键说明                                   |
| :--------------- | :----------------------------------------------------------- | :------------------------------------------------- |
| **命名空间**     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                               |
| **程序集**       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                                 |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → MenuBase → ContextMenu` | 与 `Menu` 平级，共享菜单体系全部能力               |
| **默认条目容器** | `MenuItem`                                                   | 与主菜单完全复用，支持图标、复选、命令、多级子菜单 |
| **承载宿主**     | `Popup` 独立悬浮窗口                                         | 不在主界面视觉树内，是绑定问题的根源               |
| **核心设计**     | 右键触发 + 悬浮弹出 + 自动边界适配                           | 快捷操作入口，不占用固定界面空间                   |
| **工业核心场景** | 设备列表右键操作、表格行快捷编辑、批量操作入口、控件功能菜单 | 所有高频快捷操作场景                               |

> 🔑 架构同源性：`ContextMenu` 与 `Menu` 仅在弹出方式、定位逻辑上有差异，**所有 `MenuItem` 的用法、样式、数据模板完全通用**，主菜单的代码可无缝迁移到右键菜单。

------

## 二、静态依赖属性全量深度解析

`ContextMenu` 自身扩展了 8 个与弹出定位、显隐控制相关的依赖属性，其余集合管理、样式模板相关属性全部继承自 `MenuBase` / `ItemsControl`。

### 1. IsOpenProperty

csharp:

```c#
public static readonly DependencyProperty IsOpenProperty;
public bool IsOpen { get; set; }
```

- **类型**：`bool`，默认值 `false`
- **读写性**：支持双向绑定
- **官方作用**：控制右键菜单的打开与关闭状态。
- **底层机制**：
  - 设为 `true` 时，创建 Popup 窗口并显示菜单；
  - 设为 `false` 时，关闭 Popup 窗口；
  - 用户点击菜单外部区域、按下 Esc 键时，菜单会自动关闭，并反向同步更新 `IsOpen` 为 `false`。
- **工业场景价值**：
  - 代码中可手动控制菜单弹出，适配触摸屏长按弹出、按钮点击弹出等自定义触发方式；
  - MVVM 架构下可绑定到 ViewModel 布尔属性，通过数据控制菜单显隐。

### 2. PlacementProperty

csharp:

```c#
public static readonly DependencyProperty PlacementProperty;
public PlacementMode Placement { get; set; }
```

- **类型**：`PlacementMode` 枚举，默认值 `MousePoint`

- **官方作用**：设置菜单弹出的定位参考模式，决定菜单以什么为基准计算位置。

- **工业常用枚举值详解**：

  表格

  

  

  

  | 枚举值               | 定位规则                         | 典型工业场景                         |
  | :------------------- | :------------------------------- | :----------------------------------- |
  | `MousePoint`（默认） | 以鼠标右键点击的坐标为左上角弹出 | 通用右键菜单，设备列表、表格行操作   |
  | `Bottom`             | 对齐到目标元素的底部下方         | 下拉按钮、筛选器菜单、工具栏扩展菜单 |
  | `Right`              | 对齐到目标元素的右侧             | 侧边工具栏扩展、树节点操作菜单       |
  | `Center`             | 对齐到目标元素的中心             | 长按触发的圆形操作菜单               |
  | `Absolute`           | 相对于屏幕左上角绝对定位         | 自定义位置的全局操作菜单             |

### 3. PlacementTargetProperty（MVVM 绑定核心桥梁）

csharp:

```c#
public static readonly DependencyProperty PlacementTargetProperty;
public UIElement PlacementTarget { get; set; }
```

- **类型**：`UIElement`

- **官方作用**：菜单定位的参考目标元素，所有位置计算都基于该元素的坐标。

- **自动赋值机制**：

  当通过 `元素.ContextMenu`属性关联菜单时，WPF 框架会自动将 `PlacementTarget`设置为该宿主元素，无需手动赋值。

- **底层核心价值**：

  这是解决「右键菜单数据绑定失效」的唯一桥梁。由于 `ContextMenu`在独立 `Popup` 窗口中，不在主界面视觉树内，无法继承父级 `DataContext`；通过 `PlacementTarget.DataContext`可以间接获取宿主元素的数据上下文，实现命令、数据的绑定。

- **工业场景必用**：所有 MVVM 架构的右键菜单，命令绑定都必须通过 `PlacementTarget` 中转取值。

### 4. PlacementRectangleProperty

csharp:

```c#
public static readonly DependencyProperty PlacementRectangleProperty;
public Rect PlacementRectangle { get; set; }
```

- **类型**：`Rect`，默认值 `Rect.Empty`
- **官方作用**：在 `PlacementTarget` 元素内部，进一步指定一个矩形区域作为定位参考。
- **适用场景**：只需要在控件的局部区域（如图标、按钮）弹出菜单，而非整个控件区域。

### 5. HorizontalOffsetProperty / VerticalOffsetProperty

csharp:

```c#
public static readonly DependencyProperty HorizontalOffsetProperty;
public static readonly DependencyProperty VerticalOffsetProperty;

public double HorizontalOffset { get; set; }
public double VerticalOffset { get; set; }
```

- **类型**：`double`，默认值 0
- **官方作用**：在基础定位之上，额外的像素偏移量，用于微调菜单的弹出位置。
- **典型用法**：自定义菜单样式后，调整弹出位置实现像素级对齐，适配工业深色主题的边框、边距。

### 6. StaysOpenProperty

csharp:

```c#
public static readonly DependencyProperty StaysOpenProperty;
public bool StaysOpen { get; set; }
```

- **类型**：`bool`，默认值 `false`
- **官方作用**：点击菜单外部区域时，是否保持菜单打开状态。
- **行为差异**：
  - `false`（默认）：点击外部区域、按下 Esc、点击菜单项后，菜单自动关闭，符合常规右键菜单交互习惯；
  - `true`：必须手动将 `IsOpen` 设为 `false` 才会关闭。
- **工业适用场景**：
  - 多选操作菜单、参数配置菜单，需要连续操作多个选项；
  - 常驻式工具菜单，避免误点外部导致菜单消失。

### 7. HasDropShadowProperty

csharp:

```c#
public static readonly DependencyProperty HasDropShadowProperty;
public bool HasDropShadow { get; set; }
```

- **类型**：`bool`，默认值由系统主题决定
- **官方作用**：控制菜单窗口是否显示投影效果。
- **工业场景优化**：深色工控主题下通常设为 `false`，关闭投影，减少视觉干扰，降低眩光。

### 2.2 继承的高频核心属性

全部继承自 `MenuBase` 与 `ItemsControl`，与 `Menu` 用法完全一致：

| 分类     | 属性                                        | 作用                         |
| :------- | :------------------------------------------ | :--------------------------- |
| 数据绑定 | `ItemsSource`                               | 绑定菜单集合，动态生成菜单项 |
| 样式模板 | `ItemContainerStyle`                        | 自定义 `MenuItem` 容器样式   |
| 分层模板 | `ItemTemplate` + `HierarchicalDataTemplate` | 递归生成多级子菜单           |
| 外观控制 | `Background` / `Foreground` / `FontSize`    | 控制菜单整体外观             |

------

## 三、核心事件体系

### 3.1 生命周期专属事件

| 事件     | 事件处理委托         | 触发时机                           | 工业典型用法                                                 |
| :------- | :------------------- | :--------------------------------- | :----------------------------------------------------------- |
| `Opened` | `RoutedEventHandler` | 菜单完全弹出、界面渲染完成后触发   | 子菜单懒加载：弹出时异步查询数据库加载动态菜单项，避免初始化时全量加载拖慢速度 |
| `Closed` | `RoutedEventHandler` | 菜单完全关闭、Popup 窗口隐藏后触发 | 清理临时资源、取消数据订阅、保存临时编辑状态                 |

### 3.2 继承的菜单项路由事件

`MenuItem` 的 `Click`、`Checked`、`Unchecked`、`SubmenuOpened` 等事件均为**冒泡路由事件**，可直接在 `ContextMenu` 层级统一监听处理，适合批量菜单逻辑的集中管控。

------

## 四、核心方法逐行解析

### 4.1 条目容器生命周期方法

csharp:

```c#
protected override DependencyObject GetContainerForItemOverride();
protected override bool IsItemItsOwnContainerOverride(object item);
```

- **官方实现**：与 `Menu` 完全一致，返回 `new MenuItem()` 作为默认条目容器；判断项本身是否为 `MenuItem` 类型。
- **设计意义**：菜单项体系 100% 复用，所有 `MenuItem` 的特性（图标、复选、命令、子菜单）在右键菜单中完全可用，学习与维护成本为零。

### 4.2 生命周期触发方法

csharp:

```c#
protected virtual void OnOpened(RoutedEventArgs e);
protected virtual void OnClosed(RoutedEventArgs e);
```

- **作用**：菜单弹出 / 关闭的核心入口，内部触发 `Opened` / `Closed` 公共事件。
- **扩展价值**：自定义子类可重写这两个方法，实现：
  - 弹出前权限校验，无权限则阻止菜单弹出；
  - 弹出时动态构建菜单项；
  - 关闭时统一释放资源，防止内存泄漏。

### 4.3 自动化支持

csharp:

```c#
protected override AutomationPeer OnCreateAutomationPeer();
```

- 返回 `ContextMenuAutomationPeer`，提供 UI 自动化支持，适配无障碍访问与自动化测试框架。

------

## 五、核心底层工作机制

### 5.1 Popup 独立窗口承载（视觉树隔离的根源）

这是 `ContextMenu` 最核心的底层特性，也是所有绑定问题的本质原因。

1. **实现方式**：`ContextMenu` 的内容寄宿在 `Popup` 控件中，而 `Popup` 会创建一个独立的 Win32 顶层窗口来承载内容；
2. **视觉树隔离**：菜单的视觉元素与主窗口分属两个不同的窗口，不在同一个 WPF 视觉树中，因此**无法通过元素层级继承父级的 `DataContext`**；
3. **层级特性**：菜单始终悬浮在窗口最顶层，不会被主界面的其他控件遮挡；
4. **绑定解决方案**：通过 `PlacementTarget` 引用主视觉树中的宿主元素，间接获取数据上下文，这是 MVVM 绑定的标准路径。

> 🔑 一句话总结：右键菜单命令绑定失效，99% 的原因都是忽略了「菜单不在主视觉树」这个底层事实。

### 5.2 自动定位与边界检测

1. 弹出时基于 `Placement` 模式和 `PlacementTarget` 计算初始坐标；
2. 自动检测屏幕边界：如果按默认方向弹出会超出屏幕可视区域，则自动反向弹出（如下方空间不足就向上弹，右侧不足就向左弹）；
3. 原生支持多显示器场景，保证菜单始终完整显示在当前鼠标所在的屏幕内，不会跨屏割裂。

### 5.3 菜单体系复用机制

- 完全复用 `MenuItem` 作为条目单元，支持多级子菜单、复选、命令、图标等全部能力；
- 共享 `MenuBase` 的容器生成、样式应用、事件路由逻辑；
- 主菜单的样式、模板、数据模型，可无缝迁移到右键菜单，架构一致性极高。

------

## 六、设计本质总结

`ContextMenu` 是 `MenuBase` 体系在「快捷操作」场景的落地：它保留了完整的菜单能力，将承载方式从「固定菜单栏」改为「悬浮弹出窗口」，通过 `Placement` 系列属性实现灵活定位，同时也因独立窗口的特性带来了视觉树隔离的绑定问题。理解 `PlacementTarget` 的桥梁作用、Popup 独立窗口的本质，是用好右键菜单、排查绑定问题、实现复杂动态菜单的核心基础。