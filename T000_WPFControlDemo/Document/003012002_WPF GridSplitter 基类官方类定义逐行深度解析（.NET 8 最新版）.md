# 003012002_WPF GridSplitter 基类官方类定义逐行深度解析（.NET 8 最新版）

基于 **.NET 8 官方开源源码** 完整解析，包含所有公共 / 受保护成员、特性、设计意图和工业场景应用。`GridSplitter`是 WPF**最常用的布局控件之一**，专门用于在`Grid`中拖动分割列或行的大小，是工业上位机界面中实现可调整布局的核心控件。

------

## 一、GridSplitter 在 WPF 类层次结构中的位置

plaintext:

```tex
System.Object
  ↳ System.Windows.Threading.DispatcherObject
    ↳ System.Windows.DependencyObject
      ↳ System.Windows.Media.Visual
        ↳ System.Windows.UIElement
          ↳ System.Windows.FrameworkElement
            ↳ System.Windows.Controls.Control
              ↳ System.Windows.Controls.Primitives.Thumb
                ↳ System.Windows.Controls.GridSplitter  ← 我们今天的主角
```

**核心设计意义**：

- 继承自`Thumb`：拥有完整的鼠标拖动逻辑
- 专门优化`Grid`布局：自动检测所在的 Grid 和相邻的列 / 行
- 支持多种分割模式：水平分割、垂直分割、基于对齐方式自动判断
- 提供预览模式：拖动时只显示预览线，释放后再调整大小，提升复杂界面的流畅性

------

## 二、完整官方类定义（.NET 8 源码级）

csharp:

```c#
using System.Windows.Automation.Peers;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows.Controls
{
    /// <summary>
    /// 表示一个可以拖动以调整Grid列或行大小的控件
    /// </summary>
    /// <remarks>
    /// GridSplitter 必须放在Grid的单独列或行中，并且该列或行的宽度/高度应设置为Auto。
    /// 它会自动调整相邻列或行的大小。
    /// </remarks>
    [DefaultEvent("DragDelta")]
    [Localizability(LocalizationCategory.None)]
    public class GridSplitter : Thumb
    {
        // ==============================================
        // 依赖属性定义
        // ==============================================
        public static readonly DependencyProperty ResizeDirectionProperty;
        public static readonly DependencyProperty ResizeBehaviorProperty;
        public static readonly DependencyProperty ShowsPreviewProperty;
        public static readonly DependencyProperty DragIncrementProperty;
        public static readonly DependencyProperty KeyboardIncrementProperty;

        // ==============================================
        // 静态构造函数
        // ==============================================
        static GridSplitter()
        {
            // 注册依赖属性
            ResizeDirectionProperty = DependencyProperty.Register(
                nameof(ResizeDirection),
                typeof(GridResizeDirection),
                typeof(GridSplitter),
                new FrameworkPropertyMetadata(GridResizeDirection.Auto),
                new ValidateValueCallback(IsValidResizeDirection));

            ResizeBehaviorProperty = DependencyProperty.Register(
                nameof(ResizeBehavior),
                typeof(GridResizeBehavior),
                typeof(GridSplitter),
                new FrameworkPropertyMetadata(GridResizeBehavior.BasedOnAlignment));

            ShowsPreviewProperty = DependencyProperty.Register(
                nameof(ShowsPreview),
                typeof(bool),
                typeof(GridSplitter),
                new FrameworkPropertyMetadata(false));

            DragIncrementProperty = DependencyProperty.Register(
                nameof(DragIncrement),
                typeof(double),
                typeof(GridSplitter),
                new FrameworkPropertyMetadata(1.0),
                new ValidateValueCallback(IsValidDragIncrement));

            KeyboardIncrementProperty = DependencyProperty.Register(
                nameof(KeyboardIncrement),
                typeof(double),
                typeof(GridSplitter),
                new FrameworkPropertyMetadata(10.0),
                new ValidateValueCallback(IsValidDragIncrement));

            // 重写默认样式键
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GridSplitter),
                new FrameworkPropertyMetadata(typeof(GridSplitter)));

            // 注册键盘快捷键
            KeyboardNavigation.IsTabStopProperty.OverrideMetadata(
                typeof(GridSplitter),
                new FrameworkPropertyMetadata(true));
        }

        // ==============================================
        // 公共构造函数
        // ==============================================
        public GridSplitter();

        // ==============================================
        // 公共属性
        // ==============================================
        [Bindable(true)]
        [Category("Behavior")]
        public GridResizeDirection ResizeDirection { get; set; }

        [Bindable(true)]
        [Category("Behavior")]
        public GridResizeBehavior ResizeBehavior { get; set; }

        [Bindable(true)]
        [Category("Behavior")]
        public bool ShowsPreview { get; set; }

        [Bindable(true)]
        [Category("Behavior")]
        public double DragIncrement { get; set; }

        [Bindable(true)]
        [Category("Behavior")]
        public double KeyboardIncrement { get; set; }

        // ==============================================
        // 受保护方法
        // ==============================================
        protected override AutomationPeer OnCreateAutomationPeer();
        protected override void OnInitialized(EventArgs e);
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo);
        protected override void OnDragStarted(DragStartedEventArgs e);
        protected override void OnDragDelta(DragDeltaEventArgs e);
        protected override void OnDragCompleted(DragCompletedEventArgs e);
        protected override void OnKeyDown(KeyEventArgs e);
    }
}
```

------

## 三、类级特性逐行解析

### 1. `[DefaultEvent("DragDelta")]`

csharp:

```c#
[DefaultEvent("DragDelta")]
```

- **作用**：指定控件的默认事件
- **设计意图**：在 Visual Studio 设计器中双击 GridSplitter 时，自动生成`DragDelta`事件的处理方法
- **工业场景意义**：符合开发人员直觉，因为 GridSplitter 最常用的操作就是响应拖动事件

### 2. `[Localizability(LocalizationCategory.None)]`

csharp:

```c#
[Localizability(LocalizationCategory.None)]
```

- **作用**：本地化特性，告诉本地化工具该类不需要本地化
- **设计意图**：GridSplitter 是纯布局控件，没有需要本地化的文本内容

------

## 四、静态构造函数解析（核心初始化逻辑）

静态构造函数是 GridSplitter 最关键的部分，负责所有核心依赖属性的注册。

### 1. `ResizeDirectionProperty` 注册

csharp:

```c#
ResizeDirectionProperty = DependencyProperty.Register(
    nameof(ResizeDirection),
    typeof(GridResizeDirection),
    typeof(GridSplitter),
    new FrameworkPropertyMetadata(GridResizeDirection.Auto),
    new ValidateValueCallback(IsValidResizeDirection));
```

- **类型**：`GridResizeDirection`枚举
- **默认值**：`GridResizeDirection.Auto`
- **验证回调**：`IsValidResizeDirection`，确保值是有效的枚举值
- **核心作用**：控制 GridSplitter 是分割列还是分割行

### 2. `ResizeBehaviorProperty` 注册

csharp:

```c#
ResizeBehaviorProperty = DependencyProperty.Register(
    nameof(ResizeBehavior),
    typeof(GridResizeBehavior),
    typeof(GridSplitter),
    new FrameworkPropertyMetadata(GridResizeBehavior.BasedOnAlignment));
```

- **类型**：`GridResizeBehavior`枚举
- **默认值**：`GridResizeBehavior.BasedOnAlignment`
- **核心作用**：控制 GridSplitter 调整哪些相邻列或行的大小
- **这是 GridSplitter 最容易被误解的属性**，也是大多数人用不好 GridSplitter 的原因

### 3. `ShowsPreviewProperty` 注册

csharp:

```c#
ShowsPreviewProperty = DependencyProperty.Register(
    nameof(ShowsPreview),
    typeof(bool),
    typeof(GridSplitter),
    new FrameworkPropertyMetadata(false));
```

- **类型**：`bool`
- **默认值**：`false`（实时调整大小）
- **核心作用**：控制拖动时是实时调整大小还是只显示预览线
- **工业场景意义**：当面板内容复杂时，实时调整会导致卡顿，设置为`true`可以大幅提升流畅性

### 4. `DragIncrementProperty` 注册

csharp:

```c#
DragIncrementProperty = DependencyProperty.Register(
    nameof(DragIncrement),
    typeof(double),
    typeof(GridSplitter),
    new FrameworkPropertyMetadata(1.0),
    new ValidateValueCallback(IsValidDragIncrement));
```

- **类型**：`double`
- **默认值**：`1.0`像素
- **验证回调**：`IsValidDragIncrement`，确保值大于 0
- **核心作用**：控制鼠标拖动时的最小调整增量

### 5. `KeyboardIncrementProperty` 注册

csharp:

```c#
KeyboardIncrementProperty = DependencyProperty.Register(
    nameof(KeyboardIncrement),
    typeof(double),
    typeof(GridSplitter),
    new FrameworkPropertyMetadata(10.0),
    new ValidateValueCallback(IsValidDragIncrement));
```

- **类型**：`double`
- **默认值**：`10.0`像素
- **验证回调**：`IsValidDragIncrement`，确保值大于 0
- **核心作用**：控制键盘方向键调整时的增量

------

## 五、核心依赖属性逐行解析

### 1. `ResizeDirection` 属性（分割方向）

csharp:

```c#
[Bindable(true)]
[Category("Behavior")]
public GridResizeDirection ResizeDirection { get; set; }
```

#### 逐句解析：

- **`[Category("Behavior")]`**：在属性窗口中归类到 "行为" 组

- **类型**：`GridResizeDirection`枚举，有三个可选值：

  - `GridResizeDirection.Auto`（默认）：根据 GridSplitter 所在的列和行自动判断
  - `GridResizeDirection.Columns`：水平分割，调整列的宽度
  - `GridResizeDirection.Rows`：垂直分割，调整行的高度

  

#### 自动判断规则：

- 如果 GridSplitter 的`HorizontalAlignment`设置为`Stretch`，并且`VerticalAlignment`不是`Stretch`，则分割行
- 如果 GridSplitter 的`VerticalAlignment`设置为`Stretch`，并且`HorizontalAlignment`不是`Stretch`，则分割列
- 如果两者都是`Stretch`，则优先分割列

#### 工业场景最佳实践：

**永远不要依赖 Auto 模式**，总是显式指定`ResizeDirection`。Auto 模式在复杂布局中经常会判断错误，导致 GridSplitter 无法正常工作。

#### 示例：

xaml:

```xaml
<!-- 水平分割列 -->
<GridSplitter Grid.Column="1"
              ResizeDirection="Columns"
              Width="5"
              HorizontalAlignment="Stretch"
              VerticalAlignment="Stretch"/>

<!-- 垂直分割行 -->
<GridSplitter Grid.Row="1"
              ResizeDirection="Rows"
              Height="5"
              HorizontalAlignment="Stretch"
              VerticalAlignment="Stretch"/>
```

### 2. `ResizeBehavior` 属性（分割行为，最重要）

csharp:

```c#
[Bindable(true)]
[Category("Behavior")]
public GridResizeBehavior ResizeBehavior { get; set; }
```

#### 逐句解析：

- **类型**：`GridResizeBehavior`枚举，有四个可选值：

  - `GridResizeBehavior.BasedOnAlignment`（默认）：根据 GridSplitter 的对齐方式决定
  - `GridResizeBehavior.PreviousAndNext`：同时调整前一个和后一个列 / 行的大小
  - `GridResizeBehavior.Previous`：只调整前一个列 / 行的大小
  - `GridResizeBehavior.Next`：只调整后一个列 / 行的大小

  

#### 详细说明：

| ResizeBehavior 值  | 水平分割（Columns）                                          | 垂直分割（Rows）                                             |
| :----------------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| `PreviousAndNext`  | 同时调整左边和右边的列                                       | 同时调整上边和下边的行                                       |
| `Previous`         | 只调整左边的列                                               | 只调整上边的行                                               |
| `Next`             | 只调整右边的列                                               | 只调整下边的行                                               |
| `BasedOnAlignment` | 根据 HorizontalAlignment 判断：- Left：只调整左边- Right：只调整右边- Center/Stretch：同时调整两边 | 根据 VerticalAlignment 判断：- Top：只调整上边- Bottom：只调整下边- Center/Stretch：同时调整两边 |

#### 工业场景最佳实践：

**永远显式指定`ResizeBehavior="PreviousAndNext"`**。这是最符合用户直觉的行为，拖动分割线时两边的大小都会调整。默认的`BasedOnAlignment`模式经常会导致意外的行为。

#### 示例：

xaml:

```xaml
<!-- 同时调整左右两边的列 -->
<GridSplitter Grid.Column="1"
              ResizeDirection="Columns"
              ResizeBehavior="PreviousAndNext"
              Width="5"
              HorizontalAlignment="Stretch"
              VerticalAlignment="Stretch"/>
```

### 3. `ShowsPreview` 属性（预览模式）

csharp:

```c#
[Bindable(true)]
[Category("Behavior")]
public bool ShowsPreview { get; set; }
```

#### 逐句解析：

- **类型**：`bool`

- **默认值**：`false`

- **核心作用**：

  - `false`：实时调整大小，拖动过程中列 / 行的大小会实时变化
  - `true`：拖动时只显示一条预览线，释放鼠标后才调整大小

  

#### 工业场景关键应用：

**当面板内容复杂时，必须设置`ShowsPreview="True"`**。工业上位机界面经常包含大量的图表、表格和实时数据，实时调整大小会导致严重的卡顿。使用预览模式可以大幅提升界面的流畅性。

#### 示例：

xaml:

```xaml
<!-- 预览模式，拖动时只显示预览线 -->
<GridSplitter Grid.Column="1"
              ResizeDirection="Columns"
              ResizeBehavior="PreviousAndNext"
              ShowsPreview="True"
              Width="5"
              HorizontalAlignment="Stretch"
              VerticalAlignment="Stretch"/>
```

### 4. `DragIncrement` 和 `KeyboardIncrement` 属性

csharp:

```c#
[Bindable(true)]
[Category("Behavior")]
public double DragIncrement { get; set; }

[Bindable(true)]
[Category("Behavior")]
public double KeyboardIncrement { get; set; }
```

- **`DragIncrement`**：鼠标拖动时的最小调整增量，默认 1 像素
- **`KeyboardIncrement`**：键盘方向键调整时的增量，默认 10 像素
- **工业场景应用**：对于需要精确调整大小的场景，可以增大增量值，避免调整过于灵敏

------

## 六、受保护方法逐行解析

### 1. `OnInitialized()` 方法

csharp:

```c#
protected override void OnInitialized(EventArgs e);
```

- **触发时机**：当控件初始化完成时调用
- **核心逻辑**：检查 GridSplitter 是否放在 Grid 中，如果不是则抛出警告
- **重要提示**：GridSplitter 只能在 Grid 中使用，放在其他布局容器中不会生效

### 2. `OnRenderSizeChanged()` 方法

csharp:

```c#
protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo);
```

- **触发时机**：当控件的大小发生变化时调用
- **核心逻辑**：更新 GridSplitter 的内部状态，确保分割方向和行为正确

### 3. 拖动事件处理方法

csharp:

```c#
protected override void OnDragStarted(DragStartedEventArgs e);
protected override void OnDragDelta(DragDeltaEventArgs e);
protected override void OnDragCompleted(DragCompletedEventArgs e);
```

- **`OnDragStarted`**：拖动开始时调用，记录初始位置和列 / 行的大小
- **`OnDragDelta`**：拖动过程中调用，根据鼠标移动量调整列 / 行的大小或更新预览线位置
- **`OnDragCompleted`**：拖动完成时调用，如果是预览模式则应用最终的大小调整

### 4. `OnKeyDown()` 方法

csharp:

```c#
protected override void OnKeyDown(KeyEventArgs e);
```

- **触发时机**：当按下键盘按键时调用
- **核心逻辑**：支持使用方向键调整 GridSplitter 的位置，每次调整的增量由`KeyboardIncrement`属性控制

------

## 七、GridSplitter 核心工作原理

### 7.1 初始化流程

1. 当 GridSplitter 被添加到 Grid 中时，`OnInitialized`方法会检查父容器是否为 Grid
2. 根据`ResizeDirection`和`ResizeBehavior`属性确定要调整的列或行
3. 记录相邻列 / 行的初始大小

### 7.2 拖动流程（实时模式）

1. 用户按下鼠标左键，触发`OnDragStarted`方法
2. 记录鼠标初始位置和相邻列 / 行的初始大小
3. 用户拖动鼠标，触发`OnDragDelta`方法
4. 根据鼠标移动量计算新的列 / 行大小
5. 应用新的大小到 Grid 的列 / 行定义
6. 用户释放鼠标左键，触发`OnDragCompleted`方法

### 7.3 拖动流程（预览模式）

1. 用户按下鼠标左键，触发`OnDragStarted`方法
2. 记录鼠标初始位置和相邻列 / 行的初始大小
3. 创建一条预览线并显示在 Grid 上
4. 用户拖动鼠标，触发`OnDragDelta`方法
5. 根据鼠标移动量更新预览线的位置
6. 用户释放鼠标左键，触发`OnDragCompleted`方法
7. 应用最终的大小调整到 Grid 的列 / 行定义
8. 隐藏预览线

------

## 八、工业上位机典型应用实例

### 实例 1：标准左右分割主界面

这是工业上位机最常用的布局，左侧是导航菜单，右侧是内容区域。

xaml:

```xaml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200" MinWidth="150"/>
        <ColumnDefinition Width="5"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>

    <!-- 左侧导航菜单 -->
    <Border Grid.Column="0" Background="#F5F5F5" BorderBrush="#E0E0E0" BorderThickness="0,0,1,0">
        <TreeView ItemsSource="{Binding MenuItems}"/>
    </Border>

    <!-- 分割线 -->
    <GridSplitter Grid.Column="1"
                  ResizeDirection="Columns"
                  ResizeBehavior="PreviousAndNext"
                  ShowsPreview="True"
                  Width="5"
                  HorizontalAlignment="Stretch"
                  VerticalAlignment="Stretch"
                  Background="#E0E0E0"/>

    <!-- 右侧内容区域 -->
    <Border Grid.Column="2" Background="White">
        <ContentControl Content="{Binding CurrentViewModel}"/>
    </Border>
</Grid>
```

### 实例 2：上下分割参数面板

xaml:

```xaml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="*" MinHeight="200"/>
        <RowDefinition Height="5"/>
        <RowDefinition Height="200" MinHeight="150"/>
    </Grid.RowDefinitions>

    <!-- 上方数据显示区域 -->
    <Border Grid.Row="0" Background="White">
        <DataGrid ItemsSource="{Binding ProductionData}"/>
    </Border>

    <!-- 分割线 -->
    <GridSplitter Grid.Row="1"
                  ResizeDirection="Rows"
                  ResizeBehavior="PreviousAndNext"
                  ShowsPreview="True"
                  Height="5"
                  HorizontalAlignment="Stretch"
                  VerticalAlignment="Stretch"
                  Background="#E0E0E0"/>

    <!-- 下方参数设置区域 -->
    <Border Grid.Row="2" Background="#F5F5F5" BorderBrush="#E0E0E0" BorderThickness="0,1,0,0">
        <GroupBox Header="参数设置" Margin="10">
            <Grid>
                <!-- 参数输入控件 -->
            </Grid>
        </GroupBox>
    </Border>
</Grid>
```

### 实例 3：嵌套 GridSplitter（复杂布局）

xaml:

```xaml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200" MinWidth="150"/>
        <ColumnDefinition Width="5"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>

    <!-- 左侧导航 -->
    <Border Grid.Column="0" Background="#F5F5F5">
        <TreeView ItemsSource="{Binding MenuItems}"/>
    </Border>

    <!-- 垂直分割线 -->
    <GridSplitter Grid.Column="1"
                  ResizeDirection="Columns"
                  ResizeBehavior="PreviousAndNext"
                  ShowsPreview="True"
                  Width="5"
                  Background="#E0E0E0"/>

    <!-- 右侧内容区域，包含上下分割 -->
    <Grid Grid.Column="2">
        <Grid.RowDefinitions>
            <RowDefinition Height="*" MinHeight="200"/>
            <RowDefinition Height="5"/>
            <RowDefinition Height="200" MinHeight="150"/>
        </Grid.RowDefinitions>

        <!-- 上方图表 -->
        <Border Grid.Row="0" Background="White">
            <Chart:LineChart ItemsSource="{Binding TrendData}"/>
        </Border>

        <!-- 水平分割线 -->
        <GridSplitter Grid.Row="1"
                      ResizeDirection="Rows"
                      ResizeBehavior="PreviousAndNext"
                      ShowsPreview="True"
                      Height="5"
                      Background="#E0E0E0"/>

        <!-- 下方数据表格 -->
        <Border Grid.Row="2" Background="White">
            <DataGrid ItemsSource="{Binding ProductionData}"/>
        </Border>
    </Grid>
</Grid>
```

------

## 九、最佳实践与常见问题（工业场景必看）

### 9.1 最佳实践

1. **永远显式指定`ResizeDirection`和`ResizeBehavior`**：不要依赖默认的 Auto 模式，避免意外行为
2. **GridSplitter 必须放在单独的列或行中**：并且该列或行的宽度 / 高度应设置为固定值（如 5）
3. **复杂界面必须使用预览模式**：设置`ShowsPreview="True"`，大幅提升拖动流畅性
4. **总是设置最小宽度 / 高度**：为相邻的列 / 行设置`MinWidth`或`MinHeight`，防止被拖动到看不见
5. **统一 GridSplitter 的样式**：使用全局样式确保所有分割线的外观一致
6. **避免在 ScrollViewer 中使用 GridSplitter**：会导致拖动行为异常

### 9.2 常见问题与解决方案

#### 问题 1：GridSplitter 无法拖动

**可能原因**：

1. GridSplitter 没有放在单独的列或行中
2. 没有显式设置`ResizeDirection`
3. GridSplitter 的宽度 / 高度设置为 0 或 Auto
4. 父容器不是 Grid

**解决方案**：

1. 确保 GridSplitter 放在单独的列或行中
2. 显式设置`ResizeDirection="Columns"`或`ResizeDirection="Rows"`
3. 设置 GridSplitter 的宽度为 5（水平分割）或高度为 5（垂直分割）
4. 确保父容器是 Grid

#### 问题 2：拖动时只有一边的大小变化

**原因**：`ResizeBehavior`设置不正确，默认的`BasedOnAlignment`模式可能只调整一边

**解决方案**：显式设置`ResizeBehavior="PreviousAndNext"`

#### 问题 3：拖动时界面卡顿

**原因**：面板内容复杂，实时调整大小导致重绘频繁

**解决方案**：设置`ShowsPreview="True"`，使用预览模式

#### 问题 4：GridSplitter 被拖动到看不见

**原因**：相邻的列 / 行没有设置最小宽度 / 高度

**解决方案**：为相邻的列 / 行设置`MinWidth`或`MinHeight`

#### 问题 5：GridSplitter 的外观不好看

**解决方案**：自定义 GridSplitter 的样式

xaml:

```xaml
<Style TargetType="GridSplitter">
    <Setter Property="Background" Value="#E0E0E0"/>
    <Setter Property="Cursor" Value="SizeWE"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="GridSplitter">
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="#BDBDBD"
                        BorderThickness="1,0,1,0">
                    <Rectangle Width="2"
                               Height="20"
                               Fill="#BDBDBD"
                               HorizontalAlignment="Center"
                               VerticalAlignment="Center"/>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

------

## 十、官方设计意图总结

微软设计 GridSplitter 的核心目标是：

1. **提供简单易用的布局调整功能**：让用户可以通过拖动直观地调整界面布局
2. **支持多种分割模式**：满足不同的布局需求
3. **提供预览模式**：优化复杂界面的拖动性能
4. **支持键盘操作**：提供完整的无障碍支持
5. **与 Grid 布局深度集成**：自动检测和调整相邻的列 / 行

------

## 总结

`GridSplitter`是 WPF 中实现可调整布局的核心控件，它的核心特性包括：

- `ResizeDirection`：控制分割方向（水平 / 垂直）
- `ResizeBehavior`：控制调整哪些列 / 行的大小
- `ShowsPreview`：预览模式，提升复杂界面的流畅性
- `DragIncrement`/`KeyboardIncrement`：控制调整增量

在工业上位机开发中，掌握 GridSplitter 的正确使用方法，可以开发出更加灵活、易用的用户界面。记住三个黄金法则：

1. 永远显式指定`ResizeDirection`和`ResizeBehavior`
2. 复杂界面必须使用预览模式
3. 总是为相邻的列 / 行设置最小宽度 / 高度