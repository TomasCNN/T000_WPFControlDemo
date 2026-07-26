# 006004001_WPF `Rectangle` 矩形类官方深度解析 + 工业场景实战

`Rectangle` 是 WPF 矢量图形体系中 `Shape` 抽象基类的**密封派生类**，用于绘制直角 / 圆角矩形，是工业上位机中使用频率最高的基础图形：检测 ROI 区域、工位状态卡片、缺陷外框、进度指示、按钮背景等场景均会用到。它仅新增两个圆角控制属性，其余填充、描边、布局、交互能力全部继承自 `Shape` 基类，开发成本低、渲染性能优异。

------

## 一、官方类定义与继承体系

### 1. 基础元信息

- **命名空间**：`System.Windows.Shapes`
- **程序集**：`PresentationFramework.dll`（WPF 核心框架程序集）
- **类修饰符**：`public sealed`（公共密封类，禁止被其他类继承）
- **设计定位**：通用矩形矢量图形，兼顾直角与圆角场景，是 Shape 体系中最常用的具体实现。

### 2. 完整继承链

plaintext:

```tex
System.Object
  └─ System.Windows.Threading.DispatcherObject
     └─ System.Windows.DependencyObject
        └─ System.Windows.Media.Visual
           └─ System.Windows.UIElement
              └─ System.Windows.FrameworkElement
                 └─ System.Windows.Shapes.Shape
                    └─ System.Windows.Shapes.Rectangle
```

各层能力继承：

- `FrameworkElement`：天然融入 WPF 布局系统，支持数据绑定、样式、动画、路由事件；
- `Shape`：提供统一的填充、描边、拉伸、几何渲染等图形通用能力；
- `Rectangle`：仅实现矩形的几何定义与圆角控制，是继承链最末端的具体图形实现。

### 3. 官方类完整核心声明

csharp:

```c#
public sealed class Rectangle : Shape
{
    // 自有依赖属性标识符
    public static readonly DependencyProperty RadiusXProperty;
    public static readonly DependencyProperty RadiusYProperty;

    // 公共构造函数
    public Rectangle();

    // 圆角控制属性
    public double RadiusX { get; set; }
    public double RadiusY { get; set; }

    // 重写 Shape 抽象属性：定义矩形的原始几何
    protected override Geometry DefiningGeometry { get; }
}
```

#### 关键结构解读

1. **极简实现**：仅新增 2 个圆角依赖属性 + 1 个重写几何属性，无自定义方法，所有外观、布局、交互逻辑全部复用基类；
2. **密封类约束**：不允许派生继承，如需自定义矩形变体（带十字标记的矩形），需直接继承 `Shape` 基类；
3. **闭合图形特性**：属于封闭图形，`Fill` 填充和 `Stroke` 描边属性均有效，这是和 `Line`/`Polyline` 等开放图形的核心区别。

------

## 二、核心属性深度解析

### 1. Rectangle 独有属性：圆角控制

这是矩形区别于其他图形的核心属性，均为依赖属性，原生支持数据绑定、动画驱动。

表格







| 属性      | 类型     | 默认值 | 官方说明                                                     |
| :-------- | :------- | :----- | :----------------------------------------------------------- |
| `RadiusX` | `double` | `0.0`  | 矩形圆角的**水平方向半径**，值越大圆角越明显；最大值不超过矩形宽度的一半 |
| `RadiusY` | `double` | `0.0`  | 矩形圆角的**垂直方向半径**，值越大圆角越明显；最大值不超过矩形高度的一半 |

#### 关键特性

- 当 `RadiusX = 0` 且 `RadiusY = 0` 时，为标准直角矩形；
- 当 `RadiusX = Width/2` 且 `RadiusY = Height/2` 时，矩形退化为椭圆（宽高相等时为正圆）；
- 两个值可分别设置，实现水平、垂直方向不同弧度的椭圆角；
- 属性变更时自动触发重绘，无需手动刷新界面。

### 2. 继承自 Shape 的核心属性

矩形是闭合图形，填充与描边属性全部生效，工业场景高频使用：

| 属性分类 | 属性名               | 工业场景用途                                                 |
| :------- | :------------------- | :----------------------------------------------------------- |
| 内部填充 | `Fill`（Brush 类型） | 填充矩形内部区域，半透明填充用于高亮检测区域、工位状态底色   |
| 轮廓基础 | `Stroke`             | 矩形边框画刷 / 颜色，为 `null` 时边框完全不显示              |
| 轮廓基础 | `StrokeThickness`    | 边框厚度，单位为与设备无关像素，支持小数                     |
| 轮廓拐角 | `StrokeLineJoin`     | 边框拐角样式：`Miter`(尖角)、`Bevel`(斜切)、`Round`(圆角)    |
| 虚线边框 | `StrokeDashArray`    | 虚线边框序列，按「实线长度、空白长度」循环，用于检测框、选中态 |
| 虚线偏移 | `StrokeDashOffset`   | 虚线起始偏移，配合动画实现流动效果                           |
| 拉伸模式 | `Stretch`            | 控制矩形如何适配布局空间：None/Fill/Uniform/UniformToFill    |
| 渲染几何 | `RenderedGeometry`   | 只读，最终渲染几何（含边框厚度影响），用于精确命中测试       |

> 注意：矩形边框为**居中绘制**，例如 2px 厚度的边框，1px 在矩形内部、1px 在外部，高精度坐标对齐时需考虑边框厚度的偏移。

### 3. 继承自 FrameworkElement 的布局属性

矩形的形状大小本质由布局尺寸决定：

- `Width` / `Height`：显式设置矩形外接矩形宽高，是最常用的尺寸控制方式；
- `Canvas.Left` / `Canvas.Top`：Canvas 绝对定位时的左上角坐标，与视觉检测 ROI 的 `x,y,width,height` 定义天然匹配；
- `HorizontalAlignment` / `VerticalAlignment`：Grid 等自适应容器中的对齐方式。

> 与 Ellipse 的区别：Rectangle 默认左上角对齐，直接对应工业场景中 “左上角坐标 + 宽高” 的 ROI 定义习惯，无需额外偏移。

### 4. 核心重写属性：DefiningGeometry

csharp:

```c#
protected override Geometry DefiningGeometry { get; }
```

这是 `Rectangle` 唯一重写的核心成员，也是 `Shape` 基类强制要求的扩展点。

#### 官方内部实现逻辑

csharp:

```c#
protected override Geometry DefiningGeometry
{
    get
    {
        // 以控件渲染尺寸为宽高，结合圆角半径构造矩形几何
        return new RectangleGeometry(
            new Rect(0, 0, RenderSize.Width, RenderSize.Height),
            RadiusX,
            RadiusY);
    }
}
```

#### 详细解读

1. **职责单一**：仅返回矩形的原始几何定义，不关心描边、拉伸、变换等渲染细节；
2. **设计思想**：形状定义与渲染执行分离，所有 Shape 派生类只负责定义形状，布局、描边、命中测试统一由基类处理；
3. **与 RenderedGeometry 的区别**：`DefiningGeometry` 是原始设计几何，不含边框厚度、拉伸变换；`RenderedGeometry` 是最终渲染几何，用于精确命中测试和边界计算。

------

## 三、核心功能与底层原理

### 1. 形状定义规则

- 直角矩形：`RadiusX = RadiusY = 0`，四边笔直、拐角尖锐；
- 圆角矩形：`RadiusX`、`RadiusY` 大于 0，拐角平滑过渡；
- 矩形的外接矩形与布局尺寸完全重合，边缘与布局边界对齐。

### 2. 完整渲染流程

`Rectangle` 没有重写布局渲染方法，完全复用 `Shape` 基类的标准流水线：

1. **测量阶段（Measure）**：基类读取 `DefiningGeometry` 边界，结合边框厚度、拉伸模式，计算控件期望尺寸；
2. **排列阶段（Arrange）**：布局系统分配最终空间，基类计算缩放与偏移变换，生成 `GeometryTransform`；
3. **渲染阶段（OnRender）**：
   - 将原始矩形几何应用变换，得到最终渲染几何；
   - 若 `Fill` 不为 null，先用填充画刷绘制内部区域；
   - 再根据 `Stroke`、厚度、圆角、虚线等参数构造 `Pen`，绘制边框；
   - 全部通过 DirectX 硬件加速输出到屏幕。

### 3. 精确命中测试

`Rectangle` 基于实际几何形状做命中测试：

- 直角矩形：点击矩形外接矩形内的区域均响应；
- 圆角矩形：圆角外的四角空白区域不会触发点击，命中精度远高于矩形按钮；
- 工业场景提示：如果需要点击矩形内部空白区域也响应事件，必须将 `Fill` 设为 `Transparent`，不能留空（`null`），否则空白区域不参与命中测试。

### 4. 布局拉伸特性

通过 `Stretch` 属性控制矩形适配容器的方式：

- `None`（默认）：按 `Width`/`Height` 原始尺寸渲染，不拉伸；
- `Fill`：拉伸填满整个容器，宽高比可能失真；
- `Uniform`：等比例缩放至完全容纳在容器内，不变形；
- `UniformToFill`：等比例缩放至完全覆盖容器，超出部分裁剪。

------

## 四、基础使用方法

### 1. XAML 基础用法

最常用的声明方式，直接控制尺寸、圆角与外观。

xaml:

```xaml
<StackPanel Orientation="Horizontal" Margin="20">
    <!-- 直角矩形：红色边框，无填充 -->
    <Rectangle Width="120" Height="80"
               Stroke="Red" StrokeThickness="2"/>

    <!-- 圆角矩形：蓝色半透明填充，圆角半径8 -->
    <Rectangle Width="120" Height="80"
               RadiusX="8" RadiusY="8"
               Fill="#330066FF"
               Stroke="#FF0066FF"
               StrokeThickness="1.5"
               Margin="20,0,0,0"/>
</StackPanel>
```

### 2. C# 后台动态创建

适合算法动态生成检测区域、缺陷框的场景。

csharp:

```c#
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

// 创建检测框
Rectangle roiRect = new Rectangle();
roiRect.Width = 200;
roiRect.Height = 150;
roiRect.RadiusX = 4;
roiRect.RadiusY = 4;

// 样式设置
roiRect.Stroke = new SolidColorBrush(Colors.LimeGreen);
roiRect.StrokeThickness = 2;
roiRect.Fill = new SolidColorBrush(Color.FromArgb(0x33, 0x00, 0xFF, 0x00));

// 添加到Canvas，定位到(100,80)
canvas.Children.Add(roiRect);
Canvas.SetLeft(roiRect, 100);
Canvas.SetTop(roiRect, 80);
```

### 3. 不同布局容器中的特性

- **Canvas 绝对定位**：最常用，`Canvas.Left/Top` 对应矩形左上角坐标，与视觉检测 ROI 定义完全一致；
- **Grid 自适应**：不设置宽高时自动拉伸填满单元格，适合做背景、边框；
- **StackPanel 流式布局**：参与标准流式布局，适合做列表项背景、分隔块。

------

## 五、工业上位机实战实例

### 案例 1：AOI 检测 ROI 区域框

**场景**：视觉检测的感兴趣区域标记，半透明填充既高亮又不遮挡下方图像，虚线边框区分不同类型 ROI。

xaml:

```xaml
<Canvas Width="800" Height="600" Background="#222" SnapsToDevicePixels="True">
    <!-- 主检测ROI：绿色实线框 + 半透明填充 -->
    <Rectangle Canvas.Left="150" Canvas.Top="100"
               Width="400" Height="300"
               Stroke="LimeGreen" StrokeThickness="2"
               Fill="#2200FF00"/>

    <!-- 次检测区：蓝色虚线框 -->
    <Rectangle Canvas.Left="580" Canvas.Top="150"
               Width="150" Height="120"
               Stroke="#FF4488FF" StrokeThickness="1.5"
               StrokeDashArray="6,3"
               Fill="#114488FF"/>
</Canvas>
```

> 工业要点：`SnapsToDevicePixels="True"` 保证边框对齐物理像素，高精度场景下不发虚、不模糊。

### 案例 2：圆角工位状态卡片

**场景**：产线工位状态展示，圆角矩形做背景，配合颜色区分运行 / 待机 / 故障状态。

xaml:

```xaml
<Grid Width="180" Height="100">
    <!-- 卡片背景：圆角矩形，运行状态绿色渐变 -->
    <Rectangle RadiusX="8" RadiusY="8"
               Stroke="#FF008800" StrokeThickness="1">
        <Rectangle.Fill>
            <LinearGradientBrush StartPoint="0,0" EndPoint="0,1">
                <GradientStop Color="#FFEEFFEE" Offset="0"/>
                <GradientStop Color="#FFCCEECC" Offset="1"/>
            </LinearGradientBrush>
        </Rectangle.Fill>
    </Rectangle>
    
    <!-- 内部文字与状态灯 -->
    <StackPanel Margin="12">
        <TextBlock Text="上料工位" FontSize="14" FontWeight="Bold"/>
        <TextBlock Text="运行中" Foreground="Green" Margin="0,8,0,0"/>
    </StackPanel>
</Grid>
```

> MVVM 适配：将 `Fill`、`Stroke` 绑定到视图模型的工位状态枚举，配合值转换器自动切换颜色。

### 案例 3：缺陷标注选中框

**场景**：点击缺陷后显示选中外框，双边框 + 半透明填充，醒目且不遮挡图像。

xaml:

```xaml
<Canvas>
    <!-- 缺陷外框：双实线选中效果 -->
    <Rectangle Canvas.Left="220" Canvas.Top="180"
               Width="60" Height="45"
               Stroke="Yellow" StrokeThickness="2"
               Fill="#33FFFF00"/>
    <!-- 外层虚线装饰 -->
    <Rectangle Canvas.Left="215" Canvas.Top="175"
               Width="70" Height="55"
               Stroke="Yellow" StrokeThickness="1"
               StrokeDashArray="4,2"
               Fill="Transparent"/>
</Canvas>
```

### 案例 4：液位 / 进度条指示

**场景**：设备液位、物料余量、任务进度的可视化，通过高度绑定动态展示比例。

xaml:

```xaml
<Grid Width="40" Height="200" Background="#333">
    <!-- 进度填充：底部对齐，高度绑定进度值 -->
    <Rectangle VerticalAlignment="Bottom"
               Width="40"
               Height="{Binding FillPercentage, Converter={StaticResource PercentToHeightConverter}}"
               Fill="RoyalBlue"
               RadiusX="2" RadiusY="2"/>
    <!-- 外边框 -->
    <Rectangle Stroke="Gray" StrokeThickness="1" Fill="Transparent"/>
</Grid>
```

### 案例 5：MVVM 批量缺陷框渲染

**场景**：算法输出一批缺陷外接矩形，通过 `ItemsControl` 数据驱动渲染，纯 MVVM 模式，无需操作 UI 元素。

#### 数据模型

csharp:

```c#
public class DefectRect
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public bool IsCritical { get; set; }
}
```

#### XAML 界面

xaml:

```xaml
<Canvas Width="800" Height="600">
    <ItemsControl ItemsSource="{Binding DefectRectList}">
        <ItemsControl.ItemsPanel>
            <ItemsPanelTemplate>
                <Canvas/>
            </ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>

        <ItemsControl.ItemContainerStyle>
            <Style TargetType="ContentPresenter">
                <Setter Property="Canvas.Left" Value="{Binding X}"/>
                <Setter Property="Canvas.Top" Value="{Binding Y}"/>
            </Style>
        </ItemsControl.ItemContainerStyle>

        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <Rectangle Width="{Binding Width}"
                           Height="{Binding Height}"
                           Stroke="{Binding IsCritical, Converter={StaticResource CriticalToBrushConverter}}"
                           StrokeThickness="1.5"
                           Fill="Transparent"/>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</Canvas>
```

> 工业价值：算法线程只需更新集合属性，UI 自动渲染所有缺陷框，完全避免跨线程操作 UI 的问题。

### 案例 6：选中态呼吸高亮动画

**场景**：选中的检测区域呼吸闪烁，提醒操作人员当前操作对象。

xaml:

```xaml
<Rectangle Width="200" Height="150"
           Stroke="Cyan" StrokeThickness="2"
           Fill="#2200FFFF">
    <Rectangle.Triggers>
        <EventTrigger RoutedEvent="Loaded">
            <BeginStoryboard>
                <Storyboard RepeatBehavior="Forever" AutoReverse="True">
                    <DoubleAnimation Storyboard.TargetProperty="Opacity"
                                     From="1.0" To="0.4"
                                     Duration="0:0:1"/>
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
    </Rectangle.Triggers>
</Rectangle>
```

------

## 六、常见避坑与最佳实践

### 1. 圆角半径的边界限制

`RadiusX` 最大不能超过宽度的一半，`RadiusY` 最大不能超过高度的一半；超出后 WPF 会自动裁剪到最大值，当两者都等于半宽半高时，矩形变为椭圆。

### 2. 点击内部无响应：Fill 设为 Transparent

如果矩形只设置了 `Stroke` 边框，`Fill` 留空（`null`），那么点击矩形内部空白区域不会触发鼠标事件。需要内部也响应点击时，必须设置 `Fill="Transparent"`。

### 3. 1px 边框模糊必开像素对齐

高精度绘图场景必须设置 `SnapsToDevicePixels="True"`，避免亚像素渲染导致的 1px 边框发虚、粗细不均。

### 4. 大量矩形的性能注意事项

- 几十以内的静态矩形，Rectangle 开发效率高、易维护；
- 上百个高频刷新的矩形（如实时缺陷跟踪），每个 Rectangle 都是完整 `FrameworkElement`，有布局开销；超大量场景建议改用 `DrawingVisual` 或 `WriteableBitmap` 低层绘制；
- 静态画刷调用 `Freeze()` 冻结，可显著提升渲染性能。

### 5. 不要试图继承 Rectangle

`Rectangle` 是密封类，无法继承；需要自定义矩形变体时，直接继承 `Shape` 基类，重写 `DefiningGeometry` 组合几何即可。

### 6. 边框厚度的坐标偏移

边框是居中绘制的，高精度对齐场景下，若边框厚度为 2px，矩形实际外边界会比 `Width`/`Height` 向外扩展 1px，坐标计算时需考虑该偏移量。