# 005008002_WPF `TreeView` 树形控件源码级官方类定义深度解析

`TreeView` 是 WPF 原生的分层树形结构控件，直接继承自 `ItemsControl` 集合基类，**不继承 `Selector` 单选体系**，这是它与 `ListBox`/`ComboBox`/`TabControl` 最本质的架构差异。它通过递归嵌套的 `TreeViewItem` 节点容器实现无限层级渲染，自带展开折叠、节点选中、键盘导航等完整树形交互，是工业场景下设备结构树、工艺 BOM、组织权限、导航菜单的标准控件。

其核心设计思想是**节点即容器**：每个树节点本身就是一个 `HeaderedItemsControl`，天然支持子节点嵌套，配合分层数据模板可实现纯数据驱动的自动递归渲染。

------

## 一、官方类定义总览与核心元数据

### 1.1 完整类签名（官方原生定义）

csharp:

```c#
namespace System.Windows.Controls
{
    [System.Windows.StyleTypedPropertyAttribute(Property = "ItemContainerStyle", StyleTargetType = typeof(System.Windows.Controls.TreeViewItem))]
    public class TreeView : ItemsControl
    {
        // 核心依赖属性字段
        public static readonly DependencyProperty SelectedItemProperty;
        public static readonly DependencyProperty SelectedValueProperty;
        public static readonly DependencyProperty SelectedValuePathProperty;

        // 构造函数
        public TreeView();

        // 公共属性
        public object SelectedItem { get; }
        public object SelectedValue { get; }
        public string SelectedValuePath { get; set; }

        // 受保护内部属性
        protected internal override bool HandlesScrolling { get; }

        // 公共事件
        public event RoutedPropertyChangedEventHandler<object> SelectedItemChanged;

        // 受保护重写方法
        protected override DependencyObject GetContainerForItemOverride();
        protected override bool IsItemItsOwnContainerOverride(object item);
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
        protected override void ClearContainerForItemOverride(DependencyObject element, object item);
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e);
        protected virtual void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e);
        protected override AutomationPeer OnCreateAutomationPeer();
        protected override void OnKeyDown(KeyEventArgs e);
        protected override void OnGotFocus(RoutedEventArgs e);
    }
}
```

### 1.2 核心元数据（官方精确值）

| 项               | 官方精确值                                                   | 工业场景关键说明                                             |
| :--------------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| **命名空间**     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                                         |
| **程序集**       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                                           |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → TreeView` | 直接继承集合基类，无 Selector 依赖，选中逻辑自行实现         |
| **默认节点容器** | `TreeViewItem`                                               | 继承自 `HeaderedItemsControl`，自身也是集合容器，支持无限嵌套子节点 |
| **默认布局面板** | `StackPanel`                                                 | 根节点垂直排列，子节点同样使用 StackPanel 嵌套               |
| **核心设计**     | 递归分层渲染 + 选中冒泡机制 + 原生展开折叠交互               | 纯数据驱动自动生成树形结构，无需手动嵌套 UI                  |
| **工业核心场景** | 设备层级结构、工艺 BOM 清单、组织权限树、系统导航菜单        | 所有具有层级从属关系的数据展示与交互                         |

> ⚠️ 架构关键差异：`TreeView` 不继承 `Selector`，因此**没有 `SelectedIndex`、`SelectionMode`、`SelectedItems` 等属性**，原生仅支持单选，且 `SelectedItem` 为只读属性，这是 MVVM 开发最容易踩坑的核心点。

### 1.3 类级特性解析

**`[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(TreeViewItem))]`**

- 向设计器与 XAML 解析器声明，`ItemContainerStyle` 的目标容器类型为 `TreeViewItem`，提供样式智能提示与编译期类型校验；
- 与 `ListBox`、`ComboBox` 遵循完全一致的 `ItemsControl` 体系约定，保证整个控件家族用法统一。

------

## 二、静态依赖属性全量深度解析

`TreeView` 自身仅新增 3 个核心依赖属性，其余全部继承自 `ItemsControl` 与 `Control`。

### 2.1 TreeView 新增核心依赖属性

#### 1. SelectedItemProperty

csharp:

```c#
public static readonly DependencyProperty SelectedItemProperty;
public object SelectedItem { get; }
```

- **类型**：`object`

- **读写性**：**只读依赖属性**，对外仅暴露 getter，内部由选中冒泡逻辑维护

- **官方作用**：获取当前选中树节点对应的数据对象。

- **底层机制**：

  1. 用户点击节点时，对应 `TreeViewItem` 的 `IsSelected` 变为 true；
  2. `Selected` 路由事件向上冒泡，到达 TreeView 根节点时，框架将该节点的数据上下文赋值给 `SelectedItem`；
  3. 同一时间仅保留一个选中项，原生为单选模式。

- **MVVM 核心坑点**：

  

  不能直接双向绑定 `SelectedItem`，直接写 `{Binding SelectedNode, Mode=TwoWay}`会静默失效。标准解决方案是通过 `ItemContainerStyle`将 `TreeViewItem.IsSelected`双向绑定到数据模型属性，配合 `SelectedItemChanged`事件中转同步到 ViewModel。

#### 2. SelectedValueProperty + SelectedValuePathProperty

csharp:

```c#
public static readonly DependencyProperty SelectedValueProperty;
public static readonly DependencyProperty SelectedValuePathProperty;

public object SelectedValue { get; }
public string SelectedValuePath { get; set; }
```

- **类型**：`SelectedValue` 为 `object`（只读），`SelectedValuePath` 为 `string`（可读写）
- **官方作用**：按指定属性路径提取选中节点的值，无需获取完整数据对象。
- **工作逻辑**：设置 `SelectedValuePath="DeviceCode"` 后，选中节点时，`SelectedValue` 自动返回该节点 `DeviceCode` 属性的值。
- **工业场景价值**：批量操作、参数下发时，只需获取设备编号等关键字段，无需传递完整节点对象，逻辑更轻量。

### 2.2 受保护内部属性

#### HandlesScrolling

csharp:

```c#
protected internal override bool HandlesScrolling { get; }
```

- **官方返回值**：`true`
- **作用**：向 WPF 输入与导航系统声明，控件自身负责管理滚动逻辑。
- **底层意义**：TreeView 内置 `ScrollViewer`，不需要外层额外包裹；键盘方向键同时控制节点选中与滚动定位，保证选中项始终在可视区域内。
- **避坑提示**：外层不要嵌套 `ScrollViewer`，否则会导致滚动冲突、虚拟化失效、性能下降。

### 2.3 继承的高频核心属性

全部继承自 `ItemsControl`，但在树形场景下有特殊用法：

| 分类     | 属性                                     | 树形场景说明                                                 |
| :------- | :--------------------------------------- | :----------------------------------------------------------- |
| 数据绑定 | `ItemsSource`                            | 仅绑定根节点集合，子节点由分层模板的 `ItemsSource` 递归绑定  |
| 节点模板 | `ItemTemplate`                           | 树形场景使用 `HierarchicalDataTemplate` 分层模板替代普通 DataTemplate |
| 容器样式 | `ItemContainerStyle`                     | 自定义 `TreeViewItem` 外观、绑定 `IsSelected`/`IsExpanded` 到数据模型 |
| 布局面板 | `ItemsPanel`                             | 大数据量替换为 `VirtualizingStackPanel` 开启层级虚拟化       |
| 外观     | `Background` / `BorderBrush` / `Padding` | 控件整体外观，适配工业深色 / 浅色主题                        |

------

## 三、核心事件体系全解析

### 3.1 控件级专属事件

| 事件                  | 事件参数类型                             | 触发时机               | 工业核心用法                                                 |
| :-------------------- | :--------------------------------------- | :--------------------- | :----------------------------------------------------------- |
| `SelectedItemChanged` | `RoutedPropertyChangedEventArgs<object>` | 选中节点发生变化时触发 | 主从联动核心：选中设备 / 工序节点后，右侧加载对应详情、参数、报警数据；是 TreeView 最常用的事件 |

> 事件参数包含 `OldValue`（原选中项）和 `NewValue`（新选中项），可直接获取切换前后的数据对象。

### 3.2 节点级路由事件（冒泡到 TreeView）

这些事件属于 `TreeViewItem`，通过路由冒泡机制可在 TreeView 层面统一监听处理：

| 事件                      | 触发时机                  | 典型工业用法                                                 |
| :------------------------ | :------------------------ | :----------------------------------------------------------- |
| `Expanded`                | 节点展开时触发            | 子节点懒加载：展开时才异步查询子级数据，避免一次性加载全量树导致卡顿 |
| `Collapsed`               | 节点折叠时触发            | 清理子节点资源、取消数据订阅、释放非托管资源                 |
| `Selected` / `Unselected` | 节点选中 / 取消选中时触发 | 节点级选中逻辑、级联选中状态更新                             |

### 3.3 继承的常用交互事件

- `MouseDoubleClick`：双击节点，常用于打开详情窗口、切换展开折叠状态；
- `MouseRightButtonUp`：右键点击，用于弹出节点右键菜单（新增、删除、编辑、下发等操作）；
- `KeyDown`：键盘导航，原生支持方向键展开折叠、上下移动、回车选中，适配工控无鼠标场景。

------

## 四、核心方法逐行解析

### 4.1 条目容器生命周期方法

完全遵循 `ItemsControl` 的容器契约，负责根节点的生成、准备与清理，子节点的同类逻辑由 `TreeViewItem` 自身递归执行。

| 方法                                         | 官方实现                             | 核心作用                                                |
| :------------------------------------------- | :----------------------------------- | :------------------------------------------------------ |
| `GetContainerForItemOverride()`              | 返回 `new TreeViewItem()`            | 指定根节点的默认容器类型                                |
| `IsItemItsOwnContainerOverride(object item)` | 判断 `item is TreeViewItem`          | 支持 XAML 直接添加静态 `<TreeViewItem>` 节点            |
| `PrepareContainerForItemOverride(...)`       | 绑定数据、应用样式、同步节点状态     | 节点生成 / 复用时初始化内容，建立父子层级关系           |
| `ClearContainerForItemOverride(...)`         | 清理节点状态、解绑数据、移除事件订阅 | 节点移除 / 虚拟化回收时清理资源，防止状态残留与内存泄漏 |

### 4.2 集合变更处理

csharp:

```c#
protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e);
```

- 根节点集合增删改时触发，同步更新选中项有效性；
- 集合重置时重新生成所有根节点容器。

### 4.3 选中事件触发入口

csharp:

```c#
protected virtual void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e);
```

- 节点选中事件冒泡到 TreeView 时调用；
- 内部更新 `SelectedItem`、`SelectedValue` 只读属性；
- 触发 `SelectedItemChanged` 公共事件；
- 子类重写可扩展多选、级联选中、权限校验等自定义逻辑。

### 4.4 输入与焦点处理

#### OnKeyDown

csharp:

```c#
protected override void OnKeyDown(KeyEventArgs e);
```

- 内置完整键盘导航逻辑，适配纯键盘操作：
  - ↑ ↓：上下移动选中节点；
  - ←：折叠当前节点，若已折叠则选中父节点；
  - →：展开当前节点，若已展开则选中第一个子节点；
  - Home / End：跳到首节点 / 末节点；
  - 空格：切换节点选中状态。

#### OnGotFocus

csharp:

```c#
protected override void OnGotFocus(RoutedEventArgs e);
```

- 控件获得焦点时，自动将焦点移动到当前选中节点；
- 无选中项时聚焦第一个节点，保证键盘导航始终有明确目标。

### 4.5 自动化支持

csharp:

```c#
protected override AutomationPeer OnCreateAutomationPeer();
```

- 返回 `TreeViewAutomationPeer`，提供 UI 自动化支持，适配无障碍访问与自动化测试框架。

------

## 五、配套节点容器：`TreeViewItem` 类定义深度解析

`TreeViewItem` 是树形结构的核心载体，继承自 `HeaderedItemsControl`，**自身也是一个 ItemsControl**，因此可以无限嵌套子节点，这是 TreeView 能实现递归层级的底层基础。

### 5.1 官方精简类定义

csharp:

```c#
namespace System.Windows.Controls
{
    public class TreeViewItem : HeaderedItemsControl
    {
        public static readonly DependencyProperty IsSelectedProperty;
        public static readonly DependencyProperty IsExpandedProperty;
        public static readonly DependencyProperty IsSelectionActiveProperty;

        public bool IsSelected { get; set; }
        public bool IsExpanded { get; set; }
        public bool IsSelectionActive { get; }

        public event RoutedEventHandler Expanded;
        public event RoutedEventHandler Collapsed;
        public event RoutedEventHandler Selected;
        public event RoutedEventHandler Unselected;

        public void ExpandSubtree();
        public void CollapseSubtree();

        protected virtual void OnExpanded(RoutedEventArgs e);
        protected virtual void OnCollapsed(RoutedEventArgs e);
        protected virtual void OnSelected(RoutedEventArgs e);
        protected virtual void OnUnselected(RoutedEventArgs e);
        protected override DependencyObject GetContainerForItemOverride();
        protected override bool IsItemItsOwnContainerOverride(object item);
    }
}
```

### 5.2 核心依赖属性

| 属性                | 类型           | 官方作用                             | MVVM 绑定价值                                                |
| :------------------ | :------------- | :----------------------------------- | :----------------------------------------------------------- |
| `IsSelected`        | `bool`         | 当前节点是否处于选中状态             | 选中双向绑定的唯一入口，通过样式绑定到数据模型，即可实现 ViewModel 控制节点选中 |
| `IsExpanded`        | `bool`         | 当前节点是否处于展开状态             | 绑定到数据模型可持久化展开状态，避免虚拟化滚动、节点复用时状态丢失 |
| `IsSelectionActive` | `bool`（只读） | 选中状态是否处于激活态（控件有焦点） | 用于区分「有焦点选中」和「失焦选中」，定制样式时避免失焦后选中态看不清 |

### 5.3 核心路由事件

| 事件                      | 路由策略 | 触发时机                                              |
| :------------------------ | :------- | :---------------------------------------------------- |
| `Expanded` / `Collapsed`  | 冒泡     | 节点展开 / 折叠时触发                                 |
| `Selected` / `Unselected` | 冒泡     | 节点选中 / 取消选中时触发，最终冒泡到 TreeView 根节点 |

> 正是因为 `Selected` 事件的冒泡机制，TreeView 才能在根层面统一管理选中项，这也是 `SelectedItem` 为只读的底层原因 —— 选中的源头在子节点，TreeView 只是被动接收结果。

### 5.4 核心方法

- `ExpandSubtree()`：递归展开当前节点下所有层级的子节点；
- `CollapseSubtree()`：递归折叠当前节点下所有子节点；
- 同样重写了 `GetContainerForItemOverride` 等容器生命周期方法，负责生成子层级的节点容器，实现递归渲染。

------

## 六、官方核心工作机制

### 6.1 分层数据模板递归渲染机制

这是 TreeView 数据驱动的核心，依赖 `HierarchicalDataTemplate` 分层数据模板实现。

#### 原理

普通 `DataTemplate` 只能定义单层级项外观；`HierarchicalDataTemplate` 继承自 `DataTemplate`，额外扩展了 `ItemsSource` 属性，用于指定子层级的数据源路径。渲染时框架会自动递归应用：

1. 根节点应用模板，生成节点头；
2. 根据模板的 `ItemsSource` 绑定子集合，生成子节点；
3. 子节点自动复用同一分层模板，重复上述过程，直到子集合为空。

#### 典型模板示例

xaml:

```xaml
<HierarchicalDataTemplate DataType="{x:Type local:DeviceNode}" ItemsSource="{Binding Children}">
    <TextBlock Text="{Binding NodeName}"/>
</HierarchicalDataTemplate>
```

- 无需手动写嵌套 UI，纯数据驱动自动生成完整树结构；
- 支持不同数据类型使用不同分层模板，实现混合层级渲染。

### 6.2 选中冒泡机制

TreeView 没有 Selector 基类的选中管理，选中完全由子节点驱动、冒泡传递：

plaintext:

```tex
用户点击节点 → TreeViewItem.IsSelected = true
    ↓
触发 Selected 路由事件（冒泡策略）
    ↓
事件逐层向上传递，经过所有父节点
    ↓
到达 TreeView 根节点
    ↓
更新 SelectedItem / SelectedValue 只读属性
    ↓
触发 SelectedItemChanged 公共事件
```

> 🔑 为什么 SelectedItem 是只读的？
>
> 因为选中状态的所有权在子节点，TreeView 只是冒泡结果的接收者。如果要通过代码选中节点，必须操作对应 `TreeViewItem` 的 `IsSelected` 属性，或通过数据绑定直接修改数据模型的选中字段。

### 6.3 展开折叠与懒加载机制

1. 每个节点左侧有 `ToggleButton` 展开按钮，点击切换 `IsExpanded` 属性；
2. `IsExpanded = true` 时，才会创建子节点的 UI 容器并显示；
3. `IsExpanded = false` 时，子节点容器隐藏（或虚拟化回收）；
4. 工业场景最佳实践：监听 `Expanded` 事件，展开时才异步加载子数据，初始只加载根节点，大幅提升大树的加载速度。

### 6.4 层级 UI 虚拟化原理

TreeView 支持**层级感知的 UI 虚拟化**，是千级以上节点的性能保障：

1. 只生成屏幕可见区域的节点容器，滚出屏幕的节点被回收复用；
2. 未展开的父节点，不会生成任何子节点容器；
3. 开启方式：替换 `ItemsPanel` 为 `VirtualizingStackPanel`，设置 `VirtualizingStackPanel.IsVirtualizing="True"`；
4. 效果：上千节点的树，内存占用降低 90% 以上，初始加载和滚动流畅度提升数倍。

------

## 总结

`TreeView` 是 `ItemsControl` 体系在分层数据场景的深度延伸，它没有沿用 Selector 单选体系，而是通过「节点即容器 + 事件冒泡 + 分层模板」的设计，实现了轻量且灵活的树形交互。理解 `SelectedItem` 只读的本质原因、分层模板的递归渲染逻辑、层级虚拟化的工作原理，是用好 TreeView、规避 MVVM 绑定坑点、优化大树性能的核心基础。