# 005002002_WPF `Selector` 选择基类官方源码级完整解析

`Selector` 是 WPF 所有**具备选择能力的集合控件**的抽象基类，直接继承自 `ItemsControl`，在「集合数据可视化 + 容器生命周期管理」的完整能力之上，新增了**统一选中状态模型、选择变更通知、选中值映射、集合视图同步**四大核心能力。

`ListBox`、`ComboBox`、`ListView`、`TabControl` 等工业软件高频控件全部派生自该类，是设备选择、配方下拉、报警筛选、参数配置等交互场景的底层支撑。

本文基于 .NET 官方源码，从类定义元数据、依赖属性、事件、方法、内部机制五个维度逐行深度解析，完整还原其设计逻辑与扩展规则。

------

## 一、官方类定义总览与核心元数据

### 1.1 完整抽象类签名（官方原生定义）

csharp:

```c#
namespace System.Windows.Controls.Primitives
{
    [System.Windows.LocalizabilityAttribute(System.Windows.LocalizationCategory.None, Readability = System.Windows.Readability.Unreadable)]
    public abstract class Selector : System.Windows.Controls.ItemsControl
    {
        // 静态依赖属性字段
        public static readonly System.Windows.DependencyProperty SelectedIndexProperty;
        public static readonly System.Windows.DependencyProperty SelectedItemProperty;
        public static readonly System.Windows.DependencyProperty SelectedValueProperty;
        public static readonly System.Windows.DependencyProperty SelectedValuePathProperty;
        public static readonly System.Windows.DependencyProperty IsSynchronizedWithCurrentItemProperty;
        public static readonly System.Windows.DependencyProperty IsSelectedProperty;

        // 受保护构造函数：抽象类不可直接实例化
        protected Selector();

        // 公共实例属性
        public int SelectedIndex { get; set; }
        public object SelectedItem { get; set; }
        public object SelectedValue { get; set; }
        public string SelectedValuePath { get; set; }
        public bool? IsSynchronizedWithCurrentItem { get; set; }

        // 核心事件
        public event System.Windows.Controls.SelectionChangedEventHandler SelectionChanged;

        // 静态辅助方法（附加属性读写包装）
        public static bool GetIsSelected(System.Windows.DependencyObject element);
        public static void SetIsSelected(System.Windows.DependencyObject element, bool value);
        public static int GetSelectedIndex(System.Windows.DependencyObject element);
        public static void SetSelectedIndex(System.Windows.DependencyObject element, int value);

        // 受保护扩展虚方法
        protected virtual void OnSelectionChanged(System.Windows.Controls.SelectionChangedEventArgs e);
        protected override void PrepareContainerForItemOverride(System.Windows.DependencyObject element, object item);
        protected override void ClearContainerForItemOverride(System.Windows.DependencyObject element, object item);
        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e);
        protected virtual void OnSelectedIndexChanged(System.Windows.DependencyPropertyChangedEventArgs e);
        protected virtual void OnSelectedItemChanged(System.Windows.DependencyPropertyChangedEventArgs e);
        protected virtual void OnSelectedValueChanged(System.Windows.DependencyPropertyChangedEventArgs e);
    }
}
```

### 1.2 核心元数据（官方精确值）

| 项               | 官方精确值                                                   | 工业场景关键说明                                             |
| :--------------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| **命名空间**     | `System.Windows.Controls.Primitives`                         | WPF 控件基类专属命名空间，表明这是底层基类而非直接使用的控件 |
| **程序集**       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                                           |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → Selector` | 在集合呈现能力之上，扩展选择语义                             |
| **类类型**       | 抽象类（abstract）                                           | 不能直接 `new Selector()`，仅作为基类被子类继承              |
| **核心直接子类** | `ListBox`、`ComboBox`、`TabControl`、`MultiSelector`         | 多选基类 `MultiSelector` 继承自它，`DataGrid`、`ListView` 等多选控件派生自 `MultiSelector` |
| **设计定位**     | 可选择集合控件的统一抽象                                     | 封装所有选择相关的通用逻辑，子类只需扩展交互方式与视觉样式   |
| **工业核心场景** | 设备选型、配方下拉、报警筛选、参数配置、主从详情视图         | 所有「列表选择→后续操作」的交互底层                          |

### 1.3 类级别特性解析

```c#
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
```

- 官方含义：控件本身无需要本地化的文本内容，选中项的文本由业务数据决定，不需要框架级本地化处理。
- 继承自 `ItemsControl` 的 `[ContentProperty("Items")]`、`[StyleTypedProperty]` 等特性全部保留。

------

## 二、静态依赖属性全量深度解析

所有属性均为依赖属性，完整支持数据绑定、样式、动画、继承。按功能分为**选中核心属性、同步控制属性、容器附加属性**三大类。

### 2.1 选中核心属性（5 个）

这是 `Selector` 最核心的对外接口，三个选中属性（`SelectedIndex`/`SelectedItem`/`SelectedValue`）内部自动双向同步，修改任意一个，另外两个会自动更新。

| 属性字段                                | 包装属性                        | 类型                | 默认值             | 官方作用                                                     | 底层机制与工业说明                                           |
| :-------------------------------------- | :------------------------------ | :------------------ | :----------------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| `SelectedIndexProperty`                 | `SelectedIndex`                 | `int`               | `-1`               | 当前选中项的从零开始的索引，`-1` 表示无选中                  | 1. 是最底层的选中标识，修改它会同步更新另外两个选中属性；2. 工业场景常用于自动选中第一条、滚动到指定行、按索引定位；3. 集合变更时自动维护有效性，删除选中项后自动置为 -1。 |
| `SelectedItemProperty`                  | `SelectedItem`                  | `object`            | `null`             | 当前选中的数据项对象本身                                     | 1. 主从详情场景首选，直接获取完整业务对象；2. 内部通过索引匹配集合中的数据项，引用类型按引用匹配；3. MVVM 场景最常用的绑定属性。 |
| `SelectedValueProperty`                 | `SelectedValue`                 | `object`            | `null`             | 选中项按 `SelectedValuePath` 提取的属性值                    | 1. 数据提交场景首选，轻量化传递关键字段（如 ID、编码）；2. 未设置 `SelectedValuePath` 时，值与 `SelectedItem` 完全相同；3. 支持反向赋值：设置该值会自动匹配对应属性的数据项并选中。 |
| `SelectedValuePathProperty`             | `SelectedValuePath`             | `string`            | `string.Empty`     | 指定从数据项中提取 `SelectedValue` 的属性路径                | 1. 与 `DisplayMemberPath` 配对使用：显示用 DisplayMemberPath，提交值用 SelectedValuePath；2. 底层通过反射获取对象属性值，支持嵌套路径（如 `DeviceInfo.DeviceId`）。 |
| `IsSynchronizedWithCurrentItemProperty` | `IsSynchronizedWithCurrentItem` | `bool?`（可空布尔） | `null`（自动模式） | 是否与 `ItemsSource` 对应的 `ICollectionView` 集合视图的 `CurrentItem` 保持同步 | 1. `true`：强制同步，选中项变化同步更新集合视图当前指针，反之亦然；2. `false`：完全不同步；3. `null`（默认）：自动判断，数据源为集合视图时自动同步；4. 工业价值：多控件共享同一数据源时，开启后可自动联动选中，无需额外代码。 |

> 🔑 易混点澄清：
>
> - `DisplayMemberPath`（继承自 ItemsControl）：控制**界面显示**对象的哪个属性；
> - `SelectedValuePath`：控制 `SelectedValue` 取对象的哪个属性值；
> - 二者完全独立，工业场景通常配置为：显示名称，提交 ID。

### 2.2 容器附加属性（1 个）

csharp:

```c#
public static readonly DependencyProperty IsSelectedProperty;
```

- **属性类型**：`bool`
- **附加目标**：条目容器（如 `ListBoxItem`、`ComboBoxItem`），而非 `Selector` 控件本身
- **读写方式**：通过 `Selector.GetIsSelected(element)` / `Selector.SetIsSelected(element, value)` 静态方法操作
- **官方核心作用**：
  1. 是选中状态在 UI 层的载体，`Selector` 内部通过设置该属性控制容器的选中视觉效果；
  2. 样式触发器可绑定该属性，实现选中态高亮、变色、边框等自定义效果；
  3. 所有子类的 `IsSelected` 实例属性，本质都是对该附加属性的强类型包装。
- **工业场景价值**：自定义工业列表的选中样式时，直接绑定 `Selector.IsSelected` 附加属性即可，不需要针对每个子类写不同的样式。

------

## 三、核心事件：SelectionChanged

csharp:

```c#
public event SelectionChangedEventHandler SelectionChanged;
```

### 3.1 事件签名

csharp:

```c#
public delegate void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs e);
```

### 3.2 事件参数详解

`SelectionChangedEventArgs` 继承自 `RoutedEventArgs`，包含两个核心集合：

| 属性           | 类型    | 说明                               |
| :------------- | :------ | :--------------------------------- |
| `AddedItems`   | `IList` | 本次变更中**新增选中**的数据项集合 |
| `RemovedItems` | `IList` | 本次变更中**取消选中**的数据项集合 |

> 单选模式下，`AddedItems` 和 `RemovedItems` 最多各有 1 个元素；多选模式下可包含多个。

### 3.3 触发时机

以下任意一种情况都会触发该事件：

1. 用户通过鼠标 / 键盘交互改变选中项；
2. 代码修改 `SelectedIndex` / `SelectedItem` / `SelectedValue`；
3. 绑定的数据源集合变更，导致选中项失效或变化；
4. 集合视图的 `CurrentItem` 变化（开启同步时）。

### 3.4 工业场景使用注意

1. 禁止在事件中执行耗时操作（数据库查询、PLC 通信、磁盘 IO），否则直接阻塞 UI 线程，造成界面卡顿；耗时逻辑必须异步执行。
2. MVVM 场景优先通过绑定 `SelectedItem` 响应变化，尽量不用事件，保持逻辑在 ViewModel 中可测试。
3. 初始化阶段会触发一次事件，建议增加判空保护：`if (e.AddedItems.Count == 0) return;`。

------

## 四、静态公共方法全解析

全部为附加属性的强类型包装方法，用于操作条目容器或附加属性，常规业务开发很少直接使用，自定义控件时常用。

| 方法签名                                                     | 官方作用                               | 典型使用场景                           |
| :----------------------------------------------------------- | :------------------------------------- | :------------------------------------- |
| `public static bool GetIsSelected(DependencyObject element)` | 读取指定元素的 `IsSelected` 附加属性值 | 视觉树遍历中，判断某个容器是否被选中   |
| `public static void SetIsSelected(DependencyObject element, bool value)` | 设置指定元素的 `IsSelected` 附加属性   | 自定义控件中，通过代码修改容器选中状态 |
| `public static int GetSelectedIndex(DependencyObject element)` | 读取附加在元素上的选中索引             | 极少直接使用，一般通过实例属性访问     |
| `public static void SetSelectedIndex(DependencyObject element, int value)` | 设置附加在元素上的选中索引             | 内部机制使用，业务代码不建议调用       |

------

## 五、受保护虚方法全解析（扩展核心）

这是自定义选择控件的核心扩展点，`Selector` 在 `ItemsControl` 的基础上，重写了容器生命周期方法，新增了选择变更、属性变更的回调入口。

### 5.1 选择变更核心入口

csharp:

```c#
protected virtual void OnSelectionChanged(SelectionChangedEventArgs e);
```

- **官方默认实现**：触发 `SelectionChanged` 路由事件。
- **子类重写场景**：
  1. 选中变化时执行自定义逻辑，如联动其他控件、刷新关联数据；
  2. 选中前做校验，不符合条件则取消选择；
  3. 埋点统计、日志记录。
- **注意事项**：重写时必须调用 `base.OnSelectionChanged(e)`，否则 `SelectionChanged` 事件不会触发。

### 5.2 容器生命周期重写（虚拟化适配核心）

`Selector` 重写了 `ItemsControl` 的两个容器生命周期方法，注入选中状态管理，是虚拟化下选中状态不丢失的核心保障。

#### 1. PrepareContainerForItemOverride

csharp:

```c#
protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
```

- **官方执行逻辑**：
  1. 先调用基类方法，完成数据上下文、样式、模板的基础准备；
  2. 根据数据项的选中状态，设置容器的 `IsSelected` 附加属性，同步视觉效果；
  3. 从 `IContainItemStorage` 中读取该数据项的持久化选中状态，恢复到容器上。
- **子类扩展**：重写时可附加自定义选中相关的绑定、事件。

#### 2. ClearContainerForItemOverride

csharp:

```c#
protected override void ClearContainerForItemOverride(DependencyObject element, object item);
```

- **官方执行逻辑**：
  1. 容器回收前，清除容器的 `IsSelected` 状态，避免复用后状态残留；
  2. 将当前选中状态持久化到 `IContainItemStorage`，绑定到数据项上；
  3. 调用基类方法，清理基础数据上下文。
- ⚠️ **工业大坑**：自定义子类如果扩展了选中相关属性，必须在此方法中对应清理并持久化，否则虚拟化滚动时会出现**状态错乱、选中丢失、事件重复触发**。

### 5.3 集合变更处理

csharp:

```c#
protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e);
```

- **官方扩展逻辑**：在基类增量更新容器的基础上，同步维护选中状态的有效性：
  1. 添加项：不改变当前选中；
  2. 删除项：如果删除的是选中项，将 `SelectedIndex` 置为 -1；
  3. 重置（Clear）：清空所有选中状态；
  4. 移动项：同步更新选中索引。

### 5.4 属性变更回调方法

三个选中属性各自对应一个受保护虚方法，属性变更时自动调用，子类可重写响应变化：

| 方法签名                                                     | 触发时机                     |
| :----------------------------------------------------------- | :--------------------------- |
| `protected virtual void OnSelectedIndexChanged(DependencyPropertyChangedEventArgs e)` | `SelectedIndex` 属性值变化时 |
| `protected virtual void OnSelectedItemChanged(DependencyPropertyChangedEventArgs e)` | `SelectedItem` 属性值变化时  |
| `protected virtual void OnSelectedValueChanged(DependencyPropertyChangedEventArgs e)` | `SelectedValue` 属性值变化时 |

> 设计思想：每个属性都提供独立的重写入口，子类不需要自己注册依赖属性变更回调，直接重写对应方法即可，代码更清晰、扩展性更强。

------

## 六、官方核心工作机制

### 6.1 选中状态的双层映射模型

`Selector` 的核心设计是**数据层与 UI 层分离**，选中状态本质绑定在数据项上，而非 UI 容器上，这是它支持虚拟化的根本原因。

| 层级       | 载体                                               | 作用                       | 生命周期                           |
| :--------- | :------------------------------------------------- | :------------------------- | :--------------------------------- |
| **数据层** | `SelectedIndex` / `SelectedItem` / `SelectedValue` | 面向业务逻辑，持久有效     | 与数据集合同生命周期，始终存在     |
| **UI 层**  | 容器的 `IsSelected` 附加属性                       | 面向视觉呈现，仅可见项存在 | 与容器同生命周期，虚拟化回收时销毁 |

**同步规则**：

- 数据层变化 → 自动同步到所有可见容器；
- 容器生成（滚入屏幕）→ 自动从数据层恢复选中状态；
- 用户点击容器 → 更新数据层 → 同步到其他容器。

### 6.2 三大选中属性的同步逻辑

`SelectedIndex`、`SelectedItem`、`SelectedValue` 三者通过依赖属性的变更回调实现双向自动同步：

1. 修改 `SelectedIndex` → 从集合取对应索引的数据项赋值给 `SelectedItem` → 按 `SelectedValuePath` 提取值赋值给 `SelectedValue`；
2. 修改 `SelectedItem` → 计算索引赋值给 `SelectedIndex` → 提取值赋值给 `SelectedValue`；
3. 修改 `SelectedValue` → 遍历集合匹配对应属性值的项赋值给 `SelectedItem` → 同步索引。

### 6.3 虚拟化下的状态持久化

配合 `ItemsControl` 的 `IContainItemStorage` 接口实现：

1. 容器滚出屏幕被回收前，将该数据项的 `IsSelected` 状态存入存储接口，与数据项绑定；
2. 新数据滚入屏幕、容器复用时，从存储接口读取对应数据项的选中状态，恢复到容器上；
3. **效果**：无论滚动多远、数据量多大，选中状态始终准确，不会因为虚拟化回收而丢失或错乱。

- 工业价值：上万条的报警历史、生产记录列表，开启虚拟化后选中功能依然稳定，是长列表性能优化的基础保障。

### 6.4 选择变更完整执行流程

以用户点击选中某行为例：

1. 条目容器接收鼠标点击，触发内部选中逻辑；
2. 更新 `SelectedIndex` 依赖属性；
3. 属性变更回调触发，同步更新 `SelectedItem` 和 `SelectedValue`；
4. 遍历所有可见容器，旧选中项取消 `IsSelected`，新选中项设置 `IsSelected`；
5. 若开启集合视图同步，更新 `ICollectionView.CurrentItem`；
6. 调用 `OnSelectionChanged` 虚方法，触发 `SelectionChanged` 路由事件。

### 6.5 集合视图同步机制

当 `IsSynchronizedWithCurrentItem` 为 `true` 时：

- `Selector` 选中项变化 → 同步设置集合视图的 `CurrentItem`；
- 集合视图的 `CurrentItem` 变化 → 同步选中对应的数据项；
- 典型应用：多个控件绑定同一个 `CollectionViewSource`，自动实现选中联动，无需写任何同步代码。

------

## 七、核心设计思想与工业场景适配要点

### 7.1 核心设计思想

1. **数据驱动选择**：选中状态是数据的属性，不是 UI 的属性，UI 只是呈现，这是支持虚拟化、支持 MVVM 的核心基础。
2. **单一职责**：基类只做选择状态管理，不关心选择交互方式（单击、双击、框选），由子类实现具体交互。
3. **全扩展点开放**：所有关键节点都提供受保护虚方法，子类可以按需重写，不需要修改基类逻辑。

### 7.2 工业场景最佳实践

1. **绑定方式按需选择**：
   - 主从详情、需要完整对象 → 绑定 `SelectedItem`；
   - 下拉提交、只需要关键字段 → 绑定 `SelectedValue` + `SelectedValuePath`。
2. **选中样式纯样式实现**：通过 `ItemContainerStyle` 绑定 `IsSelected` 触发器，不要在数据模型中加 `IsSelected` 属性，保持数据与 UI 分离。
3. **长列表放心开虚拟化**：选中状态由底层持久化存储，不会因为滚动丢失，大数据量必须开启 `VirtualizingStackPanel`。
4. **永远操作数据层**：不要直接操作 UI 容器的选中状态，始终通过修改 `SelectedItem`/`SelectedIndex` 实现，虚拟化下操作容器会失效。

### 7.3 常见坑点

1. **SelectedItem 绑定不更新**：通常是数据项引用不匹配，不是集合中的同一个对象实例，或未实现 `Equals` 方法。
2. **SelectionChanged 多次触发**：初始化、集合重置都会触发，务必增加判空和防抖逻辑。
3. **虚拟化下遍历容器找不到选中项**：不可见的项没有生成容器，永远通过数据层操作，不要遍历视觉树。

------

## 总结

`Selector` 是 WPF 集合控件体系中承上启下的关键抽象：向下完整继承 `ItemsControl` 的集合呈现与容器管理能力，向上为所有可选择控件提供了统一、一致的选中模型。理解它的双层状态映射、虚拟化持久化、属性同步机制，不仅能正确使用 `ListBox`/`ComboBox` 等常用控件，也是自定义工业专用选择列表、规避虚拟化陷阱的必备基础。