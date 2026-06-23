# 005002004_WPF `ItemsControl` 基类官方解析与实战实例

`Selector` 是抽象基类，无法直接实例化，所有案例均基于其标准派生控件（`ListBox`/`ComboBox`）实现，**核心能力全部由 `Selector` 基类提供**，可无缝迁移到所有 `Selector` 子类。案例从基础绑定到高级扩展逐步深入，全部贴合工业上位机、设备监控、生产管理等典型业务场景。

------

## 案例 1：MVVM 主从详情视图（`SelectedItem` 双向绑定）

### 场景说明

左侧设备列表，选中某台设备后右侧自动显示实时参数、运行状态，是工业监控系统最经典的主从布局。核心依赖 `Selector` 基类的 `SelectedItem` 属性实现数据驱动的选择联动。

### 1. 数据模型

csharp：

```c#
public class DeviceInfo : INotifyPropertyChanged
{
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

    private string _status;
    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
```

### 2. ViewModel

csharp:

```c#
public class DeviceMonitorViewModel : INotifyPropertyChanged
{
    public ObservableCollection<DeviceInfo> DeviceList { get; set; }

    private DeviceInfo _selectedDevice;
    /// <summary>
    /// 绑定Selector的SelectedItem属性，选中变化自动同步
    /// </summary>
    public DeviceInfo SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            _selectedDevice = value;
            OnPropertyChanged();
            // 选中变化后可加载详情、订阅实时数据
            if (value != null)
            {
                LoadDeviceRealtimeData(value);
            }
        }
    }

    public DeviceMonitorViewModel()
    {
        // 初始化模拟数据
        DeviceList = new ObservableCollection<DeviceInfo>
        {
            new DeviceInfo { DeviceName = "PLC-喷涂A01", Temperature = 45.2, Status = "运行中" },
            new DeviceInfo { DeviceName = "PLC-喷涂A02", Temperature = 52.8, Status = "运行中" },
            new DeviceInfo { DeviceName = "PLC-固化B01", Temperature = 38.1, Status = "待机" },
        };
    }

    private void LoadDeviceRealtimeData(DeviceInfo device)
    {
        // 加载设备详情数据、订阅实时推送
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
```

### 3. XAML 界面

xaml:

```xaml
<Window.DataContext>
    <local:DeviceMonitorViewModel/>
</Window.DataContext>

<Grid Margin="10">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="220"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>

    <!-- 左侧设备列表：ListBox继承自Selector -->
    <ListBox Grid.Column="0" 
             ItemsSource="{Binding DeviceList}"
             SelectedItem="{Binding SelectedDevice}"
             DisplayMemberPath="DeviceName"
             BorderThickness="1" BorderBrush="#DDD"/>

    <!-- 右侧设备详情面板 -->
    <Border Grid.Column="1" Margin="10 0 0 0" Background="#F8F9FA" Padding="15" BorderBrush="#DDD" BorderThickness="1">
        <StackPanel DataContext="{Binding SelectedDevice}">
            <TextBlock FontSize="18" FontWeight="Bold" Text="{Binding DeviceName}"/>
            <Separator Margin="0 10"/>
            <TextBlock Text="{Binding Temperature, StringFormat=当前温度：{0:F1}℃}" FontSize="14"/>
            <TextBlock Margin="0 8" Text="{Binding Status, StringFormat=运行状态：{0}}" FontSize="14"/>
        </StackPanel>
    </Border>
</Grid>
```

### 对应 Selector 核心特性

- `SelectedItem` 依赖属性：双向绑定，选中变化自动同步到 ViewModel；
- 数据驱动选择：选中状态绑定在数据对象上，而非 UI 容器，天然支持 UI 虚拟化。

------

## 案例 2：下拉选择提交（`SelectedValuePath` + `SelectedValue`）

### 场景说明

生产配方下拉选择，界面显示配方名称，后台仅提交配方 ID，实现「显示友好 + 数据精简」的分离。核心依赖 `Selector` 基类的 `SelectedValue` 与 `SelectedValuePath` 属性。

### 1. 数据模型

csharp:

```c#
public class RecipeInfo
{
    /// <summary>
    /// 配方ID：提交给PLC/数据库的关键字段
    /// </summary>
    public int RecipeId { get; set; }
    /// <summary>
    /// 配方名称：界面显示用
    /// </summary>
    public string RecipeName { get; set; }
    public string Description { get; set; }
}
```

### 2. ViewModel

csharp:

```c#
public class RecipeViewModel : INotifyPropertyChanged
{
    public ObservableCollection<RecipeInfo> RecipeList { get; set; }

    private int _selectedRecipeId;
    /// <summary>
    /// 绑定Selector的SelectedValue，仅保存ID，轻量化交互
    /// </summary>
    public int SelectedRecipeId
    {
        get => _selectedRecipeId;
        set { _selectedRecipeId = value; OnPropertyChanged(); }
    }

    public RecipeViewModel()
    {
        RecipeList = new ObservableCollection<RecipeInfo>
        {
            new RecipeInfo { RecipeId = 101, RecipeName = "标准喷涂工艺", Description = "常规产品默认工艺" },
            new RecipeInfo { RecipeId = 102, RecipeName = "加厚涂层工艺", Description = "高防腐产品专用" },
            new RecipeInfo { RecipeId = 103, RecipeName = "低温固化工艺", Description = "热敏产品专用" },
        };
        // 默认选中第一个
        SelectedRecipeId = 101;
    }

    // 下发配方命令
    public void SendRecipeToPlc()
    {
        // 只需要传递SelectedRecipeId，无需传递整个对象
        int recipeId = SelectedRecipeId;
        // PLC通信下发逻辑...
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
```

### 3. XAML 界面

xaml:

```xaml
<StackPanel Margin="20" Width="300" HorizontalAlignment="Left">
    <TextBlock Text="选择生产配方：" Margin="0 0 0 5"/>
    <!-- ComboBox继承自Selector -->
    <ComboBox ItemsSource="{Binding RecipeList}"
              DisplayMemberPath="RecipeName"    <!-- 界面显示配方名称 -->
              SelectedValuePath="RecipeId"     <!-- 选中值提取RecipeId属性 -->
              SelectedValue="{Binding SelectedRecipeId}" <!-- 绑定到ViewModel的ID -->
              Height="30"/>

    <Button Content="下发到设备" Margin="0 15 0 0" Height="30" Click="SendRecipe_Click"/>
</StackPanel>
```

### 对应 Selector 核心特性

- `DisplayMemberPath`：控制界面显示的属性，继承自 `ItemsControl`；
- `SelectedValuePath`：指定选中值的提取路径，`Selector` 基类核心属性；
- `SelectedValue`：选中项的轻量化值，适合数据提交场景；
- 支持反向赋值：设置 `SelectedRecipeId` 可自动匹配并选中对应项。

------

## 案例 3：自定义选中样式（`IsSelected` + `IsSelectionActive` 触发器）

### 场景说明

工业报警列表自定义选中效果：选中且控件有焦点时深蓝色高亮，选中但失焦时浅灰色显示，区分激活状态。核心依赖 `Selector` 的两个附加属性：`IsSelected` 和 `IsSelectionActive`。

xaml:

```xaml
<ListBox ItemsSource="{Binding AlarmList}" Width="400" HorizontalAlignment="Left">
    <ListBox.ItemContainerStyle>
        <Style TargetType="ListBoxItem">
            <Setter Property="Padding" Value="8 4"/>
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#333"/>
            <Setter Property="BorderThickness" Value="0 0 1 0"/>
            <Setter Property="BorderBrush" Value="#EEE"/>

            <Style.Triggers>
                <!-- 1. 选中 + 控件有焦点：深蓝色高亮 -->
                <MultiTrigger>
                    <MultiTrigger.Conditions>
                        <Condition Property="Selector.IsSelected" Value="True"/>
                        <Condition Property="Selector.IsSelectionActive" Value="True"/>
                    </MultiTrigger.Conditions>
                    <Setter Property="Background" Value="#1677FF"/>
                    <Setter Property="Foreground" Value="White"/>
                </MultiTrigger>

                <!-- 2. 选中 + 控件失焦：浅灰色，保留选中标识 -->
                <MultiTrigger>
                    <MultiTrigger.Conditions>
                        <Condition Property="Selector.IsSelected" Value="True"/>
                        <Condition Property="Selector.IsSelectionActive" Value="False"/>
                    </MultiTrigger.Conditions>
                    <Setter Property="Background" Value="#E5E7EB"/>
                    <Setter Property="Foreground" Value="#333"/>
                </MultiTrigger>

                <!-- 3. 鼠标悬浮效果 -->
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#F0F7FF"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </ListBox.ItemContainerStyle>

    <ListBox.ItemTemplate>
        <DataTemplate>
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="120"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                <TextBlock Text="{Binding AlarmTime, StringFormat=HH:mm:ss}"/>
                <TextBlock Grid.Column="1" Text="{Binding Message}"/>
            </Grid>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

### 对应 Selector 核心特性

- `IsSelected` 附加属性：标记单个容器是否选中，样式触发器的核心数据源；
- `IsSelectionActive` 附加属性：标记选择器是否处于键盘激活状态，实现焦点态 / 失焦态的视觉区分；
- 纯样式实现，与业务数据完全解耦，可全局复用。

------

## 案例 4：多控件选中联动（`IsSynchronizedWithCurrentItem`）

### 场景说明

同一份设备数据，同时在列表和下拉框中显示，任意一个控件选中，另一个自动同步，无需手动写同步代码。核心依赖 `Selector` 基类的 `IsSynchronizedWithCurrentItem` 属性 + 集合视图机制。

### 1. ViewModel

csharp:

```c#
public class LinkageViewModel : INotifyPropertyChanged
{
    public ObservableCollection<DeviceInfo> DeviceList { get; set; }
    public ICollectionView DeviceView { get; set; }

    public LinkageViewModel()
    {
        DeviceList = new ObservableCollection<DeviceInfo>
        {
            new DeviceInfo { DeviceName = "PLC-A01", Status = "运行中" },
            new DeviceInfo { DeviceName = "PLC-A02", Status = "运行中" },
            new DeviceInfo { DeviceName = "PLC-B01", Status = "待机" },
        };
        // 创建集合视图，作为共享数据源
        DeviceView = CollectionViewSource.GetDefaultView(DeviceList);
    }
}
```

### 2. XAML 界面

xaml:

```xaml
<StackPanel Margin="20" Width="250">
    <!-- 下拉框：开启与集合视图同步 -->
    <ComboBox ItemsSource="{Binding DeviceView}"
              DisplayMemberPath="DeviceName"
              IsSynchronizedWithCurrentItem="True"
              Height="30"/>

    <!-- 列表：同样开启同步 -->
    <ListBox Margin="0 15 0 0"
             ItemsSource="{Binding DeviceView}"
             DisplayMemberPath="DeviceName"
             IsSynchronizedWithCurrentItem="True"
             Height="150" BorderBrush="#DDD" BorderThickness="1"/>
</StackPanel>
```

### 运行效果

- 下拉框选中某设备 → 列表自动选中同一项；
- 列表点击选中 → 下拉框自动切换到对应项；
- 完全无需后台同步代码，由 `Selector` 基类通过集合视图自动联动。

### 对应 Selector 核心特性

- `IsSynchronizedWithCurrentItem`：控制是否与 `ICollectionView` 的 `CurrentItem` 双向同步；
- 多个 `Selector` 控件绑定同一个集合视图时，天然实现选中联动，适合多维度数据展示场景。

------

## 案例 5：容器级选中交互（`Selected` 附加路由事件）

### 场景说明

报警行被选中时，播放一个左侧指示条滑入的动画，实现细粒度的选中交互。核心依赖 `Selector` 基类的 `SelectedEvent` 附加路由事件，事件在每个容器被选中时独立触发。

### XAML 实现

xaml:

```xaml
<ListBox ItemsSource="{Binding AlarmList}" Width="400" HorizontalAlignment="Left">
    <ListBox.ItemContainerStyle>
        <Style TargetType="ListBoxItem">
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="ListBoxItem">
                        <DockPanel Background="{TemplateBinding Background}" Height="36">
                            <!-- 左侧选中指示条：默认宽度为0，选中时动画展开 -->
                            <Border DockPanel.Dock="Left" x:Name="IndicatorBar" 
                                    Width="0" Background="#F5222D">
                                <Border.Triggers>
                                    <!-- 监听Selector.Selected附加事件，触发动画 -->
                                    <EventTrigger RoutedEvent="Selector.Selected">
                                        <BeginStoryboard>
                                            <Storyboard>
                                                <DoubleAnimation Storyboard.TargetName="IndicatorBar"
                                                                 Storyboard.TargetProperty="Width"
                                                                 To="4" Duration="0:0:0.2"/>
                                            </Storyboard>
                                        </BeginStoryboard>
                                    </EventTrigger>
                                    
                                    <!-- 取消选中时收回 -->
                                    <EventTrigger RoutedEvent="Selector.Unselected">
                                        <BeginStoryboard>
                                            <Storyboard>
                                                <DoubleAnimation Storyboard.TargetName="IndicatorBar"
                                                                 Storyboard.TargetProperty="Width"
                                                                 To="0" Duration="0:0:0.2"/>
                                            </Storyboard>
                                        </BeginStoryboard>
                                    </EventTrigger>
                                </Border.Triggers>
                            </Border>
                            
                            <ContentPresenter Margin="8 0" VerticalAlignment="Center"/>
                        </DockPanel>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
    </ListBox.ItemContainerStyle>

    <ListBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Message}"/>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

### 对应 Selector 核心特性

- `SelectedEvent` / `UnselectedEvent`：容器级附加路由事件，每个条目选中 / 取消选中时独立触发；
- 事件冒泡机制，可在容器模板内直接监听，实现单条粒度的交互效果。

------

## 案例 6：自定义 Selector 子类（扩展选中逻辑）

### 场景说明

实现一个工业专用的报警选择列表，重写容器生命周期方法，给报警容器自动附加等级颜色，同时兼容 UI 虚拟化。核心依赖 `Selector` 基类的扩展虚方法。

### 1. 自定义 Selector 子类

csharp:

```c#
public class AlarmSelector : Selector
{
    /// <summary>
    /// 重写：创建自定义报警条目容器
    /// </summary>
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new AlarmItem();
    }

    /// <summary>
    /// 重写：判断对象本身是否是容器
    /// </summary>
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is AlarmItem;
    }

    /// <summary>
    /// 重写：容器准备阶段，根据报警等级设置样式
    /// </summary>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        
        if (element is AlarmItem container && item is AlarmRecord alarm)
        {
            // 根据报警等级设置容器背景色
            container.LevelColor = alarm.Level switch
            {
                "严重" => Brushes.Red,
                "警告" => Brushes.Orange,
                _ => Brushes.Gray
            };
        }
    }

    /// <summary>
    /// 重写：容器回收时清理状态，适配虚拟化
    /// </summary>
    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        base.ClearContainerForItemOverride(element, item);
        
        if (element is AlarmItem container)
        {
            // 必须清理自定义属性，否则虚拟化复用时会出现颜色错乱
            container.ClearValue(AlarmItem.LevelColorProperty);
        }
    }
}

/// <summary>
/// 自定义报警条目容器
/// </summary>
public class AlarmItem : ContentControl
{
    public static readonly DependencyProperty LevelColorProperty = DependencyProperty.Register(
        "LevelColor", typeof(Brush), typeof(AlarmItem), new PropertyMetadata(Brushes.Gray));

    public Brush LevelColor
    {
        get => (Brush)GetValue(LevelColorProperty);
        set => SetValue(LevelColorProperty, value);
    }

    static AlarmItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AlarmItem), 
            new FrameworkPropertyMetadata(typeof(AlarmItem)));
    }
}
```

### 2. 对应 Selector 核心特性

- `GetContainerForItemOverride`：替换默认容器类型；
- `PrepareContainerForItemOverride`：容器生成 / 复用时初始化状态；
- `ClearContainerForItemOverride`：容器回收时清理状态，是 UI 虚拟化兼容的关键；
- 所有选中逻辑由 `Selector` 基类自动提供，子类只需关注业务扩展。

------

## 工业场景最佳实践

1. **数据驱动优先**：永远通过绑定 `SelectedItem`/`SelectedValue` 操作选中状态，不要直接操作 UI 容器；虚拟化下不可见的容器不存在，操作容器会失效。
2. **场景化选择绑定方式**：
   - 主从详情、需要完整对象 → 绑定 `SelectedItem`；
   - 下拉提交、只需要关键字段 → 绑定 `SelectedValue` + `SelectedValuePath`。
3. **选中样式纯样式实现**：通过 `IsSelected` 触发器实现视觉效果，不要在数据模型中加 `IsSelected` 属性，保持数据与 UI 分离。
4. **选中事件轻量处理**：`SelectionChanged` 事件中禁止执行耗时操作（PLC 通信、数据库查询），耗时逻辑必须异步执行，避免阻塞 UI 线程。
5. **长列表开启虚拟化**：`Selector` 底层已实现选中状态持久化，开启 `VirtualizingStackPanel` 不会丢失选中状态，大数据量必须开启。