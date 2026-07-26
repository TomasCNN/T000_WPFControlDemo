# 004024002_WPF `MediaElement` WPF 内容控件基础使用案例

下面补充一批**工业场景高频、代码精简可直接复用**的内容控件案例，覆盖交互按钮、状态提示、分组布局、动态视图、可复用组件五大类，均基于 `ContentControl` 体系的特性实现。

------

## 一、交互按钮类

### 案例 1：RepeatButton 数值微调按钮

**对应控件**：`RepeatButton`（继承 `ButtonBase` → `ContentControl`）

**工业场景**：PLC 参数微调、速度 / 温度数值加减，按住不放连续增减。

xaml:

```c#
<StackPanel Orientation="Horizontal" HorizontalAlignment="Center" Margin="20">
    <TextBlock Text="运行节拍：" VerticalAlignment="Center" Width="80"/>
    <TextBlock x:Name="txtValue" Text="12.0" VerticalAlignment="Center" Width="50" TextAlignment="Center"/>
    <TextBlock Text=" 秒/片" VerticalAlignment="Center"/>
    
    <!-- 减按钮：按住连续减 -->
    <RepeatButton Content="-" Width="30" Height="24" Margin="5,0"
                  Click="BtnDecrease_Click" Interval="200"/>
    <!-- 加按钮：按住连续加 -->
    <RepeatButton Content="+" Width="30" Height="24"
                  Click="BtnIncrease_Click" Interval="200"/>
</StackPanel>
```

后台逻辑：

csharp:

```c#
private double _beat = 12.0;
private void BtnIncrease_Click(object sender, RoutedEventArgs e)
{
    _beat += 0.1;
    txtValue.Text = _beat.ToString("F1");
}
private void BtnDecrease_Click(object sender, RoutedEventArgs e)
{
    _beat -= 0.1;
    txtValue.Text = _beat.ToString("F1");
}
```

**核心特性**：`Interval` 控制触发间隔（毫秒），长按连续触发 `Click` 事件。

------

### 案例 2：ToggleButton 开关式状态按钮

**对应控件**：`ToggleButton`（继承 `ButtonBase` → `ContentControl`）

**工业场景**：设备启停、功能开关，比 CheckBox 更具按钮视觉感。

xaml:

```xaml
<ToggleButton x:Name="toggleAuto" Width="120" Height="32"
              IsChecked="{Binding IsAutoMode}">
    <ToggleButton.Style>
        <Style TargetType="ToggleButton">
            <Setter Property="Content" Value="手动模式"/>
            <Setter Property="Background" Value="#DDD"/>
            <Style.Triggers>
                <Trigger Property="IsChecked" Value="True">
                    <Setter Property="Content" Value="自动模式"/>
                    <Setter Property="Background" Value="#007ACC"/>
                    <Setter Property="Foreground" Value="White"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </ToggleButton.Style>
</ToggleButton>
```

**核心特性**：`IsChecked` 双向绑定，两种状态自动切换外观与文本。

------

## 二、信息提示类

### 案例 3：ToolTip 带详情的悬浮提示

**对应控件**：`ToolTip`（继承 `ContentControl`）

**工业场景**：设备状态、报警图标悬浮显示完整详情，不占用主界面空间。

xaml:

```xaml
<StackPanel Orientation="Horizontal" Margin="20">
    <Ellipse Width="12" Height="12" Fill="Green" VerticalAlignment="Center">
        <Ellipse.ToolTip>
            <!-- ToolTip 内容可任意组合 -->
            <StackPanel Width="180">
                <TextBlock Text="设备运行正常" FontWeight="Bold" FontSize="14"/>
                <TextBlock Text="设备编号：AOI-003" Margin="0,3"/>
                <TextBlock Text="当前产量：3256 片"/>
                <TextBlock Text="运行时长：6h 23min" Foreground="Gray"/>
            </StackPanel>
        </Ellipse.ToolTip>
    </Ellipse>
    <TextBlock Text="设备状态" Margin="8,0" VerticalAlignment="Center"/>
</StackPanel>
```

**核心特性**：`ToolTip` 是内容控件，支持任意 UI 元素，不局限于纯文本。

------

### 案例 4：StatusBarItem 状态栏信息项

**对应控件**：`StatusBarItem`（继承 `ContentControl`）

**工业场景**：窗口底部状态栏，分块显示运行状态、时间、用户信息。

xaml:

```xaml
<StatusBar DockPanel.Dock="Bottom">
    <!-- 左侧运行状态 -->
    <StatusBarItem>
        <StackPanel Orientation="Horizontal" Spacing="6">
            <Ellipse Width="8" Height="8" Fill="Green" VerticalAlignment="Center"/>
            <TextBlock Text="系统运行中"/>
        </StackPanel>
    </StatusBarItem>
    
    <!-- 右侧分隔 + 时间 -->
    <Separator/>
    <StatusBarItem HorizontalAlignment="Right">
        <TextBlock Text="{Binding SystemTime, StringFormat='当前时间：{0:yyyy-MM-dd HH:mm:ss}'}"/>
    </StatusBarItem>
</StatusBar>
```

------

## 三、分组容器类

### 案例 5：GroupBox 多级嵌套参数分组

**对应控件**：`GroupBox`（继承 `HeaderedContentControl` → `ContentControl`）

**工业场景**：复杂参数配置页，按层级分组，结构清晰。

xaml:

```xaml
<GroupBox Header="设备运行参数" Margin="20" Padding="10" BorderBrush="#DDD">
    <StackPanel Spacing="10">
        <!-- 子分组1：速度参数 -->
        <GroupBox Header="速度配置" Padding="8" BorderBrush="#EEE">
            <StackPanel Spacing="5">
                <TextBox Text="运行速度：1200 mm/s"/>
                <TextBox Text="加速时间：0.5s"/>
            </StackPanel>
        </GroupBox>
        
        <!-- 子分组2：阈值参数 -->
        <GroupBox Header="报警阈值" Padding="8" BorderBrush="#EEE">
            <StackPanel Spacing="5">
                <TextBox Text="温度上限：60℃"/>
                <TextBox Text="节拍阈值：15s/片"/>
            </StackPanel>
        </GroupBox>
    </StackPanel>
</GroupBox>
```

------

### 案例 6：TabItem 多标签功能页

**对应控件**：`TabItem`（继承 `HeaderedContentControl` → `ContentControl`）

**工业场景**：单窗口集成监控、参数、报警、报表多个功能页。

xaml:

```xaml
<TabControl Margin="10">
    <!-- 实时监控页 -->
    <TabItem Header="实时监控">
        <Border Background="#F0F8FF">
            <TextBlock Text="设备运行画面 + 实时数据面板" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>
    </TabItem>
    
    <!-- 参数配置页 -->
    <TabItem Header="参数配置">
        <ScrollViewer>
            <StackPanel Margin="10">
                <TextBox Text="PLC地址配置"/>
                <TextBox Text="产能参数配置" Margin="0,5"/>
            </StackPanel>
        </ScrollViewer>
    </TabItem>
    
    <!-- 报警记录页 -->
    <TabItem Header="报警记录">
        <DataGrid AutoGenerateColumns="False"/>
    </TabItem>
</TabControl>
```

------

## 四、动态视图类

### 案例 7：ContentControl + 触发器 状态视图切换

**对应控件**：`ContentControl`（最基础内容控件）

**工业场景**：根据设备运行状态，自动切换显示不同的状态卡片，纯 XAML 实现，无需后台代码。

xaml:

```xaml
<ContentControl Margin="20" Width="200" BorderBrush="#DDD" BorderThickness="1" Padding="15">
    <ContentControl.Style>
        <Style TargetType="ContentControl">
            <!-- 默认：停机状态 -->
            <Setter Property="ContentTemplate">
                <Setter.Value>
                    <DataTemplate>
                        <StackPanel>
                            <Ellipse Width="40" Height="40" Fill="Gray" HorizontalAlignment="Center"/>
                            <TextBlock Text="设备停机" HorizontalAlignment="Center" Margin="0,8" FontWeight="Bold"/>
                            <TextBlock Text="待启动" Foreground="Gray" HorizontalAlignment="Center"/>
                        </StackPanel>
                    </DataTemplate>
                </Setter.Value>
            </Setter>
            
            <!-- 运行状态 -->
            <Style.Triggers>
                <DataTrigger Binding="{Binding IsRunning}" Value="True">
                    <Setter Property="ContentTemplate">
                        <Setter.Value>
                            <DataTemplate>
                                <StackPanel>
                                    <Ellipse Width="40" Height="40" Fill="Green" HorizontalAlignment="Center"/>
                                    <TextBlock Text="设备运行中" HorizontalAlignment="Center" Margin="0,8" FontWeight="Bold" Foreground="Green"/>
                                    <TextBlock Text="产量：3256" Foreground="Gray" HorizontalAlignment="Center"/>
                                </StackPanel>
                            </DataTemplate>
                        </Setter.Value>
                    </Setter>
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </ContentControl.Style>
</ContentControl>
```

**核心特性**：通过 `ContentTemplate` + 数据触发器，实现「数据驱动视图切换」，是 MVVM 模式的典型用法。

------

### 案例 8：ScrollViewer 长表单滚动容器

**对应控件**：`ScrollViewer`（继承 `ContentControl`）

**工业场景**：参数配置项很多，固定高度区域内滚动查看，避免页面过长。

xaml:

```xaml
<ScrollViewer MaxHeight="350" 
              VerticalScrollBarVisibility="Auto"
              HorizontalScrollBarVisibility="Disabled"
              Margin="20" BorderBrush="#DDD" BorderThickness="1">
    <StackPanel Margin="10" Spacing="8">
        <!-- 批量参数项 -->
        <TextBox Text="参数1：xxx"/>
        <TextBox Text="参数2：xxx"/>
        <TextBox Text="参数3：xxx"/>
        <TextBox Text="参数4：xxx"/>
        <TextBox Text="参数5：xxx"/>
        <TextBox Text="参数6：xxx"/>
        <TextBox Text="参数7：xxx"/>
        <TextBox Text="参数8：xxx"/>
        <TextBox Text="参数9：xxx"/>
        <TextBox Text="参数10：xxx"/>
    </StackPanel>
</ScrollViewer>
```

**核心特性**：内容超出高度自动出现滚动条，只占固定布局空间。

------

## 五、可复用组件类

### 案例 9：UserControl 封装设备状态卡片

**对应控件**：`UserControl`（继承 `ContentControl`）

**工业场景**：多设备监控页面，每个设备卡片样式一致，封装为可复用控件。

#### 新建 `DeviceStatusCard.xaml`

xaml:

```xaml
<UserControl x:Class="IndustrialDemo.DeviceStatusCard"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Border Width="180" BorderBrush="#DDD" BorderThickness="1" Padding="10" CornerRadius="4">
        <StackPanel>
            <TextBlock Text="{Binding EqpId}" FontSize="14" FontWeight="Bold"/>
            <StackPanel Orientation="Horizontal" Spacing="6" Margin="0,5">
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
                <TextBlock Text="{Binding StatusText}"/>
            </StackPanel>
            <TextBlock Text="{Binding Yield, StringFormat=产量：{0} 片}" Foreground="Gray" FontSize="12"/>
        </StackPanel>
    </Border>
</UserControl>
```

#### 主页面复用

xaml:

```xaml
<WrapPanel Margin="20" Spacing="10">
    <local:DeviceStatusCard DataContext="{Binding DeviceA}"/>
    <local:DeviceStatusCard DataContext="{Binding DeviceB}"/>
    <local:DeviceStatusCard DataContext="{Binding DeviceC}"/>
</WrapPanel>
```

**核心价值**：一次定义，多处复用，修改样式只需改一处。



## 六、装饰容器类

### 案例 10：Border 边框装饰容器

**对应基类**：`Decorator`（单内容装饰器，用法与内容控件高度一致）

**工业场景**：给参数卡片、状态面板加边框、圆角、背景色，是最基础的 UI 装饰元素。

xaml:

```xaml
<Border Width="220"
        Background="#F8F9FA"
        BorderBrush="#CED4DA"
        BorderThickness="1"
        CornerRadius="6"
        Padding="15"
        Margin="20">
    <!-- 内部只能放一个子元素，通常是布局面板 -->
    <StackPanel Spacing="6">
        <TextBlock Text="设备运行总览" FontWeight="Bold" FontSize="14"/>
        <TextBlock Text="今日产量：3,428 片"/>
        <TextBlock Text="运行时长：7h 42min"/>
        <TextBlock Text="良品率：98.7%" Foreground="Green"/>
    </StackPanel>
</Border>
```

**核心特性**：`Child` 属性承载单一内容，提供背景、边框、圆角、内边距、外边距五大装饰能力。

------

### 案例 11：Viewbox 自适应缩放容器

**对应基类**：`Decorator`

**工业场景**：大屏看板、固定尺寸的仪表盘、流程图，自动缩放适配不同分辨率屏幕，不变形不裁剪。

xaml:

```xaml
<Viewbox Stretch="Uniform" StretchDirection="Both" Width="400" Height="300">
    <!-- 内部内容按原始尺寸设计，Viewbox 自动整体缩放 -->
    <Grid Width="800" Height="600" Background="#1E1E1E">
        <TextBlock Text="产线总览看板" Foreground="White" FontSize="48" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        <!-- 更多固定尺寸的仪表盘、图表 -->
    </Grid>
</Viewbox>
```

**核心特性**：整体缩放内部所有元素，包括字体、线条、图标，保持原始比例不变。

------

## 七、弹窗与窗口类

### 案例 12：Window 自定义弹窗窗口

**对应基类**：`Window`（继承 `ContentControl`）

**工业场景**：参数配置弹窗、报警详情弹窗，独立窗口承载完整表单。

#### 新建 `ConfigDialog.xaml`

xaml:

```xaml
<Window x:Class="IndustrialDemo.ConfigDialog"
        Title="参数配置"
        Width="400" Height="300"
        WindowStartupLocation="CenterOwner"
        ResizeMode="NoResize">
    <Grid Margin="15">
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- 窗口主体内容 -->
        <StackPanel Grid.Row="0" Spacing="8">
            <TextBox Text="设备编号：AOI-003"/>
            <TextBox Text="PLC地址：192.168.1.50"/>
            <TextBox Text="上报间隔：60 秒"/>
        </StackPanel>

        <!-- 底部按钮 -->
        <StackPanel Grid.Row="1" Orientation="Horizontal" HorizontalAlignment="Right" Spacing="10">
            <Button Content="确定" Width="80" Height="28" Click="BtnOk_Click"/>
            <Button Content="取消" Width="80" Height="28" Click="BtnCancel_Click"/>
        </StackPanel>
    </Grid>
</Window>
```

#### 调用方式

csharp:

```c#
private void OpenConfig_Click(object sender, RoutedEventArgs e)
{
    ConfigDialog dialog = new ConfigDialog();
    dialog.Owner = this;
    bool? result = dialog.ShowDialog(); // 模态弹窗
    
    if (result == true)
    {
        UpdateProcess("配置已保存");
    }
}
```

------

### 案例 13：Popup 悬浮弹出面板

**对应控件**：`Popup`（内容控件，单内容承载）

**工业场景**：右键菜单、快捷操作面板、详情气泡，不占用主布局空间，悬浮显示。

xaml:

```xaml
<Button x:Name="btnMore" Content="更多操作" Width="100" Height="30" Click="BtnMore_Click"/>

<Popup x:Name="popupMenu"
       PlacementTarget="{Binding ElementName=btnMore}"
       Placement="Bottom"
       StaysOpen="False"
       AllowsTransparency="True">
    <Border Background="White" BorderBrush="#DDD" BorderThickness="1" Padding="5">
        <StackPanel>
            <Button Content="导出报表" HorizontalAlignment="Stretch"/>
            <Button Content="打印记录" HorizontalAlignment="Stretch" Margin="0,3"/>
            <Button Content="查看详情" HorizontalAlignment="Stretch"/>
        </StackPanel>
    </Border>
</Popup>
```

csharp:

```c#
private void BtnMore_Click(object sender, RoutedEventArgs e)
{
    popupMenu.IsOpen = !popupMenu.IsOpen;
}
```

**核心特性**：`IsOpen` 控制显示隐藏，`Placement` 控制弹出位置，不影响主界面布局。

------

## 八、导航与页面类

### 案例 14：Frame + Page 页面导航

**对应控件**：`Frame`（继承 `ContentControl`）、`Page`（继承 `ContentControl`）

**工业场景**：多页面系统，比如「登录页 → 主菜单 → 设备监控 → 报表查询」的流程化导航。

#### 主窗口布局

xaml:

```xaml
<DockPanel>
    <StackPanel DockPanel.Dock="Top" Orientation="Horizontal" Background="#F0F0F0">
        <Button Content="设备监控" Click="NavMonitor_Click" Padding="10,6"/>
        <Button Content="产能报表" Click="NavReport_Click" Padding="10,6"/>
        <Button Content="系统设置" Click="NavSetting_Click" Padding="10,6"/>
    </StackPanel>

    <!-- 导航容器 -->
    <Frame x:Name="mainFrame" NavigationUIVisibility="Hidden"/>
</DockPanel>
```

#### 后台导航逻辑

csharp:

```c#
private void NavMonitor_Click(object sender, RoutedEventArgs e)
{
    mainFrame.Navigate(new MonitorPage());
}

private void NavReport_Click(object sender, RoutedEventArgs e)
{
    mainFrame.Navigate(new ReportPage());
}
```

> 每个功能页单独建 `Page` 文件，类似 Window 但更轻量，通过 Frame 切换显示。

------

## 九、数据呈现类

### 案例 15：ContentPresenter 内容占位符

**对应控件**：`ContentPresenter`

**工业场景**：自定义控件模板、样式中，用来标记「内容显示在这里」的占位符，是理解控件模板的核心元素。

以自定义按钮样式为例：

xaml:

```xaml
<Style TargetType="Button" x:Key="IndustrialButton">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border x:Name="border"
                        Background="#007ACC"
                        CornerRadius="4"
                        Padding="15,8">
                    <!-- 内容占位符：按钮的 Content 会渲染到这里 -->
                    <ContentPresenter HorizontalAlignment="Center"
                                      VerticalAlignment="Center"
                                      TextElement.Foreground="White"/>
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter TargetName="border" Property="Background" Value="#005A9E"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

**核心作用**：`ContentPresenter` 是所有内容控件模板的核心，负责把 `Content` 属性的值渲染到指定位置。

------

## 十、进阶：自定义内容控件

### 案例 16：继承 ContentControl 的状态卡片

**工业场景**：封装带状态边框的通用卡片，内部可放任意内容，外部只需要绑定状态。

csharp:

```xaml
public class StatusCard : ContentControl
{
    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(
            "Status",
            typeof(string),
            typeof(StatusCard),
            new PropertyMetadata("Normal"));

    public string Status
    {
        get => (string)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
}
```

#### 通用样式

xaml:

```xaml
<Style TargetType="local:StatusCard">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="local:StatusCard">
                <Border BorderThickness="2" Padding="10" CornerRadius="4">
                    <ContentPresenter/>
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="Status" Value="Alarm">
                        <Setter Property="BorderBrush" Value="Red"/>
                        <Setter Property="Background" Value="#FFF0F0"/>
                    </Trigger>
                    <Trigger Property="Status" Value="Normal">
                        <Setter Property="BorderBrush" Value="Green"/>
                        <Setter Property="Background" Value="#F0FFF0"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

#### 使用方式

xaml:

```xaml
<local:StatusCard Status="Alarm" Width="200" Margin="20">
    <!-- 内部任意内容，自动带上状态边框 -->
    <StackPanel>
        <TextBlock Text="设备A01" FontWeight="Bold"/>
        <TextBlock Text="温度超限报警" Foreground="Red"/>
    </StackPanel>
</local:StatusCard>
```

**设计价值**：把「状态边框」的通用逻辑封装起来，内部内容完全自由，完美体现内容控件的设计思想。



## 十一、头部内容控件进阶

### 案例 17：HeaderedContentControl 自定义分组

**对应控件**：`HeaderedContentControl`（双内容模型基类，GroupBox/Expander 的父类）

**工业场景**：需要自定义标题样式的分组，不想要 GroupBox 的默认边框，纯自定义标题 + 内容结构。

xaml:

```xaml
<HeaderedContentControl Margin="20">
    <!-- 标题部分 -->
    <HeaderedContentControl.Header>
        <Border Background="#007ACC" Padding="8,4">
            <TextBlock Text="生产节拍统计" Foreground="White" FontWeight="Bold"/>
        </Border>
    </HeaderedContentControl.Header>
    
    <!-- 内容部分 -->
    <Border BorderBrush="#007ACC" BorderThickness="1" Padding="10">
        <StackPanel Spacing="4">
            <TextBlock Text="平均节拍：12.3 秒/片"/>
            <TextBlock Text="最快节拍：10.8 秒/片"/>
            <TextBlock Text="最慢节拍：15.2 秒/片"/>
        </StackPanel>
    </Border>
</HeaderedContentControl>
```

**核心特性**：只有 `Header` + `Content` 双内容属性，无默认外观，完全自定义样式，适合封装业务分组组件。

------

### 案例 18：Expander 自定义箭头标题

**对应控件**：`Expander`（继承 HeaderedContentControl）

**工业场景**：默认箭头样式不匹配工业深色主题，自定义标题栏与展开图标。

xaml：

```xaml
<Expander Margin="20" IsExpanded="True">
    <Expander.Template>
        <ControlTemplate TargetType="Expander">
            <StackPanel>
                <!-- 自定义标题栏 -->
                <Border Background="#2D2D30" Padding="8">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="Auto"/>
                        </Grid.ColumnDefinitions>
                        <ContentPresenter Grid.Column="0" ContentSource="Header" Foreground="White"/>
                        <TextBlock Grid.Column="1" x:Name="arrow" Text="▼" Foreground="Gray" FontSize="10"/>
                    </Grid>
                </Border>
                
                <!-- 内容区域 -->
                <ContentPresenter x:Name="content" Visibility="Visible"/>
            </StackPanel>
            
            <ControlTemplate.Triggers>
                <Trigger Property="IsExpanded" Value="False">
                    <Setter TargetName="content" Property="Visibility" Value="Collapsed"/>
                    <Setter TargetName="arrow" Property="Text" Value="▶"/>
                </Trigger>
            </ControlTemplate.Triggers>
        </ControlTemplate>
    </Expander.Template>
    
    <Expander.Header>
        <TextBlock Text="报警参数配置" FontWeight="Bold"/>
    </Expander.Header>
    
    <StackPanel Margin="10" Spacing="5">
        <CheckBox Content="启用声光报警"/>
        <CheckBox Content="启用邮件推送"/>
    </StackPanel>
</Expander>
```

------

## 十二、装饰与排版类

### 案例 19：BulletDecorator 项目符号列表

**对应控件**：`BulletDecorator`（继承 Decorator，单内容装饰器）

**工业场景**：参数说明、报警项列表，带圆点 / 方块符号的条目排版。

xaml:

```xaml
<StackPanel Margin="20" Spacing="6">
    <BulletDecorator>
        <BulletDecorator.Bullet>
            <Ellipse Width="6" Height="6" Fill="Red"/>
        </BulletDecorator.Bullet>
        <TextBlock Text="一级报警：设备紧急停机" Margin="8,0,0,0"/>
    </BulletDecorator>
    
    <BulletDecorator>
        <BulletDecorator.Bullet>
            <Ellipse Width="6" Height="6" Fill="Orange"/>
        </BulletDecorator.Bullet>
        <TextBlock Text="二级报警：参数超出阈值" Margin="8,0,0,0"/>
    </BulletDecorator>
    
    <BulletDecorator>
        <BulletDecorator.Bullet>
            <Ellipse Width="6" Height="6" Fill="Green"/>
        </BulletDecorator.Bullet>
        <TextBlock Text="提示信息：设备正常运行" Margin="8,0,0,0"/>
    </BulletDecorator>
</StackPanel>
```

**核心特性**：`Bullet` 定义符号，`Child` 定义内容，自动垂直居中对齐。

------

### 案例 20：Viewbox 自适应仪表盘数字

**对应控件**：`Viewbox`（继承 Decorator）

**工业场景**：大屏数字看板，数字大小自动填满容器，不同分辨率下保持比例。

xaml:

```xaml
<Border Width="300" Height="150" Background="#1E1E1E" CornerRadius="8">
    <Viewbox Stretch="Uniform" Margin="10">
        <StackPanel HorizontalAlignment="Center">
            <TextBlock Text="今日产量" Foreground="Gray" FontSize="16" HorizontalAlignment="Center"/>
            <TextBlock Text="3,428" Foreground="#00FF7F" FontSize="72" FontWeight="Bold" HorizontalAlignment="Center"/>
            <TextBlock Text="单位：片" Foreground="Gray" FontSize="14" HorizontalAlignment="Center"/>
        </StackPanel>
    </Viewbox>
</Border>
```

**核心特性**：整体缩放，无需根据容器尺寸手动计算字体大小。

------

## 十三、表单与验证类

### 案例 21：Label + AccessText 标准表单标签

**对应控件**：`Label`（继承 ContentControl）

**工业场景**：参数配置表单，支持 Alt + 快捷键快速聚焦输入框，符合工业系统键盘操作习惯。

xaml:

```xaml
<StackPanel Width="350" Margin="20">
    <Label Target="{Binding ElementName=txtEqpId}">
        <AccessText Text="设备编号(_E)："/>
    </Label>
    <TextBox x:Name="txtEqpId" Text="AOI-003" Margin="0,2,0,10"/>
    
    <Label Target="{Binding ElementName=txtIp}">
        <AccessText Text="PLC地址(_I)："/>
    </Label>
    <TextBox x:Name="txtIp" Text="192.168.1.50" Margin="0,2,0,10"/>
    
    <Label Target="{Binding ElementName=txtPort}">
        <AccessText Text="端口号(_P)："/>
    </Label>
    <TextBox x:Name="txtPort" Text="502" Margin="0,2"/>
</StackPanel>
```

**操作说明**：按 `Alt+E` 自动聚焦设备编号输入框，`Alt+I` 聚焦 IP 输入框。

------

### 案例 22：Validation.ErrorTemplate 输入错误装饰

**对应基类**：`Control` 验证模板 + `AdornedElementPlaceholder`

**工业场景**：表单输入校验，错误时在输入框旁显示红色提示，不破坏原有布局。

xaml:

```xaml
<Window.Resources>
    <ControlTemplate x:Key="ErrorTemplate">
        <DockPanel>
            <TextBlock DockPanel.Dock="Right" Foreground="Red" Text="!" FontSize="16" FontWeight="Bold" Margin="3,0"/>
            <AdornedElementPlaceholder/>
        </DockPanel>
    </ControlTemplate>
</Window.Resources>

<StackPanel Margin="20" Width="250">
    <TextBox Text="{Binding EqpId, UpdateSourceTrigger=PropertyChanged, ValidatesOnDataErrors=True}"
             Validation.ErrorTemplate="{StaticResource ErrorTemplate}"/>
    <TextBlock Text="设备编号" Foreground="Gray" FontSize="12" Margin="0,3"/>
</StackPanel>
```

**核心特性**：`AdornedElementPlaceholder` 占位原始控件，在其外围叠加装饰，是 WPF 验证体系的核心元素。

------

## 十四、命令与交互进阶

### 案例 23：Button 绑定 MVVM 命令

**对应控件**：`Button`（继承 ContentControl）

**工业场景**：MVVM 模式下，按钮点击直接绑定 ViewModel 命令，无后台代码。

#### ViewModel 命令

csharp:

```c#
public ICommand StartCommand { get; }

public DeviceViewModel()
{
    StartCommand = new RelayCommand(StartDevice, () => !IsRunning);
}

private void StartDevice()
{
    // 启动设备业务逻辑
    IsRunning = true;
}
```

#### XAML 绑定

xaml:

```xaml
<Button Width="120" Height="36" Command="{Binding StartCommand}">
    <StackPanel Orientation="Horizontal" Spacing="6">
        <Path Data="M0 0 L8 4 L0 8Z" Fill="White" Width="8" Height="8" VerticalAlignment="Center"/>
        <TextBlock Text="启动设备" Foreground="White" VerticalAlignment="Center"/>
    </StackPanel>
</Button>
```

**核心特性**：`Command` 自动处理 CanExecute 状态，条件不满足时按钮自动禁用。

------

### 案例 24：CheckBox 三态参数配置

**对应控件**：`CheckBox`（继承 ToggleButton → ContentControl）

**工业场景**：批量参数配置，支持「全选 / 不全选 / 全不选」三种状态。

xaml:

```xaml
<StackPanel Margin="20" Width="200">
    <CheckBox Content="全部报警类型" IsThreeState="True" 
              IsChecked="{Binding AllAlarmsChecked, Mode=TwoWay}"
              FontWeight="Bold" Margin="0,0,0,8"/>
    
    <CheckBox Content="温度报警" IsChecked="{Binding TempAlarmChecked}" Margin="15,3"/>
    <CheckBox Content="速度报警" IsChecked="{Binding SpeedAlarmChecked}" Margin="15,3"/>
    <CheckBox Content="产能报警" IsChecked="{Binding YieldAlarmChecked}" Margin="15,3"/>
</StackPanel>
```

**核心特性**：`IsThreeState="True"` 开启第三种 `null` 状态（不确定态），适合父子级复选框联动。



## 十五、表单选择类

### 案例 25：RadioButton 卡片式单选（班次选择）

**对应控件**：`RadioButton`（继承 ToggleButton → ContentControl）

**工业场景**：生产班次选择，用卡片式样式替代默认小圆点，选中状态更醒目，适合工位操作界面。

xaml:

```xaml
<StackPanel Orientation="Horizontal" Spacing="10" Margin="20">
    <RadioButton GroupName="ShiftGroup" IsChecked="True">
        <RadioButton.Template>
            <ControlTemplate TargetType="RadioButton">
                <Border x:Name="border" Width="100" Height="60" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4" Padding="10">
                    <ContentPresenter VerticalAlignment="Center" HorizontalAlignment="Center"/>
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsChecked" Value="True">
                        <Setter TargetName="border" Property="BorderBrush" Value="#007ACC"/>
                        <Setter TargetName="border" Property="Background" Value="#E6F2FF"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </RadioButton.Template>
        <StackPanel>
            <TextBlock Text="白班" FontWeight="Bold" HorizontalAlignment="Center"/>
            <TextBlock Text="08:00-20:00" FontSize="11" Foreground="Gray" HorizontalAlignment="Center"/>
        </StackPanel>
    </RadioButton>

    <RadioButton GroupName="ShiftGroup">
        <RadioButton.Template>
            <ControlTemplate TargetType="RadioButton">
                <Border x:Name="border" Width="100" Height="60" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4" Padding="10">
                    <ContentPresenter VerticalAlignment="Center" HorizontalAlignment="Center"/>
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsChecked" Value="True">
                        <Setter TargetName="border" Property="BorderBrush" Value="#007ACC"/>
                        <Setter TargetName="border" Property="Background" Value="#E6F2FF"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </RadioButton.Template>
        <StackPanel>
            <TextBlock Text="夜班" FontWeight="Bold" HorizontalAlignment="Center"/>
            <TextBlock Text="20:00-08:00" FontSize="11" Foreground="Gray" HorizontalAlignment="Center"/>
        </StackPanel>
    </RadioButton>
</StackPanel>
```

**核心特性**：通过重写 `Template` 完全自定义单选框外观，`GroupName` 保证同组互斥，内容可任意组合。

------

### 案例 26：ComboBox 自定义下拉选项（设备选择）

**对应控件**：`ComboBoxItem`（继承 ContentControl，下拉框每一项都是内容控件）

**工业场景**：设备选择下拉框，每个选项同时显示状态灯、设备编号、设备名称，信息更直观。

xaml:

```xaml
<ComboBox Width="220" Margin="20" SelectedIndex="0">
    <ComboBox.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal" Spacing="8">
                <Ellipse Width="8" Height="8" Fill="{Binding StatusColor}" VerticalAlignment="Center"/>
                <TextBlock Text="{Binding EqpId}" Width="80"/>
                <TextBlock Text="{Binding EqpName}" Foreground="Gray"/>
            </StackPanel>
        </DataTemplate>
    </ComboBox.ItemTemplate>

    <!-- 静态项示例 -->
    <ComboBoxItem>
        <StackPanel Orientation="Horizontal" Spacing="8">
            <Ellipse Width="8" Height="8" Fill="Green" VerticalAlignment="Center"/>
            <TextBlock Text="AOI-001"/>
            <TextBlock Text="AOI检测机1号" Foreground="Gray"/>
        </StackPanel>
    </ComboBoxItem>
    <ComboBoxItem>
        <StackPanel Orientation="Horizontal" Spacing="8">
            <Ellipse Width="8" Height="8" Fill="Orange" VerticalAlignment="Center"/>
            <TextBlock Text="AOI-002"/>
            <TextBlock Text="AOI检测机2号(待机)" Foreground="Gray"/>
        </StackPanel>
    </ComboBoxItem>
</ComboBox>
```

**核心特性**：每个下拉项都是内容控件，支持任意 UI 组合；MVVM 模式下通过 `ItemTemplate` 统一渲染。

------

## 十六、格式化与标签类

### 案例 27：ContentControl 字符串格式化显示

**对应控件**：`ContentControl`

**工业场景**：显示数值 + 单位（产量、温度、速度），无需编写值转换器，直接通过 `ContentStringFormat` 快速格式化。

xaml:

```xaml
<StackPanel Margin="20" Spacing="8">
    <!-- 绑定数值，自动拼接单位 -->
    <ContentControl Content="{Binding TodayYield}" 
                    ContentStringFormat="今日产量：{0} 片"
                    FontSize="14" FontWeight="Bold"/>
    
    <ContentControl Content="{Binding DeviceTemp}" 
                    ContentStringFormat="设备温度：{0:F1} ℃"
                    Foreground="Gray"/>
    
    <ContentControl Content="{Binding LineSpeed}" 
                    ContentStringFormat="运行速度：{0} mm/s"/>
</StackPanel>
```

**核心特性**：`ContentStringFormat` 是 ContentControl 基类自带能力，简单格式化场景无需转换器，代码更精简。

------

### 案例 28：Label 带必填标记的表单标签

**对应控件**：`Label`（继承 ContentControl）

**工业场景**：参数配置表单，必填项标签增加红色星号标识，同时保留 Alt + 快捷键聚焦能力。

xaml:

```xaml
<StackPanel Width="300" Margin="20">
    <Label Target="{Binding ElementName=txtEqpCode}">
        <StackPanel Orientation="Horizontal">
            <TextBlock Text="*" Foreground="Red" Margin="0,0,2,0"/>
            <AccessText Text="设备编码(_E)："/>
        </StackPanel>
    </Label>
    <TextBox x:Name="txtEqpCode" Margin="0,2,0,10"/>

    <Label Target="{Binding ElementName=txtLine}">
        <StackPanel Orientation="Horizontal">
            <TextBlock Text="*" Foreground="Red" Margin="0,0,2,0"/>
            <AccessText Text="所属产线(_L)："/>
        </StackPanel>
    </Label>
    <TextBox x:Name="txtLine" Margin="0,2"/>
</StackPanel>
```

**核心特性**：Label 的 Content 可以是任意布局组合，同时保留 `Target` 快捷键聚焦能力，兼顾标识与操作效率。

------

## 十七、工具栏与操作类

### 案例 29：Button 矢量图标按钮（Path 绘制）

**对应控件**：`Button`（继承 ContentControl）

**工业场景**：工具栏功能按钮，用 Path 绘制矢量图标，缩放无损、体积小，适配深色 / 浅色主题。

xaml:

```xaml
<StackPanel Orientation="Horizontal" Spacing="8" Margin="20">
    <!-- 启动按钮 -->
    <Button Width="80" Height="32" Background="#2E8B57" Foreground="White" BorderThickness="0">
        <StackPanel Orientation="Horizontal" Spacing="6">
            <Path Data="M 0 0 L 8 4 L 0 8 Z" Fill="White" Width="8" Height="8" VerticalAlignment="Center"/>
            <TextBlock Text="启动" VerticalAlignment="Center"/>
        </StackPanel>
    </Button>

    <!-- 停止按钮 -->
    <Button Width="80" Height="32" Background="#DC3912" Foreground="White" BorderThickness="0">
        <StackPanel Orientation="Horizontal" Spacing="6">
            <Rectangle Width="8" Height="8" Fill="White" VerticalAlignment="Center"/>
            <TextBlock Text="停止" VerticalAlignment="Center"/>
        </StackPanel>
    </Button>

    <!-- 刷新按钮 -->
    <Button Width="80" Height="32" Background="#007ACC" Foreground="White" BorderThickness="0">
        <StackPanel Orientation="Horizontal" Spacing="6">
            <Path Data="M 0 4 A 4 4 0 1 1 4 8" Stroke="White" StrokeThickness="1.5" Width="10" Height="10" VerticalAlignment="Center"/>
            <TextBlock Text="刷新" VerticalAlignment="Center"/>
        </StackPanel>
    </Button>
</StackPanel>
```

**核心特性**：Path 矢量图标可任意缩放、改色，适配不同分辨率与主题，是工业界面图标首选方案。

------

### 案例 30：Popup 自动消失的操作提示气泡

**对应控件**：`Popup`（内容控件）

**工业场景**：操作成功 / 失败轻量提示，右下角弹出，几秒后自动消失，不打断用户操作流程。

xaml:

```xaml
<Popup x:Name="tipPopup"
       Placement="BottomRight"
       AllowsTransparency="True"
       StaysOpen="True">
    <Border Background="#2E8B57" Foreground="White" Padding="12,8" CornerRadius="4" Margin="10">
        <StackPanel Orientation="Horizontal" Spacing="8">
            <Ellipse Width="8" Height="8" Fill="White" VerticalAlignment="Center"/>
            <TextBlock x:Name="txtTip" Text="保存成功" VerticalAlignment="Center"/>
        </StackPanel>
    </Border>
</Popup>
```

后台逻辑：

csharp:

```c#
private void ShowTip(string message)
{
    txtTip.Text = message;
    tipPopup.IsOpen = true;

    // 2秒后自动关闭
    DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
    timer.Tick += (s, e) =>
    {
        tipPopup.IsOpen = false;
        timer.Stop();
    };
    timer.Start();
}

// 调用示例
private void BtnSave_Click(object sender, RoutedEventArgs e)
{
    // 保存逻辑...
    ShowTip("参数保存成功");
}
```

**核心特性**：Popup 悬浮于所有控件之上，自定义内容自由度高，适合非阻塞式轻量提示。

------

## 十八、布局与预览类

### 案例 31：Viewbox 自适应报表预览

**对应控件**：`Viewbox`（继承 Decorator，单内容装饰器）

**工业场景**：固定尺寸的生产报表、标签预览，窗口缩放时整体等比缩放，保持排版不变形。

xaml:

```xaml
<Border BorderBrush="#DDD" BorderThickness="1" Margin="20" Width="500" Height="350">
    <Viewbox Stretch="Uniform" StretchDirection="Both">
        <!-- 固定尺寸的报表模板，Viewbox 自动整体缩放 -->
        <Grid Width="800" Height="600" Background="White" Margin="20">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>
            
            <TextBlock Grid.Row="0" Text="生产日报表" FontSize="24" FontWeight="Bold" HorizontalAlignment="Center"/>
            <DataGrid Grid.Row="1" AutoGenerateColumns="False" Margin="0,20">
                <!-- 报表表格内容 -->
            </DataGrid>
            <TextBlock Grid.Row="2" Text="打印时间：2026-06-16" HorizontalAlignment="Right" Foreground="Gray"/>
        </Grid>
    </Viewbox>
</Border>
```

**核心特性**：整体等比缩放，保留原始排版比例，是报表预览、标签打印预览的标准实现方案。

------

### 案例 32：HeaderedContentControl 统一样式分组卡片

**对应控件**：`HeaderedContentControl`（双内容模型基类）

**工业场景**：页面内多个参数分组，统一定义标题栏 + 内容边框样式，一次定义多处复用，保证界面风格一致。

首先定义全局样式：

xaml:

```xaml
<Window.Resources>
    <Style TargetType="HeaderedContentControl" x:Key="GroupCardStyle">
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="HeaderedContentControl">
                    <Border BorderBrush="#DDD" BorderThickness="1" CornerRadius="4">
                        <DockPanel>
                            <!-- 标题栏 -->
                            <Border DockPanel.Dock="Top" Background="#F5F5F5" Padding="8,5">
                                <ContentPresenter ContentSource="Header" FontWeight="Bold"/>
                            </Border>
                            <!-- 内容区 -->
                            <ContentPresenter DockPanel.Dock="Bottom" Padding="10"/>
                        </DockPanel>
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</Window.Resources>
```

使用方式：

xaml:

```xaml
<StackPanel Margin="20" Spacing="10">
    <HeaderedContentControl Style="{StaticResource GroupCardStyle}" Header="基础参数">
        <StackPanel Spacing="5">
            <TextBox Text="设备编号：AOI-003"/>
            <TextBox Text="所属产线：SMT-2线"/>
        </StackPanel>
    </HeaderedContentControl>

    <HeaderedContentControl Style="{StaticResource GroupCardStyle}" Header="通信参数">
        <StackPanel Spacing="5">
            <TextBox Text="PLC地址：192.168.1.50"/>
            <TextBox Text="端口：502"/>
        </StackPanel>
    </HeaderedContentControl>
</StackPanel>
```

**核心特性**：基于基类封装通用分组样式，替代 GroupBox/Expander 的默认外观，完全掌控视觉风格，适合定制化工业界面。

## 十九、菜单与快捷操作类

### 案例 33：ContextMenu 右键操作菜单

**对应控件**：`ContextMenu`、`MenuItem`（均为内容控件体系）

**工业场景**：设备卡片、数据行右键弹出操作菜单，快速执行启动、停止、查看详情、导出数据等操作。

xaml:

```xaml
<Border Width="200" BorderBrush="#DDD" BorderThickness="1" Padding="10" Margin="20">
    <Border.ContextMenu>
        <ContextMenu>
            <MenuItem Header="启动设备">
                <MenuItem.Icon>
                    <Path Data="M0 0 L8 4 L0 8Z" Fill="Green" Width="12" Height="12"/>
                </MenuItem.Icon>
            </MenuItem>
            <MenuItem Header="停止设备">
                <MenuItem.Icon>
                    <Rectangle Width="12" Height="12" Fill="Red"/>
                </MenuItem.Icon>
            </MenuItem>
            <Separator/>
            <MenuItem Header="查看详情"/>
            <MenuItem Header="导出报表"/>
        </ContextMenu>
    </Border.ContextMenu>
    
    <StackPanel>
        <TextBlock Text="设备 AOI-003" FontWeight="Bold"/>
        <TextBlock Text="运行中" Foreground="Green"/>
    </StackPanel>
</Border>
```

**核心特性**：每个 `MenuItem` 都是内容控件，`Header` 和 `Icon` 均可承载任意 UI 元素；右键自动定位到鼠标位置。

------

### 案例 34：MenuItem 带快捷键的顶部菜单栏

**对应控件**：`MenuItem`（继承 HeaderedItemsControl，内容模型一致）

**工业场景**：窗口顶部系统菜单，支持图标、快捷键提示、多级子菜单，符合工业软件操作习惯。

xaml:

```xaml
<Menu DockPanel.Dock="Top">
    <MenuItem Header="系统(_S)">
        <MenuItem Header="登录" InputGestureText="Ctrl+L"/>
        <MenuItem Header="退出" InputGestureText="Alt+F4"/>
    </MenuItem>
    
    <MenuItem Header="设备(_D)">
        <MenuItem Header="全部启动">
            <MenuItem.Icon>
                <Path Data="M0 0 L8 4 L0 8Z" Fill="Green" Width="12" Height="12"/>
            </MenuItem.Icon>
        </MenuItem>
        <MenuItem Header="全部停止"/>
        <Separator/>
        <MenuItem Header="参数配置" InputGestureText="F2"/>
    </MenuItem>
    
    <MenuItem Header="报表(_R)">
        <MenuItem Header="日报表"/>
        <MenuItem Header="月报表"/>
    </MenuItem>
</Menu>
```

**核心特性**：`InputGestureText` 显示快捷键提示，`_S` 支持 Alt 快捷访问，多级嵌套菜单结构清晰。

------

## 二十、列表项定制类

### 案例 35：ListBoxItem 设备状态列表项

**对应控件**：`ListBoxItem`（继承 ContentControl）

**工业场景**：设备列表，每一项同时显示状态指示灯、设备编号、当前产量、运行时长，信息一目了然。

xaml:

```xaml
<ListBox Width="280" Margin="20" BorderBrush="#DDD">
    <ListBoxItem Padding="8">
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>
            
            <Ellipse Grid.Column="0" Width="10" Height="10" Fill="Green" VerticalAlignment="Center" Margin="0,0,8,0"/>
            <StackPanel Grid.Column="1">
                <TextBlock Text="AOI-001" FontWeight="Bold"/>
                <TextBlock Text="运行时长：6h 23min" FontSize="11" Foreground="Gray"/>
            </StackPanel>
            <TextBlock Grid.Column="2" Text="3,428" Foreground="Green" VerticalAlignment="Center"/>
        </Grid>
    </ListBoxItem>
    
    <ListBoxItem Padding="8">
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>
            
            <Ellipse Grid.Column="0" Width="10" Height="10" Fill="Orange" VerticalAlignment="Center" Margin="0,0,8,0"/>
            <StackPanel Grid.Column="1">
                <TextBlock Text="AOI-002" FontWeight="Bold"/>
                <TextBlock Text="待机中" FontSize="11" Foreground="Gray"/>
            </StackPanel>
            <TextBlock Grid.Column="2" Text="1,256" Foreground="Gray" VerticalAlignment="Center"/>
        </Grid>
    </ListBoxItem>
</ListBox>
```

**核心特性**：列表项本质是内容控件，内部可放任意布局，适合定制化数据展示。

------

### 案例 36：ComboBoxItem 富内容下拉选项

**对应控件**：`ComboBoxItem`（继承 ContentControl）

**工业场景**：报警等级、设备类型下拉选择，每个选项带颜色标识和说明文字，直观易选。

xaml:

```xaml
<ComboBox Width="220" Margin="20" SelectedIndex="0">
    <ComboBoxItem>
        <StackPanel Orientation="Horizontal" Spacing="8">
            <Rectangle Width="12" Height="12" Fill="Red" VerticalAlignment="Center"/>
            <StackPanel>
                <TextBlock Text="一级报警" FontWeight="Bold"/>
                <TextBlock Text="紧急停机级别" FontSize="10" Foreground="Gray"/>
            </StackPanel>
        </StackPanel>
    </ComboBoxItem>
    
    <ComboBoxItem>
        <StackPanel Orientation="Horizontal" Spacing="8">
            <Rectangle Width="12" Height="12" Fill="Orange" VerticalAlignment="Center"/>
            <StackPanel>
                <TextBlock Text="二级报警" FontWeight="Bold"/>
                <TextBlock Text="参数超限预警" FontSize="10" Foreground="Gray"/>
            </StackPanel>
        </StackPanel>
    </ComboBoxItem>
    
    <ComboBoxItem>
        <StackPanel Orientation="Horizontal" Spacing="8">
            <Rectangle Width="12" Height="12" Fill="Green" VerticalAlignment="Center"/>
            <StackPanel>
                <TextBlock Text="提示信息" FontWeight="Bold"/>
                <TextBlock Text="正常运行提示" FontSize="10" Foreground="Gray"/>
            </StackPanel>
        </StackPanel>
    </ComboBoxItem>
</ComboBox>
```

------

## 二十一、动态模板切换类

### 案例 37：ContentControl + DataTemplateSelector 状态视图切换

**对应控件**：`ContentControl`

**工业场景**：根据设备运行状态，自动切换不同的详情卡片模板（正常 / 报警 / 停机），纯数据驱动，无需后台代码操作 UI。

第一步：定义模板选择器

csharp:

```c#
public class DeviceStateTemplateSelector : DataTemplateSelector
{
    public DataTemplate NormalTemplate { get; set; }
    public DataTemplate AlarmTemplate { get; set; }
    public DataTemplate StopTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is DeviceViewModel device)
        {
            return device.State switch
            {
                "Normal" => NormalTemplate,
                "Alarm" => AlarmTemplate,
                "Stop" => StopTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
        return base.SelectTemplate(item, container);
    }
}
```

第二步：XAML 定义模板并注册

xaml:

```xaml
<Window.Resources>
    <DataTemplate x:Key="NormalTpl">
        <Border Background="#F0FFF0" BorderBrush="Green" BorderThickness="1" Padding="10" Width="200">
            <StackPanel>
                <TextBlock Text="设备运行正常" Foreground="Green" FontWeight="Bold"/>
                <TextBlock Text="当前产量：3428" Margin="0,5"/>
                <TextBlock Text="温度：45℃"/>
            </StackPanel>
        </Border>
    </DataTemplate>

    <DataTemplate x:Key="AlarmTpl">
        <Border Background="#FFF0F0" BorderBrush="Red" BorderThickness="1" Padding="10" Width="200">
            <StackPanel>
                <TextBlock Text="温度超限报警" Foreground="Red" FontWeight="Bold"/>
                <TextBlock Text="当前温度：68℃" Margin="0,5"/>
                <TextBlock Text="已触发降速保护" Foreground="Orange"/>
            </StackPanel>
        </Border>
    </DataTemplate>

    <DataTemplate x:Key="StopTpl">
        <Border Background="#F5F5F5" BorderBrush="Gray" BorderThickness="1" Padding="10" Width="200">
            <StackPanel>
                <TextBlock Text="设备已停机" Foreground="Gray" FontWeight="Bold"/>
                <TextBlock Text="停机时长：2h 15min" Margin="0,5"/>
            </StackPanel>
        </Border>
    </DataTemplate>

    <local:DeviceStateTemplateSelector x:Key="StateSelector"
                                       NormalTemplate="{StaticResource NormalTpl}"
                                       AlarmTemplate="{StaticResource AlarmTpl}"
                                       StopTemplate="{StaticResource StopTpl}"/>
</Window.Resources>
```

第三步：使用

xaml:

```xaml
<ContentControl Content="{Binding CurrentDevice}"
                ContentTemplateSelector="{StaticResource StateSelector}"
                Margin="20"/>
```

**核心价值**：状态变化时 UI 自动切换，视图与逻辑完全解耦，符合 MVVM 设计思想。

------

## 二十二、叠加层与装饰类

### 案例 38：Grid 叠加遮罩层（加载 / 禁用状态）

**对应基类**：单内容装饰思想，Grid 多子层叠加实现

**工业场景**：设备数据加载中、设备离线时，叠加半透明遮罩与提示文字，禁止用户操作底层内容。

xaml:

```xaml
<Grid Width="250" Margin="20">
    <!-- 底层：正常内容 -->
    <Border BorderBrush="#DDD" BorderThickness="1" Padding="10">
        <StackPanel>
            <TextBlock Text="设备参数配置" FontWeight="Bold"/>
            <TextBox Text="192.168.1.50" Margin="0,5"/>
            <TextBox Text="502"/>
        </StackPanel>
    </Border>

    <!-- 顶层：加载遮罩，控制Visibility即可切换 -->
    <Border x:Name="maskOverlay" Background="#80000000" Visibility="Visible">
        <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
            <TextBlock Text="数据加载中..." Foreground="White" FontSize="14"/>
            <TextBlock Text="请稍候" Foreground="White" Opacity="0.7" FontSize="12" HorizontalAlignment="Center" Margin="0,3"/>
        </StackPanel>
    </Border>
</Grid>
```

**核心特性**：不破坏原有布局结构，通过控制遮罩层可见性实现状态切换，是工业界面常用的加载 / 禁用方案。

------

## 二十三、选项卡定制类

### 案例 39：TabItem 带关闭按钮的选项卡

**对应控件**：`TabItem`（继承 HeaderedContentControl）

**工业场景**：多文档界面，打开多个设备详情页，标签页标题带关闭按钮，支持动态关闭。

xaml:

```xaml
<TabControl Margin="20">
    <TabItem>
        <!-- 自定义标题：文本 + 关闭按钮 -->
        <TabItem.Header>
            <StackPanel Orientation="Horizontal" Spacing="6">
                <TextBlock Text="设备监控" VerticalAlignment="Center"/>
                <Button Content="×" FontSize="10" Width="16" Height="16" Padding="0"
                        VerticalAlignment="Center" BorderThickness="0" Background="Transparent"/>
            </StackPanel>
        </TabItem.Header>
        
        <!-- 选项卡内容 -->
        <TextBlock Text="监控画面区域" HorizontalAlignment="Center" VerticalAlignment="Center"/>
    </TabItem>
    
    <TabItem>
        <TabItem.Header>
            <StackPanel Orientation="Horizontal" Spacing="6">
                <TextBlock Text="参数配置" VerticalAlignment="Center"/>
                <Button Content="×" FontSize="10" Width="16" Height="16" Padding="0"
                        VerticalAlignment="Center" BorderThickness="0" Background="Transparent"/>
            </StackPanel>
        </TabItem.Header>
        <TextBlock Text="配置表单区域" HorizontalAlignment="Center" VerticalAlignment="Center"/>
    </TabItem>
</TabControl>
```

**核心特性**：`Header` 是完整的内容控件，可任意组合标题元素；内容区承载完整页面逻辑。

------

## 二十四、向导式流程类

### 案例 40：ContentControl 分步配置向导

**对应控件**：`ContentControl`

**工业场景**：设备初始化向导、参数配置向导，分步骤引导用户填写，上一步 / 下一步切换视图。

xaml:

```xaml
<Window.Resources>
    <DataTemplate x:Key="Step1Tpl">
        <StackPanel Spacing="8">
            <TextBlock Text="步骤1：基础信息" FontSize="14" FontWeight="Bold"/>
            <TextBox Text="设备编号"/>
            <TextBox Text="所属产线"/>
        </StackPanel>
    </DataTemplate>

    <DataTemplate x:Key="Step2Tpl">
        <StackPanel Spacing="8">
            <TextBlock Text="步骤2：通信配置" FontSize="14" FontWeight="Bold"/>
            <TextBox Text="PLC地址"/>
            <TextBox Text="端口号"/>
        </StackPanel>
    </DataTemplate>

    <DataTemplate x:Key="Step3Tpl">
        <StackPanel Spacing="8">
            <TextBlock Text="步骤3：完成" FontSize="14" FontWeight="Bold" Foreground="Green"/>
            <TextBlock Text="配置已保存，设备即将启动"/>
        </StackPanel>
    </DataTemplate>
</Window.Resources>
```

主体布局：

xaml:

```xaml
<Grid Width="350" Margin="20">
    <Grid.RowDefinitions>
        <RowDefinition Height="*"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>

    <!-- 内容区：根据当前步骤切换模板 -->
    <ContentControl Grid.Row="0" x:Name="wizardContent">
        <ContentControl.Style>
            <Style TargetType="ContentControl">
                <Setter Property="ContentTemplate" Value="{StaticResource Step1Tpl}"/>
                <Style.Triggers>
                    <DataTrigger Binding="{Binding CurrentStep}" Value="2">
                        <Setter Property="ContentTemplate" Value="{StaticResource Step2Tpl}"/>
                    </DataTrigger>
                    <DataTrigger Binding="{Binding CurrentStep}" Value="3">
                        <Setter Property="ContentTemplate" Value="{StaticResource Step3Tpl}"/>
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </ContentControl.Style>
    </ContentControl>

    <!-- 底部按钮 -->
    <StackPanel Grid.Row="1" Orientation="Horizontal" HorizontalAlignment="Right" Spacing="8" Margin="0,15,0,0">
        <Button Content="上一步" Width="80" Command="{Binding PrevCommand}" IsEnabled="{Binding CanPrev}"/>
        <Button Content="下一步" Width="80" Command="{Binding NextCommand}"/>
    </StackPanel>
</Grid>
```

**核心价值**：单一容器承载多步骤内容，步骤逻辑与视图分离，易于扩展新增步骤，是工业配置向导的标准实现方案。



