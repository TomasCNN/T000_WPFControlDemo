# 006005001_WPWPF `Polyline` 折线类官方深度解析 + 工业场景实战

`Polyline` 是 WPF 矢量图形体系中 `Shape` 抽象基类的**密封派生类**，用于绘制由一组顶点依次连接而成的连续折线，是工业上位机中实现运动轨迹、数据趋势曲线、扫描路径、轮廓标注的核心图形控件。相比用多个 `Line` 拼接，`Polyline` 性能更优、拐角过渡更自然、代码更简洁，是多段连续线条场景的首选方案。

------

## 一、官方类定义与继承体系

### 1. 基础元信息

- **命名空间**：`System.Windows.Shapes`
- **程序集**：`PresentationFramework.dll`（WPF 核心框架程序集）
- **类修饰符**：`public sealed`（公共密封类，禁止被其他类继承）
- **设计定位**：连续多段折线矢量图形，专注于顶点序列连接的开放路径绘制

### 2. 完整继承链

plaintext：

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

各层能力继承：

- `FrameworkElement`：天然融入 WPF 布局系统，支持数据绑定、样式、动画、路由事件；
- `Shape`：提供统一的填充、描边、拉伸、几何渲染等图形通用能力；
- `Polyline`：仅实现折线的顶点集合定义与填充规则，是继承链最末端的具体图形实现。

### 3. 官方类核心成员声明

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

1. **极简实现**：仅新增 2 个依赖属性 + 1 个重写几何属性，无自定义方法，所有外观、布局、交互逻辑全部复用基类；
2. **密封类约束**：不允许派生继承，如需自定义折线变体，需直接继承 `Shape` 基类；
3. **半开放图形特性**：视觉上是开放折线，但设置 `Fill` 后会**自动将起点与终点首尾相连形成闭合区域**并填充，这是最容易踩坑的特性；
4. **集合驱动渲染**：顶点集合变更时自动触发重绘，适合动态轨迹、实时曲线等场景。

------

## 二、核心属性深度解析

### 1. 独有核心属性：Points 顶点集合

这是 `Polyline` 最核心的属性，决定了折线的形状、长度和走向。

| 属性项        | 详细说明                                                     |
| :------------ | :----------------------------------------------------------- |
| **类型**      | `PointCollection`（点集合，继承自 `Freezable`）              |
| **默认值**    | 空集合（无顶点，折线不可见）                                 |
| **坐标体系**  | 相对于 Polyline 元素自身布局区域的左上角，X 向右、Y 向下，与 WPF 标准坐标系一致 |
| **绑定支持**  | 基于依赖属性注册，原生支持数据绑定；集合内点的增删会自动触发重绘 |
| **XAML 写法** | 空格分隔顶点，每个顶点用「x,y」格式，例如 `Points="0,0 50,30 100,0 150,50"` |

> 工业场景说明：`PointCollection` 具备变更通知能力，实时轨迹、动态曲线场景下，只需向集合中追加新点，界面会自动刷新，无需手动调用重绘。

### 2. 独有属性：FillRule 填充规则

控制填充区域的计算规则，仅在设置了 `Fill` 属性时生效，对应两种经典填充算法：

| 枚举值            | 官方说明                                                     | 工业场景影响                                         |
| :---------------- | :----------------------------------------------------------- | :--------------------------------------------------- |
| `EvenOdd`（默认） | 奇偶环绕规则：从点向外发射射线，穿过奇数条边则填充，偶数条则镂空 | 复杂交叉折线填充时会出现镂空效果                     |
| `Nonzero`         | 非零环绕规则：根据边的方向计数，计数非零则填充               | 大多数闭合区域都会填充，镂空更少，更符合常规视觉预期 |

> 注意：该属性仅影响填充区域，不影响描边效果；如果不设置 `Fill`，该属性完全无意义。

### 3. 继承自 Shape 的关键属性

折线有大量拐角，描边类属性对最终效果影响极大，是工业场景重点调优项：

| 属性分类     | 属性名                                    | 工业场景作用                                                 |
| :----------- | :---------------------------------------- | :----------------------------------------------------------- |
| 描边基础     | `Stroke`                                  | 折线线条的画刷 / 颜色，为 `null` 时线条完全不显示            |
| 描边基础     | `StrokeThickness`                         | 线条厚度，单位为与设备无关像素，支持小数                     |
| **拐角样式** | **`StrokeLineJoin`**                      | **多段线拐角的连接方式：**- `Miter`：尖角（默认），小角度下会出现长尖刺；- `Bevel`：斜切平角；- `Round`：圆角平滑过渡，运动轨迹、曲线推荐使用 |
| 尖角限制     | `StrokeMiterLimit`                        | 斜接尖角的最大延伸比例，默认 10，用于防止小角度拐角出现超长尖刺 |
| 端点样式     | `StrokeStartLineCap` / `StrokeEndLineCap` | 折线起点、终点的端点样式：Flat/Square/Round/Triangle         |
| 虚线样式     | `StrokeDashArray` / `StrokeDashOffset`    | 虚线折线，用于规划路径、预期轨迹等场景                       |
| 填充         | `Fill`                                    | 自动首尾闭合后填充内部区域；无需填充时请勿设置，避免意外效果 |
| 拉伸模式     | `Stretch`                                 | 控制折线如何适配布局空间：None/Fill/Uniform/UniformToFill    |

> 工业最佳实践：绘制运动轨迹、平滑曲线时，务必设置 `StrokeLineJoin="Round"`，让拐角自然过渡，更贴合真实物理运动效果。

### 4. 布局属性的误区

`Width`、`Height` 是布局属性，**不直接决定折线的尺寸**：

- 折线的形状和大小由 `Points` 集合的坐标范围决定；
- `Stretch="Fill"` 时，折线会拉伸填满 `Width`/`Height` 指定的区域，可能失真；
- 工业绘图推荐使用 `Stretch="None"`，配合 Canvas 绝对定位，保证坐标精度。

### 5. 核心重写属性：DefiningGeometry

csharp:

```c#
protected override Geometry DefiningGeometry { get; }
```

这是 `Polyline` 唯一重写的核心成员，内部实现逻辑：

1. 遍历 `Points` 集合中的所有顶点；
2. 构造 `PathGeometry`，通过 `PolyLineSegment` 依次连接所有顶点；
3. 返回该几何对象供基类渲染使用。

> 设计思想：形状定义与渲染执行分离，`Polyline` 只负责定义顶点序列，描边、填充、布局、命中测试全部由基类统一处理，保证所有 Shape 派生类行为一致。

------

## 三、核心功能与底层原理

### 1. 形状定义规则

- 折线由 `Points` 集合中的顶点按顺序依次连接而成，N 个顶点对应 N-1 段直线；
- 默认是开放图形，起点和终点不自动相连；
- 设置 `Fill` 后，WPF 会在内部隐式连接起点和终点，形成闭合区域后计算填充；
- 顶点顺序直接决定折线形状，顺序错乱会出现交叉、回折等异常效果。

### 2. 完整渲染流程

`Polyline` 没有重写布局渲染方法，完全复用 `Shape` 基类的标准流水线：

1. **测量阶段**：基类读取 `DefiningGeometry` 的边界，结合线条厚度、拉伸模式，计算元素期望尺寸；
2. **排列阶段**：布局系统分配最终空间，基类计算缩放与偏移变换；
3. **渲染阶段**：
   - 应用几何变换得到最终渲染路径；
   - 若设置了 `Fill`，先按 `FillRule` 计算填充区域并绘制；
   - 再根据描边参数构造 `Pen`，绘制折线轮廓；
   - 全部通过 DirectX 硬件加速输出。

### 3. 精确命中测试

- 基于实际折线几何做命中测试，只有点击到线条实体（含厚度范围）才会触发鼠标事件；
- 折线外接矩形内的空白区域不会响应点击，交互精度远高于矩形控件；
- 如果需要扩大点击热区，可增加 `StrokeThickness` 并设置透明描边。

### 4. 与多个 Line 拼接的性能对比

| 对比项   | 单个 Polyline                 | 多个 Line 拼接           |
| :------- | :---------------------------- | :----------------------- |
| 渲染开销 | 单次绘制调用，性能高          | 多个元素多次绘制，开销大 |
| 拐角效果 | 统一拐角样式，过渡自然        | 线段衔接处有断点、毛刺   |
| 内存占用 | 单个 FrameworkElement，占用低 | 多个元素，占用高         |
| 维护成本 | 单个集合管理，简洁            | 多个对象逐个管理，繁琐   |

> 结论：连续多段线条场景，优先使用 `Polyline`，不要用多个 `Line` 拼接。

------

## 四、基础使用方法

### 1. XAML 基础用法

通过 `Points` 属性直接声明顶点序列，空格分隔每个顶点，逗号分隔 x、y 坐标。

xaml:

```xaml
<Canvas Width="300" Height="150" SnapsToDevicePixels="True">
    <!-- 基础折线：4个顶点，3段线 -->
    <Polyline Points="0,100 50,40 150,80 250,20"
              Stroke="RoyalBlue"
              StrokeThickness="2"
              StrokeLineJoin="Round"/>
</Canvas>
```

### 2. C# 后台动态创建

适合实时轨迹、动态曲线等场景，向 `Points` 集合追加数据即可自动刷新。

csharp:

```c#
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

// 创建折线实例
Polyline trackLine = new Polyline();
trackLine.Stroke = Brushes.Crimson;
trackLine.StrokeThickness = 1.5;
trackLine.StrokeLineJoin = PenLineJoin.Round;

// 添加顶点
trackLine.Points.Add(new Point(0, 100));
trackLine.Points.Add(new Point(50, 80));
trackLine.Points.Add(new Point(100, 120));
trackLine.Points.Add(new Point(200, 90));

// 添加到画布
canvas.Children.Add(trackLine);
```

### 3. 不同拉伸模式效果

xaml:

```xaml
<Grid Width="200" Height="100" ShowGridLines="True">
    <!-- Fill 模式：拉伸填满整个单元格，宽高比可能失真 -->
    <Polyline Points="0,0 50,30 100,0"
              Stretch="Fill"
              Stroke="Red" StrokeThickness="2"/>
</Grid>
```

> 工业高精度场景推荐使用 `Stretch="None"`，保证坐标比例准确。

------

## 五、工业上位机实战实例

### 案例 1：伺服轴运动轨迹

**场景**：记录并展示 X-Y 运动平台的实际行走轨迹，圆角拐角模拟真实运动平滑过渡。

xaml:

```xaml
<Canvas Width="500" Height="400" Background="#1E1E1E" SnapsToDevicePixels="True">
    <!-- 网格背景 -->
    <!-- 运动轨迹：绿色平滑折线 -->
    <Polyline Points="50,350 100,300 200,300 200,150 350,150 350,80 450,80"
              Stroke="LimeGreen"
              StrokeThickness="2"
              StrokeLineJoin="Round"
              StrokeStartLineCap="Round"
              StrokeEndLineCap="Round"/>
</Canvas>
```

> 关键优化：`StrokeLineJoin="Round"` 让拐角处平滑过渡，更贴合伺服轴加减速的真实运动轨迹。

### 案例 2：实时温度趋势曲线

**场景**：设备温度、压力等工艺参数的趋势图，动态追加数据点。

xaml:

```xaml
<Canvas Width="600" Height="200" Background="#F5F5F5">
    <!-- 坐标轴基线 -->
    <Line X1="50" Y1="180" X2="580" Y2="180" Stroke="Gray" StrokeThickness="1"/>
    <Line X1="50" Y1="20" X2="50" Y2="180" Stroke="Gray" StrokeThickness="1"/>

    <!-- 温度趋势曲线 -->
    <Polyline x:Name="TempTrendLine"
              Canvas.Left="50" Canvas.Top="20"
              Stroke="Crimson"
              StrokeThickness="1.5"
              StrokeLineJoin="Round"/>
</Canvas>
```

后台动态追加数据：

csharp:

```c#
// 定时采集温度，追加到折线
private void AddTemperaturePoint(double timeIndex, double temperature)
{
    // 坐标换算：温度映射到Y轴高度
    double y = 160 - (temperature - 20) * 2;
    TempTrendLine.Points.Add(new Point(timeIndex * 10, y));

    // 超过最大点数移除最早的点，实现滚动效果
    if (TempTrendLine.Points.Count > 50)
    {
        TempTrendLine.Points.RemoveAt(0);
    }
}
```

### 案例 3：规划路径虚线折线

**场景**：设备预设的运动路径、规划扫描路线，用虚线区分实际轨迹与规划轨迹。

xaml:

```
<Canvas Width="500" Height="300" Background="#F0F0F0">
    <!-- 规划路径：蓝色虚线 -->
    <Polyline Points="50,250 150,100 300,100 400,200 450,50"
              Stroke="RoyalBlue"
              StrokeThickness="1.5"
              StrokeDashArray="6,3"
              StrokeLineJoin="Round"
              StrokeDashCap="Round"/>

    <!-- 实际轨迹：绿色实线 -->
    <Polyline Points="50,250 145,105 298,102 405,195 448,55"
              Stroke="LimeGreen"
              StrokeThickness="2"
              StrokeLineJoin="Round"/>
</Canvas>
```

### 案例 4：不规则检测区域轮廓

**场景**：视觉检测中的不规则 ROI 区域轮廓，半透明填充高亮。

xaml:

```xaml
<Canvas Width="400" Height="300" Background="#222">
    <!-- 不规则检测区：折线自动闭合填充 -->
    <Polyline Points="80,200 120,100 250,80 320,150 300,220 150,240"
              Stroke="Cyan"
              StrokeThickness="2"
              Fill="#2200FFFF"
              StrokeLineJoin="Round"
              FillRule="Nonzero"/>
</Canvas>
```

> 注意：这里设置了 `Fill`，折线会自动首尾相连形成闭合区域填充；如果只需要轮廓线，不要设置 `Fill`。

### 案例 5：MVVM 数据绑定趋势图

**场景**：纯 MVVM 模式，视图模型提供点集合，UI 自动渲染，无后台操作 UI 代码。

#### 视图模型

csharp:

```c#
public class TrendViewModel : INotifyPropertyChanged
{
    public PointCollection DataPoints { get; set; } = new PointCollection();

    public void AddData(double x, double y)
    {
        DataPoints.Add(new Point(x, y));
        OnPropertyChanged(nameof(DataPoints));
    }
}
```

#### XAML 绑定

xaml:

```xaml
<Canvas Width="600" Height="200">
    <Polyline Points="{Binding DataPoints}"
              Stroke="RoyalBlue"
              StrokeThickness="1.5"
              StrokeLineJoin="Round"/>
</Canvas>
```

------

## 六、常见避坑与最佳实践

### 1. 高频坑：设置 Fill 后意外闭合填充

`Polyline` 视觉上是开放折线，但只要设置了 `Fill`，WPF 会**自动连接起点和终点形成闭合区域**并填充。如果只需要轮廓线，千万不要设置 `Fill`，或设为 `Transparent`。

### 2. 小角度拐角出现长尖刺

默认 `StrokeLineJoin="Miter"` 尖角模式下，小角度拐角会延伸出很长的尖刺。解决方案：

- 设置 `StrokeLineJoin="Round"` 或 `Bevel`，替换拐角样式；
- 调小 `StrokeMiterLimit` 值，限制尖角最大延伸长度。

### 3. 大量点高频刷新的性能优化

- 点数在 100 以内、刷新频率不高时，`Polyline` 性能足够；
- 点数上千、每秒多次刷新时，每次集合变更都会触发重绘，性能开销增大；
- 超高性能场景建议改用 `StreamGeometry` 直接绘制，内存和渲染开销更低。

### 4. 不要用多个 Line 拼接折线

连续多段线条优先用 `Polyline`：渲染次数更少、拐角更平滑、内存占用更低、代码更易维护。只有彼此独立的离散线段，才使用 `Line`。

### 5. 坐标精度与像素对齐

高精度绘图场景，父容器开启 `SnapsToDevicePixels="True"`，顶点尽量使用整数坐标，避免亚像素渲染导致的线条发虚、模糊。

### 6. 清空折线的正确方式

不要逐个移除点，直接调用 `Points.Clear()` 效率更高，清空后折线自动消失。

------

## 补充：Polyline 与 Polygon、Path 的区别

| 控件       | 闭合性                   | 核心用途                 | 灵活度                 |
| :--------- | :----------------------- | :----------------------- | :--------------------- |
| `Polyline` | 默认开放，填充时自动闭合 | 连续折线、轨迹、趋势曲线 | 中，仅支持直线段       |
| `Polygon`  | 强制闭合                 | 多边形区域、闭合轮廓     | 中，仅支持直线段       |
| `Path`     | 可开可合                 | 任意复杂图形、贝塞尔曲线 | 最高，支持所有几何类型 |

简单选型原则：连续直线轨迹用 `Polyline`，闭合多边形区域用 `Polygon`，复杂曲线 / 组合图形用 `Path`。