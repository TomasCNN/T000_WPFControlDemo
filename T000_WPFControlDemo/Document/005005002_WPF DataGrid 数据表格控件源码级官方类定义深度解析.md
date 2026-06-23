# 005005002_WPF `DataGrid` 数据表格控件源码级官方类定义深度解析

`DataGrid` 是 WPF 原生功能最完整的**可编辑数据表格控件**，直接继承自 `MultiSelector` 多选基类，在 `ItemsControl` 集合呈现 + `Selector` 选择体系的基础上，提供了原生单元格编辑、列类型体系、内置排序、行详情、数据验证、行列双级虚拟化等完整表格能力。

它是工业软件中**配方参数编辑、设备配置、可编辑生产台账**场景的首选控件；对比 `ListView+GridView`，其核心差异在于原生编辑能力 —— 只读展示优先用更轻量的 `ListView`，需要数据录入 / 修改时再用 `DataGrid`。

本文基于 .NET 官方源码，从类定义元数据、依赖属性、事件体系、核心方法、配套类型、底层机制六个维度逐行深度解析。

------

## 一、官方类定义总览与核心元数据

### 1.1 完整类签名（官方原生定义）

csharp:

```c#
namespace System.Windows.Controls
{
    [System.Windows.LocalizabilityAttribute(System.Windows.LocalizationCategory.None, Readability = System.Windows.Readability.Unreadable)]
    [System.Windows.StyleTypedPropertyAttribute(Property = "RowStyle", StyleTargetType = typeof(System.Windows.Controls.DataGridRow))]
    [System.Windows.StyleTypedPropertyAttribute(Property = "CellStyle", StyleTargetType = typeof(System.Windows.Controls.DataGridCell))]
    public class DataGrid : System.Windows.Controls.Primitives.MultiSelector
    {
        // 核心依赖属性字段
        public static readonly System.Windows.DependencyProperty AutoGenerateColumnsProperty;
        public static readonly System.Windows.DependencyProperty IsReadOnlyProperty;
        public static readonly System.Windows.DependencyProperty CanUserAddRowsProperty;
        public static readonly System.Windows.DependencyProperty CanUserDeleteRowsProperty;
        public static readonly System.Windows.DependencyProperty CanUserReorderColumnsProperty;
        public static readonly System.Windows.DependencyProperty CanUserResizeColumnsProperty;
        public static readonly System.Windows.DependencyProperty CanUserResizeRowsProperty;
        public static readonly System.Windows.DependencyProperty CanUserSortColumnsProperty;
        public static readonly System.Windows.DependencyProperty SelectionModeProperty;
        public static readonly System.Windows.DependencyProperty SelectionUnitProperty;
        public static readonly System.Windows.DependencyProperty ColumnsProperty;
        public static readonly System.Windows.DependencyProperty RowStyleProperty;
        public static readonly System.Windows.DependencyProperty CellStyleProperty;
        public static readonly System.Windows.DependencyProperty AlternatingRowBackgroundProperty;
        public static readonly System.Windows.DependencyProperty EnableRowVirtualizationProperty;
        public static readonly System.Windows.DependencyProperty EnableColumnVirtualizationProperty;
        public static readonly System.Windows.DependencyProperty GridLinesVisibilityProperty;
        public static readonly System.Windows.DependencyProperty HeadersVisibilityProperty;
        public static readonly System.Windows.DependencyProperty RowDetailsTemplateProperty;
        public static readonly System.Windows.DependencyProperty RowDetailsVisibilityModeProperty;

        // 构造函数
        public DataGrid();

        // 核心公共属性
        public bool AutoGenerateColumns { get; set; }
        public bool IsReadOnly { get; set; }
        public bool CanUserAddRows { get; set; }
        public bool CanUserDeleteRows { get; set; }
        public bool CanUserReorderColumns { get; set; }
        public bool CanUserResizeColumns { get; set; }
        public bool CanUserResizeRows { get; set; }
        public bool CanUserSortColumns { get; set; }
        public DataGridSelectionMode SelectionMode { get; set; }
        public DataGridSelectionUnit SelectionUnit { get; set; }
        public System.Collections.ObjectModel.ObservableCollection<DataGridColumn> Columns { get; }
        public Style RowStyle { get; set; }
        public Style CellStyle { get; set; }
        public Brush AlternatingRowBackground { get; set; }
        public bool EnableRowVirtualization { get; set; }
        public bool EnableColumnVirtualization { get; set; }
        public DataGridGridLinesVisibility GridLinesVisibility { get; set; }
        public DataGridHeadersVisibility HeadersVisibility { get; set; }
        public DataTemplate RowDetailsTemplate { get; set; }
        public DataGridRowDetailsVisibilityMode RowDetailsVisibilityMode { get; set; }

        // 核心事件
        public event EventHandler<DataGridBeginningEditEventArgs> BeginningEdit;
        public event EventHandler<DataGridCellEditEndingEventArgs> CellEditEnding;
        public event EventHandler<DataGridRowEditEndingEventArgs> RowEditEnding;
        public event EventHandler<DataGridRowEventArgs> LoadingRow;
        public event EventHandler<DataGridRowEventArgs> UnloadingRow;
        public event DataGridSortingEventHandler Sorting;
        public event EventHandler<DataGridAutoGeneratingColumnEventArgs> AutoGeneratingColumn;

        // 公共方法
        public bool BeginEdit();
        public bool CommitEdit();
        public void CancelEdit();
        public void ScrollIntoView(object item);
        public new void SelectAll();
        public new void UnselectAll();

        // 受保护重写方法
        protected override DependencyObject GetContainerForItemOverride();
        protected override bool IsItemItsOwnContainerOverride(object item);
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
        protected override void ClearContainerForItemOverride(DependencyObject element, object item);
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e);
        protected override AutomationPeer OnCreateAutomationPeer();
    }
}
```

### 1.2 核心元数据（官方精确值）

| 项               | 官方精确值                                                   | 工业场景关键说明                               |
| :--------------- | :----------------------------------------------------------- | :--------------------------------------------- |
| **命名空间**     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                           |
| **程序集**       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                             |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → Selector → MultiSelector → DataGrid` | 完整继承集合呈现、多选管理、虚拟化基础能力     |
| **默认行容器**   | `DataGridRow`                                                | 每条数据对应一个行容器，承载一行所有单元格     |
| **单元格容器**   | `DataGridCell`                                               | 每个数据字段对应一个单元格容器，承载编辑与展示 |
| **核心设计**     | 行列二维表格模型 + 原生编辑生命周期 + 双级虚拟化             | 功能最完整的原生列表类控件                     |
| **工业核心场景** | 配方参数编辑、设备配置表、可编辑生产台账、数据录入界面       | 所有需要表格编辑的业务场景                     |

### 1.3 类级特性深度解析

1. **双 `[StyleTypedProperty]` 声明**
   - 分别指定 `RowStyle` 目标类型为 `DataGridRow`、`CellStyle` 目标类型为 `DataGridCell`；
   - 相比 `ListView` 仅支持行容器样式，`DataGrid` 将样式粒度细化到**行 + 单元格**两级，定制能力更强。
2. **`[Localizability(LocalizationCategory.None)]`**
   - 控件本身无固定本地化文本，所有内容由业务数据与列定义决定，无需框架级本地化处理。

------

## 二、核心依赖属性全量深度解析

按功能分为六大类，全部为依赖属性，完整支持数据绑定、样式、动画。

### 2.1 数据生成与编辑权限类

控制表格的编辑能力边界，是工业场景最核心的配置项，直接决定操作规范性与数据安全性。

| 属性                  | 类型   | 默认值  | 官方作用                                               | 工业场景最佳实践                                             |
| :-------------------- | :----- | :------ | :----------------------------------------------------- | :----------------------------------------------------------- |
| `AutoGenerateColumns` | `bool` | `true`  | 是否根据数据源的属性反射自动生成列                     | **生产环境必须设为 `false`**，手动定义列顺序、类型、格式更可控，避免反射生成的列不符合业务规范 |
| `IsReadOnly`          | `bool` | `false` | 全局是否禁止所有单元格编辑                             | 纯展示表格设为 `true`；可编辑表格通过单列 `IsReadOnly` 控制关键字段只读 |
| `CanUserAddRows`      | `bool` | `true`  | 是否在表格底部显示空白新增行，支持用户直接回车新增数据 | 工业场景大多设为 `false`，通过「新增」按钮统一执行校验后新增，避免脏数据 |
| `CanUserDeleteRows`   | `bool` | `true`  | 是否允许用户按 Delete 键直接删除行                     | 关键业务表格必须关闭，删除操作需走业务按钮 + 二次确认流程    |
| `CanUserResizeRows`   | `bool` | `false` | 是否允许拖动行高                                       | 工业表格统一固定行高，保持界面规整，一般保持默认关闭         |

> 🔑 优先级规则：**单列 `IsReadOnly` > 全局 `IsReadOnly`**。即全局可编辑时，可单独锁定某列；全局只读时，单列无法单独开启编辑。

### 2.2 列交互与布局类

控制列的行为、布局与交互能力，对应 `GridView` 的列能力但更丰富。

| 属性                    | 类型                                   | 默认值         | 官方作用                                   | 工业场景说明                                                 |
| :---------------------- | :------------------------------------- | :------------- | :----------------------------------------- | :----------------------------------------------------------- |
| `Columns`               | `ObservableCollection<DataGridColumn>` | 空集合         | 所有列定义的集合，表格的核心配置载体       | 手动定义列的唯一入口，支持文本、复选框、下拉、模板等多种列类型；集合变更自动刷新 UI |
| `CanUserReorderColumns` | `bool`                                 | `true`         | 是否允许拖动列头调整列顺序                 | 关键业务表格设为 `false`，固定列序防止操作人员误操作打乱布局 |
| `CanUserResizeColumns`  | `bool`                                 | `true`         | 是否允许拖动列边缘调整列宽                 | 一般开启，方便查看长内容；固定布局场景可关闭                 |
| `CanUserSortColumns`    | `bool`                                 | `true`         | 是否允许点击列头排序                       | 查询类表格建议开启，提升数据查看效率                         |
| `ColumnWidth`           | `DataGridLength`                       | `SizeToHeader` | 全局默认列宽，支持固定值、`Auto`、星号比例 | 大数据量避免大量 `Auto` 列，会逐行计算宽度导致性能下降       |

### 2.3 选择交互类

在 `MultiSelector` 多选能力基础上，扩展了更细粒度的选择单位控制。

| 属性                                               | 类型                    | 默认值     | 官方作用   | 说明                                                         |
| :------------------------------------------------- | :---------------------- | :--------- | :--------- | :----------------------------------------------------------- |
| `SelectionMode`                                    | `DataGridSelectionMode` | `Extended` | 选择模式   | 枚举值：`Single` 单选；`Extended` 扩展多选（Ctrl 点选、Shift 连选） |
| `SelectionUnit`                                    | `DataGridSelectionUnit` | `FullRow`  | 选择单位   | 枚举值：`FullRow` 整行选中（工业默认）；`Cell` 单元格级选中；`CellOrRowHeader` 行头 + 单元格 |
| `SelectedItem` / `SelectedItems` / `SelectedIndex` | -                       | -          | 选中数据项 | 全部继承自 `Selector` / `MultiSelector`，用法与 `ListBox` 完全一致 |

### 2.4 视觉样式类

提供行、单元格、表头、网格线等全层级样式控制，粒度远细于 `ListView`。

| 属性                                   | 类型                          | 官方作用                                                     |
| :------------------------------------- | :---------------------------- | :----------------------------------------------------------- |
| `RowStyle`                             | `Style`                       | 行容器 `DataGridRow` 的统一样式，控制行高、背景、选中态、高度等 |
| `CellStyle`                            | `Style`                       | 单元格 `DataGridCell` 的统一样式，控制内边距、字体、对齐、边框等 |
| `AlternatingRowBackground`             | `Brush`                       | 交替行背景色，直接设置奇偶行不同背景，比 `ListView` 的交替行实现更简单 |
| `AlternationCount`                     | `int`                         | 交替行周期，默认 2，可配置多行交替                           |
| `GridLinesVisibility`                  | `DataGridGridLinesVisibility` | 网格线显示方式：`Horizontal` / `Vertical` / `All` / `None`   |
| `HeadersVisibility`                    | `DataGridHeadersVisibility`   | 表头显示方式：`Column` 仅列头 / `Row` 仅行头 / `All` / `None` |
| `ColumnHeaderStyle` / `RowHeaderStyle` | `Style`                       | 列头 / 行头的统一样式                                        |

### 2.5 行详情类

`DataGrid` 特色功能，支持每行展开显示二级详情面板。

| 属性                       | 类型                               | 官方作用                                                     |
| :------------------------- | :--------------------------------- | :----------------------------------------------------------- |
| `RowDetailsTemplate`       | `DataTemplate`                     | 行展开后的详情内容模板                                       |
| `RowDetailsVisibilityMode` | `DataGridRowDetailsVisibilityMode` | 详情显示模式：`VisibleWhenSelected` 选中时展开（默认）；`Visible` 全部展开；`Collapsed` 全部折叠 |
| `AreRowDetailsFrozen`      | `bool`                             | 详情面板是否随横向滚动固定，避免滚动后看不到详情             |

### 2.6 性能与虚拟化类

大数据量表格的核心优化开关，工业历史数据场景必备。

| 属性                                        | 类型     | 默认值             | 官方作用                                 | 工业最佳实践                                                 |
| :------------------------------------------ | :------- | :----------------- | :--------------------------------------- | :----------------------------------------------------------- |
| `EnableRowVirtualization`                   | `bool`   | `true`（WPF 4.0+） | 是否开启行虚拟化，仅生成可见区域的行容器 | 默认开启，万级数据的性能核心保障；关闭会全量生成 UI，内存暴涨 |
| `EnableColumnVirtualization`                | `bool`   | `false`            | 是否开启列虚拟化，仅生成可见列的单元格   | 列数 > 20 时开启，列数少时关闭避免横向滚动的渲染开销         |
| `VirtualizingStackPanel.VirtualizationMode` | 附加属性 | `Recycling`        | 虚拟化回收模式，复用行 / 单元格容器      | 保持默认 `Recycling`，大幅减少对象创建与 GC 压力             |

------

## 三、核心事件体系全解析

按生命周期分为三类，是业务逻辑接入与表格定制的核心入口。

### 3.1 编辑生命周期事件

控制数据编辑的全流程，支持校验、拦截、初始化。

| 事件                   | 触发时机                                         | 典型工业用法                                                 |
| :--------------------- | :----------------------------------------------- | :----------------------------------------------------------- |
| `BeginningEdit`        | 单元格进入编辑状态**之前**触发，可取消           | 编辑前校验权限，关键参数禁止普通操作员修改；不符合条件时 `e.Cancel = true` 取消编辑 |
| `PreparingCellForEdit` | 单元格进入编辑状态、编辑控件已生成时触发         | 初始化编辑控件的默认值、下拉选项、输入范围限制               |
| `CellEditEnding`       | 单元格编辑结束、提交到数据源**之前**触发，可取消 | 单元格级数据校验，数值超量程、格式非法时取消提交，提示错误   |
| `RowEditEnding`        | 整行编辑提交**之前**触发                         | 行级数据完整性校验，关联字段逻辑校验，事务性提交             |

### 3.2 容器生命周期事件

与 UI 虚拟化强相关，自定义行状态必须成对处理，否则滚动必然出现状态错乱。

| 事件                   | 触发时机                             | 典型工业用法                                                 |
| :--------------------- | :----------------------------------- | :----------------------------------------------------------- |
| `AutoGeneratingColumn` | 自动生成列模式下，每生成一列触发一次 | 自动生成列时修改列头文本、设置显示格式、隐藏不需要的列       |
| `LoadingRow`           | 行容器生成 / 从回收池取出复用时触发  | 自定义行号、行级状态标记、特殊行样式（如报警行标红）         |
| `UnloadingRow`         | 行容器滚出屏幕、进入回收池时触发     | 清理行的自定义状态、解绑事件、释放资源，**必须与 LoadingRow 成对实现** |

> ⚠️ 虚拟化关键提醒：
>
> 不要通过后台代码直接修改 `DataGridRow` 的属性（如背景色），否则滚动回收后状态会残留到其他行。正确做法是通过数据绑定 + 样式触发器实现，或在 `LoadingRow` 中赋值、`UnloadingRow` 中重置。

### 3.3 交互事件

| 事件                | 触发时机                   | 典型用法                                         |
| :------------------ | :------------------------- | :----------------------------------------------- |
| `Sorting`           | 点击列头排序时触发，可取消 | 自定义排序逻辑、多列组合排序、服务端分页排序     |
| `SelectionChanged`  | 选中项变化时触发           | 选中联动详情面板，继承自 `Selector`              |
| `LoadingRowDetails` | 行详情展开时触发           | 异步加载详情数据，避免一次性加载所有详情拖慢性能 |

------

## 四、核心方法逐行解析

### 4.1 公共方法

| 方法签名                                  | 官方作用                              | 典型工业场景                                                 |
| :---------------------------------------- | :------------------------------------ | :----------------------------------------------------------- |
| `public bool BeginEdit()`                 | 让当前聚焦的单元格进入编辑状态        | 新增行后自动聚焦第一个可编辑单元格，提升录入效率             |
| `public bool CommitEdit()`                | 提交当前单元格 / 行的编辑内容到数据源 | 保存按钮中统一调用，确保所有未提交的编辑全部生效；关闭窗口前必须调用，防止数据丢失 |
| `public void CancelEdit()`                | 取消当前编辑，恢复单元格原始值        | 取消按钮、撤销操作场景                                       |
| `public void ScrollIntoView(object item)` | 将指定数据行滚动到可视区域内          | 新增、搜索定位后自动滚动到目标行；虚拟化下同样有效           |
| `public void SelectAll() / UnselectAll()` | 全选 / 取消全选                       | 批量操作工具栏按钮，多选模式下有效                           |

### 4.2 受保护重写方法

`DataGrid` 重写了 `ItemsControl` 的容器生命周期方法，适配表格行容器体系，是虚拟化兼容的核心。

#### 1. `GetContainerForItemOverride`

csharp:

```c#
protected override DependencyObject GetContainerForItemOverride();
```

- 官方实现：返回 `new DataGridRow()`；
- 作用：将条目容器具体化为 `DataGridRow`，作为一行的 UI 载体。

#### 2. `IsItemItsOwnContainerOverride`

csharp:

```c#
protected override bool IsItemItsOwnContainerOverride(object item);
```

- 官方实现：判断 `item is DataGridRow`；
- 作用：支持直接添加 `DataGridRow` 元素作为行，无需额外包装。

#### 3. `PrepareContainerForItemOverride`

csharp:

```c#
protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
```

- 官方执行流程：
  1. 调用基类方法，完成数据上下文、选中状态、行样式的基础准备；
  2. 为该行生成所有列对应的 `DataGridCell` 单元格；
  3. 应用单元格样式、绑定数据、设置编辑状态；
  4. 触发 `LoadingRow` 事件。
- 虚拟化适配：容器复用时重新生成单元格并绑定新数据，保证滚动后内容正确。

#### 4. `ClearContainerForItemOverride`

csharp:

```c#
protected override void ClearContainerForItemOverride(DependencyObject element, object item);
```

- 官方执行流程：
  1. 清理该行所有单元格的绑定、样式、编辑状态；
  2. 触发 `UnloadingRow` 事件；
  3. 调用基类方法，行容器进入回收池等待复用。
- 关键意义：完整的清理逻辑是行虚拟化稳定运行的基础，避免状态残留与内存泄漏。

#### 5. `OnItemsChanged`

csharp:

```c#
protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e);
```

- 官方扩展逻辑：集合增删改时，同步维护选中状态、更新列布局、刷新虚拟化范围；
- 集合重置时强制重建可见行，保证数据与 UI 一致。

------

## 五、配套核心类型体系

`DataGrid` 不是单一控件，而是一套完整的表格体系，核心配套类型决定了其扩展能力。

### 5.1 列类型体系（`DataGridColumn` 抽象基类）

所有列均继承自 `DataGridColumn` 抽象类，官方提供 5 种标准列，覆盖绝大多数业务场景。

表格

| 列类型                    | 适用数据           | 核心能力                     | 工业典型场景                           |
| :------------------------ | :----------------- | :--------------------------- | :------------------------------------- |
| `DataGridTextColumn`      | 字符串、数值、日期 | 纯文本显示与文本框编辑       | 设备名称、编号、数值参数等绝大多数列   |
| `DataGridCheckBoxColumn`  | `bool` 布尔值      | 复选框显示与编辑             | 是否启用、是否合格、开关类参数         |
| `DataGridComboBoxColumn`  | 枚举 / 固定选项    | 下拉选择编辑                 | 设备类型、参数等级、班次等固定选项     |
| `DataGridHyperlinkColumn` | 超链接             | 链接跳转                     | 文档链接、详情页跳转                   |
| `DataGridTemplateColumn`  | 任意类型           | 完全自定义显示模板与编辑模板 | 状态指示灯、进度条、行内按钮、复杂布局 |

**列通用核心属性**：`Header`（列头）、`Binding`（数据绑定）、`IsReadOnly`（是否只读）、`Width`（列宽）、`CellTemplate` / `CellEditingTemplate`（模板列专属）。

### 5.2 行与单元格容器

表格

| 类型                   | 对应层级 | 核心作用                                                     |
| :--------------------- | :------- | :----------------------------------------------------------- |
| `DataGridRow`          | 行级     | 每条数据的 UI 容器，承载一行所有单元格，控制行级样式、选中状态、行详情 |
| `DataGridCell`         | 单元格级 | 每个字段的 UI 容器，控制单元格样式、编辑状态、内容呈现，是编辑生命周期的载体 |
| `DataGridColumnHeader` | 列头     | 列标题容器，处理点击排序、拖拽列宽、列重排交互               |
| `DataGridRowHeader`    | 行头     | 行标题容器，默认显示选中箭头，可自定义行号、状态标记         |

------

## 六、官方核心工作机制

### 6.1 列生成机制

- **自动生成模式**（`AutoGenerateColumns="True"`）：

  通过反射遍历数据源类型的公共属性，按类型自动匹配列（`bool`→ 复选框列，其他 → 文本列）；适合快速原型开发，顺序不可控、样式单一，生产环境不推荐。

- **手动定义模式**（`AutoGenerateColumns="False"`）：

  完全由开发者定义列的数量、顺序、类型、样式、格式；

  工业生产环境推荐

  ，稳定可控、符合业务规范。

### 6.2 单元格编辑完整生命周期

plaintext:

```tex
用户双击/按F2/点击单元格 → 触发 BeginningEdit 事件
        ↓ 可取消，e.Cancel=true 则终止
生成对应列的编辑控件 → 触发 PreparingCellForEdit
        ↓
用户修改内容
        ↓
失去焦点/按回车 → 触发 CellEditEnding 事件
        ↓ 可取消提交，e.Cancel=true 则保持编辑状态
验证通过 → 提交值到数据源 → 编辑结束
验证不通过 → 显示错误提示 → 保持编辑状态
```

### 6.3 UI 虚拟化原理

#### 行虚拟化

- 只生成可见区域的 `DataGridRow` 容器，滚出屏幕的行被回收复用；
- 选中状态、编辑状态绑定在数据层，不依赖 UI 容器，滚动时状态不丢失；
- 万级数据下，内存占用降低 90% 以上，滚动流畅度提升数倍。

#### 列虚拟化

- 列数很多时，只生成可见列的 `DataGridCell`，横向滚动时动态创建 / 销毁单元格；
- 列数少时关闭，避免横向滚动的渲染闪烁与性能开销。

### 6.4 内置排序机制

- 默认基于 `ICollectionView` 实现，点击列头切换升序 / 降序；
- 支持自定义排序逻辑，可扩展多列组合排序、服务端分页排序；
- 排序是数据层面的排序，不影响虚拟化机制。

------

## 七、本质定位与工业选型提示

### ListView+GridView vs DataGrid 核心对比

| 维度         | ListView + GridView                        | DataGrid                                 |
| :----------- | :----------------------------------------- | :--------------------------------------- |
| 核心定位     | 只读多列数据展示                           | 可编辑数据表格                           |
| 原生编辑     | 无，需自定义模板实现                       | 原生支持单元格 / 行编辑、验证            |
| 性能表现     | 更轻量，渲染开销更低                       | 功能全面，开销相对更高                   |
| 虚拟化       | 仅行虚拟化                                 | 行虚拟化 + 列虚拟化                      |
| 样式粒度     | 行级                                       | 行级 + 单元格级                          |
| **选型结论** | 设备台账、报警明细、生产记录查询等只读场景 | 配方编辑、参数配置、数据录入等可编辑场景 |

> 💡 工业场景原则：**能不用 DataGrid 就不用，只读优先 ListView**；`DataGrid` 功能强大但更重、坑点更多，仅在确实需要原生编辑时使用。

------

## 总结

`DataGrid` 是 WPF `ItemsControl` 体系的集大成者：它继承了完整的集合呈现与多选能力，在此之上构建了「行列二维表格 + 原生编辑生命周期 + 双级虚拟化」的完整体系，是原生控件中功能最强大、复杂度最高的列表类控件。理解其依赖属性、编辑事件、容器生命周期与虚拟化原理，是用好该控件、规避工业大数据场景下性能与状态问题的核心基础。