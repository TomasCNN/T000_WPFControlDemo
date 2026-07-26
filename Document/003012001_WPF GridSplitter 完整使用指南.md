# 003012001_WPF GridSplitter 完整使用指南

`GridSplitter`是 WPF 中**唯一官方支持的可拖拽窗口分割控件**，专门用于动态调整 Grid 行列的大小，是工业上位机主界面、监控界面、参数界面的必备组件。

------

## 一、核心原理与黄金使用规则

### 1. 核心原理

`GridSplitter`不是独立的布局容器，它**必须依附于 Grid**，通过拖动来重新分配**相邻两个行 / 列**的空间。它本身不占用布局空间（或只占用极小的分割线空间），所有的尺寸变化都作用在 Grid 的`RowDefinition`和`ColumnDefinition`上。

### 2. 必须遵守的 4 个黄金规则（否则拖不动 / 不显示）

| 规则                     | 说明                                                         | 错误后果     |
| :----------------------- | :----------------------------------------------------------- | :----------- |
| ✅ 必须放在 Grid 内部     | 不能放在 StackPanel/DockPanel 等其他容器中                   | 完全失效     |
| ✅ 必须设置正确的对齐方式 | 垂直分割：`VerticalAlignment="Stretch"`；水平分割：`HorizontalAlignment="Stretch"` | 拖不动       |
| ✅ 必须设置尺寸和背景     | 垂直分割：`Width="5"`；水平分割：`Height="5"`；必须设置`Background` | 看不见但能拖 |
| ✅ 必须有相邻的可调整行列 | 分割线两侧必须有至少一个`*`号尺寸的行 / 列                   | 拖动无效果   |

------

## 二、基础使用案例

### 案例 1：垂直分割（左右分栏，最常用）

**适用场景**：左导航 + 右内容、左参数 + 右图像

xaml:

```xaml
<Window Title="垂直分割示例" Width="800" Height="500">
    <Grid>
        <!-- 定义3列：左栏 + 分割线 + 右栏 -->
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="200*" MinWidth="150"/> <!-- 左栏，最小150px -->
            <ColumnDefinition Width="Auto"/> <!-- 分割线列，自动宽度 -->
            <ColumnDefinition Width="600*" MinWidth="300"/> <!-- 右栏，最小300px -->
        </Grid.ColumnDefinitions>

        <!-- 左栏内容 -->
        <Border Grid.Column="0" Background="#F5F6F8">
            <TextBlock Text="左侧导航栏" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>

        <!-- 垂直分割线（核心） -->
        <GridSplitter 
            Grid.Column="1"
            Width="5"
            Background="#DDD"
            VerticalAlignment="Stretch"
            HorizontalAlignment="Center"
            ShowsPreview="True"
            ResizeBehavior="PreviousAndNext"/>

        <!-- 右栏内容 -->
        <Border Grid.Column="2" Background="White">
            <TextBlock Text="右侧内容区" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>
    </Grid>
</Window>
```

### 案例 2：水平分割（上下分栏）

**适用场景**：上图像 + 下日志、上参数 + 下结果

xaml:

```xaml
<Window Title="水平分割示例" Width="800" Height="500">
    <Grid>
        <!-- 定义3行：上栏 + 分割线 + 下栏 -->
        <Grid.RowDefinitions>
            <RowDefinition Height="300*" MinHeight="200"/> <!-- 上栏 -->
            <RowDefinition Height="Auto"/> <!-- 分割线行 -->
            <RowDefinition Height="200*" MinHeight="100"/> <!-- 下栏 -->
        </Grid.RowDefinitions>

        <!-- 上栏内容 -->
        <Border Grid.Row="0" Background="#000">
            <TextBlock Text="图像显示区" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>

        <!-- 水平分割线（核心） -->
        <GridSplitter 
            Grid.Row="1"
            Height="5"
            Background="#DDD"
            HorizontalAlignment="Stretch"
            VerticalAlignment="Center"
            ShowsPreview="True"/>

        <!-- 下栏内容 -->
        <Border Grid.Row="2" Background="#F5F6F8">
            <TextBlock Text="日志输出区" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>
    </Grid>
</Window>
```

### 关键属性说明

| 属性                               | 作用                               | 推荐值                         |
| :--------------------------------- | :--------------------------------- | :----------------------------- |
| `ShowsPreview="True"`              | 拖动时只显示预览线，松开再更新布局 | 工业场景必须开，避免拖动时卡顿 |
| `ResizeBehavior="PreviousAndNext"` | 同时调整前后两个行列的大小         | 默认值，最常用                 |
| `ResizeDirection`                  | 强制指定调整方向（Rows/Columns）   | 自动识别，一般不用设置         |
| `DragIncrement="1"`                | 最小拖动单位（像素）               | 1，最流畅                      |

------

## 三、工业上位机经典布局案例

### 案例 1：三栏主界面（工业标准）

**适用场景**：左导航 + 中监控 + 右参数，90% 工业上位机的主界面布局

xaml:

```xaml
<Window Title="工业上位机主界面" Width="1200" Height="700">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="180" MinWidth="150"/> <!-- 左导航（固定宽度） -->
            <ColumnDefinition Width="Auto"/>
            <ColumnDefinition Width="*" MinWidth="400"/> <!-- 中间监控区（自适应） -->
            <ColumnDefinition Width="Auto"/>
            <ColumnDefinition Width="250" MinWidth="200"/> <!-- 右参数（固定宽度） -->
        </Grid.ColumnDefinitions>

        <!-- 左侧导航栏 -->
        <Border Grid.Column="0" Background="#2C3E50">
            <StackPanel Margin="10">
                <Button Content="实时监控" Height="40" Margin="0,5" Background="#34495E" Foreground="White" BorderThickness="0"/>
                <Button Content="参数配置" Height="40" Margin="0,5" Background="#34495E" Foreground="White" BorderThickness="0"/>
                <Button Content="数据记录" Height="40" Margin="0,5" Background="#34495E" Foreground="White" BorderThickness="0"/>
            </StackPanel>
        </Border>

        <!-- 左分割线 -->
        <GridSplitter Grid.Column="1" Width="4" Background="#1A242F" VerticalAlignment="Stretch"/>

        <!-- 中间监控区（嵌套上下分割） -->
        <Grid Grid.Column="2">
            <Grid.RowDefinitions>
                <RowDefinition Height="*" MinHeight="300"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="150" MinHeight="100"/>
            </Grid.RowDefinitions>

            <Border Grid.Row="0" Background="#000">
                <TextBlock Text="实时图像" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center" FontSize="20"/>
            </Border>

            <GridSplitter Grid.Row="1" Height="4" Background="#DDD" HorizontalAlignment="Stretch"/>

            <Border Grid.Row="2" Background="#F5F6F8">
                <TextBlock Text="运行日志" HorizontalAlignment="Center" VerticalAlignment="Center"/>
            </Border>
        </Grid>

        <!-- 右分割线 -->
        <GridSplitter Grid.Column="3" Width="4" Background="#DDD" VerticalAlignment="Stretch"/>

        <!-- 右侧参数面板 -->
        <Border Grid.Column="4" Background="#F8F9FA">
            <StackPanel Margin="10">
                <TextBlock Text="实时参数" FontWeight="Bold" FontSize="14" Margin="0,0,0,10"/>
                <TextBlock Text="温度：25.6℃" Margin="0,5"/>
                <TextBlock Text="速度：120m/min" Margin="0,5"/>
                <TextBlock Text="产量：1256PCS" Margin="0,5"/>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```

### 案例 2：四象限图像监控界面

**适用场景**：多相机监控、多工位同时显示

xaml:

```xaml
<Window Title="四象限监控" Width="1000" Height="800">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*"/>
            <ColumnDefinition Width="Auto"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>

        <!-- 左上相机 -->
        <Border Grid.Row="0" Grid.Column="0" Background="#000" Margin="2">
            <TextBlock Text="相机1" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>

        <!-- 右上相机 -->
        <Border Grid.Row="0" Grid.Column="2" Background="#000" Margin="2">
            <TextBlock Text="相机2" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>

        <!-- 左下相机 -->
        <Border Grid.Row="2" Grid.Column="0" Background="#000" Margin="2">
            <TextBlock Text="相机3" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>

        <!-- 右下相机 -->
        <Border Grid.Row="2" Grid.Column="2" Background="#000" Margin="2">
            <TextBlock Text="相机4" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>

        <!-- 垂直分割线 -->
        <GridSplitter Grid.Column="1" Grid.RowSpan="3" Width="5" Background="#DDD" VerticalAlignment="Stretch"/>

        <!-- 水平分割线 -->
        <GridSplitter Grid.Row="1" Grid.ColumnSpan="3" Height="5" Background="#DDD" HorizontalAlignment="Stretch"/>
    </Grid>
</Window>
```

------

## 四、工业级高级功能

### 1. 锁定 / 解锁分割线（防止误操作）

**工业场景必备**：调试时允许调整，运行时锁定分割线防止误操作

xaml:

```xaml
<GridSplitter 
    x:Name="mainSplitter"
    IsEnabled="{Binding IsDebugMode}"
    Width="5"
    Background="{Binding IsDebugMode, Converter={StaticResource BoolToColorConverter}}"/>
```

- `IsEnabled="False"`时，分割线变灰且无法拖动
- 可以通过按钮切换`IsDebugMode`来控制锁定状态

### 2. 保存和恢复用户布局

**用户体验必备**：用户调整好窗口大小后，下次打开自动恢复

csharp:

```xaml
// 保存布局（窗口关闭时）
private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
{
    Properties.Settings.Default.LeftColumnWidth = mainGrid.ColumnDefinitions[0].Width.ToString();
    Properties.Settings.Default.RightColumnWidth = mainGrid.ColumnDefinitions[2].Width.ToString();
    Properties.Settings.Default.Save();
}

// 恢复布局（窗口加载时）
private void Window_Loaded(object sender, RoutedEventArgs e)
{
    try
    {
        var converter = new GridLengthConverter();
        mainGrid.ColumnDefinitions[0].Width = (GridLength)converter.ConvertFromString(Properties.Settings.Default.LeftColumnWidth);
        mainGrid.ColumnDefinitions[2].Width = (GridLength)converter.ConvertFromString(Properties.Settings.Default.RightColumnWidth);
    }
    catch { }
}
```

### 3. 限制拖动范围（防止区域消失）

**工业场景必须设置**：防止用户把某个区域拖到看不见

xaml:

```xaml
<Grid.ColumnDefinitions>
    <!-- 左栏最小150px，最大300px -->
    <ColumnDefinition Width="200*" MinWidth="150" MaxWidth="300"/>
    <ColumnDefinition Width="Auto"/>
    <!-- 右栏最小300px -->
    <ColumnDefinition Width="*" MinWidth="300"/>
</Grid.ColumnDefinitions>
```

- 不要直接限制 GridSplitter 的尺寸，而是限制对应行列的`MinWidth`/`MaxWidth`
- 这样可以保证每个区域都有最小的显示空间

### 4. 自定义分割线样式

xaml:

```xaml
<Style TargetType="GridSplitter">
    <Setter Property="Background" Value="#DDD"/>
    <Setter Property="Cursor" Value="SizeWE"/> <!-- 垂直分割用SizeWE，水平用SizeNS -->
    <Style.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="Background" Value="#1976D2"/>
        </Trigger>
        <Trigger Property="IsEnabled" Value="False">
            <Setter Property="Background" Value="#EEE"/>
            <Setter Property="Cursor" Value="Arrow"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

------

## 五、常见坑与解决方案

### 1. ❌ 分割线拖不动

**原因**：对齐方式设置错误

**解决**：

- 垂直分割：`VerticalAlignment="Stretch"`，`HorizontalAlignment="Center"`
- 水平分割：`HorizontalAlignment="Stretch"`，`VerticalAlignment="Center"`

### 2. ❌ 分割线看不见但能拖

**原因**：没有设置`Background`属性

**解决**：必须设置`Background`，哪怕是透明的`Background="Transparent"`

### 3. ❌ 拖动时布局卡顿

**原因**：`ShowsPreview="False"`，拖动时实时更新布局

**解决**：设置`ShowsPreview="True"`，拖动时只显示预览线，松开再更新

### 4. ❌ 分割线被其他控件挡住

**原因**：GridSplitter 声明顺序在其他控件前面

**解决**：把 GridSplitter 放在所有子元素的最后面，或者设置`Panel.ZIndex="10"`

### 5. ❌ 嵌套分割失效

**原因**：内层 Grid 没有设置`MinWidth`/`MinHeight`

**解决**：给内层 Grid 的行列设置最小尺寸

------

## 六、工业场景最佳实践

1. **永远设置最小尺寸**：所有可调整的行列都必须设置`MinWidth`/`MinHeight`，防止用户把区域拖没
2. **开启预览模式**：`ShowsPreview="True"`，避免拖动时复杂界面卡顿
3. **固定 + 自适应结合**：导航栏、参数栏用固定宽度，内容区用`*`号自适应
4. **保存用户布局**：用户调整过的布局下次打开自动恢复，提升体验
5. **运行时锁定**：正常运行时锁定分割线，只在调试模式下允许调整
6. **简洁样式**：分割线用浅灰色，鼠标悬停时变蓝色，不要用太花哨的样式