# 005008003_WPF `TreeView` 树形控件源码级官方类定义深度解析

**源码：**

```c#
public class TreeView : ItemsControl
{
    public static readonly DependencyProperty SelectedItemProperty;
    public static readonly DependencyProperty SelectedValueProperty;
    public static readonly DependencyProperty SelectedValuePathProperty;
    public static readonly RoutedEvent SelectedItemChangedEvent;
 
    public TreeView();
 
    public string SelectedValuePath { get; set; }
    public object SelectedValue { get; }
    public object SelectedItem { get; }
    protected internal override bool HandlesScrolling { get; }
 
    public event RoutedPropertyChangedEventHandler<object> SelectedItemChanged;
 
    protected virtual bool ExpandSubtree(TreeViewItem container);
    protected override DependencyObject GetContainerForItemOverride();
    protected override bool IsItemItsOwnContainerOverride(object item);
    protected override AutomationPeer OnCreateAutomationPeer();
    protected override void OnGotFocus(RoutedEventArgs e);
    protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e);
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e);
    protected override void OnKeyDown(KeyEventArgs e);
    protected virtual void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e);
}

```

你给出的是更贴近 .NET 运行时原生源码的 `TreeView` 类定义，完整呈现了**路由事件注册字段、受保护扩展方法、全链路焦点管理**等底层核心成员，清晰还原了 TreeView 的架构本质：它直接继承自 `ItemsControl`，不依赖 `Selector` 单选体系，通过「路由事件冒泡 + 节点自管理选中」的模式实现树形单选，同时内置滚动自处理、键盘导航、焦点同步等完整交互能力。

这份定义是自定义 TreeView 子类、排查选中与焦点疑难问题、深度定制树形交互的核心依据。

------

## 一、类定义总览与核心元数据

### 1.1 完整类签名（对应原生源码）

csharp:

```c#
namespace System.Windows.Controls
{
    public class TreeView : ItemsControl
    {
        // 静态依赖属性字段
        public static readonly DependencyProperty SelectedItemProperty;
        public static readonly DependencyProperty SelectedValueProperty;
        public static readonly DependencyProperty SelectedValuePathProperty;
        // 路由事件注册字段
        public static readonly RoutedEvent SelectedItemChangedEvent;

        // 构造函数
        public TreeView();

        // 公共属性
        public string SelectedValuePath { get; set; }
        public object SelectedValue { get; }
        public object SelectedItem { get; }
        // 受保护内部属性
        protected internal override bool HandlesScrolling { get; }

        // 公共事件包装
        public event RoutedPropertyChangedEventHandler<object> SelectedItemChanged;

        // 受保护核心方法
        protected virtual bool ExpandSubtree(TreeViewItem container);
        protected override DependencyObject GetContainerForItemOverride();
        protected override bool IsItemItsOwnContainerOverride(object item);
        protected override AutomationPeer OnCreateAutomationPeer();
        protected override void OnGotFocus(RoutedEventArgs e);
        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e);
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e);
        protected override void OnKeyDown(KeyEventArgs e);
        protected virtual void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e);
    }
}
```

### 1.2 核心元数据（官方精确值）

| 项           | 官方精确值                                                   | 工业场景关键说明                                             |
| :----------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| 命名空间     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                                         |
| 程序集       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                                           |
| 完整继承链   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → TreeView` | 纯集合控件体系，无 Selector 依赖，选中逻辑独立实现           |
| 默认节点容器 | `TreeViewItem`                                               | 继承自 `HeaderedItemsControl`，自身也是集合容器，支持无限嵌套子节点 |
| 核心设计     | 路由事件驱动选中 + 滚动自管理 + 递归层级渲染                 | 纯数据驱动生成树形结构，选中由子节点冒泡到根                 |
| 工业核心场景 | 设备层级结构、工艺 BOM、组织权限树、导航菜单                 | 所有层级从属关系的数据可视化                                 |

> ⚠️ 架构本质差异：TreeView **没有继承 Selector**，因此原生没有 `SelectedIndex`、`SelectionMode`、多选集合等能力，单选逻辑完全通过 `TreeViewItem` 的选中事件冒泡实现，这是它与 ListBox/ComboBox/TabControl 最核心的架构区别。

------

## 二、静态字段全量深度解析

### 2.1 依赖属性静态字段

WPF 依赖属性的标准注册模式：静态字段存储依赖属性标识，实例属性包装读写逻辑。

| 字段名                      | 对应包装属性        | 属性类型 | 读写性 | 官方作用                     |
| :-------------------------- | :------------------ | :------- | :----- | :--------------------------- |
| `SelectedItemProperty`      | `SelectedItem`      | `object` | 只读   | 存储当前选中节点的数据上下文 |
| `SelectedValueProperty`     | `SelectedValue`     | `object` | 只读   | 存储按路径提取的选中节点值   |
| `SelectedValuePathProperty` | `SelectedValuePath` | `string` | 可读写 | 指定选中值的提取路径         |

#### 核心细节：SelectedItem 的只读实现

`SelectedItemProperty` 内部以**只读依赖属性**（`RegisterReadOnly`）方式注册，对外只暴露 getter，内部通过私有 `SetValue` 方法更新。这就是 MVVM 中无法直接双向绑定 `SelectedItem` 的底层原因：属性元数据本身就不允许外部赋值，只能由控件内部的选中冒泡逻辑更新。

### 2.2 路由事件静态字段：SelectedItemChangedEvent

csharp:

```c#
public static readonly RoutedEvent SelectedItemChangedEvent;
```

- 这是 `SelectedItemChanged` 事件的底层路由事件标识，采用**冒泡路由策略**注册；
- 所有 `TreeViewItem` 的 `Selected` 事件冒泡到根节点后，最终触发该路由事件；
- 完全遵循 WPF 路由事件的标准注册模式：静态字段 + CLR 事件包装 + 受保护触发方法。
- 扩展意义：自定义子类可通过 `AddHandler` 直接监听该路由事件，实现更底层的选中拦截逻辑。

------

## 三、实例属性逐行解析

### 3.1 公共属性

#### 1. SelectedValuePath

csharp:

```c#
public string SelectedValuePath { get; set; }
```

- 类型：字符串，默认值为 `string.Empty`
- 官方作用：指定 `SelectedValue` 的属性提取路径。
- 工作机制：设置为某个属性名（如 `DeviceCode`）后，选中节点时，框架通过反射从 `SelectedItem` 中提取对应属性的值，赋值给 `SelectedValue`。
- 工业场景价值：批量操作场景下只需获取设备编号等关键字段，无需传递完整数据对象，逻辑更轻量。

#### 2. SelectedValue

csharp:

```c#
public object SelectedValue { get; }
```

- 类型：`object`，只读
- 官方作用：根据 `SelectedValuePath` 提取的选中节点值。
- 联动逻辑：`SelectedItem` 变化时，自动按路径重新计算并更新该属性；若 `SelectedValuePath` 为空，则与 `SelectedItem` 等价。

#### 3. SelectedItem

csharp:

```c#
public object SelectedItem { get; }
```

- 类型：`object`，只读
- 官方作用：当前选中树节点对应的数据对象（即节点的 DataContext）。
- 更新时机：节点选中事件冒泡到根节点时，由内部逻辑赋值；同一时间仅保留一个选中项。
- MVVM 标准解决方案：通过 `ItemContainerStyle` 将 `TreeViewItem.IsSelected` 双向绑定到数据模型属性，配合 `SelectedItemChanged` 事件中转同步到 ViewModel。

### 3.2 受保护内部属性：HandlesScrolling

csharp:

```c#
protected internal override bool HandlesScrolling { get; }
```

- 官方返回值：`true`
- 核心作用：向 WPF 输入导航系统声明「控件自身负责滚动逻辑」，是控件内置滚动能力的官方标识。
- 底层联动效果：
  1. 内置 `ScrollViewer`，不需要外层额外包裹；
  2. 键盘方向键切换选中项时，自动调用 `BringIntoView` 保证选中项始终在可视区域内；
  3. 滚动逻辑与选中逻辑深度联动，避免选中项滚出屏幕不可见。
- 避坑提示：外层禁止嵌套 `ScrollViewer`，否则会破坏滚动联动、导致虚拟化失效、产生双滚动条。

------

## 四、事件体系解析

### SelectedItemChanged 事件

csharp:

```c#
public event RoutedPropertyChangedEventHandler<object> SelectedItemChanged;
```

- 事件委托类型：`RoutedPropertyChangedEventHandler<object>`，参数包含 `OldValue` 和 `NewValue`，可直接获取切换前后的选中数据对象。
- 底层关联：包装 `SelectedItemChangedEvent` 路由事件，采用冒泡策略。
- 触发时机：选中节点发生变化时触发，无论是鼠标点击、键盘导航还是代码导致的选中变更都会触发。
- 工业核心用法：主从联动 —— 选中设备 / 工序节点后，右侧加载对应详情、参数、报警数据，是 TreeView 最核心的业务入口。

------

## 五、受保护核心方法逐行深度解析

这部分是自定义 TreeView 子类的全部扩展点，按职责分为容器生命周期、选中管理、焦点管理、输入交互、集合变更五大类。

### 5.1 条目容器生命周期

#### GetContainerForItemOverride

csharp:

```c#
protected override DependencyObject GetContainerForItemOverride();
```

- 官方实现：返回 `new TreeViewItem()`
- 设计意义：指定根层级节点的默认容器类型为 `TreeViewItem`，是 ItemsControl 容器契约的标准实现。
- 扩展场景：自定义节点容器时重写，返回继承自 TreeViewItem 的自定义容器。

#### IsItemItsOwnContainerOverride

csharp:

```c#
protected override bool IsItemItsOwnContainerOverride(object item);
```

- 官方实现：判断 `item is TreeViewItem`，是则返回 true
- 作用：支持 XAML 中直接添加 `<TreeViewItem>` 静态子元素，无需框架额外包装。

### 5.2 选中管理核心方法

#### OnSelectedItemChanged

csharp:

```c#
protected virtual void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e);
```

- 触发时机：选中项变化时调用，是 `SelectedItemChanged` 事件的官方触发入口。
- 官方执行流程：
  1. 更新 `SelectedItem` 只读依赖属性的值；
  2. 根据 `SelectedValuePath` 重新计算并更新 `SelectedValue`；
  3. 触发 `SelectedItemChanged` 公共路由事件；
  4. 自动滚动使新选中项进入可视区域。
- 扩展价值：子类重写可实现权限校验（无权限的节点禁止选中）、多选逻辑扩展、选中前拦截等定制能力。

#### ExpandSubtree

csharp:

```c#
protected virtual bool ExpandSubtree(TreeViewItem container);
```

- 官方作用：递归展开指定节点及其所有层级的子节点。
- 参数：`container` 为目标树节点容器
- 返回值：展开成功返回 `true`，失败返回 `false`
- 底层执行逻辑：
  1. 设置当前节点 `IsExpanded = true`；
  2. 等待子节点容器生成（适配懒加载场景）；
  3. 遍历所有子节点，递归调用 `ExpandSubtree`；
  4. 全部展开完成后返回结果。
- 扩展场景：自定义「全部展开」命令、搜索定位节点后自动展开全路径。
- 注意：这是受保护方法，外部无法直接调用；业务层展开全树通常通过遍历数据模型设置 `IsExpanded` 属性实现。

### 5.3 焦点管理方法

TreeView 是典型的复合控件，焦点可能落在任意子节点上，这两个方法负责维护全局焦点状态的一致性。

#### OnGotFocus

csharp:

```c#
protected override void OnGotFocus(RoutedEventArgs e);
```

- 触发时机：控件整体获得键盘焦点时。
- 官方默认逻辑：
  1. 若已有选中项，将焦点移动到选中的节点上；
  2. 若无选中项，将焦点移动到第一个可见节点；
  3. 保证键盘导航始终有明确的目标节点，避免焦点丢失。

#### OnIsKeyboardFocusWithinChanged

csharp:

```c#
protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e);
```

- 触发时机：控件内部的键盘焦点发生迁移时（比如焦点从一个节点移到另一个节点）。
- 官方核心逻辑：
  1. 同步更新所有节点的 `IsSelectionActive` 状态：控件内部有焦点时，选中态为激活高亮；失焦时为失焦灰显；
  2. 统一更新视觉状态，避免因为焦点在子节点上，导致整体选中态视觉异常。
- 样式定制价值：通过 `IsSelectionActive` 触发器可区分「激活选中」和「失焦选中」两种视觉效果，提升工业界面的状态清晰度，减少操作失误。

### 5.4 集合变更处理

csharp:

```c#
protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e);
```

- 触发时机：根节点数据源集合发生增删改、重置时触发。
- 官方执行逻辑：
  1. 调用基类方法，同步更新根节点容器；
  2. 校验当前选中项的有效性：若选中项被删除，自动清空选中状态；
  3. 集合重置后，重置所有节点状态。

### 5.5 键盘输入处理

csharp:

```c#
protected override void OnKeyDown(KeyEventArgs e);
```

- 触发时机：键盘按键按下时，冒泡到 TreeView 根节点。
- 官方内置完整树形键盘导航，完全适配工控无鼠标场景：

| 按键       | 对应行为                                     |
| :--------- | :------------------------------------------- |
| ↑ / ↓      | 向上 / 向下移动选中节点（同级节点间切换）    |
| ←          | 若节点展开则折叠；若已折叠则选中父节点       |
| →          | 若节点折叠则展开；若已展开则选中第一个子节点 |
| Home / End | 跳转到第一个 / 最后一个根节点                |
| 空格       | 切换节点选中状态                             |

- 扩展价值：子类重写可自定义工业常用快捷键，比如 F2 重命名节点、Delete 删除节点等。

### 5.6 自动化支持

csharp:

```c#
protected override AutomationPeer OnCreateAutomationPeer();
```

- 返回 `TreeViewAutomationPeer`，提供 UI 自动化支持，适配无障碍访问与自动化测试框架。

------

## 六、核心底层工作机制

### 6.1 选中冒泡完整链路

TreeView 选中逻辑的核心是「子节点触发，根节点汇总」，完整流程如下：

plaintext:

```tex
用户点击节点 / 键盘切换选中
    ↓
对应 TreeViewItem 的 IsSelected = true
    ↓
触发 TreeViewItem.Selected 冒泡路由事件
    ↓
事件逐层向上经过所有父节点
    ↓
到达 TreeView 根节点
    ↓
调用 OnSelectedItemChanged 方法
    ↓
1. 更新 SelectedItem / SelectedValue 只读属性
2. 触发 SelectedItemChanged 公共事件
3. 自动滚动到选中项
```

这就是 `SelectedItem` 为只读的本质：选中的决策权在子节点，根节点只负责接收和汇总结果。

### 6.2 滚动自管理机制

因为 `HandlesScrolling = true`，TreeView 的滚动与选中深度绑定：

1. 键盘切换选中项时，自动调用 `BringIntoView` 保证选中项可见；
2. 鼠标滚轮直接作用于内置 ScrollViewer，不需要额外处理；
3. 虚拟化模式下，滚动与容器回收复用联动，保证性能稳定。

### 6.3 焦点与选中态分离机制

通过 `IsSelectionActive` 属性区分两种选中状态：

- 控件有焦点时：`IsSelectionActive = true`，选中态高亮显示；
- 控件失焦时：`IsSelectionActive = false`，选中态灰显但不丢失；
- 工业界面价值：多面板布局下，用户能清晰区分哪个面板处于激活状态，避免操作失误。

------

## 总结

这份源码级定义完整还原了 TreeView 的架构本质：它是 ItemsControl 在分层场景的延伸，没有复用 Selector 体系，而是通过路由事件冒泡实现了轻量的单选管理，同时内置滚动自处理、全键盘导航、焦点状态同步等完整交互能力。理解 `SelectedItem` 只读的底层原因、选中冒泡链路、焦点与选中态分离机制，是定制树形交互、排查 MVVM 绑定问题、优化工业场景交互体验的核心基础。