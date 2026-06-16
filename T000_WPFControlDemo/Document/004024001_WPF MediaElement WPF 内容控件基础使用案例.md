# 004024001_WPF `MediaElement` WPF 内容控件基础使用案例

WPF 内容控件的核心特征是**继承自 `ContentControl`，仅包含一个 `Content` 属性**，内容不局限于纯文本，可以是任意 UI 元素（布局面板、图片、图标、嵌套控件等）。常见代表：`Button`、`Label`、`CheckBox`、`RadioButton`、`GroupBox`、`Expander`、`ContentControl`。

以下均为可直接复制运行的简单案例，贴合工业软件常用场景。

------

## 案例 1：Button 自定义内容（图标 + 文字）

最常用的内容控件场景：按钮不再是纯文本，而是「图标 + 文字」的组合。

xaml:

```xaml
<Button Width="120" Height="36" Click="BtnStart_Click">
    <!-- Content 内放 StackPanel，实现图文组合 -->
    <StackPanel Orientation="Horizontal" Spacing="6">
        <Path Data="M 0 0 L 8 4 L 0 8 Z" Fill="White" Width="8" Height="8" VerticalAlignment="Center"/>
        <TextBlock Text="启动设备" Foreground="White" VerticalAlignment="Center"/>
    </StackPanel>
</Button>
```

**说明**：

- `Content` 属性默认支持隐式赋值，直接写在控件内部即可
- 内容可以是任意 WPF 元素，不限于文本
- 工业场景常用于：启动 / 停止按钮、功能操作按钮

------

## 案例 2：Label 带快捷键的标签

`Label` 是专门的标签类内容控件，支持访问键（Alt + 快捷键快速聚焦关联控件），这是它和 `TextBlock` 的核心区别。

xaml:

```xaml
<StackPanel Width="300" Margin="20">
    <!-- Alt+P 自动聚焦后面的 TextBox -->
    <Label Content="设备编号(_P)：" Target="{Binding ElementName=txtEqpId}"/>
    <TextBox x:Name="txtEqpId" Text="AOI-003" Margin="0,2,0,10"/>

    <Label Content="设备IP(_I)：" Target="{Binding ElementName=txtIp}"/>
    <TextBox x:Name="txtIp" Text="192.168.1.50" Margin="0,2"/>
</StackPanel>
```

**说明**：

- `_P` 定义访问键，按 `Alt+P` 自动跳转到对应输入框
- 工业表单场景常用，提升键盘操作效率

------

## 案例 3：CheckBox 富内容选项

复选框的 `Content` 不止能放文字，可以做「标题 + 描述」的双层选项。

xaml:

```xaml
<CheckBox Margin="20" IsChecked="True">
    <StackPanel>
        <TextBlock Text="启用产能自动上报" FontWeight="Bold" FontSize="14"/>
        <TextBlock Text="每小时自动向MES上传时段产能数据" Foreground="Gray" FontSize="12" Margin="0,2"/>
    </StackPanel>
</CheckBox>
```

同理 `RadioButton` 也支持完全一样的自定义内容：

xaml:

```xaml
<RadioButton GroupName="Shift" IsChecked="True">
    <StackPanel Orientation="Horizontal" Spacing="8">
        <Ellipse Width="8" Height="8" Fill="Orange" VerticalAlignment="Center"/>
        <TextBlock Text="白班 (08:00-20:00)"/>
    </StackPanel>
</RadioButton>
```

------

## 案例 4：GroupBox 分组容器

`GroupBox` 是带标题的分组容器，继承自 `HeaderedContentControl`（标题 + 内容双内容模型），适合表单参数分组。

xaml:

```xaml
<GroupBox Header="通信参数" Margin="20" Padding="10" BorderBrush="#DDD">
    <!-- Content 内放完整表单布局 -->
    <StackPanel Spacing="8">
        <TextBox Text="PLC地址：192.168.1.50"/>
        <TextBox Text="端口号：502"/>
        <TextBox Text="超时时间：3000ms"/>
        <CheckBox Content="启用断线重连"/>
    </StackPanel>
</GroupBox>
```

**说明**：

- `Header` 是标题，`Content` 是主体内容
- 工业场景常用于：设备参数、系统配置、报警设置等分组展示

------

## 案例 5：Expander 可折叠分组

同样继承 `HeaderedContentControl`，比 GroupBox 多了展开 / 折叠功能，适合次要参数分组。

xaml:

```xaml
<Expander Header="高级参数" Margin="20" IsExpanded="False" BorderBrush="#DDD" BorderThickness="1" Padding="8">
    <StackPanel Spacing="6">
        <CheckBox Content="启用日志记录"/>
        <CheckBox Content="启用调试模式"/>
        <TextBox Text="日志保留天数：30"/>
    </StackPanel>
</Expander>
```

------

## 案例 6：ContentControl 动态内容切换

`ContentControl` 是最基础的内容控件，本身无额外功能，核心作用是**动态承载内容**，MVVM 场景下常用于页面切换、状态视图切换。

### 基础用法

xaml:

```xaml
<ContentControl x:Name="contentHost" Margin="20">
    <ContentControl.Content>
        <!-- 默认内容 -->
        <Border Background="#F5F5F5" Padding="20">
            <TextBlock Text="请选择设备查看详情" HorizontalAlignment="Center"/>
        </Border>
    </ContentControl.Content>
</ContentControl>
```

### 后台动态切换

csharp:

```c#
// 选中设备后，切换为详情视图
private void SelectDevice()
{
    // 直接替换 Content 为任意 UI 元素
    StackPanel detailPanel = new StackPanel();
    detailPanel.Children.Add(new TextBlock { Text = "设备编号：AOI-003" });
    detailPanel.Children.Add(new TextBlock { Text = "运行状态：正常" });
    
    contentHost.Content = detailPanel;
}
```

------

## 案例 7：DataTemplate 绑定内容（MVVM 入门）

内容控件配合 `ContentTemplate` 数据模板，可实现「数据驱动 UI」，是 MVVM 的核心用法。

xaml:

```xaml
<Window.Resources>
    <!-- 数据模板：定义数据如何显示 -->
    <DataTemplate x:Key="DeviceInfoTemplate">
        <StackPanel Spacing="4">
            <TextBlock Text="{Binding EqpId}" FontSize="16" FontWeight="Bold"/>
            <TextBlock Text="{Binding Status}"/>
            <TextBlock Text="{Binding Yield, StringFormat=当前产量：{0}}"/>
        </StackPanel>
    </DataTemplate>
</Window.Resources>

<Grid>
    <!-- Content 绑定数据源，ContentTemplate 指定显示样式 -->
    <ContentControl Content="{Binding CurrentDevice}"
                    ContentTemplate="{StaticResource DeviceInfoTemplate}"
                    Margin="20"/>
</Grid>
```

csharp:

```c#
// ViewModel 中只需要赋值数据，UI 自动渲染
public DeviceInfo CurrentDevice { get; set; } = new DeviceInfo
{
    EqpId = "AOI-003",
    Status = "运行中",
    Yield = 3256
};
```

------

## 核心总结

1. 所有内容控件都遵循「**单一 Content + 任意元素**」的规则
2. `HeaderedContentControl` 系列（GroupBox、Expander、TabItem）额外多一个 `Header` 标题
3. 纯文本只是最简单的用法，实际项目中几乎都会用布局面板组合复杂内容
4. `ContentControl` 本身无外观，是 MVVM 动态视图切换的核心载体