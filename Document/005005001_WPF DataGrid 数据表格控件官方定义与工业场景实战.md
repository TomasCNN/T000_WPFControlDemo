# 005005001_WPF `DataGrid` 数据表格控件官方定义与工业场景实战

`DataGrid` 是 WPF 原生功能最完整的**可编辑数据表格控件**，直接继承自 `MultiSelector` 多选基类，在集合呈现、多选能力的基础上，提供了原生单元格编辑、列定义体系、内置排序、行详情、数据验证、UI 虚拟化等完整表格能力。

它是工业软件中**配方编辑、参数配置、可编辑数据台账**的首选控件。对比 `ListView+GridView`，`DataGrid` 核心优势在于原生编辑能力，适合需要修改数据的场景；而只读展示场景优先用更轻量的 `ListView+GridView`。

------

## 一、官方类定义总览与核心元数据

### 1.1 完整类签名（官方原生定义）

csharp:

```c#
namespace System.Windows.Controls
{
    [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
    [StyleTypedProperty(Property = "RowStyle", StyleTargetType = typeof(DataGridRow))]
    [StyleTypedProperty(Property = "CellStyle", StyleTargetType = typeof(DataGridCell))]
    public class DataGrid : System.Windows.Controls.Primitives.MultiSelector
    {
        // 核心依赖属性字段
        public static readonly DependencyProperty AutoGenerateColumnsProperty;
        public static readonly DependencyProperty IsReadOnlyProperty;
        public static readonly DependencyProperty CanUserAddRowsProperty;
        public static readonly DependencyProperty CanUserDeleteRowsProperty;
        public static readonly DependencyProperty SelectionModeProperty;
        public static readonly DependencyProperty SelectionUnitProperty;
        public static readonly DependencyProperty ColumnsProperty;
        public static readonly DependencyProperty RowStyleProperty;
        public static readonly DependencyProperty CellStyleProperty;
        public static readonly DependencyProperty AlternatingRowBackgroundProperty;
        public static readonly DependencyProperty EnableRowVirtualizationProperty;
        public static readonly DependencyProperty GridLinesVisibilityProperty;
        public static readonly DependencyProperty HeadersVisibilityProperty;

        // 构造函数
        public DataGrid();

        // 核心公共属性
        public bool AutoGenerateColumns { get; set; }
        public bool IsReadOnly { get; set; }
        public bool CanUserAddRows { get; set; }
        public bool CanUserDeleteRows { get; set; }
        public DataGridSelectionMode SelectionMode { get; set; }
        public DataGridSelectionUnit SelectionUnit { get; set; }
        public ObservableCollection<DataGridColumn> Columns { get; }
        public Style RowStyle { get; set; }
        public Style CellStyle { get; set; }
        public Brush AlternatingRowBackground { get; set; }
        public bool EnableRowVirtualization { get; set; }
        public DataGridGridLinesVisibility GridLinesVisibility { get; set; }
        public DataGridHeadersVisibility HeadersVisibility { get; set; }

        // 核心事件
        public event EventHandler<DataGridBeginningEditEventArgs> BeginningEdit;
        public event EventHandler<DataGridCellEditEndingEventArgs> CellEditEnding;
        public event EventHandler<DataGridRowEventArgs> LoadingRow;
        public event EventHandler<DataGridRowEventArgs> UnloadingRow;
        public event DataGridSortingEventHandler Sorting;
        public event EventHandler<DataGridAutoGeneratingColumnEventArgs> AutoGeneratingColumn;

        // 公共方法
        public bool BeginEdit();
        public bool CommitEdit();
        public void CancelEdit();
        public void ScrollIntoView(object item);
        public void SelectAll();
        public void UnselectAll();

        // 受保护重写方法
        protected override DependencyObject GetContainerForItemOverride();
        protected override bool IsItemItsOwnContainerOverride(object item);
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
        protected override void ClearContainerForItemOverride(DependencyObject element, object item);
    }
}
```

### 1.2 核心元数据（官方精确值）

| 项               | 官方精确值                                                   | 工业场景关键说明                         |
| :--------------- | :----------------------------------------------------------- | :--------------------------------------- |
| **命名空间**     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                     |
| **程序集**       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                       |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → Selector → MultiSelector → DataGrid` | 完整继承集合呈现、多选、选择管理全部能力 |
| **默认行容器**   | `DataGridRow`                                                | 每行对应一条业务数据                     |
| **单元格容器**   | `DataGridCell`                                               | 每个单元格对应一个数据字段               |
| **核心内置功能** | 单元格编辑、列定义、排序、行详情、数据验证、行 / 列虚拟化    | 原生支持表格全生命周期                   |
| **工业核心场景** | 配方参数编辑、设备配置表、可编辑生产台账、数据录入界面       | 所有需要表格编辑的业务场景               |

### 1.3 类级特性说明

1. **`[StyleTypedProperty]` 双声明**
   - 分别指定 `RowStyle` 目标类型为 `DataGridRow`、`CellStyle` 目标类型为 `DataGridCell`，设计器可提供准确的样式智能提示；
   - 相比 `ListView` 只有行容器样式，`DataGrid` 细化到单元格级别的样式控制。
2. **`[Localizability]`**：控件本身无固定文本，内容由业务数据与列定义决定。

------

## 二、核心依赖属性全量解析

按功能分为六大类，全部为依赖属性，支持数据绑定、样式、动画。

### 2.1 数据与编辑控制类

控制表格的编辑权限、数据生成规则，是工业场景最常配置的属性。

| 属性                  | 类型   | 默认值  | 官方作用                                   | 工业最佳实践                                                 |
| :-------------------- | :----- | :------ | :----------------------------------------- | :----------------------------------------------------------- |
| `AutoGenerateColumns` | `bool` | `true`  | 是否根据数据源类型自动生成列               | **建议设为 `false`**，手动定义列顺序、格式更可控，避免自动生成的列不符合业务需求 |
| `IsReadOnly`          | `bool` | `false` | 全局是否只读，禁止所有单元格编辑           | 纯展示表格设为 `true`；可编辑表格通过单列 `IsReadOnly` 控制部分列只读 |
| `CanUserAddRows`      | `bool` | `true`  | 是否在表格底部显示空白行，支持用户新增数据 | 工业场景大多设为 `false`，通过业务按钮控制新增，避免误操作产生脏数据 |
| `CanUserDeleteRows`   | `bool` | `true`  | 是否允许用户通过 Delete 键删除行           | 关键数据表格建议关闭，通过业务按钮执行删除并做二次确认       |
| `CanUserResizeRows`   | `bool` | `false` | 是否允许用户拖动调整行高                   | 工业表格一般固定行高，保持界面整齐                           |

> 🔑 优先级规则：**单列的 `IsReadOnly` > 表格全局 `IsReadOnly`**，即全局可编辑时，可单独设置某列只读；全局只读时，单列设置可编辑无效。

### 2.2 列交互控制类

控制列的行为与布局，对应 `GridView` 的列能力，但更丰富。

| 属性                    | 类型                                   | 默认值         | 官方作用                               | 工业最佳实践                                                 |
| :---------------------- | :------------------------------------- | :------------- | :------------------------------------- | :----------------------------------------------------------- |
| `Columns`               | `ObservableCollection<DataGridColumn>` | 空集合         | 所有列定义的集合，表格的核心配置       | 手动定义列的唯一入口，支持文本、复选框、下拉、模板等多种列类型 |
| `CanUserReorderColumns` | `bool`                                 | `true`         | 是否允许拖动列头调整列顺序             | 关键业务表格设为 `false`，固定列序防止误操作                 |
| `CanUserResizeColumns`  | `bool`                                 | `true`         | 是否允许拖动调整列宽                   | 一般开启，方便用户查看长内容                                 |
| `CanUserSortColumns`    | `bool`                                 | `true`         | 是否允许点击列头排序                   | 数据查询表格建议开启，提升查询效率                           |
| `ColumnWidth`           | `DataGridLength`                       | `SizeToHeader` | 列宽默认值，支持固定值、Auto、星号比例 | 大数据量避免大量 `Auto` 列，会逐行计算宽度降低性能           |

### 2.3 选择交互类

在 `MultiSelector` 多选能力基础上，扩展了更细粒度的选择单位控制。

| 属性                                               | 类型                    | 默认值     | 官方作用                       | 说明                                                         |
| :------------------------------------------------- | :---------------------- | :--------- | :----------------------------- | :----------------------------------------------------------- |
| `SelectionMode`                                    | `DataGridSelectionMode` | `Extended` | 选择模式：单选 / 扩展多选      | 枚举值：`Single`（单选）、`Extended`（Ctrl 点选、Shift 连选） |
| `SelectionUnit`                                    | `DataGridSelectionUnit` | `FullRow`  | 选择单位：单元格 / 整行 / 行头 | 工业场景默认整行选择；需要单元格级操作时设为 `Cell`          |
| `SelectedItem` / `SelectedItems` / `SelectedIndex` | -                       | -          | 选中数据项                     | 全部继承自 `Selector` / `MultiSelector`，用法与 `ListBox` 一致 |

### 2.4 视觉样式类

提供行、单元格、表头、网格线等全层级的样式控制。

| 属性                                   | 类型                          | 说明                                                         |
| :------------------------------------- | :---------------------------- | :----------------------------------------------------------- |
| `RowStyle`                             | `Style`                       | 行容器 `DataGridRow` 的统一样式，控制行高、背景、选中态等    |
| `CellStyle`                            | `Style`                       | 单元格 `DataGridCell` 的统一样式，控制内边距、字体、边框等   |
| `AlternatingRowBackground`             | `Brush`                       | 交替行背景色，直接设置奇偶行不同背景，比 `ListView` 的交替行更简单 |
| `AlternationCount`                     | `int`                         | 交替行周期，默认 2，可设置多行交替                           |
| `GridLinesVisibility`                  | `DataGridGridLinesVisibility` | 网格线显示方式：`Horizontal`、`Vertical`、`All`、`None`      |
| `HeadersVisibility`                    | `DataGridHeadersVisibility`   | 表头显示方式：`Column`（仅列头）、`Row`（仅行头）、`All`、`None` |
| `ColumnHeaderStyle` / `RowHeaderStyle` | `Style`                       | 列头 / 行头的统一样式                                        |

### 2.5 行详情类

支持每行展开显示详情面板，是 `DataGrid` 特色功能。

| 属性                       | 类型                               | 作用                                                         |
| :------------------------- | :--------------------------------- | :----------------------------------------------------------- |
| `RowDetailsTemplate`       | `DataTemplate`                     | 行展开后的详情内容模板                                       |
| `RowDetailsVisibilityMode` | `DataGridRowDetailsVisibilityMode` | 详情显示模式：`VisibleWhenSelected`（选中展开）、`Visible`（全部展开）、`Collapsed`（全部折叠） |
| `AreRowDetailsFrozen`      | `bool`                             | 详情面板是否随横向滚动固定                                   |

### 2.6 性能优化类

大数据量表格的核心优化开关。

| 属性                                        | 类型     | 默认值      | 作用                                       |                                              |
| :------------------------------------------ | :------- | :---------- | :----------------------------------------- | :------------------------------------------- |
| `EnableRowVirtualization`                   | `bool`   | `true`      | 是否开启行虚拟化，仅生成可见行的 UI        | WPF 4.0+ 默认开启，万级数据的性能保障        |
| `EnableColumnVirtualization`                | `bool`   | `false`     | 是否开启列虚拟化，列数很多（>20 列）时开启 | 列数少时关闭，避免列滚动时的渲染开销         |
| `VirtualizingStackPanel.VirtualizationMode` | 附加属性 | `Recycling` | 虚拟化回收模式，复用行容器                 | 工业长列表建议设为 `Recycling`，减少 GC 压力 |

------

## 三、核心事件全解析

按生命周期分为三类，是表格定制与业务逻辑接入的核心入口。

### 3.1 编辑生命周期事件

| 事件                   | 触发时机                                 | 典型工业用法                         |
| :--------------------- | :--------------------------------------- | :----------------------------------- |
| `BeginningEdit`        | 单元格进入编辑状态前触发                 | 编辑前校验权限，禁止修改关键参数     |
| `PreparingCellForEdit` | 单元格进入编辑状态、编辑控件已生成时触发 | 初始化编辑控件的默认值、下拉选项     |
| `CellEditEnding`       | 单元格编辑结束、提交前触发               | 数据合法性校验，不符合条件可取消提交 |
| `RowEditEnding`        | 整行编辑提交前触发                       | 行级数据校验、事务性提交             |

### 3.2 行与列生命周期事件

| 事件                   | 触发时机                | 典型工业用法                                                 |
| :--------------------- | :---------------------- | :----------------------------------------------------------- |
| `AutoGeneratingColumn` | 自动生成列时逐列触发    | 自动生成列时修改列名、设置格式、隐藏不需要的列               |
| `LoadingRow`           | 行容器生成 / 复用时触发 | 自定义行样式、行号、行级状态标记                             |
| `UnloadingRow`         | 行容器回收时触发        | 清理行的自定义状态、解绑事件，防止虚拟化下状态错乱与内存泄漏 |

> ⚠️ 虚拟化关键：自定义行状态必须成对处理 `LoadingRow` / `UnloadingRow`，否则滚动时必然出现状态错乱，原理与 `PrepareContainerForItemOverride` / `ClearContainerForItemOverride` 完全一致。

### 3.3 交互事件

| 事件                | 触发时机           | 典型用法                            |
| :------------------ | :----------------- | :---------------------------------- |
| `Sorting`           | 点击列头排序时触发 | 自定义排序逻辑、服务端排序          |
| `SelectionChanged`  | 选中项变化时触发   | 选中联动详情面板，继承自 `Selector` |
| `LoadingRowDetails` | 行详情展开时触发   | 异步加载详情数据，提升性能          |

------

## 四、核心方法解析

### 4.1 公共方法

| 方法                          | 官方作用                   | 典型场景                                     |
| :---------------------------- | :------------------------- | :------------------------------------------- |
| `BeginEdit()`                 | 让当前单元格进入编辑状态   | 代码触发编辑，如新增行后自动聚焦第一个单元格 |
| `CommitEdit()`                | 提交当前单元格 / 行的编辑  | 保存按钮中统一提交所有未提交的编辑           |
| `CancelEdit()`                | 取消当前编辑，恢复原始值   | 取消按钮、撤销操作                           |
| `ScrollIntoView(object item)` | 将指定数据行滚动到可视区域 | 新增、搜索定位后自动滚动到目标行             |
| `SelectAll() / UnselectAll()` | 全选 / 取消全选            | 批量操作工具栏按钮                           |

### 4.2 受保护重写方法

`DataGrid` 重写了 `ItemsControl` 的容器生命周期方法，适配表格行容器：

1. **`GetContainerForItemOverride`**：返回 `new DataGridRow()` 作为行容器；
2. **`PrepareContainerForItemOverride`**：行生成 / 复用时，绑定数据、应用行样式、生成单元格；
3. **`ClearContainerForItemOverride`**：行回收时，清理数据、状态、单元格，适配虚拟化；
4. **`IsItemItsOwnContainerOverride`**：判断对象是否为 `DataGridRow`，支持直接添加行元素。

------

## 五、配套核心类型体系

`DataGrid` 是一套完整的表格控件体系，核心配套类型决定了其灵活性与扩展能力。

### 5.1 列类型体系（`DataGridColumn` 抽象基类）

所有列都继承自 `DataGridColumn`，官方提供 5 种标准列，覆盖绝大多数业务场景。

| 列类型                    | 适用数据类型       | 核心能力             | 工业典型场景                                     |
| :------------------------ | :----------------- | :------------------- | :----------------------------------------------- |
| `DataGridTextColumn`      | 字符串、数值、日期 | 纯文本显示与编辑     | 设备名称、编号、数值参数等绝大多数列             |
| `DataGridCheckBoxColumn`  | `bool` 布尔值      | 复选框显示与编辑     | 是否启用、是否合格、开关类参数                   |
| `DataGridComboBoxColumn`  | 枚举 / 固定选项    | 下拉选择编辑         | 设备类型、参数等级、班次等固定选项               |
| `DataGridHyperlinkColumn` | 超链接             | 链接跳转             | 文档链接、详情页跳转                             |
| `DataGridTemplateColumn`  | 任意类型           | 完全自定义单元格模板 | 状态指示灯、进度条、行内按钮、复杂布局等高级场景 |

**列通用核心属性**：`Header`（列头）、`Binding`（数据绑定）、`IsReadOnly`（是否只读）、`Width`（列宽）、`CellTemplate` / `CellEditingTemplate`（模板列专属）。

### 5.2 行与单元格容器

| 类型                   | 对应层级 | 核心作用                                                     |
| :--------------------- | :------- | :----------------------------------------------------------- |
| `DataGridRow`          | 行级     | 每条数据的 UI 容器，承载一行的所有单元格，控制行级样式、选中状态 |
| `DataGridCell`         | 单元格级 | 每个字段的 UI 容器，控制单元格样式、编辑状态、内容呈现       |
| `DataGridColumnHeader` | 列头     | 列标题容器，处理点击排序、拖拽列宽、列重排                   |
| `DataGridRowHeader`    | 行头     | 行标题容器，默认显示行选择箭头，可自定义行号、状态标记       |

------

## 六、核心工作机制

### 6.1 列生成机制

- **自动生成模式**（`AutoGenerateColumns="True"`）：根据数据源的属性类型自动匹配列类型（布尔→复选框列，其他→文本列），适合快速开发；
- **手动定义模式**（`AutoGenerateColumns="False"`）：完全由开发者定义列的数量、顺序、类型、样式，**工业生产环境推荐**，更稳定可控。

### 6.2 单元格编辑生命周期

plaintext:

```tex
用户双击/按F2进入编辑
    ↓
触发 BeginningEdit 事件（可取消）
    ↓
生成编辑控件，触发 PreparingCellForEdit
    ↓
用户修改内容
    ↓
失去焦点/按回车 → 触发 CellEditEnding 事件（可取消提交）
    ↓
验证通过 → 提交值到数据源；验证不通过 → 显示错误提示，保持编辑状态
```

### 6.3 UI 虚拟化机制

- **行虚拟化**：只生成可见区域的 `DataGridRow` 容器，滚出屏幕的行被回收复用，万级数据内存占用降低 90% 以上；
- **列虚拟化**：列数很多时，只生成可见列的单元格，横向滚动时动态生成；
- **状态持久化**：选中状态、编辑状态绑定在数据层，行容器回收时不丢失，复用时自动恢复。

### 6.4 内置排序机制

- 默认开启，点击列头切换升序 / 降序；
- 基于 `ICollectionView` 实现，支持自定义排序逻辑；
- 工业场景可扩展为多列组合排序、服务端排序。

------

## 七、基础使用方法

### 标准使用步骤

1. **绑定数据源**：`ItemsSource` 绑定 `ObservableCollection<T>` 业务集合，支持增删自动同步；
2. **配置列**：关闭自动生成列，手动添加对应类型的列，设置绑定与格式；
3. **配置编辑权限**：根据业务设置全局 / 单列 `IsReadOnly`，控制是否允许用户增删行；
4. **配置选择模式**：设置 `SelectionMode` 与 `SelectionUnit`，匹配业务交互需求；
5. **样式定制**：通过 `RowStyle`、`CellStyle`、`AlternatingRowBackground` 调整视觉效果；
6. **性能优化**：大数据量开启行虚拟化，优化列宽设置，避免过度嵌套的模板列。

------

## 八、工业场景实战实例

### 实例 1：生产配方编辑表格（文本列 + 复选框列 + 下拉列）

#### 场景说明

配方参数配置表，支持编辑参数名称、数值、是否启用、参数类型下拉选择，是工业配方管理的典型场景。

#### 1. 数据模型

csharp:

```c#
public class RecipeParam : INotifyPropertyChanged
{
    private string _paramName;
    public string ParamName
    {
        get => _paramName;
        set { _paramName = value; OnPropertyChanged(); }
    }

    private double _paramValue;
    public double ParamValue
    {
        get => _paramValue;
        set { _paramValue = value; OnPropertyChanged(); }
    }

    private bool _isEnabled;
    public bool IsEnabled
    {
        get => _isEnabled;
        set { _isEnabled = value; OnPropertyChanged(); }
    }

    private string _paramType;
    public string ParamType
    {
        get => _paramType;
        set { _paramType = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

#### 2. ViewModel

csharp:

```c#
public class RecipeViewModel : INotifyPropertyChanged
{
    public ObservableCollection<RecipeParam> ParamList { get; set; }
    public List<string> ParamTypeOptions { get; } = new List<string> { "温度", "压力", "时间", "速度" };

    public RecipeViewModel()
    {
        ParamList = new ObservableCollection<RecipeParam>
        {
            new RecipeParam { ParamName = "预热温度", ParamValue = 85.5, IsEnabled = true, ParamType = "温度" },
            new RecipeParam { ParamName = "固化时间", ParamValue = 120, IsEnabled = true, ParamType = "时间" },
            new RecipeParam { ParamName = "输送速度", ParamValue = 0.5, IsEnabled = false, ParamType = "速度" },
        };
    }

    // INotifyPropertyChanged 实现略
}
```

#### 3. XAML 界面

xaml:

```xaml
<Window.DataContext>
    <local:RecipeViewModel/>
</Window.DataContext>

<Grid Margin="10">
    <DataGrid ItemsSource="{Binding ParamList}"
              AutoGenerateColumns="False"
              CanUserAddRows="False"
              CanUserDeleteRows="False"
              SelectionMode="Single"
              SelectionUnit="FullRow"
              GridLinesVisibility="All"
              HeadersVisibility="Column"
              BorderBrush="#DDD" BorderThickness="1">
        
        <DataGrid.Columns>
            <!-- 文本列：参数名称 -->
            <DataGridTextColumn Header="参数名称" Binding="{Binding ParamName}" Width="120" IsReadOnly="True"/>
            
            <!-- 文本列：参数值，可编辑 -->
            <DataGridTextColumn Header="参数值" Binding="{Binding ParamValue}" Width="100"/>
            
            <!-- 复选框列：是否启用 -->
            <DataGridCheckBoxColumn Header="是否启用" Binding="{Binding IsEnabled}" Width="80"/>
            
            <!-- 下拉列：参数类型 -->
            <DataGridComboBoxColumn Header="参数类型" 
                                    SelectedItemBinding="{Binding ParamType}"
                                    ItemsSource="{Binding ParamTypeOptions, Source={x:Static local:RecipeViewModel.Instance}}"
                                    Width="100"/>
        </DataGrid.Columns>
    </DataGrid>
</Grid>
```

------

### 实例 2：设备参数配置表（交替行 + 只读列 + 自定义行样式）

#### 场景说明

设备参数列表，关键参数字段只读，奇偶行交替背景，选中行高亮，适配工业操作习惯。

xaml:

```xaml
<DataGrid ItemsSource="{Binding DeviceParamList}"
          AutoGenerateColumns="False"
          IsReadOnly="False"
          AlternatingRowBackground="#F8F9FA"
          GridLinesVisibility="Horizontal"
          CanUserReorderColumns="False"
          BorderBrush="#DDD" BorderThickness="1">
    
    <!-- 行容器样式 -->
    <DataGrid.RowStyle>
        <Style TargetType="DataGridRow">
            <Setter Property="Height" Value="30"/>
            <Setter Property="Background" Value="White"/>
            <Style.Triggers>
                <Trigger Property="IsSelected" Value="True">
                    <Setter Property="Background" Value="#E6F4FF"/>
                    <Setter Property="Foreground" Value="#333"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </DataGrid.RowStyle>

    <!-- 单元格样式 -->
    <DataGrid.CellStyle>
        <Style TargetType="DataGridCell">
            <Setter Property="VerticalContentAlignment" Value="Center"/>
            <Setter Property="Padding" Value="6 2"/>
            <Setter Property="BorderThickness" Value="0"/>
        </Style>
    </DataGrid.CellStyle>

    <DataGrid.Columns>
        <DataGridTextColumn Header="参数编码" Binding="{Binding ParamCode}" Width="100" IsReadOnly="True"/>
        <DataGridTextColumn Header="参数名称" Binding="{Binding ParamName}" Width="150" IsReadOnly="True"/>
        <DataGridTextColumn Header="当前值" Binding="{Binding CurrentValue}" Width="100"/>
        <DataGridTextColumn Header="单位" Binding="{Binding Unit}" Width="60" IsReadOnly="True"/>
        <DataGridTextColumn Header="量程范围" Binding="{Binding Range}" Width="120" IsReadOnly="True"/>
    </DataGrid.Columns>
</DataGrid>
```

------

### 实例 3：万级历史数据高性能表格（行虚拟化 + 性能优化）

#### 场景说明

上万条生产历史记录，开启虚拟化优化，保证滚动流畅、内存占用低。

xaml:

```xaml
<DataGrid ItemsSource="{Binding HistoryRecordList}"
          AutoGenerateColumns="False"
          EnableRowVirtualization="True"
          VirtualizingStackPanel.VirtualizationMode="Recycling"
          VirtualizingStackPanel.IsVirtualizing="True"
          ScrollViewer.IsDeferredScrollingEnabled="True"
          CanUserReorderColumns="False"
          CanUserSortColumns="True"
          AlternatingRowBackground="#F8F9FA"
          Height="500" BorderBrush="#DDD" BorderThickness="1">
    
    <DataGrid.Columns>
        <DataGridTextColumn Header="记录时间" Binding="{Binding RecordTime, StringFormat=yyyy-MM-dd HH:mm:ss}" Width="150"/>
        <DataGridTextColumn Header="设备编号" Binding="{Binding DeviceCode}" Width="100"/>
        <DataGridTextColumn Header="温度" Binding="{Binding Temperature, StringFormat=F1}" Width="80"/>
        <DataGridTextColumn Header="压力" Binding="{Binding Pressure, StringFormat=F2}" Width="80"/>
        <DataGridTextColumn Header="状态" Binding="{Binding Status}" Width="80"/>
        <DataGridTextColumn Header="备注" Binding="{Binding Remark}" Width="*"/>
    </DataGrid.Columns>
</DataGrid>
```

#### 性能说明

- 开启行虚拟化 + 回收模式后，10 万条数据内存占用仅为全量渲染的 5%~10%；
- 延迟滚动开启后，拖动滚动条时仅显示提示，松开后再渲染，大幅提升拖动流畅度；
- 关闭自动生成列、固定列宽，避免逐行计算宽度，进一步提升性能。

------

### 实例 4：行详情展开（设备详情查看）

#### 场景说明

表格行选中时展开，显示设备的详细参数、维护记录，适合信息量大的台账场景。

xaml:

```xaml
<DataGrid ItemsSource="{Binding DeviceList}"
          AutoGenerateColumns="False"
          RowDetailsVisibilityMode="VisibleWhenSelected"
          AreRowDetailsFrozen="True"
          BorderBrush="#DDD" BorderThickness="1">
    
    <DataGrid.Columns>
        <DataGridTextColumn Header="设备编号" Binding="{Binding DeviceCode}" Width="100"/>
        <DataGridTextColumn Header="设备名称" Binding="{Binding DeviceName}" Width="150"/>
        <DataGridTextColumn Header="运行状态" Binding="{Binding StatusText}" Width="80"/>
    </DataGrid.Columns>

    <!-- 行详情模板 -->
    <DataGrid.RowDetailsTemplate>
        <DataTemplate>
            <Border Background="#F5F7FA" Padding="15" BorderBrush="#DDD" BorderThickness="1 0 1 1">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>
                    <StackPanel>
                        <TextBlock Text="{Binding DeviceDesc}" TextWrapping="Wrap"/>
                        <TextBlock Margin="0 5" Text="{Binding InstallTime, StringFormat=安装时间：{0:yyyy-MM-dd}}"/>
                    </StackPanel>
                    <StackPanel Grid.Column="1">
                        <TextBlock Text="{Binding MaintainTime, StringFormat=上次维护：{0:yyyy-MM-dd}}"/>
                        <TextBlock Margin="0 5" Text="{Binding Maintainer, StringFormat=维护人员：{0}}"/>
                    </StackPanel>
                </Grid>
            </Border>
        </DataTemplate>
    </DataGrid.RowDetailsTemplate>
</DataGrid>
```

------

### 实例 5：自定义模板列（行内操作按钮）

#### 场景说明

每行带编辑、删除操作按钮，是业务管理表格的常用交互。

xaml:

```xaml
<DataGrid ItemsSource="{Binding RecipeList}"
          AutoGenerateColumns="False"
          CanUserAddRows="False"
          BorderBrush="#DDD" BorderThickness="1">
    
    <DataGrid.Columns>
        <DataGridTextColumn Header="配方编号" Binding="{Binding RecipeCode}" Width="100"/>
        <DataGridTextColumn Header="配方名称" Binding="{Binding RecipeName}" Width="150"/>
        <DataGridTextColumn Header="适用产品" Binding="{Binding ProductType}" Width="120"/>
        
        <!-- 模板列：操作按钮 -->
        <DataGridTemplateColumn Header="操作" Width="120">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <StackPanel Orientation="Horizontal">
                        <Button Content="编辑" Click="EditRecipe_Click" Margin="0 0 8 0" Padding="8 2"/>
                        <Button Content="删除" Click="DeleteRecipe_Click" Padding="8 2" Foreground="Red"/>
                    </StackPanel>
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>
    </DataGrid.Columns>
</DataGrid>
```

csharp:

```c#
private void EditRecipe_Click(object sender, RoutedEventArgs e)
{
    // 获取当前行的数据
    var button = sender as Button;
    var recipe = button?.DataContext as RecipeInfo;
    if (recipe != null)
    {
        // 打开编辑窗口
    }
}
```

------

## 九、选型对比与最佳实践

### 9.1 ListView+GridView vs DataGrid 选型对比

| 维度             | ListView + GridView              | DataGrid                                 |
| :--------------- | :------------------------------- | :--------------------------------------- |
| 核心定位         | 只读多列数据展示                 | 可编辑数据表格                           |
| 原生编辑能力     | 无，需自定义模板实现             | 原生支持单元格 / 行编辑、验证            |
| 性能表现         | 更轻量，渲染开销更低             | 功能丰富，开销相对更高                   |
| 虚拟化支持       | 行虚拟化                         | 行虚拟化 + 列虚拟化                      |
| 学习成本         | 较低                             | 较高，属性与事件更多                     |
| **典型工业场景** | 设备台账、报警明细、生产记录查询 | 配方编辑、参数配置、数据录入、可编辑台账 |

> 💡 选型原则：**只读展示优先用 ListView+GridView，需要编辑再用 DataGrid**。ListView 性能更优、样式更灵活，能覆盖绝大多数只读多列场景；DataGrid 功能全面但更重，仅在需要编辑时使用。

### 9.2 工业场景最佳实践

1. **关闭不必要的功能**：生产环境关闭自动生成列、用户增删行、列重排，提升稳定性与操作规范性。
2. **大数据量必开虚拟化**：500 行以上默认开启行虚拟化，20 列以上开启列虚拟化，配合回收模式降低内存与 GC 压力。
3. **验证逻辑下沉到模型**：数据校验实现 `IDataErrorInfo` 接口，不要大量写在 `CellEditEnding` 事件中，逻辑更清晰、可复用。
4. **模板列避免过度嵌套**：单元格内视觉树层级过深会严重影响滚动性能，尽量用简单布局。
5. **永远操作数据层**：不要遍历 `DataGridRow` / `DataGridCell` 获取或修改数据，直接操作绑定的业务集合，虚拟化下操作 UI 必然失效。

### 9.3 常见坑点

1. **虚拟化下遍历行返回 null**：不可见的行没有生成容器，永远通过数据源操作数据。
2. **自动生成列顺序不可控**：自动生成列的顺序由反射属性顺序决定，不稳定，生产环境建议手动定义列。
3. **编辑状态未提交导致数据丢失**：关闭窗口前调用 `CommitEdit()`，确保所有未提交的编辑保存到数据源。
4. **交替行滚动错乱**：自定义行样式时，不要在代码 - behind 中直接修改行的背景色，必须通过数据绑定或样式触发器实现，否则虚拟化回收后状态错乱。

------

## 总结

`DataGrid` 是 WPF 原生最强大的数据表格控件，提供了从展示到编辑、从排序到验证的完整能力，是工业软件中可编辑表格场景的首选。它继承了 `ItemsControl` 体系的虚拟化能力与数据驱动思想，通过丰富的列类型与事件扩展，既能快速实现简单表格，也能支撑复杂的编辑与校验业务。