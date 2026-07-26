# 006002001_WPF `Ellipse` 椭圆类完整官方解析 + 工业场景实战

`Ellipse` 是 WPF 矢量图形体系中最常用的 Shape 派生类之一，用于绘制椭圆与正圆形，完全继承 `Shape` 基类的所有能力，是工业上位机中实现状态指示灯、圆形检测 ROI、焊点标记、缺陷标注的核心控件。

------

## 一、官方类定义与继承体系

### 1. 基础信息

- **命名空间**：`System.Windows.Shapes`
- **程序集**：`PresentationFramework.dll`（WPF 核心框架程序集）
- **类修饰符**：`sealed`（密封类，不可被继承）

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
                    └─ System.Windows.Shapes.Ellipse
```

### 3. 官方类声明

csharp:

```c#
public sealed class Ellipse : Shape
{
    public Ellipse();

    protected override Geometry DefiningGeometry { get; }
}
```

#### 关键解读

1. **密封类**：不允许其他类继承 `Ellipse`，如果需要自定义圆形变体，应直接继承 `Shape` 基类重写 `DefiningGeometry`；
2. **无新增公开属性**：`Ellipse` 没有定义任何属于自己的依赖属性，椭圆的形状完全由 `FrameworkElement` 的 `Width` / `Height` 属性决定，所有外观控制（填充、描边、虚线、拉伸等）全部继承自 `Shape` 基类；
3. **唯一核心实现**：仅重写了 `Shape` 的抽象属性 `DefiningGeometry`，返回一个 `EllipseGeometry` 椭圆几何对象，这是椭圆形状的底层定义。

------

## 二、核心功能与特性

### 1. 形状定义规则

`Ellipse` 的形状由布局系统分配的最终渲染尺寸决定：

- 当 `Width == Height` 时，渲染为**正圆形**；
- 当 `Width != Height` 时，渲染为**椭圆**；
- 图形的外接矩形与布局尺寸完全重合，椭圆边缘与外接矩形四边相切。

> 对比说明：与 `Rectangle` 不同，`Ellipse` 没有 `RadiusX` / `RadiusY` 这类形状控制属性，它本身就是 “完整的椭圆”，所有形状变化都通过宽高和变换实现。

### 2. 完整继承 Shape 基类能力

所有 `Shape` 基类的属性 `Ellipse` 全部原生支持，工业场景最常用的包括：

| 属性分类 | 核心属性                               | 工业用途                                |
| :------- | :------------------------------------- | :-------------------------------------- |
| 填充     | `Fill`（Brush 类型）                   | 填充圆形内部，区分 OK/NG 状态、高亮区域 |
| 描边     | `Stroke` / `StrokeThickness`           | 绘制圆形轮廓、检测框、标记圈            |
| 虚线     | `StrokeDashArray` / `StrokeDashOffset` | 虚线选中框、流动扫描效果                |
| 线帽拐角 | `StrokeDashCap` / `StrokeLineJoin`     | 控制虚线端点、轮廓拐角样式              |
| 拉伸     | `Stretch`                              | 控制椭圆如何适配布局容器空间            |
| 变换     | `RenderTransform` / `LayoutTransform`  | 缩放、旋转、平移，实现动态效果          |
| 命中测试 | 基于几何精确命中                       | 点击圆形区域响应事件，边角空白不响应    |

### 3. 底层渲染原理

`Ellipse` 重写了 `DefiningGeometry` 属性，内部逻辑：

1. 获取当前控件的最终渲染尺寸（`RenderSize`）；
2. 以 `(RenderSize.Width/2, RenderSize.Height/2)` 为圆心，以宽高的一半为半径，构造 `EllipseGeometry` 对象；
3. 基类 `Shape` 的 `OnRender` 方法拿到该几何对象后，执行「填充内部 + 绘制轮廓」的标准渲染流程，由 WPF 可视化层硬件加速输出。

### 4. 精确命中测试

`Ellipse` 重写了命中测试逻辑，**严格按照椭圆几何边界判断**：

- 点击椭圆外接矩形的四个角空白区域，不会触发点击事件；
- 只有点击到椭圆实体区域（填充或描边范围）才会响应；
- 工业场景中做缺陷点点击查看详情时，体验远优于矩形按钮。

------

## 三、基础使用方法

### 1. XAML 基础用法（最常用）

直接在布局容器中声明，通过 `Width` / `Height` 控制大小，`Fill` / `Stroke` 控制外观。

xaml:

```xaml
<!-- 正圆：宽高相等 -->
<Ellipse Width="50" Height="50" 
         Fill="LimeGreen" 
         Stroke="DarkGreen" 
         StrokeThickness="2"/>

<!-- 椭圆：宽高不等 -->
<Ellipse Width="80" Height="40" 
         Fill="Transparent" 
         Stroke="Blue" 
         StrokeThickness="1.5"/>
```

### 2. C# 后台代码动态创建

适合算法动态生成缺陷标记、检测区域等场景。

csharp:

```c#
using System.Windows.Media;
using System.Windows.Shapes;

// 创建椭圆实例
Ellipse ellipse = new Ellipse();
ellipse.Width = 30;
ellipse.Height = 30;
ellipse.Fill = new SolidColorBrush(Colors.Red);
ellipse.Stroke = new SolidColorBrush(Colors.DarkRed);
ellipse.StrokeThickness = 2;

// 添加到 Canvas 容器
canvas.Children.Add(ellipse);
Canvas.SetLeft(ellipse, 100);
Canvas.SetTop(ellipse, 100);
```

### 3. 不同布局容器中的特性

- **Canvas 绝对定位**：`Canvas.Left` / `Canvas.Top` 定位的是椭圆外接矩形的左上角，而非圆心；
- **Grid 自适应**：不设置宽高时，默认拉伸填满 Grid 单元格，由 `Stretch` 属性控制拉伸模式；
- **StackPanel 流式布局**：参与标准流式布局，和普通控件行为一致。

------

## 四、工业上位机实战场景案例

### 案例 1：圆形检测 ROI 区域框

**场景**：AOI 视觉检测的圆形感兴趣区域、镜头视野范围标记。

xaml:

```xaml
<Canvas SnapsToDevicePixels="True" Width="600" Height="400" Background="#222">
    <!-- 虚线ROI外框：半透明蓝色填充，蓝色虚线描边 -->
    <Ellipse Canvas.Left="150" Canvas.Top="100"
             Width="300" Height="200"
             Fill="#220088FF"
             Stroke="#FF0088FF"
             StrokeThickness="2"
             StrokeDashArray="6,3"/>
</Canvas>
```

**要点**：

- `SnapsToDevicePixels="True"` 对齐物理像素，避免线条模糊；
- 半透明填充既高亮区域，又不遮挡下方的图像内容；
- `StrokeDashArray` 实现虚线，区分不同类型的检测区域。

------

### 案例 2：焊点 / 缺陷中心对齐标记

**场景**：算法输出缺陷中心坐标，在图像上精准标记，要求圆心与坐标点完全对齐。

#### 错误写法（左上角对齐，圆心偏移）

xaml:

```xaml
<!-- 圆心实际在 (100+15, 100+15) = (115,115)，不是(100,100) -->
<Ellipse Canvas.Left="100" Canvas.Top="100" Width="30" Height="30" Stroke="Red"/>
```

#### 正确写法 1：负 Margin 偏移（推荐）

xaml:

```xaml
<Ellipse Canvas.Left="100" Canvas.Top="100"
         Width="30" Height="30"
         Margin="-15,-15,0,0"  <!-- 左、上各偏移一半宽高，圆心对准(100,100) -->
         Stroke="Red"
         StrokeThickness="2"
         Fill="Transparent"/>
```

#### 正确写法 2：RenderTransform 平移

xaml:

```xaml
<Ellipse Canvas.Left="100" Canvas.Top="100" Width="30" Height="30" Stroke="Red">
    <Ellipse.RenderTransform>
        <TranslateTransform X="-15" Y="-15"/>
    </Ellipse.RenderTransform>
</Ellipse>
```

> 工业标准习惯：所有点标记均以中心对齐坐标，和视觉算法的坐标系完全对应，避免坐标换算错误。

------

### 案例 3：设备状态指示灯（渐变填充 + 多状态）

**场景**：工位运行、待机、故障三种状态的圆形指示灯，带发光质感，直观展示设备状态。

xaml:

```xaml
<!-- 运行状态：绿色渐变指示灯 -->
<Ellipse Width="24" Height="24"
         Stroke="#FF006600" StrokeThickness="1">
    <Ellipse.Fill>
        <RadialGradientBrush GradientOrigin="0.3,0.3" Center="0.4,0.4" RadiusX="0.8" RadiusY="0.8">
            <GradientStop Color="#FF88FF88" Offset="0"/>
            <GradientStop Color="#FF00CC00" Offset="0.7"/>
            <GradientStop Color="#FF008800" Offset="1"/>
        </RadialGradientBrush>
    </Ellipse.Fill>
</Ellipse>
```

#### MVVM 绑定状态示例

视图模型中定义状态枚举，配合值转换器自动切换颜色：

csharp:

```c#
public enum DeviceState { Run, Idle, Error }

// 视图模型属性
public DeviceState CurrentState { get; set; }
```

xaml:

```xaml
<!-- 绑定状态，通过转换器自动切换填充色 -->
<Ellipse Width="24" Height="24"
         Fill="{Binding CurrentState, Converter={StaticResource StateToBrushConverter}}"
         Stroke="Black" StrokeThickness="1"/>
```

------

### 案例 4：批量缺陷点渲染（MVVM + ItemsControl）

**场景**：算法输出一批缺陷坐标，批量渲染到图像上，纯数据驱动，无需后台操作 UI。

#### 数据模型

csharp:

```c#
public class DefectPoint
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Diameter { get; set; } // 缺陷直径
    public bool IsCritical { get; set; } // 是否严重缺陷
}
```

#### XAML 界面

xaml:

```xaml
<Canvas Width="800" Height="600">
    <ItemsControl ItemsSource="{Binding DefectList}">
        <ItemsControl.ItemsPanel>
            <ItemsPanelTemplate>
                <Canvas/>
            </ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>
        
        <!-- 定位：左上角对齐，通过负Margin实现圆心对齐 -->
        <ItemsControl.ItemContainerStyle>
            <Style TargetType="ContentPresenter">
                <Setter Property="Canvas.Left" Value="{Binding X}"/>
                <Setter Property="Canvas.Top" Value="{Binding Y}"/>
            </Style>
        </ItemsControl.ItemContainerStyle>

        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <Ellipse Width="{Binding Diameter}"
                         Height="{Binding Diameter}"
                         Margin="{Binding Diameter, Converter={StaticResource HalfNegativeMarginConverter}}"
                         Stroke="{Binding IsCritical, Converter={StaticResource BoolToRedYellowConverter}}"
                         StrokeThickness="1.5"
                         Fill="Transparent"/>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</Canvas>
```

**工业价值**：算法线程计算缺陷后，只需更新集合属性，UI 自动渲染，完全符合 MVVM 规范，无跨线程操作 UI 的问题。

------

### 案例 5：选中态圆环 + 动画效果

**场景**：鼠标选中缺陷点后，显示放大的虚线选中环，带呼吸动画效果。

xaml:

```xaml
<Ellipse Width="40" Height="40"
         Fill="Transparent"
         Stroke="Yellow"
         StrokeThickness="2"
         StrokeDashArray="4,2"
         Margin="-20,-20,0,0">
    <Ellipse.Triggers>
        <EventTrigger RoutedEvent="Loaded">
            <BeginStoryboard>
                <Storyboard RepeatBehavior="Forever" AutoReverse="True">
                    <!-- 呼吸缩放动画 -->
                    <DoubleAnimation Storyboard.TargetProperty="Opacity"
                                     From="1" To="0.4" Duration="0:0:1"/>
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
    </Ellipse.Triggers>
</Ellipse>
```

------

## 五、常见问题与避坑指南

### 1. Ellipse 完全不显示的常见原因

- ❌ 未设置 `Width` 和 `Height`，且父容器没有分配固定尺寸，椭圆大小为 0 不可见；
- ❌ 只设置了 `StrokeThickness`，未设置 `Stroke` 画刷（默认 `null`，轮廓完全透明）；
- ❌ `Fill` 和 `Stroke` 都没设置，图形完全透明；
- ❌ 父容器裁剪、坐标超出可视范围。

### 2. 圆心对齐的标准做法

工业视觉场景中，**永远用中心对齐坐标点**，不要用左上角对齐。推荐使用负 `Margin` 方案，简单直接，不影响布局测量。

### 3. 拉伸变形问题

- 在 Grid 等自适应容器中，如果不设置固定宽高，`Ellipse` 会被拉伸填满容器，宽高比失真；
- 如需保持正圆，可设置 `Stretch="Uniform"`，并配合 `HorizontalAlignment="Center"` `VerticalAlignment="Center"` 居中显示。

### 4. 性能注意事项

- 单个 Ellipse 开销极低，但如果同时存在上百个动态刷新的 Ellipse（如每秒更新位置的缺陷点），每个都是完整 `FrameworkElement`，有布局开销；
- 超大量点场景建议改用 `DrawingVisual` 或 `WriteableBitmap` 低层绘制；
- 静态图形建议将 `Fill` / `Stroke` 使用的画刷冻结（`Freeze()`），提升渲染性能。

### 5. 不要试图继承 Ellipse

`Ellipse` 是密封类，无法继承；如需自定义圆形变体（如带十字的标记圆），请直接继承 `Shape` 基类，重写 `DefiningGeometry` 组合多个几何对象。

------

## 总结

`Ellipse` 是 WPF 中最简单、最常用的矢量图形之一，它本身没有复杂的自定义属性，全部能力都来自 `Shape` 基类和 WPF 框架体系。对于工业上位机场景，它是实现圆形标记、状态指示、检测区域的首选控件，配合数据绑定和动画，可以低成本实现高质量的可视化效果。