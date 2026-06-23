# 005002003_WPF `Selector` 选择基类源码级完整定义深度解析

**源码：**

```c#
public abstract class Selector : ItemsControl
{
    public static readonly RoutedEvent SelectionChangedEvent;
    public static readonly RoutedEvent SelectedEvent;
    public static readonly RoutedEvent UnselectedEvent;
    public static readonly DependencyProperty IsSelectionActiveProperty;
    public static readonly DependencyProperty IsSelectedProperty;
    public static readonly DependencyProperty IsSynchronizedWithCurrentItemProperty;
    public static readonly DependencyProperty SelectedIndexProperty;
    public static readonly DependencyProperty SelectedItemProperty;
    public static readonly DependencyProperty SelectedValueProperty;
    public static readonly DependencyProperty SelectedValuePathProperty;
 
    protected Selector();
 
    public object SelectedValue { get; set; }
    public object SelectedItem { get; set; }
    public int SelectedIndex { get; set; }
    public bool? IsSynchronizedWithCurrentItem { get; set; }
    public string SelectedValuePath { get; set; }
 
    public event SelectionChangedEventHandler SelectionChanged;
 
    public static void AddSelectedHandler(DependencyObject element, RoutedEventHandler handler);
    public static void AddUnselectedHandler(DependencyObject element, RoutedEventHandler handler);
    public static bool GetIsSelected(DependencyObject element);
    public static bool GetIsSelectionActive(DependencyObject element);
    public static void RemoveSelectedHandler(DependencyObject element, RoutedEventHandler handler);
    public static void RemoveUnselectedHandler(DependencyObject element, RoutedEventHandler handler);
    public static void SetIsSelected(DependencyObject element, bool isSelected);
    protected override void ClearContainerForItemOverride(DependencyObject element, object item);
    protected override void OnInitialized(EventArgs e);
    protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e);
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e);
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue);
    protected virtual void OnSelectionChanged(SelectionChangedEventArgs e);
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
 
}
```

你提供的是更贴近 .NET 官方底层源码的完整定义，相比公开接口版补充了**路由事件字段、容器级附加事件、焦点激活状态属性、完整生命周期重写方法**等核心底层机制，完整还原了 `Selector` 「选择状态管理 + 焦点联动 + 事件路由 + 虚拟化兼容」的完整设计体系。

本文严格对照你列出的每一个成员，从底层机制、官方作用、工业场景价值三个维度逐行解析。

------

## 一、完整类定义与核心元数据

### 1.1 官方完整类签名

csharp：

```c#
namespace System.Windows.Controls.Primitives
{
    [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
    public abstract class Selector : ItemsControl
    {
        // 全部成员见你提供的代码，下文逐模块解析
    }
}
```

### 1.2 核心元数据

| 项           | 官方精确值                                                   | 说明                                                         |
| :----------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| 命名空间     | `System.Windows.Controls.Primitives`                         | WPF 底层控件基类专属命名空间                                 |
| 完整继承链   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → Selector` | 在集合呈现能力之上，扩展完整的选择语义                       |
| 类类型       | 抽象类                                                       | 不可直接实例化，仅作为基类被子类继承                         |
| 核心直接子类 | `ListBox`、`ComboBox`、`TabControl`、`MultiSelector`         | `ListView`、`DataGrid` 等多选控件继承自其子类 `MultiSelector` |
| 设计定位     | 可选择集合控件的统一抽象                                     | 封装所有选择相关通用逻辑，子类仅需实现具体交互方式与样式     |

### 1.3 本次定义的核心增量

相比公开接口版，这份源码级定义补充了 4 类关键底层成员：

1. 3 个路由事件字段（含 2 个容器级附加事件）
2. `IsSelectionActive` 焦点激活附加属性
3. 焦点变化、初始化、数据源更换等完整生命周期重写方法
4. 附加事件的静态注册 / 注销方法

------

## 二、路由事件体系全解析

这是本次定义最核心的增量部分。`Selector` 采用**两级路由事件机制**：容器级单条选中事件 + 控件级整体变更事件，既支持细粒度的单条交互，也支持整体选择变更通知。

### 2.1 三个路由事件字段

| 静态字段                | 事件类型           | 路由策略       | 官方作用                                   |
| :---------------------- | :----------------- | :------------- | :----------------------------------------- |
| `SelectionChangedEvent` | 控件级路由事件     | Bubble（冒泡） | 选中项整体发生变更时触发，是对外的主要事件 |
| `SelectedEvent`         | 容器级附加路由事件 | Bubble（冒泡） | 单个条目容器被选中时，在该容器上触发       |
| `UnselectedEvent`       | 容器级附加路由事件 | Bubble（冒泡） | 单个条目容器被取消选中时，在该容器上触发   |

#### 1. SelectionChangedEvent

- 对应包装事件：`public event SelectionChangedEventHandler SelectionChanged`
- 触发时机：任意选中项变化（新增选中、取消选中）都会触发，事件参数包含 `AddedItems` 和 `RemovedItems` 两个集合。
- 工业场景：用于整体选中变更后的联动逻辑，如更新按钮状态、刷新详情面板。

#### 2. SelectedEvent / UnselectedEvent

- **附加路由事件**：不是定义在 `Selector` 控件上，而是附加到每个条目容器（如 `ListBoxItem`）上的事件。
- 触发时机：每个容器被选中 / 取消选中的瞬间，在该容器元素上独立触发。
- 核心价值：
  1. 支持单条粒度的交互响应，如选中时播放动画、高亮特效；
  2. 事件冒泡向上传递，可在父级统一监听所有条目的选中状态变化；
  3. 自定义容器控件时，可重写该事件的默认处理逻辑。

### 2.2 附加事件静态方法

对应 `SelectedEvent` / `UnselectedEvent` 两个附加事件，提供了标准的附加事件访问器：

| 方法签名                                                     | 官方作用                       | 典型应用                                 |
| :----------------------------------------------------------- | :----------------------------- | :--------------------------------------- |
| `AddSelectedHandler(DependencyObject element, RoutedEventHandler handler)` | 给指定元素注册选中事件监听     | 自定义容器中注册选中回调，执行自定义动画 |
| `AddUnselectedHandler(DependencyObject element, RoutedEventHandler handler)` | 给指定元素注册取消选中事件监听 | 取消选中时回收资源、停止动画             |
| `RemoveSelectedHandler(DependencyObject element, RoutedEventHandler handler)` | 移除选中事件监听               | 容器回收时解绑事件，避免内存泄漏         |
| `RemoveUnselectedHandler(DependencyObject element, RoutedEventHandler handler)` | 移除取消选中事件监听           | 虚拟化回收时清理事件绑定                 |

> 🔑 设计意图：通过附加事件机制，将「单条选中通知」能力附加到任意容器元素上，不强制要求容器继承特定基类，保持了 `ItemsControl` 容器模型的灵活性。

------

## 三、依赖属性全量深度解析

共 8 个静态依赖属性，分为**状态附加属性、选中核心属性、同步控制属性**三类。

### 3.1 状态附加属性（2 个）

#### 1. IsSelectionActiveProperty

csharp：

```c#
public static readonly DependencyProperty IsSelectionActiveProperty;
```

- **属性类型**：`bool`，只读附加属性（内部可写，外部只读）

- **附加目标**：`Selector` 控件本身

- **官方作用**：指示选择器是否处于**激活状态**—— 当控件拥有键盘焦点时为 `true`，失去键盘焦点时为 `false`。

- **视觉表现**：

  - 激活状态（有焦点）：选中项为高亮主题色（如系统蓝色）；
  - 未激活状态（失焦）：选中项为灰色，表明选中状态存在但控件未获得焦点。

- **底层触发**：由 `OnIsKeyboardFocusWithinChanged` 方法自动更新。

- **工业场景价值**：

  自定义工业列表样式时，可通过触发器绑定该属性，区分「焦点内选中」和「失焦选中」的视觉效果，符合工业软件多控件联动的操作习惯。

- **读取方式**：`Selector.GetIsSelectionActive(element)`

#### 2. IsSelectedProperty

csharp:

```c#
public static readonly DependencyProperty IsSelectedProperty;
```

- **属性类型**：`bool`，附加属性
- **附加目标**：每个条目容器
- **官方作用**：标记单个容器是否被选中，是选中状态在 UI 层的唯一载体。
- **读写方式**：`Selector.GetIsSelected(element)` / `Selector.SetIsSelected(element, value)`
- **核心机制**：
  - `Selector` 内部通过设置该属性控制容器选中态；
  - 样式触发器绑定该属性实现选中视觉效果；
  - 虚拟化回收时，状态持久化到 `IContainItemStorage`，复用时恢复。

### 3.2 选中核心属性（4 个）

| 属性字段                    | 包装属性            | 类型     | 默认值         | 底层同步机制                                            |
| :-------------------------- | :------------------ | :------- | :------------- | :------------------------------------------------------ |
| `SelectedIndexProperty`     | `SelectedIndex`     | `int`    | `-1`           | 最底层选中标识，修改它会同步更新另外两个选中属性        |
| `SelectedItemProperty`      | `SelectedItem`      | `object` | `null`         | 与索引双向同步，引用类型按引用匹配                      |
| `SelectedValueProperty`     | `SelectedValue`     | `object` | `null`         | 按 `SelectedValuePath` 反射提取属性值，支持反向匹配选中 |
| `SelectedValuePathProperty` | `SelectedValuePath` | `string` | `string.Empty` | 指定 `SelectedValue` 的提取路径，支持嵌套属性           |

> 三者自动双向同步：修改任意一个，另外两个会通过依赖属性变更回调自动更新，始终保持一致。

### 3.3 同步控制属性（1 个）

#### IsSynchronizedWithCurrentItemProperty

csharp:

```c#
public static readonly DependencyProperty IsSynchronizedWithCurrentItemProperty;
```

- 包装属性：`IsSynchronizedWithCurrentItem`，类型 `bool?`（可空布尔）
- 默认值：`null`（自动模式）
- 官方作用：控制是否与 `ItemsSource` 对应的 `ICollectionView` 集合视图的 `CurrentItem` 保持同步。
- 取值说明：
  - `true`：强制双向同步，选中项变化同步更新集合视图当前指针，反之亦然；
  - `false`：完全不同步；
  - `null`（默认）：自动判断，数据源为集合视图时自动开启同步。
- 工业价值：多控件共享同一数据源时，开启后可自动联动选中，无需额外同步代码。

------

## 四、公共属性与事件

### 4.1 公共实例属性

| 属性                            | 类型     | 工业场景最佳实践                               |
| :------------------------------ | :------- | :--------------------------------------------- |
| `SelectedIndex`                 | `int`    | 用于按索引定位、自动选中第一条、滚动到指定行   |
| `SelectedItem`                  | `object` | 主从详情场景首选，直接获取完整业务对象         |
| `SelectedValue`                 | `object` | 数据提交场景首选，轻量化传递 ID / 编码         |
| `SelectedValuePath`             | `string` | 与 `DisplayMemberPath` 配对：显示名称，提交 ID |
| `IsSynchronizedWithCurrentItem` | `bool?`  | 多控件联动时设为 `true`，共享集合视图当前项    |

### 4.2 核心公共事件

csharp:

```c#
public event SelectionChangedEventHandler SelectionChanged;
```

- 是 `SelectionChangedEvent` 路由事件的 CLR 包装事件；
- 事件参数 `SelectionChangedEventArgs` 包含 `AddedItems`（新增选中）和 `RemovedItems`（取消选中）；
- 单选模式下两个集合各最多 1 个元素，多选模式下可包含多个。

------

## 五、受保护重写方法全量解析

共 7 个核心重写方法，覆盖了**初始化、焦点变化、数据源更换、集合变更、容器生命周期、选中变更**完整生命周期，是 `Selector` 所有能力的底层执行入口。

### 1. OnInitialized

csharp:

```c#
protected override void OnInitialized(EventArgs e);
```

- **触发时机**：控件初始化完成时调用。
- **官方执行逻辑**：
  1. 调用基类初始化方法；
  2. 初始化选中状态，验证 `SelectedIndex` / `SelectedItem` 的有效性；
  3. 绑定初始集合视图的同步关系。
- **子类扩展**：可重写执行自定义初始化逻辑，如设置默认选中项。

### 2. OnIsKeyboardFocusWithinChanged

csharp:

```c#
protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e);
```

- **触发时机**：控件内部键盘焦点状态变化时触发。
- **官方核心作用**：更新 `IsSelectionActive` 附加属性的值：
  - 控件获得键盘焦点 → `IsSelectionActive = true` → 选中项高亮显示；
  - 控件失去键盘焦点 → `IsSelectionActive = false` → 选中项灰色显示。
- **底层价值**：这是 WPF 列表「失焦选中变灰」效果的底层实现，所有派生控件默认继承该行为。

### 3. OnItemsSourceChanged

csharp:

```c#
protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue);
```

- **触发时机**：`ItemsSource` 属性值更换时触发。
- **官方执行逻辑**：
  1. 解绑旧数据源对应的集合视图事件，移除 `CurrentItem` 同步监听；
  2. 重置当前选中状态（`SelectedIndex` 置为 -1）；
  3. 绑定新数据源的集合视图，若开启同步则注册 `CurrentItem` 变更事件；
  4. 调用基类方法，触发容器重新生成。
- **工业大坑提醒**：运行中更换 `ItemsSource` 会清空选中状态，若需保留选中需自行处理。

### 4. OnItemsChanged

csharp:

```c#
protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e);
```

- **触发时机**：集合内容发生增、删、改、重置时触发。
- **官方执行逻辑**：
  - **添加项**：不改变当前选中，仅增量生成容器；
  - **删除项**：如果删除的是选中项，自动将 `SelectedIndex` 置为 -1 或调整到相邻项；
  - **重置（Clear）**：清空所有选中状态；
  - **移动项**：同步更新选中索引的位置。
- **核心价值**：保证选中状态在集合动态变化时始终有效，不会出现索引越界、选中项丢失等异常。

### 5. PrepareContainerForItemOverride

csharp:

```c#
protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
```

- **触发时机**：条目容器生成 / 复用时调用。
- **官方扩展逻辑**（在基类基础上增加）：
  1. 从持久化存储中读取该数据项的 `IsSelected` 状态，同步到容器附加属性；
  2. 给容器附加 `Selected` / `Unselected` 事件监听；
  3. 同步 `AlternationIndex` 等其他附加属性。
- **虚拟化适配**：容器复用时自动恢复对应数据项的选中状态，保证滚动时选中不丢失、不错乱。

### 6. ClearContainerForItemOverride

csharp:

```c#
protected override void ClearContainerForItemOverride(DependencyObject element, object item);
```

- **触发时机**：容器滚出屏幕、进入回收池时调用。
- **官方扩展逻辑**（在基类基础上增加）：
  1. 将容器当前的 `IsSelected` 状态持久化到 `IContainItemStorage`，绑定到数据项；
  2. 移除容器上的 `Selected` / `Unselected` 事件监听，避免内存泄漏；
  3. 清除容器的 `IsSelected` 附加属性值，防止复用时状态残留。
- ⚠️ **工业场景必知**：自定义子类如果扩展了选中相关属性 / 事件，必须在此方法中对应清理并持久化，否则虚拟化滚动必然出现状态错乱、内存泄漏。

### 7. OnSelectionChanged

csharp:

```c#
protected virtual void OnSelectionChanged(SelectionChangedEventArgs e);
```

- **触发时机**：选中状态发生变更时调用。
- **官方默认实现**：
  1. 更新 `IsSelectionActive` 状态；
  2. 同步集合视图的 `CurrentItem`（开启同步时）；
  3. 触发 `SelectionChanged` 路由事件。
- **子类扩展**：
  - 重写可注入自定义校验逻辑，不符合条件可取消选中变更；
  - 可添加埋点、日志、联动逻辑。
- **注意**：重写必须调用 `base.OnSelectionChanged(e)`，否则路由事件不会触发。

------

## 六、核心底层工作机制补充

### 6.1 两级选中事件机制

| 层级   | 事件                      | 触发粒度        | 典型用途                     |
| :----- | :------------------------ | :-------------- | :--------------------------- |
| 容器级 | `Selected` / `Unselected` | 单条选中 / 取消 | 单条动画、特效、细粒度交互   |
| 控件级 | `SelectionChanged`        | 整体选中变更    | 业务联动、状态更新、详情刷新 |

两者配合，既满足了细粒度 UI 交互需求，也保证了业务层的简洁性。

### 6.2 焦点 - 选中态联动机制

1. 用户点击列表 → 控件获得键盘焦点 → `OnIsKeyboardFocusWithinChanged` 触发 → `IsSelectionActive = true` → 选中项高亮；
2. 用户点击其他输入框 → 控件失去焦点 → `IsSelectionActive = false` → 选中项变灰；
3. 选中状态本身不会丢失，只是视觉呈现变化，符合桌面端交互直觉。

### 6.3 虚拟化下的状态一致性保障

1. 容器滚出屏幕 → `ClearContainerForItemOverride` 将选中状态持久化到数据项存储；
2. 数据项滚入屏幕 → `PrepareContainerForItemOverride` 从存储中恢复状态到新容器；
3. 效果：无论滚动多快、数据量多大，选中状态始终绑定在数据上，不会因为 UI 容器的销毁和复用出现错乱。

### 6.4 集合视图同步机制

当 `IsSynchronizedWithCurrentItem="True"` 时：

- `Selector` 选中变更 → 自动移动 `ICollectionView` 的 `CurrentItem` 指针；
- 集合视图 `CurrentItem` 变化 → 自动同步选中对应的数据项；
- 多控件绑定同一 `CollectionViewSource` 时，天然实现选中联动，无需额外代码。

------

## 七、工业场景典型应用示例

### 示例 1：自定义激活 / 失焦选中样式

通过 `IsSelectionActive` 触发器区分焦点状态，适配工业多控件联动场景：

xsml:

```xaml
<Style TargetType="ListBoxItem">
    <Setter Property="Background" Value="Transparent"/>
    <Style.Triggers>
        <!-- 选中且有焦点：深蓝色背景 -->
        <MultiTrigger>
            <MultiTrigger.Conditions>
                <Condition Property="Selector.IsSelected" Value="True"/>
                <Condition Property="Selector.IsSelectionActive" Value="True"/>
            </MultiTrigger.Conditions>
            <Setter Property="Background" Value="#1677FF"/>
            <Setter Property="Foreground" Value="White"/>
        </MultiTrigger>
        
        <!-- 选中但失焦：浅灰色背景 -->
        <MultiTrigger>
            <MultiTrigger.Conditions>
                <Condition Property="Selector.IsSelected" Value="True"/>
                <Condition Property="Selector.IsSelectionActive" Value="False"/>
            </MultiTrigger.Conditions>
            <Setter Property="Background" Value="#E5E7EB"/>
            <Setter Property="Foreground" Value="#333"/>
        </MultiTrigger>
    </Style.Triggers>
</Style>
```

### 示例 2：容器级选中动画

订阅 `Selected` 附加事件，实现选中时的高亮动画，适合报警列表的选中强调效果。

------

## 总结

这份源码级定义完整展现了 `Selector` 的设计全貌：它不仅提供了常用的选中属性与事件，还通过**附加属性、附加事件、生命周期重写**构建了一套完整的「状态管理 + 焦点联动 + 虚拟化兼容」体系。所有派生控件（ListBox/ComboBox 等）都共享这套底层机制，理解这些底层成员，不仅能精准使用现有控件，也是自定义工业专用选择列表、规避虚拟化陷阱的核心基础。