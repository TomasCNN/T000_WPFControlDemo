# 005003004_WPF `ListBox` 工业场景实战案例合集

以下案例全部贴合工业上位机、设备监控、生产管理等真实业务场景，从基础绑定到高级交互逐步深入，每个案例明确标注对应 `ListBox` 的核心特性，可直接复用至项目中。

------

## 案例 1：设备监控主从视图（MVVM 单选联动）

### 场景说明

左侧设备列表，选中某台设备后右侧自动加载实时参数与运行状态，是工业监控系统最经典的主从布局。

### 对应 ListBox 核心特性

- `ItemsSource` 数据绑定
- `SelectedItem` 双向绑定
- `ItemTemplate` 自定义条目外观
- 数据驱动选择，天然支持 UI 虚拟化

### 1. 数据模型

csharp:

```c#
public class DeviceInfo : INotifyPropertyChanged
{
    private string _deviceCode;
    public string DeviceCode
    {
        get => _deviceCode;
        set { _deviceCode = value; OnPropertyChanged(); }
    }

    private string _deviceName;
    public string DeviceName
    {
        get => _deviceName;
        set { _deviceName = value; OnPropertyChanged(); }
    }

    private double _temperature;
    public double Temperature
    {
        get => _temperature;
        set { _temperature = value; OnPropertyChanged(); }
    }

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set { _isRunning = value; OnPropertyChanged(); }
    }

    public string StatusText => IsRunning ? "运行中" : "待机";

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### 2. ViewModel

csharp:

```c#
public class DeviceMonitorViewModel : INotifyPropertyChanged
{
    public ObservableCollection<DeviceInfo> DeviceList { get; set; }

    private DeviceInfo _selectedDevice;
    public DeviceInfo SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            _selectedDevice = value;
            OnPropertyChanged();
            // 选中变化后可加载详情数据、订阅实时推送
            if (value != null) LoadDeviceDetail(value);
        }
    }

    public DeviceMonitorViewModel()
    {
        // 模拟初始化设备数据
        DeviceList = new ObservableCollection<DeviceInfo>
        {
            new DeviceInfo { DeviceCode = "PLC-001", DeviceName = "喷涂机器人A1", Temperature = 42.5, IsRunning = true },
            new DeviceInfo { DeviceCode = "PLC-002", DeviceName = "喷涂机器人A2", Temperature = 45.1, IsRunning = true },
            new DeviceInfo { DeviceCode = "PLC-003", DeviceName = "固化炉B1", Temperature = 85.3, IsRunning = false },
            new DeviceInfo { DeviceCode = "PLC-004", DeviceName = "上料机C1", Temperature = 36.2, IsRunning = true },
        };
        SelectedDevice = DeviceList.First();
    }

    private void LoadDeviceDetail(DeviceInfo device)
    {
        // 加载设备详情、参数、历史数据等业务逻辑
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### 3. XAML 界面

xaml:

```c#
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
            <DataTemplate DataType="{x:Type local:DeviceInfo}">
                <DockPanel Height="44" Margin="2">
                    <!-- 运行状态指示灯 -->
                    <Ellipse DockPanel.Dock="Left" Width="10" Height="10" 
                             VerticalAlignment="Center" Margin="0 0 8 0">
                        <Ellipse.Style>
                            <Style TargetType="Ellipse">
                                <Setter Property="Fill" Value="#999"/>
                                <Style.Triggers>
                                    <DataTrigger Binding="{Binding IsRunning}" Value="True">
                                        <Setter Property="Fill" Value="#52C41A"/>
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

    <!-- 右侧设备详情面板 -->
    <Border Grid.Column="1" Margin="10 0 0 0" 
            Background="#F8F9FA" Padding="20" 
            BorderBrush="#DDD" BorderThickness="1">
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

## 案例 2：报警批量确认（多选 + 批量操作）

### 场景说明

报警列表支持 Ctrl 点选、Shift 连选，批量执行确认、导出、删除操作，是工业报警系统的标配功能。

### 对应 ListBox 核心特性

- `SelectionMode="Extended"` 扩展多选模式
- `SelectedItems` 选中集合
- `SelectionChanged` 选中变更事件
- `SelectAll()` / `UnselectAll()` 批量操作方法

### 1. XAML 界面

xaml:

```xaml
<Grid Margin="10">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>

    <!-- 顶部操作栏 -->
    <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0 0 0 8">
        <Button Content="全选" Click="SelectAll_Click" Margin="0 0 8 0" Padding="12 4"/>
        <Button Content="取消全选" Click="UnselectAll_Click" Margin="0 0 8 0" Padding="12 4"/>
        <Button Content="批量确认" Click="BatchConfirm_Click" Margin="0 0 8 0" Padding="12 4"/>
        <Button Content="导出选中" Click="ExportSelected_Click" Padding="12 4"/>
        <TextBlock Margin="20 0 0 0" VerticalAlignment="Center" Foreground="#666">
            <Run>已选中：</Run>
            <Run x:Name="SelectedCountText">0</Run>
            <Run> 条</Run>
        </TextBlock>
    </StackPanel>

    <!-- 报警列表 -->
    <ListBox Grid.Row="1"
             x:Name="AlarmListBox"
             ItemsSource="{Binding AlarmList}"
             SelectionMode="Extended"
             SelectionChanged="AlarmListBox_SelectionChanged"
             BorderBrush="#DDD" BorderThickness="1">
        <ListBox.ItemTemplate>
            <DataTemplate>
                <Grid Height="32">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="150"/>
                        <ColumnDefinition Width="80"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>
                    <TextBlock Text="{Binding AlarmTime, StringFormat=yyyy-MM-dd HH:mm:ss}" VerticalAlignment="Center"/>
                    <TextBlock Grid.Column="1" Text="{Binding Level}">
                        <TextBlock.Style>
                            <Style TargetType="TextBlock">
                                <Setter Property="Foreground" Value="#FA8C16"/>
                                <Style.Triggers>
                                    <DataTrigger Binding="{Binding Level}" Value="严重">
                                        <Setter Property="Foreground" Value="#F5222D"/>
                                    </DataTrigger>
                                </Style.Triggers>
                            </Style>
                        </TextBlock.Style>
                    </TextBlock>
                    <TextBlock Grid.Column="2" Text="{Binding Message}" VerticalAlignment="Center" TextTrimming="CharacterEllipsis"/>
                </Grid>
            </DataTemplate>
        </ListBox.ItemTemplate>
    </ListBox>
</Grid>
```

### 2. 后台交互逻辑

csharp:

```c#
// 选中数量统计
private void AlarmListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    SelectedCountText.Text = AlarmListBox.SelectedItems.Count.ToString();
}

// 全选
private void SelectAll_Click(object sender, RoutedEventArgs e)
{
    if (AlarmListBox.SelectionMode == SelectionMode.Single) return;
    AlarmListBox.SelectAll();
}

// 取消全选
private void UnselectAll_Click(object sender, RoutedEventArgs e)
{
    AlarmListBox.UnselectAll();
}

// 批量确认
private void BatchConfirm_Click(object sender, RoutedEventArgs e)
{
    var selectedAlarms = AlarmListBox.SelectedItems.Cast<AlarmRecord>().ToList();
    if (selectedAlarms.Count == 0)
    {
        MessageBox.Show("请先选择要确认的报警记录", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        return;
    }

    // 批量确认业务逻辑
    foreach (var alarm in selectedAlarms)
    {
        alarm.IsConfirmed = true;
    }

    MessageBox.Show($"已成功确认 {selectedAlarms.Count} 条报警", "操作成功", MessageBoxButton.OK, MessageBoxImage.Information);
}

// 导出选中
private void ExportSelected_Click(object sender, RoutedEventArgs e)
{
    var selectedAlarms = AlarmListBox.SelectedItems.Cast<AlarmRecord>().ToList();
    // 导出 Excel / CSV 逻辑
}
```

> 💡 MVVM 多选方案：纯 MVVM 场景可通过自定义附加属性，监听 `SelectionChanged` 事件，将 `SelectedItems` 同步到 ViewModel 的集合属性，避免后台代码操作控件。

------

## 案例 3：工业深色主题自定义列表样式

### 场景说明

适配工控机深色操作界面，自定义选中态、悬浮态、交替行样式，区分激活 / 失焦选中效果。

### 对应 ListBox 核心特性

- `ItemContainerStyle` 容器样式定制
- `AlternationCount` 交替行
- `Selector.IsSelected` 选中触发器
- `Selector.IsSelectionActive` 焦点激活触发器

xaml:

```xaml
<ListBox ItemsSource="{Binding AlarmList}"
         AlternationCount="2"
         Background="#1E1E1E" Foreground="#E0E0E0"
         BorderBrush="#333" BorderThickness="1"
         Width="550" HorizontalAlignment="Left">
    
    <!-- 开启虚拟化回收模式 -->
    <ListBox.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel VirtualizationMode="Recycling"/>
        </ItemsPanelTemplate>
    </ListBox.ItemsPanel>

    <!-- 条目容器自定义样式 -->
    <ListBox.ItemContainerStyle>
        <Style TargetType="ListBoxItem">
            <Setter Property="Padding" Value="10 5"/>
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#E0E0E0"/>
            <Setter Property="BorderThickness" Value="0 0 0 1"/>
            <Setter Property="BorderBrush" Value="#2D2D2D"/>
            <Setter Property="FocusVisualStyle" Value="{x:Null}"/>

            <Style.Triggers>
                <!-- 奇偶行交替背景 -->
                <Trigger Property="ItemsControl.AlternationIndex" Value="1">
                    <Setter Property="Background" Value="#252526"/>
                </Trigger>
                
                <!-- 鼠标悬浮 -->
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#2A2D2E"/>
                </Trigger>
                
                <!-- 选中 + 控件有焦点：深蓝色高亮 -->
                <MultiTrigger>
                    <MultiTrigger.Conditions>
                        <Condition Property="IsSelected" Value="True"/>
                        <Condition Property="Selector.IsSelectionActive" Value="True"/>
                    </MultiTrigger.Conditions>
                    <Setter Property="Background" Value="#0E639C"/>
                    <Setter Property="Foreground" Value="White"/>
                </MultiTrigger>
                
                <!-- 选中 + 控件失焦：灰色标识，保留选中状态 -->
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
                    <ColumnDefinition Width="150"/>
                    <ColumnDefinition Width="70"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                <TextBlock Text="{Binding AlarmTime, StringFormat=HH:mm:ss}" Foreground="#999"/>
                <TextBlock Grid.Column="1" Text="{Binding Level}">
                    <TextBlock.Style>
                        <Style TargetType="TextBlock">
                            <Setter Property="Foreground" Value="#FA8C16"/>
                            <Style.Triggers>
                                <DataTrigger Binding="{Binding Level}" Value="严重">
                                    <Setter Property="Foreground" Value="#F5222D"/>
                                </DataTrigger>
                            </Style.Triggers>
                        </Style>
                    </TextBlock.Style>
                </TextBlock>
                <TextBlock Grid.Column="2" Text="{Binding Message}" TextTrimming="CharacterEllipsis"/>
            </Grid>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

------

## 案例 4：万级历史报警高性能列表（UI 虚拟化 + 滚动定位）

### 场景说明

上万条历史报警记录，流畅滚动无卡顿，支持搜索定位并自动滚动到匹配项。

### 对应 ListBox 核心特性

- `VirtualizingStackPanel` UI 虚拟化
- `VirtualizationMode="Recycling"` 容器回收
- `ScrollIntoView()` 滚动定位
- 内置 `ScrollViewer`，无需外层包裹

### 1. XAML 配置

xaml:

```xaml
<ListBox x:Name="HistoryAlarmList"
         ItemsSource="{Binding HistoryAlarmList}"
         VirtualizingStackPanel.IsVirtualizing="True"
         VirtualizingStackPanel.VirtualizationMode="Recycling"
         ScrollViewer.CanContentScroll="True"
         DisplayMemberPath="Message"
         BorderBrush="#DDD" BorderThickness="1"
         Height="500" Width="400"/>
```

### 2. 搜索定位代码

csharp:

```c#
private void SearchAndScroll(string keyword)
{
    var target = HistoryAlarmList.ItemsSource
        .Cast<AlarmRecord>()
        .FirstOrDefault(a => a.Message.Contains(keyword));

    if (target != null)
    {
        // 自动滚动到目标项
        HistoryAlarmList.ScrollIntoView(target);
        // 设置为选中项
        HistoryAlarmList.SelectedItem = target;
    }
}
```

### 性能说明

- 开启虚拟化后，无论数据是 1000 条还是 100000 条，内存中始终只生成可见区域的容器（约几十条）；
- 回收模式下，滚动时容器对象复用，大幅减少对象创建与 GC 压力；
- 工业长列表场景性能提升 10 倍以上，是必开的优化项。

------

## 案例 5：生产工序拖拽排序

### 场景说明

调整生产工序、配方步骤的执行顺序，通过鼠标拖拽上下移动位置。

### 对应 ListBox 核心特性

- 鼠标按下 / 移动 / 释放事件
- 内置滚动支持，拖拽到边缘自动滚动
- 数据驱动，操作数据源自动同步 UI

### 实现代码

xaml:

```xaml
<ListBox x:Name="ProcessList"
         ItemsSource="{Binding ProcessSteps}"
         DisplayMemberPath="StepName"
         AllowDrop="True"
         MouseMove="ProcessList_MouseMove"
         Drop="ProcessList_Drop"
         BorderBrush="#DDD" BorderThickness="1"
         Width="250" Height="350"/>
```

csharp:

```c#
private Point _startPoint;
private bool _isDragging;

// 鼠标移动时检测拖拽开始
private void ProcessList_MouseMove(object sender, MouseEventArgs e)
{
    if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
    {
        Point position = e.GetPosition(ProcessList);
        if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
        {
            _isDragging = true;
            var selectedItem = ProcessList.SelectedItem as ProcessStep;
            if (selectedItem != null)
            {
                DragDrop.DoDragDrop(ProcessList, selectedItem, DragDropEffects.Move);
            }
            _isDragging = false;
        }
    }
}

// 拖拽释放时调整顺序
private void ProcessList_Drop(object sender, DragEventArgs e)
{
    if (e.Data.GetDataPresent(typeof(ProcessStep)))
    {
        var draggedItem = e.Data.GetData(typeof(ProcessStep)) as ProcessStep;
        var targetPoint = e.GetPosition(ProcessList);
        var targetItem = GetItemAtPoint(targetPoint);

        if (draggedItem != null && targetItem != null && draggedItem != targetItem)
        {
            var sourceList = ProcessList.ItemsSource as ObservableCollection<ProcessStep>;
            int oldIndex = sourceList.IndexOf(draggedItem);
            int newIndex = sourceList.IndexOf(targetItem);
            
            // 移动数据项，UI自动同步
            sourceList.Move(oldIndex, newIndex);
        }
    }
}

// 根据坐标获取对应的数据项
private ProcessStep GetItemAtPoint(Point point)
{
    var element = VisualTreeHelper.HitTest(ProcessList, point)?.VisualHit;
    while (element != null && !(element is ListBoxItem))
    {
        element = VisualTreeHelper.GetParent(element) as DependencyObject;
    }
    return (element as ListBoxItem)?.DataContext as ProcessStep;
}
```

------

## 案例 6：按设备分组报警列表

### 场景说明

报警记录按设备分组展示，每个分组显示设备名称与报警数量，是多设备集中监控的常用模式。

### 对应 ListBox 核心特性

- `GroupStyle` 分组样式
- 配合 `CollectionViewSource` 实现数据分组
- 继承自 `ItemsControl` 的分组能力

### 1. ViewModel 分组数据源

csharp:

```c#
public ICollectionView AlarmGroupView { get; set; }

public void InitGroupData()
{
    var alarmList = new ObservableCollection<AlarmRecord>
    {
        new AlarmRecord { DeviceName = "喷涂A01", Message = "温度超限", Level = "警告" },
        new AlarmRecord { DeviceName = "喷涂A01", Message = "压力异常", Level = "严重" },
        new AlarmRecord { DeviceName = "固化B01", Message = "风机故障", Level = "严重" },
        new AlarmRecord { DeviceName = "固化B01", Message = "温度偏差", Level = "警告" },
        new AlarmRecord { DeviceName = "上料C01", Message = "物料不足", Level = "警告" },
    };

    // 创建集合视图并按设备名称分组
    var cvs = new CollectionViewSource { Source = alarmList };
    cvs.GroupDescriptions.Add(new PropertyGroupDescription("DeviceName"));
    AlarmGroupView = cvs.View;
}
```

### 2. XAML 分组展示

xaml:

```xaml
<ListBox ItemsSource="{Binding AlarmGroupView}"
         BorderBrush="#DDD" BorderThickness="1"
         Width="350">
    
    <!-- 分组头样式 -->
    <ListBox.GroupStyle>
        <GroupStyle>
            <GroupStyle.HeaderTemplate>
                <DataTemplate>
                    <Border Background="#E6F4FF" Padding="8 4" Margin="0 8 0 0">
                        <StackPanel Orientation="Horizontal">
                            <TextBlock Text="{Binding Name}" FontWeight="Bold" Foreground="#0958D9"/>
                            <TextBlock Text=" (" Foreground="#666"/>
                            <TextBlock Text="{Binding Items.Count}" Foreground="#666"/>
                            <TextBlock Text=" 条)" Foreground="#666"/>
                        </StackPanel>
                    </Border>
                </DataTemplate>
            </GroupStyle.HeaderTemplate>
        </GroupStyle>
    </ListBox.GroupStyle>

    <!-- 条目内容模板 -->
    <ListBox.ItemTemplate>
        <DataTemplate>
            <Grid Height="28" Margin="15 0 0 0">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="60"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                <TextBlock Grid.Column="0" Text="{Binding Level}" Foreground="#FA8C16" FontSize="12"/>
                <TextBlock Grid.Column="1" Text="{Binding Message}" VerticalAlignment="Center"/>
            </Grid>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

------

## 工业场景最佳实践总结

1. **大数据量必开虚拟化**：500 条以上数据必须开启 `VirtualizingStackPanel` + 回收模式，内存占用降低 90% 以上。
2. **单选用绑定，多选用附加属性**：单选直接绑定 `SelectedItem` 到 ViewModel；多选通过附加属性同步 `SelectedItems`，保持 MVVM 架构整洁。
3. **不要外层包裹 `ScrollViewer`**：`ListBox` 内置滚动，外层嵌套会导致滚动冲突、虚拟化失效、性能下降。
4. **批量操作用内置方法**：全选 / 全不选使用 `SelectAll()` / `UnselectAll()`，性能远高于循环逐条设置。
5. **样式与数据分离**：选中、交替行、状态色通过样式触发器实现，不要在数据模型中添加 UI 相关属性。