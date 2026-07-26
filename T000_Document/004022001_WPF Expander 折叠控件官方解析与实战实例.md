# 004022001_WPF `Expander` 折叠控件官方解析与实战实例

`Expander` 是 WPF 原生 `System.Windows.Controls` 命名空间下的标准内容控件，定义于 `PresentationFramework.dll`，核心能力是提供「标题头 + 可折叠内容区域」的布局容器，通过点击标题切换内容的显示 / 隐藏，是界面分组、节省布局空间的核心控件，广泛应用于参数配置、侧边导航、详情分组等工业软件场景。

------

## 一、官方类定义深度解析

### 1.1 完整继承体系

plaintext:

```tex
DispatcherObject
  → DependencyObject
    → Visual
      → UIElement
        → FrameworkElement
          → Control
            → ContentControl
              → HeaderedContentControl
                → Expander
```

- 核心定位：继承自 `HeaderedContentControl`，同时具备 `Header`（标题）和 `Content`（内容）双内容模型，标题与内容均支持任意 WPF 元素，不局限于纯文本。
- 控件本质：布局容器类控件，自身不承担输入功能，仅做信息分组与交互承载。

### 1.2 核心依赖属性

| 依赖属性标识符            | 对应 CLR 属性     | 类型                   | 默认值  | 核心说明                                                     |
| :------------------------ | :---------------- | :--------------------- | :------ | :----------------------------------------------------------- |
| `IsExpandedProperty`      | `IsExpanded`      | `bool`                 | `false` | **核心控制属性**，获取或设置内容是否展开。支持双向绑定，值变化时触发 `Expanded` / `Collapsed` 路由事件。 |
| `ExpandDirectionProperty` | `ExpandDirection` | `ExpandDirection` 枚举 | `Down`  | 内容展开方向，支持 4 种模式：- `Down`：向下展开（最常用）- `Up`：向上展开- `Left`：向左展开- `Right`：向右展开 |

#### 继承而来的关键属性

来自 `HeaderedContentControl`：

- `Header`：标题内容，支持文本、图标、复杂控件组合
- `HeaderTemplate`：标题的数据模板，MVVM 场景下自定义标题外观
- `HeaderStringFormat`：标题文本格式化字符串

来自 `ContentControl`：

- `Content`：折叠区域的内容主体，可放置任意控件、布局容器
- `ContentTemplate`：内容的数据模板
- `ContentTemplateSelector`：动态选择内容模板

### 1.3 核心路由事件

| 事件        | 委托类型             | 触发时机           |
| :---------- | :------------------- | :----------------- |
| `Expanded`  | `RoutedEventHandler` | 内容完全展开后触发 |
| `Collapsed` | `RoutedEventHandler` | 内容完全折叠后触发 |

两个事件均为**冒泡路由事件**，可在父容器统一监听多个 Expander 的状态变化。

### 1.4 可重写方法（自定义扩展用）

- `protected virtual void OnExpanded(RoutedEventArgs e)`：展开事件的触发入口，子类重写可在展开前插入业务逻辑
- `protected virtual void OnCollapsed(RoutedEventArgs e)`：折叠事件的触发入口
- `public override void OnApplyTemplate()`：控件模板加载完成后调用，自定义控件时可获取内部 ToggleButton、边框等命名元素

------

## 二、核心功能与工业典型应用场景

### 核心功能

1. **空间复用**：将低频操作、次要信息折叠隐藏，保持主界面简洁高效
2. **信息分组**：按业务维度对表单、参数、数据进行模块化组织
3. **渐进式加载**：支持展开时懒加载数据，减少界面初始化性能开销
4. **多方向适配**：支持上下左右 4 种展开方向，适配侧边栏、顶部栏等不同布局位置

### 工业软件典型场景

1. **设备参数配置页**：按「基础参数、通信参数、运行参数、报警参数」分组折叠
2. **侧边导航栏**：左侧可折叠菜单，收起时仅显示图标，最大化主内容区域
3. **产能报表页**：将「筛选条件、分时段明细、异常统计」分块折叠展示
4. **设备详情页**：折叠展示历史报警、维护记录等次要信息
5. **PLC 寄存器配置**：按功能区分组管理大量寄存器地址配置项

------

## 三、基础使用方法

### 3.1 标准 XAML 结构

最基础的用法是设置 Header 文本，内部放置业务内容：

xaml:

```xaml
<Expander Header="基础参数">
    <!-- 折叠区域内支持任意 WPF 内容 -->
    <StackPanel Margin="10">
        <TextBox Text="设备A01"/>
        <TextBox Text="192.168.1.100"/>
    </StackPanel>
</Expander>
```

- 默认折叠状态（`IsExpanded="False"`），点击标题栏箭头即可切换状态
- 整个标题区域均可点击，不局限于箭头图标

### 3.2 展开方向说明

- **Down/Up（纵向展开）**：最常用，适合页面内垂直排列的分组，宽度自适应父容器
- **Left/Right（横向展开）**：适合侧边栏场景，需要显式设置控件高度，否则内容可能无法正常显示

------

## 四、实战实例（工业场景适配）

### 实例 1：基础分组折叠 - 设备参数配置

最常用的纵向分组场景，将设备参数按类别拆分，界面整洁不拥挤。

xaml:

```xaml
<Window.Resources>
    <Style TargetType="Expander" x:Key="GroupExpander">
        <Setter Property="Margin" Value="0,5"/>
        <Setter Property="BorderBrush" Value="#DDD"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="Padding" Value="5"/>
    </Style>
</Window.Resources>

<StackPanel Width="400" Margin="20">
    <TextBlock Text="设备参数配置" FontSize="16" FontWeight="Bold" Margin="0,0,0,10"/>
    
    <!-- 基础参数组（默认展开） -->
    <Expander Style="{StaticResource GroupExpander}" Header="基础参数" IsExpanded="True">
        <StackPanel Margin="10,5">
            <TextBox Text="设备编号：AOI-003"/>
            <TextBox Text="所属产线：SMT-2线" Margin="0,5,0,0"/>
            <ComboBox Header="设备状态" SelectedIndex="0" Margin="0,5,0,0">
                <ComboBoxItem Content="运行中"/>
                <ComboBoxItem Content="待机"/>
                <ComboBoxItem Content="维护中"/>
            </ComboBox>
        </StackPanel>
    </Expander>

    <!-- 通信参数组 -->
    <Expander Style="{StaticResource GroupExpander}" Header="通信参数">
        <StackPanel Margin="10,5">
            <TextBox Text="PLC地址：192.168.1.50"/>
            <TextBox Text="端口号：502" Margin="0,5,0,0"/>
            <TextBox Text="超时时间：3000ms" Margin="0,5,0,0"/>
        </StackPanel>
    </Expander>

    <!-- 产能参数组 -->
    <Expander Style="{StaticResource GroupExpander}" Header="产能参数">
        <StackPanel Margin="10,5">
            <TextBox Text="设计节拍：12秒/片"/>
            <TextBox Text="日产能目标：6000片" Margin="0,5,0,0"/>
            <CheckBox Content="启用产能自动上报" IsChecked="True" Margin="0,5,0,0"/>
        </StackPanel>
    </Expander>
</StackPanel>
```

### 实例 2：左侧可折叠侧边栏 - 功能导航

横向展开场景，常用于系统左侧导航栏，收起时节省横向空间。

xaml:

```xaml
<DockPanel>
    <!-- 左侧折叠侧边栏 -->
    <Expander DockPanel.Dock="Left" 
              ExpandDirection="Right" 
              IsExpanded="True"
              Background="#F5F5F5"
              BorderBrush="#DDD" 
              BorderThickness="0,0,1,0">
        <Expander.Header>
            <!-- 收起时显示的竖排标题 -->
            <TextBlock Text="功能导航" FontWeight="Bold">
                <TextBlock.LayoutTransform>
                    <RotateTransform Angle="-90"/>
                </TextBlock.LayoutTransform>
            </TextBlock>
        </Expander.Header>
        
        <StackPanel Width="180" Margin="10">
            <Button Content="实时监控" Height="35" Margin="0,3"/>
            <Button Content="产能报表" Height="35" Margin="0,3"/>
            <Button Content="报警记录" Height="35" Margin="0,3"/>
            <Button Content="参数配置" Height="35" Margin="0,3"/>
            <Button Content="设备维护" Height="35" Margin="0,3"/>
        </StackPanel>
    </Expander>

    <!-- 主内容区域 -->
    <Grid Background="White">
        <TextBlock Text="主业务内容区域" HorizontalAlignment="Center" VerticalAlignment="Center"/>
    </Grid>
</DockPanel>
```

### 实例 3：MVVM 绑定 + 展开懒加载数据

结合 MVVM 模式，展开时才加载对应数据（如读取 PLC 实时数据），减少界面初始化开销。

#### ViewModel 代码

csharp:

```c#
public class DeviceViewModel : INotifyPropertyChanged
{
    private bool _isRealTimeExpanded;
    private string _currentYield;
    private string _runningSpeed;

    /// <summary>
    /// 实时数据分组是否展开
    /// </summary>
    public bool IsRealTimeExpanded
    {
        get => _isRealTimeExpanded;
        set
        {
            _isRealTimeExpanded = value;
            OnPropertyChanged();
            // 仅展开时加载实时数据
            if (value) LoadRealTimeData();
        }
    }

    public string CurrentYield
    {
        get => _currentYield;
        set { _currentYield = value; OnPropertyChanged(); }
    }

    public string RunningSpeed
    {
        get => _runningSpeed;
        set { _runningSpeed = value; OnPropertyChanged(); }
    }

    private void LoadRealTimeData()
    {
        // 模拟读取PLC实时产能、速度数据
        Task.Run(() =>
        {
            Thread.Sleep(200); // 模拟通信延迟
            CurrentYield = "3256 片";
            RunningSpeed = "11.8 秒/片";
        });
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
```

#### XAML 绑定

xaml:

```xaml
<Expander Header="实时运行数据" 
          IsExpanded="{Binding IsRealTimeExpanded, Mode=TwoWay}">
    <StackPanel Margin="10">
        <TextBlock Text="当前产量："/>
        <TextBlock Text="{Binding CurrentYield}" FontSize="18" Foreground="Green"/>
        <TextBlock Text="运行节拍：" Margin="0,10,0,0"/>
        <TextBlock Text="{Binding RunningSpeed}" FontSize="18" Foreground="Blue"/>
    </StackPanel>
</Expander>
```

### 实例 4：多级嵌套折叠 - 层级参数配置

适合层级复杂的参数配置场景，比如 PLC 寄存器按功能区、子分组多层管理。

xaml:

```xaml
<Expander Header="PLC寄存器配置" IsExpanded="True" BorderBrush="#DDD" BorderThickness="1">
    <StackPanel Margin="10,5">
        <Expander Header="输入区寄存器 (X区)">
            <DataGrid AutoGenerateColumns="False" Height="150" CanUserAddRows="False">
                <!-- 寄存器配置表格 -->
            </DataGrid>
        </Expander>
        
        <Expander Header="输出区寄存器 (Y区)" Margin="0,5,0,0">
            <DataGrid AutoGenerateColumns="False" Height="150" CanUserAddRows="False">
                <!-- 寄存器配置表格 -->
            </DataGrid>
        </Expander>
        
        <Expander Header="数据寄存器 (D区)" Margin="0,5,0,0">
            <StackPanel Margin="10,5">
                <Expander Header="产能相关地址">
                    <TextBlock Text="D100-D150：时段产能寄存器" Margin="5"/>
                </Expander>
                <Expander Header="报警相关地址" Margin="0,5,0,0">
                    <TextBlock Text="D200-D230：报警代码寄存器" Margin="5"/>
                </Expander>
            </StackPanel>
        </Expander>
    </StackPanel>
</Expander>
```

### 实例 5：手风琴效果（同一时间仅展开一个）

多个 Expander 组合，实现「展开一个、自动折叠其他」的手风琴效果，常用于分类菜单。

#### 后台逻辑

csharp:

```c#
private void Expander_Expanded(object sender, RoutedEventArgs e)
{
    Expander current = sender as Expander;
    if (current.Parent is StackPanel panel)
    {
        // 遍历父容器内所有Expander，关闭非当前项
        foreach (var child in panel.Children)
        {
            if (child is Expander exp && exp != current)
            {
                exp.IsExpanded = false;
            }
        }
    }
}
```

#### XAML 关联事件

xaml:

```xaml
<StackPanel Width="200" Margin="20">
    <Expander Header="设备监控" Expanded="Expander_Expanded"/>
    <Expander Header="产能分析" Expanded="Expander_Expanded"/>
    <Expander Header="报警管理" Expanded="Expander_Expanded"/>
    <Expander Header="系统设置" Expanded="Expander_Expanded"/>
</StackPanel>
```

------

## 五、常见问题与避坑指南

1. **横向展开内容显示异常**

   设置 `ExpandDirection="Left/Right"` 时，必须给 Expander 显式设置 `Height`，否则内容高度为 0 无法显示；同时建议给内容设置固定 `Width`。

2. **内容过长超出界面**

   折叠内容较多时，在内部包裹 `ScrollViewer` 实现滚动查看：

   xaml:

   ```xaml
   <Expander Header="长内容分组">
       <ScrollViewer MaxHeight="300" VerticalScrollBarVisibility="Auto">
           <!-- 长列表内容 -->
       </ScrollViewer>
   </Expander>
   ```

3. **IsExpanded 绑定不生效**

   确保绑定为 `TwoWay` 模式；MVVM 场景下属性变更需触发 `PropertyChanged` 通知。

4. **嵌套 Expander 事件冒泡**

   子级 Expander 的 `Expanded`/`Collapsed` 事件会冒泡到父级，如需区分事件源，可通过 `e.OriginalSource` 判断。