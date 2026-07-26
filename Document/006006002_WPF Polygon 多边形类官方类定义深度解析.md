# 006006002_WPF `Polygon` 多边形类官方类定义深度解析

`Polygon` 是 WPF 矢量图形体系中 `Shape` 抽象基类的**密封派生类**，用于绘制由一组顶点定义的**强制闭合多边形**。它与 `Polyline` 的成员结构高度相似，但核心本质区别在于：`Polygon` 会自动将起点与终点首尾相连，始终形成封闭轮廓，填充与描边均基于闭合区域，是封闭平面图形、不规则选区、功能分区场景的标准实现。

------

## 一、官方基础定义与继承体系

### 1. 基础元信息

- **命名空间**：`System.Windows.Shapes`
- **程序集**：`PresentationFramework.dll`（WPF 核心框架程序集）
- **类修饰符**：`public sealed`（公共密封类，禁止被其他类继承）
- **设计定位**：闭合多边形矢量图形，基于顶点集合驱动渲染，专注于封闭平面区域绘制

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
                    └─ System.Windows.Shapes.Polygon
```

各层级能力继承与所有 Shape 派生类完全一致：

- `DispatcherObject`：WPF 线程亲和性，只能在创建它的 UI 线程访问；
- `DependencyObject`：支持依赖属性系统，原生具备数据绑定、样式、动画、属性值继承能力；
- `Visual`：接入 WPF 可视化树，提供 DirectX 硬件加速渲染、命中测试基础；
- `UIElement`：具备路由事件、输入响应、可见性控制、裁剪等基础 UI 能力；
- `FrameworkElement`：完整融入 WPF 布局系统，支持宽高、边距、对齐、数据上下文等标准控件属性；
- `Shape`：提供统一的填充、描边、拉伸、几何渲染等图形通用抽象；
- `Polygon`：仅实现闭合多边形的顶点定义与填充规则，是继承链最末端的具体图形实现。

### 3. 官方类核心成员声明

以下为 `Polygon` 类与图形相关的核心成员精简定义（与官方源码结构一致）：

csharp:

```c#
public sealed class Polygon : Shape
{
    // 自有依赖属性标识符字段
    public static readonly DependencyProperty PointsProperty;
    public static readonly DependencyProperty FillRuleProperty;

    // 公共无参构造函数
    public Polygon();

    // 多边形顶点集合
    public PointCollection Points { get; set; }

    // 填充规则
    public FillRule FillRule { get; set; }

    // 重写 Shape 抽象属性：定义多边形的原始几何
    protected override Geometry DefiningGeometry { get; }
}
```

#### 关键结构解读

1. **极简实现**：整个类仅新增 2 个依赖属性 + 1 个重写的几何属性，无任何自定义方法，所有外观、布局、交互逻辑全部复用基类；
2. **密封类约束**：不允许派生继承，如需自定义多边形变体，需直接继承 `Shape` 基类；
3. **强制闭合特性**：**自动将起点与终点首尾相连**，始终是闭合图形，`Fill` 填充属性原生有效 —— 这是与 `Polyline` 最本质的区别；
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
  - 所有 `Polygon` 实例共享同一份属性元数据，内存高效。

### 2. FillRuleProperty

- 对应实例属性：`FillRule`
- 默认值：`FillRule.EvenOdd`
- 注册元数据：标记 `AffectsRender`（仅影响渲染，不影响布局测量）
- 设计意义：填充规则变更时仅触发重绘，不触发布局重算，性能更优。

------

## 三、构造函数

csharp:

```c#
public Polygon();
```

1. **无参数公共构造**：用于外部实例化多边形对象；
2. **轻量初始化**：构造函数仅初始化空的 `PointCollection` 集合，应用基类属性默认值，不执行几何计算，保证实例化性能；
3. **核心默认值**：
   - `Points`：空的 `PointCollection` 集合（无顶点，多边形不可见）
   - `FillRule`：`FillRule.EvenOdd`（奇偶环绕填充规则）
   - `Fill = null`、`Stroke = null`（默认无填充、无描边，完全透明）
   - `StrokeThickness = 1.0`
   - `Stretch = Stretch.None`

> 新手常见坑：实例化 Polygon 后如果只添加顶点，不设置 `Stroke` 或 `Fill`，图形会完全透明不显示，必须至少赋值其中一个。

------

## 四、实例属性深度解析

### 1. 独有核心属性：Points

这是 `Polygon` 形状定义的核心，决定了多边形的轮廓、大小与形状。

| 属性项        | 详细说明                                                     |
| :------------ | :----------------------------------------------------------- |
| **类型**      | `System.Windows.Media.PointCollection`                       |
| **集合特性**  | 继承自 `Freezable`，支持冻结优化；内置变更通知，增删点自动触发重绘 |
| **默认值**    | 空集合                                                       |
| **坐标体系**  | 相对于 Polygon 元素自身布局区域的左上角，X 轴向右递增，Y 轴向下递增，与 WPF 标准坐标系完全一致 |
| **XAML 语法** | 空格分隔多个顶点，单个顶点用「x,y」格式，例如：`Points="0,100 50,0 100,100"` |
| **边数规则**  | N 个顶点对应 N 条边，自动首尾闭合，无需手动将最后一个点设为与起点相同 |

> 与 `Polyline` 的核心本质区别：
>
> - `Polyline`：N 个顶点 → N-1 条线段，默认开放图形；
> - `Polygon`：N 个顶点 → N 条边，自动首尾相连，始终是闭合图形。

### 2. 独有属性：FillRule

控制闭合区域的填充计算规则。由于 `Polygon` 是强制闭合图形，该属性**始终生效**（`Polyline` 仅在设置 `Fill` 时才生效），对应两种经典计算机图形学填充算法：

| 枚举值              | 官方算法说明                                                 | 效果特点                                               | 典型场景                             |
| :------------------ | :----------------------------------------------------------- | :----------------------------------------------------- | :----------------------------------- |
| `EvenOdd`（默认值） | 奇偶环绕规则：从任意点向外部发射射线，穿过的边数为奇数则填充，偶数则镂空 | 交叉区域自动镂空，适合简单凸多边形                     | 普通检测区域、简单轮廓、规则分区     |
| `Nonzero`           | 非零环绕规则：根据边的绘制方向累计环绕数，计数非零则填充     | 绝大多数闭合区域都会填充，镂空更少，更符合实心图形预期 | 复杂凹多边形、自相交多边形、嵌套图形 |

> 注意：该属性仅影响填充区域的计算，对描边轮廓的形状、位置无任何影响；顶点的绘制顺序会直接影响 Nonzero 模式的填充结果。

### 3. 继承自 Shape 的关键有效属性

`Polygon` 是标准闭合图形，Shape 基类的填充、描边属性全部原生有效，其中拐角相关属性对多边形视觉效果影响极大：

| 属性分类     | 属性名                                 | 作用与场景价值                                               |
| :----------- | :------------------------------------- | :----------------------------------------------------------- |
| 内部填充     | `Fill`（Brush 类型）                   | 填充多边形闭合区域，半透明填充用于高亮检测区、功能分区底色   |
| 描边基础     | `Stroke`                               | 多边形轮廓画刷 / 颜色，为 `null` 时边框完全不显示            |
| 描边基础     | `StrokeThickness`                      | 轮廓线条厚度，单位为与设备无关像素，支持小数                 |
| **拐角控制** | **`StrokeLineJoin`**                   | **所有顶点拐角的连接方式：**- `Miter`：尖角（默认），小角度下易出现长尖刺；- `Bevel`：斜切平角；- `Round`：圆角平滑过渡，工业场景推荐使用 |
| 尖角限制     | `StrokeMiterLimit`                     | 斜接尖角的最大延伸比例，默认值 10，用于限制小角度拐角的尖刺长度 |
| 虚线轮廓     | `StrokeDashArray` / `StrokeDashOffset` | 虚线多边形边框，用于规划区域、临时选区、待确认范围           |
| 虚线段端点   | `StrokeDashCap`                        | 虚线段每一小段的端点样式                                     |
| 拉伸模式     | `Stretch`                              | 控制多边形如何适配布局分配的空间：None/Fill/Uniform/UniformToFill |
| 渲染几何     | `RenderedGeometry`                     | 只读，最终渲染生效的几何（含边框厚度、变换影响），用于精确命中测试 |
| 几何变换     | `GeometryTransform`                    | 只读，应用在原始几何上的变换矩阵，用于坐标换算               |

### 4. 布局属性的认知误区

`Width`、`Height` 是继承自 `FrameworkElement` 的布局属性，**不直接决定多边形的形状与大小**：

- 多边形的原始形状与尺寸由 `Points` 集合的坐标范围决定；
- 只有当 `Stretch` 不为 `None` 时，多边形才会根据布局尺寸进行缩放适配；
- 高精度绘图场景推荐使用 `Stretch="None"`，配合 Canvas 绝对定位，保证坐标比例准确。

### 5. 命中测试相关特性

- 基于闭合几何做精确命中测试：点击多边形填充区域或描边区域才会响应事件，外部空白区域不响应；
- 若 `Fill = null`（未设置填充），仅描边区域响应点击，内部空白区域不参与命中测试；
- 若需要内部空白区域也响应鼠标事件，必须显式设置 `Fill="Transparent"`，这是高频踩坑点。

------

## 五、核心重写属性：DefiningGeometry

csharp:

```c#
protected override Geometry DefiningGeometry { get; }
```

这是 `Polygon` 唯一重写的成员，也是 `Shape` 抽象基类强制所有派生类实现的扩展点。

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
            figure.IsClosed = true; // 核心：强制闭合，自动首尾相连
            figure.Segments.Add(new PolyLineSegment(this.Points.Skip(1), true));
            geometry.Figures.Add(figure);
        }
        
        return geometry;
    }
}
```

### 详细解读

1. **核心差异点**：`figure.IsClosed = true` 是 `Polygon` 与 `Polyline` 最本质的区别 —— 强制闭合路径，自动将最后一个顶点与第一个顶点相连，始终形成封闭轮廓；
2. **访问级别**：`protected`，外部代码无法直接访问，仅基类和自身内部使用；
3. **职责单一**：仅根据顶点集合和填充规则返回多边形的原始几何定义，不关心描边、拉伸、变换等渲染细节；
4. **设计思想**：**形状定义与渲染执行分离**。所有 Shape 派生类只需要定义 “自己是什么形状”，布局、描边、命中测试、动画等通用逻辑全部由基类统一处理，既保证了所有图形行为一致，也极大减少了重复代码；
5. **与 `RenderedGeometry` 的核心区别**：
   - `DefiningGeometry`：原始设计几何，仅包含顶点坐标与填充规则，不包含线条厚度、拉伸变换；
   - `RenderedGeometry`：最终渲染几何，包含了描边厚度、变换等全部渲染参数，用于精确命中测试和边界计算。

------

## 六、方法实现与布局渲染流程

`Polygon` 没有重写任何布局、渲染相关的方法，`MeasureOverride`、`ArrangeOverride`、`OnRender` 均直接使用 `Shape` 基类的统一实现，完整执行流程如下：

1. **测量阶段（Measure）**

   布局系统调用基类 `MeasureOverride`，读取 `DefiningGeometry` 的边界，结合线条厚度、`Stretch` 模式，计算 Polygon 元素期望的布局尺寸。

2. **排列阶段（Arrange）**

   布局系统分配最终渲染区域，基类根据区域大小和 `Stretch` 模式，计算缩放与偏移变换，生成 `GeometryTransform`。

3. **渲染阶段（OnRender）**

   渲染系统回调基类 `OnRender` 方法：

   - 将原始几何应用几何变换，得到最终渲染轮廓；
   - 若 `Fill` 不为 null，先按 `FillRule` 计算填充区域并绘制内部；
   - 再根据 `Stroke`、厚度、拐角、虚线等参数构造 `Pen` 对象，绘制闭合轮廓线；
   - 全部通过 DirectX 硬件加速输出到屏幕。

4. **属性变更响应**

   顶点集合、填充规则、描边等属性变化时，依赖属性系统根据元数据标记自动触发重测 / 重排 / 重绘，无需手动刷新。

------

## 七、官方设计的核心特点

1. **极简的具体类实现**：仅用两个属性完成多边形的差异化定义，其余能力全部复用基类，代码高度复用，维护成本极低；
2. **密封类保证性能与安全**：禁止继承避免了派生类破坏渲染逻辑，也便于运行时做内联性能优化；
3. **强制闭合语义明确**：从类的层面保证图形始终闭合，避免了 Polyline 隐式闭合的歧义，语义更清晰，更适合封闭区域场景；
4. **集合驱动渲染**：基于可观察集合实现增量更新，动态选区、轮廓调整场景开发成本极低；
5. **完全融入框架生态**：和标准控件一样支持绑定、样式、动画、事件，无需特殊处理即可融入 MVVM 架构。

------

## 补充：易混淆图形类对比

### Polygon vs Polyline

| 对比项    | Polygon                        | Polyline                         |
| :-------- | :----------------------------- | :------------------------------- |
| 闭合性    | 强制自动闭合，始终是封闭图形   | 默认开放，仅设置 Fill 时隐式闭合 |
| 边数规则  | N 个顶点 → N 条边              | N 个顶点 → N-1 条线段            |
| Fill 属性 | 原生有效，填充闭合区域         | 仅填充时生效，自动隐式闭合       |
| FillRule  | 始终生效（图形本身闭合）       | 仅在设置 Fill 时影响填充         |
| 典型用途  | 封闭区域、平面图形、选区、分区 | 连续轨迹、趋势曲线、开放路径     |

### Polygon vs Path

| 对比项   | Polygon                  | Path                                     |
| :------- | :----------------------- | :--------------------------------------- |
| 图元类型 | 仅直线段组成的闭合多边形 | 支持直线、贝塞尔曲线、圆弧等所有几何类型 |
| 灵活度   | 中等，仅支持直线顶点     | 极高，可绘制任意复杂形状                 |
| 易用性   | 高，顶点集合简单直观     | 较低，需要掌握路径语法或几何对象组合     |
| 性能     | 较好，简单场景开销低     | 更灵活但略重，复杂图形性能更优           |
| 选型原则 | 纯直线闭合多边形         | 含曲线、复合图形的复杂场景               |