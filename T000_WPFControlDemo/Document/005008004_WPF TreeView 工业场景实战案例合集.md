# 005008004_WPF `TreeView` 工业场景实战案例合集

以下案例全部贴合工业上位机、设备管理、生产工艺等真实业务场景，从基础分层绑定、状态可视化、复选框级联，到懒加载、高性能虚拟化逐步深入，每个案例明确标注对应核心特性，可直接复用到工业项目中。

------

## 案例 1：工厂设备层级导航树（基础 MVVM + 主从联动）

### 场景说明

左侧展示「工厂→车间→产线→设备」四级树形结构，选中设备节点后右侧加载对应设备的实时参数与运行详情，是工业设备管理系统最经典的主从导航布局。

### 对应核心特性

- `HierarchicalDataTemplate` 分层数据模板（递归渲染核心）
- `SelectedItemChanged` 选中变更事件
- `ItemContainerStyle` 绑定 `IsSelected`/`IsExpanded` 状态

#### 1. 分层数据模型

csharp:

```c#
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TreeViewDemo
{
    public class DeviceNode : INotifyPropertyChanged
    {
        private string _nodeName;
        public string NodeName
        {
            get => _nodeName;
            set { _nodeName = value; OnPropertyChanged(); }
        }

        private string _nodeCode;
        public string NodeCode
        {
            get => _nodeCode;
            set { _nodeCode = value; OnPropertyChanged(); }
        }

        private string _deviceType;
        public string DeviceType
        {
            get => _deviceType;
            set { _deviceType = value; OnPropertyChanged(); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        private bool _isExpanded = true;
        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged(); }
        }

        // 子节点集合
        public ObservableCollection<DeviceNode> Children { get; set; } = new ObservableCollection<DeviceNode>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

#### 2. 视图模型

csharp:

```c#
public class DeviceTreeViewModel : INotifyPropertyChanged
{
    public ObservableCollection<DeviceNode> RootNodes { get; set; }

    private DeviceNode _selectedNode;
    public DeviceNode SelectedNode
    {
        get => _selectedNode;
        set { _selectedNode = value; OnPropertyChanged(); }
    }

    public DeviceTreeViewModel()
    {
        // 模拟构建四级设备树
        RootNodes = new ObservableCollection<DeviceNode>
        {
            new DeviceNode
            {
                NodeName = "一号工厂",
                NodeCode = "F01",
                Children = new ObservableCollection<DeviceNode>
                {
                    new DeviceNode
                    {
                        NodeName = "喷涂车间",
                        NodeCode = "W01",
                        Children = new ObservableCollection<DeviceNode>
                        {
                            new DeviceNode
                            {
                                NodeName = "1号喷涂产线",
                                NodeCode = "L01",
                                Children = new ObservableCollection<DeviceNode>
                                {
                                    new DeviceNode { NodeName = "喷涂机器人A01", NodeCode = "D001", DeviceType = "六轴机器人" },
                                    new DeviceNode { NodeName = "喷涂机器人A02", NodeCode = "D002", DeviceType = "六轴机器人" }
                                }
                            }
                        }
                    },
                    new DeviceNode
                    {
                        NodeName = "固化车间",
                        NodeCode = "W02",
                        Children = new ObservableCollection<DeviceNode>
                        {
                            new DeviceNode { NodeName = "固化炉B01", NodeCode = "D003", DeviceType = "热风固化炉" }
                        }
                    }
                }
            }
        };
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

#### 3. XAML 界面

xaml:

```xaml
<Window x:Class="TreeViewDemo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:TreeViewDemo"
        Title="设备结构树" Height="450" Width="700">
    <Window.DataContext>
        <local:DeviceTreeViewModel/>
    </Window.DataContext>

    <Grid Margin="10">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="260"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>

        <!-- 左侧设备树 -->
        <TreeView x:Name="DeviceTree"
                  ItemsSource="{Binding RootNodes}"
                  SelectedItemChanged="DeviceTree_SelectedItemChanged"
                  BorderBrush="#DDD" BorderThickness="1">
            
            <!-- 节点容器样式：绑定选中/展开状态 -->
            <TreeView.ItemContainerStyle>
                <Style TargetType="TreeViewItem">
                    <Setter Property="IsSelected" Value="{Binding IsSelected, Mode=TwoWay}"/>
                    <Setter Property="IsExpanded" Value="{Binding IsExpanded, Mode=TwoWay}"/>
                    <Setter Property="Padding" Value="2 4"/>
                </Style>
            </TreeView.ItemContainerStyle>

            <!-- 分层数据模板：递归渲染所有层级 -->
            <TreeView.ItemTemplate>
                <HierarchicalDataTemplate DataType="{x:Type local:DeviceNode}" 
                                          ItemsSource="{Binding Children}">
                    <TextBlock Text="{Binding NodeName}" VerticalAlignment="Center"/>
                </HierarchicalDataTemplate>
            </TreeView.ItemTemplate>
        </TreeView>

        <!-- 右侧设备详情 -->
        <Border Grid.Column="1" Margin="10 0 0 0" 
                Background="#F8F9FA" Padding="20" 
                BorderBrush="#DDD" BorderThickness="1">
            <StackPanel DataContext="{Binding SelectedNode}">
                <TextBlock FontSize="18" FontWeight="Bold" Text="{Binding NodeName}"/>
                <TextBlock Margin="0 8" Text="{Binding NodeCode, StringFormat=设备编码：{0}}"/>
                <TextBlock Text="{Binding DeviceType, StringFormat=设备类型：{0}}"/>
                <Separator Margin="0 15"/>
                <TextBlock Text="实时运行参数区域" Foreground="#666"/>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```

#### 4. 选中事件同步（MVVM 中转）

csharp:

```c#
private void DeviceTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
{
    if (DataContext is DeviceTreeViewModel vm && e.NewValue is DeviceNode node)
    {
        vm.SelectedNode = node;
    }
}
```

> 说明：因 `TreeView.SelectedItem` 是只读属性，无法直接双向绑定，通过事件中转是 MVVM 架构下的标准方案。

------

## 案例 2：带状态指示的工艺 BOM 树（自定义节点外观）

### 场景说明

展示产品工艺 BOM 层级结构，每个节点前增加状态指示灯（正常 / 缺料 / 异常），生产现场可直观查看各工序的物料齐套状态，是生产工艺管理的典型场景。

### 对应核心特性

- 自定义节点内容模板
- 数据触发器实现状态颜色联动
- 分层模板递归渲染

xaml:

```xaml
<TreeView ItemsSource="{Binding BomList}"
          BorderBrush="#DDD" BorderThickness="1"
          Width="320">
    <TreeView.ItemContainerStyle>
        <Style TargetType="TreeViewItem">
            <Setter Property="IsExpanded" Value="True"/>
            <Setter Property="Padding" Value="2 4"/>
        </Style>
    </TreeView.ItemContainerStyle>

    <TreeView.ItemTemplate>
        <HierarchicalDataTemplate DataType="{x:Type local:BomNode}" 
                                  ItemsSource="{Binding Children}">
            <DockPanel VerticalAlignment="Center">
                <!-- 状态指示灯 -->
                <Ellipse DockPanel.Dock="Left" Width="8" Height="8" 
                         VerticalAlignment="Center" Margin="0 0 6 0">
                    <Ellipse.Style>
                        <Style TargetType="Ellipse">
                            <Setter Property="Fill" Value="#52C41A"/>
                            <Style.Triggers>
                                <DataTrigger Binding="{Binding Status}" Value="缺料">
                                    <Setter Property="Fill" Value="#FAAD14"/>
                                </DataTrigger>
                                <DataTrigger Binding="{Binding Status}" Value="异常">
                                    <Setter Property="Fill" Value="#F5222D"/>
                                </DataTrigger>
                            </Style.Triggers>
                        </Style>
                    </Ellipse.Style>
                </Ellipse>
                <TextBlock Text="{Binding NodeName}"/>
            </DockPanel>
        </HierarchicalDataTemplate>
    </TreeView.ItemTemplate>
</TreeView>
```

#### 配套 BOM 节点模型

csharp:

```c#
public class BomNode : INotifyPropertyChanged
{
    public string NodeName { get; set; }
    public string Status { get; set; } // 正常/缺料/异常
    public ObservableCollection<BomNode> Children { get; set; } = new ObservableCollection<BomNode>();
}
```

------

## 案例 3：带复选框的设备批量选择树（父子级联勾选）

### 场景说明

设备批量授权、批量下发参数场景，树节点带复选框，支持**父子联动**：

- 勾选父节点 → 自动勾选所有子节点
- 取消父节点 → 自动取消所有子节点
- 子节点全部勾选 → 父节点自动勾选
- 子节点部分勾选 → 父节点半选状态

### 对应核心特性

- 复选框节点模板
- 数据驱动级联选中逻辑
- 半选状态支持

#### 1. 支持级联的节点模型

csharp:

```c#
public class CheckableDeviceNode : INotifyPropertyChanged
{
    private bool? _isChecked = false;
    public bool? IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked == value) return;
            _isChecked = value;
            OnPropertyChanged();

            // 向下：更新所有子节点
            if (value.HasValue && Children != null)
            {
                foreach (var child in Children)
                {
                    child.IsChecked = value;
                }
            }

            // 向上：更新父节点状态
            UpdateParentState();
        }
    }

    public string NodeName { get; set; }
    public CheckableDeviceNode Parent { get; set; }
    public ObservableCollection<CheckableDeviceNode> Children { get; set; } = new ObservableCollection<CheckableDeviceNode>();

    private void UpdateParentState()
    {
        if (Parent == null) return;

        var children = Parent.Children;
        bool allTrue = children.All(c => c.IsChecked == true);
        bool allFalse = children.All(c => c.IsChecked == false);

        if (allTrue) Parent._isChecked = true;
        else if (allFalse) Parent._isChecked = false;
        else Parent._isChecked = null;

        Parent.OnPropertyChanged(nameof(IsChecked));
        Parent.UpdateParentState();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

#### 2. XAML 界面

xaml:

```xaml
<TreeView ItemsSource="{Binding DeviceCheckList}"
          BorderBrush="#DDD" BorderThickness="1"
          Width="300">
    <TreeView.ItemContainerStyle>
        <Style TargetType="TreeViewItem">
            <Setter Property="IsExpanded" Value="True"/>
            <Setter Property="Padding" Value="2 3"/>
        </Style>
    </TreeView.ItemContainerStyle>

    <TreeView.ItemTemplate>
        <HierarchicalDataTemplate DataType="{x:Type local:CheckableDeviceNode}"
                                  ItemsSource="{Binding Children}">
            <CheckBox Content="{Binding NodeName}"
                      IsThreeState="True"
                      IsChecked="{Binding IsChecked, Mode=TwoWay}"
                      VerticalAlignment="Center"/>
        </HierarchicalDataTemplate>
    </TreeView.ItemTemplate>
</TreeView>
```

> 关键：`IsThreeState="True"` 开启复选框三态，支持半选（null），完美匹配父子部分选中的场景。

------

## 案例 4：子节点异步懒加载树（按需加载）

### 场景说明

层级深、数据量大的全厂设备树，初始只加载根节点，点击展开时才异步查询并加载子节点，避免一次性加载全量数据导致界面卡顿，是大型设备管理系统的标准性能优化方案。

### 对应核心特性

- `TreeViewItem.Expanded` 冒泡事件
- 异步数据加载
- 占位符提示

#### 1. 懒加载节点模型

csharp:

```c#
public class LazyLoadNode : INotifyPropertyChanged
{
    public string NodeName { get; set; }
    public string NodeCode { get; set; }
    public bool IsLoaded { get; set; } // 是否已加载子节点
    public ObservableCollection<LazyLoadNode> Children { get; set; } = new ObservableCollection<LazyLoadNode>();
}
```

#### 2. XAML 界面

xaml:

```xaml
<TreeView ItemsSource="{Binding LazyRootNodes}"
          TreeViewItem.Expanded="TreeViewItem_Expanded"
          BorderBrush="#DDD" BorderThickness="1"
          Width="280">
    <TreeView.ItemTemplate>
        <HierarchicalDataTemplate DataType="{x:Type local:LazyLoadNode}"
                                  ItemsSource="{Binding Children}">
            <TextBlock Text="{Binding NodeName}"/>
        </HierarchicalDataTemplate>
    </TreeView.ItemTemplate>
</TreeView>
```

#### 3. 异步加载后台逻辑

csharp:

```c#
private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
{
    if (e.OriginalSource is TreeViewItem item && item.DataContext is LazyLoadNode node)
    {
        // 已加载过则跳过
        if (node.IsLoaded) return;

        // 模拟异步加载子节点（实际项目中替换为数据库/接口查询）
        Task.Run(() =>
        {
            // 模拟网络/数据库耗时
            Thread.Sleep(300);
            
            var children = new List<LazyLoadNode>
            {
                new LazyLoadNode { NodeName = $"{node.NodeName}-子节点1" },
                new LazyLoadNode { NodeName = $"{node.NodeName}-子节点2" }
            };

            Dispatcher.Invoke(() =>
            {
                foreach (var child in children)
                {
                    node.Children.Add(child);
                }
                node.IsLoaded = true;
            });
        });
    }
}
```

------

## 案例 5：千级节点高性能树（层级虚拟化）

### 场景说明

全厂上千台设备的结构树，开启层级 UI 虚拟化，保证滚动流畅、内存占用低，解决大数据量下树控件卡顿、内存暴涨的问题。

### 对应核心特性

- `VirtualizingStackPanel` 层级虚拟化
- 容器回收复用
- 延迟滚动优化

xaml:

```xaml
<TreeView ItemsSource="{Binding LargeDeviceTree}"
          VirtualizingStackPanel.IsVirtualizing="True"
          VirtualizingStackPanel.VirtualizationMode="Recycling"
          ScrollViewer.IsDeferredScrollingEnabled="True"
          BorderBrush="#DDD" BorderThickness="1"
          Height="500" Width="300">
    
    <!-- 替换为虚拟化布局面板 -->
    <TreeView.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel/>
        </ItemsPanelTemplate>
    </TreeView.ItemsPanel>

    <TreeView.ItemTemplate>
        <HierarchicalDataTemplate DataType="{x:Type local:DeviceNode}"
                                  ItemsSource="{Binding Children}">
            <TextBlock Text="{Binding NodeName}" Height="24" VerticalAlignment="Center"/>
        </HierarchicalDataTemplate>
    </TreeView.ItemTemplate>
</TreeView>
```

### 性能效果说明

1. 开启虚拟化后，无论总节点数多少，内存中只生成可见区域的节点容器，内存占用降低 90% 以上；
2. `Recycling` 回收模式下，滚动时容器对象复用，大幅减少对象创建与 GC 压力；
3. 延迟滚动开启后，拖动滚动条时仅显示提示，松开后再渲染，大幅提升拖动流畅度；
4. 固定节点高度时虚拟化性能最优，避免逐行计算高度。

------

## 工业场景最佳实践总结

1. **永远基于数据层操作**

   不要遍历 `TreeViewItem` 容器获取 / 修改数据，虚拟化下不可见节点没有容器；所有增删改查、选中、展开操作都直接操作数据模型，UI 会自动同步。

2. **选中绑定用样式中转**

   不要试图直接双向绑定 `TreeView.SelectedItem`（只读），通过 `ItemContainerStyle` 绑定 `TreeViewItem.IsSelected` 到数据模型，配合事件同步到 ViewModel。

3. **大树必做懒加载 + 虚拟化**

   - 层级深、数据量大时，优先做子节点懒加载，展开时才查询数据；
   - 节点数 > 200 时开启层级虚拟化，固定节点高度获得最优性能。

4. **状态持久化到数据模型**

   `IsSelected`、`IsExpanded`、勾选状态全部绑定到数据模型，避免虚拟化滚动、容器复用时状态丢失。

5. **状态可视化提升效率**

   工业场景优先在节点前增加颜色指示灯、图标，让操作人员扫一眼就能掌握全局状态，减少点击操作。