# 005001002_WPF `ItemsControl` 基类官方源代码级完整解析

`ItemsControl` 是 WPF 所有集合呈现控件的**核心基类**，`ListBox`/`ListView`/`ComboBox`/`DataGrid` 等所有列表类控件全部继承自它。它的核心职责是 **「数据集合驱动 UI 生成 + 条目容器管理 + 布局面板承载」**，本身不包含选择、编辑等高级功能，只专注于集合数据到可视化元素的映射与呈现，是工业软件中报警列表、设备台账、生产数据表格等场景的底层支撑。

本文基于微软官方 .NET 8 源码，从类定义、特性契约、静态成员、实例属性、核心方法、内部机制六个维度做逐行深度解析，延续此前的控件解析体系。

------

## 一、官方类定义总览与核心元数据

### 1.1 完整类签名（含所有官方特性）

csharp：

```c#
namespace System.Windows.Controls
{
    [System.Windows.LocalizabilityAttribute(System.Windows.LocalizationCategory.None, Readability = System.Windows.Readability.Unreadable)]
    [System.Windows.Markup.ContentProperty("Items")]
    [System.Windows.StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(System.Windows.FrameworkElement))]
    public class ItemsControl : System.Windows.Controls.Control, IAddChild
    {
        // 静态依赖属性
        public static readonly DependencyProperty AlternationCountProperty;
        public static readonly DependencyProperty DisplayMemberPathProperty;
        public static readonly DependencyProperty HasItemsProperty;
        public static readonly DependencyProperty IsGroupingProperty;
        public static readonly DependencyProperty IsTextSearchEnabledProperty;
        public static readonly DependencyProperty ItemContainerStyleProperty;
        public static readonly DependencyProperty ItemContainerStyleSelectorProperty;
        public static readonly DependencyProperty ItemsPanelProperty;
        public static readonly DependencyProperty ItemsSourceProperty;
        public static readonly DependencyProperty ItemStringFormatProperty;
        public static readonly DependencyProperty ItemTemplateProperty;
        public static readonly DependencyProperty ItemTemplateSelectorProperty;

        // 构造函数
        public ItemsControl();

        // 公共属性
        public int AlternationCount { get; set; }
        public string DisplayMemberPath { get; set; }
        public bool HasItems { get; }
        public bool IsGrouping { get; }
        public bool IsTextSearchEnabled { get; set; }
        public Style ItemContainerStyle { get; set; }
        public StyleSelector ItemContainerStyleSelector { get; set; }
        public ItemCollection Items { get; }
        public ItemsPanelTemplate ItemsPanel { get; set; }
        public IEnumerable ItemsSource { get; set; }
        public string ItemStringFormat { get; set; }
        public DataTemplate ItemTemplate { get; set; }
        public DataTemplateSelector ItemTemplateSelector { get; set; }
        public ItemContainerGenerator ItemContainerGenerator { get; }

        // 公共事件
        public event EventHandler StatusChanged;

        // 公共方法
        public static ItemsControl GetItemsOwner(DependencyObject element);
        public override void OnApplyTemplate();

        // 受保护虚方法（核心扩展点）
        protected virtual DependencyObject GetContainerForItemOverride();
        protected virtual bool IsItemItsOwnContainerOverride(object item);
        protected virtual void PrepareContainerForItemOverride(DependencyObject element, object item);
        protected virtual void ClearContainerForItemOverride(DependencyObject element, object item);
        protected virtual void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue);
        protected virtual void OnItemsChanged(NotifyCollectionChangedEventArgs e);
        protected override AutomationPeer OnCreateAutomationPeer();
    }
}
```

### 1.2 核心元数据（官方精确值）

| 项               | 官方精确值                                                   | 工业场景关键说明                                          |
| :--------------- | :----------------------------------------------------------- | :-------------------------------------------------------- |
| **命名空间**     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                                      |
| **程序集**       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                                        |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl` | 直接继承自 `Control`，是所有集合控件的抽象基类            |
| **核心契约**     | `IAddChild` 接口                                             | 支持 XAML 直接添加子元素，对应 `Items` 集合               |
| **设计定位**     | 集合数据的可视化呈现基类                                     | 只负责「数据→UI」的映射与排列，不包含选择、编辑等业务逻辑 |
| **核心子类**     | `Selector`（选择列表基类）、`Menu`、`StatusBar`、`DataGrid`  | 工业常用的 ListBox/ListView/ComboBox 均继承自 `Selector`  |
| **工业核心应用** | 报警列表、设备台账、生产数据列表、配方管理、日志展示         | 所有批量数据展示场景的底层支撑                            |

### 1.3 类级特性深度解析

1. **`[ContentProperty("Items")]`**

   - 官方含义：指定 `Items` 为 XAML 默认内容属性，因此在 XAML 中直接写子元素时，会自动添加到 `Items` 集合中。

   - 示例：

     xaml:

     ```xaml
     <ItemsControl>
         <TextBlock>条目1</TextBlock>
         <TextBlock>条目2</TextBlock>
     </ItemsControl>
     ```

     等价于手动添加到 `Items`集合。

2. **`[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(FrameworkElement))]`**

   - 官方含义：声明 `ItemContainerStyle` 属性对应的样式目标类型为 `FrameworkElement`，让样式编辑器和 XAML 设计器能正确识别目标类型。
   - 子类会重写该特性，例如 `ListBox` 将目标类型指定为 `ListBoxItem`。

3. **`[Localizability(LocalizationCategory.None)]`**

   - 官方含义：控件本身无需要本地化的文本内容，条目内容由业务数据决定。

------

## 二、静态依赖属性逐行解析

所有属性均为依赖属性，完整支持数据绑定、样式、动画、继承等 WPF 核心特性。按功能可分为四大类：

### 2.1 数据集合类

| 属性字段              | 包装属性      | 类型          | 默认值  | 官方作用                       | 工业最佳实践                                                 |
| :-------------------- | :------------ | :------------ | :------ | :----------------------------- | :----------------------------------------------------------- |
| `ItemsSourceProperty` | `ItemsSource` | `IEnumerable` | `null`  | 绑定外部数据集合，驱动 UI 生成 | **MVVM 唯一首选**，绑定 `ObservableCollection<T>` 实现集合变更自动同步 UI |
| `HasItemsProperty`    | `HasItems`    | `bool`        | `false` | 只读属性，指示集合是否包含数据 | 用于空状态提示的触发器，列表为空时显示「暂无数据」           |
| `IsGroupingProperty`  | `IsGrouping`  | `bool`        | `false` | 只读属性，指示是否启用了分组   | 生产数据按班次、批次分组展示时使用                           |

> ⚠️ 关键规则：设置了 `ItemsSource` 后，`Items` 集合自动变为只读，不能再手动添加删除元素；二者只能二选一，不能混用。

### 2.2 条目呈现类

| 属性字段                       | 包装属性               | 类型                   | 默认值         | 官方作用                   | 工业最佳实践                                                |
| :----------------------------- | :--------------------- | :--------------------- | :------------- | :------------------------- | :---------------------------------------------------------- |
| `ItemTemplateProperty`         | `ItemTemplate`         | `DataTemplate`         | `null`         | 定义每个数据项的可视化外观 | 列表自定义布局的核心，用数据模板将业务对象转为可视化界面    |
| `ItemTemplateSelectorProperty` | `ItemTemplateSelector` | `DataTemplateSelector` | `null`         | 根据数据动态选择不同模板   | 不同类型的报警、不同状态的设备使用不同显示样式              |
| `DisplayMemberPathProperty`    | `DisplayMemberPath`    | `string`               | `string.Empty` | 指定显示对象的哪个属性     | 简单列表只需显示单个字段时使用，无需写完整 DataTemplate     |
| `ItemStringFormatProperty`     | `ItemStringFormat`     | `string`               | `null`         | 条目内容的字符串格式化格式 | 数值、日期统一格式化显示，如温度保留 1 位小数、日期固定格式 |

### 2.3 容器样式类

| 属性字段                             | 包装属性                     | 类型            | 默认值 | 官方作用                 | 工业最佳实践                                             |
| :----------------------------------- | :--------------------------- | :-------------- | :----- | :----------------------- | :------------------------------------------------------- |
| `ItemContainerStyleProperty`         | `ItemContainerStyle`         | `Style`         | `null` | 定义每个条目容器的样式   | 控制条目高度、边距、选中效果、交替行背景色，工业列表常用 |
| `ItemContainerStyleSelectorProperty` | `ItemContainerStyleSelector` | `StyleSelector` | `null` | 根据数据动态选择容器样式 | 异常数据行高亮、报警行标红等动态样式场景                 |

### 2.4 布局与交互类

| 属性字段                      | 包装属性              | 类型                 | 默认值            | 官方作用                               | 工业最佳实践                                               |
| :---------------------------- | :-------------------- | :------------------- | :---------------- | :------------------------------------- | :--------------------------------------------------------- |
| `ItemsPanelProperty`          | `ItemsPanel`          | `ItemsPanelTemplate` | 垂直 `StackPanel` | 定义所有条目容器的布局面板             | 大数据量必须替换为 `VirtualizingStackPanel` 开启 UI 虚拟化 |
| `AlternationCountProperty`    | `AlternationCount`    | `int`                | `0`               | 交替行计数，实现奇偶行交替样式         | 工业数据列表标配，奇偶行不同背景色，提升长列表可读性       |
| `IsTextSearchEnabledProperty` | `IsTextSearchEnabled` | `bool`               | `false`           | 启用文本搜索，输入字符自动定位匹配条目 | 长列表快速检索，设备列表、配方列表推荐开启                 |

------

## 三、实例核心属性深度解析

### 3.1 数据核心：Items 与 ItemsSource

这是 `ItemsControl` 最核心的两个属性，对应两种使用方式：

1. **`Items`（`ItemCollection` 类型，只读集合对象）**
   - 是 `ItemsControl` 的内部集合，实现了 `INotifyCollectionChanged` 接口；
   - 未设置 `ItemsSource` 时，可手动 `Add/Remove` 条目；
   - 设置 `ItemsSource` 后自动变为只读，强行操作会抛出异常。
   - 适合静态、少量固定条目的场景。
2. **`ItemsSource`（`IEnumerable` 类型）**
   - 外部数据集合的绑定入口，是 MVVM 模式的标准用法；
   - 绑定 `ObservableCollection<T>` 时，集合的增删改会自动同步到 UI；
   - 工业场景**强烈推荐只用这种方式**，数据与 UI 彻底分离，逻辑更清晰。

### 3.2 呈现核心：ItemTemplate 与 ItemContainerStyle

这是最容易混淆的两个属性，职责完全不同：

| 属性                 | 作用层级   | 控制内容                                 | 类比           |
| :------------------- | :--------- | :--------------------------------------- | :------------- |
| `ItemTemplate`       | 数据内容层 | 每个条目内部显示什么内容、怎么排版       | 单元格里的内容 |
| `ItemContainerStyle` | 条目容器层 | 每个条目的整体高度、边距、背景、选中效果 | 整行的样式     |

工业场景示例：

- `ItemTemplate`：定义一行报警数据里，时间、级别、内容三个字段的排列方式；
- `ItemContainerStyle`：定义整行的高度、鼠标悬浮效果、严重报警的整行红色背景。

### 3.3 布局核心：ItemsPanel

`ItemsPanel` 指定用什么面板来排列所有条目容器，默认是垂直方向的 `StackPanel`。

- 普通列表：保持默认 `StackPanel` 即可；
- **大数据量工业列表：必须替换为 `VirtualizingStackPanel`**，开启 UI 虚拟化，只生成可见区域的条目，性能提升数十倍；
- 横向列表：替换为水平 `StackPanel` 或 `WrapPanel`；
- 自定义布局：可替换为自定义面板，实现瀑布流、卡片布局等。

XAML 示例：

xaml:

```xaml
<ItemsControl.ItemsPanel>
    <ItemsPanelTemplate>
        <VirtualizingStackPanel IsVirtualizing="True" VirtualizationMode="Recycling"/>
    </ItemsPanelTemplate>
</ItemsControl.ItemsPanel>
```

### 3.4 生成器核心：ItemContainerGenerator

`ItemContainerGenerator` 是 `ItemsControl` 最核心的内部对象，类型为 `ItemContainerGenerator`，负责**数据项到 UI 容器的完整生命周期管理**：

- 生成：根据数据项创建对应的 UI 容器；
- 映射：维护数据项与容器的双向映射，支持 `ContainerFromItem`、`ItemFromContainer` 查找；
- 回收：UI 虚拟化时，回收滚出屏幕的容器，复用给新进入屏幕的数据；
- 状态：生成过程有 `Generating`、`ContainersGenerated` 等状态，通过 `StatusChanged` 事件通知。

工业场景价值：需要操作条目 UI 元素时（如定位到指定行、获取行控件），必须通过 `ItemContainerGenerator` 查找，不能直接遍历视觉树。

------

## 四、核心事件解析

csharp:

```c#
public event EventHandler StatusChanged;
```

- **触发时机**：`ItemContainerGenerator` 的生成状态发生变化时触发；
- **状态枚举**：`GeneratorStatus` 包含 `NotStarted`、`Generating`、`ContainersGenerated` 三种；
- **典型应用**：
  1. 数据加载完成后，自动滚动到指定行；
  2. 容器全部生成后，执行行高计算、自定义布局等后续操作；
  3. 工业场景中，数据刷新后自动定位到最新的报警 / 数据行。

------

## 五、核心方法逐行解析

`ItemsControl` 的设计思想是「基类搭框架，子类填细节」，绝大多数扩展能力都通过受保护虚方法提供，是自定义列表控件的核心入口。

### 5.1 公共方法

#### `public static ItemsControl GetItemsOwner(DependencyObject element)`

- 官方作用：从某个依赖对象向上查找所属的 `ItemsControl` 父级；
- 典型场景：条目容器内部查找宿主 `ItemsControl`，获取上下文信息。

#### `public override void OnApplyTemplate()`

- 官方执行逻辑：
  1. 调用基类方法；
  2. 查找模板中的 `ItemsPresenter`（条目呈现器）；
  3. 将 `ItemsPanel` 生成的面板附加到 `ItemsPresenter` 上；
  4. 初始化 `ItemContainerGenerator`，开始生成条目容器。

### 5.2 受保护虚方法（核心扩展点）

这六个方法是自定义 `ItemsControl` 子类时必须掌握的核心，官方定义了条目容器的完整生命周期。

#### 1. `GetContainerForItemOverride()`

csharp:

```c#
protected virtual DependencyObject GetContainerForItemOverride();
```

- **作用**：创建单个条目的 UI 容器；
- **官方默认实现**：返回 `ContentPresenter`，作为最基础的条目容器；
- **子类重写示例**：`ListBox` 重写返回 `ListBoxItem`，`ListView` 返回 `ListViewItem`；
- **自定义场景**：自定义列表时重写，返回自定义的条目容器控件。

#### 2. `IsItemItsOwnContainerOverride(object item)`

csharp:

```c#
protected virtual bool IsItemItsOwnContainerOverride(object item);
```

- **作用**：判断传入的对象本身是不是已经是 UI 容器，不需要再包装一层；
- **官方默认实现**：判断是否为 `UIElement`，是则直接作为容器使用；
- **典型场景**：当直接往 `ItemsControl` 里放控件时，就不会再套一层容器。

#### 3. `PrepareContainerForItemOverride(DependencyObject element, object item)`

csharp:

```c#
protected virtual void PrepareContainerForItemOverride(DependencyObject element, object item);
```

- **作用**：容器生成后，准备数据绑定与样式，是条目初始化的核心入口；
- **官方默认执行逻辑**：
  1. 设置容器的 `DataContext` 为对应的数据项；
  2. 应用 `ItemContainerStyle` 样式；
  3. 应用 `ItemTemplate`、`DisplayMemberPath` 等呈现设置；
  4. 设置交替行索引 `AlternationIndex`。
- **自定义扩展**：重写此方法，给容器附加自定义绑定、注册事件、设置特殊状态。

#### 4. `ClearContainerForItemOverride(DependencyObject element, object item)`

csharp:

```c#
protected virtual void ClearContainerForItemOverride(DependencyObject element, object item);
```

- **作用**：容器被回收（虚拟化滚动出屏幕）时，清理绑定与事件；
- **官方默认实现**：清除数据上下文、解绑模板；
- **工业场景大坑**：如果重写了 `Prepare` 附加了事件、自定义绑定，必须在 `Clear` 中对应清理，否则会出现**内存泄漏、数据错乱、事件重复触发**等虚拟化典型问题。

#### 5. `OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)`

csharp:

```c#
protected virtual void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue);
```

- **作用**：`ItemsSource` 属性值变化时调用；
- **官方默认实现**：解绑旧集合的 `CollectionChanged` 事件，订阅新集合的变更通知，重新生成所有条目。

#### 6. `OnItemsChanged(NotifyCollectionChangedEventArgs e)`

csharp:

```c#
protected virtual void OnItemsChanged(NotifyCollectionChangedEventArgs e);
```

- **作用**：Items 集合或绑定的集合发生增删改时调用；
- **官方默认实现**：根据变更类型（添加 / 删除 / 重置），增量更新条目容器，避免全量刷新；
- **扩展场景**：集合变化时执行自定义逻辑，如自动滚动到底部、更新统计信息。

------

## 六、官方核心工作机制

### 6.1 条目容器生命周期

1. **生成阶段**：数据项进入可见区域时，调用 `GetContainerForItemOverride` 创建容器，再调用 `PrepareContainerForItemOverride` 绑定数据；
2. **使用阶段**：容器在界面上呈现，响应交互；
3. **回收阶段**：数据项滚出可见区域（虚拟化模式）时，调用 `ClearContainerForItemOverride` 清理数据，容器回收到缓存池，等待复用给新数据项。

> 这就是 UI 虚拟化的核心原理：无论数据有几万条，内存中始终只有几十个容器对象，大幅降低内存占用和渲染开销。

### 6.2 集合变更自动响应机制

当 `ItemsSource` 绑定的集合实现了 `INotifyCollectionChanged` 接口（如 `ObservableCollection<T>`）时：

1. 集合添加项 → 增量生成对应容器，插入到布局面板对应位置；
2. 集合删除项 → 移除对应容器，回收资源；
3. 集合重置（Clear）→ 清空所有容器，重新生成；
4. 集合移动项 → 移动对应容器的位置。

> 工业最佳实践：所有列表数据源统一使用 `ObservableCollection<T>`，不要手动操作 UI 元素，数据驱动 UI，代码更简洁且不易出错。

### 6.3 分层渲染架构

`ItemsControl` 的视觉树是典型的三层结构：

plaintext:

```tex
ItemsControl 控件本身
└── ItemsPresenter（条目呈现器，占位符）
    └── ItemsPanel 指定的布局面板（如 VirtualizingStackPanel）
        ├── 条目容器1（如 ContentPresenter / ListBoxItem）
        │   └── ItemTemplate 生成的内容
        ├── 条目容器2
        │   └── ItemTemplate 生成的内容
        └── ...
```

每一层职责单一，可独立替换定制，是 WPF 控件灵活可扩展的核心原因。

------

## 七、工业场景最佳实践与常见坑点

### 7.1 性能优化最佳实践

1. **大数据量必须开启 UI 虚拟化**
   - 替换 `ItemsPanel` 为 `VirtualizingStackPanel`，设置 `VirtualizationMode="Recycling"` 回收模式；
   - 几千条以上数据时，内存占用和渲染性能提升数十倍，是工业长列表的标配。
2. **优先绑定 ItemsSource，不手动操作 Items**
   - 数据逻辑全部在 ViewModel 中处理，UI 自动同步，符合 MVVM 规范；
   - 避免混用 `Items` 和 `ItemsSource`，这是新手最常见的报错原因。
3. **条目模板精简，降低视觉树层级**
   - 每个条目的视觉树越简单，整体渲染性能越好；
   - 避免在条目模板里嵌套复杂控件、大量动画，滚动时会严重卡顿。
4. **交替行用 AlternationIndex 实现**
   - 通过 `AlternationCount` + 样式触发器实现奇偶行变色，不要在数据模型里加背景色属性，保持数据与样式分离。

### 7.2 常见坑点

1. **虚拟化导致的数据错乱**
   - 现象：滚动列表后，部分行显示内容不对、事件重复触发；
   - 原因：自定义容器时，`Prepare` 加了绑定 / 事件，但 `Clear` 没对应清理；
   - 解决方案：重写 `ClearContainerForItemOverride`，完整清理所有自定义资源。
2. **遍历 Items 拿不到 UI 容器**
   - 现象：用 `foreach` 遍历 `Items`，想获取对应控件拿不到；
   - 原因：虚拟化模式下，不可见的条目没有生成容器；
   - 解决方案：通过 `ItemContainerGenerator.ContainerFromItem` 获取，且只对可见项有效。
3. **修改数据源不更新 UI**
   - 现象：给集合添加了数据，界面没变化；
   - 原因：集合没有实现 `INotifyCollectionChanged`，用了 `List<T>` 而不是 `ObservableCollection<T>`。

------

## 总结

`ItemsControl` 是 WPF 集合控件体系的基石，它抽象了「数据集合→UI 容器→布局排列」的通用逻辑，通过一组受保护虚方法提供了极高的扩展能力。理解它的容器生命周期、虚拟化机制、分层渲染架构，不仅能正确使用 ListBox/ListView 等派生控件，也是自定义高性能工业列表的必备基础。