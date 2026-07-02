# 006002003_WPF `Ellipse` 椭圆工业场景实战案例集

以下案例全部针对工业上位机视觉检测、设备状态监控、工位可视化等真实场景设计，涵盖基础用法、坐标对齐、状态指示、批量渲染、交互动画等高频需求，可直接复用。

------

## 一、基础入门案例

### 1. 基础正圆与椭圆

最基础的用法，通过 `Width`/`Height` 控制形状，`Fill`/`Stroke` 控制外观，全部属性继承自 `Shape` 基类。

xaml:

```xaml
<StackPanel Orientation="Horizontal" Margin="20">
    <!-- 正圆：宽高相等 -->
    <Ellipse Width="60" Height="60"
             Fill="LimeGreen"
             Stroke="DarkGreen"
             StrokeThickness="2"
             Margin="0,0,20,0"/>

    <!-- 椭圆：宽高不等 -->
    <Ellipse Width="80" Height="40"
             Fill="Transparent"
             Stroke="RoyalBlue"
             StrokeThickness="1.5"/>
</StackPanel>
```

**说明**：

- 宽高相等时渲染为正圆，宽高不等时为椭圆；
- `Fill` 设为 `Transparent` 时只显示轮廓，不遮挡下方内容。

### 2. 虚线圆形检测框

AOI 检测中常用虚线标注感兴趣区域（ROI），通过 `StrokeDashArray` 实现虚线效果。

xaml:

```xaml
<Canvas Width="400" Height="300" Background="#F5F5F5" SnapsToDevicePixels="True">
    <Ellipse Canvas.Left="100" Canvas.Top="80"
             Width="200" Height="140"
             Fill="#220088FF"
             Stroke="#FF0088FF"
             StrokeThickness="2"
             StrokeDashArray="6,3"/>
</Canvas>
```

**工业用途**：标记圆形检测视野、镜头有效区域，半透明填充既高亮又不遮挡下方图像。

**注意**：`SnapsToDevicePixels="True"` 对齐物理像素，避免高分辨率下线条发虚。

------

## 二、工业核心场景案例

### 案例 1：设备状态指示灯（三色渐变质感）

工业工位最常用的状态指示元件，通过径向渐变模拟发光质感，对应运行、待机、故障三种状态。

xaml:

```xaml
<StackPanel Orientation="Horizontal" Margin="20">
    <!-- 运行状态：绿色渐变指示灯 -->
    <Ellipse Width="28" Height="28" Margin="0,0,15,0"
             Stroke="#FF006600" StrokeThickness="1">
        <Ellipse.Fill>
            <RadialGradientBrush GradientOrigin="0.3,0.3" Center="0.4,0.4"
                                 RadiusX="0.8" RadiusY="0.8">
                <GradientStop Color="#FF88FF88" Offset="0"/>
                <GradientStop Color="#FF00CC00" Offset="0.7"/>
                <GradientStop Color="#FF008800" Offset="1"/>
            </RadialGradientBrush>
        </Ellipse.Fill>
    </Ellipse>

    <!-- 待机状态：黄色渐变 -->
    <Ellipse Width="28" Height="28" Margin="0,0,15,0"
             Stroke="#FF996600" StrokeThickness="1">
        <Ellipse.Fill>
            <RadialGradientBrush GradientOrigin="0.3,0.3" Center="0.4,0.4"
                                 RadiusX="0.8" RadiusY="0.8">
                <GradientStop Color="#FFFFEE88" Offset="0"/>
                <GradientStop Color="#FFFFCC00" Offset="0.7"/>
                <GradientStop Color="#FFCC9900" Offset="1"/>
            </RadialGradientBrush>
        </Ellipse.Fill>
    </Ellipse>

    <!-- 故障状态：红色渐变 -->
    <Ellipse Width="28" Height="28"
             Stroke="#FF990000" StrokeThickness="1">
        <Ellipse.Fill>
            <RadialGradientBrush GradientOrigin="0.3,0.3" Center="0.4,0.4"
                                 RadiusX="0.8" RadiusY="0.8">
                <GradientStop Color="#FFFF8888" Offset="0"/>
                <GradientStop Color="#FFEE0000" Offset="0.7"/>
                <GradientStop Color="#FFAA0000" Offset="1"/>
            </RadialGradientBrush>
        </Ellipse.Fill>
    </Ellipse>
</StackPanel>
```

**MVVM 绑定方案**：将 `Fill` 绑定到视图模型的 `DeviceState` 枚举，配合 `IValueConverter` 自动切换颜色，无需后台操作 UI。

### 案例 2：缺陷点中心对齐标记（视觉检测必用）

视觉算法输出的缺陷坐标通常是**中心点坐标**，而 Ellipse 默认以左上角定位，必须做偏移处理让圆心与坐标点对齐。

#### 推荐方案：负 Margin 偏移（最简单，不影响布局）

xaml:

```xaml
<Canvas Width="600" Height="400" Background="#222" SnapsToDevicePixels="True">
    <!-- 缺陷标记：圆心精准对齐(200, 180)坐标点 -->
    <Ellipse Canvas.Left="200" Canvas.Top="180"
             Width="30" Height="30"
             Margin="-15,-15,0,0"
             Stroke="Red" StrokeThickness="2"
             Fill="Transparent"/>

    <!-- 严重缺陷：双圈标记 -->
    <Ellipse Canvas.Left="350" Canvas.Top="220"
             Width="40" Height="40"
             Margin="-20,-20,0,0"
             Stroke="#FFFF3300" StrokeThickness="2"
             StrokeDashArray="4,2"
             Fill="#33FF3300"/>
</Canvas>
```

**原理**：`Margin="-15,-15"` 表示向左、向上各偏移 15 像素（半径长度），让圆心精准落在 `Canvas.Left/Top` 指定的坐标上，和视觉算法坐标系完全一致。

#### 备选方案：RenderTransform 平移

xaml:

```xaml
<Ellipse Canvas.Left="200" Canvas.Top="180" Width="30" Height="30" Stroke="Red">
    <Ellipse.RenderTransform>
        <TranslateTransform X="-15" Y="-15"/>
    </Ellipse.RenderTransform>
</Ellipse>
```

### 案例 3：C# 后台动态创建缺陷标记

适合算法线程计算出缺陷后，动态添加到画布的场景。

csharp:

```c#
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

public void AddDefectMark(Canvas canvas, double centerX, double centerY, double diameter)
{
    Ellipse defectMark = new Ellipse();
    defectMark.Width = diameter;
    defectMark.Height = diameter;
    defectMark.Stroke = new SolidColorBrush(Colors.Red);
    defectMark.StrokeThickness = 2;
    defectMark.Fill = Brushes.Transparent;

    // 圆心对齐坐标点
    Canvas.SetLeft(defectMark, centerX);
    Canvas.SetTop(defectMark, centerY);
    defectMark.Margin = new Thickness(-diameter / 2, -diameter / 2, 0, 0);

    // 添加到画布
    canvas.Children.Add(defectMark);
}
```

### 案例 4：批量缺陷点 MVVM 渲染（数据驱动）

工业场景中算法会输出一批缺陷列表，推荐用 `ItemsControl` 做数据绑定，纯 MVVM 模式，无需后台操作 UI 元素。

#### 数据模型

csharp:

```c#
public class DefectInfo
{
    public double CenterX { get; set; }
    public double CenterY { get; set; }
    public double Diameter { get; set; }
    public bool IsCritical { get; set; } // 严重缺陷标红，普通标黄
}
```

#### XAML 界面

xaml:

```xaml
<Canvas Width="800" Height="600" Background="#1E1E1E">
    <ItemsControl ItemsSource="{Binding DefectList}">
        <ItemsControl.ItemsPanel>
            <ItemsPanelTemplate>
                <Canvas/>
            </ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>

        <!-- 容器定位：绑定中心点坐标 -->
        <ItemsControl.ItemContainerStyle>
            <Style TargetType="ContentPresenter">
                <Setter Property="Canvas.Left" Value="{Binding CenterX}"/>
                <Setter Property="Canvas.Top" Value="{Binding CenterY}"/>
            </Style>
        </ItemsControl.ItemContainerStyle>

        <!-- 数据模板：渲染单个缺陷标记 -->
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <Ellipse Width="{Binding Diameter}"
                         Height="{Binding Diameter}"
                         Margin="{Binding Diameter, Converter={StaticResource HalfNegativeMarginConverter}}"
                         Stroke="{Binding IsCritical, Converter={StaticResource CriticalToBrushConverter}}"
                         StrokeThickness="1.5"
                         Fill="Transparent"/>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</Canvas>
```

**工业价值**：算法线程只需更新 `DefectList` 集合，UI 自动渲染所有缺陷标记，完全符合 WPF 数据驱动理念，避免跨线程操作 UI 的问题。

------

## 三、进阶交互与动画案例

### 案例 1：故障告警呼吸闪烁动画

设备故障时指示灯呼吸闪烁，提醒操作人员，纯 XAML 实现，无需后台代码。

xaml:

```xaml
<Ellipse Width="32" Height="32" Fill="Red" Stroke="DarkRed" StrokeThickness="1">
    <Ellipse.Triggers>
        <EventTrigger RoutedEvent="Loaded">
            <BeginStoryboard>
                <Storyboard RepeatBehavior="Forever" AutoReverse="True">
                    <DoubleAnimation Storyboard.TargetProperty="Opacity"
                                     From="1.0" To="0.3"
                                     Duration="0:0:0.8"/>
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
    </Ellipse.Triggers>
</Ellipse>
```

**效果**：不透明度在 1.0 到 0.3 之间循环渐变，模拟呼吸闪烁效果，工业告警场景非常常用。

### 案例 2：可点击的缺陷标记（精确命中测试）

Ellipse 基于几何精确命中测试，点击四角空白区域不会触发，体验远优于矩形按钮。

xaml:

```xaml
<Ellipse Canvas.Left="200" Canvas.Top="200"
         Width="36" Height="36"
         Margin="-18,-18,0,0"
         Stroke="Orange" StrokeThickness="2"
         Fill="#33FFA500"
         Cursor="Hand"
         MouseLeftButtonUp="DefectMark_MouseLeftButtonUp"/>
```

后台事件处理：

csharp:

```c#
private void DefectMark_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
{
    // 点击弹出缺陷详情、放大查看等逻辑
    var mark = sender as Ellipse;
    MessageBox.Show("查看缺陷详细信息");
}
```

### 案例 3：同心圆对位标记（高精度视觉对位）

相机对位、标定场景常用的靶心式标记，多层同心圆组合，精度高辨识度强。

xaml:

```xaml
<Canvas Width="60" Height="60">
    <!-- 外圈 -->
    <Ellipse Canvas.Left="0" Canvas.Top="0" Width="60" Height="60"
             Stroke="Cyan" StrokeThickness="1" Fill="Transparent"/>
    <!-- 中圈 -->
    <Ellipse Canvas.Left="15" Canvas.Top="15" Width="30" Height="30"
             Stroke="Cyan" StrokeThickness="1" Fill="Transparent"/>
    <!-- 中心点 -->
    <Ellipse Canvas.Left="28" Canvas.Top="28" Width="4" Height="4"
             Fill="Cyan"/>
</Canvas>
```

------

## 四、工业场景最佳实践与避坑

1. **永远用中心对齐坐标**

   视觉检测场景中，所有点标记都要做半径偏移，让圆心与算法坐标对齐，避免坐标换算错误。

2. **大量点慎用单个 Ellipse**

   如果缺陷点超过 100 个且高频刷新，每个 Ellipse 都是完整的 `FrameworkElement`，有布局开销。此时建议改用 `DrawingVisual` 或 `WriteableBitmap` 低层绘制方案。

3. **线条模糊必开像素对齐**

   工业高精度绘图必须设置 `SnapsToDevicePixels="True"`，避免亚像素渲染导致的线条发虚、边缘模糊。

4. **静态画刷尽量冻结**

   后台代码创建的 `Brush`、`Pen` 对象，调用 `Freeze()` 冻结，可减少内存占用，提升渲染性能。

5. **不要尝试继承 Ellipse**

   Ellipse 是密封类（`sealed`），无法继承。需要自定义圆形变体时，直接继承 `Shape` 基类，重写 `DefiningGeometry` 组合几何即可。