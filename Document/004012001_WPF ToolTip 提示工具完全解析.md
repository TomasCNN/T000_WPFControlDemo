# 004012001_WPF ToolTip 提示工具完全解析

ToolTip 是 WPF 中用于显示**悬浮提示信息**的核心控件，它继承自 `ContentControl`，因此可以承载任意类型的内容（文本、图像、富文本甚至自定义控件）。在工业自动化场景中，ToolTip 是提升界面易用性的关键组件，用于说明按钮功能、参数范围、错误原因等信息。

## 一、官方类定义与继承关系

### 1.1 基本元数据

| 项           | 官方值                                                       | 说明                                         |
| :----------- | :----------------------------------------------------------- | :------------------------------------------- |
| **命名空间** | `System.Windows.Controls`                                    | WPF 控件标准命名空间                         |
| **程序集**   | `PresentationFramework.dll`                                  | WPF 核心框架程序集                           |
| **继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ContentControl → ToolTip` | 完整继承层级                                 |
| **线程安全** | 仅 UI 线程安全                                               | 所有成员只能在创建它的 Dispatcher 线程上访问 |
| **支持版本** | .NET Framework 3.0+ / .NET Core 3.0+ / .NET 5+               | 所有 WPF 支持版本                            |

### 1.2 官方类签名（带特性）

csharp:

```c#
[System.Windows.LocalizabilityAttribute(System.Windows.LocalizationCategory.ToolTip)]
[System.Windows.StyleTypedPropertyAttribute(Property = "Style", StyleTargetType = typeof(ToolTip))]
[System.Windows.TemplatePartAttribute(Name = "PART_Popup", Type = typeof(System.Windows.Controls.Primitives.Popup))]
[System.Windows.DefaultEventAttribute("Opened")]
[System.Windows.ContentPropertyAttribute("Content")]
public class ToolTip : System.Windows.Controls.ContentControl
```

#### 特性详解

1. **`LocalizabilityAttribute(LocalizationCategory.ToolTip)`**
   - 标记该控件为提示类内容
   - 指导本地化工具将其内容视为可本地化资源
   - 工业场景中用于多语言提示的自动翻译
2. **`TemplatePartAttribute(Name="PART_Popup", Type=typeof(Popup))`**
   - 控件模板必须包含一个名为 `PART_Popup` 的 `Popup` 控件
   - 该 Popup 用于承载提示内容并实现弹出效果
   - 自定义模板时缺少此部分将导致 ToolTip 无法正常显示
3. **`ContentPropertyAttribute("Content")`**
   - 指定默认内容属性为 `Content`
   - 支持 XAML 简化语法：`<ToolTip>提示内容</ToolTip>`

### 1.3 继承关系深度解析

| 父类                   | 提供的核心能力       | ToolTip 中的体现                                     |
| :--------------------- | :------------------- | :--------------------------------------------------- |
| **`ContentControl`**   | 单内容容器           | ToolTip 可以承载任意类型的内容（文本、图像、控件等） |
| **`Control`**          | 通用控件功能         | 支持样式、模板、背景、前景等通用属性                 |
| **`FrameworkElement`** | 布局、数据绑定、样式 | 支持数据绑定和 MVVM 模式                             |
| **`UIElement`**        | 输入、渲染、可见性   | 处理鼠标、键盘输入，控制显示隐藏                     |

**核心本质**：ToolTip 本质上是一个**封装了 Popup 弹出逻辑的 ContentControl**，它的所有功能都是围绕 "在指定位置弹出内容" 设计的。

------

## 二、核心成员官方解析

### 2.1 核心属性

#### 显示控制属性

| 属性            | 类型   | 默认值  | 说明                                               |
| :-------------- | :----- | :------ | :------------------------------------------------- |
| `IsOpen`        | `bool` | `false` | 获取或设置 ToolTip 是否显示                        |
| `StaysOpen`     | `bool` | `false` | 是否保持打开状态直到手动关闭。默认鼠标移开自动关闭 |
| `HasDropShadow` | `bool` | `true`  | 是否显示阴影效果                                   |

#### 位置控制属性

| 属性                 | 类型            | 默认值  | 说明                                                         |
| :------------------- | :-------------- | :------ | :----------------------------------------------------------- |
| `Placement`          | `PlacementMode` | `Mouse` | 提示相对于目标的位置。枚举值：Mouse、Bottom、Top、Left、Right、Center 等 |
| `PlacementTarget`    | `UIElement`     | `null`  | 提示的目标控件。默认是设置了 ToolTip 的控件                  |
| `HorizontalOffset`   | `double`        | `0`     | 水平方向偏移量                                               |
| `VerticalOffset`     | `double`        | `0`     | 垂直方向偏移量                                               |
| `PlacementRectangle` | `Rect`          | `Empty` | 相对于目标的矩形区域，提示将显示在该区域的指定位置           |

#### 内容属性

| 属性              | 类型           | 默认值 | 说明                                         |
| :---------------- | :------------- | :----- | :------------------------------------------- |
| `Content`         | `object`       | `null` | 提示内容。可以是字符串、图像、控件等任意对象 |
| `ContentTemplate` | `DataTemplate` | `null` | 内容的数据模板。用于动态绑定数据             |

### 2.2 核心事件

| 事件     | 说明                  |
| :------- | :-------------------- |
| `Opened` | 当 ToolTip 显示时触发 |
| `Closed` | 当 ToolTip 关闭时触发 |

### 2.3 关键重写方法

| 方法                        | 说明                                        |
| :-------------------------- | :------------------------------------------ |
| `OnOpened(RoutedEventArgs)` | 当 ToolTip 显示时调用。可重写自定义显示逻辑 |
| `OnClosed(RoutedEventArgs)` | 当 ToolTip 关闭时调用。可重写自定义关闭逻辑 |

------

## 三、核心功能与工作原理

### 3.1 ToolTip 与 ToolTipService 的关系

**最重要的概念**：WPF 中几乎所有控件的 ToolTip 功能都是通过 `ToolTipService` 附加属性实现的，而不是直接使用 ToolTip 控件。

`ToolTipService` 提供了以下核心附加属性：

- `ToolTipService.ToolTip`：设置控件的提示内容
- `ToolTipService.InitialShowDelay`：鼠标悬浮后显示提示的延迟时间（默认 500ms）
- `ToolTipService.ShowDuration`：提示显示的持续时间（默认 5000ms）
- `ToolTipService.BetweenShowDelay`：两次提示之间的间隔时间（默认 100ms）
- `ToolTipService.ShowOnDisabled`：是否在控件禁用时显示提示（默认 false）
- `ToolTipService.IsEnabled`：是否启用提示功能（默认 true）

**工作原理**：

1. 当你写 `<Button ToolTip="启动设备"/>` 时，实际上是设置了 `ToolTipService.ToolTip` 附加属性
2. WPF 会自动将字符串内容包装成一个 ToolTip 控件
3. ToolTipService 负责监听鼠标事件，在适当的时机显示和隐藏 ToolTip
4. 只有当你需要自定义 ToolTip 的样式、位置或行为时，才需要显式创建 ToolTip 控件

### 3.2 核心功能

1. **自动显示隐藏**：鼠标悬浮到控件上自动显示，移开自动隐藏
2. **延迟显示**：避免鼠标快速滑过时误触发提示
3. **任意内容支持**：可以显示文本、图像、富文本、表格甚至自定义控件
4. **灵活的位置控制**：支持 10 种不同的放置模式和自定义偏移
5. **全局配置**：通过 ToolTipService 可以配置整个应用的提示行为
6. **样式与模板定制**：可以完全自定义提示的外观

------

## 四、基础使用方法

### 4.1 最简单的文本提示

直接设置控件的 `ToolTip` 属性即可，这是最常用的方式。

xaml:

```xaml
<!-- 基础文本提示 -->
<Button Content="启动" ToolTip="启动设备运行"/>

<!-- 带换行的提示 -->
<Button Content="停止" ToolTip="停止当前运行的任务&#x0a;生产数据将被保存"/>
```

### 4.2 显式创建 ToolTip 控件

当需要自定义 ToolTip 的属性（如位置、延迟、样式）时，需要显式创建 ToolTip 控件。

xaml:

```xaml
<Button Content="编辑">
    <Button.ToolTip>
        <ToolTip Content="编辑设备参数"
                 Placement="Right"
                 HorizontalOffset="5"
                 ToolTipService.InitialShowDelay="200"
                 ToolTipService.ShowDuration="10000"/>
    </Button.ToolTip>
</Button>
```

### 4.3 禁用控件的提示

工业场景中经常需要给禁用的控件显示提示，说明禁用原因。

xaml:

```xaml
<Button Content="导出"
        IsEnabled="False"
        ToolTip="没有可导出的数据"
        ToolTipService.ShowOnDisabled="True"/>
```

------

## 五、高级使用实例

### 5.1 带图标的复合提示

ToolTip 是 ContentControl，可以承载任意 UI 元素。

xaml:

```xaml
<Button Content="导出Excel">
    <Button.ToolTip>
        <StackPanel Orientation="Horizontal" Margin="2">
            <Image Source="/Images/excel.png" Width="16" Height="16" Margin="0,0,5,0"/>
            <TextBlock Text="导出当前数据到Excel文件"/>
        </StackPanel>
    </Button.ToolTip>
</Button>
```

### 5.2 富文本提示

支持粗体、斜体、颜色等富文本格式。

xaml:

```xaml
<TextBox x:Name="txtTemperature" Text="25.5">
    <TextBox.ToolTip>
        <StackPanel>
            <TextBlock Text="温度设定值" FontWeight="Bold" FontSize="12"/>
            <TextBlock Text="范围：0 ~ 100 ℃" Foreground="Gray"/>
            <TextBlock Text="精度：±0.1 ℃" Foreground="Gray"/>
        </StackPanel>
    </TextBox.ToolTip>
</TextBox>
```

### 5.3 工业风格自定义 ToolTip 样式

工业界面通常使用深色背景、高对比度的提示样式。

xaml:

```xaml
<!-- App.xaml 全局样式 -->
<Style TargetType="ToolTip">
    <Setter Property="Background" Value="#FF212121"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="BorderBrush" Value="#FF424242"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Padding" Value="8,6"/>
    <Setter Property="FontSize" Value="12"/>
    <Setter Property="HasDropShadow" Value="True"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="ToolTip">
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="3"
                        Padding="{TemplateBinding Padding}">
                    <ContentPresenter/>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

### 5.4 数据绑定的动态提示

使用 ContentTemplate 实现动态数据绑定的提示。

xaml:

```xaml
<DataGrid ItemsSource="{Binding Devices}">
    <DataGrid.Columns>
        <DataGridTextColumn Header="设备名称" Binding="{Binding Name}">
            <DataGridTextColumn.CellStyle>
                <Style TargetType="DataGridCell">
                    <Setter Property="ToolTip">
                        <Setter.Value>
                            <ToolTip>
                                <ToolTip.ContentTemplate>
                                    <DataTemplate>
                                        <StackPanel>
                                            <TextBlock Text="{Binding Name}" FontWeight="Bold"/>
                                            <TextBlock Text="{Binding Id, StringFormat=ID: {0}}"/>
                                            <TextBlock Text="{Binding IP, StringFormat=IP: {0}}"/>
                                            <TextBlock Text="{Binding Status, StringFormat=状态: {0}}"/>
                                        </StackPanel>
                                    </DataTemplate>
                                </ToolTip.ContentTemplate>
                            </ToolTip>
                        </Setter.Value>
                    </Setter>
                </Style>
            </DataGridTextColumn.CellStyle>
        </DataGridTextColumn>
    </DataGrid.Columns>
</DataGrid>
```

### 5.5 全局 ToolTip 配置

在 App.xaml 中配置整个应用的提示行为。

xaml:

```xaml
<Application.Resources>
    <!-- 全局 ToolTip 配置 -->
    <Style TargetType="FrameworkElement">
        <Setter Property="ToolTipService.InitialShowDelay" Value="300"/>
        <Setter Property="ToolTipService.ShowDuration" Value="8000"/>
        <Setter Property="ToolTipService.ShowOnDisabled" Value="True"/>
    </Style>
</Application.Resources>
```

### 5.6 手动控制 ToolTip 显示

通过绑定 `IsOpen` 属性手动控制提示的显示和隐藏。

xaml:

```xaml
<ToolTip x:Name="errorTip"
         Content="输入值超出范围"
         Placement="Bottom"
         PlacementTarget="{Binding ElementName=txtValue}"
         IsOpen="{Binding IsValueInvalid}"/>

<TextBox x:Name="txtValue" Text="{Binding Value, UpdateSourceTrigger=PropertyChanged}"/>
```

------

## 六、工业场景最佳实践

### 6.1 提示内容规范

1. **简洁明了**：提示内容不超过 3 行，重点信息突出
2. **包含必要信息**：参数提示要包含范围、单位和精度；操作提示要说明后果
3. **统一风格**：整个应用使用相同的提示样式和格式
4. **禁用控件必须有提示**：说明禁用原因，如 "设备未连接"、"权限不足"

### 6.2 工业场景常用示例

#### 参数输入框提示

xaml:

```xaml
<TextBox Text="{Binding Pressure}"
         ToolTip="设定压力&#x0a;范围：0 ~ 10 bar&#x0a;精度：±0.01 bar"/>
```

#### 操作按钮提示

xaml:

```xaml
<Button Content="急停"
        Style="{StaticResource EmergencyStopButtonStyle}"
        ToolTip="紧急停止所有设备&#x0a;所有运动部件将立即停止"/>
```

#### 状态指示提示

xaml:

```xaml
<Ellipse Fill="Green"
         Width="16" Height="16"
         ToolTip="设备运行正常&#x0a;当前产量：1234 件"/>
```

### 6.3 常见问题与解决方案

1. **ToolTip 显示在屏幕外**
   - 设置 `Placement="Mouse"` 或 `Placement="Center"`
   - 调整 `HorizontalOffset` 和 `VerticalOffset` 属性
2. **禁用控件不显示提示**
   - 设置 `ToolTipService.ShowOnDisabled="True"`
3. **提示显示时间太短**
   - 设置 `ToolTipService.ShowDuration="20000"`（单位毫秒）
4. **提示延迟太长**
   - 设置 `ToolTipService.InitialShowDelay="200"`（单位毫秒）

### 6.4 注意事项

1. **不要在 ToolTip 中放交互控件**：如按钮、输入框等，因为 ToolTip 默认会在鼠标移开时自动关闭
2. **不要放太复杂的内容**：ToolTip 是轻量级提示控件，复杂内容会影响性能
3. **注意可访问性**：确保提示内容对屏幕阅读器友好
4. **避免过度使用**：只在必要的地方使用提示，不要每个控件都加