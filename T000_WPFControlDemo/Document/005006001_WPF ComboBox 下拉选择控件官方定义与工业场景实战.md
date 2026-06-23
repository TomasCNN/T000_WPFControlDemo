# 005006001_WPF `ComboBox` 下拉选择控件官方定义与工业场景实战

`ComboBox` 是 WPF 最经典的下拉选择控件，直接继承自 `Selector` 单选基类，在集合呈现与单选能力的基础上，通过**折叠式下拉面板**兼顾界面空间利用率与选择便捷性，同时支持可编辑输入模式，是工业软件中参数选型、设备筛选、配方选择、字典项配置的首选下拉控件。

它的下拉面板本质是一个内嵌的单选 `ListBox`，天然继承了项模板定制、UI 虚拟化、文本搜索等能力，既能满足简单单选，也能支撑千级数据的高性能下拉选择。

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
        // 核心依赖属性字段
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

        // 核心公共属性
        public bool IsDropDownOpen { get; set; }
        public bool IsEditable { get; set; }
        public bool IsReadOnly { get; set; }
        public string Text { get; set; }
        public double MaxDropDownHeight { get; set; }
        public bool StaysOpenOnEdit { get; set; }
        public object SelectionBoxItem { get; }
        public DataTemplate SelectionBoxItemTemplate { get; }

        // 核心事件
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
        protected override System.Windows.Input.KeyDownEventArgs OnKeyDown(System.Windows.Input.KeyEventArgs e);
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer();
    }
}
```

### 1.2 核心元数据（官方精确值）

| 项               | 官方精确值                                                   | 工业场景关键说明                                   |
| :--------------- | :----------------------------------------------------------- | :------------------------------------------------- |
| **命名空间**     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                               |
| **程序集**       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                                 |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → Selector → ComboBox` | 完整继承集合呈现与单选管理能力                     |
| **默认条目容器** | `ComboBoxItem`                                               | 下拉项的 UI 容器，继承自 `ContentControl`          |
| **核心交互模式** | 普通选择模式 / 可编辑输入模式                                | 支持纯下拉选择和文本输入搜索两种交互               |
| **下拉承载机制** | `Popup` + 内嵌单选 `ListBox`                                 | 下拉面板本质是单选 ListBox，天然支持虚拟化、项模板 |
| **工业核心场景** | 参数选型、设备筛选、配方选择、字典项配置、级联下拉           | 所有单项选择的紧凑交互场景                         |

### 1.3 类级特性深度解析

1. **`[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ComboBoxItem))]`**
   - 向设计器声明 `ItemContainerStyle` 的目标容器类型为 `ComboBoxItem`，提供样式智能提示与类型校验；
   - 与 `ListBox` 设计一致，均遵循 `ItemsControl` 的容器样式约定。
2. **`[Localizability(LocalizationCategory.None)]`**
   - 控件本身无固定本地化文本，所有显示内容由业务数据与项模板决定，无需框架级本地化处理。

------

## 二、核心依赖属性全量解析

`ComboBox` 在继承 `ItemsControl` / `Selector` 全部属性的基础上，新增了下拉控制、可编辑模式两类核心属性。

### 2.1 新增核心属性

| 属性                  | 类型     | 默认值                    | 官方作用                                     | 工业最佳实践                                                 |
| :-------------------- | :------- | :------------------------ | :------------------------------------------- | :----------------------------------------------------------- |
| `IsDropDownOpen`      | `bool`   | `false`                   | 下拉列表是否展开，支持双向绑定               | 可通过 ViewModel 控制下拉展开 / 收起，实现自定义触发按钮、扫码自动展开等逻辑 |
| `IsEditable`          | `bool`   | `false`                   | 是否启用可编辑模式，顶部显示文本框支持输入   | 需要模糊搜索、支持自定义输入的场景开启，如配方编号快速检索、设备编码模糊匹配 |
| `IsReadOnly`          | `bool`   | `false`                   | 可编辑模式下，文本框是否仅可读               | 配合 `IsEditable=true` 实现「可搜索、不可修改」的效果，只能从列表中选择，不能手动输入非法值 |
| `Text`                | `string` | `string.Empty`            | 可编辑模式下文本框的显示内容                 | 可绑定实现输入联动、实时筛选；注意与 `SelectedItem` 的同步逻辑 |
| `MaxDropDownHeight`   | `double` | 系统默认值                | 下拉列表的最大高度，超出后显示垂直滚动条     | 工业长列表建议设置为 250~400px，避免下拉占满屏幕，操作体验更佳 |
| `StaysOpenOnEdit`     | `bool`   | `false`                   | 可编辑模式下，输入文本时是否保持下拉展开     | 实时筛选场景设为 `true`，边输入边过滤下拉结果，直观展示匹配项 |
| `IsTextSearchEnabled` | `bool`   | `true`（继承自 Selector） | 是否启用键盘文本搜索，输入字符自动定位匹配项 | 建议保持开启，长列表支持键盘快速定位，提升工业操作效率       |

### 2.2 继承的高频核心属性

全部继承自 `ItemsControl` 与 `Selector`，用法与 `ListBox` 完全一致：

| 分类     | 属性                                               | 作用                             |
| :------- | :------------------------------------------------- | :------------------------------- |
| 数据绑定 | `ItemsSource`                                      | 绑定下拉项数据源，MVVM 标准入口  |
| 显示与值 | `DisplayMemberPath` / `SelectedValuePath`          | 指定显示字段 / 选中值字段        |
| 选中同步 | `SelectedItem` / `SelectedIndex` / `SelectedValue` | 单选绑定核心，联动 ViewModel     |
| 呈现定制 | `ItemTemplate` / `ItemContainerStyle`              | 自定义下拉项外观与样式           |
| 性能优化 | `ItemsPanel`                                       | 替换布局面板，开启 UI 虚拟化     |
| 交替行   | `AlternationCount`                                 | 奇偶行交替背景，提升长列表可读性 |

------

## 三、核心事件与方法

### 3.1 核心事件

| 事件               | 触发时机                            | 典型工业用法                                                 |
| :----------------- | :---------------------------------- | :----------------------------------------------------------- |
| `DropDownOpened`   | 下拉面板完全展开后触发              | 延迟加载下拉数据、异步加载字典项，避免界面启动时加载大量数据 |
| `DropDownClosed`   | 下拉面板收起后触发                  | 校验输入内容合法性、同步选中值到业务模型                     |
| `SelectionChanged` | 选中项变化时触发（继承自 Selector） | 主从联动、级联下拉的核心触发点                               |
| `TextChanged`      | 可编辑模式下，文本框内容变化时触发  | 实时筛选下拉项、模糊匹配、输入校验                           |

### 3.2 核心方法

#### 公共方法

- 继承自基类的通用方法，如 `Focus()`、`FindResource()` 等；
- 无新增的高频公共方法，所有交互主要通过属性与事件驱动。

#### 受保护扩展方法（自定义子类核心）

| 方法                                         | 官方作用                                 | 扩展意义                                                |
| :------------------------------------------- | :--------------------------------------- | :------------------------------------------------------ |
| `GetContainerForItemOverride()`              | 返回 `new ComboBoxItem()` 作为下拉项容器 | 自定义下拉容器类型时重写                                |
| `IsItemItsOwnContainerOverride(object item)` | 判断项是否本身就是 `ComboBoxItem`        | 支持 XAML 直接添加静态下拉项                            |
| `PrepareContainerForItemOverride(...)`       | 容器生成 / 复用时准备数据与样式          | 注入自定义行样式、状态标记                              |
| `OnDropDownOpened(EventArgs e)`              | 下拉展开时的虚方法入口                   | 子类扩展下拉展开逻辑，如数据懒加载                      |
| `OnDropDownClosed(EventArgs e)`              | 下拉收起时的虚方法入口                   | 子类扩展输入校验、状态同步                              |
| `OnSelectionChanged(...)`                    | 选中项变更处理                           | 同步 `Text` 属性与选择框显示内容                        |
| `OnKeyDown(...)`                             | 键盘交互处理                             | 支持上下键切换选项、回车确认、Alt+↓展开下拉等标准快捷键 |

------

## 四、配套条目容器：`ComboBoxItem`

`ComboBoxItem` 是下拉列表的默认条目容器，继承自 `ContentControl`，是选中状态的 UI 载体。

### 精简类定义

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

### 核心说明

1. **`IsSelected` 属性**：标记当前项是否选中，本质是 `Selector.IsSelected` 附加属性的强类型包装；
2. **交互逻辑**：鼠标左键点击时触发选中，自动收起下拉面板；
3. **样式触发器**：支持 `IsMouseOver`、`IsSelected` 等触发器，可自定义悬浮、选中样式。

------

## 五、核心工作机制

### 5.1 下拉弹出机制

- 内部通过 `Popup` 控件承载下拉内容，默认向控件下方弹出；当屏幕下方空间不足时，自动向上弹出；
- 下拉面板的核心是一个**内嵌的单选 `ListBox`**，因此天然继承了 `ListBox` 的全部特性：项模板、键盘导航、UI 虚拟化、文本搜索等；
- 选中某项后，`Popup` 自动关闭，选择框同步显示选中项的内容。

### 5.2 两种交互模式

#### 1. 普通选择模式（`IsEditable=false`，默认）

- 顶部选择框为 `ContentPresenter`，仅用于显示选中项的内容，不可输入；
- 点击控件展开下拉，选择后自动收起，只能从给定选项中选择，不能自定义输入。
- **适用场景**：固定字典项、枚举值、分类选择等不允许自定义输入的场景。

#### 2. 可编辑模式（`IsEditable=true`）

- 顶部选择框替换为 `TextBox`，支持手动输入文本；
- 输入时自动匹配下拉项，匹配到对应项时自动选中；也可输入不在列表中的自定义值；
- 配合 `IsReadOnly=true` 可实现「可搜索、不可修改」：文本框看似可输入，实际只能通过选择改变内容，输入仅用于搜索定位。
- **适用场景**：长列表快速搜索、支持自定义值的参数输入、配方编号检索等。

### 5.3 文本搜索机制

- 基于 `TextSearch` 附加属性实现，默认匹配 `DisplayMemberPath` 对应的字段；
- 普通模式下，键盘输入字符自动跳转到第一个匹配项；
- 可编辑模式下，输入时自动补全匹配项的文本，同时高亮对应下拉项；
- 可通过 `TextSearch.TextPath` 附加属性自定义搜索匹配的字段。

### 5.4 UI 虚拟化支持

- 下拉面板内嵌 `ListBox`，因此支持和 `ListBox` 完全一致的 UI 虚拟化；
- 替换 `ItemsPanel` 为 `VirtualizingStackPanel` 即可开启，大数据量（>500 项）时，下拉展开速度提升数倍，内存占用大幅降低；
- 虚拟化模式下，选中状态持久化在数据层，滚动时不会丢失。

------

## 六、基础使用方法

### 6.1 静态下拉项（纯 XAML）

适合少量固定选项的场景，直接在 XAML 中添加子元素。

xaml:

```xaml
<ComboBox Width="180" SelectedIndex="0">
    <ComboBoxItem Content="温度参数"/>
    <ComboBoxItem Content="压力参数"/>
    <ComboBoxItem Content="速度参数"/>
    <ComboBoxItem Content="时间参数"/>
</ComboBox>
```

### 6.2 MVVM 单选绑定

工业场景标准用法，绑定业务集合，通过 `SelectedItem` 同步选中值。

xaml:

```xaml
<ComboBox Width="200"
          ItemsSource="{Binding DeviceTypeList}"
          SelectedItem="{Binding SelectedDeviceType}"
          DisplayMemberPath="TypeName"
          SelectedValuePath="TypeCode"/>
```

### 6.3 开启可编辑模式

支持输入搜索，适合长列表快速定位。

xaml:

```xaml
<ComboBox Width="200"
          IsEditable="True"
          IsTextSearchEnabled="True"
          StaysOpenOnEdit="True"
          ItemsSource="{Binding RecipeList}"
          DisplayMemberPath="RecipeCode"
          Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"/>
```

### 6.4 开启下拉虚拟化

大数据量下拉必配，提升展开速度与滚动流畅度。

xaml:

```xaml
<ComboBox ItemsSource="{Binding AllDeviceList}"
          DisplayMemberPath="DeviceName"
          MaxDropDownHeight="300">
    <ComboBox.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel VirtualizationMode="Recycling"/>
        </ItemsPanelTemplate>
    </ComboBox.ItemsPanel>
</ComboBox>
```

------

## 七、工业场景实战实例

### 实例 1：设备类型选择下拉（基础 MVVM 绑定）

#### 场景说明

设备新增 / 编辑界面，选择设备所属类型，绑定系统字典数据，选中后联动加载对应参数模板，是工业参数配置的基础场景。

#### 1. 数据模型

csharp:

```c#
public class DeviceType : INotifyPropertyChanged
{
    public string TypeCode { get; set; }
    public string TypeName { get; set; }
    public string Description { get; set; }

    // INotifyPropertyChanged 实现略
}
```

#### 2. ViewModel

csharp:

```c#
public class DeviceEditViewModel : INotifyPropertyChanged
{
    public ObservableCollection<DeviceType> DeviceTypeList { get; set; }
    
    private DeviceType _selectedType;
    public DeviceType SelectedType
    {
        get => _selectedType;
        set
        {
            _selectedType = value;
            OnPropertyChanged();
            // 选中类型后，加载对应的参数模板
            LoadParamTemplate(value);
        }
    }

    public DeviceEditViewModel()
    {
        // 模拟加载字典数据
        DeviceTypeList = new ObservableCollection<DeviceType>
        {
            new DeviceType { TypeCode = "ROBOT", TypeName = "工业机器人", Description = "多关节喷涂机器人" },
            new DeviceType { TypeCode = "OVEN", TypeName = "固化炉", Description = "热风循环固化设备" },
            new DeviceType { TypeCode = "CONVEYOR", TypeName = "输送线", Description = "皮带输送设备" },
        };
    }

    private void LoadParamTemplate(DeviceType type)
    {
        // 根据设备类型加载对应参数模板
    }

    // INotifyPropertyChanged 实现略
}
```

#### 3. XAML 界面

xaml:

```xaml
<StackPanel Width="250" Margin="10">
    <TextBlock Text="设备类型" Margin="0 0 0 4"/>
    <ComboBox ItemsSource="{Binding DeviceTypeList}"
              SelectedItem="{Binding SelectedType}"
              DisplayMemberPath="TypeName"
              SelectedValuePath="TypeCode"/>
</StackPanel>
```

------

### 实例 2：可编辑配方编号下拉（实时筛选）

#### 场景说明

配方查询界面，支持手动输入配方编号快速搜索，下拉实时过滤匹配项，边输入边展示结果，提升长列表检索效率。

#### 核心代码

xaml:

```c#
<ComboBox Width="220"
          IsEditable="True"
          IsTextSearchEnabled="True"
          StaysOpenOnEdit="True"
          ItemsSource="{Binding FilteredRecipeList}"
          DisplayMemberPath="RecipeCode"
          Text="{Binding SearchKeyword, UpdateSourceTrigger=PropertyChanged}"
          DropDownOpened="ComboBox_DropDownOpened">
    <ComboBox.ItemTemplate>
        <DataTemplate>
            <StackPanel>
                <TextBlock Text="{Binding RecipeCode}" FontWeight="SemiBold"/>
                <TextBlock Text="{Binding RecipeName}" FontSize="11" Foreground="#666"/>
            </StackPanel>
        </DataTemplate>
    </ComboBox.ItemTemplate>
</ComboBox>
```

csharp:

```c#
private string _searchKeyword;
public string SearchKeyword
{
    get => _searchKeyword;
    set
    {
        _searchKeyword = value;
        OnPropertyChanged();
        // 实时过滤配方列表
        FilterRecipeList(value);
    }
}

private void FilterRecipeList(string keyword)
{
    if (string.IsNullOrWhiteSpace(keyword))
    {
        FilteredRecipeList = new ObservableCollection<RecipeInfo>(AllRecipeList);
        return;
    }
    
    var filtered = AllRecipeList.Where(r => r.RecipeCode.Contains(keyword) || r.RecipeName.Contains(keyword));
    FilteredRecipeList = new ObservableCollection<RecipeInfo>(filtered);
}
```

------

### 实例 3：自定义下拉项模板（带状态的设备选择）

#### 场景说明

选择设备时，下拉项显示设备编号、名称、运行状态指示灯，直观展示设备在线状态，避免选择离线设备。

xaml:

```xaml
<ComboBox Width="220"
          ItemsSource="{Binding DeviceList}"
          SelectedItem="{Binding SelectedDevice}"
          DisplayMemberPath="DeviceName"
          MaxDropDownHeight="300">
    
    <!-- 下拉项自定义模板 -->
    <ComboBox.ItemTemplate>
        <DataTemplate>
            <DockPanel Height="32" Margin="2">
                <!-- 状态指示灯 -->
                <Ellipse DockPanel.Dock="Left" Width="10" Height="10" 
                         VerticalAlignment="Center" Margin="0 0 8 0">
                    <Ellipse.Style>
                        <Style TargetType="Ellipse">
                            <Setter Property="Fill" Value="Gray"/>
                            <Style.Triggers>
                                <DataTrigger Binding="{Binding IsOnline}" Value="True">
                                    <Setter Property="Fill" Value="#52C41A"/>
                                </DataTrigger>
                                <DataTrigger Binding="{Binding IsAlarm}" Value="True">
                                    <Setter Property="Fill" Value="#F5222D"/>
                                </DataTrigger>
                            </Style.Triggers>
                        </Style>
                    </Ellipse.Style>
                </Ellipse>
                
                <StackPanel>
                    <TextBlock Text="{Binding DeviceName}" FontWeight="SemiBold"/>
                    <TextBlock Text="{Binding DeviceCode}" FontSize="11" Foreground="#666"/>
                </StackPanel>
            </DockPanel>
        </DataTemplate>
    </ComboBox.ItemTemplate>

    <!-- 下拉项容器样式 -->
    <ComboBox.ItemContainerStyle>
        <Style TargetType="ComboBoxItem">
            <Setter Property="Padding" Value="6 2"/>
            <Style.Triggers>
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#E6F4FF"/>
                </Trigger>
                <Trigger Property="IsSelected" Value="True">
                    <Setter Property="Background" Value="#BAE0FF"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </ComboBox.ItemContainerStyle>
</ComboBox>
```

> 💡 说明：`DisplayMemberPath` 用于收起时选择框的显示内容；下拉项的复杂内容由 `ItemTemplate` 控制，二者各司其职。

------

### 实例 4：千级设备下拉高性能方案（UI 虚拟化）

#### 场景说明

全厂上千台设备的下拉选择，开启 UI 虚拟化保证下拉秒开、滚动流畅，避免大数据量下的卡顿与内存暴涨。

xaml:

```xaml
<ComboBox Width="220"
          ItemsSource="{Binding AllDeviceList}"
          DisplayMemberPath="DeviceFullName"
          MaxDropDownHeight="350"
          VirtualizingStackPanel.IsVirtualizing="True"
          VirtualizingStackPanel.VirtualizationMode="Recycling">
    
    <!-- 替换为虚拟化布局面板 -->
    <ComboBox.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel/>
        </ItemsPanelTemplate>
    </ComboBox.ItemsPanel>
</ComboBox>
```

#### 性能效果

- 开启虚拟化后，1000 条数据下拉展开速度从 1~2 秒缩短到毫秒级；
- 内存占用仅为全量渲染的 10% 左右，滚动流畅无卡顿；
- 回收模式下，滚动时容器复用，大幅减少 GC 压力。

------

### 实例 5：车间 - 设备二级级联下拉

#### 场景说明

先选择车间，自动加载对应车间下的设备列表，是工业数据筛选最常用的二级联动模式。

#### 1. XAML 界面

xaml:

```xaml
<StackPanel Orientation="Horizontal" Margin="10">
    <StackPanel Width="180" Margin="0 0 15 0">
        <TextBlock Text="所属车间" Margin="0 0 0 4"/>
        <ComboBox ItemsSource="{Binding WorkshopList}"
                  SelectedItem="{Binding SelectedWorkshop}"
                  DisplayMemberPath="WorkshopName"/>
    </StackPanel>
    
    <StackPanel Width="200">
        <TextBlock Text="选择设备" Margin="0 0 0 4"/>
        <ComboBox ItemsSource="{Binding FilteredDeviceList}"
                  SelectedItem="{Binding SelectedDevice}"
                  DisplayMemberPath="DeviceName"/>
    </StackPanel>
</StackPanel>
```

#### 2. ViewModel 联动逻辑

csharp:

```c#
public ObservableCollection<Workshop> WorkshopList { get; set; }
public ObservableCollection<DeviceInfo> FilteredDeviceList { get; set; }
public List<DeviceInfo> AllDeviceList { get; set; }

private Workshop _selectedWorkshop;
public Workshop SelectedWorkshop
{
    get => _selectedWorkshop;
    set
    {
        _selectedWorkshop = value;
        OnPropertyChanged();
        // 车间变化后，过滤对应设备
        FilterDevicesByWorkshop(value);
    }
}

private void FilterDevicesByWorkshop(Workshop workshop)
{
    if (workshop == null)
    {
        FilteredDeviceList = new ObservableCollection<DeviceInfo>();
        return;
    }
    
    var devices = AllDeviceList.Where(d => d.WorkshopCode == workshop.WorkshopCode);
    FilteredDeviceList = new ObservableCollection<DeviceInfo>(devices);
}
```

------

## 八、最佳实践与常见坑点

### 8.1 工业场景最佳实践

1. **大数据量必开虚拟化**
   - 下拉项 > 500 条时，必须替换 `ItemsPanel` 为 `VirtualizingStackPanel` 并开启回收模式；
   - 配合 `MaxDropDownHeight` 限制下拉高度，保证滚动性能与操作体验。
2. **延迟加载下拉数据**
   - 字典项、设备列表等大量数据，不要在页面初始化时全量加载；
   - 在 `DropDownOpened` 事件中异步加载，提升界面启动速度。
3. **可编辑模式做好输入校验**
   - 允许自定义输入时，在 `DropDownClosed` 事件中校验输入值合法性；
   - 仅允许选择列表项时，设置 `IsReadOnly=true`，避免非法输入。
4. **长列表开启文本搜索**
   - 保持 `IsTextSearchEnabled="True"`，支持键盘快速定位，提升工控机无鼠标场景的操作效率。
5. **简化下拉项模板**
   - 下拉项尽量减少视觉树层级，避免嵌套复杂布局；
   - 过于复杂的项模板会导致下拉展开慢、滚动卡顿。

### 8.2 常见坑点

1. **可编辑模式下 `Text` 与 `SelectedItem` 不同步**
   - 现象：输入文本后 `SelectedItem` 为 null，或选中项后 `Text` 显示异常；
   - 原因：输入值不在数据源中时，无法自动匹配到对应项；
   - 解决方案：`TextChanged` 事件中手动匹配，或限制只能选择列表中的值。
2. **虚拟化下选中项不自动滚动到可视区**
   - 现象：设置 `SelectedItem` 后，展开下拉看不到选中项；
   - 原因：虚拟化模式下不可见项没有生成容器，默认不自动滚动；
   - 解决方案：在 `DropDownOpened` 事件中调用内嵌 ListBox 的 `ScrollIntoView` 方法。
3. **下拉弹出位置异常、被遮挡**
   - 现象：靠近屏幕边缘时下拉错位、被其他窗口遮挡；
   - 原因：`Popup` 默认放置策略限制，或自定义模板破坏了布局计算；
   - 解决方案：尽量使用默认控件模板，必要时调整 `Popup.Placement` 属性。
4. **项模板太复杂导致下拉展开卡顿**
   - 现象：下拉展开慢、滚动掉帧；
   - 原因：每个下拉项嵌套多层布局控件，渲染开销大；
   - 解决方案：简化项模板，减少视觉树层级；大数据量必须开启虚拟化。

------

## 总结

`ComboBox` 是 WPF 单选交互的「空间友好型」控件，通过折叠式下拉面板在有限的界面空间内提供完整的选择能力，同时支持可编辑输入模式兼顾检索效率。它的下拉内核复用了 `ListBox` 的成熟能力，天然支持项模板定制、UI 虚拟化、文本搜索，既能满足简单字典选择，也能支撑工业场景下千级数据的高性能下拉交互。