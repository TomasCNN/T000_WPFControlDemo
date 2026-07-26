# 006004002_WPF `Rectangle` 矩形类官方类定义深度解析

`Rectangle` 是 WPF 矢量图形体系中 `Shape` 抽象基类的**密封派生类**，用于绘制直角或圆角矩形，是所有 Shape 派生类中工业场景使用频率最高的图形元件。它本身仅新增 2 个圆角控制依赖属性，其余填充、描边、布局、交互、渲染能力全部继承自 `Shape` 与 `FrameworkElement`，完美适配检测 ROI、工位卡片、缺陷外框、进度指示等工控可视化需求。

------

## 一、官方基础定义与继承体系

### 1. 基础元信息

- **命名空间**：`System.Windows.Shapes`
- **程序集**：`PresentationFramework.dll`（WPF 核心框架程序集）
- **类修饰符**：`public sealed`（公共密封类，禁止被其他类继承）
- **设计定位**：通用闭合矩形矢量图形，支持直角与圆角，是 Shape 体系中最具通用性的具体图形实现。

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

各层级能力继承：

- `DispatcherObject`：WPF 线程亲和性，只能在创建它的 UI 线程访问；
- `DependencyObject`：支持依赖属性系统，原生具备数据绑定、样式、动画、属性值继承能力；
- `Visual`：接入 WPF 可视化树，提供 DirectX 硬件加速渲染、命中测试基础；
- `UIElement`：具备路由事件、输入响应、可见性控制、裁剪等基础 UI 能力；
- `FrameworkElement`：完整融入 WPF 布局系统，支持宽高、边距、对齐、数据上下文等标准控件属性；
- `Shape`：提供统一的填充、描边、拉伸、几何渲染等图形通用抽象；
- `Rectangle`：仅实现矩形的几何定义与圆角控制，是继承链最末端的具体图形实现。

### 3. 官方类核心成员声明

以下为 `Rectangle` 类与图形相关的核心成员精简定义（与官方源码结构一致）：

csharp:

```c#
public sealed class Rectangle : Shape
{
    // 自有依赖属性标识符字段
    public static readonly DependencyProperty RadiusXProperty;
    public static readonly DependencyProperty RadiusYProperty;

    // 公共无参构造函数
    public Rectangle();

    // 圆角控制实例属性
    public double RadiusX { get; set; }
    public double RadiusY { get; set; }

    // 重写 Shape 抽象属性：定义矩形的原始几何
    protected override Geometry DefiningGeometry { get; }
}
```

#### 关键结构解读

1. **极简实现**：整个类仅新增 2 个依赖属性 + 1 个重写的几何属性，无任何自定义方法，所有外观、布局、交互逻辑全部复用基类；
2. **密封类约束**：不允许派生继承，如需自定义矩形变体（如带十字标记的检测框），需直接继承 `Shape` 基类；
3. **闭合图形特性**：属于封闭几何图形，`Fill` 填充与 `Stroke` 描边属性均生效，这是与 `Line`/`Polyline` 等开放图形的核心区别；
4. **无新增方法**：所有操作均通过属性与继承的基类方法完成，保证了所有 Shape 派生类的行为一致性。

------

## 二、静态依赖属性字段详解

开头的 2 个 `public static readonly DependencyProperty` 字段，是 WPF 依赖属性系统的**标识符字段**，与下方 `RadiusX`、`RadiusY` 两个实例属性一一对应（命名规则：属性名 + Property）。

### 字段列表

- `RadiusXProperty`：对应 `RadiusX` 属性的依赖属性标识符
- `RadiusYProperty`：对应 `RadiusY` 属性的依赖属性标识符

### 设计意义

1. **原生支持 WPF 核心特性**：两个圆角属性全部基于依赖属性注册，天然支持数据绑定、样式复用、动画驱动、属性变更通知，可直接绑定圆角半径到视图模型；
2. **变更自动触发重绘**：注册时均标记了 `FrameworkPropertyMetadataOptions.AffectsMeasure` 与 `AffectsRender`，圆角半径变化时会自动触发布局重测和渲染重绘，无需开发者手动刷新界面；
3. **内存高效**：静态字段在类静态构造中完成元数据注册，所有 Rectangle 实例共享同一份属性定义，避免每个实例重复存储元数据。

------

## 三、构造函数

csharp:

```c#
public Rectangle();
```

1. **无参数公共构造**：用于外部实例化矩形对象；
2. **轻量初始化**：构造函数仅完成继承属性的默认值应用，不会提前创建几何对象，几何计算延迟到布局渲染阶段执行，保证实例化性能；
3. **核心默认值**：
   - `RadiusX = 0.0`、`RadiusY = 0.0`（默认直角矩形）
   - `Fill = null`（默认无填充，完全透明）
   - `Stroke = null`（默认无描边，边框完全透明）
   - `StrokeThickness = 1.0`
   - `Stretch = Stretch.None`

> 新手常见坑：实例化 Rectangle 后如果只设置宽高，不设置 `Stroke` 或 `Fill`，图形会完全透明不显示，必须至少赋值其中一个。

------

## 四、实例属性深度解析

### 1. 独有核心属性：圆角控制

这是 Rectangle 区别于其他 Shape 派生类的唯一自有属性，用于控制四个角的圆弧过渡程度。

| 属性      | 类型     | 默认值 | 官方说明                                                     |
| :-------- | :------- | :----- | :----------------------------------------------------------- |
| `RadiusX` | `double` | `0.0`  | 矩形圆角的**水平方向半径**，单位为与设备无关像素；值越大圆角越平缓，最大值不超过矩形宽度的 1/2 |
| `RadiusY` | `double` | `0.0`  | 矩形圆角的**垂直方向半径**，单位为与设备无关像素；值越大圆角越平缓，最大值不超过矩形高度的 1/2 |

#### 取值与形状对应关系

- 当 `RadiusX = 0` 且 `RadiusY = 0`：标准直角矩形，拐角尖锐；
- 当 `RadiusX > 0` 且 `RadiusY > 0`：圆角矩形，四个角平滑过渡；
- 当 `RadiusX = Width/2` 且 `RadiusY = Height/2`：矩形完全退化为椭圆；宽高相等时退化为正圆；
- 当赋值超过最大值时，WPF 会自动将其裁剪到最大有效值，不会抛出异常。

#### 工业场景说明

- 直角矩形：用于检测 ROI、缺陷外框等需要精准边界的场景；
- 圆角矩形：用于工位卡片、按钮背景、状态面板，视觉更柔和。

### 2. 继承自 Shape 的核心有效属性

Rectangle 是闭合图形，Shape 基类的填充、描边、拉伸属性全部生效，是控制外观的核心手段：

| 属性分类   | 属性名               | 工业场景用途                                                 |
| :--------- | :------------------- | :----------------------------------------------------------- |
| 内部填充   | `Fill`（Brush 类型） | 填充矩形内部区域，半透明填充用于高亮检测区域、工位状态底色   |
| 轮廓基础   | `Stroke`             | 矩形边框画刷 / 颜色，为 `null` 时边框完全不显示              |
| 轮廓基础   | `StrokeThickness`    | 边框厚度，单位为与设备无关像素，支持小数                     |
| 拐角描边   | `StrokeLineJoin`     | 直角边框的拐角连接方式：`Miter`(尖角)、`Bevel`(斜切)、`Round`(圆角)，小厚度下差异不明显 |
| 尖角限制   | `StrokeMiterLimit`   | 斜接尖角的最大延伸比例，限制小角度拐角的尖刺长度             |
| 虚线边框   | `StrokeDashArray`    | 虚线边框序列，按「实线长度、空白长度」循环，用于检测框、选中态 |
| 虚线偏移   | `StrokeDashOffset`   | 虚线起始偏移，配合动画实现流动扫描效果                       |
| 虚线段端点 | `StrokeDashCap`      | 虚线段的端点样式                                             |
| 拉伸模式   | `Stretch`            | 控制矩形如何适配布局分配的空间：None/Fill/Uniform/UniformToFill |
| 渲染几何   | `RenderedGeometry`   | 只读，最终渲染生效的几何（含边框厚度、变换影响），用于精确命中测试 |
| 几何变换   | `GeometryTransform`  | 只读，应用在原始几何上的变换矩阵，用于坐标换算               |

> 边框绘制特性：矩形边框为**居中绘制**。例如 2px 厚度的边框，1px 落在矩形内部、1px 落在外部；高精度坐标对齐场景下，需考虑边框厚度带来的外边界扩展。

### 3. 继承自 FrameworkElement 的布局属性

矩形的形状大小本质由布局系统分配的尺寸决定，核心布局属性包括：

- `Width` / `Height`：显式设置矩形外接矩形宽高，是最常用的尺寸控制方式；
- `MinWidth` / `MaxWidth` / `MinHeight` / `MaxHeight`：尺寸范围约束；
- `Canvas.Left` / `Canvas.Top`：Canvas 绝对定位时的左上角坐标，与视觉检测 ROI 的「左上角坐标 + 宽高」定义天然匹配，无需额外偏移；
- `HorizontalAlignment` / `VerticalAlignment`：Grid 等自适应容器中的对齐方式；
- `Margin` / `Padding`：外边距、内边距。

> 与 Ellipse 的核心区别：Rectangle 默认以左上角为定位基准，直接对应工业场景中 ROI 的常规定义方式，不需要做半径偏移。

### 4. 命中测试相关特性

- 矩形基于实际几何形状做精确命中测试：圆角矩形的四个角空白区域不会触发点击；
- 若 `Fill = null`（未设置填充），矩形内部空白区域不参与命中测试，只有边框区域响应点击；
- 若需要内部空白区域也响应鼠标事件，必须显式设置 `Fill="Transparent"`，这是高频踩坑点。

------

## 五、核心重写属性：DefiningGeometry

csharp:

```c#
protected override Geometry DefiningGeometry { get; }
```

这是 `Rectangle` 唯一重写的成员，也是 `Shape` 抽象基类强制所有派生类实现的扩展点。

### 官方内部实现逻辑

csharp:

```c#
protected override Geometry DefiningGeometry
{
    get
    {
        // 以控件渲染尺寸为外接矩形，结合圆角半径构造矩形几何
        return new RectangleGeometry(
            new Rect(0, 0, RenderSize.Width, RenderSize.Height),
            RadiusX,
            RadiusY);
    }
}
```

### 详细解读

1. **访问级别**：`protected`，外部代码无法直接访问，仅基类和自身内部使用；
2. **职责单一**：仅返回矩形的原始几何定义，不关心描边、拉伸、变换等渲染细节；
3. **设计思想**：**形状定义与渲染执行分离**。所有 Shape 派生类只需要定义 “自己是什么形状”，布局、描边、命中测试、动画等通用逻辑全部由基类统一处理，既保证了所有图形行为一致，也极大减少了重复代码；
4. **与 `RenderedGeometry` 的核心区别**：
   - `DefiningGeometry`：原始设计几何，仅包含形状坐标和圆角，不包含边框厚度、拉伸变换；
   - `RenderedGeometry`：最终渲染几何，包含了描边厚度、变换等全部渲染参数，用于精确命中测试和边界计算。

------

## 六、方法实现与布局渲染流程

`Rectangle` 没有重写任何布局、渲染相关的方法，`MeasureOverride`、`ArrangeOverride`、`OnRender` 均直接使用 `Shape` 基类的统一实现，完整执行流程如下：

1. **测量阶段（Measure）**

   布局系统调用基类 `MeasureOverride`，读取 `DefiningGeometry` 的边界，结合边框厚度、`Stretch` 模式，计算 Rectangle 元素期望的布局尺寸。

2. **排列阶段（Arrange）**

   布局系统分配最终渲染区域，基类根据区域大小和 `Stretch` 模式，计算缩放与偏移变换，生成 `GeometryTransform`。

3. **渲染阶段（OnRender）**

   渲染系统回调基类 `OnRender` 方法：

   - 将原始 `RectangleGeometry` 应用几何变换，得到最终渲染几何；
   - 若 `Fill` 不为 null，先用填充画刷绘制矩形内部区域；
   - 再根据 `Stroke`、厚度、圆角、虚线等参数构造 `Pen` 对象，绘制矩形边框；
   - 全部通过 DirectX 硬件加速输出到屏幕。

4. **属性变更响应**

   任意尺寸、圆角、描边属性变化时，依赖属性系统自动触发重测 / 重排 / 重绘，无需手动刷新。

------

## 七、官方设计的核心特点

1. **极简的具体类实现**：仅用两个属性完成矩形的差异化定义，其余能力全部复用基类，代码高度复用，维护成本极低；
2. **密封类保证性能与安全**：禁止继承避免了派生类破坏渲染逻辑，也便于运行时做内联性能优化；
3. **闭合图形完整能力**：同时支持填充与描边，适配背景、边框、高亮等多种场景；
4. **布局驱动形状**：形状尺寸完全由布局系统决定，天然适配 WPF 的布局体系，自适应不同容器与分辨率；
5. **完全融入框架生态**：和标准控件一样支持绑定、样式、模板、动画、事件，无需特殊处理即可融入 MVVM 架构。

------

## 补充：Rectangle 与 RectangleGeometry 的区别

很多开发者容易混淆二者，官方定位有本质区别：

| 对比维度 | `Rectangle`（Shape 派生类）   | `RectangleGeometry`（几何对象） |
| :------- | :---------------------------- | :------------------------------ |
| 类型     | UI 元素，控件级               | 纯数据，图形原语级              |
| 继承     | `FrameworkElement`            | `Geometry`                      |
| 布局能力 | 参与布局系统，有对齐、边距    | 不参与布局，只有坐标和尺寸数据  |
| 交互能力 | 支持鼠标事件、命中测试        | 无交互能力，仅描述形状          |
| 重量     | 较重，有依赖属性和布局开销    | 极轻量，纯几何数据              |
| 适用场景 | UI 层直接展示、交互、数据绑定 | 底层绘图、自定义图形、几何组合  |

简单来说：`Rectangle` 是 “可以直接放到界面上用的控件”，`RectangleGeometry` 是 “用来描述矩形形状的底层数据”，`Rectangle` 内部就是通过 `RectangleGeometry` 来定义自身形状的。