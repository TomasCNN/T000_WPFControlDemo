# 005007001_ WPF `TabControl` 标签页控件官方定义与工业场景实战

`TabControl` 是 WPF 标准的**多页面切换容器控件**，直接继承自 `Selector` 单选基类，在 `ItemsControl` 集合呈现 + `Selector` 单选管理的基础上，实现「标签头 + 内容区」的折叠式多页布局：同一时间只显示一个页面的内容，通过顶部 / 侧边的标签头快速切换，在有限的界面空间内组织大量分类内容。

它是工业软件中设备详情、系统设置、报表中心、参数配置等场景的标准布局控件，核心价值是**用标签分类替代多窗口弹窗**，让操作更聚焦、界面更整洁。

------

## 一、官方类定义总览与核心元数据

### 1.1 完整类签名（官方原生定义）

csharp:

```c#
namespace System.Windows.Controls
{
    [System.Windows.StyleTypedPropertyAttribute(Property = "ItemContainerStyle", StyleTargetType = typeof(System.Windows.Controls.TabItem))]
    public class TabControl : System.Windows.Controls.Primitives.Selector
    {
        // 核心依赖属性字段
        public static readonly System.Windows.DependencyProperty TabStripPlacementProperty;
        public static readonly System.Windows.DependencyProperty SelectedContentProperty;
        public static readonly System.Windows.DependencyProperty SelectedContentTemplateProperty;
        public static readonly System.Windows.DependencyProperty SelectedContentTemplateSelectorProperty;
        public static readonly System.Windows.DependencyProperty SelectedContentStringFormatProperty;

        // 构造函数
        public TabControl();

        // 核心公共属性
        public System.Windows.Controls.Dock TabStripPlacement { get; set; }
        public object SelectedContent { get; }
        public System.Windows.DataTemplate SelectedContentTemplate { get; }
        public System.Windows.Controls.DataTemplateSelector SelectedContentTemplateSelector { get; }
        public string SelectedContentStringFormat { get; }

        // 受保护重写方法
        protected override System.Windows.DependencyObject GetContainerForItemOverride();
        protected override bool IsItemItsOwnContainerOverride(object item);
        protected override void PrepareContainerForItemOverride(System.Windows.DependencyObject element, object item);
        protected override void OnSelectionChanged(System.Windows.Controls.SelectionChangedEventArgs e);
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer();
    }
}
```

### 1.2 核心元数据（官方精确值）

| 项               | 官方精确值                                                   | 工业场景关键说明                                             |
| :--------------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| **命名空间**     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                                         |
| **程序集**       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                                           |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → Selector → TabControl` | 完整继承集合呈现、单选管理、容器生命周期全部能力             |
| **默认条目容器** | `TabItem`                                                    | 每个标签页的容器，继承自 `HeaderedContentControl`，自带「标签头 + 内容」双部分 |
| **核心设计**     | 单选驱动的多页容器：同一时间仅渲染选中页的内容，通过标签头切换 | 分类组织大量内容，节省界面空间                               |
| **工业核心场景** | 设备详情分页面、系统参数分类配置、多报表切换、多设备并行监控 | 所有需要按类别拆分界面的业务场景                             |

### 1.3 类级特性深度解析

**`[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(TabItem))]`**

- 向设计器声明 `ItemContainerStyle` 的目标容器类型为 `TabItem`，提供样式智能提示与编译期类型校验；
- 与 `ListBox`、`ComboBox` 遵循完全一致的 `ItemsControl` 体系约定，保证用法统一。

------

## 二、核心依赖属性全量解析

`TabControl` 自身新增 5 个核心依赖属性，其余全部继承自 `ItemsControl` 与 `Selector`。

### 2.1 新增核心属性

表格

| 属性                              | 类型                   | 默认值 | 官方作用                                              | 工业场景价值                                                 |
| :-------------------------------- | :--------------------- | :----- | :---------------------------------------------------- | :----------------------------------------------------------- |
| `TabStripPlacement`               | `Dock` 枚举            | `Top`  | 标签条的停靠位置：`Top` / `Bottom` / `Left` / `Right` | 工业系统常用 `Left` 左侧垂直标签，做成侧边导航式布局，操作路径更短 |
| `SelectedContent`                 | `object`               | 只读   | 获取当前选中标签页的内容对象                          | 代码中获取当前页的内容控件或数据上下文                       |
| `SelectedContentTemplate`         | `DataTemplate`         | 只读   | 当前选中内容使用的渲染模板                            | 动态内容场景下获取当前模板                                   |
| `SelectedContentTemplateSelector` | `DataTemplateSelector` | 只读   | 当前内容的动态模板选择器                              | 复杂场景下根据数据类型选择不同内容模板                       |
| `SelectedContentStringFormat`     | `string`               | 只读   | 内容文本的格式化字符串                                | 纯文本内容场景下统一格式化                                   |

> 💡 关键说明：`SelectedContent` 系列属性均为**只读**，由控件内部根据选中的 `TabItem` 自动同步，外部不能直接赋值，只能通过切换选中项间接改变。

### 2.2 继承的高频核心属性

全部继承自 `ItemsControl` 与 `Selector`，是日常开发最常用的配置项：

| 分类         | 属性                                               | 来源         | 核心作用                                       |
| :----------- | :------------------------------------------------- | :----------- | :--------------------------------------------- |
| 数据绑定     | `ItemsSource`                                      | ItemsControl | 绑定标签页数据源，MVVM 动态生成标签            |
| 选择控制     | `SelectedIndex` / `SelectedItem` / `SelectedValue` | Selector     | 控制当前选中的标签，双向绑定联动 ViewModel     |
| 标签头显示   | `DisplayMemberPath`                                | ItemsControl | 指定标签头显示的字段名，简单绑定场景快速使用   |
| 标签头模板   | `ItemTemplate`                                     | ItemsControl | 完全自定义标签头的外观，可加图标、状态灯、按钮 |
| 标签容器样式 | `ItemContainerStyle`                               | ItemsControl | 自定义 `TabItem` 的高度、背景、选中态、字体等  |
| 对齐边距     | `Padding` / `BorderBrush` / `Background`           | Control      | 控制整体外观与内边距                           |

------

## 三、配套核心类型：`TabItem` 标签页容器

`TabItem` 是 `TabControl` 的默认条目容器，继承自 `HeaderedContentControl`，天然分为「标签头（Header）」和「页面内容（Content）」两部分，是标签页的基本单元。

### 3.1 官方精简类定义

csharp:

```c#
public class TabItem : HeaderedContentControl
{
    public static readonly DependencyProperty IsSelectedProperty;

    public bool IsSelected { get; set; }

    protected virtual void OnSelected(RoutedEventArgs e);
    protected virtual void OnUnselected(RoutedEventArgs e);
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e);
}
```

### 3.2 核心成员解析

1. **`IsSelected` 属性**
   - 标记当前标签是否选中，本质是 `Selector.IsSelected` 附加属性的强类型包装；
   - 支持双向绑定，可通过 ViewModel 直接控制标签选中状态。
2. **`OnSelected / OnUnselected` 虚方法**
   - 选中 / 取消选中时触发，子类可重写扩展自定义逻辑，比如选中时加载数据、取消时释放资源。
3. **鼠标交互**
   - 重写左键按下事件，点击标签头时触发选中，自动切换到对应页面。
4. **双内容模型**
   - `Header`：标签头显示的内容，支持文本、图标、自定义控件；
   - `Content`：标签页的主体内容，支持任意 WPF 控件，是页面的主要区域。

------

## 四、核心方法逐行解析

### 4.1 公共方法

`TabControl` 没有新增高频公共方法，通用操作全部通过属性与事件驱动；继承自基类的 `Focus()`、`FindResource()` 等方法照常使用。

### 4.2 受保护重写方法（自定义扩展核心）

#### 1. `GetContainerForItemOverride`

csharp:

```c#
protected override DependencyObject GetContainerForItemOverride();
```

- 官方实现：返回 `new TabItem()`；
- 作用：指定标签页的默认容器类型，将抽象条目具体化为 `TabItem`。

#### 2. `IsItemItsOwnContainerOverride`

csharp:

```c#
protected override bool IsItemItsOwnContainerOverride(object item);
```

- 官方实现：判断 `item is TabItem`；
- 作用：支持 XAML 中直接添加 `<TabItem>` 静态子元素，无需额外包装。

#### 3. `PrepareContainerForItemOverride`

csharp:

```c#
protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
```

- 官方执行流程：
  1. 调用基类方法，完成数据上下文、样式、模板的基础准备；
  2. 同步 `IsSelected` 状态到 `TabItem` 容器；
  3. 绑定标签头与内容的模板。

#### 4. `OnSelectionChanged`

csharp:

```c#
protected override void OnSelectionChanged(SelectionChangedEventArgs e);
```

- 官方扩展逻辑：
  1. 调用基类方法触发 `SelectionChanged` 公共事件；
  2. 更新 `SelectedContent`、`SelectedContentTemplate` 等只读属性；
  3. 卸载旧标签页的内容，加载新选中标签页的内容，触发界面重绘。
- 这是标签切换的核心入口，也是「单页渲染」机制的实现点。

------

## 五、官方核心工作机制

### 5.1 单选驱动的页面切换

- 本质是一个单选 `Selector`：`TabItem` 是选项，选中哪个就显示哪个的内容；
- 同一时间有且仅有一个标签处于选中状态，符合 `Selector` 的单选规则；
- 切换方式：点击标签头、代码修改 `SelectedIndex`/`SelectedItem`、键盘快捷键。

### 5.2 单内容渲染机制

`TabControl` 最核心的设计特点：**同一时间只在视觉树中保留选中页的内容**。

1. 切换标签时，旧标签的内容从视觉树中移除（卸载）；
2. 新选中标签的内容被创建并加入视觉树（加载）；
3. 优点：内存占用低，标签再多也不会同时渲染所有页面；
4. 缺点：切换时会重建内容，输入框文本、滚动位置、展开状态会丢失；
5. 工业场景应对：需要保持状态时，将状态保存在 ViewModel 中，或使用扩展的「标签内容缓存」方案。

### 5.3 标签条布局

- 标签条由 `TabPanel` 布局面板负责排列，支持多行换行、溢出滚动；
- 通过 `TabStripPlacement` 可切换停靠方向，实现顶部、底部、左侧、右侧四种标签布局；
- 左侧 / 右侧垂直标签时，标签头默认横向排列，可通过自定义模板旋转文字，实现垂直文字效果。

------

## 六、基础使用方法

### 6.1 静态标签页（固定数量）

适合标签数量固定的场景，直接在 XAML 中声明 `TabItem`，结构最直观。

xaml:

```xaml
<TabControl>
    <TabItem Header="实时监控">
        <!-- 第一页内容 -->
        <TextBlock Text="实时数据监控区域"/>
    </TabItem>
    <TabItem Header="参数配置">
        <!-- 第二页内容 -->
        <TextBlock Text="参数配置表单"/>
    </TabItem>
</TabControl>
```

### 6.2 MVVM 动态绑定（标签动态增减）

适合标签数量动态变化的场景，比如打开多个设备详情页、多文档界面。

xsml:

```xaml
<TabControl ItemsSource="{Binding DeviceTabList}"
            SelectedItem="{Binding SelectedTab}"
            DisplayMemberPath="TabTitle">
    <!-- 可选：自定义内容模板 -->
    <TabControl.ContentTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding DeviceDetail}"/>
        </DataTemplate>
    </TabControl.ContentTemplate>
</TabControl>
```

### 6.3 修改标签位置

xsml:

```xaml
<!-- 左侧垂直标签条 -->
<TabControl TabStripPlacement="Left">
    ...
</TabControl>
```

------

## 七、工业场景实战实例

### 实例 1：设备详情静态标签页（基础用法）

#### 场景说明

设备详情界面分为「实时监控、参数配置、报警记录、维护信息」四个标签页，是工业设备管理系统的标准布局。

xsml:

```xaml
<Window x:Class="TabControlDemo.DeviceDetailWindow"
        Title="设备详情" Height="500" Width="700">
    <Grid Margin="10">
        <TabControl BorderBrush="#DDD" BorderThickness="1">
            <!-- 标签1：实时监控 -->
            <TabItem Header="实时监控">
                <Border Background="#F5F9FF" Padding="20">
                    <StackPanel>
                        <TextBlock FontSize="16" FontWeight="Bold" Text="设备实时运行参数"/>
                        <Separator Margin="0 10"/>
                        <TextBlock Text="当前温度：45.2 ℃"/>
                        <TextBlock Margin="0 5" Text="当前压力：0.35 MPa"/>
                        <TextBlock Text="运行状态：正常运行"/>
                    </StackPanel>
                </Border>
            </TabItem>

            <!-- 标签2：参数配置 -->
            <TabItem Header="参数配置">
                <Border Background="#FAFFFA" Padding="20">
                    <StackPanel Width="300">
                        <TextBlock FontSize="16" FontWeight="Bold" Text="工艺参数设置"/>
                        <Separator Margin="0 10"/>
                        <TextBox Margin="0 5" Text="85.5"/>
                        <TextBox Margin="0 5" Text="0.32"/>
                        <Button Content="保存参数" HorizontalAlignment="Left" Padding="12 4"/>
                    </StackPanel>
                </Border>
            </TabItem>

            <!-- 标签3：报警记录 -->
            <TabItem Header="报警记录">
                <Border Background="#FFF5F5" Padding="20">
                    <ListBox DisplayMemberPath="Message" BorderThickness="0">
                        <ListBoxItem Content="温度超限报警"/>
                        <ListBoxItem Content="压力异常报警"/>
                        <ListBoxItem Content="设备开机提醒"/>
                    </ListBox>
                </Border>
            </TabItem>

            <!-- 标签4：维护信息 -->
            <TabItem Header="维护信息">
                <Border Background="#FFF9F0" Padding="20">
                    <StackPanel>
                        <TextBlock Text="上次维护时间：2024-05-10"/>
                        <TextBlock Margin="0 5" Text="维护人员：张工"/>
                        <TextBlock Text="下次维护时间：2024-08-10"/>
                    </StackPanel>
                </Border>
            </TabItem>
        </TabControl>
    </Grid>
</Window>
```

------

### 实例 2：MVVM 动态多设备标签页

#### 场景说明

主界面可同时打开多台设备的详情标签，支持动态新增、关闭标签，类似浏览器的多标签页模式，适合多设备集中监控场景。

#### 1. 标签页 ViewModel 基类

csharp:

```c#
public class DeviceTabViewModel : INotifyPropertyChanged
{
    public string DeviceCode { get; set; }
    public string TabTitle => $"设备 {DeviceCode}";
    public string DeviceDetail { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

#### 2. 主 ViewModel

csharp:

```c#
public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<DeviceTabViewModel> DeviceTabs { get; set; }
    
    private DeviceTabViewModel _selectedTab;
    public DeviceTabViewModel SelectedTab
    {
        get => _selectedTab;
        set { _selectedTab = value; OnPropertyChanged(); }
    }

    public MainViewModel()
    {
        DeviceTabs = new ObservableCollection<DeviceTabViewModel>
        {
            new DeviceTabViewModel { DeviceCode = "PLC-001", DeviceDetail = "1号设备详情内容" },
            new DeviceTabViewModel { DeviceCode = "PLC-002", DeviceDetail = "2号设备详情内容" }
        };
        SelectedTab = DeviceTabs.First();
    }

    // 新增标签
    public void AddNewTab(string deviceCode)
    {
        var newTab = new DeviceTabViewModel { DeviceCode = deviceCode, DeviceDetail = $"{deviceCode} 详情" };
        DeviceTabs.Add(newTab);
        SelectedTab = newTab;
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
    <local:MainViewModel/>
</Window.DataContext>

<Grid Margin="10">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>

    <Button Grid.Row="0" Content="打开新设备标签" Click="AddTab_Click" HorizontalAlignment="Left" Padding="12 4" Margin="0 0 0 8"/>

    <TabControl Grid.Row="1"
                ItemsSource="{Binding DeviceTabs}"
                SelectedItem="{Binding SelectedTab}"
                DisplayMemberPath="TabTitle"
                BorderBrush="#DDD" BorderThickness="1">
        <!-- 标签内容模板 -->
        <TabControl.ContentTemplate>
            <DataTemplate>
                <Border Padding="20" Background="#F8F9FA">
                    <TextBlock Text="{Binding DeviceDetail}" FontSize="14"/>
                </Border>
            </DataTemplate>
        </TabControl.ContentTemplate>
    </TabControl>
</Grid>
```

------

### 实例 3：自定义标签头（带状态灯 + 关闭按钮）

#### 场景说明

标签头显示设备运行状态指示灯，右侧带关闭按钮，符合工业多标签监控的操作习惯。

xaml:

```xaml
<TabControl ItemsSource="{Binding DeviceTabs}"
            SelectedItem="{Binding SelectedTab}"
            BorderBrush="#DDD" BorderThickness="1">
    
    <!-- 自定义标签头模板 -->
    <TabControl.ItemTemplate>
        <DataTemplate>
            <DockPanel Width="120" Height="28">
                <!-- 关闭按钮 -->
                <Button DockPanel.Dock="Right" Content="×" 
                        Width="18" Height="18" Padding="0" 
                        Click="CloseTab_Click"
                        CommandParameter="{Binding}"
                        Background="Transparent" BorderThickness="0"
                        Foreground="#999" Cursor="Hand"/>
                
                <!-- 状态指示灯 -->
                <Ellipse DockPanel.Dock="Left" Width="8" Height="8" 
                         VerticalAlignment="Center" Margin="0 0 6 0">
                    <Ellipse.Style>
                        <Style TargetType="Ellipse">
                            <Setter Property="Fill" Value="#999"/>
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
                
                <!-- 标签标题 -->
                <TextBlock Text="{Binding TabTitle}" VerticalAlignment="Center" TextTrimming="CharacterEllipsis"/>
            </DockPanel>
        </DataTemplate>
    </TabControl.ItemTemplate>

    <!-- 内容模板 -->
    <TabControl.ContentTemplate>
        <DataTemplate>
            <Border Padding="20">
                <TextBlock Text="{Binding DeviceDetail}"/>
            </Border>
        </DataTemplate>
    </TabControl.ContentTemplate>
</TabControl>
```

#### 关闭标签后台逻辑

csharp:

```c#
private void CloseTab_Click(object sender, RoutedEventArgs e)
{
    var button = sender as Button;
    var tab = button?.DataContext as DeviceTabViewModel;
    if (tab != null)
    {
        var vm = DataContext as MainViewModel;
        vm?.DeviceTabs.Remove(tab);
    }
}
```

------

### 实例 4：左侧垂直标签导航

#### 场景说明

标签条放在左侧，做成侧边导航式布局，适合系统设置、参数分类等页面，操作路径更短，符合工业软件左侧导航的使用习惯。

xaml:

```xaml
<TabControl TabStripPlacement="Left"
            BorderBrush="#DDD" BorderThickness="1"
            Height="400" Width="600">
    
    <!-- 标签容器样式：设置标签宽度 -->
    <TabControl.ItemContainerStyle>
        <Style TargetType="TabItem">
            <Setter Property="Height" Value="40"/>
            <Setter Property="HorizontalContentAlignment" Value="Center"/>
            <Setter Property="VerticalContentAlignment" Value="Center"/>
        </Style>
    </TabControl.ItemContainerStyle>

    <TabItem Header="系统参数">
        <Border Padding="20" Background="#F8F9FA">
            <TextBlock Text="系统参数配置页面内容"/>
        </Border>
    </TabItem>
    <TabItem Header="通讯配置">
        <Border Padding="20" Background="#F8F9FA">
            <TextBlock Text="PLC通讯参数配置"/>
        </Border>
    </TabItem>
    <TabItem Header="报警配置">
        <Border Padding="20" Background="#F8F9FA">
            <TextBlock Text="报警阈值与通知配置"/>
        </Border>
    </TabItem>
    <TabItem Header="用户管理">
        <Border Padding="20" Background="#F8F9FA">
            <TextBlock Text="用户权限与账号管理"/>
        </Border>
    </TabItem>
</TabControl>
```

> 💡 扩展提示：如果需要垂直排列的文字，可以在 `HeaderTemplate` 中使用 `LayoutTransform` 将文本旋转 90 度，实现竖排文字效果。

------

## 八、最佳实践与常见坑点

### 8.1 工业场景最佳实践

1. **固定标签用静态，动态标签用 MVVM**
   - 数量固定的分类页面直接写静态 `TabItem`，结构清晰性能好；
   - 多设备、多文档等动态场景用 `ItemsSource` 绑定，统一管理。
2. **大数据内容懒加载**
   - 不要在窗口初始化时加载所有标签页的数据；
   - 在 `SelectionChanged` 事件中判断，切换到对应标签才加载数据，大幅提升启动速度。
3. **状态保存在 ViewModel**
   - 由于默认切换标签会重建内容，输入框、滚动位置等状态会丢失；
   - 重要状态全部绑定到 ViewModel，切换后自动恢复，避免状态丢失。
4. **标签数量控制**
   - 同时打开的标签不宜过多（建议不超过 10 个），过多会增加用户认知负担；
   - 配合标签关闭功能，让用户自主管理打开的页面。
5. **工业界面优先左侧导航**
   - 宽屏工业显示器，左侧垂直标签导航操作效率更高，符合工业软件的使用习惯。

### 8.2 常见坑点

1. **切换标签内容状态丢失**
   - 现象：输入框填了一半，切走再切回来内容没了；
   - 原因：默认会卸载旧内容，重建新内容；
   - 解决：数据绑定到 ViewModel，或使用自定义 TabControl 扩展实现内容缓存。
2. **数据绑定后标签头显示类名**
   - 现象：标签头显示 `xxx.ViewModel` 类名路径；
   - 原因：没有设置 `DisplayMemberPath` 或 `ItemTemplate`；
   - 解决：指定 `DisplayMemberPath` 或自定义 `ItemTemplate`。
3. **垂直标签文字方向不对**
   - 现象：左侧标签文字还是横向的，标签条很宽；
   - 解决：通过 `HeaderTemplate` + `RotateTransform` 旋转文字。
4. **嵌套过深导致操作繁琐**
   - 现象：TabControl 里又套 TabControl，层级超过 2 层；
   - 建议：尽量扁平化布局，用分组、Expander 替代内层标签，降低操作复杂度。

------

## 总结

`TabControl` 是 `Selector` 单选体系在「多页面容器」场景的经典应用：它复用了完整的集合管理与单选能力，通过 `TabItem` 的头 - 内容双模型，实现了紧凑的多页切换布局。它的核心价值是**用空间换组织**—— 在有限的界面区域内，通过标签分类承载大量业务内容，是工业软件中设备详情、系统配置、报表中心等场景的标准布局控件。