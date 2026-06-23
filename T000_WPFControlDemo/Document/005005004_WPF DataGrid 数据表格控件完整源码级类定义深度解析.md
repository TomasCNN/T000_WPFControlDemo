# 005005004_WPF `DataGrid` 数据表格控件完整源码级类定义深度解析

**源码：**

```c#
public class DataGrid : MultiSelector
{
    public static readonly DependencyProperty CanUserResizeColumnsProperty;
    public static readonly DependencyProperty CurrentItemProperty;
    public static readonly DependencyProperty CurrentColumnProperty;
    public static readonly DependencyProperty CurrentCellProperty;
    public static readonly DependencyProperty CanUserAddRowsProperty;
    public static readonly DependencyProperty CanUserDeleteRowsProperty;
    public static readonly DependencyProperty RowDetailsVisibilityModeProperty;
    public static readonly DependencyProperty AreRowDetailsFrozenProperty;
    public static readonly DependencyProperty RowDetailsTemplateProperty;
    public static readonly DependencyProperty RowDetailsTemplateSelectorProperty;
    public static readonly DependencyProperty CanUserResizeRowsProperty;
    public static readonly DependencyProperty NewItemMarginProperty;
    public static readonly DependencyProperty SelectionModeProperty;
    public static readonly DependencyProperty SelectionUnitProperty;
    public static readonly DependencyProperty CanUserSortColumnsProperty;
    public static readonly DependencyProperty AutoGenerateColumnsProperty;
    public static readonly DependencyProperty FrozenColumnCountProperty;
    public static readonly DependencyProperty NonFrozenColumnsViewportHorizontalOffsetProperty;
    public static readonly DependencyProperty EnableColumnVirtualizationProperty;
    public static readonly DependencyProperty CanUserReorderColumnsProperty;
    public static readonly DependencyProperty DragIndicatorStyleProperty;
    public static readonly DependencyProperty DropLocationIndicatorStyleProperty;
    public static readonly DependencyProperty ClipboardCopyModeProperty;
    public static readonly DependencyProperty CellsPanelHorizontalOffsetProperty;
    public static readonly DependencyProperty IsReadOnlyProperty;
    public static readonly RoutedCommand CancelEditCommand;
    public static readonly DependencyProperty EnableRowVirtualizationProperty;
    public static readonly RoutedCommand BeginEditCommand;
    public static readonly RoutedCommand CommitEditCommand;
    public static readonly DependencyProperty ColumnWidthProperty;
    public static readonly DependencyProperty MinColumnWidthProperty;
    public static readonly DependencyProperty MaxColumnWidthProperty;
    public static readonly DependencyProperty HorizontalGridLinesBrushProperty;
    public static readonly DependencyProperty VerticalGridLinesBrushProperty;
    public static readonly DependencyProperty RowStyleProperty;
    public static readonly DependencyProperty RowValidationErrorTemplateProperty;
    public static readonly DependencyProperty RowStyleSelectorProperty;
    public static readonly DependencyProperty RowBackgroundProperty;
    public static readonly DependencyProperty AlternatingRowBackgroundProperty;
    public static readonly DependencyProperty RowHeightProperty;
    public static readonly DependencyProperty GridLinesVisibilityProperty;
    public static readonly DependencyProperty RowHeaderWidthProperty;
    public static readonly DependencyProperty VerticalScrollBarVisibilityProperty;
    public static readonly DependencyProperty MinRowHeightProperty;
    public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty;
    public static readonly DependencyProperty RowHeaderTemplateProperty;
    public static readonly DependencyProperty RowHeaderStyleProperty;
    public static readonly DependencyProperty RowHeaderTemplateSelectorProperty;
    public static readonly DependencyProperty CellStyleProperty;
    public static readonly DependencyProperty HeadersVisibilityProperty;
    public static readonly DependencyProperty ColumnHeaderHeightProperty;
    public static readonly DependencyProperty RowHeaderActualWidthProperty;
    public static readonly DependencyProperty ColumnHeaderStyleProperty;
 
    public DataGrid();
 
    public static ComponentResourceKey FocusBorderBrushKey { get; }
    public static RoutedUICommand SelectAllCommand { get; }
    public static IValueConverter HeadersVisibilityConverter { get; }
    public static IValueConverter RowDetailsScrollingConverter { get; }
    public static RoutedUICommand DeleteCommand { get; }
    public DataTemplate RowHeaderTemplate { get; set; }
    public DataTemplateSelector RowHeaderTemplateSelector { get; set; }
    public ScrollBarVisibility VerticalScrollBarVisibility { get; set; }
    public ScrollBarVisibility HorizontalScrollBarVisibility { get; set; }
    public bool CanUserAddRows { get; set; }
    public object CurrentItem { get; set; }
    public DataGridColumn CurrentColumn { get; set; }
    public DataGridCellInfo CurrentCell { get; set; }
    public bool CanUserDeleteRows { get; set; }
    public Style RowHeaderStyle { get; set; }
    public DataGridRowDetailsVisibilityMode RowDetailsVisibilityMode { get; set; }
    public bool IsReadOnly { get; set; }
    public Style ColumnHeaderStyle { get; set; }
    public Style RowStyle { get; set; }
    public DataGridHeadersVisibility HeadersVisibility { get; set; }
    public bool AreRowDetailsFrozen { get; set; }
    public Brush AlternatingRowBackground { get; set; }
    public Brush RowBackground { get; set; }
    public StyleSelector RowStyleSelector { get; set; }
    public ObservableCollection<ValidationRule> RowValidationRules { get; }
    public ControlTemplate RowValidationErrorTemplate { get; set; }
    public Brush VerticalGridLinesBrush { get; set; }
    public Brush HorizontalGridLinesBrush { get; set; }
    public DataGridGridLinesVisibility GridLinesVisibility { get; set; }
    public double MaxColumnWidth { get; set; }
    public double MinColumnWidth { get; set; }
    public DataGridLength ColumnWidth { get; set; }
    public bool CanUserResizeColumns { get; set; }
    public ObservableCollection<DataGridColumn> Columns { get; }
    public double RowHeaderWidth { get; set; }
    public double RowHeaderActualWidth { get; }
    public double ColumnHeaderHeight { get; set; }
    public Style CellStyle { get; set; }
    public DataTemplate RowDetailsTemplate { get; set; }
    public double MinRowHeight { get; set; }
    public bool CanUserResizeRows { get; set; }
    public double RowHeight { get; set; }
    public DataTemplateSelector RowDetailsTemplateSelector { get; set; }
    public double CellsPanelHorizontalOffset { get; }
    public DataGridClipboardCopyMode ClipboardCopyMode { get; set; }
    public Style DropLocationIndicatorStyle { get; set; }
    public bool CanUserReorderColumns { get; set; }
    public bool EnableColumnVirtualization { get; set; }
    public bool EnableRowVirtualization { get; set; }
    public Style DragIndicatorStyle { get; set; }
    public double NonFrozenColumnsViewportHorizontalOffset { get; }
    public int FrozenColumnCount { get; set; }
    public bool AutoGenerateColumns { get; set; }
    public Thickness NewItemMargin { get; }
    public bool CanUserSortColumns { get; set; }
    public DataGridSelectionUnit SelectionUnit { get; set; }
    public DataGridSelectionMode SelectionMode { get; set; }
    public IList<DataGridCellInfo> SelectedCells { get; }
    protected internal override bool HandlesScrolling { get; }
 
    public event DataGridSortingEventHandler Sorting;
    public event EventHandler AutoGeneratedColumns;
    public event EventHandler<DataGridAutoGeneratingColumnEventArgs> AutoGeneratingColumn;
    public event EventHandler<DragDeltaEventArgs> ColumnHeaderDragDelta;
    public event EventHandler<DragStartedEventArgs> ColumnHeaderDragStarted;
    public event EventHandler<DragCompletedEventArgs> ColumnHeaderDragCompleted;
    public event SelectedCellsChangedEventHandler SelectedCellsChanged;
    public event EventHandler<DataGridColumnReorderingEventArgs> ColumnReordering;
    public event EventHandler<DataGridRowDetailsEventArgs> RowDetailsVisibilityChanged;
    public event EventHandler<DataGridRowEventArgs> UnloadingRow;
    public event EventHandler<DataGridRowDetailsEventArgs> LoadingRowDetails;
    public event InitializingNewItemEventHandler InitializingNewItem;
    public event EventHandler<DataGridPreparingCellForEditEventArgs> PreparingCellForEdit;
    public event EventHandler<DataGridBeginningEditEventArgs> BeginningEdit;
    public event EventHandler<EventArgs> CurrentCellChanged;
    public event EventHandler<DataGridCellEditEndingEventArgs> CellEditEnding;
    public event EventHandler<DataGridRowEditEndingEventArgs> RowEditEnding;
    public event EventHandler<DataGridRowEventArgs> LoadingRow;
    public event EventHandler<DataGridColumnEventArgs> ColumnDisplayIndexChanged;
    public event EventHandler<DataGridRowDetailsEventArgs> UnloadingRowDetails;
    public event EventHandler<AddingNewItemEventArgs> AddingNewItem;
    public event EventHandler<DataGridRowClipboardEventArgs> CopyingRowClipboardContent;
    public event EventHandler<DataGridColumnEventArgs> ColumnReordered;
 
    public static Collection<DataGridColumn> GenerateColumns(IItemProperties itemProperties);
    public bool BeginEdit();
    public bool BeginEdit(RoutedEventArgs editingEventArgs);
    public bool CancelEdit();
    public bool CancelEdit(DataGridEditingUnit editingUnit);
    public void ClearDetailsVisibilityForItem(object item);
    public DataGridColumn ColumnFromDisplayIndex(int displayIndex);
    public bool CommitEdit();
    public bool CommitEdit(DataGridEditingUnit editingUnit, bool exitEditingMode);
    public Visibility GetDetailsVisibilityForItem(object item);
    public override void OnApplyTemplate();
    public void ScrollIntoView(object item);
    public void ScrollIntoView(object item, DataGridColumn column);
    public void SelectAllCells();
    public void SetDetailsVisibilityForItem(object item, Visibility detailsVisibility);
    public void UnselectAllCells();
    protected override void ClearContainerForItemOverride(DependencyObject element, object item);
    protected override DependencyObject GetContainerForItemOverride();
    protected override bool IsItemItsOwnContainerOverride(object item);
    protected override Size MeasureOverride(Size availableSize);
    protected virtual void OnAddingNewItem(AddingNewItemEventArgs e);
    protected virtual void OnAutoGeneratedColumns(EventArgs e);
    protected virtual void OnAutoGeneratingColumn(DataGridAutoGeneratingColumnEventArgs e);
    protected virtual void OnBeginningEdit(DataGridBeginningEditEventArgs e);
    protected virtual void OnCanExecuteBeginEdit(CanExecuteRoutedEventArgs e);
    protected virtual void OnCanExecuteCancelEdit(CanExecuteRoutedEventArgs e);
    protected virtual void OnCanExecuteCommitEdit(CanExecuteRoutedEventArgs e);
    protected virtual void OnCanExecuteCopy(CanExecuteRoutedEventArgs args);
    protected virtual void OnCanExecuteDelete(CanExecuteRoutedEventArgs e);
    protected virtual void OnCellEditEnding(DataGridCellEditEndingEventArgs e);
    protected override void OnContextMenuOpening(ContextMenuEventArgs e);
    protected virtual void OnCopyingRowClipboardContent(DataGridRowClipboardEventArgs args);
    protected override AutomationPeer OnCreateAutomationPeer();
    protected virtual void OnCurrentCellChanged(EventArgs e);
    protected virtual void OnExecutedBeginEdit(ExecutedRoutedEventArgs e);
    protected virtual void OnExecutedCancelEdit(ExecutedRoutedEventArgs e);
    protected virtual void OnExecutedCommitEdit(ExecutedRoutedEventArgs e);
    protected virtual void OnExecutedCopy(ExecutedRoutedEventArgs args);
    protected virtual void OnExecutedDelete(ExecutedRoutedEventArgs e);
    protected virtual void OnInitializingNewItem(InitializingNewItemEventArgs e);
    protected override void OnIsMouseCapturedChanged(DependencyPropertyChangedEventArgs e);
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e);
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue);
    protected override void OnKeyDown(KeyEventArgs e);
    protected virtual void OnLoadingRow(DataGridRowEventArgs e);
    protected virtual void OnLoadingRowDetails(DataGridRowDetailsEventArgs e);
    protected override void OnMouseMove(MouseEventArgs e);
    protected virtual void OnRowEditEnding(DataGridRowEditEndingEventArgs e);
    protected virtual void OnSelectedCellsChanged(SelectedCellsChangedEventArgs e);
    protected override void OnSelectionChanged(SelectionChangedEventArgs e);
    protected virtual void OnSorting(DataGridSortingEventArgs eventArgs);
    protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate);
    protected override void OnTextInput(TextCompositionEventArgs e);
    protected virtual void OnUnloadingRow(DataGridRowEventArgs e);
    protected virtual void OnUnloadingRowDetails(DataGridRowDetailsEventArgs e);
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
    protected internal virtual void OnColumnDisplayIndexChanged(DataGridColumnEventArgs e);
    protected internal virtual void OnColumnHeaderDragCompleted(DragCompletedEventArgs e);
    protected internal virtual void OnColumnHeaderDragDelta(DragDeltaEventArgs e);
    protected internal virtual void OnColumnHeaderDragStarted(DragStartedEventArgs e);
    protected internal virtual void OnColumnReordered(DataGridColumnEventArgs e);
    protected internal virtual void OnColumnReordering(DataGridColumnReorderingEventArgs e);
    protected internal virtual void OnPreparingCellForEdit(DataGridPreparingCellForEditEventArgs e);
    protected internal virtual void OnRowDetailsVisibilityChanged(DataGridRowDetailsEventArgs e);
 
}
```

你提供的是 WPF `DataGrid` 最完整的原生源码级类定义。它直接继承自 `MultiSelector` 多选基类，是 WPF 原生控件体系中功能最完整、复杂度最高的**二维可编辑数据表格控件**。

它在完整继承 `ItemsControl` 集合呈现、`Selector` 单选管理、`MultiSelector` 批量多选优化全部能力的基础上，新增了**单元格级编辑体系、列类型系统、冻结列、行详情、行列双虚拟化、路由命令驱动、剪贴板支持、列拖拽重排**等完整表格能力，是工业软件中配方编辑、参数配置、可编辑生产台账的标准控件。

本文按「属性 - 静态成员 - 事件 - 方法 - 机制」逐层拆解，完整还原其设计全貌与工业场景价值。

------

## 一、类定义总览与核心元数据

### 1.1 完整继承链

plaintext:

```tex
Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement
    → Control → ItemsControl → Selector → MultiSelector → DataGrid
```

- **类类型**：具体公共类，可直接实例化使用
- **命名空间**：`System.Windows.Controls`
- **程序集**：`PresentationFramework.dll`
- **核心设计**：以「行 + 列」二维模型为基础，路由命令驱动编辑生命周期，支持行列双级虚拟化，提供从展示、编辑、验证到排序的完整表格能力。

### 1.2 核心能力总览

| 能力层级 | 继承自基类       | DataGrid 独有扩展                                   |
| :------- | :--------------- | :-------------------------------------------------- |
| 集合呈现 | ItemsControl     | 二维行列布局、列头 / 行头体系                       |
| 选择能力 | Selector（单选） | 单元格级选择、整行 / 单元格切换选择单位             |
| 多选优化 | MultiSelector    | 单元格批量选择、批量更新挂起通知                    |
| 编辑能力 | 无原生编辑       | 完整单元格 / 行编辑生命周期、数据验证、路由命令驱动 |
| 虚拟化   | 行虚拟化         | 行虚拟化 + 列虚拟化双级优化                         |
| 交互能力 | 基础键盘鼠标     | 列宽拖拽、列重排、冻结列、剪贴板复制                |
| 扩展能力 | 容器样式         | 行详情、行验证、多列类型体系                        |

------

## 二、静态依赖属性全量分类解析

DataGrid 拥有数十个依赖属性，按职责可分为 8 大类，全部支持数据绑定、样式、动画。

### 2.1 编辑控制与当前单元格体系（核心）

这是 DataGrid 区别于所有列表控件的核心：引入了「当前活动单元格」概念，类似 Excel 的活动单元格，是编辑功能的基石。

| 属性                  | 类型               | 默认值  | 官方作用                             | 工业场景说明                                                 |
| :-------------------- | :----------------- | :------ | :----------------------------------- | :----------------------------------------------------------- |
| `IsReadOnly`          | `bool`             | `false` | 全局是否禁止所有单元格编辑           | 纯展示表格设为 `true`；可编辑表格通过单列 `IsReadOnly` 锁定关键字段 |
| `AutoGenerateColumns` | `bool`             | `true`  | 是否根据数据源属性反射自动生成列     | 生产环境必须设为 `false`，手动定义列更稳定可控               |
| `CanUserAddRows`      | `bool`             | `true`  | 是否显示底部空白新增行               | 工业场景大多关闭，通过业务按钮新增并校验数据                 |
| `CanUserDeleteRows`   | `bool`             | `true`  | 是否允许 Delete 键直接删除行         | 关键数据必须关闭，删除走业务按钮 + 二次确认                  |
| `CurrentItem`         | `object`           | `null`  | 当前活动行对应的数据项               | 编辑生命周期的行级锚点                                       |
| `CurrentColumn`       | `DataGridColumn`   | `null`  | 当前活动列                           | 编辑生命周期的列级锚点                                       |
| `CurrentCell`         | `DataGridCellInfo` | 空      | 当前活动单元格信息（结构体，值类型） | 编辑操作的核心定位，区分「选中」与「正在编辑」               |

> 🔑 关键概念：**选中 ≠ 当前单元格**。选中是高亮状态，当前单元格是键盘输入、编辑操作的目标；多选模式下可选中多个单元格，但当前单元格始终只有一个。

### 2.2 选择交互体系

在 `MultiSelector` 行级多选基础上，扩展了单元格级选择能力。

| 属性            | 类型                      | 默认值     | 官方作用                                               | 说明                                     |
| :-------------- | :------------------------ | :--------- | :----------------------------------------------------- | :--------------------------------------- |
| `SelectionMode` | `DataGridSelectionMode`   | `Extended` | 选择模式：Single 单选 / Extended 扩展多选              | 继承并强化 MultiSelector 的多选能力      |
| `SelectionUnit` | `DataGridSelectionUnit`   | `FullRow`  | 选择单位：FullRow 整行 / Cell 单元格 / CellOrRowHeader | 工业默认整行选择；需要单元格级操作时切换 |
| `SelectedCells` | `IList<DataGridCellInfo>` | 空集合     | 所有选中的单元格集合                                   | 单元格选择模式下的批量操作入口           |

### 2.3 列交互与布局体系

控制列的行为、布局与拖拽交互，对应工业表格的列操作需求。

| 属性                                | 类型                                   | 默认值         | 官方作用                                 | 工业最佳实践                                               |
| :---------------------------------- | :------------------------------------- | :------------- | :--------------------------------------- | :--------------------------------------------------------- |
| `Columns`                           | `ObservableCollection<DataGridColumn>` | 空集合         | 所有列定义的集合，表格核心配置           | 手动定义列的唯一入口，支持 5 种标准列类型                  |
| `CanUserReorderColumns`             | `bool`                                 | `true`         | 是否允许拖动列头调整列顺序               | 关键业务表格设为 `false`，固定列序防止误操作               |
| `CanUserResizeColumns`              | `bool`                                 | `true`         | 是否允许拖动调整列宽                     | 一般开启，方便查看长内容                                   |
| `CanUserResizeRows`                 | `bool`                                 | `false`        | 是否允许拖动调整行高                     | 工业表格固定行高，保持默认关闭                             |
| `CanUserSortColumns`                | `bool`                                 | `true`         | 是否允许点击列头排序                     | 查询类表格开启，提升数据查看效率                           |
| `ColumnWidth`                       | `DataGridLength`                       | `SizeToHeader` | 全局默认列宽，支持固定值、Auto、星号比例 | 大数据量避免大量 Auto 列，会逐行计算宽度降低性能           |
| `MinColumnWidth` / `MaxColumnWidth` | `double`                               | 系统默认       | 列宽的最小 / 最大值限制                  | 防止列被拖得过窄或过宽                                     |
| `FrozenColumnCount`                 | `int`                                  | `0`            | 左侧冻结的列数，横向滚动时固定不动       | 工业宽表格必备，冻结设备编号、名称等关键列，滚动时始终可见 |
| `DragIndicatorStyle`                | `Style`                                | `null`         | 列拖拽时的指示图标样式                   | 自定义列重排的视觉效果                                     |
| `DropLocationIndicatorStyle`        | `Style`                                | `null`         | 列放置位置的指示线样式                   | 自定义拖拽落点提示                                         |

### 2.4 行详情体系

DataGrid 特色功能，支持每行展开显示二级详情面板。

| 属性                         | 类型                               | 官方作用                                     |
| :--------------------------- | :--------------------------------- | :------------------------------------------- |
| `RowDetailsTemplate`         | `DataTemplate`                     | 行展开后的详情内容模板                       |
| `RowDetailsTemplateSelector` | `DataTemplateSelector`             | 根据数据动态选择详情模板                     |
| `RowDetailsVisibilityMode`   | `DataGridRowDetailsVisibilityMode` | 详情显示模式：选中展开 / 全部展开 / 全部折叠 |
| `AreRowDetailsFrozen`        | `bool`                             | 详情面板是否随横向滚动固定                   |

### 2.5 性能与虚拟化体系

工业大数据量表格的核心优化项。

| 属性                                       | 类型     | 默认值         | 官方作用                             | 说明                                   |
| :----------------------------------------- | :------- | :------------- | :----------------------------------- | :------------------------------------- |
| `EnableRowVirtualization`                  | `bool`   | `true`         | 是否开启行虚拟化，仅生成可见行       | WPF 4.0+ 默认开启，万级数据性能保障    |
| `EnableColumnVirtualization`               | `bool`   | `false`        | 是否开启列虚拟化，仅生成可见列       | 列数 > 20 时开启，列数少时关闭避免闪烁 |
| `CellsPanelHorizontalOffset`               | `double` | 只读           | 单元格面板的水平偏移量（内部计算用） | 配合列虚拟化与冻结列使用               |
| `NonFrozenColumnsViewportHorizontalOffset` | `double` | 只读           | 非冻结列视口的水平偏移               | 冻结列滚动计算的内部属性               |
| `HandlesScrolling`                         | `bool`   | `true`（重写） | 声明控件自管滚动                     | 内置 ScrollViewer，不需要外层包裹      |

### 2.6 视觉与样式体系

粒度细化到行、单元格、列头、行头、网格线五个层级。

| 分类       | 属性                                                         | 作用                                   |
| :--------- | :----------------------------------------------------------- | :------------------------------------- |
| 行样式     | `RowStyle` / `RowStyleSelector`                              | 行容器统一样式 / 动态选择行样式        |
| 单元格样式 | `CellStyle`                                                  | 单元格统一样式，控制内边距、对齐、字体 |
| 表头样式   | `ColumnHeaderStyle` / `RowHeaderStyle`                       | 列头 / 行头的统一样式                  |
| 行背景     | `RowBackground` / `AlternatingRowBackground`                 | 行默认背景 / 交替行背景                |
| 网格线     | `GridLinesVisibility` / `HorizontalGridLinesBrush` / `VerticalGridLinesBrush` | 网格线显示方式与颜色                   |
| 尺寸       | `RowHeight` / `MinRowHeight` / `ColumnHeaderHeight` / `RowHeaderWidth` / `RowHeaderActualWidth` | 各行各部分尺寸控制                     |
| 可见性     | `HeadersVisibility`                                          | 表头显示方式：列头 / 行头 / 全部 / 无  |
| 滚动条     | `HorizontalScrollBarVisibility` / `VerticalScrollBarVisibility` | 滚动条显示策略                         |
| 行验证     | `RowValidationErrorTemplate` / `RowValidationRules`          | 行级错误提示模板 / 验证规则集合        |

### 2.7 剪贴板与其他

| 属性                                              | 类型                        | 官方作用                                 |
| :------------------------------------------------ | :-------------------------- | :--------------------------------------- |
| `ClipboardCopyMode`                               | `DataGridClipboardCopyMode` | 剪贴板复制模式：是否包含列头、是否仅文本 |
| `NewItemMargin`                                   | `Thickness`                 | 新增行的外边距（只读）                   |
| `RowHeaderTemplate` / `RowHeaderTemplateSelector` | 行头内容模板 / 动态选择器   |                                          |

### 2.8 内置路由命令

DataGrid 采用**路由命令驱动编辑**的设计，所有编辑操作都通过标准路由命令触发，天然支持键盘快捷键、菜单、按钮统一调用。

| 命令字段            | 对应操作       | 默认快捷键 |
| :------------------ | :------------- | :--------- |
| `BeginEditCommand`  | 进入单元格编辑 | F2         |
| `CommitEditCommand` | 提交编辑内容   | Enter      |
| `CancelEditCommand` | 取消编辑       | Esc        |
| `DeleteCommand`     | 删除选中行     | Delete     |
| `SelectAllCommand`  | 全选           | Ctrl+A     |

> 设计优势：命令与 UI 解耦，无论通过键盘、按钮、右键菜单触发，都走同一套编辑逻辑，行为一致、易于扩展。

------

## 三、静态成员解析

### 3.1 静态资源与工具

| 成员                           | 类型                   | 作用                             |
| :----------------------------- | :--------------------- | :------------------------------- |
| `FocusBorderBrushKey`          | `ComponentResourceKey` | 焦点边框的资源键，用于自定义主题 |
| `HeadersVisibilityConverter`   | `IValueConverter`      | 内置表头可见性转换器             |
| `RowDetailsScrollingConverter` | `IValueConverter`      | 行详情滚动计算转换器             |

### 3.2 静态核心方法

csharp:

```c#
public static Collection<DataGridColumn> GenerateColumns(IItemProperties itemProperties);
```

- **官方作用**：自动生成列的核心实现，根据数据类型的属性集合生成对应的列对象；
- **调用时机**：`AutoGenerateColumns="True"` 时，内部自动调用；
- **生成规则**：`bool` 类型生成 `DataGridCheckBoxColumn`，其余类型生成 `DataGridTextColumn`；
- **工业场景**：一般不直接调用，自动列模式下配合 `AutoGeneratingColumn` 事件定制列。

------

## 四、事件体系全解析

按生命周期分为 7 大类，是业务逻辑接入与表格定制的核心入口。

### 4.1 编辑生命周期事件

| 事件                   | 触发时机                   | 典型工业用法                           |
| :--------------------- | :------------------------- | :------------------------------------- |
| `BeginningEdit`        | 进入编辑前，可取消         | 权限校验，关键参数禁止普通操作员修改   |
| `PreparingCellForEdit` | 编辑控件已生成，进入编辑时 | 初始化下拉选项、输入范围、默认值       |
| `CellEditEnding`       | 单元格编辑提交前，可取消   | 单元格级数据校验，超量程、非法格式拦截 |
| `RowEditEnding`        | 整行编辑提交前             | 行级完整性校验、关联字段逻辑校验       |
| `CurrentCellChanged`   | 当前活动单元格切换时       | 联动提示、实时校验                     |

### 4.2 列操作事件

| 事件                                                         | 触发时机                     | 典型用法                           |
| :----------------------------------------------------------- | :--------------------------- | :--------------------------------- |
| `AutoGeneratingColumn`                                       | 自动生成列时逐列触发         | 修改列名、设置格式、隐藏不需要的列 |
| `AutoGeneratedColumns`                                       | 所有列自动生成完成后         | 生成后的整体调整                   |
| `ColumnReordering`                                           | 列重排开始前，可取消         | 禁止特定列被拖动                   |
| `ColumnReordered`                                            | 列重排完成后                 | 保存用户自定义列序                 |
| `ColumnDisplayIndexChanged`                                  | 列显示顺序变化后             | 列序变更后的联动逻辑               |
| `ColumnHeaderDragStarted` / `ColumnHeaderDragDelta` / `ColumnHeaderDragCompleted` | 列宽拖拽的开始 / 过程 / 结束 | 自定义列宽调整逻辑                 |

### 4.3 行生命周期事件（虚拟化关键）

| 事件                          | 触发时机            | 工业场景用法                                                 |
| :---------------------------- | :------------------ | :----------------------------------------------------------- |
| `LoadingRow`                  | 行容器生成 / 复用时 | 自定义行号、特殊行标色、行级状态初始化                       |
| `UnloadingRow`                | 行容器回收时        | **必须与 LoadingRow 成对清理**，防止虚拟化下状态错乱、内存泄漏 |
| `LoadingRowDetails`           | 行详情展开时        | 异步加载详情数据，避免一次性加载全部详情                     |
| `UnloadingRowDetails`         | 行详情折叠时        | 清理详情资源、解绑事件                                       |
| `RowDetailsVisibilityChanged` | 行详情可见性变化时  | 详情展开 / 折叠的联动逻辑                                    |

### 4.4 新增行事件

| 事件                  | 触发时机         | 典型用法                     |
| :-------------------- | :--------------- | :--------------------------- |
| `AddingNewItem`       | 新增行创建前     | 注入默认值、初始化业务对象   |
| `InitializingNewItem` | 新增行对象创建后 | 给新行赋初始值、填充默认参数 |

### 4.5 选择与排序事件

| 事件                   | 触发时机               | 说明                              |
| :--------------------- | :--------------------- | :-------------------------------- |
| `SelectionChanged`     | 选中行变化时           | 继承自 Selector，行选择模式下使用 |
| `SelectedCellsChanged` | 选中单元格变化时       | 单元格选择模式下使用              |
| `Sorting`              | 点击列头排序前，可取消 | 自定义排序、服务端排序、多列排序  |

### 4.6 剪贴板事件

| 事件                         | 触发时机                 | 典型用法                     |
| :--------------------------- | :----------------------- | :--------------------------- |
| `CopyingRowClipboardContent` | 复制行到剪贴板时逐行触发 | 自定义复制格式、添加额外字段 |

------

## 五、公共方法全解析

### 5.1 编辑控制

| 方法                                                     | 作用                          | 典型场景                                           |
| :------------------------------------------------------- | :---------------------------- | :------------------------------------------------- |
| `BeginEdit()` / `BeginEdit(RoutedEventArgs)`             | 让当前单元格进入编辑状态      | 新增行后自动聚焦第一个可编辑单元格                 |
| `CommitEdit()` / `CommitEdit(DataGridEditingUnit, bool)` | 提交编辑，支持单元格 / 行两级 | 保存按钮中统一提交，关闭窗口前强制提交防止数据丢失 |
| `CancelEdit()` / `CancelEdit(DataGridEditingUnit)`       | 取消编辑，恢复原始值          | 取消按钮、撤销操作                                 |

### 5.2 滚动定位

| 方法                                                 | 作用             | 说明                               |
| :--------------------------------------------------- | :--------------- | :--------------------------------- |
| `ScrollIntoView(object item)`                        | 滚动到指定数据行 | 继承并强化，虚拟化下同样有效       |
| `ScrollIntoView(object item, DataGridColumn column)` | 滚动到指定单元格 | 支持横向滚动到指定列，搜索定位常用 |

### 5.3 选择操作

| 方法                                      | 作用                  | 说明                                             |
| :---------------------------------------- | :-------------------- | :----------------------------------------------- |
| `SelectAll()` / `UnselectAll()`           | 全选 / 取消全选行     | 继承自 MultiSelector，使用批量更新机制，性能优异 |
| `SelectAllCells()` / `UnselectAllCells()` | 全选 / 取消全选单元格 | 单元格选择模式下的批量操作                       |

### 5.4 行详情控制

| 方法                                                   | 作用                   |
| :----------------------------------------------------- | :--------------------- |
| `GetDetailsVisibilityForItem(object item)`             | 获取指定行的详情可见性 |
| `SetDetailsVisibilityForItem(object item, Visibility)` | 设置指定行的详情可见性 |
| `ClearDetailsVisibilityForItem(object item)`           | 清除详情可见性设置     |

### 5.5 列查询

| 方法                                       | 作用                   |
| :----------------------------------------- | :--------------------- |
| `ColumnFromDisplayIndex(int displayIndex)` | 根据显示序号获取列对象 |

------

## 六、受保护扩展方法全解析

这是自定义 DataGrid 子类的全部扩展点，覆盖生命周期、命令、事件、输入四大维度。

### 6.1 容器生命周期（继承自 ItemsControl）

| 方法                              | 核心职责                                                     |
| :-------------------------------- | :----------------------------------------------------------- |
| `GetContainerForItemOverride`     | 返回 `new DataGridRow()` 作为行容器                          |
| `IsItemItsOwnContainerOverride`   | 判断是否为 DataGridRow 类型                                  |
| `PrepareContainerForItemOverride` | 行生成 / 复用时，绑定数据、创建单元格、应用样式、触发 LoadingRow |
| `ClearContainerForItemOverride`   | 行回收时，清理单元格、解绑事件、触发 UnloadingRow，适配虚拟化 |

### 6.2 路由命令处理

每个内置路由命令都对应一对 `OnCanExecuteXXX` / `OnExecutedXXX` 虚方法，子类可重写扩展：

- `OnCanExecuteBeginEdit` / `OnExecutedBeginEdit`
- `OnCanExecuteCommitEdit` / `OnExecutedCommitEdit`
- `OnCanExecuteCancelEdit` / `OnExecutedCancelEdit`
- `OnCanExecuteDelete` / `OnExecutedDelete`
- `OnCanExecuteCopy` / `OnExecutedCopy`

> 设计价值：编辑逻辑完全基于路由命令，子类不需要重新绑定键盘事件，只需重写命令处理方法即可扩展行为。

### 6.3 事件触发入口

每个公共事件都对应一个受保护的 `OnXXX` 虚方法，负责触发事件，子类可重写注入自定义逻辑：

- 编辑类：`OnBeginningEdit`、`OnPreparingCellForEdit`、`OnCellEditEnding`、`OnRowEditEnding`
- 行生命周期：`OnLoadingRow`、`OnUnloadingRow`、`OnLoadingRowDetails`、`OnUnloadingRowDetails`
- 列操作：`OnAutoGeneratingColumn`、`OnAutoGeneratedColumns`、`OnColumnReordering`、`OnColumnReordered`
- 选择排序：`OnSelectionChanged`、`OnSelectedCellsChanged`、`OnSorting`
- 新增行：`OnAddingNewItem`、`OnInitializingNewItem`

### 6.4 输入与交互重写

| 方法                       | 核心作用                                                     |
| :------------------------- | :----------------------------------------------------------- |
| `OnKeyDown`                | 处理方向键导航、回车提交、F2 编辑、Delete 删除等所有键盘逻辑 |
| `OnMouseMove`              | 处理列宽拖拽、单元格拖拽选择等鼠标交互                       |
| `OnTextInput`              | 处理输入字符直接进入编辑模式                                 |
| `OnIsMouseCapturedChanged` | 处理拖拽开始与结束的鼠标捕获状态                             |
| `OnContextMenuOpening`     | 右键菜单打开前的处理                                         |

### 6.5 生命周期重写

| 方法                   | 作用                                            |
| :--------------------- | :---------------------------------------------- |
| `OnApplyTemplate`      | 模板应用完成，获取内部 ScrollViewer、列头等部件 |
| `OnTemplateChanged`    | 模板切换时清理旧部件、初始化新部件              |
| `OnItemsChanged`       | 数据源集合变更时同步选中状态、更新虚拟化        |
| `OnItemsSourceChanged` | 数据源更换时重置列、清空选中、重新生成自动列    |
| `MeasureOverride`      | 自定义测量逻辑，计算行列布局                    |

------

## 七、核心底层工作机制

### 7.1 编辑状态机

DataGrid 的编辑是完整的状态流转：

plaintext:

```tex
正常浏览状态
    ↓ 双击/F2/输入字符
进入编辑模式 → 触发 BeginningEdit（可取消）
    ↓
生成编辑控件 → 触发 PreparingCellForEdit
    ↓ 用户修改
失去焦点/回车 → 触发 CellEditEnding（可取消）
    ↓ 校验通过
提交值到数据源 → 退出编辑模式
    ↓ 校验不通过
显示错误标记 → 保持编辑状态
```

### 7.2 行列双级虚拟化

1. **行虚拟化**：只生成可见区域的 `DataGridRow`，滚出屏幕回收复用，万级数据内存降低 90%+；
2. **列虚拟化**：列数很多时，只生成可见列的 `DataGridCell`，横向滚动时动态创建 / 销毁；
3. **状态持久化**：选中、编辑、展开状态全部绑定在数据层，不依赖 UI 容器，滚动时状态不丢失。

### 7.3 冻结列实现原理

- 前 `FrozenColumnCount` 列属于冻结区域，放置在独立的视觉容器中；
- 横向滚动时，冻结区域不偏移，只有非冻结区域滚动；
- 通过 `NonFrozenColumnsViewportHorizontalOffset` 维护非冻结区的偏移量，保证行对齐。
- **工业价值**：宽表格中锁定编号、名称等关键字段，滚动时始终可见，是工业数据表格的标配功能。

### 7.4 批量选择性能优化

完全继承 `MultiSelector` 的批量更新机制：

- `SelectAll`、单元格全选等批量操作，内部自动使用 `BeginUpdateSelectedItems` / `EndUpdateSelectedItems` 包裹；
- 上千条数据批量选中，只触发一次 `SelectionChanged` 事件，性能比逐条设置高一个数量级。

------

## 八、工业场景最佳实践

1. **生产环境标准配置**
   - `AutoGenerateColumns="False"` 手动定义列；
   - `CanUserAddRows="False"`、`CanUserDeleteRows="False"` 走业务按钮操作；
   - 关键列设置 `IsReadOnly="True"`，仅允许修改指定字段；
   - 宽表格设置 `FrozenColumnCount` 冻结关键列。
2. **性能优化要点**
   - 500 行以上默认开启行虚拟化，20 列以上开启列虚拟化；
   - 避免大量 `Width="Auto"` 列，优先固定或比例宽度；
   - 模板列减少视觉树嵌套，避免滚动卡顿；
   - 批量操作使用内置 `SelectAll` 等方法，不要循环设置选中。
3. **虚拟化避坑**
   - 不要直接修改 `DataGridRow`/`DataGridCell` 的属性，用数据绑定 + 样式触发器；
   - `LoadingRow` 中设置的自定义状态，必须在 `UnloadingRow` 中对应清理；
   - 永远操作数据源，不要遍历行容器获取数据。

------

## 总结

`DataGrid` 是 WPF `ItemsControl` 体系的集大成者：它站在 `Selector` + `MultiSelector` 的肩膀上，将一维列表扩展为二维可编辑表格，通过路由命令、编辑生命周期、双级虚拟化、冻结列等设计，构建了一套完整的工业级表格解决方案。理解它的属性分类、事件生命周期与虚拟化机制，是用好该控件、规避大数据场景下性能与状态问题的核心基础。