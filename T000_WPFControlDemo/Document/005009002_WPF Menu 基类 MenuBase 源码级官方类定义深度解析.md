# 005009002_WPF `Menu` 基类 `MenuBase` 源码级官方类定义深度解析

`MenuBase` 是 WPF 菜单体系的**抽象基类**，位于 `System.Windows.Controls.Primitives` 命名空间，直接继承自 `ItemsControl`，是 `Menu`（主菜单）和 `ContextMenu`（右键上下文菜单）的共同父类。它定义了菜单控件的通用容器规范、样式契约与基础行为，所有菜单项的生成、样式应用、层级逻辑都在这个基类中统一约定，保证了主菜单与右键菜单的行为一致性。

`Menu` 作为 `MenuBase` 的标准主菜单实现，仅扩展了主菜单专属的 `IsMainMenu` 特性与键盘激活逻辑，核心菜单能力全部继承自基类体系。

------

## 一、类层级总览

### 完整继承链

plaintext:

```tex
Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → MenuBase → Menu → ContextMenu
```

### 核心元数据

| 项           | 官方精确值                           | 说明                                                    |
| :----------- | :----------------------------------- | :------------------------------------------------------ |
| 基类命名空间 | `System.Windows.Controls.Primitives` | 基础控件原语命名空间                                    |
| 类修饰符     | `abstract`                           | 不能直接实例化，只能通过 Menu/ContextMenu 使用          |
| 程序集       | `PresentationFramework.dll`          | WPF 核心框架程序集                                      |
| 默认条目容器 | `MenuItem`                           | 所有菜单项的统一载体                                    |
| 设计思想     | 统一契约 + 差异化实现                | 基类定义通用规则，子类实现主菜单 / 右键菜单的差异化行为 |

------

## 二、MenuBase 抽象基类官方类定义

### 2.1 完整类签名（官方原生定义）

csharp:

```c#
namespace System.Windows.Controls.Primitives
{
    [System.Windows.Localizability(System.Windows.LocalizationCategory.Menu)]
    [System.Windows.StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(System.Windows.Controls.MenuItem))]
    public abstract class MenuBase : ItemsControl
    {
        // 依赖属性静态字段
        public static readonly DependencyProperty ItemContainerTemplateSelectorProperty;
        public static readonly DependencyProperty UsesItemContainerTemplateProperty;

        // 受保护构造函数
        protected MenuBase();

        // 公共属性
        public DataTemplateSelector ItemContainerTemplateSelector { get; set; }
        public bool UsesItemContainerTemplate { get; set; }

        // 受保护重写方法
        protected override DependencyObject GetContainerForItemOverride();
        protected override bool IsItemItsOwnContainerOverride(object item);
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e);
        protected override AutomationPeer OnCreateAutomationPeer();
    }
}
```

### 2.2 类级特性深度解析

#### 1. Localizability 本地化标记

csharp:

```c#
[Localizability(LocalizationCategory.Menu)]
```

- 向本地化工具标记该控件属于「菜单」类别，在做多语言适配时会按照菜单的扫描规则提取可本地化文本；
- 菜单体系所有可显示文本（菜单项标题、快捷键提示等）都会被纳入本地化扫描范围。

#### 2. StyleTypedProperty 样式类型契约

csharp:

```c#
[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(MenuItem))]
```

- 向设计器与 XAML 解析器声明，`ItemContainerStyle` 属性的目标容器类型为 `MenuItem`，提供样式智能提示与编译期类型校验；
- 与 `ListBox`、`TreeView` 遵循完全一致的 `ItemsControl` 体系约定，保证整个控件家族用法统一。

### 2.3 核心依赖属性解析

MenuBase 自身扩展了 2 个用于容器模板动态选择的依赖属性，用于复杂动态菜单场景。

#### 1. ItemContainerTemplateSelectorProperty

csharp:

```c#
public static readonly DependencyProperty ItemContainerTemplateSelectorProperty;
public DataTemplateSelector ItemContainerTemplateSelector { get; set; }
```

- **类型**：`DataTemplateSelector`
- **官方作用**：自定义容器模板选择器，根据数据项的类型、状态动态选择不同的 `MenuItem` 容器模板。
- **工业场景价值**：动态权限菜单场景下，不同类型的菜单项（分隔线、普通命令、复选命令）使用不同的容器模板，实现纯数据驱动的菜单渲染。

#### 2. UsesItemContainerTemplateProperty

csharp:

```c#
public static readonly DependencyProperty UsesItemContainerTemplateProperty;
public bool UsesItemContainerTemplate { get; set; }
```

- **类型**：`bool`，默认值 `false`
- **官方作用**：是否启用容器模板选择器。设置为 `true` 时，才会使用 `ItemContainerTemplateSelector` 动态选择容器模板。
- **底层意义**：默认关闭，保证普通场景下的菜单性能；需要动态模板时手动开启，是性能与灵活性的开关。

### 2.4 核心受保护方法解析

#### 1. 构造函数

csharp:

```c#
protected MenuBase();
```

- 受保护构造函数，仅子类可调用；
- 内部初始化默认样式元数据、默认容器类型，注册菜单通用的事件处理逻辑。

#### 2. 条目容器生命周期方法

MenuBase 重写了 ItemsControl 的容器契约方法，统一菜单项的生成规则：

| 方法                                         | 基类职责                       | 子类扩展                               |
| :------------------------------------------- | :----------------------------- | :------------------------------------- |
| `GetContainerForItemOverride()`              | 约定返回 MenuItem 类型容器     | Menu/ContextMenu 可返回自定义派生容器  |
| `IsItemItsOwnContainerOverride(object item)` | 判断 item 是否为 MenuItem 类型 | 支持 XAML 直接添加静态 MenuItem 子元素 |
| `OnItemsChanged(...)`                        | 集合变更时同步更新菜单项       | 处理菜单展开状态、键盘导航的有效性     |

> 设计价值：所有菜单子类共享同一套容器生成逻辑，保证 Menu 和 ContextMenu 中 MenuItem 的行为完全一致，降低学习与维护成本。

------

## 三、Menu 主菜单类官方类定义

`Menu` 是 `MenuBase` 的标准主菜单实现，针对窗口顶部菜单栏的场景扩展了主菜单专属的键盘激活与焦点行为。

### 3.1 完整类签名（官方原生定义）

csharp:

```c#
namespace System.Windows.Controls
{
    public class Menu : MenuBase
    {
        // 核心依赖属性字段
        public static readonly DependencyProperty IsMainMenuProperty;

        // 构造函数
        public Menu();

        // 核心公共属性
        public bool IsMainMenu { get; set; }

        // 受保护重写方法
        protected override DependencyObject GetContainerForItemOverride();
        protected override bool IsItemItsOwnContainerOverride(object item);
        protected override void OnKeyDown(KeyEventArgs e);
        protected override AutomationPeer OnCreateAutomationPeer();
    }
}
```

### 3.2 核心依赖属性：IsMainMenu

csharp:

```c#
public static readonly DependencyProperty IsMainMenuProperty;
public bool IsMainMenu { get; set; }
```

- **类型**：`bool`，默认值 `true`
- **官方作用**：标识当前菜单是否为窗口的主菜单栏。
- **底层联动效果**：
  1. **Alt 键激活**：设为 `true` 时，按下 `Alt` 键可自动激活菜单，进入键盘导航模式，首个菜单项获得焦点；
  2. **自动关闭**：点击窗口其他区域、菜单失去焦点时，自动关闭所有展开的子菜单；
  3. **访问键支持**：支持 `Alt+字母` 快捷访问（如 `_文件` 对应 `Alt+F`），接收窗口的菜单键盘消息；
  4. **焦点管理**：主菜单获得焦点时，自动接管窗口的键盘导航，Esc 键可逐级退出菜单。
- **工业最佳实践**：
  - 窗口顶部的系统菜单栏保持默认 `true`，符合 Windows 标准交互习惯；
  - 用户控件内的局部菜单、内嵌菜单设为 `false`，避免与窗口主菜单的键盘行为冲突。

### 3.3 核心重写方法解析

#### 1. GetContainerForItemOverride

csharp:

```c#
protected override DependencyObject GetContainerForItemOverride();
```

- 官方实现：返回 `new MenuItem()`；
- 作用：指定主菜单的默认条目容器为标准 `MenuItem`。

#### 2. IsItemItsOwnContainerOverride

csharp:

```c#
protected override bool IsItemItsOwnContainerOverride(object item);
```

- 官方实现：判断 `item is MenuItem`；
- 作用：支持 XAML 中直接声明 `<MenuItem>` 静态菜单项，无需框架额外包装。

#### 3. OnKeyDown

csharp:

```c#
protected override void OnKeyDown(KeyEventArgs e);
```

- 扩展主菜单专属的键盘导航逻辑：
  - `Alt` 键：激活 / 取消激活菜单；
  - 左右方向键：顶级菜单项之间切换；
  - 上下方向键：打开子菜单并在子菜单项间移动；
  - `Esc` 键：逐级关闭子菜单，最终退出菜单激活模式；
  - 访问键（字母）：直接打开对应下划线标记的菜单。
- 工业场景价值：完整支持纯键盘操作，适配工控机无鼠标、触摸屏操作场景，符合工业软件操作规范。

#### 4. OnCreateAutomationPeer

csharp:

```c#
protected override AutomationPeer OnCreateAutomationPeer();
```

- 返回 `MenuAutomationPeer`，提供 UI 自动化支持，适配无障碍访问与自动化测试框架。

------

## 四、菜单体系核心底层工作机制

### 4.1 层级嵌套与弹出机制

- 菜单项 `MenuItem` 继承自 `HeaderedItemsControl`，自身也是集合容器，可无限嵌套子菜单；
- 子菜单通过 `Popup` 控件承载，悬浮在父菜单右侧 / 下方；
- 鼠标悬浮到带子菜单的父项时，自动延时展开子菜单（默认延时 400ms，避免误触）；
- 菜单弹出时自动检测屏幕边界，空间不足时反向弹出，避免超出屏幕可视区域。

### 4.2 主菜单键盘激活链路

plaintext:

```tex
按下 Alt 键
    ↓
窗口消息传递到主菜单（IsMainMenu=true）
    ↓
菜单进入激活模式，首个顶级菜单项获得焦点
    ↓
方向键 / 访问键导航选择菜单项
    ↓
回车执行命令 / Esc 退出激活模式
    ↓
点击空白区域 → 失去焦点 → 自动关闭所有子菜单
```

### 4.3 命令路由机制

菜单体系完整复用 WPF 命令路由：

1. 点击 `MenuItem` 时，执行其 `Command` 属性绑定的命令；
2. 命令以冒泡路由向上传递，直到找到对应 `CommandBinding`；
3. `CanExecute` 状态自动同步到菜单项的可用状态，命令不可用时菜单项自动灰显；
4. 工业场景下可通过命令的 `CanExecute` 统一实现权限控制，无权限的菜单项自动禁用。

------

## 五、Menu 与 ContextMenu 的同源差异

两者同继承自 `MenuBase`，核心菜单项逻辑完全一致，仅场景与触发方式不同：

| 维度             | Menu（主菜单）         | ContextMenu（右键菜单）            |
| :--------------- | :--------------------- | :--------------------------------- |
| 典型位置         | 窗口顶部固定菜单栏     | 任意控件的右键弹出层               |
| 触发方式         | Alt 键 / 鼠标点击      | 右键点击目标控件                   |
| 核心扩展属性     | IsMainMenu             | PlacementTarget、IsOpen、Placement |
| 焦点行为         | 激活时接管窗口键盘焦点 | 弹出时获得焦点，失焦自动关闭       |
| 视觉树位置       | 主视觉树内             | 独立 Popup 窗口，不在主视觉树      |
| DataContext 继承 | 直接继承父级           | 需通过 PlacementTarget 中转绑定    |

------

## 总结

`MenuBase` 抽象基类是 WPF 菜单体系的骨架：它基于 `ItemsControl` 体系，统一了菜单项的容器规范、样式契约与基础行为，让 `Menu` 和 `ContextMenu` 可以专注于场景化的差异扩展。理解 `MenuBase` 的通用契约与 `IsMainMenu` 的底层行为，是自定义菜单样式、实现动态权限菜单、排查键盘交互问题的核心基础。