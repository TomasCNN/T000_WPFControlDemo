# 006005002_WPF `Polyline` 折线类官方类定义深度解析

`Polyline` 是 WPF 矢量图形体系中 `Shape` 抽象基类的**密封派生类**，用于绘制由一组顶点依次连接而成的连续多段折线，是工业上位机实现运动轨迹、工艺趋势曲线、扫描路径、轮廓标注的核心图形控件。它仅新增顶点集合与填充规则两个依赖属性，其余描边、布局、交互、渲染能力全部继承自 `Shape` 与 `FrameworkElement`，是连续线条场景下性能与开发效率最优的方案。

------

## 一、官方基础定义与继承体系

### 1. 基础元信息

- **命名空间**：`System.Windows.Shapes`
- **程序集**：`PresentationFramework.dll`（WPF 核心框架程序集）
- **类修饰符**：`public sealed`（公共密封类，禁止被其他类继承）
- **设计定位**：连续多段开放折线矢量图形，基于顶点集合驱动渲染，专注于直线段连接的路径绘制

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
                    └─ System.Windows.Shapes.Polyline
```

各层级能力继承：

- `DispatcherObject`：WPF 线程亲和性，只能在创建它的 UI 线程访问；
- `DependencyObject`：支持依赖属性系统，原生具备数据绑定、样式、动画、属性值继承能力；
- `Visual`：接入 WPF 可视化树，提供 DirectX 硬件加速渲染、命中测试基础；
- `UIElement`：具备路由事件、输入响应、可见性控制、裁剪等基础 UI 能力；
- `FrameworkElement`：完整融入 WPF 布局系统，支持宽高、边距、对齐、数据上下文等标准控件属性；
- `Shape`：提供统一的填充、描边、拉伸、几何渲染等图形通用抽象；
- `Polyline`：仅实现折线的顶点集合定义与填充规则，是继承链最末端的具体图形实现。

### 3. 官方类核心成员声明

以下为 `Polyline` 类与图形相关的核心成员精简定义（与官方源码结构一致）：

csharp:

```c#
public sealed class Polyline : Shape
{
    // 自有依赖属性标识符字段
    public static readonly DependencyProperty PointsProperty;
    public static readonly DependencyProperty FillRuleProperty;

    // 公共无参构造函数
    public Polyline();

    // 折线顶点集合
    public PointCollection Points { get; set; }

    // 填充规则
    public FillRule FillRule { get; set; }

    // 重写 Shape 抽象属性：定义折线的原始几何
    protected override Geometry DefiningGeometry { get; }
}
```

#### 关键结构解读

1. **极简实现**：整个类仅新增 2 个依赖属性 + 1 个重写的几何属性，无任何自定义方法，所有外观、布局、交互逻辑全部复用基类；
2. **密封类约束**：不允许派生继承，如需自定义折线变体，需直接继承 `Shape` 基类；
3. **半开放图形特性**：视觉上是开放折线，但设置 `Fill` 后会**隐式自动首尾相连形成闭合区域**并填充，这是最高频的踩坑特性；
4. **集合驱动渲染**：顶点集合具备变更通知能力，增删顶点时自动触发重绘，无需手动调用刷新。

------

## 二、静态依赖属性字段详解

开头的 2 个 `public static readonly DependencyProperty` 字段，是 WPF 依赖属性系统的**标识符字段**，与下方两个实例属性一一对应（命名规则：属性名 + Property）。

### 1. PointsProperty

- 对应实例属性：`Points`
- 注册元数据：标记 `FrameworkPropertyMetadataOptions.AffectsMeasure` 与 `AffectsRender`
- 设计意义：
  - 顶点集合变更（增、删、替换）时，自动触发布局重测与渲染重绘，无需开发者手动刷新；
  - 原生支持数据绑定，可直接绑定视图模型中的点集合，适配 MVVM 架构；
  - 所有 `Polyline` 实例共享同一份属性元数据，内存高效。

### 2. FillRuleProperty

- 对应实例属性：`FillRule`
- 默认值：`FillRule.EvenOdd`
- 注册元数据：标记 `AffectsRender`（仅影响渲染，不影响布局测量）
- 设计意义：填充规则变更时仅触发重绘，不触发布局重算，性能更优。

------

## 三、构造函数

csharp:

```c#
public Polyline();
```

1. **无参数公共构造**：用于外部实例化折线对象；
2. **轻量初始化**：构造函数仅初始化空的 `PointCollection` 集合，应用基类属性默认值，不执行几何计算，保证实例化性能；
3. **核心默认值**：
   - `Points`：空的 `PointCollection` 集合（无顶点，折线不可见）
   - `FillRule`：`FillRule.EvenOdd`（奇偶环绕填充规则）
   - `Fill = null`、`Stroke = null`（默认无填充、无描边，完全透明）
   - `StrokeThickness = 1.0`
   - `Stretch = Stretch.None`

> 新手常见坑：实例化 Polyline 后如果只添加顶点，不设置 `Stroke`，线条会完全透明不显示，必须至少赋值 `Stroke` 画刷。

------

## 四、实例属性深度解析

### 1. 独有核心属性：Points

这是 `Polyline` 形状定义的核心，决定了折线的位置、长度、走向与形状。

| 属性项        | 详细说明                                                     |
| :------------ | :----------------------------------------------------------- |
| **类型**      | `System.Windows.Media.PointCollection`                       |
| **集合特性**  | 继承自 `Freezable`，支持冻结优化；内置变更通知，增删点自动触发重绘 |
| **默认值**    | 空集合                                                       |
| **坐标体系**  | 相对于 Polyline 元素自身布局区域的左上角，X 轴向右递增，Y 轴向下递增，与 WPF 标准坐标系完全一致 |
| **XAML 语法** | 空格分隔多个顶点，单个顶点用「x,y」格式，例如：`Points="0,100 50,40 150,80 250,20"` |
| **数量关系**  | N 个顶点对应 N-1 段直线，顶点顺序直接决定折线形状            |

#### 工业场景关键特性

- 实时轨迹、趋势曲线场景下，只需向 `Points` 集合追加新顶点，UI 自动刷新，无需手动调用重绘方法；
- 集合实现了高效的增量更新，单点追加的性能开销极低，满足中高频刷新需求；
- 若需批量更新大量顶点，建议先临时替换整个集合，避免多次触发重绘。

### 2. 独有属性：FillRule

控制填充区域的计算规则，**仅在设置了 `Fill` 属性时生效**，对应两种经典计算机图形学填充算法：

| 枚举值              | 官方算法说明                                                 | 效果特点                                               |
| :------------------ | :----------------------------------------------------------- | :----------------------------------------------------- |
| `EvenOdd`（默认值） | 奇偶环绕规则：从任意点向外部发射射线，穿过的边数为奇数则填充，偶数则镂空 | 交叉区域自动镂空，适合简单图形                         |
| `Nonzero`           | 非零环绕规则：根据边的绘制方向累计环绕数，计数非零则填充     | 绝大多数闭合区域都会填充，镂空更少，更符合常规视觉预期 |

> 注意：该属性仅影响填充区域的计算，对描边线条的形状、位置无任何影响；如果不设置 `Fill`，该属性完全无意义。

### 3. 继承自 Shape 的关键有效属性

折线存在大量拐角，描边类属性对最终效果影响极大，是工业场景重点调优项：

| 属性分类     | 属性名                                    | 作用与工业场景价值                                           |
| :----------- | :---------------------------------------- | :----------------------------------------------------------- |
| 描边基础     | `Stroke`                                  | 折线线条的画刷 / 颜色，为 `null` 时线条完全不显示            |
| 描边基础     | `StrokeThickness`                         | 线条厚度，单位为与设备无关像素，支持小数                     |
| **拐角控制** | **`StrokeLineJoin`**                      | **多段线拐角的连接方式：**- `Miter`：尖角（默认），小角度下会出现长尖刺；- `Bevel`：斜切平角；- `Round`：圆角平滑过渡，运动轨迹、趋势曲线推荐使用 |
| 尖角限制     | `StrokeMiterLimit`                        | 斜接尖角的最大延伸比例，默认值 10，用于限制小角度拐角的尖刺长度 |
| 端点样式     | `StrokeStartLineCap` / `StrokeEndLineCap` | 整个折线**起点、终点**的端点样式，仅作用于首尾两端，不影响中间拐角 |
| 虚线样式     | `StrokeDashArray` / `StrokeDashOffset`    | 虚线折线，用于规划路径、预期轨迹等场景；配合动画可实现流动效果 |
| 虚线段端点   | `StrokeDashCap`                           | 虚线段每一小段的端点样式                                     |
| 填充         | `Fill`                                    | 隐式连接起点与终点形成闭合区域后填充内部；无需填充时请勿设置，避免意外闭合效果 |
| 拉伸模式     | `Stretch`                                 | 控制折线如何适配布局分配的空间：None/Fill/Uniform/UniformToFill |
| 渲染几何     | `RenderedGeometry`                        | 只读，最终渲染生效的几何（含线条厚度、变换影响），用于精确命中测试 |

> 工业最佳实践：绘制运动轨迹、平滑曲线时，务必设置 `StrokeLineJoin="Round"`，让拐角自然过渡，更贴合真实物理运动效果。

### 4. 布局属性的认知误区

`Width`、`Height` 是继承自 `FrameworkElement` 的布局属性，**不直接决定折线的尺寸与形状**：

- 折线的原始形状与大小由 `Points` 集合的坐标范围决定；
- 只有当 `Stretch` 不为 `None` 时，折线才会根据 `Width`/`Height` 进行缩放适配；
- 工业高精度绘图推荐使用 `Stretch="None"`，配合 Canvas 绝对定位，保证坐标比例准确。

### 5. 命中测试相关特性

- 基于实际折线几何做精确命中测试，只有点击到线条实体（含厚度范围）才会触发鼠标事件；
- 折线外接矩形内的空白区域不会响应点击，交互精度远高于矩形控件；
- 如果需要扩大点击热区，可增加 `StrokeThickness` 并设置透明描边。

------

## 五、核心重写属性：DefiningGeometry

csharp:

```c#
protected override Geometry DefiningGeometry { get; }
```

这是 `Polyline` 唯一重写的成员，也是 `Shape` 抽象基类强制所有派生类实现的扩展点。

### 官方内部实现逻辑

csharp:

```c#
protected override Geometry DefiningGeometry
{
    get
    {
        PathGeometry geometry = new PathGeometry();
        geometry.FillRule = this.FillRule;
        
        if (this.Points.Count > 0)
        {
            PathFigure figure = new PathFigure();
            figure.StartPoint = this.Points[0];
            // 用 PolyLineSegment 依次连接所有剩余顶点
            figure.Segments.Add(new PolyLineSegment(this.Points.Skip(1), true));
            geometry.Figures.Add(figure);
        }
        
        return geometry;
    }
}
```

### 详细解读

1. **访问级别**：`protected`，外部代码无法直接访问，仅基类和自身内部使用；
2. **职责单一**：仅根据顶点集合和填充规则返回折线的原始几何定义，不关心描边、拉伸、变换等渲染细节；
3. **设计思想**：**形状定义与渲染执行分离**。所有 Shape 派生类只需要定义 “自己是什么形状”，布局、描边、命中测试、动画等通用逻辑全部由基类统一处理，既保证了所有图形行为一致，也极大减少了重复代码；
4. **与 `RenderedGeometry` 的核心区别**：
   - `DefiningGeometry`：原始设计几何，仅包含顶点坐标与填充规则，不包含线条厚度、拉伸变换；
   - `RenderedGeometry`：最终渲染几何，包含了描边厚度、变换等全部渲染参数，用于精确命中测试和边界计算。

------

## 六、方法实现与布局渲染流程

`Polyline` 没有重写任何布局、渲染相关的方法，`MeasureOverride`、`ArrangeOverride`、`OnRender` 均直接使用 `Shape` 基类的统一实现，完整执行流程如下：

1. **测量阶段（Measure）**

   布局系统调用基类 `MeasureOverride`，读取 `DefiningGeometry` 的边界，结合线条厚度、`Stretch` 模式，计算 Polyline 元素期望的布局尺寸。

2. **排列阶段（Arrange）**

   布局系统分配最终渲染区域，基类根据区域大小和 `Stretch` 模式，计算缩放与偏移变换，生成 `GeometryTransform`。

3. **渲染阶段（OnRender）**

   渲染系统回调基类 `OnRender` 方法：

   - 将原始几何应用几何变换，得到最终渲染路径；
   - 若 `Fill` 不为 null，先按 `FillRule` 计算填充区域并绘制内部；
   - 再根据 `Stroke`、厚度、拐角、虚线等参数构造 `Pen` 对象，绘制折线轮廓；
   - 全部通过 DirectX 硬件加速输出到屏幕。

4. **属性变更响应**

   顶点集合、填充规则、描边等属性变化时，依赖属性系统根据元数据标记自动触发重测 / 重排 / 重绘，无需手动刷新。

------

## 七、官方设计的核心特点

1. **极简的具体类实现**：仅用两个属性完成折线的差异化定义，其余能力全部复用基类，代码高度复用，维护成本极低；
2. **密封类保证性能与安全**：禁止继承避免了派生类破坏渲染逻辑，也便于运行时做内联性能优化；
3. **半开放图形特性**：描边时表现为开放折线，填充时自动闭合，兼顾了线条绘制与区域填充两种需求；
4. **集合驱动渲染**：基于可观察集合实现增量更新，动态轨迹、曲线场景开发成本极低；
5. **完全融入框架生态**：和标准控件一样支持绑定、样式、动画、事件，无需特殊处理即可融入 MVVM 架构。

------

## 补充：易混淆图形类对比

### Polyline vs 多个 Line 拼接

| 对比项   | 单个 Polyline                 | 多个 Line 拼接           |
| :------- | :---------------------------- | :----------------------- |
| 渲染调用 | 单次绘制调用，性能高          | 多个元素多次绘制，开销大 |
| 拐角效果 | 统一拐角样式，过渡自然        | 线段衔接处有断点、毛刺   |
| 内存占用 | 单个 FrameworkElement，占用低 | 多个元素，占用高         |
| 维护成本 | 单个集合管理，简洁            | 多个对象逐个管理，繁琐   |
| 适用场景 | 连续轨迹、曲线                | 离散独立线段             |

> 结论：连续多段线条场景，优先使用 `Polyline`，不要用多个 `Line` 拼接。

### Polyline vs Polygon vs Path

表格

| 控件       | 闭合性                   | 核心用途                           | 灵活度                 |
| :--------- | :----------------------- | :--------------------------------- | :--------------------- |
| `Polyline` | 默认开放，填充时自动闭合 | 连续折线、运动轨迹、趋势曲线       | 中，仅支持直线段       |
| `Polygon`  | 强制首尾闭合             | 多边形区域、闭合轮廓、平面图形     | 中，仅支持直线段       |
| `Path`     | 可开可合                 | 任意复杂图形、贝塞尔曲线、几何组合 | 最高，支持所有几何类型 |

选型原则：连续直线轨迹用 `Polyline`，闭合多边形区域用 `Polygon`，复杂曲线 / 组合图形用 `Path`。