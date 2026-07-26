# 006006001_WPF `Polygon` 多边形类官方深度解析 + 实战案例

`Polygon` 是 WPF 矢量图形体系中 `Shape` 抽象基类的**密封派生类**，用于绘制由一组顶点定义的**强制闭合多边形**，是工业上位机中实现不规则检测 ROI、工位轮廓、功能分区的核心图形控件。它与 `Polyline` 高度相似，但核心区别在于：`Polygon` 会自动将起点与终点首尾相连形成闭合轮廓，填充与描边均基于闭合区域，是封闭平面图形场景的首选方案。

------

## 一、官方类定义与继承体系

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

各层能力继承与其他 Shape 派生类完全一致：

- 继承 `FrameworkElement`：天然融入 WPF 布局系统，支持数据绑定、样式、动画、路由事件；
- 继承 `Shape`：提供统一的填充、描边、拉伸、几何渲染等图形通用能力；
- `Polygon` 自身：仅实现闭合多边形的顶点定义与填充规则，是继承链最末端的具体图形实现。

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

1. **极简实现**：仅新增 2 个依赖属性 + 1 个重写几何属性，无自定义方法，所有外观、布局、交互逻辑全部复用基类；
2. **密封类约束**：不允许派生继承，如需自定义多边形变体，需直接继承 `Shape` 基类；
3. **强制闭合特性**：**自动将起点与终点首尾相连**，始终是闭合图形，`Fill` 填充属性原生有效，这是与 `Polyline` 最本质的区别；
4. **集合驱动渲染**：顶点集合具备变更通知能力，增删顶点时自动触发重绘，无需手动刷新。

------

## 二、核心属性深度解析

### 1. 独有核心属性：Points 顶点集合

这是 `Polygon` 形状定义的核心，决定了多边形的轮廓、大小与形状。

| 属性项        | 详细说明                                                     |
| :------------ | :----------------------------------------------------------- |
| **类型**      | `System.Windows.Media.PointCollection`                       |
| **集合特性**  | 继承自 `Freezable`，支持冻结优化；内置变更通知，增删点自动触发重绘 |
| **默认值**    | 空集合（无顶点，多边形不可见）                               |
| **坐标体系**  | 相对于 Polygon 元素自身布局区域的左上角，X 向右、Y 向下，与 WPF 标准坐标系一致 |
| **XAML 语法** | 空格分隔多个顶点，单个顶点用「x,y」格式，例如：`Points="0,100 50,0 100,100"` |
| **边数规则**  | N 个顶点对应 N 条边（自动首尾相连），无需手动将最后一个点设为与起点相同 |

> 与 `Polyline` 的核心区别：
>
> - `Polyline`：N 个顶点 → N-1 条线段，默认开放；
> - `Polygon`：N 个顶点 → N 条边，自动闭合，始终是封闭图形。

### 2. 独有属性：FillRule 填充规则

控制闭合区域的填充计算规则，由于 `Polygon` 是强制闭合图形，该属性始终生效（`Polyline` 仅在设置 `Fill` 时才生效），对应两种经典填充算法：

| 枚举值              | 官方算法说明                                                 | 效果特点                           | 典型场景                 |
| :------------------ | :----------------------------------------------------------- | :--------------------------------- | :----------------------- |
| `EvenOdd`（默认值） | 奇偶环绕规则：从任意点向外发射射线，穿过的边数为奇数则填充，偶数则镂空 | 交叉区域自动镂空，适合简单多边形   | 普通检测区域、简单轮廓   |
| `Nonzero`           | 非零环绕规则：根据边的绘制方向累计环绕数，计数非零则填充     | 绝大多数闭合区域都会填充，镂空更少 | 复杂嵌套多边形、实心区域 |

> 注意：该属性仅影响填充区域，不影响描边轮廓的形状；顶点顺序会影响 Nonzero 模式的填充结果。

### 3. 继承自 Shape 的核心有效属性

`Polygon` 是标准闭合图形，Shape 基类的填充、描边属性全部原生有效：

| 属性分类     | 属性名                                 | 作用与工业场景价值                                           |
| :----------- | :------------------------------------- | :----------------------------------------------------------- |
| 内部填充     | `Fill`（Brush 类型）                   | 填充多边形闭合区域，半透明填充用于高亮检测区、功能分区       |
| 描边基础     | `Stroke`                               | 多边形轮廓画刷 / 颜色，为 `null` 时边框完全不显示            |
| 描边基础     | `StrokeThickness`                      | 轮廓线条厚度，单位为与设备无关像素，支持小数                 |
| **拐角控制** | **`StrokeLineJoin`**                   | **所有拐角的连接方式：**- `Miter`：尖角（默认）；- `Bevel`：斜切平角；- `Round`：圆角平滑过渡 |
| 尖角限制     | `StrokeMiterLimit`                     | 斜接尖角的最大延伸比例，默认 10，防止小角度拐角出现超长尖刺  |
| 虚线轮廓     | `StrokeDashArray` / `StrokeDashOffset` | 虚线多边形边框，用于规划区域、临时选区                       |
| 拉伸模式     | `Stretch`                              | 控制多边形如何适配布局空间：None/Fill/Uniform/UniformToFill  |
| 渲染几何     | `RenderedGeometry`                     | 只读，最终渲染生效的几何（含边框厚度、变换影响），用于精确命中测试 |

### 4. 布局属性的认知误区

`Width`、`Height` 是布局属性，**不直接决定多边形的形状与大小**：

- 多边形的原始形状由 `Points` 集合的坐标范围决定；
- 只有当 `Stretch` 不为 `None` 时，多边形才会根据布局尺寸缩放适配；
- 高精度绘图推荐使用 `Stretch="None"`，配合 Canvas 绝对定位，保证坐标比例准确。

### 5. 核心重写属性：DefiningGeometry

csharp:

```c#
protected override Geometry DefiningGeometry { get; }
```

这是 `Polygon` 唯一重写的核心成员，也是 `Shape` 基类强制要求的扩展点。

#### 官方内部实现逻辑

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

#### 详细解读

1. **核心差异点**：`figure.IsClosed = true`，这是与 `Polyline` 最本质的区别 —— 自动闭合，始终形成封闭轮廓；
2. **职责单一**：仅根据顶点集合和填充规则返回多边形的原始几何定义，不关心描边、拉伸、变换等渲染细节；
3. **与 `RenderedGeometry` 的区别**：
   - `DefiningGeometry`：原始设计几何，仅包含顶点坐标与填充规则，不包含线条厚度、拉伸变换；
   - `RenderedGeometry`：最终渲染几何，包含了描边厚度、变换等全部参数，用于精确命中测试。

------

## 三、核心功能与底层原理

### 1. 形状定义规则

- 由 `Points` 集合中的顶点按顺序依次连接，**自动将最后一个顶点与第一个顶点相连**，形成闭合轮廓；
- 顶点顺序直接决定多边形的形状，顺序错乱会出现交叉、自相交等异常；
- 支持凸多边形、凹多边形、自相交多边形，配合不同 `FillRule` 可实现丰富的填充效果。

### 2. 完整渲染流程

`Polygon` 没有重写布局渲染方法，完全复用 `Shape` 基类的标准流水线：

1. **测量阶段**：基类读取 `DefiningGeometry` 边界，结合线条厚度、拉伸模式，计算元素期望尺寸；
2. **排列阶段**：布局系统分配最终空间，基类计算缩放与偏移变换；
3. **渲染阶段**：
   - 应用几何变换得到最终渲染轮廓；
   - 若 `Fill` 不为 null，按 `FillRule` 计算填充区域并绘制内部；
   - 再根据描边参数构造 `Pen`，绘制闭合轮廓线；
   - 全部通过 DirectX 硬件加速输出。

### 3. 精确命中测试

- 基于闭合几何做精确命中测试：点击多边形填充区域或描边区域才会响应，外部空白区域不响应；
- 若 `Fill = null`，仅描边区域响应点击，内部空白区域不参与命中测试；
- 若需要内部空白区域也响应点击，必须显式设置 `Fill="Transparent"`，这是高频踩坑点。

### 4. 与 Polyline 的本质区别

| 对比项    | Polygon                      | Polyline                         |
| :-------- | :--------------------------- | :------------------------------- |
| 闭合性    | 强制自动闭合，始终是封闭图形 | 默认开放，仅设置 Fill 时隐式闭合 |
| 边数      | N 个顶点 → N 条边            | N 个顶点 → N-1 条线段            |
| Fill 属性 | 原生有效，填充闭合区域       | 仅填充时生效，自动隐式闭合       |
| FillRule  | 始终生效（图形本身闭合）     | 仅在设置 Fill 时影响填充         |
| 典型用途  | 封闭区域、平面图形、选区     | 连续轨迹、趋势曲线、开放路径     |

------

## 四、基础使用方法

### 1. XAML 基础用法

通过 `Points` 属性声明顶点序列，自动闭合形成多边形。

xaml:

```xaml
<Canvas Width="300" Height="200" SnapsToDevicePixels="True">
    <!-- 正三角形：3个顶点自动闭合 -->
    <Polygon Points="150,20 20,180 280,180"
             Stroke="DarkBlue"
             StrokeThickness="2"
             Fill="#334169E1"
             StrokeLineJoin="Round"/>
</Canvas>
```

### 2. C# 后台动态创建

适合算法动态生成检测区域、不规则选区的场景。

csharp:

```c#
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

// 创建五边形
Polygon pentagon = new Polygon();
pentagon.Stroke = Brushes.DarkGreen;
pentagon.StrokeThickness = 2;
pentagon.Fill = new SolidColorBrush(Color.FromArgb(0x33, 0x00, 0x80, 0x00));
pentagon.StrokeLineJoin = PenLineJoin.Round;

// 添加5个顶点
pentagon.Points.Add(new Point(100, 20));
pentagon.Points.Add(new Point(180, 60));
pentagon.Points.Add(new Point(150, 160));
pentagon.Points.Add(new Point(50, 160));
pentagon.Points.Add(new Point(20, 60));

// 添加到画布
canvas.Children.Add(pentagon);
```

### 3. 不同拉伸模式效果

xaml:

```xaml
<Grid Width="200" Height="200" ShowGridLines="True">
    <!-- Fill 模式：拉伸填满整个单元格，宽高比可能失真 -->
    <Polygon Points="0,0 100,0 100,100 0,100"
             Stretch="Fill"
             Stroke="Red" StrokeThickness="2"
             Fill="Transparent"/>
</Grid>
```

------

## 五、实战场景案例

### 案例 1：工业不规则检测 ROI 区域

**场景**：视觉检测中的异形感兴趣区域，贴合产品轮廓，半透明填充高亮。

xaml:

```xaml
<Canvas Width="600" Height="400" Background="#1E1E1E" SnapsToDevicePixels="True">
    <!-- 异形检测区：贴合产品轮廓的闭合多边形 -->
    <Polygon Points="120,300 180,120 320,100 420,180 450,320 300,350 150,340"
             Stroke="Cyan"
             StrokeThickness="2"
             Fill="#2200FFFF"
             StrokeLineJoin="Round"
             FillRule="Nonzero"/>
</Canvas>
```

> 工业价值：可精确贴合不规则产品、工件的边缘，比矩形 ROI 更精准，减少无效检测区域。

### 案例 2：工位功能分区轮廓

**场景**：产线工位布局图中的功能分区，不同颜色区分上料区、检测区、下料区。

xaml:

```xaml
<Canvas Width="500" Height="300" Background="#F5F5F5">
    <!-- 上料区：蓝色 -->
    <Polygon Points="0,0 150,0 150,300 0,300"
             Stroke="#1E90FF" StrokeThickness="1.5"
             Fill="#331E90FF"/>
    
    <!-- 检测区：绿色 -->
    <Polygon Points="150,0 400,0 400,200 150,200"
             Stroke="#32CD32" StrokeThickness="1.5"
             Fill="#3332CD32"/>
    
    <!-- 下料区：橙色 -->
    <Polygon Points="150,200 400,200 400,300 150,300 500,300 500,200"
             Stroke="#FF8C00" StrokeThickness="1.5"
             Fill="#33FF8C00"/>
</Canvas>
```

### 案例 3：雷达图（蜘蛛图）数据可视化

**场景**：能力评估、多维度指标对比的雷达图，纯 Polygon 实现，无需第三方图表库。

xaml:

```xaml
<Canvas Width="300" Height="300" Background="White">
    <!-- 背景网格：三层五边形 -->
    <Polygon Points="150,30 265,115 220,260 80,260 35,115"
             Stroke="#DDD" StrokeThickness="1" Fill="Transparent"/>
    <Polygon Points="150,70 225,125 195,220 105,220 75,125"
             Stroke="#DDD" StrokeThickness="1" Fill="Transparent"/>
    <Polygon Points="150,110 185,135 170,180 130,180 115,135"
             Stroke="#DDD" StrokeThickness="1" Fill="Transparent"/>
    
    <!-- 数据区域：半透明填充 -->
    <Polygon Points="150,50 240,130 200,230 90,210 50,120"
             Stroke="#409EFF" StrokeThickness="2"
             Fill="#66409EFF"
             StrokeLineJoin="Round"/>
</Canvas>
```

### 案例 4：FillRule 填充效果对比

直观展示奇偶环绕与非零环绕的差异，自相交多边形效果最明显。

xaml:

```xaml
<StackPanel Orientation="Horizontal" Margin="20" Spacing="50">
    <!-- EvenOdd：奇偶镂空，交叉区域空白 -->
    <Canvas Width="200" Height="200">
        <Polygon Points="20,100 100,20 180,100 100,180"
                 Stroke="Black" StrokeThickness="2"
                 Fill="#888888"
                 FillRule="EvenOdd"/>
        <TextBlock Text="EvenOdd 奇偶规则" Canvas.Top="180" FontSize="12"/>
    </Canvas>

    <!-- Nonzero：非零填充，全部实心 -->
    <Canvas Width="200" Height="200">
        <Polygon Points="20,100 100,20 180,100 100,180"
                 Stroke="Black" StrokeThickness="2"
                 Fill="#888888"
                 FillRule="Nonzero"/>
        <TextBlock Text="Nonzero 非零规则" Canvas.Top="180" FontSize="12"/>
    </Canvas>
</StackPanel>
```

### 案例 5：可点击的多边形选区（精确命中）

**场景**：地图分区、工位区域的点击交互，点击多边形内部触发对应操作。

xaml:

```xaml
<Polygon Points="50,200 150,50 300,80 350,250 180,300"
         Stroke="RoyalBlue" StrokeThickness="2"
         Fill="Transparent"
         Cursor="Hand"
         MouseLeftButtonUp="Region_MouseLeftButtonUp"/>
```

后台事件处理：

csharp:

```c#
private void Region_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
{
    MessageBox.Show("点击了检测区域，打开区域参数配置");
}
```

> 优势：基于几何精确命中，只有点击多边形内部 / 边框才响应，边角空白区域不会误触发，交互精度远高于矩形控件。

------

## 六、常见避坑与最佳实践

### 1. 不要手动重复起点

`Polygon` 会自动首尾相连，无需把最后一个顶点设置为与第一个顶点相同，否则会出现重叠边，影响拐角渲染和填充计算。

### 2. 自相交多边形注意 FillRule

自相交、嵌套的复杂多边形，默认 `EvenOdd` 会出现镂空效果；如果需要全部实心填充，设置 `FillRule="Nonzero"`。

### 3. 小角度拐角尖刺问题

默认 `StrokeLineJoin="Miter"` 尖角模式下，小角度拐角会延伸出长尖刺。解决方案：

- 设置 `StrokeLineJoin="Round"` 或 `Bevel`，替换拐角样式；
- 调小 `StrokeMiterLimit` 值，限制尖角最大延伸长度。

### 4. 内部点击无响应：设置 Fill="Transparent"

只设置描边不设置填充时，多边形内部空白区域不参与命中测试；需要内部也响应点击，必须显式设置透明填充。

### 5. 性能注意事项

- 顶点数几十以内的静态多边形，`Polygon` 开发效率最高；
- 上百个顶点高频刷新时，性能开销会增大，超高性能场景建议改用 `StreamGeometry` 直接绘制；
- 静态场景下将画刷、几何对象调用 `Freeze()` 冻结，可显著提升渲染性能。

### 6. 选型原则

- 闭合平面区域、选区、轮廓 → 用 `Polygon`；
- 开放轨迹、曲线、趋势线 → 用 `Polyline`；
- 含贝塞尔曲线、复杂组合图形 → 用 `Path`。