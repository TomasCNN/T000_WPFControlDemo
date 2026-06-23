# 005004001_WPF `ListView` 数据列表控件官方定义与工业场景实战

`ListView` 是 WPF 结构化数据展示的核心控件，直接继承自 `ListBox`，在完整保留列表选择、内置滚动、UI 虚拟化等全部能力的基础上，**新增了可插拔的视图架构**，最典型的实现是 `GridView` 多列表格视图。

它兼顾了 `ListBox` 的轻量高性能与表格控件的多列展示能力，是工业软件中设备台账、生产记录、报警明细、参数清单等结构化数据展示的首选控件。

------

## 一、官方类定义总览与核心元数据

### 1.1 完整类签名（官方原生定义）

csharp:

```c#
namespace System.Windows.Controls
{
    [System.Windows.LocalizabilityAttribute(System.Windows.LocalizationCategory.None, Readability = System.Windows.Readability.Unreadable)]
    [System.Windows.StyleTypedPropertyAttribute(Property = "ItemContainerStyle", StyleTargetType = typeof(System.Windows.Controls.ListViewItem))]
    public class ListView : System.Windows.Controls.ListBox
    {
        // 新增核心依赖属性
        public static readonly System.Windows.DependencyProperty ViewProperty;

        // 构造函数
        public ListView();

        // 新增核心属性
        public System.Windows.Controls.ViewBase View { get; set; }

        // 受保护重写方法
        protected override System.Windows.DependencyObject GetContainerForItemOverride();
        protected override bool IsItemItsOwnContainerOverride(object item);
        protected override void PrepareContainerForItemOverride(System.Windows.DependencyObject element, object item);
        protected override void ClearContainerForItemOverride(System.Windows.DependencyObject element, object item);
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer();
        
        // 视图变更回调
        protected virtual void OnViewChanged(System.Windows.Controls.ViewBase oldView, System.Windows.Controls.ViewBase newView);
    }
}
```

### 1.2 核心元数据（官方精确值）

| 项               | 官方精确值                                                   | 工业场景说明                               |
| :--------------- | :----------------------------------------------------------- | :----------------------------------------- |
| **命名空间**     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                       |
| **程序集**       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                         |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → Selector → ListBox → ListView` | 完整继承列表选择与集合呈现能力             |
| **默认条目容器** | `ListViewItem`                                               | 继承自 `ListBoxItem`，完全兼容列表选中逻辑 |
| **核心扩展**     | `View` 属性 + `ViewBase` 视图抽象                            | 支持多列表格、图标列表等多种视图模式切换   |
| **内置标准视图** | `GridView`（多列表格）                                       | 工业场景最常用的多列数据展示方案           |
| **工业核心场景** | 设备台账、生产记录、报警明细、参数清单、配方管理             | 所有结构化多列数据的只读 / 轻交互展示      |

### 1.3 类级特性说明

1. **`[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ListViewItem))]`**
   - 声明 `ItemContainerStyle` 的目标容器类型为 `ListViewItem`，设计器可提供准确的样式智能提示。
2. **`[Localizability]`**：控件本身无固定文本，内容全部由业务数据与列头定义决定。

------

## 二、核心依赖属性全解析

### 2.1 新增核心属性：View

csharp:

```c#
public static readonly DependencyProperty ViewProperty;
public ViewBase View { get; set; }
```

- **类型**：`ViewBase`（抽象基类）
- **默认值**：`null`，此时表现和普通 `ListBox` 完全一致
- **官方作用**：指定列表的视图呈现模式，是 `ListView` 区别于 `ListBox` 的核心标志
- **内置实现**：WPF 官方仅提供 `GridView` 一种标准实现（多列表格），可通过继承 `ViewBase` 自定义图标视图、卡片视图等
- **工业场景价值**：
  - 设置为 `GridView` 实现多列结构化数据展示，替代轻量表格；
  - 可动态切换视图（列表模式 / 表格模式），满足不同查看需求；
  - 视图与数据完全解耦，同一数据源可切换不同展示形式。

### 2.2 继承的高频核心属性

`ListView` 完整继承 `ListBox` / `Selector` / `ItemsControl` 的全部属性，工业场景高频使用的包括：

| 分类     | 属性                                               | 来源         | 作用                                 |
| :------- | :------------------------------------------------- | :----------- | :----------------------------------- |
| 数据绑定 | `ItemsSource`                                      | ItemsControl | 绑定业务数据集合，MVVM 标准入口      |
| 选择能力 | `SelectionMode`                                    | ListBox      | 单选 / 简单多选 / 扩展多选，默认单选 |
|          | `SelectedItem` / `SelectedIndex` / `SelectedValue` | Selector     | 单选数据绑定                         |
|          | `SelectedItems`                                    | ListBox      | 多选模式下的选中集合                 |
| 容器样式 | `ItemContainerStyle`                               | ItemsControl | 自定义 `ListViewItem` 行样式         |
| 布局性能 | `ItemsPanel`                                       | ItemsControl | 替换虚拟化面板，大数据量性能优化     |
| 交替行   | `AlternationCount`                                 | ItemsControl | 奇偶行交替背景，提升可读性           |
| 文本搜索 | `IsTextSearchEnabled`                              | Selector     | 键盘输入快速定位条目                 |
| 分组     | `GroupStyle`                                       | ItemsControl | 按字段分组展示数据                   |

------

## 三、配套核心类型说明

`ListView` 的视图架构由一组配套类型共同实现，是理解其工作原理的关键。

### 3.1 ListViewItem：条目容器

csharp:

```c#
public class ListViewItem : ListBoxItem
{ }
```

- 继承自 `ListBoxItem`，**本身无额外逻辑**，仅作为类型区分，方便样式与视图适配；
- 完整继承选中、鼠标交互、键盘导航等全部能力；
- `ListView` 重写 `GetContainerForItemOverride` 时返回该类型。

### 3.2 ViewBase：视图抽象基类

csharp:

```c#
public abstract class ViewBase : DependencyObject
{
    protected internal abstract object DefaultStyleKey { get; }
    protected internal virtual void PrepareItem(ListViewItem item);
    protected internal virtual void ClearItem(ListViewItem item);
}
```

- 所有视图模式的抽象基类，定义了视图的生命周期契约；
- 负责条目容器的样式准备、清理，以及控件默认样式的切换；
- `GridView` 就是该类的标准实现，自定义视图必须继承此类。

### 3.3 GridView：标准多列视图

- 官方唯一内置的 `ViewBase` 实现，以表格列的形式展示数据；
- 核心属性 `Columns` 是 `GridViewColumn` 集合，定义每一列的表头、绑定、宽度、模板；
- 支持列宽拖动调整、列头点击排序（需自行实现排序逻辑）、列头样式自定义；
- 是工业场景 90% 以上 `ListView` 场景的选择。

### 3.4 GridViewColumn：列定义

| 核心属性               | 类型           | 作用                                                 |
| :--------------------- | :------------- | :--------------------------------------------------- |
| `Header`               | `object`       | 列头显示内容，通常为字符串                           |
| `DisplayMemberBinding` | `BindingBase`  | 单元格数据绑定，简单文本展示用                       |
| `CellTemplate`         | `DataTemplate` | 单元格自定义模板，复杂内容（状态灯、进度条、按钮）用 |
| `Width`                | `double`       | 列宽，支持固定值、`Auto`、星号比例                   |
| `HeaderTemplate`       | `DataTemplate` | 自定义列头外观                                       |

> 🔑 优先级规则：设置了 `CellTemplate` 时，`DisplayMemberBinding` 无效，优先使用自定义模板。

------

## 四、核心方法与重写逻辑

### 4.1 容器生命周期重写

`ListView` 重写了 `ListBox` 的容器生命周期方法，主要是适配视图架构：

1. **`GetContainerForItemOverride()`**
   - 返回 `new ListViewItem()`，指定默认容器类型；
   - 视图会附加到容器上，控制单元格呈现。
2. **`PrepareContainerForItemOverride(DependencyObject element, object item)`**
   - 调用基类完成基础准备；
   - 调用当前 `View` 的 `PrepareItem` 方法，给容器应用视图对应的样式与模板。
3. **`ClearContainerForItemOverride(DependencyObject element, object item)`**
   - 调用当前 `View` 的 `ClearItem` 方法，清理视图相关的绑定与样式；
   - 调用基类完成基础清理，适配 UI 虚拟化的容器回收。

### 4.2 视图变更回调

csharp:

```c#
protected virtual void OnViewChanged(ViewBase oldView, ViewBase newView);
```

- `View` 属性变更时触发；
- 负责卸载旧视图、挂载新视图，刷新所有条目的呈现方式；
- 自定义子类可重写此方法，注入视图切换的自定义逻辑。

------

## 五、核心功能与选型定位

### 5.1 核心能力清单

1. **多列结构化展示**：通过 `GridView` 实现表格样式的多列数据展示，支持自定义单元格内容；
2. **完整选择能力**：完整继承 `ListBox` 的单选 / 多选、Shift 连选、Ctrl 点选、拖拽框选；
3. **高性能虚拟化**：支持 UI 虚拟化与容器回收，万级数据流畅滚动；
4. **视图可切换**：同一数据源可动态切换不同视图模式（表格 / 列表 / 卡片）；
5. **分组 / 排序 / 过滤**：配合 `CollectionViewSource` 实现数据分组、排序、筛选；
6. **样式高度可定制**：行样式、单元格样式、列头样式均可完全自定义。

### 5.2 工业场景选型对比：ListBox / ListView / DataGrid

| 维度             | ListBox                      | ListView + GridView                    | DataGrid                           |
| :--------------- | :--------------------------- | :------------------------------------- | :--------------------------------- |
| 核心定位         | 单列列表选择                 | 多列只读 / 轻交互列表                  | 可编辑复杂表格                     |
| 列数             | 单列（自定义模板可模拟多列） | 多列，原生支持                         | 多列，原生支持                     |
| 编辑能力         | 无原生编辑                   | 无原生编辑，需自定义模板               | 原生支持单元格编辑、行编辑         |
| 性能表现         | 最优，轻量                   | 优秀，多列开销略高于 ListBox           | 功能最全但开销最大                 |
| 虚拟化支持       | 完美支持                     | 完美支持                               | 支持但配置更复杂                   |
| 学习成本         | 最低                         | 较低                                   | 较高                               |
| **典型工业场景** | 设备列表、菜单导航、报警单选 | 设备台账、生产记录、报警明细、参数清单 | 配方编辑、参数配置、可编辑数据表格 |

> 💡 选型原则：**只读多列展示优先用 ListView+GridView，需要编辑再用 DataGrid**。ListView 性能更优、样式更灵活，是工业软件中绝大多数只读结构化数据场景的最佳选择。

------

## 六、基础使用方法

### 6.1 标准使用步骤

1. **绑定数据源**：`ItemsSource` 绑定 `ObservableCollection<T>` 业务集合；
2. **配置视图**：设置 `View` 为 `GridView`，添加 `GridViewColumn` 定义每一列；
3. **配置选择模式**：根据业务需求设置 `SelectionMode`；
4. **性能优化**：大数据量替换 `ItemsPanel` 为虚拟化面板；
5. **样式定制**：通过 `ItemContainerStyle` 自定义行样式，通过 `CellTemplate` 自定义单元格。

### 6.2 GridView 基础写法

xaml:

```xaml
<ListView ItemsSource="{Binding DeviceList}">
    <!-- 设置视图为多列表格 -->
    <ListView.View>
        <GridView>
            <!-- 每一列对应数据的一个属性 -->
            <GridViewColumn Header="设备编号" DisplayMemberBinding="{Binding DeviceCode}" Width="100"/>
            <GridViewColumn Header="设备名称" DisplayMemberBinding="{Binding DeviceName}" Width="150"/>
            <GridViewColumn Header="当前温度" DisplayMemberBinding="{Binding Temperature, StringFormat={}{0:F1}℃}" Width="100"/>
            <GridViewColumn Header="运行状态" DisplayMemberBinding="{Binding StatusText}" Width="80"/>
        </GridView>
    </ListView.View>
</ListView>
```

------

## 七、工业场景实战实例

### 实例 1：设备台账多列展示（GridView 基础用法）

#### 场景说明

展示车间所有设备的编号、名称、类型、温度、状态等结构化信息，是设备管理系统的基础页面。

#### 1. 数据模型

csharp:

```c#
public class DeviceRecord : INotifyPropertyChanged
{
    public string DeviceCode { get; set; }
    public string DeviceName { get; set; }
    public string DeviceType { get; set; }
    public double Temperature { get; set; }
    public bool IsRunning { get; set; }
    public string StatusText => IsRunning ? "运行中" : "待机";

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

#### 2. ViewModel

csharp:

```c#
public class DeviceViewModel : INotifyPropertyChanged
{
    public ObservableCollection<DeviceRecord> DeviceList { get; set; }
    
    private DeviceRecord _selectedDevice;
    public DeviceRecord SelectedDevice
    {
        get => _selectedDevice;
        set { _selectedDevice = value; OnPropertyChanged(); }
    }

    public DeviceViewModel()
    {
        DeviceList = new ObservableCollection<DeviceRecord>
        {
            new DeviceRecord { DeviceCode = "PLC-001", DeviceName = "喷涂机器人A1", DeviceType = "机器人", Temperature = 42.5, IsRunning = true },
            new DeviceRecord { DeviceCode = "PLC-002", DeviceName = "喷涂机器人A2", DeviceType = "机器人", Temperature = 45.1, IsRunning = true },
            new DeviceRecord { DeviceCode = "PLC-003", DeviceName = "固化炉B1", DeviceType = "加热设备", Temperature = 85.3, IsRunning = false },
            new DeviceRecord { DeviceCode = "PLC-004", DeviceName = "上料机C1", DeviceType = "输送设备", Temperature = 36.2, IsRunning = true },
        };
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

#### 3. XAML 界面

xaml:

```xaml
<Window.DataContext>
    <local:DeviceViewModel/>
</Window.DataContext>

<Grid Margin="10">
    <ListView ItemsSource="{Binding DeviceList}"
              SelectedItem="{Binding SelectedDevice}"
              BorderBrush="#DDD" BorderThickness="1">
        <ListView.View>
            <GridView>
                <GridViewColumn Header="设备编号" DisplayMemberBinding="{Binding DeviceCode}" Width="100"/>
                <GridViewColumn Header="设备名称" DisplayMemberBinding="{Binding DeviceName}" Width="150"/>
                <GridViewColumn Header="设备类型" DisplayMemberBinding="{Binding DeviceType}" Width="100"/>
                <GridViewColumn Header="温度(℃)" DisplayMemberBinding="{Binding Temperature, StringFormat=F1}" Width="80"/>
                <GridViewColumn Header="运行状态" DisplayMemberBinding="{Binding StatusText}" Width="80"/>
            </GridView>
        </ListView.View>
    </ListView>
</Grid>
```

------

### 实例 2：生产记录多选批量导出

#### 场景说明

生产记录列表支持 Ctrl 点选、Shift 连选，批量导出选中的记录为 Excel/CSV 文件。

#### 核心代码

xaml:

```xaml
<Grid Margin="10">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>

    <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0 0 0 8">
        <Button Content="导出选中" Click="ExportSelected_Click" Padding="12 4"/>
        <TextBlock Margin="20 0 0 0" VerticalAlignment="Center">
            已选中 <Run Text="{Binding SelectedCount}"/> 条
        </TextBlock>
    </StackPanel>

    <ListView Grid.Row="1"
              x:Name="ProductionList"
              ItemsSource="{Binding ProductionRecords}"
              SelectionMode="Extended"
              SelectionChanged="ProductionList_SelectionChanged"
              BorderBrush="#DDD" BorderThickness="1">
        <ListView.View>
            <GridView>
                <GridViewColumn Header="生产批次" DisplayMemberBinding="{Binding BatchNo}" Width="120"/>
                <GridViewColumn Header="产品型号" DisplayMemberBinding="{Binding ProductModel}" Width="120"/>
                <GridViewColumn Header="产量" DisplayMemberBinding="{Binding Qty}" Width="80"/>
                <GridViewColumn Header="良率" DisplayMemberBinding="{Binding YieldRate, StringFormat={}{0:P1}}" Width="80"/>
                <GridViewColumn Header="生产时间" DisplayMemberBinding="{Binding ProduceTime, StringFormat=yyyy-MM-dd HH:mm}" Width="140"/>
            </GridView>
        </ListView.View>
    </ListView>
</Grid>
```

csharp:

```c#
public int SelectedCount { get; set; }

private void ProductionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    SelectedCount = ProductionList.SelectedItems.Count;
    OnPropertyChanged(nameof(SelectedCount));
}

private void ExportSelected_Click(object sender, RoutedEventArgs e)
{
    var selected = ProductionList.SelectedItems.Cast<ProductionRecord>().ToList();
    if (selected.Count == 0)
    {
        MessageBox.Show("请先选择要导出的记录");
        return;
    }
    
    // 批量导出Excel/CSV业务逻辑
    ExportToCsv(selected);
    MessageBox.Show($"成功导出 {selected.Count} 条记录");
}
```

------

### 实例 3：自定义状态列（状态灯 + 颜色标识）

#### 场景说明

运行状态列不用文字，用颜色圆点直观展示，异常温度高亮显示，提升工业监控界面的可读性。

xaml:

```xaml
<ListView ItemsSource="{Binding DeviceList}" BorderBrush="#DDD" BorderThickness="1">
    <ListView.View>
        <GridView>
            <GridViewColumn Header="设备编号" DisplayMemberBinding="{Binding DeviceCode}" Width="100"/>
            <GridViewColumn Header="设备名称" DisplayMemberBinding="{Binding DeviceName}" Width="150"/>
            
            <!-- 自定义状态列：颜色圆点 -->
            <GridViewColumn Header="状态" Width="60">
                <GridViewColumn.CellTemplate>
                    <DataTemplate>
                        <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
                            <Ellipse Width="10" Height="10" VerticalAlignment="Center">
                                <Ellipse.Style>
                                    <Style TargetType="Ellipse">
                                        <Setter Property="Fill" Value="Gray"/>
                                        <Style.Triggers>
                                            <DataTrigger Binding="{Binding IsRunning}" Value="True">
                                                <Setter Property="Fill" Value="#52C41A"/>
                                            </DataTrigger>
                                            <DataTrigger Binding="{Binding IsAlarm}" Value="True">
                                                <Setter Property="Fill" Value="#F5222D"/>
                                            </DataTrigger>
                                        </Style.Triggers>
                                    </Style>
                                </Ellipse.Style>
                            </Ellipse>
                        </StackPanel>
                    </DataTemplate>
                </GridViewColumn.CellTemplate>
            </GridViewColumn>
            
            <!-- 温度列：超温红色高亮 -->
            <GridViewColumn Header="温度(℃)" Width="80">
                <GridViewColumn.CellTemplate>
                    <DataTemplate>
                        <TextBlock Text="{Binding Temperature, StringFormat=F1}" VerticalAlignment="Center">
                            <TextBlock.Style>
                                <Style TargetType="TextBlock">
                                    <Setter Property="Foreground" Value="#333"/>
                                    <Style.Triggers>
                                        <DataTrigger Binding="{Binding IsOverTemp}" Value="True">
                                            <Setter Property="Foreground" Value="#F5222D"/>
                                            <Setter Property="FontWeight" Value="Bold"/>
                                        </DataTrigger>
                                    </Style.Triggers>
                                </Style>
                            </TextBlock.Style>
                        </TextBlock>
                    </DataTemplate>
                </GridViewColumn.CellTemplate>
            </GridViewColumn>
        </GridView>
    </ListView.View>
</ListView>
```

------

### 实例 4：万级历史数据高性能列表

#### 场景说明

上万条历史报警记录，开启 UI 虚拟化与容器回收，配合交替行样式，保证滚动流畅、内存占用低。

xaml:

```xaml
<ListView ItemsSource="{Binding HistoryAlarmList}"
          AlternationCount="2"
          VirtualizingStackPanel.IsVirtualizing="True"
          VirtualizingStackPanel.VirtualizationMode="Recycling"
          ScrollViewer.IsDeferredScrollingEnabled="True"
          BorderBrush="#DDD" BorderThickness="1"
          Height="500">
    
    <!-- 虚拟化布局面板 -->
    <ListView.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel/>
        </ItemsPanelTemplate>
    </ListView.ItemsPanel>

    <!-- 行样式：交替行背景 -->
    <ListView.ItemContainerStyle>
        <Style TargetType="ListViewItem">
            <Setter Property="Background" Value="White"/>
            <Style.Triggers>
                <Trigger Property="ItemsControl.AlternationIndex" Value="1">
                    <Setter Property="Background" Value="#F8F9FA"/>
                </Trigger>
                <Trigger Property="IsSelected" Value="True">
                    <Setter Property="Background" Value="#E6F4FF"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </ListView.ItemContainerStyle>

    <ListView.View>
        <GridView>
            <GridViewColumn Header="时间" DisplayMemberBinding="{Binding AlarmTime, StringFormat=yyyy-MM-dd HH:mm:ss}" Width="150"/>
            <GridViewColumn Header="级别" DisplayMemberBinding="{Binding Level}" Width="60"/>
            <GridViewColumn Header="设备" DisplayMemberBinding="{Binding DeviceName}" Width="100"/>
            <GridViewColumn Header="报警内容" DisplayMemberBinding="{Binding Message}" Width="*"/>
        </GridView>
    </ListView.View>
</ListView>
```

> 💡 性能说明：开启虚拟化后，10 万条数据内存占用仅为全量生成的 5%~10%，滚动流畅无卡顿，是工业大数据量列表的标配优化。

------

## 八、最佳实践与常见坑点

### 8.1 工业场景最佳实践

1. **只读多列优先用 ListView+GridView**

   相比 DataGrid 更轻量、性能更优、样式更灵活，绝大多数只读展示场景完全够用。

2. **大数据量必开虚拟化**

   500 行以上必须开启 `VirtualizingStackPanel` + `Recycling` 回收模式，大幅降低内存占用与 GC 压力。

3. **简单列用 DisplayMemberBinding，复杂列用 CellTemplate**

   纯文本展示直接用 `DisplayMemberBinding`，性能更好；状态灯、按钮、进度条等复杂内容用 `CellTemplate`。

4. **行样式与单元格样式分离**

   整行背景、高度、边距用 `ItemContainerStyle`；单元格内部内容用 `CellTemplate`，职责清晰、易维护。

5. **多选场景复用 ListBox 经验**

   `ListView` 完全继承 `ListBox` 的选择逻辑，`SelectionMode`、`SelectedItems`、`SelectAll()` 等用法完全一致。

### 8.2 常见坑点

1. **GridView 列宽设置不当导致性能差**
   - 大量列设置 `Width="Auto"` 会导致每一行都要计算宽度，性能急剧下降；
   - 优化：固定列宽或比例列宽，大数据量避免大量 Auto 列。
2. **虚拟化失效**
   - 常见原因：外层包裹了 `ScrollViewer`、`ItemsPanel` 被替换为普通 `StackPanel`、行高不固定；
   - 后果：全量生成容器，内存暴涨、滚动卡顿。
3. **单元格数据不更新**
   - 现象：修改数据属性，表格内容不刷新；
   - 原因：数据模型未实现 `INotifyPropertyChanged`，或绑定方式错误。
4. **列头与内容不对齐**
   - 虚拟化滚动时偶尔出现列偏移，通常是因为自定义模板内边距、边距不一致导致，统一单元格内边距即可。

------

## 总结

`ListView` 是 `ListBox` 的超集，核心价值在于 `View` 视图抽象架构，通过 `GridView` 实现了轻量级的多列表格展示。它既保留了列表控件的高性能与高灵活性，又满足了结构化数据的多列展示需求，是工业软件中设备台账、生产记录、报警明细等场景的最优选择。