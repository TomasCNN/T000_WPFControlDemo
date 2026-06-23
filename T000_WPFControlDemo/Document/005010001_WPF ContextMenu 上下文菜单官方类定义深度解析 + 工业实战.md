# 005010001_WPF `ContextMenu` 上下文菜单官方类定义深度解析 + 工业实战

`ContextMenu` 是 WPF 原生的右键弹出式菜单控件，与 `Menu` 同属 `MenuBase` 抽象基类体系，完全复用 `MenuItem` 作为菜单项载体，核心能力与主菜单完全一致。两者的本质区别在于承载形式：`ContextMenu` 寄宿在独立 `Popup` 悬浮窗口中，右键点击目标控件时在鼠标位置弹出，是工业软件中设备快捷操作、数据批量处理、表单快捷编辑的核心交互控件。

其最核心的底层特性是**视觉树隔离**：菜单位于独立窗口中，不继承主界面的数据上下文，这也是 MVVM 绑定时最容易踩坑的根源。

------

## 一、官方类定义总览与核心元数据

### 1.1 完整类签名（官方原生定义）

csharp:

```c#
namespace System.Windows.Controls
{
    public class ContextMenu : MenuBase
    {
        // 特有依赖属性字段
        public static readonly DependencyProperty IsOpenProperty;
        public static readonly DependencyProperty PlacementProperty;
        public static readonly DependencyProperty PlacementTargetProperty;
        public static readonly DependencyProperty PlacementRectangleProperty;
        public static readonly DependencyProperty HorizontalOffsetProperty;
        public static readonly DependencyProperty VerticalOffsetProperty;
        public static readonly DependencyProperty StaysOpenProperty;
        public static readonly DependencyProperty HasDropShadowProperty;

        // 构造函数
        public ContextMenu();

        // 核心公共属性
        public bool IsOpen { get; set; }
        public PlacementMode Placement { get; set; }
        public UIElement PlacementTarget { get; set; }
        public Rect PlacementRectangle { get; set; }
        public double HorizontalOffset { get; set; }
        public double VerticalOffset { get; set; }
        public bool StaysOpen { get; set; }
        public bool HasDropShadow { get; set; }

        // 核心公共事件
        public event RoutedEventHandler Opened;
        public event RoutedEventHandler Closed;

        // 受保护重写方法
        protected override DependencyObject GetContainerForItemOverride();
        protected override bool IsItemItsOwnContainerOverride(object item);
        protected virtual void OnOpened(RoutedEventArgs e);
        protected virtual void OnClosed(RoutedEventArgs e);
        protected override AutomationPeer OnCreateAutomationPeer();
    }
}
```

### 1.2 核心元数据（官方精确值）

| 项           | 官方精确值                                                   | 工业场景关键说明                                         |
| :----------- | :----------------------------------------------------------- | :------------------------------------------------------- |
| 命名空间     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                                     |
| 程序集       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                                       |
| 完整继承链   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → MenuBase → ContextMenu` | 与 `Menu` 平级，共享菜单体系全部能力                     |
| 默认条目容器 | `MenuItem`                                                   | 与主菜单完全复用，支持图标、复选、命令、子菜单等全部特性 |
| 承载宿主     | `Popup` 独立悬浮窗口                                         | 不在主界面视觉树内，是绑定问题的根源                     |
| 核心设计     | 右键触发 + 悬浮弹出 + 自动边界适配                           | 快捷操作入口，不占用固定界面空间                         |
| 工业核心场景 | 设备列表右键操作、表格行快捷编辑、控件功能菜单、批量操作入口 | 所有高频快捷操作场景                                     |

### 1.3 继承的类级特性

`ContextMenu` 继承了 `MenuBase` 的样式契约特性：

csharp:

```c#
[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(MenuItem))]
```

- 向设计器声明 `ItemContainerStyle` 的目标类型为 `MenuItem`，提供智能提示与编译期校验；
- 所有适用于主菜单的 `MenuItem` 样式、模板，可直接复用到右键菜单。

------

## 二、核心依赖属性全量解析

`ContextMenu` 的属性分为两类：**菜单体系继承属性**（与 `Menu` 完全一致，如 `ItemsSource`、`ItemContainerStyle`）和**弹出定位特有属性**（右键菜单核心能力）。

### 2.1 ContextMenu 特有核心属性

#### 1. IsOpen

csharp:

```c#
public bool IsOpen { get; set; }
```

- **类型**：`bool`，默认值 `false`
- **官方作用**：控制右键菜单的打开 / 关闭状态，支持双向绑定。
- **典型用法**：
  - 代码中手动控制菜单弹出与收起；
  - 绑定到 ViewModel 实现 MVVM 方式控制菜单显隐。
- **注意**：用户点击菜单外部区域时，菜单会自动关闭并同步更新 `IsOpen` 为 `false`。

#### 2. Placement

csharp:

```c#
public PlacementMode Placement { get; set; }
```

- **类型**：`PlacementMode` 枚举，默认值 `MousePoint`

- **官方作用**：设置菜单弹出的定位模式，工业场景常用值：

  | 枚举值               | 行为说明               | 典型场景                 |
  | :------------------- | :--------------------- | :----------------------- |
  | `MousePoint`（默认） | 鼠标右键点击的位置弹出 | 通用右键菜单             |
  | `Bottom`             | 目标元素底部对齐弹出   | 按钮下拉菜单、筛选器菜单 |
  | `Right`              | 目标元素右侧弹出       | 侧边工具栏扩展菜单       |
  | `Center`             | 目标元素中心弹出       | 长按触发的操作菜单       |

#### 3. PlacementTarget

csharp:

```c#
public UIElement PlacementTarget { get; set; }
```

- **类型**：`UIElement`，默认值为右键点击的目标控件
- **官方作用**：菜单定位的参考目标元素，所有位置计算都基于该元素。
- **MVVM 核心价值**：这是解决右键菜单数据绑定问题的唯一桥梁。由于菜单不在主视觉树中，无法直接继承父级 `DataContext`，通过 `PlacementTarget.DataContext` 可以间接获取目标元素的数据上下文，实现命令绑定。

#### 4. PlacementRectangle

csharp:

```c#
public Rect PlacementRectangle { get; set; }
```

- **类型**：`Rect`，默认值 `Rect.Empty`
- **官方作用**：在目标元素内指定一个矩形区域作为定位参考，适合只需要在控件局部区域弹出菜单的场景。

#### 5. HorizontalOffset / VerticalOffset

csharp:

```c#
public double HorizontalOffset { get; set; }
public double VerticalOffset { get; set; }
```

- **类型**：`double`，默认值 0
- **官方作用**：在定位基础上额外的像素偏移量，用于微调菜单位置，适配自定义样式。

#### 6. StaysOpen

csharp:

```c#
public bool StaysOpen { get; set; }
```

- **类型**：`bool`，默认值 `false`
- **官方作用**：点击菜单外部区域时，是否保持菜单打开状态。
- **适用场景**：
  - 默认 `false`：点击外部自动关闭，符合常规右键菜单交互习惯；
  - 设为 `true`：需要手动调用 `IsOpen=false` 才会关闭，适合常驻工具栏、多选操作菜单等场景。

#### 7. HasDropShadow

csharp:

```c#
public bool HasDropShadow { get; set; }
```

- **类型**：`bool`，默认值由系统主题决定
- **官方作用**：控制菜单是否显示投影效果，自定义主题时可关闭以适配深色工业界面。

### 2.2 继承的高频常用属性

全部继承自 `MenuBase` 与 `ItemsControl`，与 `Menu` 用法完全一致：

| 分类     | 属性                                        | 作用                         |
| :------- | :------------------------------------------ | :--------------------------- |
| 数据绑定 | `ItemsSource`                               | 绑定菜单集合，动态生成菜单项 |
| 样式模板 | `ItemContainerStyle`                        | 自定义 `MenuItem` 样式       |
| 分层模板 | `ItemTemplate` + `HierarchicalDataTemplate` | 生成多级子菜单               |
| 外观     | `Background` / `Foreground` / `FontSize`    | 控制菜单整体外观             |

------

## 三、核心事件体系

### 3.1 生命周期事件

| 事件     | 触发时机           | 典型工业用法                                                 |
| :------- | :----------------- | :----------------------------------------------------------- |
| `Opened` | 菜单完全弹出后触发 | 子菜单懒加载：弹出时异步加载动态菜单项，避免初始化时加载大量数据 |
| `Closed` | 菜单完全关闭后触发 | 清理临时资源、取消订阅、保存临时状态                         |

### 3.2 菜单项路由事件

`MenuItem` 的 `Click`、`Checked`、`SubmenuOpened` 等事件均为冒泡路由事件，可直接在 `ContextMenu` 层级统一监听处理，适合批量菜单逻辑。

------

## 四、核心方法解析

### 4.1 条目容器生命周期方法

csharp:

```c#
protected override DependencyObject GetContainerForItemOverride();
protected override bool IsItemItsOwnContainerOverride(object item);
```

- 与 `Menu` 完全一致：默认返回 `new MenuItem()` 作为条目容器；
- 设计意义：菜单项体系完全复用，所有 `MenuItem` 的特性（图标、复选、命令、子菜单）在右键菜单中 100% 可用，学习成本为零。

### 4.2 状态触发虚方法

csharp:

```c#
protected virtual void OnOpened(RoutedEventArgs e);
protected virtual void OnClosed(RoutedEventArgs e);
```

- 菜单弹出 / 关闭的核心入口，触发对应公共事件；
- 子类扩展点：重写可实现弹出前权限校验、关闭后资源回收等自定义逻辑。

### 4.3 自动化支持

csharp:

```c#
protected override AutomationPeer OnCreateAutomationPeer();
```

- 返回 `ContextMenuAutomationPeer`，提供 UI 自动化支持，适配无障碍访问与自动化测试。

------

## 五、核心底层工作机制

### 5.1 Popup 独立窗口承载（视觉树隔离的根源）

`ContextMenu` 的内容本质是寄宿在 `Popup` 控件中，而 `Popup` 会创建独立的 Win32 窗口来承载内容，因此：

1. **不在主界面视觉树内**：菜单的视觉元素与主窗口分属两个窗口，无法通过父级元素继承 `DataContext`；
2. **层级独立**：菜单始终悬浮在窗口最顶层，不会被其他控件遮挡；
3. **绑定必须中转**：所有数据绑定都需要通过 `PlacementTarget` 作为桥梁，间接获取主视觉树的数据。

> 🔑 这就是「右键菜单命令绑定失效」的底层原因：不是绑定语法错了，是数据源根本不在同一个视觉树里。

### 5.2 自动定位与边界检测

1. 弹出时自动计算位置，基于 `Placement` 和 `PlacementTarget` 计算坐标；
2. 自动检测屏幕边界：如果按默认方向弹出会超出屏幕，则自动反向弹出（如下方空间不足就向上弹）；
3. 支持多显示器场景，保证菜单始终显示在当前屏幕内。

### 5.3 菜单体系复用机制

- 完全复用 `MenuItem` 作为条目单元，支持多级子菜单、复选、命令、图标等全部能力；
- 共享 `MenuBase` 的容器生成、样式应用、事件路由逻辑；
- 主菜单的样式、模板、数据模型，可无缝迁移到右键菜单。

------

## 六、标准使用方法

### 6.1 基础用法：静态右键菜单

通过 `FrameworkElement.ContextMenu` 属性关联，所有继承自 `FrameworkElement` 的控件（Grid、Border、ListBoxItem 等）都支持。

xaml:

```xaml
<Border Background="Transparent" Width="200" Height="120" BorderBrush="#DDD" BorderThickness="1">
    <!-- 给 Border 区域添加右键菜单 -->
    <Border.ContextMenu>
        <ContextMenu>
            <MenuItem Header="刷新数据"/>
            <MenuItem Header="导出报表"/>
            <Separator/>
            <MenuItem Header="全屏显示" IsCheckable="True"/>
        </ContextMenu>
    </Border.ContextMenu>
    <TextBlock Text="右键点击此区域" HorizontalAlignment="Center" VerticalAlignment="Center"/>
</Border>
```

> ⚠️ 注意：容器必须设置背景（`Transparent` 也可以），背景为 `null` 时无法接收鼠标事件，右键不会弹出菜单。

### 6.2 代码手动控制弹出

csharp:

```c#
// 手动在指定位置弹出右键菜单
MyContextMenu.PlacementTarget = TargetElement;
MyContextMenu.Placement = PlacementMode.Bottom;
MyContextMenu.IsOpen = true;
```

### 6.3 MVVM 命令绑定（核心标准方案）

通过 `PlacementTarget.DataContext` 中转数据上下文，是工业 MVVM 项目的标准写法。

#### 场景：列表项右键菜单，命令在 ViewModel 中

xaml:

```xaml
<ListBox.ItemContainerStyle>
    <Style TargetType="ListBoxItem">
        <Setter Property="ContextMenu">
            <Setter.Value>
                <ContextMenu>
                    <!-- 
                        绑定路径说明：
                        1. 向上找到 ContextMenu 本身
                        2. 通过 PlacementTarget 获取右键点击的 ListBoxItem
                        3. ListBoxItem.DataContext 就是单条设备数据
                        4. 若命令在全局ViewModel，可向上找ListBox的DataContext
                    -->
                    <MenuItem Header="启动设备"
                              Command="{Binding PlacementTarget.DataContext.StartDeviceCommand, 
                                        RelativeSource={RelativeSource AncestorType=ContextMenu}}"
                              CommandParameter="{Binding}"/>
                </ContextMenu>
            </Setter.Value>
        </Setter>
    </Style>
</ListBox.ItemContainerStyle>
```

### 6.4 动态数据驱动菜单

菜单项完全由数据集合生成，配合 `HierarchicalDataTemplate` 支持多级子菜单，适合权限动态菜单、配置化菜单。

xaml:

```xaml
<ContextMenu ItemsSource="{Binding PlacementTarget.DataContext.ContextMenuItems, 
                          RelativeSource={RelativeSource AncestorType=ContextMenu}}">
    <ContextMenu.ItemTemplate>
        <HierarchicalDataTemplate DataType="{x:Type local:MenuCommandItem}"
                                  ItemsSource="{Binding Children}">
            <TextBlock Text="{Binding MenuText}"/>
        </HierarchicalDataTemplate>
    </ContextMenu.ItemTemplate>
</ContextMenu>
```

------

## 七、工业场景实战实例：设备列表右键快捷操作菜单

### 场景说明

设备监控列表，右键单台设备弹出操作菜单，包含启动、停止、查看详情、导出记录；根据设备运行状态动态显隐启动 / 停止按钮；采用 MVVM 命令架构，传递设备对象参数。

### 1. 设备数据模型

csharp:

```c#
public class DeviceInfo : INotifyPropertyChanged
{
    private string _deviceName;
    public string DeviceName
    {
        get => _deviceName;
        set { _deviceName = value; OnPropertyChanged(); }
    }

    private string _deviceCode;
    public string DeviceCode
    {
        get => _deviceCode;
        set { _deviceCode = value; OnPropertyChanged(); }
    }

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set { _isRunning = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### 2. ViewModel 与命令

csharp:

```c#
public class DeviceMonitorViewModel : INotifyPropertyChanged
{
    public ObservableCollection<DeviceInfo> DeviceList { get; set; }
    public ICommand StartDeviceCommand { get; }
    public ICommand StopDeviceCommand { get; }
    public ICommand ViewDetailCommand { get; }

    public DeviceMonitorViewModel()
    {
        DeviceList = new ObservableCollection<DeviceInfo>
        {
            new() { DeviceName = "喷涂机器人A01", DeviceCode = "D001", IsRunning = true },
            new() { DeviceName = "固化炉B01", DeviceCode = "D002", IsRunning = false },
            new() { DeviceName = "上料机C01", DeviceCode = "D003", IsRunning = true }
        };

        StartDeviceCommand = new RelayCommand(ExecuteStartDevice);
        StopDeviceCommand = new RelayCommand(ExecuteStopDevice);
        ViewDetailCommand = new RelayCommand(ExecuteViewDetail);
    }

    private void ExecuteStartDevice(object parameter)
    {
        if (parameter is DeviceInfo device)
            device.IsRunning = true;
    }

    private void ExecuteStopDevice(object parameter)
    {
        if (parameter is DeviceInfo device)
            device.IsRunning = false;
    }

    private void ExecuteViewDetail(object parameter)
    {
        // 打开设备详情窗口
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### 3. 完整 XAML 界面

xaml:

```xaml
<Window x:Class="ContextMenuDemo.DeviceListWindow"
        Title="设备监控列表" Height="450" Width="650"
        xmlns:local="clr-namespace:ContextMenuDemo">
    <Window.DataContext>
        <local:DeviceMonitorViewModel/>
    </Window.DataContext>

    <Grid Margin="12">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="260"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>

        <!-- 左侧设备列表 -->
        <ListBox ItemsSource="{Binding DeviceList}"
                 SelectedItem="{Binding SelectedDevice}"
                 BorderBrush="#DDD" BorderThickness="1">
            <ListBox.ItemContainerStyle>
                <Style TargetType="ListBoxItem">
                    <Setter Property="Padding" Value="8 4"/>
                    <!-- 右键菜单 -->
                    <Setter Property="ContextMenu">
                        <Setter.Value>
                            <ContextMenu>
                                <!-- 启动设备：停机时显示 -->
                                <MenuItem Header="启动设备"
                                          Command="{Binding PlacementTarget.DataContext.StartDeviceCommand, 
                                                    RelativeSource={RelativeSource AncestorType=ContextMenu}}"
                                          CommandParameter="{Binding}">
                                    <MenuItem.Icon>
                                        <Ellipse Width="12" Height="12" Fill="#52C41A" VerticalAlignment="Center"/>
                                    </MenuItem.Icon>
                                    <MenuItem.Style>
                                        <Style TargetType="MenuItem">
                                            <Setter Property="Visibility" Value="Visible"/>
                                            <Style.Triggers>
                                                <DataTrigger Binding="{Binding IsRunning}" Value="True">
                                                    <Setter Property="Visibility" Value="Collapsed"/>
                                                </DataTrigger>
                                            </Style.Triggers>
                                        </Style>
                                    </MenuItem.Style>
                                </MenuItem>

                                <!-- 停止设备：运行时显示 -->
                                <MenuItem Header="停止设备"
                                          Command="{Binding PlacementTarget.DataContext.StopDeviceCommand, 
                                                    RelativeSource={RelativeSource AncestorType=ContextMenu}}"
                                          CommandParameter="{Binding}">
                                    <MenuItem.Icon>
                                        <Ellipse Width="12" Height="12" Fill="#FAAD14" VerticalAlignment="Center"/>
                                    </MenuItem.Icon>
                                    <MenuItem.Style>
                                        <Style TargetType="MenuItem">
                                            <Setter Property="Visibility" Value="Collapsed"/>
                                            <Style.Triggers>
                                                <DataTrigger Binding="{Binding IsRunning}" Value="True">
                                                    <Setter Property="Visibility" Value="Visible"/>
                                                </DataTrigger>
                                            </Style.Triggers>
                                        </Style>
                                    </MenuItem.Style>
                                </MenuItem>

                                <Separator/>

                                <MenuItem Header="查看详情"
                                          Command="{Binding PlacementTarget.DataContext.ViewDetailCommand, 
                                                    RelativeSource={RelativeSource AncestorType=ContextMenu}}"
                                          CommandParameter="{Binding}"/>
                                <MenuItem Header="导出运行记录"/>
                            </ContextMenu>
                        </Setter.Value>
                    </Setter>
                </Style>
            </ListBox.ItemContainerStyle>

            <ListBox.ItemTemplate>
                <DataTemplate>
                    <DockPanel>
                        <Ellipse DockPanel.Dock="Left" Width="8" Height="8" 
                                 VerticalAlignment="Center" Margin="0 0 8 0">
                            <Ellipse.Style>
                                <Style TargetType="Ellipse">
                                    <Setter Property="Fill" Value="#BFBFBF"/>
                                    <Style.Triggers>
                                        <DataTrigger Binding="{Binding IsRunning}" Value="True">
                                            <Setter Property="Fill" Value="#52C41A"/>
                                        </DataTrigger>
                                    </Style.Triggers>
                                </Style>
                            </Ellipse.Style>
                        </Ellipse>
                        <TextBlock Text="{Binding DeviceName}"/>
                    </DockPanel>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>

        <!-- 右侧设备详情 -->
        <Border Grid.Column="1" Margin="10 0 0 0"
                Background="#F8F9FA" Padding="20"
                BorderBrush="#DDD" BorderThickness="1">
            <StackPanel DataContext="{Binding SelectedDevice}">
                <TextBlock FontSize="18" FontWeight="Bold" Text="{Binding DeviceName}"/>
                <TextBlock Margin="0 8" Text="{Binding DeviceCode, StringFormat=设备编码：{0}}"/>
                <TextBlock Text="{Binding IsRunning, StringFormat=运行状态：{0}}"/>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```

------

## 八、常见坑点与最佳实践

### 8.1 常见坑点

1. **右键点击没反应**
   - 原因：目标控件背景为 `null`，不接收鼠标事件；
   - 解决：设置 `Background="Transparent"`。
2. **命令绑定无效果**
   - 原因：ContextMenu 在独立 Popup 窗口，不在主视觉树，无法继承 DataContext；
   - 解决：通过 `PlacementTarget.DataContext` 中转绑定。
3. **菜单项持续灰显**
   - 原因 1：命令 `CanExecute` 返回 false；
   - 原因 2：绑定路径错误，Command 实际为 null；
   - 排查：查看 VS 输出窗口的绑定错误信息。

### 8.2 工业场景最佳实践

1. **高频操作下沉到右键**：设备启停、详情查看、数据导出等高频操作放到右键菜单，缩短操作路径，提升工控效率。
2. **状态驱动菜单项显隐**：通过数据触发器动态显示 / 隐藏菜单项，不要在代码中手动增删，保持 MVVM 架构整洁。
3. **大菜单懒加载**：配方、产品等数据量大的子菜单，在 `Opened` 事件中异步加载，优化界面启动速度。
4. **统一命令架构**：所有菜单操作统一使用 `ICommand`，便于权限管控、单元测试和逻辑复用。
5. **工业主题适配**：深色工控模式下自定义菜单样式，关闭默认投影，避免与深色界面反差过大造成视觉疲劳。