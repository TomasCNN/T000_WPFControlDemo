# 004022004_WPF Expander 基类体系官方类定义逐层解析

下面的实例全部围绕 Expander 各层基类的核心特性展开，对应 `HeaderedContentControl`、`ContentControl`、`Control`、`FrameworkElement` 等基类的能力，结合工业软件常见场景落地，帮助你理解「基类特性 → Expander 实际用法」的对应关系。

------

## 一、基于 HeaderedContentControl 基类（双内容模型）

这是 Expander 最直接的父类，提供 `Header` / `HeaderTemplate` / `HeaderStringFormat` 完整标题体系，搭配继承自 ContentControl 的 `Content` 内容体系，构成「标题 + 折叠内容」的核心结构。

### 实例 1：自定义复杂标题（图标 + 文字 + 状态灯）

**用到的基类特性**：`Header` 属性类型为 `object`，支持任意 UI 元素，而非仅纯文本。

工业场景：设备参数分组，标题同时显示分组名、运行状态指示灯、配置项数量。

xaml:

```xaml
<Expander IsExpanded="True" BorderBrush="#DDD" BorderThickness="1" Padding="5">
    <Expander.Header>
        <StackPanel Orientation="Horizontal" Spacing="8">
            <Ellipse Width="10" Height="10" Fill="Green" VerticalAlignment="Center"/>
            <TextBlock Text="运行参数组" FontWeight="Bold" FontSize="14"/>
            <TextBlock Text="(12项配置)" Foreground="Gray" FontSize="12" VerticalAlignment="Center"/>
        </StackPanel>
    </Expander.Header>
    
    <StackPanel Margin="10,5">
        <TextBox Text="运行速度：12s/片"/>
        <TextBox Text="节拍阈值：15s" Margin="0,5,0,0"/>
        <CheckBox Content="启用自动调速" IsChecked="True" Margin="0,5,0,0"/>
    </StackPanel>
</Expander>
```

### 实例 2：HeaderTemplate 数据模板（MVVM 动态标题）

**用到的基类特性**：`HeaderTemplate` 依赖属性，MVVM 场景下通过数据模板渲染标题，无需后台代码操作 UI。

ViewModel 定义：

csharp:

```c#
public class ParamGroupViewModel : INotifyPropertyChanged
{
    public string GroupName { get; set; }
    public int ItemCount { get; set; }
    public bool IsRunning { get; set; }
}
```

XAML 绑定：

xaml:

```xaml
<Expander IsExpanded="{Binding IsExpanded, Mode=TwoWay}">
    <Expander.HeaderTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal" Spacing="8">
                <Ellipse Width="8" Height="8" VerticalAlignment="Center">
                    <Ellipse.Style>
                        <Style TargetType="Ellipse">
                            <Setter Property="Fill" Value="Gray"/>
                            <Style.Triggers>
                                <DataTrigger Binding="{Binding IsRunning}" Value="True">
                                    <Setter Property="Fill" Value="Green"/>
                                </DataTrigger>
                            </Style.Triggers>
                        </Style>
                    </Ellipse.Style>
                </Ellipse>
                <TextBlock Text="{Binding GroupName}" FontWeight="Bold"/>
                <TextBlock Text="{Binding ItemCount, StringFormat=({0}项)}" Foreground="Gray"/>
            </StackPanel>
        </DataTemplate>
    </Expander.HeaderTemplate>

    <!-- 折叠内容区 -->
    <ItemsControl ItemsSource="{Binding ParamItems}"/>
</Expander>
```

### 实例 3：HeaderStringFormat 快速格式化标题

**用到的基类特性**：`HeaderStringFormat` 字符串格式化，纯文本标题时快速格式化，无需编写值转换器。

xaml:

```c#
<Expander Header="{Binding ReportDate}" 
          HeaderStringFormat="产能报表 - {0:yyyy年MM月dd日}">
    <!-- 报表表格内容 -->
</Expander>
```

------

## 二、基于 ContentControl 基类（单内容模型）

Expander 的折叠内容区完全继承自 ContentControl 的 `Content` 体系，支持内容模板、模板选择器等高级特性。

### 实例：ContentTemplateSelector 动态切换内容模板

**用到的基类特性**：`ContentTemplateSelector`，根据数据状态自动选择不同的内容模板。

工业场景：报警分组，正常状态显示参数列表，异常状态显示报警详情。

1. 定义模板选择器

csharp:

```c#
public class AlarmContentSelector : DataTemplateSelector
{
    public DataTemplate NormalTemplate { get; set; }
    public DataTemplate AlarmTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is AlarmGroupViewModel group)
        {
            return group.HasActiveAlarm ? AlarmTemplate : NormalTemplate;
        }
        return base.SelectTemplate(item, container);
    }
}
```

1. XAML 资源中注册模板

xaml:

```xaml
<Window.Resources>
    <DataTemplate x:Key="NormalTpl">
        <TextBlock Text="当前分组无异常" Foreground="Green" Padding="10"/>
    </DataTemplate>
    
    <DataTemplate x:Key="AlarmTpl">
        <StackPanel Margin="10">
            <TextBlock Text="存在活跃报警" Foreground="Red" FontWeight="Bold"/>
            <ItemsControl ItemsSource="{Binding AlarmList}" Margin="0,5,0,0"/>
        </StackPanel>
    </DataTemplate>
    
    <local:AlarmContentSelector x:Key="AlarmSelector"
                                NormalTemplate="{StaticResource NormalTpl}"
                                AlarmTemplate="{StaticResource AlarmTpl}"/>
</Window.Resources>
```

1. Expander 中使用

xaml:

```xaml
<Expander Header="报警分组" 
          Content="{Binding AlarmGroupData}"
          ContentTemplateSelector="{StaticResource AlarmSelector}"/>
```

------

## 三、基于 Control 基类（样式与控件模板）

Control 基类提供了 `Template`、`Style`、`Background`、`BorderBrush`、`FontSize` 等通用控件外观能力，是自定义 Expander 外观的核心。

### 实例 1：样式触发器（状态联动外观）

**用到的基类特性**：`Style` 样式 + `Trigger` 触发器，基于 `IsExpanded`、`IsEnabled` 等基类属性自动切换外观。

xaml:

```xaml
<Style TargetType="Expander" x:Key="IndustrialExpander">
    <Setter Property="BorderBrush" Value="#333"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Background" Value="#1E1E1E"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="Margin" Value="0,5"/>
    <Setter Property="Padding" Value="5"/>
    
    <Style.Triggers>
        <!-- 展开状态高亮边框 -->
        <Trigger Property="IsExpanded" Value="True">
            <Setter Property="BorderBrush" Value="#007ACC"/>
            <Setter Property="Background" Value="#252526"/>
        </Trigger>
        <!-- 禁用状态半透明 -->
        <Trigger Property="IsEnabled" Value="False">
            <Setter Property="Opacity" Value="0.5"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

> `IsEnabled`、`Opacity` 等属性全部继承自 Control/UIElement 基类，可直接在触发器中使用。

### 实例 2：自定义控件模板（完全重写外观）

**用到的基类特性**：`Template` 控件模板，完全替换 Expander 的视觉结构，只保留逻辑行为。

实现工业深色主题 + 自定义箭头样式：

xaml:

```xaml
<Style TargetType="Expander" x:Key="DarkCustomExpander">
    <Setter Property="Background" Value="#1E1E1E"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="BorderBrush" Value="#333"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Expander">
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}">
                    <DockPanel>
                        <!-- 标题栏 -->
                        <ToggleButton DockPanel.Dock="Top" 
                                      IsChecked="{Binding IsExpanded, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}"
                                      Content="{TemplateBinding Header}"
                                      ContentTemplate="{TemplateBinding HeaderTemplate}"
                                      Padding="8"
                                      Background="Transparent" 
                                      BorderThickness="0"
                                      HorizontalContentAlignment="Left">
                            <ToggleButton.Template>
                                <ControlTemplate TargetType="ToggleButton">
                                    <DockPanel Background="Transparent">
                                        <!-- 自定义箭头 -->
                                        <Path DockPanel.Dock="Left" 
                                              Data="M 0 4 L 4 0 L 8 4 Z" 
                                              Fill="Gray" Margin="0,0,8,0"
                                              VerticalAlignment="Center"
                                              x:Name="ArrowPath">
                                            <Path.RenderTransform>
                                                <RotateTransform Angle="-90" CenterX="4" CenterY="4"/>
                                            </Path.RenderTransform>
                                        </Path>
                                        <ContentPresenter VerticalAlignment="Center"/>
                                    </DockPanel>
                                    <ControlTemplate.Triggers>
                                        <Trigger Property="IsChecked" Value="True">
                                            <Setter TargetName="ArrowPath" Property="RenderTransform">
                                                <Setter.Value>
                                                    <RotateTransform Angle="0" CenterX="4" CenterY="4"/>
                                                </Setter.Value>
                                            </Setter>
                                            <Setter TargetName="ArrowPath" Property="Fill" Value="#007ACC"/>
                                        </Trigger>
                                    </ControlTemplate.Triggers>
                                </ControlTemplate>
                            </ToggleButton.Template>
                        </ToggleButton>
                        
                        <!-- 内容区域 -->
                        <ContentPresenter x:Name="ContentSite" 
                                          Visibility="Collapsed"
                                          Margin="8,0,8,8"/>
                    </DockPanel>
                </Border>
                
                <ControlTemplate.Triggers>
                    <Trigger Property="IsExpanded" Value="True">
                        <Setter TargetName="ContentSite" Property="Visibility" Value="Visible"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

------

## 四、基于 UIElement / FrameworkElement 基类（交互与布局）

### 实例 1：路由事件冒泡（父容器统一监听）

**用到的基类特性**：`Expanded` / `Collapsed` 是冒泡路由事件，继承自 UIElement 的路由事件体系，可在父容器统一处理。

工业场景：多个参数分组，统一在父容器监听展开事件，实现懒加载，无需逐个订阅。

XAML：

xaml:

```xaml
<StackPanel Expander.Expanded="Group_Expanded" Margin="20">
    <Expander Header="基础参数"/>
    <Expander Header="通信参数"/>
    <Expander Header="产能参数"/>
    <Expander Header="报警参数"/>
</StackPanel>
```

后台统一处理：

csharp:

```c#
private void Group_Expanded(object sender, RoutedEventArgs e)
{
    // e.OriginalSource 就是当前触发展开的 Expander
    if (e.OriginalSource is Expander expander)
    {
        string groupName = expander.Header.ToString();
        // 统一处理：展开时异步加载对应分组的PLC配置数据
        Task.Run(() => LoadGroupConfigData(groupName));
    }
}
```

### 实例 2：基类布局属性复用

**用到的基类特性**：`Margin`、`HorizontalAlignment`、`Width`、`VerticalAlignment` 等布局属性，全部继承自 FrameworkElement 基类。

侧边栏横向展开场景：

xaml:

```xaml
<Expander Header="功能导航" 
          ExpandDirection="Right"
          Width="200"
          HorizontalAlignment="Left"
          VerticalAlignment="Stretch"
          Margin="5"
          BorderBrush="#DDD" BorderThickness="0,0,1,0">
    <StackPanel>
        <Button Content="实时监控" Height="35" Margin="0,3"/>
        <Button Content="产能报表" Height="35" Margin="0,3"/>
        <Button Content="报警记录" Height="35" Margin="0,3"/>
    </StackPanel>
</Expander>
```

### 实例 3：IsEnabled 禁用状态控制

**用到的基类特性**：`IsEnabled` 依赖属性，继承自 UIElement，控制控件是否可交互。

工业场景：设备离线时，禁用对应参数分组的展开操作。

xaml:

```xaml
<Expander Header="设备运行参数" IsEnabled="{Binding IsDeviceOnline}">
    <!-- 设备离线时无法点击展开，内容灰化 -->
    <StackPanel>
        <TextBox Text="运行速度"/>
        <TextBox Text="目标产能"/>
    </StackPanel>
</Expander>
```

------

## 五、综合实例：继承 Expander 的自定义工业控件

结合所有基类特性，封装一个支持「展开懒加载」的工业分组控件：

csharp:

```c#
public class IndustrialExpander : Expander
{
    // 自定义依赖属性（继承 DependencyObject 基类能力）
    public static readonly DependencyProperty GroupCodeProperty =
        DependencyProperty.Register(
            "GroupCode", 
            typeof(string), 
            typeof(IndustrialExpander), 
            new PropertyMetadata(string.Empty));

    public string GroupCode
    {
        get => (string)GetValue(GroupCodeProperty);
        set => SetValue(GroupCodeProperty, value);
    }

    private bool _dataLoaded;

    // 重写虚方法，展开时才加载数据
    protected override void OnExpanded()
    {
        base.OnExpanded();
        
        if (!_dataLoaded)
        {
            _dataLoaded = true;
            LoadGroupDataAsync();
        }
    }

    private async void LoadGroupDataAsync()
    {
        // 异步读取PLC对应分组的寄存器数据
        await Task.Delay(200); // 模拟通信延迟
        // 数据加载完成后更新 Content
    }
}
```