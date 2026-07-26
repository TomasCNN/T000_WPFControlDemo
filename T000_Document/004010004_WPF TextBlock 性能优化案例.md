# 004010004_WPF TextBlock 性能优化案例

## 一、最核心：批量更新 Text（日志 / 数据流必备）

### 错误写法（高频卡顿元凶）

csharp：

```c#
// ❌ 千万不要这样写！每次 += 都会触发布局+渲染
for (int i = 0; i < 1000; i++)
{
    txtLog.Text += $"日志 {i}\r\n"; 
}
```

### 正确写法（性能提升 50～200 倍）

csharp：

```c#
// ✅ 只更新一次，性能爆炸
StringBuilder sb = new StringBuilder();

for (int i = 0; i < 1000; i++)
{
    sb.AppendLine($"日志 {i}");
}

txtLog.Text = sb.ToString(); // 只赋值1次
```

------

## 二、高频实时数据更新（温度 / 压力 / 转速）

工业设备每秒刷新 10～50 次，直接赋值会巨卡。

### 优化方案：**节流更新（Throttle）**

csharp:

```c#
private DateTime _lastUpdate = DateTime.MinValue;
private readonly int _updateIntervalMs = 100; // 100ms = 每秒最多10次

// 外部高频调用（比如PLC数据推送）
public void UpdateTemperature(double value)
{
    // 节流：限制刷新频率
    if ((DateTime.Now - _lastUpdate).TotalMilliseconds < _updateIntervalMs)
        return;

    _lastUpdate = DateTime.Now;

    // UI 线程安全更新
    Dispatcher.Invoke(() =>
    {
        txtTemp.Text = $"{value:F1} ℃";
    });
}
```

------

## 三、关闭不必要的布局刷新（高性能文本）

如果你只改**颜色 / 透明度**，不需要重新计算布局。

### XAML 开启高性能渲染

xaml:

```xaml
<TextBlock 
    Text="高性能文本"
    UseLayoutRounding="True"
    SnapsToDevicePixels="True"
    RenderOptions.BitmapScalingMode="LowQuality"
    RenderOptions.ClearTypeHint="Enabled"/>
```

------

## 四、超大文本（>1000 行）不卡顿方案

### 错误：直接用 TextBlock 承载 1 万行日志 → 内存爆炸

#### 正确：**限制最大行数 + 自动清理**

csharp:

```c#
private StringBuilder _logBuilder = new StringBuilder();
private const int MAX_LOG_LINES = 500; // 最多保留500行

public void AddLog(string msg)
{
    _logBuilder.AppendLine($"[{DateTime.Now:HH:mm:ss}] {msg}");

    // 超过行数自动裁剪前面的旧日志
    if (_logBuilder.Length > 20000) 
    {
        string log = _logBuilder.ToString();
        int lines = log.Split('\n').Length;

        if (lines > MAX_LOG_LINES)
        {
            int trimIndex = log.IndexOf('\n') + 1;
            _logBuilder.Remove(0, trimIndex);
        }
    }

    txtLog.Text = _logBuilder.ToString();
}
```

------

## 五、避免 Inlines 频繁操作（格式化文本）

### 错误：循环 Add (...) → 每次都刷新布局

csharp:

```c#
// ❌ 慢
txt.Inlines.Clear();
txt.Inlines.Add(new Run("A"));
txt.Inlines.Add(new Run("B"));
txt.Inlines.Add(new Run("C"));
```

### 正确：先构建完，最后一次性赋值

csharp:

```c#
// ✅ 快
var inlines = new InlineCollection();
inlines.Add(new Run("A"));
inlines.Add(new Run("B"));
inlines.Add(new Run("C"));

txt.Inlines.Clear();
foreach (var item in inlines)
    txt.Inlines.Add(item);
```

------

## 六、后台线程拼接文本，不卡 UI

csharp:

```c#
// ✅ 后台拼接，UI只负责显示
Task.Run(() =>
{
    StringBuilder sb = new StringBuilder();
    for (int i = 0; i < 5000; i++)
    {
        sb.AppendLine($"数据行 {i}");
    }
    return sb.ToString();
}).ContinueWith(t =>
{
    Dispatcher.Invoke(() =>
    {
        txtLog.Text = t.Result;
    });
});
```

------

## 七、静态文本不触发任何布局刷新

如果你有**不变的标题 / 标签**，加上：

xaml:

```xaml
<TextBlock 
    Text="固定标题"
    IsHitTestVisible="False"
    Focusable="False"
    CanBeHyphenated="False"
/>
```

关闭命中测试、聚焦、自动断字 → **减少 30% 渲染消耗**。

------

## 八、高性能状态文本（颜色变化不触发布局）

颜色变化只触发渲染，不触发布局，性能极高：

csharp:

```c#
// ✅ 颜色变化极快，不卡顿
Dispatcher.Invoke(() =>
{
    if (isError)
        txtState.Foreground = Brushes.Red;
    else
        txtState.Foreground = Brushes.Green;
});
```

------

## 九、终极工业级高性能 TextBlock 模板

xaml:

```xaml
<TextBlock 
    x:Name="txtHighPerformance"
    Text="高性能文本"
    FontSize="12"
    UseLayoutRounding="True"
    SnapsToDevicePixels="True"
    RenderOptions.ClearTypeHint="Enabled"
    TextOptions.TextFormattingMode="Ideal"
    TextOptions.TextRenderingMode="Auto"
    IsTextScaleFactorEnabled="False"
/>
```

------

## 十、性能优化总结（工业开发必背）

1. **不要用 txt.Text += ...** → 必卡
2. **必须用 StringBuilder** 批量拼接
3. **高频数据必须节流**（100ms 刷新一次足够）
4. **固定文本关闭命中测试**
5. **大文本自动裁剪**，避免内存爆炸
6. **颜色修改极快**，布局修改很慢