# 005004005_WPF `ListView` 工业场景实战案例合集

以下案例全部贴合工业上位机、设备监控、生产管理等真实业务场景，围绕 `ListView + GridView` 核心组合展开，从基础绑定到高级性能优化逐步深入，每个案例明确标注对应核心特性，可直接复用至项目中。

------

## 案例 1：设备台账基础多列表（GridView 基础用法 + MVVM 绑定）

### 场景说明

车间设备管理台账，展示设备编号、名称、类型、实时温度、运行状态等结构化信息，选中设备后可联动详情面板，是工业软件最基础的多列数据展示场景。

### 对应核心特性

- `ListView.View` + `GridView` 多列视图架构
- `GridViewColumn.DisplayMemberBinding` 文本列绑定
- `SelectedItem` 双向绑定（继承自 `Selector`）
- 纯 MVVM 数据驱动，天然支持 UI 虚拟化

### 1. 数据模型

csharp:

```c#
public class DeviceRecord : INotifyPropertyChanged
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

    private string _deviceType;
    public string DeviceType
    {
        get => _deviceType;
        set { _deviceType = value; OnPropertyChanged(); }
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
        SelectedDevice = DeviceList.First();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### 3. XAML 界面

xaml:

```xaml
<Window.DataContext>
    <local:DeviceViewModel/>
</Window.DataContext>

<Grid Margin="10">
    <ListView ItemsSource="{Binding DeviceList}"
              SelectedItem="{Binding SelectedDevice}"
              BorderBrush="#DDD" BorderThickness="1">
        <!-- 配置多列视图 -->
        <ListView.View>
            <GridView AllowsColumnReorder="False">
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

> 💡 说明：`AllowsColumnReorder="False"` 关闭列拖拽重排，避免操作人员误操作打乱固定列序，符合工业软件操作规范。

------

## 案例 2：自定义状态单元格（CellTemplate + 数据触发器）

### 场景说明

运行状态用彩色圆点直观展示，温度超阈值时红色加粗高亮，替代纯文本，提升工业监控界面的可读性。

### 对应核心特性

- `GridViewColumn.CellTemplate` 自定义单元格模板
- `DataTrigger` 数据触发器实现状态联动
- 纯 XAML 实现，与业务数据完全解耦

xaml:

```xaml
<ListView ItemsSource="{Binding DeviceList}" BorderBrush="#DDD" BorderThickness="1">
    <ListView.View>
        <GridView>
            <GridViewColumn Header="设备编号" DisplayMemberBinding="{Binding DeviceCode}" Width="100"/>
            <GridViewColumn Header="设备名称" DisplayMemberBinding="{Binding DeviceName}" Width="150"/>

            <!-- 自定义状态列：颜色指示灯 -->
            <GridViewColumn Header="状态" Width="60">
                <GridViewColumn.CellTemplate>
                    <DataTemplate>
                        <Ellipse Width="10" Height="10" VerticalAlignment="Center" HorizontalAlignment="Center">
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
                    </DataTemplate>
                </GridViewColumn.CellTemplate>
            </GridViewColumn>

            <!-- 温度列：超温红色高亮 -->
            <GridViewColumn Header="温度(℃)" Width="80">
                <GridViewColumn.CellTemplate>
                    <DataTemplate>
                        <TextBlock Text="{Binding Temperature, StringFormat=F1}" VerticalAlignment="Center" HorizontalAlignment="Right">
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

> 🔑 优先级规则：设置 `CellTemplate` 后，`DisplayMemberBinding` 自动失效，单元格内容完全由模板控制。

------

## 案例 3：生产记录多选批量操作（扩展多选 + 批量处理）

### 场景说明

生产记录列表支持 Ctrl 点选、Shift 连选，批量执行导出、删除、归档等操作，是工业数据管理系统的标配功能。

### 对应核心特性

- `SelectionMode="Extended"` 扩展多选模式（继承自 `ListBox`）
- `SelectedItems` 选中集合（只读）
- 批量操作性能优化，单次事件通知

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
        <Button Content="批量导出" Click="BatchExport_Click" Margin="0 0 8 0" Padding="12 4"/>
        <Button Content="批量归档" Click="BatchArchive_Click" Padding="12 4"/>
        <TextBlock Margin="20 0 0 0" VerticalAlignment="Center" Foreground="#666">
            已选中 <Run x:Name="SelectedCountText">0</Run> 条
        </TextBlock>
    </StackPanel>

    <!-- 生产记录列表 -->
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

### 2. 后台交互逻辑

csharp:

```c#
// 选中数量统计
private void ProductionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    SelectedCountText.Text = ProductionList.SelectedItems.Count.ToString();
}

// 全选
private void SelectAll_Click(object sender, RoutedEventArgs e)
{
    ProductionList.SelectAll();
}

// 取消全选
private void UnselectAll_Click(object sender, RoutedEventArgs e)
{
    ProductionList.UnselectAll();
}

// 批量导出
private void BatchExport_Click(object sender, RoutedEventArgs e)
{
    var selected = ProductionList.SelectedItems.Cast<ProductionRecord>().ToList();
    if (selected.Count == 0)
    {
        MessageBox.Show("请先选择要导出的记录");
        return;
    }
    
    // 批量导出 CSV/Excel 逻辑
    ExportHelper.ExportToCsv(selected);
    MessageBox.Show($"成功导出 {selected.Count} 条生产记录");
}

// 批量归档
private void BatchArchive_Click(object sender, RoutedEventArgs e)
{
    var selected = ProductionList.SelectedItems.Cast<ProductionRecord>().ToList();
    // 批量归档业务逻辑
}
```

> 💡 MVVM 多选方案：纯 MVVM 场景可通过自定义附加属性，监听 `SelectionChanged` 事件，将 `SelectedItems` 同步到 ViewModel 集合，避免后台代码直接操作控件。

------

## 案例 4：万级历史报警高性能列表（UI 虚拟化 + 交替行优化）

### 场景说明

上万条历史报警记录，要求滚动流畅、内存占用低，搭配交替行样式提升长列表可读性，是工业历史数据查询的核心性能优化场景。

### 对应核心特性

- `VirtualizingStackPanel` UI 虚拟化（继承自 `ItemsControl`）
- `VirtualizationMode="Recycling"` 容器回收模式
- `AlternationCount` 交替行计数
- `ScrollViewer.IsDeferredScrollingEnabled` 延迟滚动优化

xml:

```xaml
<ListView ItemsSource="{Binding HistoryAlarmList}"
          AlternationCount="2"
          VirtualizingStackPanel.IsVirtualizing="True"
          VirtualizingStackPanel.VirtualizationMode="Recycling"
          ScrollViewer.IsDeferredScrollingEnabled="True"
          ScrollViewer.CanContentScroll="True"
          BorderBrush="#DDD" BorderThickness="1"
          Height="500">

    <!-- 虚拟化布局面板 -->
    <ListView.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel/>
        </ItemsPanelTemplate>
    </ListView.ItemsPanel>

    <!-- 行容器样式：交替行背景 + 选中样式 -->
    <ListView.ItemContainerStyle>
        <Style TargetType="ListViewItem">
            <Setter Property="Background" Value="White"/>
            <Setter Property="Height" Value="28"/>
            <Setter Property="VerticalContentAlignment" Value="Center"/>
            <Style.Triggers>
                <!-- 奇偶行交替 -->
                <Trigger Property="ItemsControl.AlternationIndex" Value="1">
                    <Setter Property="Background" Value="#F8F9FA"/>
                </Trigger>
                <!-- 选中高亮 -->
                <Trigger Property="IsSelected" Value="True">
                    <Setter Property="Background" Value="#E6F4FF"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </ListView.ItemContainerStyle>

    <ListView.View>
        <GridView>
            <GridViewColumn Header="报警时间" DisplayMemberBinding="{Binding AlarmTime, StringFormat=yyyy-MM-dd HH:mm:ss}" Width="150"/>
            <GridViewColumn Header="级别" DisplayMemberBinding="{Binding Level}" Width="60"/>
            <GridViewColumn Header="设备名称" DisplayMemberBinding="{Binding DeviceName}" Width="120"/>
            <GridViewColumn Header="报警内容" DisplayMemberBinding="{Binding Message}" Width="*"/>
        </GridView>
    </ListView.View>
</ListView>
```

### 性能效果说明

- 开启虚拟化后，无论数据是 1000 条还是 100000 条，内存中始终只生成可见区域的行容器（约几十条）；
- 回收模式下，滚动时容器对象复用，大幅减少对象创建与 GC 压力；
- 延迟滚动开启后，拖动滚动条时仅更新提示，松开后才渲染内容，大幅提升大数据量下的拖动流畅度；
- 工业长列表场景性能提升 10 倍以上，是必开的优化项。

------

## 案例 5：按设备分组报警列表（分组视图 + GridView）

### 场景说明

报警记录按设备分组展示，每个分组显示设备名称与报警数量，多设备集中监控场景常用。

### 对应核心特性

- `GroupStyle` 分组样式（继承自 `ItemsControl`）
- 配合 `CollectionViewSource` 实现数据分组
- GridView 多列与分组无缝结合

### 1. ViewModel 分组数据源

csharp:

```c#
public ICollectionView AlarmGroupView { get; set; }

public void InitGroupData()
{
    var alarmList = new ObservableCollection<AlarmRecord>
    {
        new AlarmRecord { DeviceName = "喷涂A01", Message = "温度超限", Level = "警告", AlarmTime = DateTime.Now.AddMinutes(-5) },
        new AlarmRecord { DeviceName = "喷涂A01", Message = "压力异常", Level = "严重", AlarmTime = DateTime.Now.AddMinutes(-10) },
        new AlarmRecord { DeviceName = "固化B01", Message = "风机故障", Level = "严重", AlarmTime = DateTime.Now.AddMinutes(-3) },
        new AlarmRecord { DeviceName = "固化B01", Message = "温度偏差", Level = "警告", AlarmTime = DateTime.Now.AddMinutes(-20) },
        new AlarmRecord { DeviceName = "上料C01", Message = "物料不足", Level = "警告", AlarmTime = DateTime.Now.AddMinutes(-15) },
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
<ListView ItemsSource="{Binding AlarmGroupView}"
          BorderBrush="#DDD" BorderThickness="1"
          Width="500">

    <!-- 分组头样式 -->
    <ListView.GroupStyle>
        <GroupStyle>
            <GroupStyle.HeaderTemplate>
                <DataTemplate>
                    <Border Background="#E6F4FF" Padding="8 4" Margin="0 8 0 0">
                        <StackPanel Orientation="Horizontal">
                            <TextBlock Text="{Binding Name}" FontWeight="Bold" Foreground="#0958D9"/>
                            <TextBlock Text=" (" Foreground="#666"/>
                            <TextBlock Text="{Binding Items.Count}" Foreground="#666"/>
                            <TextBlock Text=" 条报警)" Foreground="#666"/>
                        </StackPanel>
                    </Border>
                </DataTemplate>
            </GroupStyle.HeaderTemplate>
        </GroupStyle>
    </ListView.GroupStyle>

    <ListView.View>
        <GridView>
            <GridViewColumn Header="时间" DisplayMemberBinding="{Binding AlarmTime, StringFormat=HH:mm:ss}" Width="80"/>
            <GridViewColumn Header="级别" DisplayMemberBinding="{Binding Level}" Width="60"/>
            <GridViewColumn Header="报警内容" DisplayMemberBinding="{Binding Message}" Width="*"/>
        </GridView>
    </ListView.View>
</ListView>
```

------

## 案例 6：自定义列头与右键菜单（列头模板 + 交互扩展）

### 场景说明

自定义列头样式，增加排序指示箭头；列头右键菜单支持显示 / 隐藏列、按本列排序，提升工业数据表格的操作效率。

### 对应核心特性

- `GridView.ColumnHeaderTemplate` 全局列头模板
- `GridView.ColumnHeaderContextMenu` 列头右键菜单
- `AllowsColumnReorder` 列重排控制

xaml:

```xaml
<ListView ItemsSource="{Binding DeviceList}" BorderBrush="#DDD" BorderThickness="1">
    <ListView.View>
        <GridView AllowsColumnReorder="True">
            <!-- 全局列头右键菜单 -->
            <GridView.ColumnHeaderContextMenu>
                <ContextMenu>
                    <MenuItem Header="按本列升序" Click="SortAscending_Click"/>
                    <MenuItem Header="按本列降序" Click="SortDescending_Click"/>
                    <Separator/>
                    <MenuItem Header="显示/隐藏列" IsCheckable="True"/>
                    <MenuItem Header="导出本列数据"/>
                </ContextMenu>
            </GridView.ColumnHeaderContextMenu>

            <!-- 全局列头模板：带下划线、排序箭头占位 -->
            <GridView.ColumnHeaderTemplate>
                <DataTemplate>
                    <DockPanel Height="28" VerticalAlignment="Center">
                        <TextBlock Text="{Binding}" VerticalAlignment="Center"/>
                        <TextBlock x:Name="SortArrow" DockPanel.Dock="Right" Margin="5 0 0 0" Foreground="#1677FF" Visibility="Collapsed">▲</TextBlock>
                    </DockPanel>
                </DataTemplate>
            </GridView.ColumnHeaderTemplate>

            <GridViewColumn Header="设备编号" DisplayMemberBinding="{Binding DeviceCode}" Width="100"/>
            <GridViewColumn Header="设备名称" DisplayMemberBinding="{Binding DeviceName}" Width="150"/>
            <GridViewColumn Header="温度(℃)" DisplayMemberBinding="{Binding Temperature, StringFormat=F1}" Width="80"/>
        </GridView>
    </ListView.View>
</ListView>
```

------

## 案例 7：工业深色主题完整样式

### 场景说明

适配工控机深色操作界面，自定义行选中态、悬浮态、交替行样式，区分激活与失焦选中效果，符合工业现场低眩光操作需求。

### 对应核心特性

- `ItemContainerStyle` 行容器完全自定义
- `MultiTrigger` 组合条件触发
- `Selector.IsSelectionActive` 焦点激活状态

xaml:

```xaml
<ListView ItemsSource="{Binding AlarmList}"
          AlternationCount="2"
          Background="#1E1E1E" Foreground="#E0E0E0"
          BorderBrush="#333" BorderThickness="1"
          Width="600">

    <ListView.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel VirtualizationMode="Recycling"/>
        </ItemsPanelTemplate>
    </ListView.ItemsPanel>

    <ListView.ItemContainerStyle>
        <Style TargetType="ListViewItem">
            <Setter Property="Padding" Value="8 4"/>
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#E0E0E0"/>
            <Setter Property="BorderThickness" Value="0 0 0 1"/>
            <Setter Property="BorderBrush" Value="#2D2D2D"/>
            <Setter Property="FocusVisualStyle" Value="{x:Null}"/>
            <Setter Property="VerticalContentAlignment" Value="Center"/>

            <Style.Triggers>
                <!-- 奇偶行交替 -->
                <Trigger Property="ItemsControl.AlternationIndex" Value="1">
                    <Setter Property="Background" Value="#252526"/>
                </Trigger>

                <!-- 鼠标悬浮 -->
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#2A2D2E"/>
                </Trigger>

                <!-- 选中 + 有焦点：深蓝色高亮 -->
                <MultiTrigger>
                    <MultiTrigger.Conditions>
                        <Condition Property="IsSelected" Value="True"/>
                        <Condition Property="Selector.IsSelectionActive" Value="True"/>
                    </MultiTrigger.Conditions>
                    <Setter Property="Background" Value="#0E639C"/>
                    <Setter Property="Foreground" Value="White"/>
                </MultiTrigger>

                <!-- 选中 + 失焦：灰色标识 -->
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
    </ListView.ItemContainerStyle>

    <ListView.View>
        <GridView>
            <GridViewColumn Header="时间" DisplayMemberBinding="{Binding AlarmTime, StringFormat=HH:mm:ss}" Width="100"/>
            <GridViewColumn Header="级别" DisplayMemberBinding="{Binding Level}" Width="60"/>
            <GridViewColumn Header="设备" DisplayMemberBinding="{Binding DeviceName}" Width="100"/>
            <GridViewColumn Header="内容" DisplayMemberBinding="{Binding Message}" Width="*"/>
        </GridView>
    </ListView.View>
</ListView>
```

------

## 工业场景最佳实践总结

1. **只读多列优先选 ListView+GridView**：相比 `DataGrid` 更轻量、性能更优、样式更灵活，80% 以上的只读展示场景完全够用。
2. **大数据量必开虚拟化**：500 行以上必须开启 `VirtualizingStackPanel` + `Recycling` 回收模式，内存占用降低 90% 以上。
3. **简单列用 DisplayMemberBinding，复杂列用 CellTemplate**：纯文本用绑定性能最优，状态灯、按钮等复杂内容再用自定义模板。
4. **关键表格关闭列重排**：工业生产数据通常有固定列序要求，设置 `AllowsColumnReorder="False"` 防止误操作。
5. **避免大量 Auto 列宽**：`Width="Auto"` 会逐行计算宽度，大数据量下性能急剧下降，优先使用固定宽度或比例宽度。