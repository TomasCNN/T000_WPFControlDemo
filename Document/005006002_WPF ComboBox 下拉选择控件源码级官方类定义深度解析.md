# 005006002_WPF `ComboBox` 下拉选择控件源码级官方类定义深度解析

`ComboBox` 是 WPF 标准的下拉单选控件，直接继承自 `Selector` 单选基类，在 `ItemsControl` 集合呈现 + `Selector` 单选管理的完整能力基础上，通过 **Popup 折叠式下拉面板** 实现紧凑布局下的选项选择，同时扩展了可编辑输入模式与文本搜索能力。它是工业软件中字典选型、参数配置、数据筛选、级联联动的核心下拉控件。

其下拉面板复用 `ItemsControl` 完整的条目生成与虚拟化机制，和 `ListBox` 同属 `Selector` 体系，天然支持项模板定制、UI 虚拟化、键盘导航，既能满足简单字典选择，也能支撑千级数据的高性能下拉交互。

------

## 一、官方类定义总览与核心元数据

### 1.1 完整类签名（官方原生定义）

csharp:

```c#
namespace System.Windows.Controls
{
    [System.Windows.LocalizabilityAttribute(System.Windows.LocalizationCategory.None, Readability = System.Windows.Readability.Unreadable)]
    [System.Windows.StyleTypedPropertyAttribute(Property = "ItemContainerStyle", StyleTargetType = typeof(System.Windows.Controls.ComboBoxItem))]
    public class ComboBox : System.Windows.Controls.Primitives.Selector
    {
        // 新增静态依赖属性字段
        public static readonly System.Windows.DependencyProperty IsDropDownOpenProperty;
        public static readonly System.Windows.DependencyProperty IsEditableProperty;
        public static readonly System.Windows.DependencyProperty IsReadOnlyProperty;
        public static readonly System.Windows.DependencyProperty TextProperty;
        public static readonly System.Windows.DependencyProperty MaxDropDownHeightProperty;
        public static readonly System.Windows.DependencyProperty StaysOpenOnEditProperty;
        public static readonly System.Windows.DependencyProperty SelectionBoxItemProperty;
        public static readonly System.Windows.DependencyProperty SelectionBoxItemTemplateProperty;

        // 构造函数
        public ComboBox();

        // 新增公共属性
        public bool IsDropDownOpen { get; set; }
        public bool IsEditable { get; set; }
        public bool IsReadOnly { get; set; }
        public string Text { get; set; }
        public double MaxDropDownHeight { get; set; }
        public bool StaysOpenOnEdit { get; set; }
        public object SelectionBoxItem { get; }
        public DataTemplate SelectionBoxItemTemplate { get; }

        // 新增公共事件
        public event EventHandler DropDownOpened;
        public event EventHandler DropDownClosed;
        public event TextChangedEventHandler TextChanged;

        // 受保护重写方法
        protected override System.Windows.DependencyObject GetContainerForItemOverride();
        protected override bool IsItemItsOwnContainerOverride(object item);
        protected override void PrepareContainerForItemOverride(System.Windows.DependencyObject element, object item);
        protected override void ClearContainerForItemOverride(System.Windows.DependencyObject element, object item);
        protected override void OnSelectionChanged(System.Windows.Controls.SelectionChangedEventArgs e);
        protected virtual void OnDropDownOpened(EventArgs e);
        protected virtual void OnDropDownClosed(EventArgs e);
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e);
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer();
    }
}
```

### 1.2 核心元数据（官方精确值）

| 项               | 官方精确值                                                   | 工业场景关键说明                                       |
| :--------------- | :----------------------------------------------------------- | :----------------------------------------------------- |
| **命名空间**     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                                   |
| **程序集**       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                                     |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → Selector → ComboBox` | 完整继承集合呈现、单选管理、容器生命周期全部能力       |
| **默认条目容器** | `ComboBoxItem`                                               | 下拉项的 UI 容器，继承自 `ContentControl`              |
| **下拉承载机制** | `Popup` + `ItemsPresenter`                                   | 下拉面板复用 ItemsControl 条目生成逻辑，天然支持虚拟化 |
| **核心交互模式** | 普通选择模式 / 可编辑输入模式                                | 支持纯下拉选择和文本输入搜索两种交互                   |
| **工业核心场景** | 参数选型、设备筛选、配方选择、字典配置、级联下拉             | 所有单项选择的紧凑交互场景                             |

### 1.3 类级特性深度解析

1. **`[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ComboBoxItem))]`**
   - 向设计器与 XAML 解析器声明，`ItemContainerStyle` 属性的目标容器类型为 `ComboBoxItem`，提供样式智能提示与类型校验；
   - 与 `ListBox`、`ListView` 遵循完全一致的 `ItemsControl` 容器样式约定，保证体系内用法统一。
2. **`[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]`**
   - 控件本身无固定本地化文本，所有显示内容由业务数据与项模板决定，无需框架级本地化处理。

------

## 二、静态依赖属性全量深度解析

`ComboBox` 自身新增 8 个依赖属性，其余全部继承自 `ItemsControl` 与 `Selector`。以下按「新增核心属性」和「继承高频属性」分类说明。

### 2.1 ComboBox 新增核心依赖属性

#### 1. IsDropDownOpenProperty

csharp:

```c#
public static readonly DependencyProperty IsDropDownOpenProperty;
public bool IsDropDownOpen { get; set; }
```

- **类型**：`bool`
- **默认值**：`false`
- **官方作用**：控制下拉面板的展开 / 收起状态，支持双向绑定。
- **底层机制**：属性变更时，内部同步控制 `Popup` 的 `IsOpen` 状态；展开时触发布局计算、生成可见条目、滚动到选中项；收起时释放输入焦点。
- **工业场景价值**：可通过 ViewModel 直接控制下拉状态，实现自定义触发按钮、扫码自动展开、校验失败自动展开等定制交互。

#### 2. IsEditableProperty

csharp:

```c#
public static readonly DependencyProperty IsEditableProperty;
public bool IsEditable { get; set; }
```

- **类型**：`bool`
- **默认值**：`false`
- **官方作用**：是否启用可编辑模式。
- **底层行为差异**：
  - `false`（默认）：顶部选择框为 `ContentPresenter`，仅用于展示选中项，不可输入文本；
  - `true`：顶部选择框替换为 `TextBox`，支持手动输入文本，自动匹配下拉项。
- **工业场景**：长列表快速检索、支持自定义值的参数输入、配方编号模糊匹配等场景开启。

#### 3. IsReadOnlyProperty

csharp:

```c#
public static readonly DependencyProperty IsReadOnlyProperty;
public bool IsReadOnly { get; set; }
```

- **类型**：`bool`
- **默认值**：`false`
- **生效前提**：仅在 `IsEditable = true` 时有意义。
- **官方作用**：可编辑模式下，文本框是否仅可读。
- **典型组合**：`IsEditable="True"` + `IsReadOnly="True"` = 「可搜索、不可修改」模式。用户可输入文本进行搜索定位，但无法输入列表以外的自定义值，既保证检索效率，又避免非法输入，是工业参数选型的黄金组合。

#### 4. TextProperty

csharp:

```c#
public static readonly DependencyProperty TextProperty;
public string Text { get; set; }
```

- **类型**：`string`
- **默认值**：`string.Empty`
- **官方作用**：可编辑模式下，顶部文本框的显示文本。
- **同步机制**：
  - 选中下拉项时，自动将 `DisplayMemberPath` 对应的值同步到 `Text`；
  - 输入文本时，自动匹配下拉项，匹配成功则同步更新 `SelectedItem`；
  - 输入值不在数据源中时，`SelectedItem` 为 null，仅 `Text` 有值。
- **工业场景**：绑定到 ViewModel 实现实时筛选、输入校验、模糊搜索。

#### 5. MaxDropDownHeightProperty

csharp:

```c#
public static readonly DependencyProperty MaxDropDownHeightProperty;
public double MaxDropDownHeight { get; set; }
```

- **类型**：`double`
- **默认值**：系统自动计算值（约占屏幕 1/3 高度）
- **官方作用**：下拉面板的最大高度，超出后显示垂直滚动条。
- **工业最佳实践**：长列表建议手动设置为 250~400px，避免下拉占满屏幕，同时保证一次能展示足够多的选项，提升选择效率。

#### 6. StaysOpenOnEditProperty

csharp:

```c#
public static readonly DependencyProperty StaysOpenOnEditProperty;
public bool StaysOpenOnEdit { get; set; }
```

- **类型**：`bool`
- **默认值**：`false`
- **生效前提**：仅在 `IsEditable = true` 时有意义。
- **官方作用**：输入文本时是否保持下拉面板展开。
- **典型场景**：实时筛选模式下设为 `true`，边输入边过滤下拉结果，直观展示匹配项，符合搜索直觉。

#### 7. SelectionBoxItemProperty / SelectionBoxItemTemplateProperty

csharp:

```c#
public static readonly DependencyProperty SelectionBoxItemProperty;
public static readonly DependencyProperty SelectionBoxItemTemplateProperty;

public object SelectionBoxItem { get; }
public DataTemplate SelectionBoxItemTemplate { get; }
```

- **类型**：`object` / `DataTemplate`
- **性质**：**只读内部属性**，对外仅暴露 getter
- **官方作用**：下拉收起时，顶部选择框的显示内容与模板。
- **底层机制**：
  - `SelectionBoxItem` 存储当前选中项的显示内容；
  - `SelectionBoxItemTemplate` 对应选择框的内容模板，默认与下拉项模板一致；
  - 选中项变更时，内部自动更新这两个属性，驱动选择框重绘。
- **开发说明**：业务开发几乎不会直接操作这两个属性，它们是控件内部渲染选择框的核心载体；自定义控件模板时可通过绑定使用。

### 2.2 继承的高频核心属性

全部继承自 `ItemsControl` 与 `Selector`，用法与 `ListBox` 完全一致：

| 分类     | 属性                                               | 来源         | 工业场景作用                     |
| :------- | :------------------------------------------------- | :----------- | :------------------------------- |
| 数据绑定 | `ItemsSource`                                      | ItemsControl | 绑定下拉项数据源，MVVM 标准入口  |
| 显示与值 | `DisplayMemberPath` / `SelectedValuePath`          | Selector     | 指定显示字段 / 选中值字段        |
| 选中同步 | `SelectedItem` / `SelectedIndex` / `SelectedValue` | Selector     | 单选绑定核心，联动 ViewModel     |
| 呈现定制 | `ItemTemplate` / `ItemContainerStyle`              | ItemsControl | 自定义下拉项外观与样式           |
| 性能优化 | `ItemsPanel`                                       | ItemsControl | 替换布局面板，开启 UI 虚拟化     |
| 文本搜索 | `IsTextSearchEnabled` / `TextSearch.TextPath`      | Selector     | 键盘输入快速定位选项，长列表必备 |
| 交替行   | `AlternationCount` / `AlternationIndex`            | ItemsControl | 奇偶行交替背景，提升长列表可读性 |

------

## 三、核心事件体系全解析

### 3.1 新增专属事件

| 事件             | 触发时机                           | 典型工业用法                                                 |
| :--------------- | :--------------------------------- | :----------------------------------------------------------- |
| `DropDownOpened` | 下拉面板完全展开后触发             | 延迟加载下拉数据、异步拉取字典项，避免页面初始化时加载大量数据，提升启动速度 |
| `DropDownClosed` | 下拉面板完全收起后触发             | 校验输入内容合法性、同步选中值到业务模型、提交筛选条件       |
| `TextChanged`    | 可编辑模式下，文本框内容变化时触发 | 实时过滤下拉项、模糊匹配、输入格式校验                       |

### 3.2 继承核心事件

- `SelectionChanged`：选中项变化时触发，继承自 `Selector`，是主从联动、级联下拉的核心触发点。

------

## 四、核心方法逐行解析

### 4.1 公共方法

`ComboBox` 没有新增高频公共方法，通用操作全部通过属性与事件驱动；继承自 `Control` / `ItemsControl` 的标准方法（如 `Focus()`、`FindResource()`）照常使用。

### 4.2 受保护重写方法（自定义扩展核心）

这部分是自定义 `ComboBox` 子类的核心扩展点，覆盖容器生命周期、下拉生命周期、输入处理三个维度。

#### 1. 容器生命周期方法

##### GetContainerForItemOverride

csharp:

```c#
protected override DependencyObject GetContainerForItemOverride();
```

- **官方实现**：返回 `new ComboBoxItem()`。
- **设计意义**：将基类抽象的条目容器具体化为 `ComboBoxItem`，是下拉项的标准 UI 载体。

##### IsItemItsOwnContainerOverride

csharp:

```c#
protected override bool IsItemItsOwnContainerOverride(object item);
```

- **官方实现**：判断 `item is ComboBoxItem`，是则返回 true。
- **作用**：支持 XAML 中直接添加 `<ComboBoxItem>` 静态子元素，无需额外包装。

##### PrepareContainerForItemOverride

csharp:

```c#
protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
```

- **官方执行逻辑**：
  1. 调用基类方法，完成数据上下文、样式、模板、选中状态的基础准备；
  2. 同步 `IsSelected` 状态到 `ComboBoxItem` 容器；
  3. 应用文本搜索匹配逻辑。
- **虚拟化适配**：容器复用时，从持久化存储恢复该数据项的选中状态。

##### ClearContainerForItemOverride

csharp:

```c#
protected override void ClearContainerForItemOverride(DependencyObject element, object item);
```

- **官方执行逻辑**：
  1. 清除容器的 `IsSelected` 状态，避免复用时残留；
  2. 将选中状态持久化到数据层；
  3. 调用基类方法清理数据上下文与模板。
- ⚠️ 自定义扩展必知：重写 Prepare 时附加的自定义属性 / 事件，必须在此方法中对应清理，否则虚拟化滚动会出现状态错乱与内存泄漏。

#### 2. 下拉生命周期方法

##### OnDropDownOpened

csharp:

```c#
protected virtual void OnDropDownOpened(EventArgs e);
```

- **触发时机**：下拉面板完全展开后。
- **官方默认逻辑**：触发 `DropDownOpened` 公共事件，将键盘焦点移动到下拉面板，滚动到当前选中项。
- **扩展价值**：子类重写可实现数据懒加载、展开动画、默认焦点设置等自定义逻辑。

##### OnDropDownClosed

csharp:

```c#
protected virtual void OnDropDownClosed(EventArgs e);
```

- **触发时机**：下拉面板完全收起后。
- **官方默认逻辑**：触发 `DropDownClosed` 公共事件，将焦点移回选择框，校验输入文本合法性。
- **扩展价值**：子类重写可实现输入补全、自动修正、失焦校验等逻辑。

#### 3. 选中与输入处理

##### OnSelectionChanged

csharp:

```c#
protected override void OnSelectionChanged(SelectionChangedEventArgs e);
```

- **官方扩展逻辑**：
  1. 调用基类方法触发 `SelectionChanged` 事件；
  2. 同步更新 `Text` 属性（可编辑模式）；
  3. 更新 `SelectionBoxItem` 与 `SelectionBoxItemTemplate`，刷新选择框显示；
  4. 下拉展开状态下，自动滚动到新选中项。

##### OnKeyDown

csharp:

```c#
protected override void OnKeyDown(KeyEventArgs e);
```

- **官方实现**：完整的键盘交互逻辑
  - 方向键上下：切换选中项；
  - Alt + ↓ / F4：展开 / 收起下拉；
  - 回车：确认选中并收起下拉；
  - Esc：取消选择并收起下拉；
  - 字母键：文本搜索定位匹配项；
  - 可编辑模式下支持文本输入、全选、删除等标准文本框快捷键。
- **工业价值**：完整支持纯键盘操作，适配工控机无鼠标、触摸屏操作场景。

------

## 五、配套条目容器：`ComboBoxItem` 类定义解析

`ComboBoxItem` 是下拉列表的默认条目容器，继承自 `ContentControl`，是选中状态与交互的 UI 载体。

### 5.1 官方精简类定义

csharp:

```c#
public class ComboBoxItem : ContentControl
{
    public static readonly DependencyProperty IsSelectedProperty;

    public bool IsSelected { get; set; }

    protected virtual void OnSelected(RoutedEventArgs e);
    protected virtual void OnUnselected(RoutedEventArgs e);
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e);
}
```

### 5.2 核心成员说明

1. **`IsSelected` 属性**
   - 本质是 `Selector.IsSelected` 附加属性的强类型包装，内部直接操作附加属性；
   - 是样式触发器、数据绑定的核心目标属性。
2. **`OnSelected / OnUnselected` 虚方法**
   - 对应 `Selector.SelectedEvent` / `Selector.UnselectedEvent` 附加路由事件的类处理；
   - 选中 / 取消选中时触发，子类可重写扩展自定义逻辑。
3. **鼠标交互逻辑**
   - 重写左键抬起事件，触发选中逻辑并自动收起下拉面板；
   - 配合 `ComboBox` 的选择模式，实现单击选中并关闭下拉的标准交互。

------

## 六、官方核心工作机制

### 6.1 下拉弹出机制

- 内部通过 `Popup` 控件承载下拉内容，默认向控件下方弹出；当屏幕下方空间不足时，自动向上弹出，避免超出屏幕边界；
- 下拉面板的内容宿主是 `ItemsPresenter`，完全复用 `ItemsControl` 的条目生成、布局、虚拟化机制，和 `ListBox` 能力对齐，并非内嵌独立的 `ListBox` 控件；
- 选中某项后，`Popup` 自动关闭，选择框同步更新显示内容。

### 6.2 两种交互模式的本质差异

| 维度       | 普通选择模式（IsEditable=false） | 可编辑模式（IsEditable=true）                 |
| :--------- | :------------------------------- | :-------------------------------------------- |
| 顶部选择框 | `ContentPresenter`，仅展示       | `TextBox`，可输入文本                         |
| 输入能力   | 仅支持键盘字符搜索               | 支持完整文本输入、编辑、删除                  |
| 值范围     | 只能从数据源中选择               | 可输入自定义值，也可选择列表项                |
| 文本同步   | 无 Text 属性同步                 | 选中项自动同步到 Text，输入匹配自动更新选中项 |
| 典型场景   | 固定字典、枚举、分类选择         | 长列表搜索、支持自定义值的参数输入            |

### 6.3 文本搜索机制

- 基于 `TextSearch` 附加属性实现，默认匹配 `DisplayMemberPath` 对应的字段；
- 普通模式下，键盘输入字符自动跳转到第一个匹配项；
- 可编辑模式下，输入时自动补全匹配项的文本，同时高亮对应下拉项；
- 可通过 `TextSearch.TextPath` 附加属性自定义搜索匹配的字段，支持显示字段和搜索字段分离。

### 6.4 UI 虚拟化兼容原理

`ComboBox` 完美支持 UI 虚拟化，依赖三层保障：

1. **基类支持**：`ItemsControl` 原生支持 `VirtualizingStackPanel` 虚拟化；
2. **容器生命周期**：`Prepare/ClearContainerForItemOverride` 完整成对实现，支持容器回收复用；
3. **状态持久化**：选中状态绑定在数据层，不依赖 UI 容器，下拉展开、滚动时状态不丢失。

- 实际效果：千级数据下拉，开启虚拟化后展开速度从秒级缩短到毫秒级，内存占用降低 80% 以上。

------

## 总结

`ComboBox` 是 `Selector` 单选体系的「折叠式」实现：它完整复用了 `ItemsControl` 的集合呈现能力与 `Selector` 的单选管理能力，通过 `Popup` 将选项面板折叠起来，在有限的界面空间内提供完整的选择功能，同时扩展可编辑模式兼顾检索效率。其设计的精髓在于**复用而非重造**—— 下拉面板完全复用 ItemsControl 成熟的容器、模板、虚拟化体系，保证了控件的稳定性与性能上限。