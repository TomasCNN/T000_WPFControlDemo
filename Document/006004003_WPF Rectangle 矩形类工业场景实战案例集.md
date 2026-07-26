# 006004003_WPF `Rectangle` 矩形类工业场景实战案例集

以下案例全部面向工业上位机、视觉检测、工位可视化等真实工控场景设计，覆盖基础绘制、检测 ROI、状态卡片、交互动画、批量渲染、数据绑定等高频需求，可直接复制复用。

------

## 一、基础绘制案例（入门必看）

### 1. 直角矩形 + 圆角矩形

最基础的用法，通过 `RadiusX`/`RadiusY` 控制圆角，`Fill`/`Stroke` 控制外观。

xaml:

```xaml
<StackPanel Orientation="Horizontal" Margin="20" Spacing="30">
    <!-- 直角矩形：红色边框，无填充，用于精准检测框 -->
    <Rectangle Width="120" Height="80"
               Stroke="Red" StrokeThickness="2"/>

    <!-- 圆角矩形：半透明填充，圆角半径8，用于工位卡片、按钮背景 -->
    <Rectangle Width="120" Height="80"
               RadiusX="8" RadiusY="8"
               Fill="#330066FF"
               Stroke="#FF0066FF"
               StrokeThickness="1.5"/>
</StackPanel>
```

> 工业场景：直角矩形用于检测 ROI、缺陷外框等需要精准边界的场景；圆角矩形用于 UI 面板、状态卡片，视觉更柔和。

### 2. 虚线检测框

通过 `StrokeDashArray` 实现虚线边框，是 AOI 检测中区分不同类型 ROI 的标准方式。

xaml:

```xaml
<Canvas Width="400" Height="300" Background="#F5F5F5" SnapsToDevicePixels="True">
    <!-- 实线主检测框 -->
    <Rectangle Canvas.Left="80" Canvas.Top="60"
               Width="200" Height="150"
               Stroke="LimeGreen" StrokeThickness="2"
               Fill="#2200FF00"/>

    <!-- 虚线次检测框 -->
    <Rectangle Canvas.Left="300" Canvas.Top="100"
               Width="80" Height="60"
               Stroke="RoyalBlue" StrokeThickness="1.5"
               StrokeDashArray="6,3"
               Fill="#224169E1"/>
</Canvas>
```

> 关键参数：`StrokeDashArray="6,3"` 表示 6 像素实线 + 3 像素空白，循环重复；`SnapsToDevicePixels="True"` 避免边框发虚。

------

## 二、工业核心场景案例

### 案例 1：AOI 多层 ROI 检测区域

**场景**：视觉检测界面的多区域感兴趣标记，半透明填充既高亮又不遮挡下方图像。

xaml:

```xaml
<Canvas Width="800" Height="600" Background="#1E1E1E" SnapsToDevicePixels="True">
    <!-- 全局视野框：最外层虚线 -->
    <Rectangle Canvas.Left="50" Canvas.Top="50"
               Width="700" Height="500"
               Stroke="Gray" StrokeThickness="1"
               StrokeDashArray="4,2"
               Fill="Transparent"/>

    <!-- 主检测区：绿色实线 + 半透明填充 -->
    <Rectangle Canvas.Left="150" Canvas.Top="120"
               Width="400" Height="300"
               Stroke="LimeGreen" StrokeThickness="2"
               Fill="#2200FF00"/>

    <!-- 精细检测区：黄色虚线 + 高透明度填充 -->
    <Rectangle Canvas.Left="280" Canvas.Top="220"
               Width="120" Height="80"
               Stroke="Yellow" StrokeThickness="1.5"
               StrokeDashArray="5,2.5"
               Fill="#33FFFF00"/>
</Canvas>
```

> 工业价值：通过不同线型、颜色、透明度区分检测优先级，操作人员一眼即可识别重点区域。

### 案例 2：工位状态卡片

**场景**：产线工位状态展示面板，圆角矩形做背景，配合渐变填充区分运行 / 待机 / 故障状态。

xaml:

```xaml
<Grid Width="200" Height="110" Margin="10">
    <!-- 卡片背景：圆角矩形，运行状态绿色渐变 -->
    <Rectangle RadiusX="8" RadiusY="8"
               Stroke="#FF008800" StrokeThickness="1">
        <Rectangle.Fill>
            <LinearGradientBrush StartPoint="0,0" EndPoint="0,1">
                <GradientStop Color="#FFEEFFEE" Offset="0"/>
                <GradientStop Color="#FFCCEECC" Offset="1"/>
            </LinearGradientBrush>
        </Rectangle.Fill>
    </Rectangle>
    
    <!-- 卡片内容 -->
    <StackPanel Margin="15">
        <TextBlock Text="上料工位" FontSize="15" FontWeight="Bold" Foreground="#FF222222"/>
        <TextBlock Text="运行中" Foreground="Green" Margin="0,10,0,0" FontSize="13"/>
        <TextBlock Text="产量：1256" Foreground="#FF666666" Margin="0,5,0,0" FontSize="12"/>
    </StackPanel>
</Grid>
```

> MVVM 适配：将 `Fill` 和 `Stroke` 绑定到视图模型的 `StationState` 枚举，配合 `IValueConverter` 自动切换颜色，无需后台操作 UI。

### 案例 3：缺陷选中标注框

**场景**：点击缺陷后显示选中效果，双边框 + 半透明填充，醒目且不遮挡图像细节。

xaml:

```xaml
<Canvas Width="300" Height="200" Background="#222">
    <!-- 内层实线主框 -->
    <Rectangle Canvas.Left="100" Canvas.Top="70"
               Width="80" Height="50"
               Stroke="Yellow" StrokeThickness="2"
               Fill="#33FFFF00"/>
    <!-- 外层虚线装饰框 -->
    <Rectangle Canvas.Left="95" Canvas.Top="65"
               Width="90" Height="60"
               Stroke="Yellow" StrokeThickness="1"
               StrokeDashArray="4,2"
               Fill="Transparent"/>
</Canvas>
```

### 案例 4：液位 / 物料余量进度指示

**场景**：储料罐液位、物料余量、任务进度的可视化，通过高度绑定动态展示比例。

xaml:

```xaml
<Grid Width="50" Height="200" Margin="20">
    <!-- 背景槽 -->
    <Rectangle Stroke="Gray" StrokeThickness="1" Fill="#FF333333" RadiusX="3" RadiusY="3"/>
    <!-- 液位填充：底部对齐，高度绑定进度百分比 -->
    <Rectangle VerticalAlignment="Bottom"
               Height="{Binding FillPercent, Converter={StaticResource PercentToHeightConverter}}"
               Fill="RoyalBlue"
               RadiusX="2" RadiusY="2"/>
    <!-- 刻度边框 -->
    <Rectangle Stroke="LightGray" StrokeThickness="1" Fill="Transparent" RadiusX="3" RadiusY="3"/>
</Grid>
```

### 案例 5：控件虚线边框

**场景**：输入框、显示区域的虚线边框，用于标记待配置、选中态、告警状态。

xaml:

```xaml
<Grid Width="200" Height="35" Margin="10">
    <Rectangle Stroke="Orange" StrokeThickness="1"
               StrokeDashArray="4,2"
               Fill="Transparent"
               RadiusX="4" RadiusY="4"/>
    <TextBlock Text="待配置参数" Foreground="Orange" 
               HorizontalAlignment="Center" VerticalAlignment="Center"/>
</Grid>
```

------

## 三、交互与动画案例

### 案例 1：选中态呼吸高亮动画

**场景**：当前选中的检测区域、缺陷标记呼吸闪烁，提醒操作人员当前操作对象。

xaml:

```xaml
<Rectangle Width="200" Height="150"
           Stroke="Cyan" StrokeThickness="2"
           Fill="#2200FFFF">
    <Rectangle.Triggers>
        <EventTrigger RoutedEvent="Loaded">
            <BeginStoryboard>
                <Storyboard RepeatBehavior="Forever" AutoReverse="True">
                    <DoubleAnimation Storyboard.TargetProperty="Opacity"
                                     From="1.0" To="0.4"
                                     Duration="0:0:1"/>
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
    </Rectangle.Triggers>
</Rectangle>
```

### 案例 2：检测中流动边框

**场景**：检测执行中的动态效果，通过虚线偏移动画模拟扫描流动，直观展示运行状态。

xaml:

```xaml
<Rectangle Width="250" Height="180"
           Stroke="LimeGreen" StrokeThickness="2"
           StrokeDashArray="8,4"
           Fill="Transparent">
    <Rectangle.Triggers>
        <EventTrigger RoutedEvent="Loaded">
            <BeginStoryboard>
                <Storyboard RepeatBehavior="Forever">
                    <DoubleAnimation Storyboard.TargetProperty="StrokeDashOffset"
                                     From="0" To="-12" Duration="0:0:0.8"/>
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
    </Rectangle.Triggers>
</Rectangle>
```

### 案例 3：可点击的检测区域（精确命中）

**场景**：点击检测区域弹出配置窗口，基于几何精确命中测试，圆角外的空白区域不会误触发。

xaml:

```xaml
<Rectangle Canvas.Left="100" Canvas.Top="80"
           Width="200" Height="150"
           RadiusX="6" RadiusY="6"
           Stroke="RoyalBlue" StrokeThickness="1.5"
           Fill="Transparent"
           Cursor="Hand"
           MouseLeftButtonUp="RoiConfig_MouseLeftButtonUp"/>
```

后台事件处理：

csharp:

```c#
private void RoiConfig_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
{
    // 弹出ROI参数配置窗口
    MessageBox.Show("配置检测区域参数：阈值、灵敏度、检测类型");
}
```

> 注意：必须设置 `Fill="Transparent"`，内部空白区域才会响应点击；如果 `Fill` 为 `null`，只有边框能触发点击。

------

## 四、C# 后台动态创建案例

### 案例 1：算法动态生成缺陷外接框

适合视觉算法输出缺陷结果后，动态添加到画布的场景。

csharp:

```xaml
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

/// <summary>
/// 在Canvas上添加一个缺陷外接矩形框
/// </summary>
/// <param name="canvas">目标画布</param>
/// <param name="x">左上角X坐标</param>
/// <param name="y">左上角Y坐标</param>
/// <param name="width">宽度</param>
/// <param name="height">高度</param>
/// <param name="isCritical">是否严重缺陷</param>
public void AddDefectRect(Canvas canvas, double x, double y, double width, double height, bool isCritical)
{
    Rectangle defectRect = new Rectangle();
    defectRect.Width = width;
    defectRect.Height = height;
    
    // 画刷冻结，提升渲染性能
    Brush strokeBrush = isCritical 
        ? new SolidColorBrush(Colors.Red) 
        : new SolidColorBrush(Colors.Orange);
    strokeBrush.Freeze();
    
    defectRect.Stroke = strokeBrush;
    defectRect.StrokeThickness = 1.5;
    defectRect.Fill = new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0x00, 0x00));

    // 左上角定位，与ROI坐标定义一致
    Canvas.SetLeft(defectRect, x);
    Canvas.SetTop(defectRect, y);

    canvas.Children.Add(defectRect);
}
```

### 案例 2：批量生成定位网格单元格

csharp:

```c#
/// <summary>
/// 绘制等距网格背景
/// </summary>
public void DrawGridCells(Canvas canvas, double totalWidth, double totalHeight, double cellSize)
{
    Brush cellBrush = new SolidColorBrush(Color.FromArgb(20, 128, 128, 128));
    cellBrush.Freeze();
    Brush borderBrush = new SolidColorBrush(Color.FromArgb(60, 128, 128, 128));
    borderBrush.Freeze();

    for (double x = 0; x < totalWidth; x += cellSize)
    {
        for (double y = 0; y < totalHeight; y += cellSize)
        {
            Rectangle cell = new Rectangle();
            cell.Width = cellSize;
            cell.Height = cellSize;
            cell.Fill = cellBrush;
            cell.Stroke = borderBrush;
            cell.StrokeThickness = 0.5;
            
            Canvas.SetLeft(cell, x);
            Canvas.SetTop(cell, y);
            canvas.Children.Add(cell);
        }
    }
}
```

------

## 五、MVVM 数据驱动案例（批量缺陷渲染）

**场景**：算法输出一批缺陷外接矩形列表，通过 `ItemsControl` 数据绑定批量渲染，纯 MVVM 模式，无需操作 UI 元素。

### 数据模型

csharp:

```c#
public class DefectRectInfo
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public bool IsCritical { get; set; }
}
```

### XAML 界面

xaml:

```xaml
<Canvas Width="800" Height="600" Background="#1E1E1E">
    <ItemsControl ItemsSource="{Binding DefectRectList}">
        <ItemsControl.ItemsPanel>
            <ItemsPanelTemplate>
                <Canvas/>
            </ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>

        <!-- 容器定位：绑定左上角坐标 -->
        <ItemsControl.ItemContainerStyle>
            <Style TargetType="ContentPresenter">
                <Setter Property="Canvas.Left" Value="{Binding X}"/>
                <Setter Property="Canvas.Top" Value="{Binding Y}"/>
            </Style>
        </ItemsControl.ItemContainerStyle>

        <!-- 数据模板：单个缺陷矩形 -->
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <Rectangle Width="{Binding Width}"
                           Height="{Binding Height}"
                           Stroke="{Binding IsCritical, Converter={StaticResource CriticalToBrushConverter}}"
                           StrokeThickness="1.5"
                           Fill="Transparent"/>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</Canvas>
```

> 工业价值：算法线程只需更新 `DefectRectList` 集合，UI 自动渲染所有缺陷框，完全符合 WPF 数据驱动理念，避免跨线程操作 UI 的问题。

------

## 六、工业场景避坑提示

1. **内部点击无响应：设置 `Fill="Transparent"`**

   只设置边框不设置填充时，矩形内部空白区域不参与命中测试；需要内部也响应点击，必须显式设置透明填充。

2. **1px 边框模糊：开启像素对齐**

   高精度绘图场景必须在父容器设置 `SnapsToDevicePixels="True"`，避免亚像素渲染导致的边框发虚、粗细不均。

3. **边框居中绘制：注意坐标偏移**

   边框是居中绘制的，2px 厚度的边框会向外扩展 1px；高精度对齐场景下，需要考虑边框厚度带来的外边界偏移。

4. **大量矩形注意性能**

   几十以内的静态矩形，Rectangle 开发效率最高；上百个高频刷新的矩形（如实时跟踪缺陷），每个都是完整`FrameworkElement`，有布局开销，超大量场景建议改用`DrawingVisual`低层绘制。

5. **圆角半径最大值限制**

   `RadiusX`最大不超过宽度的一半，`RadiusY`最大不超过高度的一半；超出后 WPF 会自动裁剪，当两者等于半宽半高时，矩形会变成椭圆。