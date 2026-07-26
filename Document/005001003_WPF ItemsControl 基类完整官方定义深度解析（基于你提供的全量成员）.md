# 005001003_WPF `ItemsControl` 基类**完整官方定义**深度解析（基于你提供的全量成员）

**源码：**

```c#
public class ItemsControl : Control, IAddChild, IGeneratorHost, IContainItemStorage
{
    public static readonly DependencyProperty ItemsSourceProperty;
    public static readonly DependencyProperty HasItemsProperty;
    public static readonly DependencyProperty DisplayMemberPathProperty;
    public static readonly DependencyProperty ItemTemplateProperty;
    public static readonly DependencyProperty ItemTemplateSelectorProperty;
    public static readonly DependencyProperty ItemStringFormatProperty;
    public static readonly DependencyProperty ItemBindingGroupProperty;
    public static readonly DependencyProperty ItemContainerStyleProperty;
    public static readonly DependencyProperty ItemContainerStyleSelectorProperty;
    public static readonly DependencyProperty ItemsPanelProperty;
    public static readonly DependencyProperty IsGroupingProperty;
    public static readonly DependencyProperty GroupStyleSelectorProperty;
    public static readonly DependencyProperty AlternationCountProperty;
    public static readonly DependencyProperty AlternationIndexProperty;
    public static readonly DependencyProperty IsTextSearchEnabledProperty;
    public static readonly DependencyProperty IsTextSearchCaseSensitiveProperty;
 
    public ItemsControl();
 
    public int AlternationCount { get; set; }
    public GroupStyleSelector GroupStyleSelector { get; set; }
    public ObservableCollection<GroupStyle> GroupStyle { get; }
    public bool IsGrouping { get; }
    public ItemsPanelTemplate ItemsPanel { get; set; }
    public StyleSelector ItemContainerStyleSelector { get; set; }
    public Style ItemContainerStyle { get; set; }
    public BindingGroup ItemBindingGroup { get; set; }
    public string ItemStringFormat { get; set; }
    public DataTemplateSelector ItemTemplateSelector { get; set; }
    public DataTemplate ItemTemplate { get; set; }
    public string DisplayMemberPath { get; set; }
    public bool HasItems { get; }
    public ItemContainerGenerator ItemContainerGenerator { get; }
    public IEnumerable ItemsSource { get; set; }
    public ItemCollection Items { get; }
    public bool IsTextSearchCaseSensitive { get; set; }
    public bool IsTextSearchEnabled { get; set; }
    protected internal override IEnumerator LogicalChildren { get; }
 
    public static DependencyObject ContainerFromElement(ItemsControl itemsControl, DependencyObject element);
    public static int GetAlternationIndex(DependencyObject element);
    public static ItemsControl GetItemsOwner(DependencyObject element);
    public static ItemsControl ItemsControlFromItemContainer(DependencyObject container);
    public override void BeginInit();
    public DependencyObject ContainerFromElement(DependencyObject element);
    public override void EndInit();
    public bool IsItemItsOwnContainer(object item);
    public bool ShouldSerializeGroupStyle();
    public bool ShouldSerializeItems();
    public override string ToString();
    protected virtual void AddChild(object value);
    protected virtual void AddText(string text);
    protected virtual void ClearContainerForItemOverride(DependencyObject element, object item);
    protected virtual DependencyObject GetContainerForItemOverride();
    protected virtual bool IsItemItsOwnContainerOverride(object item);
    protected virtual void OnAlternationCountChanged(int oldAlternationCount, int newAlternationCount);
    protected virtual void OnDisplayMemberPathChanged(string oldDisplayMemberPath, string newDisplayMemberPath);
    protected virtual void OnGroupStyleSelectorChanged(GroupStyleSelector oldGroupStyleSelector, GroupStyleSelector newGroupStyleSelector);
    protected virtual void OnItemBindingGroupChanged(BindingGroup oldItemBindingGroup, BindingGroup newItemBindingGroup);
    protected virtual void OnItemContainerStyleChanged(Style oldItemContainerStyle, Style newItemContainerStyle);
    protected virtual void OnItemContainerStyleSelectorChanged(StyleSelector oldItemContainerStyleSelector, StyleSelector newItemContainerStyleSelector);
    protected virtual void OnItemsChanged(NotifyCollectionChangedEventArgs e);
    protected virtual void OnItemsPanelChanged(ItemsPanelTemplate oldItemsPanel, ItemsPanelTemplate newItemsPanel);
    protected virtual void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue);
    protected virtual void OnItemStringFormatChanged(string oldItemStringFormat, string newItemStringFormat);
    protected virtual void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate);
    protected virtual void OnItemTemplateSelectorChanged(DataTemplateSelector oldItemTemplateSelector, DataTemplateSelector newItemTemplateSelector);
    protected override void OnKeyDown(KeyEventArgs e);
    protected override void OnTextInput(TextCompositionEventArgs e);
    protected virtual void PrepareContainerForItemOverride(DependencyObject element, object item);
    protected virtual bool ShouldApplyItemContainerStyle(DependencyObject container, object item);
 
}
```

你给出的是更贴近 .NET 官方源码的完整类定义，相比公开接口版补充了**两个核心内部接口实现、分组体系、绑定组、完整属性变更虚方法、初始化生命周期**等大量底层成员。这份定义才是 `ItemsControl` 作为所有集合控件基类的完整能力全貌。

本文严格对照你列出的每一个成员，从**接口契约、静态依赖属性、实例属性、静态方法、实例方法、受保护扩展点**六个维度逐行解析，补充官方内部实现逻辑与工业场景的应用价值。

------

## 一、完整类定义与核心元数据

### 1.1 官方完整类签名

csharp:

```c#
namespace System.Windows.Controls
{
    [ContentProperty("Items")]
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(FrameworkElement))]
    [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
    public class ItemsControl : Control, 
        IAddChild, 
        IGeneratorHost, 
        IContainItemStorage
    {
        // 全部成员见你提供的代码，下文逐段解析
    }
}
```

### 1.2 核心元数据

| 项         | 官方精确值                                                   | 说明                                                         |
| :--------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| 命名空间   | `System.Windows.Controls`                                    | WPF 标准控件命名空间                                         |
| 程序集     | `PresentationFramework.dll`                                  | WPF 核心框架程序集                                           |
| 完整继承链 | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl` | 所有列表 / 集合控件的抽象基类                                |
| 实现接口   | `IAddChild`、`IGeneratorHost`、`IContainItemStorage`         | 分别对应 XAML 内容支持、生成器宿主、容器状态存储             |
| 核心子类   | `Selector`、`Menu`、`StatusBar`、`TreeView`、`DataGrid`      | `ListBox/ListView/ComboBox` 均继承自 `Selector`              |
| 设计定位   | 通用集合数据可视化基类                                       | 只负责「数据→容器→布局」的通用逻辑，不含选择、编辑等业务语义 |

### 1.3 三大接口深度解析

这是你这份定义的核心增量，三个接口分别支撑了 `ItemsControl` 三大核心能力：

#### 1. `IAddChild`

- **官方作用**：XAML 解析器约定接口，支持在 XAML 标签内直接写子元素，自动添加到 `Items` 集合。
- **对应方法**：`AddChild(object value)` + `AddText(string text)`
- **工业意义**：让静态列表、菜单、工具栏等可以用简洁的 XAML 声明式写法，无需后台代码手动 Add。

#### 2. `IGeneratorHost`

- **官方作用**：**内部接口**，是 `ItemContainerGenerator` 的宿主契约，为容器生成器提供所有必要的上下文（样式、模板、面板、交替计数等）。
- **核心价值**：把容器生成逻辑抽离到 `ItemContainerGenerator` 独立类中，`ItemsControl` 只负责提供配置，生成器负责执行生成，职责分离。
- **对应成员**：`GetContainerForItemOverride`、`IsItemItsOwnContainerOverride`、`PrepareContainerForItemOverride` 等虚方法，都是该接口约定的扩展点。

#### 3. `IContainItemStorage`

- **官方作用**：**内部接口**，支持 UI 虚拟化下的**容器状态持久化**。
- **解决的问题**：虚拟化滚动时，滚出屏幕的容器会被回收复用，如果用户手动设置了容器的某些属性（比如背景色），复用后会错乱。该接口提供了「按数据项存储状态」的能力，容器复用时可以恢复对应数据项的状态。
- **工业意义**：自定义行样式、行状态标记的工业列表（如报警行标红、选中状态保持），在虚拟化模式下不会因为滚动出现状态错乱，底层就是靠这个接口支撑。

------

## 二、静态依赖属性全量逐行解析

按功能分为 7 大类，你列出的 16 个依赖属性全部覆盖：

### 2.1 数据集合类

| 属性字段              | 包装属性      | 类型          | 默认值  | 官方作用                                     |
| :-------------------- | :------------ | :------------ | :------ | :------------------------------------------- |
| `ItemsSourceProperty` | `ItemsSource` | `IEnumerable` | `null`  | 外部数据集合绑定入口，数据驱动 UI 生成       |
| `HasItemsProperty`    | `HasItems`    | `bool`        | `false` | 只读，指示集合是否包含数据，用于空状态触发器 |

> 🔑 核心规则：设置 `ItemsSource` 后，`Items` 集合自动变为只读，二者不可混用。工业 MVVM 场景**只使用 `ItemsSource` 绑定 `ObservableCollection<T>`**。

### 2.2 条目呈现类

| 属性字段                       | 包装属性               | 类型                   | 默认值         | 官方作用                                 |
| :----------------------------- | :--------------------- | :--------------------- | :------------- | :--------------------------------------- |
| `ItemTemplateProperty`         | `ItemTemplate`         | `DataTemplate`         | `null`         | 每个数据项的内容呈现模板                 |
| `ItemTemplateSelectorProperty` | `ItemTemplateSelector` | `DataTemplateSelector` | `null`         | 根据数据动态选择不同的内容模板           |
| `DisplayMemberPathProperty`    | `DisplayMemberPath`    | `string`               | `string.Empty` | 指定显示对象的某个属性，简化简单列表     |
| `ItemStringFormatProperty`     | `ItemStringFormat`     | `string`               | `null`         | 条目内容的字符串格式化（数值、日期等）   |
| `ItemBindingGroupProperty`     | `ItemBindingGroup`     | `BindingGroup`         | `null`         | 每个条目容器的绑定组，用于条目级批量验证 |

> 工业场景重点：`ItemBindingGroup` 可用于可编辑列表的行级数据校验，比如配方参数修改后整行统一验证、统一提交，非常适合设备参数配置场景。

### 2.3 容器样式类

| 属性字段                             | 包装属性                     | 类型            | 默认值 | 官方作用                 |
| :----------------------------------- | :--------------------------- | :-------------- | :----- | :----------------------- |
| `ItemContainerStyleProperty`         | `ItemContainerStyle`         | `Style`         | `null` | 条目容器的统一样式       |
| `ItemContainerStyleSelectorProperty` | `ItemContainerStyleSelector` | `StyleSelector` | `null` | 根据数据动态选择容器样式 |

### 2.4 布局与交替行类

| 属性字段                   | 包装属性           | 类型                 | 默认值               | 官方作用                                                     |
| :------------------------- | :----------------- | :------------------- | :------------------- | :----------------------------------------------------------- |
| `ItemsPanelProperty`       | `ItemsPanel`       | `ItemsPanelTemplate` | `StackPanel`（垂直） | 条目容器的布局面板                                           |
| `AlternationCountProperty` | `AlternationCount` | `int`                | `0`                  | 交替行周期数，设为 2 即奇偶行交替                            |
| `AlternationIndexProperty` | （附加属性）       | `int`                | `0`                  | **附加属性**，标记每个容器的交替索引（0/1/2...），触发器绑定用 |

> 🔑 关键说明：`AlternationIndex` 是附加属性，不是实例属性，通过 `GetAlternationIndex` 静态方法读取。工业列表实现奇偶行变色，就是绑定容器的 `AlternationIndex` 附加属性，通过触发器切换背景色。

### 2.5 分组类

| 属性字段                     | 包装属性             | 类型                 | 默认值  | 官方作用                         |
| :--------------------------- | :------------------- | :------------------- | :------ | :------------------------------- |
| `IsGroupingProperty`         | `IsGrouping`         | `bool`               | `false` | 只读，指示当前是否启用了分组呈现 |
| `GroupStyleSelectorProperty` | `GroupStyleSelector` | `GroupStyleSelector` | `null`  | 根据分组数据动态选择分组样式     |

> 工业场景应用：生产数据按班次、批次、设备分组展示，分组头显示汇总信息（数量、良率），底层由 `CollectionViewSource` 提供分组数据，`ItemsControl` 负责可视化呈现。

### 2.6 文本搜索类

| 属性字段                            | 包装属性                    | 类型   | 默认值  | 官方作用                                 |
| :---------------------------------- | :-------------------------- | :----- | :------ | :--------------------------------------- |
| `IsTextSearchEnabledProperty`       | `IsTextSearchEnabled`       | `bool` | `false` | 启用键盘文本搜索，输入字符自动定位匹配项 |
| `IsTextSearchCaseSensitiveProperty` | `IsTextSearchCaseSensitive` | `bool` | `false` | 文本搜索是否区分大小写                   |

> 工业场景应用：设备列表、配方列表、物料清单等长列表，用户输入首字母 / 编号快速定位，大幅提升操作效率。

------

## 三、实例属性全量逐行解析

### 3.1 数据核心属性

#### `public ItemCollection Items { get; }`

- 类型：`ItemCollection`（实现 `INotifyCollectionChanged` 的内部集合）
- 未设置 `ItemsSource` 时可手动增删；设置后自动变为只读。
- 同时承载数据项和 UI 元素，是 `IAddChild` 接口的实际存储。

#### `public IEnumerable ItemsSource { get; set; }`

- MVVM 标准数据绑定入口，绑定 `ObservableCollection<T>` 可实现集合变更自动同步 UI。
- 工业最佳实践：所有业务列表统一使用该属性，彻底分离数据与视图。

#### `public bool HasItems { get; }`

- 只读快捷属性，等价于 `Items.Count > 0`。
- 常用作控件模板触发器，列表为空时显示「暂无数据」占位图。

### 3.2 条目呈现属性

#### `public DataTemplate ItemTemplate { get; set; }`

#### `public DataTemplateSelector ItemTemplateSelector { get; set; }`

#### `public string DisplayMemberPath { get; set; }`

#### `public string ItemStringFormat { get; set; }`

#### `public BindingGroup ItemBindingGroup { get; set; }`

- 优先级：`ItemTemplate` > `DisplayMemberPath` > 默认 `ToString()`。
- `ItemBindingGroup`：每个条目容器共享一个绑定组，可实现行级事务式验证 —— 所有字段校验通过才提交到数据源，适合可编辑的参数配置列表。

### 3.3 容器与样式属性

#### `public Style ItemContainerStyle { get; set; }`

#### `public StyleSelector ItemContainerStyleSelector { get; set; }`

#### `public ItemContainerGenerator ItemContainerGenerator { get; }`

- `ItemContainerGenerator` 是核心生成器，负责容器的创建、复用、映射，是 UI 虚拟化的执行主体。
- 提供 `ContainerFromItem`、`ItemFromContainer` 等双向查找方法。

### 3.4 布局与分组属性

#### `public ItemsPanelTemplate ItemsPanel { get; set; }`

- 替换布局面板，工业大数据量必须替换为 `VirtualizingStackPanel` 开启虚拟化。

#### `public int AlternationCount { get; set; }`

- 交替行周期，设为 2 时，容器的 `AlternationIndex` 按 0、1、0、1 循环。

#### `public bool IsGrouping { get; }`

- 只读，当数据源启用分组视图时自动为 `true`。

#### `public ObservableCollection<GroupStyle> GroupStyle { get; }`

- 分组样式集合，支持多级分组（如先按班次、再按设备）。
- 每个 `GroupStyle` 可定义分组头模板、分组容器样式、分组面板。

#### `public GroupStyleSelector GroupStyleSelector { get; set; }`

- 动态选择分组样式，不同层级的分组使用不同的视觉呈现。

### 3.5 文本搜索属性

#### `public bool IsTextSearchEnabled { get; set; }`

#### `public bool IsTextSearchCaseSensitive { get; set; }`

- 键盘输入时自动匹配条目的显示文本，定位到第一个匹配项。
- 匹配源默认取 `DisplayMemberPath` 指定的属性，或 `TextSearch.Text` 附加属性。

### 3.6 内部重写属性

#### `protected internal override IEnumerator LogicalChildren { get; }`

- 重写逻辑树子元素枚举，返回 `Items` 集合中的元素，支持 WPF 逻辑树遍历。

------

## 四、静态公共方法全量解析

你列出的 5 个静态方法，全部是辅助查找与属性读取的工具方法：

### 1. `public static ItemsControl GetItemsOwner(DependencyObject element)`

- **作用**：从任意子元素向上查找，找到所属的 `ItemsControl` 宿主。
- **典型场景**：条目容器内部的按钮点击事件中，获取父级 `ItemsControl`。

### 2. `public static ItemsControl ItemsControlFromItemContainer(DependencyObject container)`

- **作用**：从一个条目容器，反向查找所属的 `ItemsControl`。
- **工业场景**：自定义容器控件中，需要访问宿主的属性或方法时使用。

### 3. `public static DependencyObject ContainerFromElement(ItemsControl itemsControl, DependencyObject element)`

- **作用**：给定 `ItemsControl` 和一个子元素，向上找到它所属的条目容器。
- **典型场景**：点击列表行内的按钮，找到对应的行容器和数据项。

### 4. `public static int GetAlternationIndex(DependencyObject element)`

- **作用**：读取元素的 `AlternationIndex` 附加属性值。
- **说明**：`AlternationIndex` 是附加在条目容器上的，不是 `ItemsControl` 的实例属性，所以通过静态方法读取。XAML 中用 `ItemsControl.AlternationIndex` 绑定。

------

## 五、公共实例方法全量解析

### 5.1 初始化生命周期方法

#### `public override void BeginInit()`

#### `public override void EndInit()`

- 实现 `ISupportInitialize` 接口，对应 XAML 解析的初始化阶段。
- **机制**：`BeginInit` 到 `EndInit` 之间，批量设置属性不会触发中间的集合刷新、容器生成；`EndInit` 调用后才一次性完成初始化和生成。
- **意义**：避免 XAML 解析时每设置一个属性就刷新一次 UI，大幅提升加载性能。

### 5.2 容器查找方法

#### `public DependencyObject ContainerFromElement(DependencyObject element)`

- 实例版本，从子元素查找所属的条目容器，无需传入 `ItemsControl` 参数。

#### `public bool IsItemItsOwnContainer(object item)`

- 公开入口，判断一个对象本身是不是 UI 容器，不需要再包装。内部调用 `IsItemItsOwnContainerOverride`。

### 5.3 序列化设计器方法

#### `public bool ShouldSerializeGroupStyle()`

#### `public bool ShouldSerializeItems()`

- XAML 设计器序列化约定方法，判断是否需要序列化该属性。
- 比如 `GroupStyle` 为空时就不序列化到 XAML 中，保持代码简洁。

### 5.4 其他

#### `public override string ToString()`

- 返回控件类型名 + 条目数量信息，调试用。

------

## 六、受保护虚方法全量解析（核心扩展点）

这是 `ItemsControl` 扩展性的核心，你列出的 20+ 个受保护方法，分为**容器生命周期、属性变更通知、输入处理、XAML 内容支持、样式判断**五大类。

### 6.1 容器生命周期方法（最核心）

这四个方法定义了条目容器的完整生命周期，是自定义列表控件必须掌握的：

#### 1. `GetContainerForItemOverride()`

csharp:

```c#
protected virtual DependencyObject GetContainerForItemOverride();
```

- **作用**：创建新的条目容器对象。
- **默认实现**：返回 `ContentPresenter`。
- **子类重写**：`ListBox` 返回 `ListBoxItem`，`ListView` 返回 `ListViewItem`。

#### 2. `IsItemItsOwnContainerOverride(object item)`

csharp:

```c#
protected virtual bool IsItemItsOwnContainerOverride(object item);
```

- **作用**：判断 item 本身是不是容器，是则直接使用，不包装。
- **默认实现**：判断是否为 `UIElement`。

#### 3. `PrepareContainerForItemOverride(DependencyObject element, object item)`

csharp:

```c#
protected virtual void PrepareContainerForItemOverride(DependencyObject element, object item);
```

- **作用**：容器生成后，绑定数据、应用样式、设置上下文。
- **默认执行**：设置 `DataContext`、应用 `ItemContainerStyle`、应用 `ItemTemplate`、设置 `AlternationIndex`。
- **工业扩展**：重写此方法附加自定义绑定、注册行内事件、设置状态标记。

#### 4. `ClearContainerForItemOverride(DependencyObject element, object item)`

csharp:

```c#
protected virtual void ClearContainerForItemOverride(DependencyObject element, object item);
```

- **作用**：容器回收复用时，清理数据绑定和事件。
- ⚠️ **工业大坑**：如果在 `Prepare` 中注册了事件、附加了自定义属性，必须在这里对应清理，否则虚拟化滚动时会出现**内存泄漏、事件重复触发、数据错乱**。

### 6.2 属性变更通知方法

所有 `OnXXXChanged` 方法，都是对应依赖属性变更后的回调入口，子类可重写响应属性变化：

| 方法                                  | 触发时机           | 扩展场景                                   |
| :------------------------------------ | :----------------- | :----------------------------------------- |
| `OnItemsSourceChanged`                | `ItemsSource` 更换 | 解绑旧集合事件，订阅新集合                 |
| `OnItemsChanged`                      | 集合内容增删改     | 增量更新容器，执行自定义逻辑（如自动滚动） |
| `OnItemsPanelChanged`                 | 布局面板更换       | 重新生成布局                               |
| `OnItemTemplateChanged`               | 数据模板更换       | 刷新所有条目的呈现                         |
| `OnItemTemplateSelectorChanged`       | 模板选择器更换     | 刷新呈现                                   |
| `OnDisplayMemberPathChanged`          | 显示路径更换       | 刷新文本显示                               |
| `OnItemStringFormatChanged`           | 字符串格式更换     | 刷新文本显示                               |
| `OnItemContainerStyleChanged`         | 容器样式更换       | 更新所有容器样式                           |
| `OnItemContainerStyleSelectorChanged` | 容器样式选择器更换 | 更新容器样式                               |
| `OnAlternationCountChanged`           | 交替计数更换       | 更新所有容器的交替索引                     |
| `OnGroupStyleSelectorChanged`         | 分组样式选择器更换 | 更新分组呈现                               |
| `OnItemBindingGroupChanged`           | 绑定组更换         | 更新条目的绑定验证组                       |

> 设计思想：每个属性变更都提供独立的虚方法，子类不需要自己注册属性变更回调，直接重写对应方法即可，代码更清晰。

### 6.3 输入处理方法

#### `protected override void OnKeyDown(KeyEventArgs e)`

#### `protected override void OnTextInput(TextCompositionEventArgs e)`

- 处理键盘输入，是文本搜索功能的底层实现。
- 子类可重写扩展自定义快捷键、导航逻辑。

### 6.4 XAML 内容支持（IAddChild 接口实现）

#### `protected virtual void AddChild(object value)`

#### `protected virtual void AddText(string text)`

- XAML 解析器调用，将子元素添加到 `Items` 集合。
- 子类可重写控制子元素的添加逻辑。

### 6.5 样式判断方法

#### `protected virtual bool ShouldApplyItemContainerStyle(DependencyObject container, object item)`

- **作用**：判断是否要给当前容器应用 `ItemContainerStyle`。
- **默认返回 true**。
- **扩展场景**：某些特殊条目（如分组头、分隔符）不需要应用普通容器样式，可重写此方法返回 false。

------

## 七、官方核心工作机制补充

### 7.1 完整初始化流程

plaintext:

```tex
XAML 解析开始
    ↓
BeginInit() 进入初始化模式，属性变更不触发UI刷新
    ↓
批量设置 ItemsSource、ItemTemplate、ItemsPanel 等所有属性
    ↓
EndInit() 结束初始化，一次性触发：
    1. 创建 ItemsPanel 布局面板
    2. 初始化 ItemContainerGenerator
    3. 根据数据源生成首批可见容器
    ↓
OnApplyTemplate 完成模板挂载
```

### 7.2 分组呈现机制

当数据源通过 `CollectionViewSource` 启用分组后：

1. `IsGrouping` 自动变为 `true`；
2. 生成器不再直接生成数据项容器，而是先生成分组头容器，再在每个分组内生成数据项容器；
3. 分组样式由 `GroupStyle` 定义，支持多级嵌套分组。

### 7.3 文本搜索工作原理

1. 用户按下键盘字符，进入 `OnTextInput`；
2. 累积输入字符形成搜索字符串；
3. 遍历可见条目，按 `DisplayMemberPath` 或 `TextSearch.Text` 属性匹配；
4. 找到第一个匹配项，将其设置为当前项并滚动到可见区域；
5. 短时间无输入则清空搜索字符串。

### 7.4 虚拟化 + 状态存储机制

配合 `VirtualizingStackPanel` 和 `IContainItemStorage`：

1. 只生成可见区域的容器，滚出屏幕的容器进入回收池；
2. 容器回收前，通过 `IContainItemStorage` 保存该数据项的所有本地值；
3. 容器复用时，恢复对应数据项的保存状态；
4. 效果：滚动时视觉状态、选中状态、自定义属性都不会错乱。

------

## 八、工业场景最佳实践与避坑

1. **大数据量必开虚拟化 + 回收模式**

   xaml:

   ```xaml
   <ItemsControl.ItemsPanel>
       <ItemsPanelTemplate>
           <VirtualizingStackPanel VirtualizationMode="Recycling"/>
       </ItemsPanelTemplate>
   </ItemsControl.ItemsPanel>
   ```

   万级数据量下，内存占用降低 90% 以上，滚动流畅度提升数倍。

2. **自定义容器必须成对重写 Prepare/Clear**

   只要在准备时附加了事件、绑定、资源，就必须在清理时对应释放，否则虚拟化滚动必然出现内存泄漏和数据错乱。

3. **奇偶行用 AlternationIndex 实现**

   不要在数据模型里加背景色属性，通过样式触发器绑定 `AlternationIndex` 附加属性实现，保持数据与样式分离。

4. **行级验证用 ItemBindingGroup**

   可编辑的参数列表，用 `ItemBindingGroup` 实现整行统一校验、统一提交，避免半修改状态写入设备。

5. **分组展示用 CollectionViewSource + GroupStyle**

   生产数据按班次、批次分组时，不要手动拼装嵌套集合，用官方分组机制性能更优、代码更简洁。

------

## 总结

你提供的这份定义，是 `ItemsControl` 接近源码级的完整接口全貌。它的核心设计思想是 **「约定 + 扩展」**：基类搭好完整的容器生命周期、属性变更通知、初始化流程框架，通过大量受保护虚方法提供扩展点，子类只需要重写少数方法就能实现特定业务的集合控件。理解这份完整定义，不仅能正确使用所有派生列表控件，也是自定义高性能工业列表控件的必备基础。