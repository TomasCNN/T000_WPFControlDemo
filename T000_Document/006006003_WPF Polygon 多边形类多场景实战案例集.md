# 006006003_WPF `Polygon` 多边形类多场景实战案例集

以下案例覆盖基础绘图、工业检测、数据可视化、交互编辑、属性对比等高频场景，所有代码可直接复制复用，兼顾入门理解与实战落地。

------

## 一、基础绘制案例（入门必看）

### 1. 基础闭合多边形（三角形 / 五边形）

最基础的用法，通过 `Points` 顶点集合定义形状，**自动首尾闭合**，无需手动重复起点。

xaml:

```xaml
<StackPanel Orientation="Horizontal" Margin="20" Spacing="40">
    <!-- 正三角形：3个顶点自动形成3条边 -->
    <Canvas Width="150" Height="150">
        <Polygon Points="75,10 10,140 140,140"
                 Stroke="DarkBlue"
                 StrokeThickness="2"
                 Fill="#334169E1"
                 StrokeLineJoin="Round"/>
    </Canvas>

    <!-- 正五边形：5个顶点自动闭合 -->
    <Canvas Width="150" Height="150">
        <Polygon Points="75,5 145,55 120,145 30,145 5,55"
                 Stroke="DarkGreen"
                 StrokeThickness="2"
                 Fill="#3332CD32"
                 StrokeLineJoin="Round"/>
    </Canvas>
</StackPanel>
```

> 核心特性：`Polygon` 会自动将最后一个顶点与第一个顶点相连，始终形成闭合轮廓，无需手动添加重复的起点。

### 2. 虚线边框多边形

常用于规划区域、临时选区、待确认范围等场景，通过 `StrokeDashArray` 实现虚线轮廓。

xaml:

```xaml
<Canvas Width="300" Height="200" Background="#F5F5F5" SnapsToDevicePixels="True">
    <!-- 规划选区：虚线边框 + 半透明填充 -->
    <Polygon Points="50,150 100,50 220,30 280,120 200,180 80,170"
             Stroke="#FF8C00"
             StrokeThickness="2"
             StrokeDashArray="6,3"
             Fill="#33FF8C00"
             StrokeLineJoin="Round"/>
</Canvas>
```

------

## 二、工业 / 业务场景实战案例

### 案例 1：视觉检测不规则 ROI 区域

**场景**：AOI 视觉检测中，贴合产品 / 工件轮廓的异形感兴趣区域，比矩形 ROI 更精准，减少无效检测范围。

xaml:

```xaml
<Canvas Width="600" Height="400" Background="#1E1E1E" SnapsToDevicePixels="True">
    <!-- 异形检测区：贴合工件轮廓的闭合多边形 -->
    <Polygon Points="120,300 180,120 320,100 420,180 450,320 300,350 150,340"
             Stroke="Cyan"
             StrokeThickness="2"
             Fill="#2200FFFF"
             StrokeLineJoin="Round"
             FillRule="Nonzero"/>
    
    <!-- 精细检测子区域 -->
    <Polygon Points="250,180 320,160 340,220 270,240"
             Stroke="Yellow"
             StrokeThickness="1.5"
             StrokeDashArray="4,2"
             Fill="#33FFFF00"/>
</Canvas>
```

> 工业价值：可精确匹配不规则工件边缘，大幅缩小检测范围，提升算法运行效率。

### 案例 2：产线工位功能分区

**场景**：产线布局图中的功能分区，用不同颜色区分上料区、检测区、下料区、缓存区。

xaml:

```xaml
<Canvas Width="500" Height="300" Background="#F8F9FA">
    <!-- 上料区：蓝色 -->
    <Polygon Points="0,0 150,0 150,300 0,300"
             Stroke="#1E90FF" StrokeThickness="1.5"
             Fill="#221E90FF"/>
    <TextBlock Text="上料区" Canvas.Left="50" Canvas.Top="140" Foreground="#1E90FF" FontWeight="Bold"/>

    <!-- 检测区：绿色 -->
    <Polygon Points="150,0 400,0 400,200 150,200"
             Stroke="#32CD32" StrokeThickness="1.5"
             Fill="#2232CD32"/>
    <TextBlock Text="检测区" Canvas.Left="250" Canvas.Top="90" Foreground="#32CD32" FontWeight="Bold"/>

    <!-- 下料区：橙色 -->
    <Polygon Points="150,200 400,200 400,300 150,300"
             Stroke="#FF8C00" StrokeThickness="1.5"
             Fill="#22FF8C00"/>
    <TextBlock Text="下料区" Canvas.Left="250" Canvas.Top="240" Foreground="#FF8C00" FontWeight="Bold"/>

    <!-- 缓存区：灰色 -->
    <Polygon Points="400,0 500,0 500,300 400,300"
             Stroke="#888" StrokeThickness="1.5"
             Fill="#22888888"/>
    <TextBlock Text="缓存区" Canvas.Left="420" Canvas.Top="140" Foreground="#666" FontWeight="Bold"/>
</Canvas>
```

### 案例 3：自定义方向箭头标

**场景**：物料流向指示、工序流转箭头，用多边形实现实心箭头，比 Line+Polygon 拼接更简洁。

xaml:

```xaml
<Canvas Width="200" Height="100">
    <!-- 水平向右实心箭头 -->
    <Polygon Points="0,35 120,35 120,15 180,50 120,85 120,65 0,65"
             Fill="#32CD32"
             Stroke="#228B22"
             StrokeThickness="1"
             StrokeLineJoin="Round"/>
</Canvas>
```

------

## 三、数据可视化案例

### 案例 1：雷达图（蜘蛛图）

**场景**：多维度能力评估、指标对比，纯 Polygon 实现，无需引入第三方图表库。

xaml:

```xaml
<Canvas Width="300" Height="300" Background="White">
    <!-- 背景网格：三层五边形 -->
    <Polygon Points="150,30 265,115 220,260 80,260 35,115"
             Stroke="#E0E0E0" StrokeThickness="1" Fill="Transparent"/>
    <Polygon Points="150,70 225,125 195,220 105,220 75,125"
             Stroke="#E0E0E0" StrokeThickness="1" Fill="Transparent"/>
    <Polygon Points="150,110 185,135 170,180 130,180 115,135"
             Stroke="#E0E0E0" StrokeThickness="1" Fill="Transparent"/>
    
    <!-- 数据层：半透明填充 + 实线描边 -->
    <Polygon Points="150,50 240,130 200,230 90,210 50,120"
             Stroke="#409EFF" StrokeThickness="2"
             Fill="#66409EFF"
             StrokeLineJoin="Round"/>
</Canvas>
```

> 扩展方式：叠加多个 Polygon 可实现多组数据对比，配合数据绑定可动态更新指标。

### 案例 2：填充面积对比图

**场景**：两组数据的面积对比，利用闭合填充直观展示数据差异。

xaml:

```xaml
<Canvas Width="500" Height="250" Background="#F8F9FA" SnapsToDevicePixels="True">
    <Line X1="40" Y1="200" X2="480" Y2="200" Stroke="#DDD" StrokeThickness="1"/>
    
    <!-- 系列A：蓝色面积 -->
    <Polygon Points="40,180 110,140 180,160 250,90 320,110 390,60 460,90 460,200 40,200"
             Fill="#33409EFF" Stroke="#409EFF" StrokeThickness="1.5"
             StrokeLineJoin="Round"/>
    
    <!-- 系列B：橙色面积 -->
    <Polygon Points="40,150 110,160 180,110 250,130 320,90 390,110 460,70 460,200 40,200"
             Fill="#33FF8C00" Stroke="#FF8C00" StrokeThickness="1.5"
             StrokeLineJoin="Round"/>
</Canvas>
```

------

## 四、交互编辑案例

### 案例：鼠标手绘多边形选区（图像标注工具）

**场景**：数据标注平台、影像阅片工具中的手绘多边形选区，点击添加顶点，双击完成闭合。

#### XAML 界面

xml:

```xaml
<Canvas x:Name="drawCanvas" 
        Width="600" Height="400" 
        Background="#F5F5F5"
        MouseLeftButtonDown="DrawCanvas_MouseLeftButtonDown"
        MouseMove="DrawCanvas_MouseMove"
        MouseDoubleClick="DrawCanvas_MouseDoubleClick"/>
```

#### 后台逻辑

csharp:

```c#
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

public partial class MainWindow : Window
{
    private Polygon _currentPolygon;
    private bool _isDrawing;

    private void DrawCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Point clickPoint = e.GetPosition(drawCanvas);

        // 首次点击：创建新多边形
        if (!_isDrawing)
        {
            _isDrawing = true;
            _currentPolygon = new Polygon
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0x00, 0x00)),
                StrokeLineJoin = PenLineJoin.Round
            };
            drawCanvas.Children.Add(_currentPolygon);
        }

        // 添加顶点
        _currentPolygon.Points.Add(clickPoint);
    }

    private void DrawCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        // 绘制过程中可扩展预览逻辑（实时显示待定点连线）
    }

    private void DrawCanvas_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // 双击结束绘制，多边形自动闭合
        _isDrawing = false;
        _currentPolygon = null;
    }
}
```

------

## 五、核心属性效果对比案例

### 案例 1：FillRule 填充规则对比

直观展示 `EvenOdd`（奇偶环绕）与 `Nonzero`（非零环绕）的差异，自相交图形效果最明显。

xaml:

```xaml
<StackPanel Orientation="Horizontal" Margin="30" Spacing="60">
    <!-- EvenOdd：交叉区域镂空 -->
    <Canvas Width="200" Height="220">
        <Polygon Points="100,20 180,180 20,180 100,60"
                 Stroke="Black" StrokeThickness="2"
                 Fill="#888888"
                 FillRule="EvenOdd"/>
        <TextBlock Text="EvenOdd 奇偶规则" Canvas.Top="195" 
                   FontSize="12" HorizontalAlignment="Center" Width="200" TextAlignment="Center"/>
    </Canvas>

    <!-- Nonzero：全部实心填充 -->
    <Canvas Width="200" Height="220">
        <Polygon Points="100,20 180,180 20,180 100,60"
                 Stroke="Black" StrokeThickness="2"
                 Fill="#888888"
                 FillRule="Nonzero"/>
        <TextBlock Text="Nonzero 非零规则" Canvas.Top="195" 
                   FontSize="12" HorizontalAlignment="Center" Width="200" TextAlignment="Center"/>
    </Canvas>
</StackPanel>
```

### 案例 2：StrokeLineJoin 拐角样式对比

展示三种拐角连接方式的视觉差异，是多边形美化的核心调优点。

xaml:

```xaml
<StackPanel Orientation="Horizontal" Margin="30" Spacing="40">
    <!-- Miter：尖角（默认），小角度易出现长尖刺 -->
    <Canvas Width="120" Height="120">
        <Polygon Points="10,100 60,20 110,100"
                 Stroke="DarkBlue" StrokeThickness="8"
                 Fill="Transparent"
                 StrokeLineJoin="Miter"/>
        <TextBlock Text="Miter 尖角" Canvas.Top="100" FontSize="12"/>
    </Canvas>

    <!-- Bevel：斜切平角 -->
    <Canvas Width="120" Height="120">
        <Polygon Points="10,100 60,20 110,100"
                 Stroke="DarkBlue" StrokeThickness="8"
                 Fill="Transparent"
                 StrokeLineJoin="Bevel"/>
        <TextBlock Text="Bevel 斜切" Canvas.Top="100" FontSize="12"/>
    </Canvas>

    <!-- Round：圆角平滑，工业场景推荐 -->
    <Canvas Width="120" Height="120">
        <Polygon Points="10,100 60,20 110,100"
                 Stroke="DarkBlue" StrokeThickness="8"
                 Fill="Transparent"
                 StrokeLineJoin="Round"/>
        <TextBlock Text="Round 圆角" Canvas.Top="100" FontSize="12"/>
    </Canvas>
</StackPanel>
```

------

## 六、MVVM 数据驱动案例

**场景**：纯数据驱动多边形形状，视图模型提供顶点集合，UI 自动渲染，符合 MVVM 架构规范。

### 视图模型

csharp:

```c#
public class RegionViewModel : INotifyPropertyChanged
{
    private PointCollection _regionPoints;
    public PointCollection RegionPoints
    {
        get => _regionPoints;
        set { _regionPoints = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
```

### XAML 绑定

xaml:

```xaml
<Canvas Width="400" Height="300">
    <Polygon Points="{Binding RegionPoints}"
             Stroke="RoyalBlue"
             StrokeThickness="2"
             Fill="#334169E1"
             StrokeLineJoin="Round"/>
</Canvas>
```

------

## 七、常见避坑提示

1. **不要手动重复起点**：`Polygon` 自动首尾闭合，重复添加起点会导致重叠边，影响拐角渲染和填充计算。
2. **自相交图形注意填充规则**：复杂交叉多边形默认 `EvenOdd` 会出现镂空，需要实心填充请设置 `FillRule="Nonzero"`。
3. **小角度拐角尖刺问题**：默认尖角模式下小角度会出现长尖刺，设置 `StrokeLineJoin="Round"` 或调小 `StrokeMiterLimit` 即可解决。
4. **内部点击无响应**：只设置描边不设置填充时，内部空白不参与命中测试；需要全区域响应请设置 `Fill="Transparent"`。
5. **性能优化**：顶点数量少的静态多边形性能优异；上百个顶点高频刷新时，建议改用 `StreamGeometry` 低层绘制，性能更优。