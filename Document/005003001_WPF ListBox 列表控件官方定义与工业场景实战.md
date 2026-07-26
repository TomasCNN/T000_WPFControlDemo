# 005003001_WPF `ListBox` 列表控件官方定义与工业场景实战

`ListBox` 是 WPF 最经典的列表选择控件，是 `Selector` 选择基类的**标准完整实现**，在 `ItemsControl` 集合呈现 + `Selector` 选中管理的基础上，扩展了**多选模式、标准条目容器、键盘导航、文本搜索**等完整列表交互能力。

它是工业软件中使用频率最高的控件之一，广泛应用于报警列表、设备台账、配方管理、生产记录查询等场景，兼具灵活性与稳定性。

------

## 一、官方类定义总览与核心元数据

### 1.1 完整类签名（官方原生定义）

csharp:

```c#
namespace System.Windows.Controls
{
    [System.Windows.LocalizabilityAttribute(System.Windows.LocalizationCategory.None, Readability = System.Windows.Readability.Unreadable)]
    [System.Windows.StyleTypedPropertyAttribute(Property = "ItemContainerStyle", StyleTargetType = typeof(System.Windows.Controls.ListBoxItem))]
    public class ListBox : System.Windows.Controls.Primitives.Selector
    {
        // 静态依赖属性
        public static readonly System.Windows.DependencyProperty SelectionModeProperty;
        public static readonly System.Windows.DependencyProperty SelectedItemsProperty;

        // 构造函数
        public ListBox();

        // 公共属性
        public System.Windows.Controls.SelectionMode SelectionMode { get; set; }
        public System.Collections.IList SelectedItems { get; }

        // 受保护重写方法
        protected override System.Windows.DependencyObject GetContainerForItemOverride();
        protected override bool IsItemItsOwnContainerOverride(object item);
        protected override void PrepareContainerForItemOverride(System.Windows.DependencyObject element, object item);
        protected override void ClearContainerForItemOverride(System.Windows.DependencyObject element, object item);
        protected override void OnSelectionChanged(System.Windows.Controls.SelectionChangedEventArgs e);
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e);
    }
}
```

### 1.2 核心元数据

| 项               | 官方精确值                                                   | 工业场景说明                                    |
| :--------------- | :----------------------------------------------------------- | :---------------------------------------------- |
| **命名空间**     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                            |
| **程序集**       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                              |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → Selector → ListBox` | 继承完整的集合呈现与选择能力                    |
| **条目容器**     | `ListBoxItem`                                                | 每个条目的默认容器控件，继承自 `ContentControl` |
| **默认布局面板** | `StackPanel`（垂直排列）                                     | 可通过 `ItemsPanel` 替换为虚拟化面板            |
| **默认选择模式** | `Single`（单选）                                             | 可配置为多选模式                                |
| **工业核心场景** | 设备列表、报警记录、配方选择、批量操作、数据筛选             | 所有「列表选择 + 业务操作」场景的首选控件       |

### 1.3 类级特性说明

1. **`[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ListBoxItem))]`**
   - 声明 `ItemContainerStyle` 的目标类型为 `ListBoxItem`，设计器可正确识别样式属性；
   - 相比基类 `Selector` 的 `FrameworkElement`，进一步明确了容器类型。
2. **`[Localizability]`**：控件本身无本地化文本，内容由业务数据决定。

------

## 二、核心依赖属性全解析

`ListBox` 在继承 `ItemsControl` / `Selector` 所有属性的基础上，新增 2 个核心依赖属性，专门用于多选能力。

### 2.1 新增核心属性

| 属性字段                | 包装属性        | 类型                 | 默认值   | 官方作用                                 | 工业最佳实践                                            |
| :---------------------- | :-------------- | :------------------- | :------- | :--------------------------------------- | :------------------------------------------------------ |
| `SelectionModeProperty` | `SelectionMode` | `SelectionMode` 枚举 | `Single` | 控制选择模式：单选 / 简单多选 / 扩展多选 | 批量操作场景设为 `Extended`，支持 Ctrl 点选、Shift 连选 |
| `SelectedItemsProperty` | `SelectedItems` | `IList`（只读）      | 空集合   | 多选模式下所有选中项的集合               | 批量确认、批量导出、批量删除的核心数据源                |

#### SelectionMode 枚举详解

| 枚举值           | 行为说明                                          | 典型场景                                     |
| :--------------- | :------------------------------------------------ | :------------------------------------------- |
| `Single`（默认） | 单选，同一时间只能选中一项                        | 主从详情、下拉选择类场景                     |
| `Multiple`       | 简单多选，单击直接切换选中状态，无需按住 Ctrl     | 快速批量勾选场景                             |
| `Extended`       | 扩展多选，按住 Ctrl 点选多个，按住 Shift 连续选择 | 工业数据批量操作的标准模式，符合桌面操作习惯 |

> ⚠️ 重要注意：`SelectedItems` 是**只读依赖属性**，不能直接绑定到 ViewModel 的集合属性，MVVM 多选场景需要通过附加属性、行为或事件回调实现同步。

### 2.2 继承的核心属性（高频使用）

| 分类     | 属性                                                         | 作用                                  |
| :------- | :----------------------------------------------------------- | :------------------------------------ |
| 数据集合 | `ItemsSource`                                                | 绑定业务数据集合，MVVM 标准入口       |
| 呈现     | `ItemTemplate` / `DisplayMemberPath`                         | 自定义条目内容外观 / 简化显示单个字段 |
| 选中     | `SelectedItem` / `SelectedIndex` / `SelectedValue` / `SelectedValuePath` | 单选模式下的选中数据绑定              |
| 样式     | `ItemContainerStyle`                                         | 自定义 `ListBoxItem` 容器样式         |
| 布局     | `ItemsPanel`                                                 | 替换布局面板，开启 UI 虚拟化          |
| 交互     | `IsTextSearchEnabled`                                        | 开启键盘文本搜索，输入字符快速定位    |
| 交替行   | `AlternationCount`                                           | 奇偶行交替样式                        |

------

## 三、核心事件与方法

### 3.1 核心事件

- **`SelectionChanged`**：继承自 `Selector`，选中项变化时触发；
  - 事件参数 `SelectionChangedEventArgs` 包含 `AddedItems`（新增选中）和 `RemovedItems`（取消选中）；
  - 单选模式下各最多 1 项，多选模式下可包含多项。

### 3.2 核心重写方法

`ListBox` 重写了 `Selector` 的容器生命周期方法，将默认容器替换为 `ListBoxItem`，并实现了多选交互逻辑。

| 方法                                         | 官方实现逻辑                                   | 扩展意义                                |
| :------------------------------------------- | :--------------------------------------------- | :-------------------------------------- |
| `GetContainerForItemOverride()`              | 返回 `new ListBoxItem()` 作为条目容器          | 明确使用 `ListBoxItem` 作为默认容器     |
| `IsItemItsOwnContainerOverride(object item)` | 判断 item 是否为 `ListBoxItem` 类型            | 支持直接添加 `ListBoxItem` 控件作为条目 |
| `PrepareContainerForItemOverride(...)`       | 基类逻辑基础上，同步多选状态、绑定容器属性     | 容器复用时恢复选中状态                  |
| `ClearContainerForItemOverride(...)`         | 回收容器时清理选中状态、解绑事件               | 适配 UI 虚拟化，避免状态错乱与内存泄漏  |
| `OnSelectionChanged(...)`                    | 触发事件，更新 `SelectedItems` 集合            | 多选模式下维护选中集合的一致性          |
| `OnKeyDown(...)`                             | 处理方向键导航、空格选中、Shift 连选等键盘逻辑 | 完整的键盘交互支持                      |

------

## 四、配套容器：`ListBoxItem` 详解

`ListBoxItem` 是 `ListBox` 的默认条目容器，继承自 `ContentControl`，是选中状态的 UI 载体。

### 核心属性

- `IsSelected`：bool，当前条目是否选中，本质是 `Selector.IsSelected` 附加属性的强类型包装；
- 继承 `ContentControl` 的 `Content` / `ContentTemplate` 属性，承载条目内容。

### 样式常用触发器

- `IsSelected`：选中态样式；
- `IsMouseOver`：鼠标悬浮样式；
- `Selector.IsSelectionActive`：控件激活 / 失焦的选中态区分。

------

## 五、基础使用方法

### 5.1 静态条目（纯 XAML）

适合少量固定选项的场景，直接在 XAML 中添加子元素，自动加入 `Items` 集合。

xaml:

```xaml
<ListBox Width="200" SelectionMode="Single">
    <ListBoxItem Content="设备状态监控"/>
    <ListBoxItem Content="报警记录查询"/>
    <ListBoxItem Content="生产配方管理"/>
    <ListBoxItem Content="系统参数设置"/>
</ListBox>
```

### 5.2 MVVM 数据绑定（单选）

工业场景标准用法，绑定 `ObservableCollection<T>` 数据源，通过 `SelectedItem` 同步选中项。

xaml:

```xaml
<ListBox ItemsSource="{Binding DeviceList}"
         SelectedItem="{Binding SelectedDevice}"
         DisplayMemberPath="DeviceName"
         Width="220"/>
```

### 5.3 开启多选模式

设置 `SelectionMode="Extended"` 启用标准多选，通过 `SelectedItems` 获取选中结果。

xaml:

```xaml
<ListBox ItemsSource="{Binding AlarmList}"
         SelectionMode="Extended"
         x:Name="AlarmListBox">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Message}"/>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

### 5.4 开启 UI 虚拟化

大数据量列表必须替换 `ItemsPanel` 为 `VirtualizingStackPanel`，大幅降低内存占用、提升滚动流畅度。

xaml:

```xaml
<ListBox ItemsSource="{Binding HistoryAlarmList}"
         VirtualizingStackPanel.IsVirtualizing="True"
         VirtualizingStackPanel.VirtualizationMode="Recycling">
    <ListBox.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel/>
        </ItemsPanelTemplate>
    </ListBox.ItemsPanel>
</ListBox>
```

------

## 六、工业场景实战实例

### 实例 1：设备监控主从视图（单选 + 详情联动）

#### 场景说明

左侧设备列表，选中设备后右侧显示实时参数，是工业监控系统的经典布局。

#### 1. 数据模型

csharp:

```c#
public class DeviceInfo : INotifyPropertyChanged
{
    public string DeviceCode { get; set; }
    public string DeviceName { get; set; }
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
public class DeviceMonitorViewModel : INotifyPropertyChanged
{
    public ObservableCollection<DeviceInfo> DeviceList { get; set; }
    
    private DeviceInfo _selectedDevice;
    public DeviceInfo SelectedDevice
    {
        get => _selectedDevice;
        set { _selectedDevice = value; OnPropertyChanged(); }
    }

    public DeviceMonitorViewModel()
    {
        DeviceList = new ObservableCollection<DeviceInfo>
        {
            new DeviceInfo { DeviceCode = "PLC-001", DeviceName = "喷涂机器人A1", Temperature = 42.5, IsRunning = true },
            new DeviceInfo { DeviceCode = "PLC-002", DeviceName = "喷涂机器人A2", Temperature = 45.1, IsRunning = true },
            new DeviceInfo { DeviceCode = "PLC-003", DeviceName = "固化炉B1", Temperature = 85.3, IsRunning = false },
        };
        SelectedDevice = DeviceList.First();
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
    <local:DeviceMonitorViewModel/>
</Window.DataContext>

<Grid Margin="10">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="240"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>

    <!-- 左侧设备列表 -->
    <ListBox Grid.Column="0"
             ItemsSource="{Binding DeviceList}"
             SelectedItem="{Binding SelectedDevice}"
             BorderBrush="#DDD" BorderThickness="1">
        <ListBox.ItemTemplate>
            <DataTemplate>
                <DockPanel Height="40" Margin="2">
                    <Ellipse DockPanel.Dock="Left" Width="10" Height="10" VerticalAlignment="Center" Margin="0 0 8 0">
                        <Ellipse.Style>
                            <Style TargetType="Ellipse">
                                <Setter Property="Fill" Value="Gray"/>
                                <Style.Triggers>
                                    <DataTrigger Binding="{Binding IsRunning}" Value="True">
                                        <Setter Property="Fill" Value="LimeGreen"/>
                                    </DataTrigger>
                                </Style.Triggers>
                            </Style>
                        </Ellipse.Style>
                    </Ellipse>
                    <StackPanel>
                        <TextBlock Text="{Binding DeviceName}" FontWeight="SemiBold"/>
                        <TextBlock Text="{Binding StatusText}" FontSize="11" Foreground="#666"/>
                    </StackPanel>
                </DockPanel>
            </DataTemplate>
        </ListBox.ItemTemplate>
    </ListBox>

    <!-- 右侧设备详情 -->
    <Border Grid.Column="1" Margin="10 0 0 0" Background="#F8F9FA" Padding="20" BorderBrush="#DDD" BorderThickness="1">
        <StackPanel DataContext="{Binding SelectedDevice}">
            <TextBlock FontSize="20" FontWeight="Bold" Text="{Binding DeviceName}"/>
            <TextBlock Margin="0 5" Text="{Binding DeviceCode, StringFormat=设备编号：{0}}"/>
            <Separator Margin="0 10"/>
            <TextBlock FontSize="14" Text="{Binding Temperature, StringFormat=当前温度：{0:F1}℃}"/>
            <TextBlock Margin="0 8" FontSize="14" Text="{Binding StatusText, StringFormat=运行状态：{0}}"/>
        </StackPanel>
    </Border>
</Grid>
```

------

### 实例 2：报警批量确认（多选 + 批量操作）

#### 场景说明

报警列表支持多选，批量执行确认、导出、删除操作，是工业报警系统的标配功能。

#### 1. XAML 界面

xaml:

```xaml
<Grid Margin="10">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>

    <!-- 顶部操作栏 -->
    <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0 0 0 8">
        <Button Content="批量确认" Click="BatchConfirm_Click" Margin="0 0 8 0" Padding="12 4"/>
        <Button Content="导出选中" Click="ExportSelected_Click" Padding="12 4"/>
        <TextBlock Margin="20 0 0 0" VerticalAlignment="Center">
            <Run>已选中：</Run>
            <Run Text="{Binding SelectedCount, StringFormat={}{0} 条}"/>
        </TextBlock>
    </StackPanel>

    <!-- 报警列表 -->
    <ListBox Grid.Row="1"
             ItemsSource="{Binding AlarmList}"
             SelectionMode="Extended"
             x:Name="AlarmListBox"
             SelectionChanged="AlarmListBox_SelectionChanged"
             BorderBrush="#DDD" BorderThickness="1">
        <ListBox.ItemTemplate>
            <DataTemplate>
                <Grid Height="32">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="140"/>
                        <ColumnDefinition Width="80"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>
                    <TextBlock Text="{Binding AlarmTime, StringFormat=yyyy-MM-dd HH:mm:ss}" VerticalAlignment="Center"/>
                    <TextBlock Grid.Column="1" Text="{Binding Level}">
                        <TextBlock.Style>
                            <Style TargetType="TextBlock">
                                <Setter Property="Foreground" Value="Orange"/>
                                <Style.Triggers>
                                    <DataTrigger Binding="{Binding Level}" Value="严重">
                                        <Setter Property="Foreground" Value="Red"/>
                                    </DataTrigger>
                                </Style.Triggers>
                            </Style>
                        </TextBlock.Style>
                    </TextBlock>
                    <TextBlock Grid.Column="2" Text="{Binding Message}" VerticalAlignment="Center"/>
                </Grid>
            </DataTemplate>
        </ListBox.ItemTemplate>
    </ListBox>
</Grid>
```

#### 2. 后台交互逻辑

csharp:

```c#
public int SelectedCount { get; set; }

private void AlarmListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    SelectedCount = AlarmListBox.SelectedItems.Count;
    // 通知UI更新
    OnPropertyChanged(nameof(SelectedCount));
}

private void BatchConfirm_Click(object sender, RoutedEventArgs e)
{
    var selectedAlarms = AlarmListBox.SelectedItems.Cast<AlarmRecord>().ToList();
    if (selectedAlarms.Count == 0)
    {
        MessageBox.Show("请先选择要确认的报警记录");
        return;
    }
    
    // 批量确认业务逻辑
    foreach (var alarm in selectedAlarms)
    {
        alarm.IsConfirmed = true;
    }
    
    MessageBox.Show($"已成功确认 {selectedAlarms.Count} 条报警");
}

private void ExportSelected_Click(object sender, RoutedEventArgs e)
{
    var selectedAlarms = AlarmListBox.SelectedItems.Cast<AlarmRecord>().ToList();
    // 导出Excel逻辑...
}
```

> 💡 MVVM 多选提示：纯 MVVM 场景下，可通过自定义附加属性将 `SelectedItems` 同步到 ViewModel 集合，避免在后台代码中操作控件。

------

### 实例 3：工业深色主题自定义样式

#### 场景说明

适配工业工控机深色主题，自定义选中态、悬浮态、交替行样式。

xaml:

```xaml
<ListBox ItemsSource="{Binding AlarmList}"
         AlternationCount="2"
         Background="#1E1E1E" Foreground="#E0E0E0"
         BorderBrush="#333" BorderThickness="1"
         Width="500" HorizontalAlignment="Left">
    
    <!-- 替换为虚拟化面板 -->
    <ListBox.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel VirtualizationMode="Recycling"/>
        </ItemsPanelTemplate>
    </ListBox.ItemsPanel>

    <!-- 条目容器样式 -->
    <ListBox.ItemContainerStyle>
        <Style TargetType="ListBoxItem">
            <Setter Property="Padding" Value="10 5"/>
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#E0E0E0"/>
            <Setter Property="BorderThickness" Value="0 0 0 1"/>
            <Setter Property="BorderBrush" Value="#2D2D2D"/>

            <Style.Triggers>
                <!-- 奇偶行交替 -->
                <Trigger Property="ItemsControl.AlternationIndex" Value="1">
                    <Setter Property="Background" Value="#252526"/>
                </Trigger>
                
                <!-- 鼠标悬浮 -->
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#2A2D2E"/>
                </Trigger>
                
                <!-- 选中+激活 -->
                <MultiTrigger>
                    <MultiTrigger.Conditions>
                        <Condition Property="IsSelected" Value="True"/>
                        <Condition Property="Selector.IsSelectionActive" Value="True"/>
                    </MultiTrigger.Conditions>
                    <Setter Property="Background" Value="#0E639C"/>
                    <Setter Property="Foreground" Value="White"/>
                </MultiTrigger>
                
                <!-- 选中+失焦 -->
                <MultiTrigger>
                    <MultiTrigger.Conditions>
                        <Condition Property="IsSelected" Value="True"/>
                        <Condition Property="Selector.IsSelectionActive" Value="False"/>
                    </MultiTrigger.Conditions>
                    <Setter Property="Background" Value="#3E3E42"/>
                    <Setter Property="Foreground" Value="White"/>
                </MultiTrigger>
            </Style.Triggers>
        </Style>
    </ListBox.ItemContainerStyle>

    <!-- 条目内容模板 -->
    <ListBox.ItemTemplate>
        <DataTemplate>
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="140"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                <TextBlock Text="{Binding AlarmTime, StringFormat=HH:mm:ss}"/>
                <TextBlock Grid.Column="1" Text="{Binding Message}"/>
            </Grid>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

------

## 七、最佳实践与常见坑点

### 7.1 工业场景最佳实践

1. **大数据量必开虚拟化**
   - 超过 500 条的列表，必须替换 `ItemsPanel` 为 `VirtualizingStackPanel` 并开启 `Recycling` 回收模式；
   - 万级数据下内存占用降低 90% 以上，滚动流畅度提升数倍。
2. **单选绑定 SelectedItem，多选操作 SelectedItems**
   - 单选场景优先绑定 `SelectedItem` 到 ViewModel，纯 MVVM 无后台代码；
   - 多选场景通过 `SelectedItems` 获取结果，复杂场景可使用附加属性实现 VM 绑定。
3. **样式与数据分离**
   - 选中、交替行、状态高亮全部通过 `ItemContainerStyle` 触发器实现，不要在数据模型中加背景色、选中态等 UI 属性。
4. **SelectionChanged 轻量处理**
   - 禁止在事件中执行 PLC 通信、数据库查询等耗时操作，必须异步执行，避免阻塞 UI 线程。
5. **长列表开启文本搜索**
   - 设置 `IsTextSearchEnabled="True"`，用户输入编号 / 名称可快速定位，提升工业操作效率。

### 7.2 常见坑点

1. **SelectedItems 无法直接绑定**
   - 现象：VM 中绑定 `SelectedItems` 报错或不生效；
   - 原因：该属性是只读依赖属性，不支持双向绑定；
   - 解决方案：使用附加属性、行为，或在 `SelectionChanged` 事件中同步到 VM。
2. **虚拟化下遍历容器为空**
   - 现象：通过索引获取 `ListBoxItem`，滚出屏幕后返回 null；
   - 原因：虚拟化模式下，不可见的条目没有生成容器；
   - 解决方案：永远操作数据层，不要直接操作 UI 容器。
3. **多选模式下 SelectedItem 不准确**
   - 现象：多选时 `SelectedItem` 只返回第一个选中项；
   - 原因：`SelectedItem` 是单选语义，多选场景请使用 `SelectedItems` 集合。
4. **自定义容器未清理状态**
   - 现象：滚动列表后出现选中状态错乱、颜色不对；
   - 原因：重写 `PrepareContainerForItemOverride` 后未对应重写 `ClearContainerForItemOverride` 清理状态；
   - 解决方案：成对实现准备与清理方法，虚拟化下必须完整清理自定义属性。

------

## 总结

`ListBox` 是 WPF 列表控件的「全能选手」，继承了 `ItemsControl` 的灵活呈现能力与 `Selector` 的完整选择机制，既能满足简单单选场景，也能支撑复杂多选批量操作，是工业软件中数据列表交互的首选控件。掌握其容器生命周期、虚拟化机制、样式定制方法，就能应对绝大多数工业数据展示与交互场景。