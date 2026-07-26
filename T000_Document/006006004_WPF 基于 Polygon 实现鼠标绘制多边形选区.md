# 006006004_WPF 基于 `Polygon` 实现鼠标绘制多边形选区

这是图像标注、ROI 选区绘制、地图选区等场景的经典交互功能，核心依托 `Polygon` **自动首尾闭合**的特性，配合 Canvas 画布、鼠标事件与橡皮筋预览线，实现流畅、专业的多边形绘制体验。

------

## 一、标准交互流程与实现原理

### 交互逻辑（业界通用方案）

1. **左键单击画布**：首次点击创建新多边形并添加第一个顶点；后续每次左键点击追加一个顶点
2. **鼠标移动**：实时显示「橡皮筋预览线」（从最后一个顶点延伸到当前鼠标位置），直观预览下一条边
3. **右键单击 / 双击左键**：结束绘制，多边形自动闭合，形成最终选区
4. 支持重复绘制，生成多个独立选区

### 核心实现原理

- **容器**：用 `Canvas` 作为绘制画布，绝对定位精准采集鼠标坐标
- **主体**：`Polygon` 承载最终选区，天生自动首尾闭合，无需手动补全起点
- **预览**：用一条临时 `Line` 做橡皮筋预览线，绘制过程中跟随鼠标，提升交互体验
- **驱动**：`PointCollection` 自带变更通知，顶点增删后 UI 自动刷新，无需手动重绘

------

## 二、完整可运行代码

### 1. XAML 界面布局

xaml:

```xaml
<Window x:Name="win" x:Class="PolygonSelectDemo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="多边形选区绘制" Height="600" Width="800"
        KeyDown="Win_KeyDown">
    <Grid>
        <!-- 绘制画布
             注意：必须设置Background（即使Transparent），否则无法命中测试，收不到鼠标事件 -->
        <Canvas x:Name="DrawCanvas" 
                Background="#1E1E1E" 
                SnapsToDevicePixels="True"
                MouseLeftButtonDown="DrawCanvas_MouseLeftButtonDown"
                MouseMove="DrawCanvas_MouseMove"
                MouseRightButtonDown="DrawCanvas_MouseRightButtonDown">
            
            <!-- 操作提示 -->
            <TextBlock Text="左键添加顶点 | 右键结束绘制 | Ctrl+Z 撤销上一步" 
                       Foreground="Gray" FontSize="12"
                       Canvas.Left="10" Canvas.Top="10"/>
        </Canvas>
    </Grid>
</Window>
```

> ⚠️ 关键坑点：`Canvas` 默认 `Background = null`，无法参与命中测试，鼠标事件不会触发。哪怕需要透明背景，也必须显式设置 `Background="Transparent"`。

### 2. C# 后台逻辑

csharp:

```c#
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PolygonSelectDemo
{
    public partial class MainWindow : Window
    {
        // 是否处于绘制中状态
        private bool _isDrawing;
        // 当前正在绘制的多边形
        private Polygon _currentPolygon;
        // 橡皮筋预览线（绘制过程中实时跟随鼠标）
        private Line _previewLine;

        // 选区样式常量（工业标注常用配色）
        private const double StrokeThicknessValue = 2;
        private readonly Color _strokeColor = Colors.Cyan;
        private readonly Color _fillColor = Color.FromArgb(0x33, 0x00, 0xFF, 0xFF);

        public MainWindow()
        {
            InitializeComponent();
        }

        #region 核心鼠标事件
        /// <summary>
        /// 左键按下：开始绘制 / 添加顶点
        /// </summary>
        private void DrawCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePos = e.GetPosition(DrawCanvas);

            // 未在绘制状态：开启新的多边形绘制
            if (!_isDrawing)
            {
                StartNewPolygon(mousePos);
                return;
            }

            // 绘制中：追加新顶点
            _currentPolygon.Points.Add(mousePos);
            // 同步更新预览线的起点，让预览线始终跟随最后一个顶点
            _previewLine.X1 = mousePos.X;
            _previewLine.Y1 = mousePos.Y;
        }

        /// <summary>
        /// 鼠标移动：更新橡皮筋预览线
        /// </summary>
        private void DrawCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDrawing || _previewLine == null) return;

            Point mousePos = e.GetPosition(DrawCanvas);
            // 实时更新预览线终点，实现橡皮筋拖拽效果
            _previewLine.X2 = mousePos.X;
            _previewLine.Y2 = mousePos.Y;
        }

        /// <summary>
        /// 右键按下：结束当前绘制，闭合多边形
        /// </summary>
        private void DrawCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isDrawing) return;

            // 1. 移除临时预览线
            DrawCanvas.Children.Remove(_previewLine);
            _previewLine = null;

            // 2. 校验：至少3个顶点才能构成有效多边形
            if (_currentPolygon.Points.Count < 3)
            {
                DrawCanvas.Children.Remove(_currentPolygon);
                MessageBox.Show("至少需要3个顶点才能构成有效多边形", "提示", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // 3. 重置绘制状态
            _isDrawing = false;
            _currentPolygon = null;
            DrawCanvas.ReleaseMouseCapture();
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 初始化一个新的多边形，开启绘制
        /// </summary>
        private void StartNewPolygon(Point startPoint)
        {
            _isDrawing = true;
            DrawCanvas.CaptureMouse(); // 捕获鼠标，移出画布仍能接收事件

            // 1. 创建最终多边形对象
            _currentPolygon = new Polygon
            {
                Stroke = new SolidColorBrush(_strokeColor),
                StrokeThickness = StrokeThicknessValue,
                Fill = new SolidColorBrush(_fillColor),
                StrokeLineJoin = PenLineJoin.Round, // 拐角平滑
                FillRule = FillRule.Nonzero // 自相交也实心填充
            };
            _currentPolygon.Points.Add(startPoint);
            DrawCanvas.Children.Add(_currentPolygon);

            // 2. 创建橡皮筋预览线（虚线样式，与最终实线做视觉区分）
            _previewLine = new Line
            {
                Stroke = new SolidColorBrush(_strokeColor),
                StrokeThickness = StrokeThicknessValue,
                StrokeDashArray = new DoubleCollection { 4, 2 },
                X1 = startPoint.X,
                Y1 = startPoint.Y,
                X2 = startPoint.X,
                Y2 = startPoint.Y
            };
            DrawCanvas.Children.Add(_previewLine);
        }
        #endregion

        #region 进阶：快捷键撤销
        /// <summary>
        /// Ctrl+Z 撤销上一个顶点
        /// </summary>
        private void Win_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control && _isDrawing)
            {
                if (_currentPolygon.Points.Count > 1)
                {
                    // 移除最后一个顶点
                    _currentPolygon.Points.RemoveAt(_currentPolygon.Points.Count - 1);
                    
                    // 更新预览线起点为当前最后一个顶点
                    Point lastPoint = _currentPolygon.Points.Last();
                    _previewLine.X1 = lastPoint.X;
                    _previewLine.Y1 = lastPoint.Y;
                }
            }
        }
        #endregion

        #region 业务对接：获取选区数据
        /// <summary>
        /// 获取所有已绘制选区的顶点坐标
        /// </summary>
        public Point[][] GetAllRegionPoints()
        {
            return DrawCanvas.Children
                .OfType<Polygon>()
                .Select(p => p.Points.ToArray())
                .ToArray();
        }

        /// <summary>
        /// 清空所有选区
        /// </summary>
        public void ClearAllRegions()
        {
            var allPolygons = DrawCanvas.Children.OfType<Polygon>().ToList();
            foreach (var poly in allPolygons)
            {
                DrawCanvas.Children.Remove(poly);
            }
        }
        #endregion
    }
}
```

------

## 三、核心逻辑详解

### 1. 绘制初始化 `StartNewPolygon`

- 创建 `Polygon` 实例，配置工业标注常用的青色半透明样式，拐角设为圆角更美观
- 添加第一个顶点，将多边形加入画布
- 创建虚线样式的预览线，用于鼠标移动时的橡皮筋效果，让用户直观看到下一条边的走向
- 调用 `CaptureMouse()` 捕获鼠标，避免鼠标移出画布后事件中断

### 2. 顶点追加逻辑

- 首次左键点击触发绘制初始化
- 绘制过程中每次左键点击，向 `Points` 集合追加一个顶点
- 得益于 `PointCollection` 的变更通知机制，新增顶点后界面自动刷新，无需手动重绘
- 同步更新预览线起点，让预览线始终锚定在最后一个顶点上

### 3. 橡皮筋预览效果

- 仅在绘制状态下生效，鼠标移动时实时更新预览线终点
- 预览线使用虚线样式，与最终的实线描边做视觉区分，避免用户混淆

### 4. 结束绘制逻辑

- 移除临时预览线，清理临时资源
- 顶点数量校验：少于 3 个点无法构成有效多边形，直接移除
- **`Polygon` 会自动首尾闭合**，不需要手动把最后一个点设为与起点相同，这是用 Polygon 做选区的核心优势
- 释放鼠标捕获，重置绘制状态

------

## 四、常用高级功能扩展

### 1. 顶点拖拽编辑（专业标注工具必备）

绘制完成后，可在每个顶点位置放置一个小圆形 `Thumb` 控件，拖拽 Thumb 时实时更新对应顶点坐标，实现选区微调：

- 遍历多边形顶点，为每个顶点生成一个可拖拽的圆点
- 注册 Thumb 的 `DragDelta` 事件，同步更新 Polygon 对应索引的顶点坐标

### 2. 双击结束绘制

如果更习惯双击结束，可将结束逻辑绑定到 `MouseDoubleClick` 事件，与右键二选一即可。

### 3. 选区选中与删除

给每个 Polygon 注册鼠标点击事件，点击后高亮边框（比如加粗、变色），按 Delete 键删除当前选中的选区。

------

## 五、避坑与最佳实践

1. **Canvas 背景必须设置**

   这是新手最高频踩坑点。`Canvas` 默认无背景，无法命中测试，鼠标事件完全不触发。透明背景请显式写 `Background="Transparent"`。

2. **不要手动重复起点**

   `Polygon` 天生自动闭合，手动把最后一个点设为起点会导致重叠边，影响拐角渲染和填充计算。

3. **自相交多边形填充问题**

   如果绘制了交叉的多边形，默认 `EvenOdd` 规则会出现镂空。需要实心填充请显式设置 `FillRule="Nonzero"`。

4. **鼠标捕获的必要性**

   `CaptureMouse()` 能保证鼠标移出画布区域后仍能接收事件，避免绘制意外中断；结束绘制时必须调用 `ReleaseMouseCapture()` 释放。

5. **性能优化建议**

   - 单个多边形几十顶点性能优异；顶点数量极多时，建议改用 `StreamGeometry` 低层绘制
   - 静态画刷调用 `Freeze()` 冻结，可减少内存占用、提升渲染性能
   - 大量选区场景可考虑 UI 虚拟化，避免同时存在上百个 Polygon 元素

6. **像素对齐**

   画布开启 `SnapsToDevicePixels="True"`，避免线条发虚、模糊，保证标注精度。