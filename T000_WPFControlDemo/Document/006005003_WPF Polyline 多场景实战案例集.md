# 006005003_WPF `Polyline` 多场景实战案例集

以下案例覆盖数据可视化、交互绘图、动画特效、教学工具等多个领域，从基础用法到进阶交互均有涉及，可直接复制复用。

------

## 一、基础展示类（入门必看）

### 1. 基础数据折线图（BI 报表场景）

**场景**：销售趋势、指标变化等基础报表可视化，是各类数据面板最常用的折线呈现形式。

xaml:

```xaml
<Canvas Width="400" Height="250" Background="#F8F9FA" SnapsToDevicePixels="True">
    <!-- 纵轴、横轴基线 -->
    <Line X1="50" Y1="20" X2="50" Y2="200" Stroke="#CCCCCC" StrokeThickness="1"/>
    <Line X1="50" Y1="200" X2="380" Y2="200" Stroke="#CCCCCC" StrokeThickness="1"/>
    
    <!-- 月度销售额折线 -->
    <Polyline Canvas.Left="50" Canvas.Top="20"
              Points="0,120 50,90 100,110 150,60 200,80 250,40 300,70"
              Stroke="#2E8B57"
              StrokeThickness="2"
              StrokeLineJoin="Round"/>
</Canvas>
```

**关键要点**：

- 配合坐标轴使用，是所有折线图的基础结构；
- `StrokeLineJoin="Round"` 让数据拐点平滑过渡，视觉更连贯。

### 2. 虚线规划路径（导航 / 游戏场景）

**场景**：地图导航预设路线、游戏敌人行进路径，用虚线区分「规划路线」与「已执行轨迹」。

xaml:

```xaml
<Canvas Width="500" Height="300" Background="#F0F2F5">
    <!-- 规划路线：蓝色虚线 -->
    <Polyline Points="50,250 150,100 300,100 400,200 450,80"
              Stroke="#1E90FF"
              StrokeThickness="2"
              StrokeDashArray="8,4"
              StrokeLineJoin="Round"
              StrokeDashCap="Round"/>
    
    <!-- 已行进轨迹：绿色实线 -->
    <Polyline Points="50,250 150,100 220,100"
              Stroke="#32CD32"
              StrokeThickness="2.5"
              StrokeLineJoin="Round"
              StrokeStartLineCap="Round"/>
</Canvas>
```

### 3. 拐角样式对比

直观展示不同拐角模式的效果差异，是折线美化的核心调优点。

xaml:

```xaml
<Canvas Width="400" Height="300" Background="#1E1E1E">
    <!-- 默认尖角（Miter）：拐点尖锐，小角度易出现长尖刺 -->
    <Polyline Points="50,200 100,80 200,150 300,60 350,180"
              Stroke="#666"
              StrokeThickness="3"
              StrokeLineJoin="Miter"/>
    
    <!-- 圆角（Round）：拐点平滑，适合运动轨迹、曲线类场景 -->
    <Polyline Points="50,250 100,130 200,200 300,110 350,230"
              Stroke="#00CED1"
              StrokeThickness="3"
              StrokeLineJoin="Round"/>
</Canvas>
```

------

## 二、数据可视化类（BI / 监控场景）

### 1. 实时滚动趋势曲线（系统监控 / 数据大盘）

**场景**：服务器 CPU、设备参数等实时滚动刷新的监控曲线，新数据从右侧进入，旧数据从左侧移出。

#### XAML 界面

xaml:

```xaml
<Canvas x:Name="chartCanvas" Width="600" Height="200" Background="#1E1E1E">
    <Polyline x:Name="trendLine"
              Stroke="#00FF7F"
              StrokeThickness="1.5"
              StrokeLineJoin="Round"/>
</Canvas>
```

#### 后台逻辑（C#）

csharp:

```c#
using System.Windows.Threading;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
    private int _timeIndex = 0;
    private const int MaxPointCount = 60; // 最多保留60个数据点
    private readonly Random _random = new Random();

    public MainWindow()
    {
        InitializeComponent();
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500) // 500ms刷新一次
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        // 模拟生成波动数值
        double y = 100 + Math.Sin(_timeIndex * 0.3) * 50 + _random.NextDouble() * 20;
        
        // 尾部追加新点
        trendLine.Points.Add(new Point(_timeIndex * 8, y));
        _timeIndex++;

        // 超过容量时移除头部旧点，整体左移实现滚动
        if (trendLine.Points.Count > MaxPointCount)
        {
            trendLine.Points.RemoveAt(0);
            for (int i = 0; i < trendLine.Points.Count; i++)
            {
                trendLine.Points[i] = new Point(i * 8, trendLine.Points[i].Y);
            }
        }
    }
}
```

### 2. 面积趋势图（财务 / 数据报表）

利用 Polyline 设置 `Fill` 后自动闭合填充的特性，快速实现面积图，展示数据累计效果。

xaml:

```xaml
<Canvas Width="500" Height="250" Background="White" SnapsToDevicePixels="True">
    <Line X1="40" Y1="200" X2="480" Y2="200" Stroke="#DDD" StrokeThickness="1"/>
    
    <!-- 半透明填充 + 实线描边 -->
    <Polyline Canvas.Left="40" Canvas.Top="20"
              Points="0,150 60,120 120,130 180,70 240,90 300,50 360,80 420,60"
              Stroke="#409EFF"
              StrokeThickness="2"
              Fill="#33409EFF"
              StrokeLineJoin="Round"
              FillRule="Nonzero"/>
</Canvas>
```

> 注意：设置 `Fill` 后，折线会隐式连接起点与终点形成闭合区域并填充；只需要轮廓线时请勿设置该属性。

### 3. 多系列对比折线图

同时展示多组数据对比，如不同产品线销售趋势、多指标并行监控。

xaml:

```xaml
<Canvas Width="500" Height="250" Background="#F8F9FA">
    <!-- 产品线A：蓝色 -->
    <Polyline Points="50,180 120,140 190,160 260,100 330,120 400,80"
              Stroke="#1E90FF" StrokeThickness="2" StrokeLineJoin="Round"/>
    <!-- 产品线B：橙色 -->
    <Polyline Points="50,150 120,160 190,110 260,130 330,90 400,110"
              Stroke="#FF8C00" StrokeThickness="2" StrokeLineJoin="Round"/>
    <!-- 产品线C：绿色 -->
    <Polyline Points="50,100 120,90 190,120 260,70 330,60 400,50"
              Stroke="#32CD32" StrokeThickness="2" StrokeLineJoin="Round"/>
</Canvas>
```

------

## 三、交互编辑类（标注 / 绘图工具场景）

### 鼠标手绘折线（图像标注工具）

**场景**：数据标注平台、医学影像阅片工具中的手绘折线标注，按住鼠标绘制，松开结束。

#### XAML 界面

xaml:

```xaml
<Canvas x:Name="drawCanvas" 
        Width="600" Height="400" 
        Background="#F5F5F5"
        MouseLeftButtonDown="DrawCanvas_MouseLeftButtonDown"
        MouseMove="DrawCanvas_MouseMove"
        MouseLeftButtonUp="DrawCanvas_MouseLeftButtonUp"/>
```

#### 后台逻辑

csharp:

```c#
public partial class MainWindow : Window
{
    private Polyline _currentPolyline;
    private bool _isDrawing;

    private void DrawCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isDrawing = true;
        drawCanvas.CaptureMouse();
        
        // 创建新折线，配置平滑笔触
        _currentPolyline = new Polyline
        {
            Stroke = Brushes.Red,
            StrokeThickness = 2,
            StrokeLineJoin = PenLineJoin.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };
        
        Point startPoint = e.GetPosition(drawCanvas);
        _currentPolyline.Points.Add(startPoint);
        drawCanvas.Children.Add(_currentPolyline);
    }

    private void DrawCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDrawing || _currentPolyline == null) return;
        
        Point currentPoint = e.GetPosition(drawCanvas);
        _currentPolyline.Points.Add(currentPoint);
    }

    private void DrawCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isDrawing = false;
        drawCanvas.ReleaseMouseCapture();
        _currentPolyline = null;
    }
}
```

------

## 四、动画与特效类

### 1. 流动虚线效果（进度 / 传送带动效）

**场景**：传送带方向指示、扫描进度、流程走向，通过虚线偏移动画实现单向流动效果。

xaml:

```xaml
<Canvas Width="400" Height="100" Background="#222">
    <Polyline Points="30,50 150,50 250,20 370,20"
              Stroke="#00FF7F"
              StrokeThickness="3"
              StrokeDashArray="10,5"
              StrokeLineJoin="Round"
              StrokeDashCap="Round">
        <Polyline.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard RepeatBehavior="Forever">
                        <DoubleAnimation 
                            Storyboard.TargetProperty="StrokeDashOffset"
                            From="0" To="-15" Duration="0:0:0.8"/>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Polyline.Triggers>
    </Polyline>
</Canvas>
```

### 2. 沿折线路径运动动画（轨迹回放）

**场景**：物流轨迹回放、游戏角色沿路径移动，让 UI 元素沿着折线的路径运动。

xaml:

```xaml
<Canvas Width="500" Height="300" Background="#F0F2F5">
    <!-- 参考路径 -->
    <Polyline x:Name="movePath"
              Points="50,250 150,100 300,100 400,200 450,80"
              Stroke="#BBB"
              StrokeThickness="2"
              StrokeDashArray="6,3"/>
    
    <!-- 沿路径运动的小球 -->
    <Ellipse x:Name="moveBall" Width="16" Height="16" Fill="Red" Margin="-8,-8,0,0">
        <Ellipse.RenderTransform>
            <MatrixTransform x:Name="ballTransform"/>
        </Ellipse.RenderTransform>
        <Ellipse.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard RepeatBehavior="Forever">
                        <MatrixAnimationUsingPath
                            Storyboard.TargetName="ballTransform"
                            Storyboard.TargetProperty="Matrix"
                            Duration="0:0:5"
                            PathGeometry="{Binding ElementName=movePath, Path=RenderedGeometry}"/>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Ellipse.Triggers>
    </Ellipse>
</Canvas>
```

------

## 五、综合应用案例

### 1. 数学函数曲线绘制（教学演示）

**场景**：数学教学软件中动态生成函数曲线，修改参数实时更新图形。

csharp:

```c#
private void DrawSineCurve()
{
    Polyline sineLine = new Polyline
    {
        Stroke = Brushes.DarkBlue,
        StrokeThickness = 1.5,
        StrokeLineJoin = PenLineJoin.Round
    };

    // 生成正弦曲线坐标点
    for (double x = 0; x <= 360; x += 2)
    {
        double rad = x * Math.PI / 180;
        double y = Math.Sin(rad) * 80 + 100; // 振幅80，Y轴居中
        sineLine.Points.Add(new Point(x, y));
    }

    chartCanvas.Children.Add(sineLine);
}
```

### 2. 简易手写签名板

**场景**：办公审批、电子合同类软件的手写签名功能，基于 Polyline 实现自然笔迹。

核心逻辑与手绘折线一致，优化点：

- 设置 `StrokeLineJoin="Round"`、`StrokeStartLineCap="Round"` 让笔迹更平滑；
- 配合压感设备可动态调整 `StrokeThickness`，实现真实笔触效果。

------

## 使用注意事项

1. **填充意外闭合**：只需要轮廓线时，不要设置 `Fill` 属性，否则会自动首尾闭合填充；
2. **拐角尖刺问题**：小角度拐角出现长尖刺时，设置 `StrokeLineJoin="Round"` 或调小 `StrokeMiterLimit`；
3. **性能优化**：大量点高频刷新时，尽量批量更新而非逐个追加，减少重绘次数；超高性能场景可改用 `StreamGeometry`；
4. **坐标精度**：高精度场景开启 `SnapsToDevicePixels="True"`，避免亚像素渲染导致线条发虚模糊。

本回答由AI生成，仅供参考，请仔细甄别，谨慎投资。