# 004010006_WPF TextBlock 三个问题的工业级完整解析

## 问题 1：除了节流更新，还有哪些 TextBlock 性能优化方法？

### 一、静态文本极致优化（性能提升 30%+）

对于**永远不会变化的标题、标签、说明文字**，关闭所有不必要的特性：

xaml:

```xaml
<TextBlock 
    Text="设备参数配置"
    FontSize="14"
    FontWeight="Bold"
    <!-- 核心优化项 -->
    IsHitTestVisible="False"          <!-- 关闭鼠标命中测试，不响应任何鼠标事件 -->
    Focusable="False"                 <!-- 禁止获取焦点 -->
    IsHyphenationEnabled="False"      <!-- 关闭自动断字（中文无效） -->
    IsTextScaleFactorEnabled="False"  <!-- 禁止系统文本缩放 -->
    UseLayoutRounding="True"          <!-- 像素对齐，解决文本模糊 -->
    SnapsToDevicePixels="True"/>
```

**原理**：关闭这些特性后，WPF 会跳过大量不必要的计算和事件处理，静态文本的渲染开销降低 30% 以上。

------

### 二、渲染级优化（解决文本模糊和卡顿）

xaml:

```xaml
<TextBlock 
    Text="实时数据：123.45"
    <!-- 渲染优化 -->
    RenderOptions.ClearTypeHint="Enabled"          <!-- 强制启用ClearType抗锯齿 -->
    RenderOptions.BitmapScalingMode="NearestNeighbor" <!-- 位图缩放模式，适合工业界面 -->
    TextOptions.TextFormattingMode="Display"       <!-- 显示模式，小字体更清晰 -->
    TextOptions.TextRenderingMode="ClearType"/>    <!-- 强制ClearType渲染 -->
```

**工业场景必加**：解决工业显示器上文本发虚、边缘模糊的问题，同时提升渲染速度。

------

### 三、避免 Inlines 频繁操作（格式化文本优化）

❌ **错误写法**（每次 Add 都触发布局刷新）：

csharp:

```c#
txtStatus.Inlines.Clear();
txtStatus.Inlines.Add(new Run("状态：") { FontWeight = FontWeights.Bold });
txtStatus.Inlines.Add(new Run("正常") { Foreground = Brushes.Green });
```

✅ **正确写法**（批量操作，只刷新一次）：

csharp:

```c#
// 先在内存中构建所有内联元素
var inlines = new List<Inline>
{
    new Run("状态：") { FontWeight = FontWeights.Bold },
    new Run("正常") { Foreground = Brushes.Green }
};

// 一次性添加到TextBlock
txtStatus.Inlines.Clear();
foreach (var inline in inlines)
{
    txtStatus.Inlines.Add(inline);
}
```

**性能提升**：格式化文本的更新速度提升 5-10 倍。

------

### 四、大文本内存优化（防止内存泄漏）

对于超过 500 行的日志文本，必须实现**自动裁剪机制**：

csharp:

```c#
private readonly StringBuilder _logBuilder = new StringBuilder();
private const int MAX_LOG_LENGTH = 50000; // 最多保留50KB文本（约500行）

public void AddLog(string message)
{
    _logBuilder.AppendLine($"[{DateTime.Now:HH:mm:ss}] {message}");

    // 超过阈值自动裁剪最旧的日志
    if (_logBuilder.Length > MAX_LOG_LENGTH)
    {
        // 找到第一个换行符，删除前面的所有内容
        int firstNewLineIndex = _logBuilder.ToString().IndexOf('\n');
        if (firstNewLineIndex > 0)
        {
            _logBuilder.Remove(0, firstNewLineIndex + 1);
        }
    }

    // 只更新一次UI
    Dispatcher.InvokeAsync(() =>
    {
        txtLog.Text = _logBuilder.ToString();
    });
}
```

**工业场景必备**：防止日志无限增长导致的内存泄漏和界面卡顿。

------

### 五、数据绑定优化

❌ **错误写法**（每次属性变化都触发完整布局）：

xaml:

```c#
<TextBlock Text="{Binding CurrentTemperature, StringFormat='{0:F1}℃'}"/>
```

✅ **正确写法**（使用 OneWay 绑定，关闭不必要的更新）：

xaml:

```c#
<TextBlock Text="{Binding CurrentTemperature, Mode=OneWay, StringFormat='{0:F1}℃'}"/>
```

**原理**：OneWay 绑定比默认的 TwoWay 绑定少了反向更新的开销，对于只显示不修改的文本，性能提升明显。

------

### 六、控件选型优化（最容易被忽略的点）

| 文本量          | 推荐控件                 | 原因                   |
| :-------------- | :----------------------- | :--------------------- |
| < 100 行        | TextBlock                | 性能最高，最简单       |
| 100-1000 行     | TextBlock + 自动裁剪     | 平衡性能和功能         |
| > 1000 行       | FlowDocumentScrollViewer | 支持虚拟化，内存占用低 |
| 需要选择 / 复制 | RichTextBox              | 支持文本选择和复制     |

**工业开发铁律**：超过 1000 行的文本绝对不要用 TextBlock，否则会出现严重的内存泄漏和卡顿。

------

## 问题 2：如何在 TextBlock 中实现文本的自动换行？

### 一、基础自动换行

只需要设置 `TextWrapping="Wrap"` 即可：

xaml:

```xaml
<TextBlock 
    Text="这是一段很长的文本，当超出TextBlock的宽度时会自动换行显示，不会撑破界面布局。"
    Width="300"
    TextWrapping="Wrap"/>
```

------

### 二、TextWrapping 三个枚举值的区别

| 枚举值             | 行为                         | 适用场景           |
| :----------------- | :--------------------------- | :----------------- |
| `NoWrap`           | 不换行，超出部分截断         | 单行文本           |
| `Wrap`             | 自动换行，在单词边界换行     | 英文文本、中文文本 |
| `WrapWithOverflow` | 自动换行，长单词可能超出边界 | 极少使用           |

**工业场景推荐**：永远使用 `Wrap`，不要用 `WrapWithOverflow`。

------

### 三、最常见的坑：自动换行不生效

#### 坑 1：父容器是 StackPanel（水平方向无限宽）

❌ **错误写法**：

xaml:

```xaml
<StackPanel Orientation="Horizontal">
    <!-- 永远不会换行，因为StackPanel水平方向无限宽 -->
    <TextBlock Text="很长的文本" TextWrapping="Wrap"/>
</StackPanel>
```

✅ **正确写法**：使用 Grid 或 DockPanel 限制宽度：

xaml:

```xaml
<Grid>
    <TextBlock Text="很长的文本" TextWrapping="Wrap"/>
</Grid>
```

#### 坑 2：没有设置 TextBlock 的宽度

TextBlock 必须有一个**有限的宽度**才能换行，可以是固定宽度、最大宽度或父容器限制的宽度：

xaml:

```xaml
<!-- 固定宽度 -->
<TextBlock Text="很长的文本" TextWrapping="Wrap" Width="300"/>

<!-- 最大宽度 -->
<TextBlock Text="很长的文本" TextWrapping="Wrap" MaxWidth="300"/>

<!-- 父容器限制宽度 -->
<Grid Width="300">
    <TextBlock Text="很长的文本" TextWrapping="Wrap"/>
</Grid>
```

------

### 四、高级换行控制

#### 1. 强制换行

使用 `<LineBreak/>` 标签强制换行：

xaml:

```xaml
<TextBlock>
    <Run Text="第一行文本"/>
    <LineBreak/>
    <Run Text="第二行文本"/>
</TextBlock>
```

#### 2. 禁止换行

对于不需要换行的文本，显式设置 `TextWrapping="NoWrap"`：

xaml:

```xaml
<TextBlock Text="设备编号：PRO-2025-001" TextWrapping="NoWrap"/>
```

------

## 问题 3：如何在 TextBlock 中实现文本的滚动显示？

### 一、垂直滚动（多行日志，最常用）

结合 `ScrollViewer` 实现垂直滚动，支持自动滚动到底部：

xaml:

```xaml
<Border BorderBrush="#DDD" BorderThickness="1" CornerRadius="4" Background="#F5F5F5">
    <ScrollViewer 
        x:Name="scrollViewer"
        VerticalScrollBarVisibility="Auto"
        HorizontalScrollBarVisibility="Disabled"
        MaxHeight="300">
        <TextBlock 
            x:Name="txtLog"
            TextWrapping="Wrap"
            Padding="10"
            FontFamily="Consolas"
            FontSize="12"
            LineHeight="18"/>
    </ScrollViewer>
</Border>
```

#### 自动滚动到底部（工业日志必备）

csharp:

```c#
public void AddLog(string message)
{
    _logBuilder.AppendLine($"[{DateTime.Now:HH:mm:ss}] {message}");
    
    Dispatcher.InvokeAsync(() =>
    {
        txtLog.Text = _logBuilder.ToString();
        // 自动滚动到底部
        scrollViewer.ScrollToEnd();
    });
}
```

------

### 二、水平滚动（跑马灯效果，用于长标题 / 报警信息）

实现**平滑循环滚动**，支持鼠标悬停暂停：

xaml:

```xaml
<Grid Width="300" Height="30" ClipToBounds="True">
    <TextBlock x:Name="txtMarquee" Text="这是一条很长的报警信息：设备温度超过上限，请立即处理！">
        <TextBlock.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard x:Name="MarqueeStoryboard" RepeatBehavior="Forever">
                        <DoubleAnimation 
                            Storyboard.TargetName="txtMarquee"
                            Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.X)"
                            From="300" To="-500" Duration="0:0:10"/>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </TextBlock.Triggers>
        
        <TextBlock.RenderTransform>
            <TranslateTransform X="0"/>
        </TextBlock.RenderTransform>
        
        <!-- 鼠标悬停暂停 -->
        <TextBlock.Style>
            <Style TargetType="TextBlock">
                <Style.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter Property="Cursor" Value="Hand"/>
                        <Trigger.EnterActions>
                            <PauseStoryboard Storyboard="{StaticResource MarqueeStoryboard}"/>
                        </Trigger.EnterActions>
                        <Trigger.ExitActions>
                            <ResumeStoryboard Storyboard="{StaticResource MarqueeStoryboard}"/>
                        </Trigger.ExitActions>
                    </Trigger>
                </Style.Triggers>
            </Style>
        </TextBlock.Style>
    </TextBlock>
</Grid>
```

**参数说明**：

- `From="300"`：从右侧 300 像素处开始滚动（和 Grid 宽度一致）
- `To="-500"`：滚动到左侧 500 像素处（大于文本长度）
- `Duration="0:0:10"`：滚动一周需要 10 秒

------

### 三、滚动优化

#### 1. 平滑滚动

xaml：

```xaml
<ScrollViewer 
    ScrollViewer.CanContentScroll="False"  <!-- 平滑滚动，不是按行滚动 -->
    VerticalScrollBarVisibility="Auto">
    <TextBlock Text="很长的文本"/>
</ScrollViewer>
```

#### 2. 禁止水平滚动

工业界面一般不需要水平滚动，显式关闭：

xaml:

```xaml
<ScrollViewer HorizontalScrollBarVisibility="Disabled">
    <TextBlock TextWrapping="Wrap"/>
</ScrollViewer>
```

------

## 工业级 TextBlock 性能优化最佳实践总结

1. **静态文本**：关闭命中测试、焦点、自动断字等不必要的特性
2. **动态文本**：使用 StringBuilder 批量更新，高频数据加节流
3. **格式化文本**：先在内存中构建 Inlines，再一次性添加
4. **大文本**：实现自动裁剪，超过 1000 行使用 FlowDocumentScrollViewer
5. **渲染**：开启像素对齐和 ClearType 渲染，解决文本模糊
6. **布局**：避免在 StackPanel 中使用自动换行的 TextBlock
7. **滚动**：日志使用 ScrollViewer + 自动滚动到底部，长标题使用跑马灯