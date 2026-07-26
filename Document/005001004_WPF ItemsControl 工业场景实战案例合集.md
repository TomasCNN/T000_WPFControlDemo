# 005001004_WPF `ItemsControl` 工业场景实战案例合集

以下案例全部贴合**工业上位机、设备监控、生产数据**等典型业务场景，从基础用法到高级定制逐步深入，每个案例都明确对应 `ItemsControl` 的核心特性，可直接复用至实际项目中。

------

## 一、基础入门案例

### 案例 1：静态条目集合（纯 XAML 声明）

#### 场景说明

固定的功能按钮栏、状态指示灯组等静态条目集合，无需后台数据绑定，直接通过 XAML 添加子元素。

#### 实现代码

xaml:

```xaml
<Window x:Class="IndustrialDemo.StaticItemsView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="静态功能栏">
    <Grid>
        <!-- 静态功能按钮组：子元素自动添加到 Items 集合 -->
        <ItemsControl Width="120" HorizontalAlignment="Left" Background="#F5F5F5">
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <StackPanel Orientation="Vertical" Margin="5"/>
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
            
            <!-- 直接写子元素，等价于 Items.Add() -->
            <Button Content="启动设备" Margin="0 3" Height="30"/>
            <Button Content="停止设备" Margin="0 3" Height="30"/>
            <Button Content="复位报警" Margin="0 3" Height="30"/>
            <Button Content="手动模式" Margin="0 3" Height="30"/>
        </ItemsControl>
    </Grid>
</Window>
```

#### 核心特性对应

- 利用 `[ContentProperty("Items")]` 特性，XAML 子元素自动加入 `Items` 集合；
- 底层实现 `IAddChild` 接口，由 XAML 解析器自动调用 `AddChild` 方法；
- 适合固定、少量的静态控件集合。

------

### 案例 2：MVVM 数据绑定 + 自定义数据模板

#### 场景说明

设备运行状态列表，通过 `ItemsSource` 绑定业务数据集合，用 `ItemTemplate` 自定义每行的显示布局，是工业软件最常用的基础模式。

#### 1. 数据模型

csharp:

```xaml
// 设备信息模型
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

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set { _isRunning = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
```

#### 2. ViewModel

csharp:

```c#
public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<DeviceInfo> DeviceList { get; set; }

    public MainViewModel()
    {
        DeviceList = new ObservableCollection<DeviceInfo>
        {
            new DeviceInfo { DeviceName = "PLC-A01", Temperature = 45.2, IsRunning = true },
            new DeviceInfo { DeviceName = "PLC-A02", Temperature = 52.8, IsRunning = true },
            new DeviceInfo { DeviceName = "PLC-B01", Temperature = 38.1, IsRunning = false },
        };
    }

    // INotifyPropertyChanged 实现略
}
```

#### 3. XAML 界面

xaml:

```xaml
<Window.DataContext>
    <local:MainViewModel/>
</Window.DataContext>

<Grid Margin="10">
    <ItemsControl ItemsSource="{Binding DeviceList}">
        <!-- 布局面板：垂直排列 -->
        <ItemsControl.ItemsPanel>
            <ItemsPanelTemplate>
                <StackPanel Orientation="Vertical"/>
            </ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>

        <!-- 数据模板：定义每行的显示内容 -->
        <ItemsControl.ItemTemplate>
            <DataTemplate DataType="{x:Type local:DeviceInfo}">
                <Border Height="40" BorderBrush="#DDD" BorderThickness="0 0 0 1" Padding="5">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="20"/>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="100"/>
                        </Grid.ColumnDefinitions>
                        
                        <!-- 运行状态指示灯 -->
                        <Ellipse Grid.Column="0" Width="12" Height="12" VerticalAlignment="Center">
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
                        
                        <TextBlock Grid.Column="1" Text="{Binding DeviceName}" VerticalAlignment="Center"/>
                        <TextBlock Grid.Column="2" Text="{Binding Temperature, StringFormat={}{0:F1}℃}" 
                                   VerticalAlignment="Center" HorizontalAlignment="Right"/>
                    </Grid>
                </Border>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</Grid>
```

#### 核心特性对应

- `ItemsSource` 绑定 `ObservableCollection<T>`，集合增删自动同步 UI；
- `ItemTemplate` 定义数据项的可视化呈现，实现数据与视图分离；
- 完全符合 MVVM 架构，是工业数据列表的标准写法。

------

## 二、进阶实战案例

### 案例 3：高性能长列表（UI 虚拟化 + 交替行）

#### 场景说明

上千条的报警历史、生产记录等大数据量列表，必须开启 UI 虚拟化降低内存占用、提升滚动流畅度，同时实现奇偶行交替背景提升可读性。

#### 实现代码

xaml:

```xaml
<ItemsControl ItemsSource="{Binding AlarmHistoryList}"
              AlternationCount="2"
              VirtualizingStackPanel.IsVirtualizing="True"
              VirtualizingStackPanel.VirtualizationMode="Recycling">
    
    <!-- 替换布局面板为虚拟化栈面板 -->
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel ScrollViewer.CanContentScroll="True"/>
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>

    <!-- 条目容器样式：奇偶行交替背景 -->
    <ItemsControl.ItemContainerStyle>
        <Style TargetType="ContentPresenter">
            <Setter Property="Background" Value="White"/>
            <Style.Triggers>
                <!-- 绑定 AlternationIndex 附加属性，索引为1时换背景 -->
                <Trigger Property="ItemsControl.AlternationIndex" Value="1">
                    <Setter Property="Background" Value="#F8F9FA"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </ItemsControl.ItemContainerStyle>

    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Grid Height="32" Margin="8 0">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="150"/>
                    <ColumnDefinition Width="80"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                <TextBlock Text="{Binding AlarmTime, StringFormat=yyyy-MM-dd HH:mm:ss}"/>
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
                <TextBlock Grid.Column="2" Text="{Binding Message}"/>
            </Grid>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

#### 核心特性对应

1. **UI 虚拟化**：`ItemsPanel` 替换为 `VirtualizingStackPanel`，仅生成可见区域的容器，万级数据内存占用降低 90% 以上；
2. **回收模式**：`VirtualizationMode="Recycling"` 复用容器，减少滚动时的对象创建与 GC 压力；
3. **交替行**：`AlternationCount="2"` + `AlternationIndex` 附加属性触发器，纯样式实现奇偶行变色，无需修改数据模型。

> ⚠️ 工业场景必备：超过 500 条的列表必须开启虚拟化，否则滚动卡顿、内存暴涨。

------

### 案例 4：分组数据展示（按班次分组生产记录）

#### 场景说明

生产数据按班次、批次分组展示，分组头显示汇总信息，是生产报表、产量统计的常用模式。

#### 实现步骤

1. **ViewModel 中准备分组数据源**

csharp:

```c#
public ICollectionView ProductionGroupView { get; set; }

public MainViewModel()
{
    var productionList = new ObservableCollection<ProductionRecord>
    {
        new ProductionRecord { Shift = "早班", ProductName = "产品A", Qty = 120 },
        new ProductionRecord { Shift = "早班", ProductName = "产品B", Qty = 85 },
        new ProductionRecord { Shift = "中班", ProductName = "产品A", Qty = 150 },
        new ProductionRecord { Shift = "中班", ProductName = "产品C", Qty = 60 },
    };

    // 用 CollectionViewSource 按 Shift 字段分组
    var cvs = new CollectionViewSource { Source = productionList };
    cvs.GroupDescriptions.Add(new PropertyGroupDescription("Shift"));
    ProductionGroupView = cvs.View;
}
```

1. **XAML 分组展示**

xaml:

```xaml
<ItemsControl ItemsSource="{Binding ProductionGroupView}">
    <!-- 分组样式：定义分组头外观 -->
    <ItemsControl.GroupStyle>
        <GroupStyle>
            <GroupStyle.HeaderTemplate>
                <DataTemplate>
                    <Border Background="#E8F0FE" Padding="8 4" Margin="0 8 0 0">
                        <TextBlock FontWeight="Bold" Foreground="#1677FF">
                            <Run Text="{Binding Name}"/>
                            <Run Text="  产量合计："/>
                            <Run Text="{Binding Items.Count}"/>
                        </TextBlock>
                    </Border>
                </DataTemplate>
            </GroupStyle.HeaderTemplate>
        </GroupStyle>
    </ItemsControl.GroupStyle>

    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Grid Height="30" Margin="20 0 0 0">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="100"/>
                </Grid.ColumnDefinitions>
                <TextBlock Text="{Binding ProductName}" VerticalAlignment="Center"/>
                <TextBlock Grid.Column="1" Text="{Binding Qty}" HorizontalAlignment="Right"/>
            </Grid>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

#### 核心特性对应

- `GroupStyle` 定义分组头模板，支持多级分组；
- `IsGrouping` 只读属性自动标识分组状态；
- 配合 `CollectionViewSource` 实现数据分组，无需手动拼装嵌套集合，性能与可维护性更优。

------

### 案例 5：空状态占位提示

#### 场景说明

列表无数据时显示「暂无数据」提示，提升工业软件的交互完整性。

#### 实现代码

xaml:

```xaml
<ItemsControl ItemsSource="{Binding DeviceList}">
    <ItemsControl.Style>
        <Style TargetType="ItemsControl">
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="ItemsControl">
                        <Border Background="{TemplateBinding Background}"
                                BorderBrush="{TemplateBinding BorderBrush}"
                                BorderThickness="{TemplateBinding BorderThickness}">
                            <ScrollViewer>
                                <!-- 条目呈现器：正常显示列表 -->
                                <ItemsPresenter x:Name="ItemsPresenter"/>
                            </ScrollViewer>
                            
                            <!-- 空状态提示：默认隐藏 -->
                            <TextBlock x:Name="EmptyTip" Text="暂无数据" 
                                       Foreground="#999" FontSize="14"
                                       HorizontalAlignment="Center" VerticalAlignment="Center"
                                       Visibility="Collapsed"/>
                        </Border>
                        
                        <ControlTemplate.Triggers>
                            <!-- 绑定 HasItems 属性，无数据时显示提示 -->
                            <Trigger Property="HasItems" Value="False">
                                <Setter TargetName="ItemsPresenter" Property="Visibility" Value="Collapsed"/>
                                <Setter TargetName="EmptyTip" Property="Visibility" Value="Visible"/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
    </ItemsControl.Style>

    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding DeviceName}" Padding="5"/>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

#### 核心特性对应

- 利用 `HasItems` 只读依赖属性，通过控件模板触发器切换空状态；
- 无需额外写后台逻辑，纯 XAML 实现，简洁优雅。

------

## 三、高级定制案例

### 案例 6：自定义条目容器（带等级指示的报警行）

#### 场景说明

默认 `ContentPresenter` 容器无法满足复杂需求，通过重写 `ItemsControl` 容器生命周期方法，实现自定义条目容器。

#### 1. 自定义 ItemsControl 子类

csharp:

```c#
public class AlarmItemsControl : ItemsControl
{
    // 创建自定义容器
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new AlarmItemContainer();
    }

    // 判断是否本身就是容器
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is AlarmItemContainer;
    }

    // 容器准备：绑定数据
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is AlarmItemContainer container && item is AlarmRecord alarm)
        {
            container.AlarmLevel = alarm.Level;
        }
    }

    // 容器回收：清理状态（虚拟化必须）
    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        base.ClearContainerForItemOverride(element, item);
        if (element is AlarmItemContainer container)
        {
            container.ClearValue(AlarmItemContainer.AlarmLevelProperty);
        }
    }
}
```

#### 2. 自定义容器控件

csharp:

```c#
public class AlarmItemContainer : Control
{
    public static readonly DependencyProperty AlarmLevelProperty =
        DependencyProperty.Register("AlarmLevel", typeof(string), typeof(AlarmItemContainer));

    public string AlarmLevel
    {
        get => (string)GetValue(AlarmLevelProperty);
        set => SetValue(AlarmLevelProperty, value);
    }

    static AlarmItemContainer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AlarmItemContainer), 
            new FrameworkPropertyMetadata(typeof(AlarmItemContainer)));
    }
}
```

#### 3. 容器样式（Generic.xaml）

xaml:

```xaml
<Style TargetType="{x:Type local:AlarmItemContainer}">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:AlarmItemContainer}">
                <DockPanel Height="36" Margin="0 2">
                    <!-- 左侧等级颜色条 -->
                    <Border DockPanel.Dock="Left" Width="4" x:Name="LevelBar">
                        <Border.Style>
                            <Style TargetType="Border">
                                <Setter Property="Background" Value="Orange"/>
                                <Style.Triggers>
                                    <DataTrigger Binding="{Binding AlarmLevel, RelativeSource={RelativeSource TemplatedParent}}" Value="严重">
                                        <Setter Property="Background" Value="Red"/>
                                    </DataTrigger>
                                </Style.Triggers>
                            </Style>
                        </Border.Style>
                    </Border>
                    
                    <!-- 内容区 -->
                    <ContentPresenter Margin="8 0" VerticalAlignment="Center"/>
                </DockPanel>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

#### 4. 使用方式

xaml:

```xaml
<local:AlarmItemsControl ItemsSource="{Binding AlarmList}">
    <local:AlarmItemsControl.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Message}"/>
        </DataTemplate>
    </local:AlarmItemsControl.ItemTemplate>
</local:AlarmItemsControl>
```

#### 核心特性对应

- 重写 `GetContainerForItemOverride` 替换默认容器；
- `PrepareContainerForItemOverride` / `ClearContainerForItemOverride` 成对实现，适配 UI 虚拟化；
- 是自定义高性能工业列表控件的标准范式。

------

## 四、工业场景最佳实践总结

1. **数据驱动优先**：始终优先使用 `ItemsSource` + `ObservableCollection<T>`，不手动操作 `Items` 集合，符合 MVVM 架构。
2. **大数据必开虚拟化**：超过 500 条的列表，`ItemsPanel` 必须替换为 `VirtualizingStackPanel` 并开启回收模式。
3. **样式与数据分离**：交替行、状态高亮用样式触发器实现，不要在数据模型里加 UI 相关属性。
4. **自定义容器必配清理**：只要重写了 `PrepareContainerForItemOverride`，就必须对应重写 `ClearContainerForItemOverride`，避免虚拟化滚动时内存泄漏、状态错乱。
5. **分组用官方机制**：集合分组使用 `CollectionViewSource` + `GroupStyle`，不要手动构建嵌套集合，性能与可维护性更优。