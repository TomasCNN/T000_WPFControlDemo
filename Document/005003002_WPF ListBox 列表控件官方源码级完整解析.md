# 005003002_WPF `ListBox` 列表控件官方源码级完整解析

`ListBox` 是 WPF 最经典的可交互列表控件，是 **`Selector` 选择基类的标准完整实现 **：在 `ItemsControl` 集合呈现 + `Selector` 选中管理的完整能力之上，落地了标准条目容器、多选模式、键盘导航、内置滚动等完整列表交互能力，是工业软件中数据列表交互的首选控件。

本文基于 .NET 官方源码，从类定义元数据、依赖属性、条目容器、核心方法、内部机制五个维度逐行深度解析。

------

## 一、官方类定义总览

### 1.1 完整类签名（官方原生定义）

csharp：

```c#
namespace System.Windows.Controls
{
    [System.Windows.LocalizabilityAttribute(System.Windows.LocalizationCategory.None, Readability = System.Windows.Readability.Unreadable)]
    [System.Windows.StyleTypedPropertyAttribute(Property = "ItemContainerStyle", StyleTargetType = typeof(System.Windows.Controls.ListBoxItem))]
    public class ListBox : System.Windows.Controls.Primitives.Selector
    {
        // 新增静态依赖属性
        public static readonly System.Windows.DependencyProperty SelectionModeProperty;
        public static readonly System.Windows.DependencyProperty SelectedItemsProperty;

        // 构造函数
        public ListBox();

        // 新增公共属性
        public System.Windows.Controls.SelectionMode SelectionMode { get; set; }
        public System.Collections.IList SelectedItems { get; }

        // 公共方法
        public void ScrollIntoView(object item);

        // 受保护重写方法
        protected override System.Windows.DependencyObject GetContainerForItemOverride();
        protected override bool IsItemItsOwnContainerOverride(object item);
        protected override void PrepareContainerForItemOverride(System.Windows.DependencyObject element, object item);
        protected override void ClearContainerForItemOverride(System.Windows.DependencyObject element, object item);
        protected override void OnSelectionChanged(System.Windows.Controls.SelectionChangedEventArgs e);
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e);
        protected override AutomationPeer OnCreateAutomationPeer();
    }
}
```

### 1.2 核心元数据（官方精确值）

| 项               | 官方精确值                                                   | 工业场景说明                                      |
| :--------------- | :----------------------------------------------------------- | :------------------------------------------------ |
| **命名空间**     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                              |
| **程序集**       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                                |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → Selector → ListBox` | 完整继承集合呈现与选择能力                        |
| **条目容器**     | `ListBoxItem`                                                | 每个数据项的默认 UI 容器，继承自 `ContentControl` |
| **默认布局面板** | `StackPanel`（垂直排列）                                     | 可通过 `ItemsPanel` 替换为虚拟化面板              |
| **默认选择模式** | `SelectionMode.Single`（单选）                               | 支持切换为简单多选 / 扩展多选                     |
| **内置滚动**     | 默认控件模板包含 `ScrollViewer`                              | 天生支持滚动，无需额外包裹                        |
| **工业核心场景** | 设备台账、报警列表、配方选择、批量操作、数据筛选             | 绝大多数列表交互场景的首选控件                    |

### 1.3 类级特性深度解析

1. **`[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ListBoxItem))]`**
   - 官方作用：向设计器声明 `ItemContainerStyle` 属性的目标类型为 `ListBoxItem`，提供样式智能提示与类型校验。
   - 与基类差异：`Selector` 基类声明的目标类型是 `FrameworkElement`，`ListBox` 将其具体化为 `ListBoxItem`，是容器类型落地的标志。
2. **`[Localizability(LocalizationCategory.None)]`**
   - 控件本身无固定文本内容，所有显示文本由业务数据决定，无需框架级本地化。

------

## 二、核心依赖属性全量解析

`ListBox` 自身**新增 2 个依赖属性**，其余全部继承自 `ItemsControl` 与 `Selector`。以下按「新增属性」和「继承高频属性」分类说明。

### 2.1 ListBox 新增核心属性

#### 1. SelectionModeProperty

csharp:

```c#
public static readonly DependencyProperty SelectionModeProperty;
public SelectionMode SelectionMode { get; set; }
```

- **类型**：`SelectionMode` 枚举
- **默认值**：`SelectionMode.Single`
- **官方作用**：控制列表的选择交互模式。

**枚举值完整说明：**

| 枚举值           | 交互行为                                                     | 典型工业场景                           |
| :--------------- | :----------------------------------------------------------- | :------------------------------------- |
| `Single`（默认） | 单选，同一时间仅能选中一项；点击其他项自动取消上一项         | 主从详情、设备选择、配方下拉           |
| `Multiple`       | 简单多选，单击直接切换选中状态，无需按住 Ctrl 键             | 快速批量勾选、标签多选                 |
| `Extended`       | 扩展多选：・单击单选・按住 Ctrl 可点选多项・按住 Shift 可连续选中区间 | 工业批量操作标准模式，符合桌面操作习惯 |

> 💡 工业最佳实践：涉及批量确认、批量导出、批量删除的场景，统一使用 `Extended` 模式，兼顾操作效率与用户习惯。

#### 2. SelectedItemsProperty

csharp:

```c#
public static readonly DependencyProperty SelectedItemsProperty;
public IList SelectedItems { get; }
```

- **类型**：`IList`（非泛型集合接口）
- **默认值**：空集合实例
- **官方作用**：多选模式下，返回所有已选中的数据项集合。
- **关键性质**：**只读依赖属性**，只能读取，不能直接赋值，也不能直接做双向绑定。
- **工业场景用法**：
  - 后台代码中通过 `listBox.SelectedItems` 遍历获取所有选中项，执行批量操作；
  - MVVM 场景需通过附加属性、行为或 `SelectionChanged` 事件同步到 ViewModel。

> ⚠️ 常见误区：`SelectedItem` 是单选语义，多选场景下仅返回第一个选中项；要获取全部选中项必须使用 `SelectedItems`。

### 2.2 继承的高频核心属性

| 分类     | 属性                                                         | 来源         | 工业场景作用                          |
| :------- | :----------------------------------------------------------- | :----------- | :------------------------------------ |
| 数据绑定 | `ItemsSource`                                                | ItemsControl | 绑定业务数据集合，MVVM 标准入口       |
| 内容呈现 | `ItemTemplate` / `DisplayMemberPath`                         | ItemsControl | 自定义条目外观 / 简化显示单个字段     |
| 选中单选 | `SelectedItem` / `SelectedIndex` / `SelectedValue` / `SelectedValuePath` | Selector     | 单选模式下的数据绑定                  |
| 容器样式 | `ItemContainerStyle`                                         | ItemsControl | 自定义 `ListBoxItem` 的样式与交互效果 |
| 布局面板 | `ItemsPanel`                                                 | ItemsControl | 替换布局面板，开启 UI 虚拟化          |
| 交替行   | `AlternationCount` / `AlternationIndex`                      | ItemsControl | 奇偶行交替背景，提升长列表可读性      |
| 文本搜索 | `IsTextSearchEnabled`                                        | Selector     | 键盘输入快速定位条目，适合长列表      |
| 视图同步 | `IsSynchronizedWithCurrentItem`                              | Selector     | 多控件共享数据源时自动联动选中        |

------

## 三、配套条目容器：`ListBoxItem` 深度解析

`ListBoxItem` 是 `ListBox` 的默认条目容器，继承自 `ContentControl`，是选中状态与交互的 UI 载体。

### 3.1 官方类定义精简

csharp:

```c#
public class ListBoxItem : ContentControl
{
    public static readonly DependencyProperty IsSelectedProperty;

    public bool IsSelected { get; set; }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e);
    protected override void OnMouseEnter(MouseEventArgs e);
    protected virtual void OnSelected(RoutedEventArgs e);
    protected virtual void OnUnselected(RoutedEventArgs e);
}
```

### 3.2 核心成员说明

1. **`IsSelected` 属性**
   - 本质是 `Selector.IsSelected` 附加属性的强类型包装，内部直接操作附加属性；
   - 样式触发器、数据绑定的核心目标属性。
2. **`OnSelected / OnUnselected` 虚方法**
   - 对应 `Selector.SelectedEvent` / `Selector.UnselectedEvent` 附加路由事件的类处理；
   - 选中 / 取消选中时触发，子类可重写扩展自定义逻辑。
3. **鼠标交互逻辑**
   - 重写左键按下事件，触发选中逻辑；
   - 配合 `ListBox` 的选择模式，实现单击、Ctrl + 点击、Shift + 点击等不同交互。

------

## 四、核心方法逐行解析

### 4.1 公共方法

#### `public void ScrollIntoView(object item);`

- **官方作用**：将指定的数据项滚动到可视区域内。
- **典型工业场景**：
  - 新报警产生时，自动滚动到最新一条；
  - 搜索定位后，自动滚动到匹配项；
  - 程序启动时滚动到当前选中项。
- **注意**：虚拟化模式下同样有效，框架会先生成对应容器再滚动。

### 4.2 受保护重写方法（核心扩展点）

`ListBox` 重写了 `Selector` 的 6 个核心虚方法，将基类的抽象能力落地为具体的列表行为。

#### 1. GetContainerForItemOverride

csharp:

```c#
protected override DependencyObject GetContainerForItemOverride();
```

- **官方实现**：直接返回 `new ListBoxItem()`。
- **设计意义**：将基类抽象的「条目容器」具体化为 `ListBoxItem`，是 `ListBox` 作为具体控件的标志。

#### 2. IsItemItsOwnContainerOverride

csharp:

```c#
protected override bool IsItemItsOwnContainerOverride(object item);
```

- **官方实现**：判断 `item is ListBoxItem`，是则返回 true。
- **作用**：支持直接在 XAML 中添加 `<ListBoxItem>` 子元素，无需再包装一层容器。

#### 3. PrepareContainerForItemOverride

csharp:

```c#
protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
```

- **官方执行逻辑**：
  1. 调用基类方法，完成数据上下文、样式、模板、选中状态的基础准备；
  2. 同步 `IsSelected` 属性到 `ListBoxItem` 容器；
  3. 绑定容器的事件处理。
- **虚拟化适配**：容器复用时，从持久化存储恢复该数据项的选中状态。

#### 4. ClearContainerForItemOverride

csharp:

```c#
protected override void ClearContainerForItemOverride(DependencyObject element, object item);
```

- **官方执行逻辑**：
  1. 清除容器的 `IsSelected` 状态，避免复用时残留；
  2. 将当前选中状态持久化到 `IContainItemStorage`，绑定到数据项；
  3. 调用基类方法，清理数据上下文与模板。
- ⚠️ **自定义扩展必知**：如果子类重写 Prepare 附加了自定义属性 / 事件，必须在此方法中对应清理，否则虚拟化滚动必然出现状态错乱与内存泄漏。

#### 5. OnSelectionChanged

csharp:

```c#
protected override void OnSelectionChanged(SelectionChangedEventArgs e);
```

- **官方执行逻辑**：
  1. 更新内部 `SelectedItems` 集合，同步添加 / 移除对应项；
  2. 调用基类方法，触发 `SelectionChanged` 路由事件；
  3. 更新键盘焦点与选中项的视觉状态。
- **多选核心**：`SelectedItems` 集合的维护逻辑就在此方法中，是多选能力的核心实现。

#### 6. OnKeyDown

csharp:

```c#
protected override void OnKeyDown(KeyEventArgs e);
```

- **官方实现**：完整的键盘导航逻辑
  - 方向键：上下移动选中项；
  - 空格：切换当前项选中状态；
  - Ctrl + 方向键：移动焦点不改变选中；
  - Shift + 方向键：连续选中；
  - Home/End：跳到首项 / 末项；
  - 字母键：文本搜索定位。
- 工业价值：支持纯键盘操作，适合工控机无鼠标的操作场景。

------

## 五、核心内部工作机制

### 5.1 多选模式的状态管理

1. **单选模式**：选中新项时，自动取消上一项的选中状态，`SelectedItems` 集合始终只有 0 或 1 项。
2. **多选模式**：
   - 点击项时切换其 `IsSelected` 状态，不影响其他项；
   - 每次变更都会更新 `SelectedItems` 集合；
   - Shift 连选时，计算起点到终点的区间，批量设置选中状态。
3. **虚拟化兼容**：选中状态始终绑定在数据项上，而非容器上；容器滚出屏幕时状态持久化，滚入时恢复，多选状态不会因为滚动丢失。

### 5.2 内置滚动机制

`ListBox` 的默认控件模板中包含 `ScrollViewer`，因此天生支持滚动：

- 无需手动包裹 `ScrollViewer`；
- `ScrollIntoView` 方法内部就是通过查找模板中的 `ScrollViewer` 实现滚动定位；
- 配合 `VirtualizingStackPanel` 时，自动启用滚动虚拟化，只生成可见区域的容器。

### 5.3 键盘导航与文本搜索

- 方向键导航基于选中索引实现，支持循环、边界处理；
- 文本搜索基于 `Selector` 基类的 `IsTextSearchEnabled` 属性，输入字符自动匹配 `DisplayMemberPath` 对应的属性，定位到第一个匹配项。

------

## 六、工业场景最佳实践与常见坑点

### 6.1 最佳实践

1. **大数据量必开虚拟化**

   xaml:

   ```xaml
   <ListBox.ItemsPanel>
       <ItemsPanelTemplate>
           <VirtualizingStackPanel VirtualizationMode="Recycling"/>
       </ItemsPanelTemplate>
   </ListBox.ItemsPanel>
   ```

   500 条以上数据必须开启，万级数据内存占用降低 90% 以上。

2. **单选用绑定，多选用事件 / 附加属性**

   - 单选场景直接绑定 `SelectedItem` 到 ViewModel，纯 MVVM 无后台代码；
   - 多选场景推荐用附加属性将 `SelectedItems` 同步到 VM 集合，保持架构整洁。

3. **样式与数据分离**

   - 选中高亮、交替行、状态色全部通过 `ItemContainerStyle` 触发器实现，不要在数据模型中加 UI 相关属性。

4. **批量操作轻量处理**

   - `SelectionChanged` 事件中禁止执行耗时操作，PLC 通信、数据库查询必须异步执行，避免阻塞 UI。

### 6.2 常见坑点

1. **SelectedItems 无法直接双向绑定**
   - 原因：该属性是只读依赖属性，不支持 Set 操作；
   - 解决方案：使用自定义附加属性，监听 `SelectionChanged` 事件，将集合同步到 ViewModel。
2. **虚拟化下遍历容器返回 null**
   - 现象：用 `ItemContainerGenerator.ContainerFromIndex` 获取容器，不可见项返回 null；
   - 原因：虚拟化模式只生成可见容器；
   - 解决方案：永远操作数据层，不要直接操作 UI 容器。
3. **多选时 SelectedItem 只有第一个**
   - 原因：`SelectedItem` 是单选语义设计，多选场景必须使用 `SelectedItems` 集合。
4. **自定义容器状态错乱**
   - 现象：滚动列表后，行颜色、选中状态出现混乱；
   - 原因：重写 `PrepareContainerForItemOverride` 后，未对应重写 `ClearContainerForItemOverride` 清理状态；
   - 解决方案：成对实现准备与清理方法，所有自定义属性都要在回收时重置。

------

## 总结

`ListBox` 是 `Selector` 抽象基类的标准落地实现，它将基类的集合呈现、选中管理能力，具体化为带标准容器、支持多选、内置滚动、键盘友好的可用列表控件。理解它的容器生命周期、多选状态管理、虚拟化兼容机制，不仅能正确使用该控件，也是自定义工业专用列表的重要参考。