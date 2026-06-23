# 005006003_WPF `ListBox` 列表控件官方定义与工业场景实战

**源码：**

```c#
[Localizability(LocalizationCategory.ComboBox)]
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ComboBoxItem))]
    [TemplatePart(Name = "PART_EditableTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
public class ComboBox : Selector
{
    public static readonly DependencyProperty MaxDropDownHeightProperty;
    public static readonly DependencyProperty IsDropDownOpenProperty;
    public static readonly DependencyProperty ShouldPreserveUserEnteredPrefixProperty;
    public static readonly DependencyProperty IsEditableProperty;
    public static readonly DependencyProperty TextProperty;
    public static readonly DependencyProperty IsReadOnlyProperty;
    public static readonly DependencyProperty SelectionBoxItemProperty;
    public static readonly DependencyProperty SelectionBoxItemTemplateProperty;
    public static readonly DependencyProperty SelectionBoxItemStringFormatProperty;
    public static readonly DependencyProperty StaysOpenOnEditProperty;
 
    public ComboBox();
 
    public bool ShouldPreserveUserEnteredPrefix { get; set; }
    public bool IsEditable { get; set; }
    public string Text { get; set; }
    public bool IsReadOnly { get; set; }
    public object SelectionBoxItem { get; }
    public double MaxDropDownHeight { get; set; }
    public string SelectionBoxItemStringFormat { get; }
    public bool StaysOpenOnEdit { get; set; }
    public bool IsSelectionBoxHighlighted { get; }
    public bool IsDropDownOpen { get; set; }
    public DataTemplate SelectionBoxItemTemplate { get; }
    protected internal override bool HandlesScrolling { get; }
    protected internal override bool HasEffectiveKeyboardFocus { get; }
 
    public event EventHandler DropDownClosed;
    public event EventHandler DropDownOpened;
 
    public override void OnApplyTemplate();
    protected override DependencyObject GetContainerForItemOverride();
    protected override bool IsItemItsOwnContainerOverride(object item);
    protected override AutomationPeer OnCreateAutomationPeer();
    protected virtual void OnDropDownClosed(EventArgs e);
    protected virtual void OnDropDownOpened(EventArgs e);
    protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e);
    protected override void OnIsMouseCapturedChanged(DependencyPropertyChangedEventArgs e);
    protected override void OnKeyDown(KeyEventArgs e);
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e);
    protected override void OnPreviewKeyDown(KeyEventArgs e);
    protected override void OnSelectionChanged(SelectionChangedEventArgs e);
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
 
}
```

你给出的是更贴近 .NET 官方运行时源码的 `ComboBox` 完整类定义，相比公开精简版，补充了**模板部件契约（TemplatePart）、文本搜索前缀控制、复合焦点管理、全链路输入处理、滚动行为声明**等底层核心成员，完整还原了 ComboBox 逻辑层的全貌。这些成员是自定义控件模板、深度扩展交互、排查疑难问题的关键依据。

本文严格对照你列出的每一个成员，从官方定义、底层机制、工业场景价值三个维度逐行深度解析。

------

## 一、类级特性与模板契约

csharp:

```c#
[Localizability(LocalizationCategory.ComboBox)]
[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ComboBoxItem))]
[TemplatePart(Name = "PART_EditableTextBox", Type = typeof(TextBox))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
```

这四个特性完整定义了控件的元数据与视觉契约，其中两个 `TemplatePart` 是复合控件的核心设计精髓。

### 1. Localizability 本地化标记

- `LocalizationCategory.ComboBox` 向本地化工具声明该控件属于组合框类别，在做多语言适配时会按组合框规则扫描可本地化文本。
- 控件本身无固定硬编码文本，所有显示内容由业务数据与模板决定。

### 2. StyleTypedProperty 样式类型声明

- 向设计器声明 `ItemContainerStyle` 的目标容器类型为 `ComboBoxItem`，提供样式智能提示与编译期类型校验；
- 与 `ListBox`、`ListView` 遵循完全一致的 `ItemsControl` 体系约定。

### 3. TemplatePart 模板部件契约（核心）

WPF 控件严格遵循**逻辑与外观分离**的设计思想，`TemplatePart` 就是逻辑层与视觉层的约定契约：逻辑代码只通过固定名称查找模板中的元素，自定义控件模板时只要保留对应 `Name` 和 `Type` 的元素，所有原生功能就完全正常；如果删除或改名，对应功能会静默失效。

| 部件名称               | 类型      | 官方核心作用                                                 | 缺失后果                                            |
| :--------------------- | :-------- | :----------------------------------------------------------- | :-------------------------------------------------- |
| `PART_EditableTextBox` | `TextBox` | 可编辑模式下顶部输入框的核心载体，所有文本输入、自动补全、`Text` 属性同步都依赖该实例 | `IsEditable="True"` 时完全无法输入，`Text` 属性失效 |
| `PART_Popup`           | `Popup`   | 下拉面板的弹出载体，所有选项都渲染在该 Popup 内部；下拉展开 / 收起、弹出位置计算、屏幕边界适配都依赖它 | 下拉面板完全无法弹出，控件退化为普通文本选择框      |

> ⚠️ 自定义模板必知：这两个部件是 ComboBox 功能的「生命线」，自定义外观时可以修改样式、调整布局，但**绝对不能修改 Name 和删除对应元素**，否则功能会无报错失效，排查难度极高。

------

## 二、静态依赖属性全量解析

按职责分为四大类，全部为依赖属性，支持数据绑定、样式、动画。

### 2.1 下拉面板控制类

| 属性字段                    | 包装属性            | 类型     | 默认值       | 官方作用                                    | 工业场景说明                                                 |
| :-------------------------- | :------------------ | :------- | :----------- | :------------------------------------------ | :----------------------------------------------------------- |
| `IsDropDownOpenProperty`    | `IsDropDownOpen`    | `bool`   | `false`      | 控制下拉面板的展开 / 收起状态，支持双向绑定 | 可通过 ViewModel 直接控制下拉状态，实现扫码自动展开、校验失败自动展开等定制交互 |
| `MaxDropDownHeightProperty` | `MaxDropDownHeight` | `double` | 系统自动计算 | 下拉面板的最大高度，超出后显示垂直滚动条    | 长列表建议手动设为 250~400px，避免下拉占满屏幕，保证一次展示足够选项 |

### 2.2 可编辑模式控制类

| 属性字段                                  | 包装属性                          | 类型     | 默认值   | 官方作用                                       | 工业场景价值                                                 |
| :---------------------------------------- | :-------------------------------- | :------- | :------- | :--------------------------------------------- | :----------------------------------------------------------- |
| `IsEditableProperty`                      | `IsEditable`                      | `bool`   | `false`  | 是否启用可编辑输入模式，顶部显示文本框         | 长列表快速检索、支持自定义值的参数输入场景开启               |
| `IsReadOnlyProperty`                      | `IsReadOnly`                      | `bool`   | `false`  | 可编辑模式下文本框是否仅可读                   | 经典组合：`IsEditable="True"` + `IsReadOnly="True"` = 可搜索、不可修改，既保证检索效率，又避免非法输入，是工业参数选型的黄金配置 |
| `TextProperty`                            | `Text`                            | `string` | 空字符串 | 可编辑模式下文本框的内容                       | 绑定到 ViewModel 实现实时筛选、输入校验、模糊搜索            |
| `StaysOpenOnEditProperty`                 | `StaysOpenOnEdit`                 | `bool`   | `false`  | 输入文本时是否保持下拉展开                     | 实时筛选场景设为 `true`，边输入边展示匹配结果，符合搜索直觉  |
| `ShouldPreserveUserEnteredPrefixProperty` | `ShouldPreserveUserEnteredPrefix` | `bool`   | `false`  | 可编辑模式下是否保留用户输入前缀，关闭自动补全 | 输入配方编号、设备编码等精确编码时设为 `true`，避免自动补全干扰精确输入，配合下拉过滤更符合工控操作习惯 |

> 🔑 `ShouldPreserveUserEnteredPrefix` 细节：
>
> - `false`（默认）：输入前缀匹配到选项时，自动补全剩余文本并选中补全部分，继续输入会覆盖补全内容；
> - `true`：仅保留用户手动输入的文本，不自动补全，下拉列表自动过滤匹配项。

### 2.3 选择框显示类

这组属性全部为**只读内部属性**，由控件内部维护，用于驱动顶部选择框的渲染。

| 属性字段                               | 包装属性                       | 类型           | 官方作用                       | 说明                                                         |
| :------------------------------------- | :----------------------------- | :------------- | :----------------------------- | :----------------------------------------------------------- |
| `SelectionBoxItemProperty`             | `SelectionBoxItem`             | `object`       | 下拉收起时，选择框显示的数据项 | 内部存储当前选中项的显示内容，自定义控件模板时可绑定使用     |
| `SelectionBoxItemTemplateProperty`     | `SelectionBoxItemTemplate`     | `DataTemplate` | 选择框的内容模板               | 默认与下拉项模板一致，可通过样式单独定制选择框外观           |
| `SelectionBoxItemStringFormatProperty` | `SelectionBoxItemStringFormat` | `string`       | 选择框内容的格式化字符串       | 选中项更新时按格式渲染，下拉列表中仍使用 `ItemTemplate` 或 `DisplayMemberPath` |

------

## 三、实例属性全量解析

### 3.1 公共属性

除上述依赖属性对应的包装属性外，新增一个只读状态属性：

csharp:

```c#
public bool IsSelectionBoxHighlighted { get; }
```

- **官方作用**：指示顶部选择框是否处于高亮状态。
- **触发条件**：控件获得键盘焦点、鼠标悬浮在选择框上时为 `true`。
- **设计用途**：驱动控件模板的视觉状态，比如显示焦点边框、高亮背景，让用户清晰感知当前焦点位置。
- **工业价值**：工控深色主题中，可通过该属性统一控制焦点态外观，保证操作焦点清晰可见。

### 3.2 受保护内部属性（底层核心）

这两个属性是控件体系内部的行为声明，业务开发很少直接接触，但决定了控件的输入与滚动行为。

#### 1. HandlesScrolling

csharp:

```c#
protected internal override bool HandlesScrolling { get; }
```

- **官方作用**：向 WPF 输入与导航系统声明：控件自身是否负责处理滚动逻辑。
- **ComboBox 返回值**：`false`。
- **底层意义**：
  - ComboBox 本身不承载滚动区域，滚动逻辑完全由下拉 Popup 内部的 ScrollViewer 处理；
  - 键盘方向键默认用于切换选中项，而非滚动内容；
  - 对比：`ListBox` 该属性返回 `true`，因为自身内置 ScrollViewer，方向键同时控制选中与滚动。

#### 2. HasEffectiveKeyboardFocus

csharp:

```c#
protected internal override bool HasEffectiveKeyboardFocus { get; }
```

- **官方作用**：判断整个 ComboBox 控件是否持有**有效的键盘焦点**。
- **底层意义**：ComboBox 是复合控件，焦点可能在顶部文本框，也可能在下拉列表的条目上。该属性统一了「控件整体是否有焦点」的判断，用于驱动全局焦点视觉样式，避免因为焦点在内部子元素上导致焦点边框消失。
- **工业场景价值**：自定义工控主题时，可通过该属性统一控制焦点态外观，保证操作焦点清晰，适配触摸屏、无鼠标场景。

------

## 四、核心事件

csharp:

```c#
public event EventHandler DropDownClosed;
public event EventHandler DropDownOpened;
```

| 事件             | 触发时机           | 典型工业用法                                                 |
| :--------------- | :----------------- | :----------------------------------------------------------- |
| `DropDownOpened` | 下拉面板完全展开后 | 延迟加载下拉数据、异步拉取字典项，避免页面初始化时加载大量数据，提升启动速度 |
| `DropDownClosed` | 下拉面板完全收起后 | 校验输入内容合法性、同步选中值到业务模型、提交筛选条件       |

> 选中变更事件 `SelectionChanged` 继承自 `Selector`，是级联联动、主从交互的核心触发点。

------

## 五、核心方法逐行深度解析

按生命周期分为五大类，覆盖模板初始化、容器管理、下拉生命周期、输入交互、焦点管理。

### 5.1 模板生命周期：OnApplyTemplate

csharp:

```c#
public override void OnApplyTemplate();
```

- **触发时机**：控件模板加载完成时调用，是复合控件初始化的核心入口。
- **官方执行流程**：
  1. 调用基类方法，完成基础模板初始化；
  2. 通过 `GetTemplateChild("PART_EditableTextBox")` 获取文本框实例，缓存引用，绑定 `TextChanged`、`LostFocus` 等事件；
  3. 通过 `GetTemplateChild("PART_Popup")` 获取 Popup 实例，缓存引用，绑定 `Opened`、`Closed` 事件；
  4. 同步当前选中项到选择框与文本框，初始化下拉状态。
- **自定义模板关键**：这是模板部件契约的执行点。如果模板中缺少对应命名部件，这里会拿到 `null`，对应功能直接失效，且不会抛出异常，是自定义模板最常见的坑点。

### 5.2 条目容器生命周期

继承并实现 `ItemsControl` 的容器契约，负责下拉项的生成、准备与复用。

| 方法                                         | 官方实现                         | 作用                                           |
| :------------------------------------------- | :------------------------------- | :--------------------------------------------- |
| `GetContainerForItemOverride()`              | 返回 `new ComboBoxItem()`        | 指定下拉项的默认容器类型                       |
| `IsItemItsOwnContainerOverride(object item)` | 判断 `item is ComboBoxItem`      | 支持 XAML 直接添加静态 `<ComboBoxItem>` 子元素 |
| `PrepareContainerForItemOverride(...)`       | 绑定数据、应用样式、同步选中状态 | 容器生成 / 复用时初始化内容，适配 UI 虚拟化    |

### 5.3 下拉生命周期

csharp:

```c#
protected virtual void OnDropDownOpened(EventArgs e);
protected virtual void OnDropDownClosed(EventArgs e);
```

- **触发时机**：下拉完全展开 / 收起后调用。
- **官方默认逻辑**：
  - 展开时：将键盘焦点移入下拉列表，自动滚动到当前选中项，触发 `DropDownOpened` 公共事件；
  - 收起时：将焦点移回顶部选择框，校验输入内容，触发 `DropDownClosed` 公共事件。
- **扩展价值**：子类重写可实现数据懒加载、展开 / 收起动画、输入自动修正等自定义逻辑。

### 5.4 输入交互处理

这组方法是 ComboBox 交互体验的核心，分层处理键盘、鼠标输入。

#### 1. 键盘输入：OnPreviewKeyDown + OnKeyDown

csharp:

```c#
protected override void OnPreviewKeyDown(KeyEventArgs e);
protected override void OnKeyDown(KeyEventArgs e);
```

两个方法成对工作，分别在事件**隧道阶段**和**冒泡阶段**执行，分工明确：

- **`OnPreviewKeyDown`（隧道阶段，优先执行）**：

  拦截全局快捷键，避免被父控件拦截。主要处理 `F4`、`Alt+↓`展开 / 收起下拉等系统级快捷键。

- **`OnKeyDown`（冒泡阶段）**：

  处理常规输入逻辑：方向键切换选项、回车确认选中、Esc 取消收起、字符键文本搜索定位。

- **工业价值**：完整支持纯键盘操作，适配工控机无鼠标、触摸屏操作场景，符合工业软件操作规范。

#### 2. 鼠标输入：OnMouseLeftButtonUp

csharp:

```c#
protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e);
```

- **官方作用**：处理鼠标左键抬起事件，是下拉展开 / 收起的主要触发入口。
- **执行逻辑**：
  1. 点击选择框区域：切换 `IsDropDownOpen` 状态；
  2. 点击下拉内的选项：选中对应数据项，自动收起下拉；
  3. 标记事件已处理，避免向上层冒泡。

#### 3. 鼠标捕获：OnIsMouseCapturedChanged

csharp:

```c#
protected override void OnIsMouseCapturedChanged(DependencyPropertyChangedEventArgs e);
```

- **触发时机**：鼠标捕获状态发生变化时。
- **核心机制**：下拉展开时，控件会调用 `Mouse.Capture()` 捕获鼠标，所有鼠标事件都会发送到 ComboBox；当点击控件外部区域时，鼠标捕获自动丢失，触发该方法，内部自动收起下拉。
- **设计价值**：这是「点击外部自动关闭下拉」的底层实现，无需额外监听窗口点击事件，是所有下拉类控件的标准实现模式。

### 5.5 焦点管理

csharp:

```c#
protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e);
```

- **触发时机**：控件内部的键盘焦点发生迁移时。
- **官方执行逻辑**：
  1. 更新 `IsSelectionBoxHighlighted` 状态，驱动焦点视觉样式；
  2. 可编辑模式下，焦点从文本框移走时，校验输入内容，同步更新 `SelectedItem`；
  3. 下拉收起时，将焦点统一移回顶部选择框。
- **底层意义**：保证复合控件的焦点状态统一，避免内部焦点切换导致外观异常。

### 5.6 自动化支持

csharp:

```c#
protected override AutomationPeer OnCreateAutomationPeer();
```

- 返回 `ComboBoxAutomationPeer`，提供 UI 自动化支持，适配无障碍访问与自动化测试框架。

------

## 六、核心底层工作机制补充

### 6.1 模板部件契约机制

WPF 控件的核心设计思想是「逻辑与外观彻底分离」：

- 逻辑层（C# 代码）只关心功能实现，不依赖任何具体视觉元素，只通过约定的名称从模板中获取必要部件；
- 视觉层（XAML 模板）可以完全自定义外观、布局、动画，只要保留约定名称和类型的部件，所有原生功能就 100% 正常。
- 这种设计既保证了极高的外观定制自由度，又保证了功能的一致性与稳定性，是 WPF 控件体系的精髓。

### 6.2 可编辑模式的文本同步规则

`IsEditable="True"` 时，`Text` 与 `SelectedItem` 的双向同步遵循明确的优先级：

1. 选中下拉项 → 自动将 `DisplayMemberPath` 对应的值写入 `Text`；
2. 输入文本 → 实时匹配数据源，匹配到第一项则自动更新 `SelectedItem`；
3. 输入文本不匹配 → `SelectedItem` 为 null，仅 `Text` 保留用户输入；
4. `ShouldPreserveUserEnteredPrefix="True"` → 关闭自动补全，只过滤下拉列表，不修改用户输入文本。

### 6.3 下拉弹出与自动关闭流程

plaintext:

```tex
点击选择框 / 按 F4
    ↓
设置 IsDropDownOpen = true
    ↓
PART_Popup 弹出，计算位置（下方空间不足则向上弹出）
    ↓
捕获鼠标，监听全局鼠标事件
    ↓
点击内部选项 → 选中 → 释放鼠标捕获 → 收起下拉
点击外部区域 → 鼠标捕获丢失 → 自动收起下拉
```

------

## 总结

这份完整源码级定义完整展现了 `ComboBox` 的复合控件本质：它在 `Selector` 单选体系的基础上，通过 `Popup` 折叠选项面板，通过 `TextBox` 扩展可编辑输入，通过模板部件契约实现逻辑与外观分离。理解 `TemplatePart` 契约、输入分层处理、鼠标捕获自动关闭等底层机制，是自定义控件模板、深度扩展交互、排查疑难问题的核心基础。