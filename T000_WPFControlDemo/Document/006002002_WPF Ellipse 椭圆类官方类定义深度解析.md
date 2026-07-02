# 006002002_WPF `Ellipse` 椭圆类官方类定义深度解析

`Ellipse` 是 WPF 矢量图形体系中 `Shape` 抽象基类的**密封派生类**，专门用于绘制椭圆与正圆形。它本身没有新增复杂的自定义属性，仅通过重写基类的 `DefiningGeometry` 抽象属性提供椭圆几何定义，所有外观、布局、交互能力全部继承自 `Shape` 与 `FrameworkElement`，是 WPF 中最简洁、最常用的基础图形控件之一。

------

## 一、基础定义与继承体系

### 1. 官方基础信息

- **命名空间**：`System.Windows.Shapes`
- **程序集**：`PresentationFramework.dll`（WPF 核心框架程序集）
- **可访问性**：`public`
- **类修饰符**：`sealed`（密封类，禁止被其他类继承）

### 2. 完整继承链

plaintext:

```
System.Object
  └─ System.Windows.Threading.DispatcherObject
     └─ System.Windows.DependencyObject
        └─ System.Windows.Media.Visual
           └─ System.Windows.UIElement
              └─ System.Windows.FrameworkElement
                 └─ System.Windows.Shapes.Shape
                    └─ System.Windows.Shapes.Ellipse
```

- 继承 `DispatcherObject`：绑定 UI 线程，具备 WPF 线程亲和性；
- 继承 `DependencyObject`：支持依赖属性系统，原生支持数据绑定、样式、动画；
- 继承 `Visual`：接入 WPF 可视化树，支持硬件加速渲染、命中测试；
- 继承 `UIElement`：具备路由事件、输入响应、布局可见性等基础 UI 能力；
- 继承 `FrameworkElement`：完整融入 WPF 布局系统，支持宽高、边距、对齐、数据上下文等标准控件属性；
- 继承 `Shape`：获得统一的填充、描边、拉伸、几何渲染等图形通用能力；
- `Ellipse` 自身：仅实现椭圆的几何定义，是整个继承链中最末端的具体图形实现。

### 3. 官方类声明

csharp:

```c#
public sealed class Ellipse : Shape
{
    // 构造函数
    public Ellipse();

    // 唯一重写的核心属性：定义椭圆的几何形状
    protected override Geometry DefiningGeometry { get; }
}
```

#### 关键解读

1. **密封类设计**：`sealed` 修饰符表明不允许任何类继承 `Ellipse`。如果需要自定义圆形变体（如带十字的标记圆），只能直接继承 `Shape` 基类，不能继承 `Ellipse`。
2. **极轻量的实现**：整个类仅包含 1 个构造函数 + 1 个重写属性，没有新增任何公开字段、属性或方法，所有能力全部来自基类。
3. **单一职责**：`Ellipse` 的唯一职责就是 “提供椭圆的几何定义”，布局、渲染、描边、交互等全部逻辑都由上层基类统一实现，保证了所有 Shape 派生类行为的一致性。

------

## 二、构造函数

csharp:

```c#
public Ellipse();
```

- 无参数公共构造函数，用于实例化椭圆对象；
- 构造函数内部仅做轻量初始化，继承基类的默认属性值：
  - `Fill = null`（无填充，完全透明）
  - `Stroke = null`（无描边，完全透明）
  - `StrokeThickness = 1.0`
  - `Stretch = Stretch.None`
- 不会提前创建几何对象，几何计算延迟到布局渲染阶段执行，保证实例化性能。

------

## 三、核心属性逐析

### 1. 唯一重写属性：DefiningGeometry

csharp:

```c#
protected override Geometry DefiningGeometry { get; }
```

这是 `Ellipse` 最核心、唯一重写的成员，也是 `Shape` 抽象基类要求所有派生类必须实现的扩展点。

#### 官方实现逻辑

1. 获取控件当前的最终渲染尺寸 `RenderSize`（布局系统排列后的实际可用尺寸）；
2. 以 `(RenderSize.Width / 2, RenderSize.Height / 2)` 为圆心；
3. 分别以 `RenderSize.Width / 2`、`RenderSize.Height / 2` 为水平、垂直半径；
4. 构造一个 `EllipseGeometry` 椭圆几何对象并返回。

#### 访问级别与设计意图

- `protected` 访问级别：外部代码无法直接调用，仅基类和派生类内部使用；
- 设计思想：**形状定义与渲染执行分离**。派生类只需要关心 “形状是什么”，不需要关心 “怎么画、怎么布局”，后者全部由 `Shape` 基类统一处理，既减少重复代码，也保证了所有图形行为一致。

#### 补充说明

该属性返回的是**原始定义几何**，不包含描边厚度、拉伸变换、虚线样式的影响；如果需要获取最终渲染后的完整几何边界，应使用基类的公开属性 `RenderedGeometry`。

### 2. 继承自 Shape 基类的图形属性

`Ellipse` 原生继承 `Shape` 的全部属性，是控制椭圆外观的核心手段：

| 属性分类 | 属性名              | 类型               | 作用                                                        |
| :------- | :------------------ | :----------------- | :---------------------------------------------------------- |
| 填充     | `Fill`              | `Brush`            | 设置椭圆内部区域的填充画刷，支持纯色、渐变、纹理等          |
| 描边基础 | `Stroke`            | `Brush`            | 设置椭圆轮廓的画刷，为 `null` 时轮廓完全不显示              |
| 描边基础 | `StrokeThickness`   | `double`           | 轮廓线条的厚度，单位为与设备无关像素                        |
| 描边线帽 | `StrokeDashCap`     | `PenLineCap`       | 虚线段的端点样式                                            |
| 描边拐角 | `StrokeLineJoin`    | `PenLineJoin`      | 多段线条拐角样式（椭圆为连续曲线，该属性影响较小）          |
| 描边尖角 | `StrokeMiterLimit`  | `double`           | 斜接尖角的最大延伸比例                                      |
| 虚线样式 | `StrokeDashArray`   | `DoubleCollection` | 虚线的实线 / 空白长度序列                                   |
| 虚线偏移 | `StrokeDashOffset`  | `double`           | 虚线起始位置偏移，可动画实现流动效果                        |
| 拉伸模式 | `Stretch`           | `Stretch`          | 椭圆如何适配布局分配的空间：None/Fill/Uniform/UniformToFill |
| 渲染几何 | `RenderedGeometry`  | `Geometry`         | 只读，获取最终渲染生效的完整几何（含描边、变换影响）        |
| 几何变换 | `GeometryTransform` | `Transform`        | 只读，获取应用在原始几何上的变换矩阵                        |

### 3. 继承自 FrameworkElement 的布局属性

椭圆的形状大小本质上由布局尺寸决定，这些属性均来自 `FrameworkElement`：

- `Width` / `Height`：显式设置椭圆外接矩形的宽高，宽高相等时为正圆；
- `MinWidth` / `MaxWidth` / `MinHeight` / `MaxHeight`：尺寸范围约束；
- `Margin` / `Padding`：外边距、内边距；
- `HorizontalAlignment` / `VerticalAlignment`：在父容器中的对齐方式；
- `Canvas.Left` / `Canvas.Top`：Canvas 绝对定位坐标（附加属性）。

### 4. 继承自 UIElement 的交互与变换属性

- `IsHitTestVisible`：是否参与命中测试；
- `RenderTransform` / `RenderTransformOrigin`：渲染变换（平移、旋转、缩放）；
- `Opacity` / `OpacityMask`：透明度与透明遮罩；
- `SnapsToDevicePixels`：是否对齐物理像素，避免线条模糊；
- 所有标准路由事件：`MouseLeftButtonDown`、`MouseEnter` 等。

------

## 四、方法实现说明

`Ellipse` 没有重写任何布局、渲染相关的方法，`MeasureOverride`、`ArrangeOverride`、`OnRender` 均直接使用 `Shape` 基类的统一实现：

1. **MeasureOverride**：基类根据 `DefiningGeometry` 的边界、描边厚度、拉伸模式，计算控件期望的布局尺寸；
2. **ArrangeOverride**：基类根据最终分配的空间，计算缩放变换，生成 `GeometryTransform`；
3. **OnRender**：基类拿到最终几何后，先执行 `Fill` 填充，再用 `Pen` 执行 `Stroke` 描边，完成最终绘制。

> 设计价值：所有 Shape 派生类（矩形、椭圆、直线、多边形）共享同一套布局渲染逻辑，行为完全一致，开发者学习一个即可举一反三；同时框架优化性能时，所有图形同时受益。

------

## 五、依赖属性体系说明

`Ellipse` 自身**没有定义任何新的依赖属性字段**，所有可绑定、可动画、可样式化的属性全部继承自 `Shape` 和 `FrameworkElement`。

`Shape` 基类中定义、`Ellipse` 直接继承的依赖属性字段包括：

csharp:

```c#
public static readonly DependencyProperty FillProperty;
public static readonly DependencyProperty StrokeProperty;
public static readonly DependencyProperty StrokeThicknessProperty;
public static readonly DependencyProperty StrokeDashArrayProperty;
public static readonly DependencyProperty StrokeDashOffsetProperty;
public static readonly DependencyProperty StrokeStartLineCapProperty;
public static readonly DependencyProperty StrokeEndLineCapProperty;
public static readonly DependencyProperty StrokeDashCapProperty;
public static readonly DependencyProperty StrokeLineJoinProperty;
public static readonly DependencyProperty StrokeMiterLimitProperty;
public static readonly DependencyProperty StretchProperty;
```

所有这些属性都可以直接在 `Ellipse` 上使用，原生支持数据绑定、样式设置、动画驱动，完美适配 MVVM 开发模式。

------

## 六、完整运行原理（从实例化到渲染）

1. **实例化**：调用 `new Ellipse()` 创建对象，初始化所有继承的依赖属性默认值；
2. **加入可视化树**：被添加到父容器后，进入 WPF 布局与渲染流水线；
3. **测量阶段（Measure）**：布局系统调用 `MeasureOverride`，基类读取 `DefiningGeometry` 的原始边界，结合拉伸模式计算期望尺寸；
4. **排列阶段（Arrange）**：布局系统分配最终渲染尺寸，基类计算缩放与偏移变换，生成 `GeometryTransform`；
5. **渲染阶段（Render）**：渲染系统回调 `OnRender`，基类将原始几何应用变换后，依次绘制填充和描边，通过 DirectX 硬件加速输出到屏幕；
6. **属性变更**：任意图形相关依赖属性变化时，自动触发重测、重排、重绘，无需手动刷新。

------

## 七、官方设计的核心特点

1. **极简的具体类实现**：每个具体图形只负责定义自己的几何形状，其余全部复用基类能力，代码高度复用，一致性极强；
2. **密封类保证性能与安全**：禁止继承避免了派生类破坏渲染逻辑，也便于框架做内联优化；
3. **形状由布局驱动**：没有单独的 “半径” 属性，形状完全由布局尺寸决定，天然适配 WPF 的布局体系，自适应不同容器；
4. **完全融入框架生态**：和标准控件一样支持绑定、样式、模板、动画、事件，无需特殊处理即可融入 MVVM 架构。

------

## 补充：Ellipse 与 EllipseGeometry 的区别

很多开发者容易混淆二者，官方定位有本质区别：

| 对比维度 | `Ellipse`（Shape 派生类）           | `EllipseGeometry`（几何对象）  |
| :------- | :---------------------------------- | :----------------------------- |
| 继承体系 | `FrameworkElement` 派生，是 UI 元素 | `Geometry` 派生，是纯数据对象  |
| 布局能力 | 参与布局系统，有宽高、对齐、边距    | 不参与布局，只有坐标和半径     |
| 交互能力 | 支持鼠标事件、命中测试              | 无交互能力，仅用于形状描述     |
| 重量级别 | 较重，有依赖属性、渲染管线开销      | 极轻量，纯几何数据             |
| 适用场景 | UI 层直接展示、交互、绑定           | 底层绘图、自定义图形、路径组合 |

简单来说：`Ellipse` 是 “可以直接放到界面上用的控件”，`EllipseGeometry` 是 “用来描述形状的底层数据”，`Ellipse` 内部就是用 `EllipseGeometry` 来定义形状的。