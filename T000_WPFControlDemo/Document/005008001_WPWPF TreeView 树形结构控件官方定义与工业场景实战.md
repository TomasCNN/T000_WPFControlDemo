# 005008001_WPWPF `TreeView` 树形结构控件官方定义与工业场景实战

`TreeView` 是 WPF 原生的**分层树形结构展示与交互控件**，直接继承自 `ItemsControl` 集合基类，在集合呈现能力的基础上，通过递归嵌套的 `TreeViewItem` 节点容器，支持无限层级的数据展示与交互，自带展开 / 折叠、节点选中、键盘导航等完整树形操作能力。

它是工业软件中**设备结构树、工艺 BOM 清单、组织权限树、系统导航菜单**的首选控件，核心价值在于将具有层级从属关系的数据直观可视化，支持逐层钻取查看详情。与 `ListBox`/`DataGrid` 等平面列表控件的核心区别是：采用分层数据模板驱动递归渲染，节点本身也是集合容器，可无限嵌套子节点。

------

## 一、官方类定义总览与核心元数据

### 1.1 完整类签名（官方原生定义）

csharp:

```c#
namespace System.Windows.Controls
{
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(TreeViewItem))]
    public class TreeView : ItemsControl
    {
        // 核心依赖属性字段
        public static readonly DependencyProperty SelectedItemProperty;
        public static readonly DependencyProperty SelectedValueProperty;
        public static readonly DependencyProperty SelectedValuePathProperty;

        // 构造函数
        public TreeView();

        // 核心公共属性
        public object SelectedItem { get; }
        public object SelectedValue { get; }
        public string SelectedValuePath { get; set; }
        protected internal override bool HandlesScrolling { get; }

        // 核心事件
        public event RoutedPropertyChangedEventHandler<object> SelectedItemChanged;

        // 受保护重写方法
        protected override DependencyObject GetContainerForItemOverride();
        protected override bool IsItemItsOwnContainerOverride(object item);
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
        protected override void ClearContainerForItemOverride(DependencyObject element, object item);
        protected virtual void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e);
        protected override AutomationPeer OnCreateAutomationPeer();
    }
}
```

### 1.2 核心元数据（官方精确值）

| 项               | 官方精确值                                                   | 工业场景关键说明                                             |
| :--------------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| **命名空间**     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                                         |
| **程序集**       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                                           |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → TreeView` | 直接继承集合控件基类，无单选基类依赖                         |
| **默认节点容器** | `TreeViewItem`                                               | 每个树节点的 UI 容器，继承自 `HeaderedItemsControl`，自身也是集合容器，支持嵌套子节点 |
| **核心设计**     | 递归分层渲染 + 选中冒泡机制 + 原生展开折叠交互               | 支持无限层级嵌套，数据驱动自动生成树结构                     |
| **工业核心场景** | 设备层级结构、工艺 BOM 清单、组织权限树、系统导航菜单        | 所有具有层级从属关系的数据展示与交互                         |

> ⚠️ 关键区别：`TreeView` **不继承自 `Selector`**，这是它与 `ListBox`/`ComboBox`/`TabControl` 最本质的差异。它没有 `SelectionMode`、`SelectedIndex` 等属性，`SelectedItem` 是只读属性，选中逻辑由子节点 `TreeViewItem` 向上冒泡实现。

### 1.3 类级特性解析

**`[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(TreeViewItem))]`**

- 向设计器声明 `ItemContainerStyle` 的目标容器类型为 `TreeViewItem`，提供样式智能提示与编译期类型校验；
- 与 `ListBox`、`ComboBox` 遵循完全一致的 `ItemsControl` 体系约定，保证用法统一。

------

## 二、核心依赖属性全量解析

`TreeView` 自身仅新增 3 个核心依赖属性，其余全部继承自 `ItemsControl` 与 `Control`。

### 2.1 TreeView 新增核心属性

| 属性                | 类型     | 读写性     | 官方作用                                    | 工业场景关键说明                                             |
| :------------------ | :------- | :--------- | :------------------------------------------ | :----------------------------------------------------------- |
| `SelectedItem`      | `object` | **只读**   | 获取当前选中的树节点对应的数据对象          | 最核心属性，**只能读不能直接赋值**，是 MVVM 开发最常见的坑点；要修改选中状态需通过数据模型绑定 `TreeViewItem.IsSelected` |
| `SelectedValue`     | `object` | 只读       | 根据 `SelectedValuePath` 提取的选中节点的值 | 适合只需要获取节点 ID / 编码的场景，无需获取完整数据对象     |
| `SelectedValuePath` | `string` | 可读写     | 指定 `SelectedValue` 取值的属性路径         | 比如设置为 `DeviceCode`，则 `SelectedValue` 自动返回选中节点的设备编号 |
| `HandlesScrolling`  | `bool`   | 受保护内部 | 声明控件自身管理滚动                        | 内置 `ScrollViewer`，不需要外层包裹滚动条                    |

### 2.2 继承的高频核心属性

全部继承自 `ItemsControl`，与 `ListBox` 用法一致，但分层场景下有特殊用法：

| 分类     | 属性                                     | 核心作用                   | 树形场景说明                                                 |
| :------- | :--------------------------------------- | :------------------------- | :----------------------------------------------------------- |
| 数据绑定 | `ItemsSource`                            | 绑定根节点数据源           | 仅绑定根层级，子层级由分层模板的 `ItemsSource` 递归绑定      |
| 节点模板 | `ItemTemplate`                           | 节点内容模板               | 树形场景通常使用 `HierarchicalDataTemplate` 分层模板替代普通 DataTemplate |
| 容器样式 | `ItemContainerStyle`                     | 自定义 `TreeViewItem` 样式 | 控制节点高度、缩进、展开图标、选中态、IsSelected/IsExpanded 绑定 |
| 布局面板 | `ItemsPanel`                             | 根节点布局面板             | 大数据量时替换为 `VirtualizingStackPanel` 开启层级虚拟化     |
| 外观     | `Background` / `BorderBrush` / `Padding` | 控件整体外观               | 适配工业深色 / 浅色主题                                      |

------

## 三、核心事件体系全解析

### 3.1 TreeView 专属事件

| 事件                  | 触发时机               | 典型工业用法                                                 |
| :-------------------- | :--------------------- | :----------------------------------------------------------- |
| `SelectedItemChanged` | 选中节点发生变化时触发 | 主从联动核心：选中设备节点后，右侧加载对应详情、参数、报警等数据；是树形控件最常用的事件 |

### 3.2 节点级事件（`TreeViewItem` 承载）

这些事件属于单个树节点，可通过路由冒泡在 TreeView 层面统一监听：

| 事件                      | 触发时机                  | 典型用法                                                     |
| :------------------------ | :------------------------ | :----------------------------------------------------------- |
| `Expanded`                | 节点展开时触发            | 懒加载子节点：展开时才异步加载子级数据，避免一次性加载全量树数据 |
| `Collapsed`               | 节点折叠时触发            | 清理子节点资源、取消数据订阅                                 |
| `Selected` / `Unselected` | 节点选中 / 取消选中时触发 | 节点级选中逻辑处理                                           |

### 3.3 继承的常用交互事件

- `MouseDoubleClick`：双击节点事件，常用于双击打开详情窗口、展开 / 折叠切换；
- `MouseRightButtonUp`：右键点击事件，用于弹出节点右键菜单（新增、删除、编辑等操作）；
- `KeyDown`：键盘导航事件，支持方向键展开折叠、上下移动选中项。

------

## 四、核心方法逐行解析

### 4.1 受保护容器生命周期方法

完全遵循 `ItemsControl` 的容器生命周期约定，负责树节点的生成、准备与清理。

| 方法                                         | 官方实现                         | 核心作用                                      |
| :------------------------------------------- | :------------------------------- | :-------------------------------------------- |
| `GetContainerForItemOverride()`              | 返回 `new TreeViewItem()`        | 指定树节点的默认容器类型                      |
| `IsItemItsOwnContainerOverride(object item)` | 判断 `item is TreeViewItem`      | 支持 XAML 直接添加静态 `TreeViewItem` 节点    |
| `PrepareContainerForItemOverride(...)`       | 绑定数据、应用样式、同步节点状态 | 节点生成 / 复用时初始化内容，递归处理子节点   |
| `ClearContainerForItemOverride(...)`         | 清理节点状态、解绑数据           | 节点移除 / 虚拟化回收时清理资源，防止状态残留 |

### 4.2 选中事件触发方法

csharp:

```c#
protected virtual void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e);
```

- 节点选中变化冒泡到 TreeView 时调用，触发 `SelectedItemChanged` 公共事件；
- 子类重写可扩展选中逻辑，比如多选、级联选中。

### 4.3 常用操作方法

- `BringIntoView`：继承自 `FrameworkElement`，可将指定节点滚动到可视区域；

- `ItemContainerGenerator.ContainerFromItem(object item)`：根据数据对象获取对应的 `TreeViewItem` 容器；

  > ⚠️ 注意：虚拟化下不可见的节点没有生成容器，该方法会返回 null，工业大数据场景下禁止依赖容器操作数据。

------

## 五、配套核心类型：`TreeViewItem` 节点容器深度解析

`TreeViewItem` 是树的基本节点单元，继承自 `HeaderedItemsControl`，**自身也是一个 ItemsControl**，因此可以无限嵌套子节点，这是树形结构的核心实现基础。

### 5.1 官方精简类定义

csharp:

```c#
namespace System.Windows.Controls
{
    public class TreeViewItem : HeaderedItemsControl
    {
        public static readonly DependencyProperty IsSelectedProperty;
        public static readonly DependencyProperty IsExpandedProperty;

        public bool IsSelected { get; set; }
        public bool IsExpanded { get; set; }

        public event RoutedEventHandler Expanded;
        public event RoutedEventHandler Collapsed;
        public event RoutedEventHandler Selected;
        public event RoutedEventHandler Unselected;

        public void ExpandSubtree();
        public void CollapseSubtree();
        protected virtual void OnExpanded(RoutedEventArgs e);
        protected virtual void OnCollapsed(RoutedEventArgs e);
    }
}
```

### 5.2 核心属性解析

| 属性                    | 类型     | 官方作用         | 绑定价值                                                     |
| :---------------------- | :------- | :--------------- | :----------------------------------------------------------- |
| `IsSelected`            | `bool`   | 当前节点是否选中 | MVVM 选中绑定的核心入口：将该属性绑定到数据模型的对应字段，即可实现 ViewModel 控制节点选中 |
| `IsExpanded`            | `bool`   | 当前节点是否展开 | 控制节点展开折叠状态，绑定到数据模型可持久化展开状态，避免虚拟化下状态丢失 |
| `Header`                | `object` | 节点头显示内容   | 对应分层模板的展示内容                                       |
| `Items` / `ItemsSource` | 集合     | 子节点数据源     | 递归生成子节点的基础，由分层模板自动绑定                     |

### 5.3 核心方法

- `ExpandSubtree()`：递归展开该节点下所有子节点；
- `CollapseSubtree()`：递归折叠该节点下所有子节点。

------

## 六、官方核心工作机制

### 6.1 分层数据模板机制（绑定核心）

这是 TreeView 数据驱动渲染的核心，依赖 `HierarchicalDataTemplate` 分层数据模板实现。

#### 核心原理

- 普通 `DataTemplate` 只能定义单层级的项外观；
- `HierarchicalDataTemplate` 继承自 `DataTemplate`，额外增加了 `ItemsSource` 属性，用于指定子层级的数据源路径；
- 渲染时，框架会递归应用模板：根节点用模板生成外观，同时根据模板的 `ItemsSource` 生成子节点，子节点再复用同一模板，直到没有子数据为止。

#### 典型用法

xaml:

```xaml
<HierarchicalDataTemplate DataType="{x:Type local:DeviceNode}" ItemsSource="{Binding Children}">
    <TextBlock Text="{Binding DeviceName}"/>
</HierarchicalDataTemplate>
```

- `DataType` 指定该模板对应的数据类型；
- `ItemsSource` 指定子节点集合的属性名；
- 自动递归生成所有层级的节点，无需手动写嵌套代码。

### 6.2 选中冒泡机制

TreeView 自身不直接管理选中状态，选中完全由 `TreeViewItem` 驱动：

1. 用户点击节点 → 该 `TreeViewItem` 的 `IsSelected` 变为 true；
2. 触发 `Selected` 路由事件，向上冒泡；
3. 事件冒泡到 TreeView 根节点时，TreeView 更新自身的 `SelectedItem` 只读属性；
4. 触发 `SelectedItemChanged` 事件。

> 🔑 为什么 SelectedItem 是只读的？
>
> 因为选中状态的源头在子节点，TreeView 只是被动接收冒泡结果。如果要通过代码选中节点，必须找到对应的 `TreeViewItem` 设置 `IsSelected`，或者通过样式绑定将 `IsSelected` 绑定到数据模型，直接操作数据层。

### 6.3 展开折叠机制

- 每个节点左侧有展开 / 折叠按钮（ToggleButton），点击切换 `IsExpanded` 属性；
- `IsExpanded = true` 时，创建子节点容器并显示；
- `IsExpanded = false` 时，隐藏子节点容器；
- 支持双击节点头、小键盘加减号切换展开状态。

### 6.4 层级 UI 虚拟化

TreeView 支持**层级虚拟化**，是千级以上节点的性能保障：

- 原理：只生成屏幕可见区域的节点容器，滚出屏幕的节点被回收复用；未展开的节点不会生成子节点容器；
- 开启方式：替换 `ItemsPanel` 为 `VirtualizingStackPanel`，开启虚拟化属性；
- 优势：上千节点的树，内存占用降低 90% 以上，展开和滚动流畅。
- 注意：与平面列表虚拟化不同，TreeView 虚拟化是层级感知的，展开父节点时才会按需生成子节点。

------

## 七、基础使用方法

### 7.1 静态树（固定层级）

适合层级固定、数量少的场景，比如系统菜单。

xaml:

```xaml
<TreeView>
    <TreeViewItem Header="系统设置">
        <TreeViewItem Header="通讯配置"/>
        <TreeViewItem Header="报警配置"/>
        <TreeViewItem Header="用户管理"/>
    </TreeViewItem>
    <TreeViewItem Header="数据查询">
        <TreeViewItem Header="生产记录"/>
        <TreeViewItem Header="报警历史"/>
    </TreeViewItem>
</TreeView>
```

### 7.2 MVVM 动态数据绑定（标准用法）

标准步骤：

1. 定义分层数据模型，包含子节点集合属性；
2. 定义 `HierarchicalDataTemplate` 分层模板，指定子节点数据源；
3. TreeView 的 `ItemsSource` 绑定根节点集合；
4. 通过 `ItemContainerStyle` 绑定 `IsSelected`/`IsExpanded` 到数据模型，实现 MVVM 双向控制。

------

## 八、工业场景实战实例

### 实例 1：工厂设备层级结构树（基础分层绑定 + 选中联动）

#### 场景说明

展示「工厂→车间→产线→设备」四级结构，选中设备节点后右侧显示设备详情，是工业设备管理系统的标准树形导航场景。

#### 1. 分层数据模型

csharp:

```c#
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
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

#### 2. ViewModel

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
                            new DeviceNode { NodeName = "1号产线", NodeCode = "L01",
                                Children = new ObservableCollection<DeviceNode>
                                {
                                    new DeviceNode { NodeName = "喷涂机器人A01", NodeCode = "D001" },
                                    new DeviceNode { NodeName = "喷涂机器人A02", NodeCode = "D002" }
                                }
                            }
                        }
                    }
                }
            }
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
    <local:DeviceTreeViewModel/>
</Window.DataContext>

<Grid Margin="10">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="250"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>

    <!-- 左侧设备树 -->
    <TreeView ItemsSource="{Binding RootNodes}"
              SelectedItemChanged="TreeView_SelectedItemChanged"
              BorderBrush="#DDD" BorderThickness="1">
        
        <!-- 节点容器样式：绑定IsSelected/IsExpanded -->
        <TreeView.ItemContainerStyle>
            <Style TargetType="TreeViewItem">
                <Setter Property="IsSelected" Value="{Binding IsSelected, Mode=TwoWay}"/>
                <Setter Property="IsExpanded" Value="{Binding IsExpanded, Mode=TwoWay}"/>
                <Setter Property="Padding" Value="2 4"/>
            </Style>
        </TreeView.ItemContainerStyle>

        <!-- 分层数据模板 -->
        <TreeView.ItemTemplate>
            <HierarchicalDataTemplate DataType="{x:Type local:DeviceNode}" ItemsSource="{Binding Children}">
                <StackPanel Orientation="Horizontal">
                    <TextBlock Text="{Binding NodeName}" VerticalAlignment="Center"/>
                </StackPanel>
            </HierarchicalDataTemplate>
        </TreeView.ItemTemplate>
    </TreeView>

    <!-- 右侧设备详情 -->
    <Border Grid.Column="1" Margin="10 0 0 0" 
            Background="#F8F9FA" Padding="20" 
            BorderBrush="#DDD" BorderThickness="1">
        <StackPanel DataContext="{Binding SelectedNode}">
            <TextBlock FontSize="18" FontWeight="Bold" Text="{Binding NodeName}"/>
            <TextBlock Margin="0 8" Text="{Binding NodeCode, StringFormat=节点编码：{0}}"/>
            <Separator Margin="0 10"/>
            <TextBlock Text="设备详情区域"/>
        </StackPanel>
    </Border>
</Grid>
```

#### 4. 选中事件同步

csharp:

```c#
private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
{
    var vm = DataContext as DeviceTreeViewModel;
    if (vm != null && e.NewValue is DeviceNode node)
    {
        vm.SelectedNode = node;
    }
}
```

> 说明：因为 `SelectedItem` 是只读的，无法直接双向绑定，所以通过事件中转同步到 ViewModel，这是 MVVM 下的标准解决方案。

------

### 实例 2：带状态指示的工艺 BOM 树（自定义节点样式）

#### 场景说明

展示产品工艺 BOM 结构，每个节点带状态指示灯（正常 / 异常），直观展示各工序的物料齐套状态，是生产工艺管理的典型场景。

xaml:

```xaml
<TreeView ItemsSource="{Binding BomList}"
          BorderBrush="#DDD" BorderThickness="1"
          Width="300">
    <TreeView.ItemContainerStyle>
        <Style TargetType="TreeViewItem">
            <Setter Property="IsExpanded" Value="True"/>
            <Setter Property="Padding" Value="2 4"/>
        </Style>
    </TreeView.ItemContainerStyle>

    <TreeView.ItemTemplate>
        <HierarchicalDataTemplate DataType="{x:Type local:BomNode}" ItemsSource="{Binding Children}">
            <DockPanel VerticalAlignment="Center">
                <!-- 状态指示灯 -->
                <Ellipse DockPanel.Dock="Left" Width="8" Height="8" 
                         VerticalAlignment="Center" Margin="0 0 6 0">
                    <Ellipse.Style>
                        <Style TargetType="Ellipse">
                            <Setter Property="Fill" Value="#52C41A"/>
                            <Style.Triggers>
                                <DataTrigger Binding="{Binding Status}" Value="异常">
                                    <Setter Property="Fill" Value="#F5222D"/>
                                </DataTrigger>
                                <DataTrigger Binding="{Binding Status}" Value="缺料">
                                    <Setter Property="Fill" Value="#FAAD14"/>
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

------

### 实例 3：带复选框的设备批量选择树（父子联动）

#### 场景说明

设备批量授权、批量下发参数场景，树节点带复选框，支持全选、子节点全选时父节点自动选中，父节点选中时子节点全部选中，是工业批量操作的常用交互。

#### 核心实现思路

1. 数据模型增加 `IsChecked` 属性；
2. 节点模板加 CheckBox，绑定 `IsChecked`；
3. 选中变化时递归更新父子节点的选中状态。

#### 节点模板核心代码

xaml:

```xaml
<HierarchicalDataTemplate DataType="{x:Type local:DeviceNode}" ItemsSource="{Binding Children}">
    <StackPanel Orientation="Horizontal">
        <CheckBox IsChecked="{Binding IsChecked, Mode=TwoWay}" 
                  VerticalAlignment="Center" Margin="0 0 6 0"/>
        <TextBlock Text="{Binding NodeName}" VerticalAlignment="Center"/>
    </StackPanel>
</HierarchicalDataTemplate>
```

> 父子联动逻辑在数据模型的 `IsChecked` 属性 setter 中实现：选中父节点递归选中所有子节点，子节点状态变化时更新父节点的选中状态（全选 / 半选 / 未选）。

------

### 实例 4：千级节点高性能树（层级虚拟化）

#### 场景说明

全厂设备结构树有上千个节点，开启层级虚拟化保证滚动流畅、内存占用低。

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
        <HierarchicalDataTemplate DataType="{x:Type local:DeviceNode}" ItemsSource="{Binding Children}">
            <TextBlock Text="{Binding NodeName}" Height="24"/>
        </HierarchicalDataTemplate>
    </TreeView.ItemTemplate>
</TreeView>
```

#### 性能说明

- 开启虚拟化后，无论总节点数多少，内存中只生成可见的节点容器；
- 未展开的节点不会生成子节点，大幅降低初始加载内存；
- 固定节点高度时虚拟化性能最优，避免逐行计算高度。

------

## 九、最佳实践与常见坑点

### 9.1 工业场景最佳实践

1. **MVVM 选中绑定用样式中转**
   - 不要试图直接双向绑定 `TreeView.SelectedItem`，它是只读的；
   - 通过 `ItemContainerStyle` 将 `TreeViewItem.IsSelected` 绑定到数据模型的属性，配合 `SelectedItemChanged` 事件同步到 ViewModel。
2. **大数据量必开层级虚拟化**
   - 节点数 > 200 时建议开启虚拟化，替换 `ItemsPanel` 为 `VirtualizingStackPanel`；
   - 尽量固定节点高度，提升虚拟化性能与滚动流畅度。
3. **子节点懒加载**
   - 层级深、数据量大的树，不要一次性加载全量数据；
   - 监听 `Expanded` 事件，展开节点时才异步加载子节点，大幅提升初始加载速度。
4. **永远操作数据层**
   - 不要遍历 `TreeViewItem` 容器获取或修改数据，虚拟化下不可见节点没有容器；
   - 所有增删改查、选中、展开操作都直接操作数据模型，UI 会自动同步。
5. **状态持久化到数据模型**
   - `IsSelected`、`IsExpanded` 等状态全部绑定到数据模型，避免虚拟化滚动时状态丢失。

### 9.2 常见坑点

1. **SelectedItem 不能双向绑定**
   - 最常见的误区：`SelectedItem` 是只读依赖属性，直接 `{Binding SelectedNode, Mode=TwoWay}` 会报错或不生效。
2. **虚拟化下找不到节点容器**
   - 现象：`ContainerFromItem` 返回 null；
   - 原因：不可见 / 未展开的节点没有生成 UI 容器；
   - 解决：所有逻辑基于数据层，不要依赖 UI 容器。
3. **展开状态滚动后丢失**
   - 现象：滚动出去再滚回来，节点展开状态变了；
   - 原因：虚拟化回收容器后，状态没有持久化；
   - 解决：将 `IsExpanded` 绑定到数据模型。
4. **原生没有节点连接线**
   - WPF 原生 TreeView 没有 WinForm 那种点线连接的样式，需要自定义 `TreeViewItem` 模板实现，属于样式定制范畴。

------

## 总结

`TreeView` 是 WPF `ItemsControl` 体系在分层数据场景的经典延伸：它通过递归嵌套的 `TreeViewItem` 实现无限层级渲染，通过分层数据模板实现纯数据驱动的树形结构生成，通过选中冒泡机制实现统一的选中管理。它的核心设计思想是**节点即容器**—— 每个树节点本身就是一个集合控件，天然支持层级嵌套。理解分层模板、选中机制、层级虚拟化三个核心点，就能应对绝大多数工业树形场景的需求。