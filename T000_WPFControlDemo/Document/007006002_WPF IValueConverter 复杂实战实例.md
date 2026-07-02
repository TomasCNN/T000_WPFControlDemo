# 007006002_WPF `IValueConverter` 复杂实战实例

以下实例均面向工业软件的真实复杂场景，核心特点是**可配置、高复用、带业务规则、支持双向转换**，不再是简单的「布尔转颜色」单一场景，而是解决项目中一类通用问题，减少重复代码，统一业务规则。

所有实例均贴合你熟悉的视觉检测、PLC 通信、设备状态监控场景，可直接落地到项目中。

------

## 前置说明：复杂转换器的设计原则

1. **单一职责 + 高复用**：一个转换器解决一类问题，通过参数适配不同业务，避免重复造轮子；
2. **纯函数无状态**：相同输入永远得到相同输出，不在转换器内保存可变状态，保证多绑定共用安全；
3. **健壮性优先**：处理异常输入、边界值，失败时返回 `DependencyProperty.UnsetValue` 走兜底逻辑；
4. **性能友好**：引用类型静态缓存、高频场景做结果缓存，避免频繁创建对象。

------

## 实例 1：可配置分段阈值告警转换器

### 场景

工业现场存在大量模拟量（温度、压力、曝光时间、真空度），都需要根据阈值分段显示不同颜色（正常绿、预警黄、告警红）。如果每个参数都写一个转换器，会产生大量重复代码。

本转换器通过 `ConverterParameter` 传入阈值分段，**一个转换器适配所有模拟量告警场景**。

### 转换器实现

csharp:

```c#
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

/// <summary>
/// 数值分段阈值转颜色转换器
/// 参数格式："阈值1,阈值2,阈值3"，从小到大排列
/// 颜色对应：小于阈值1→绿色，阈值1~2→黄色，阈值2~3→橙色，大于阈值3→红色
/// </summary>
public class RangeThresholdToBrushConverter : IValueConverter
{
    // 静态缓存画刷，避免重复创建
    private static readonly Brush NormalBrush = Brushes.LimeGreen;
    private static readonly Brush WarningBrush = Brushes.Orange;
    private static readonly Brush SevereBrush = Brushes.DarkOrange;
    private static readonly Brush AlarmBrush = Brushes.Red;
    private static readonly Brush ErrorBrush = Brushes.Gray;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 1. 数值解析失败，返回灰色兜底
        if (!double.TryParse(value?.ToString(), out double numValue))
            return ErrorBrush;

        // 2. 解析阈值参数
        if (parameter == null || string.IsNullOrWhiteSpace(parameter.ToString()))
            return NormalBrush;

        string[] thresholdStrs = parameter.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (thresholdStrs.Length == 0)
            return NormalBrush;

        // 3. 转换阈值数组并排序
        double[] thresholds = new double[thresholdStrs.Length];
        for (int i = 0; i < thresholdStrs.Length; i++)
        {
            if (!double.TryParse(thresholdStrs[i].Trim(), out thresholds[i]))
                return NormalBrush;
        }
        Array.Sort(thresholds);

        // 4. 分段判断
        if (numValue < thresholds[0])
            return NormalBrush;
        
        for (int i = 0; i < thresholds.Length - 1; i++)
        {
            if (numValue < thresholds[i + 1])
            {
                return i switch
                {
                    0 => WarningBrush,
                    1 => SevereBrush,
                    _ => AlarmBrush
                };
            }
        }

        // 超过最高阈值
        return AlarmBrush;
    }

    // 单向转换，不需要反向
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
```

### XAML 使用

xaml:

```xaml
<Window.Resources>
    <local:RangeThresholdToBrushConverter x:Key="RangeThresholdConverter"/>
</Window.Resources>

<StackPanel Spacing="8">
    <!-- 温度：阈值30℃、50℃ -->
    <TextBlock Text="{Binding Temperature, StringFormat=设备温度：{0:F1}℃}"
               Foreground="{Binding Temperature, 
                   Converter={StaticResource RangeThresholdConverter},
                   ConverterParameter='30,50'}"/>

    <!-- 曝光时间：阈值2000μs、5000μs -->
    <TextBlock Text="{Binding ExposureTime, StringFormat=曝光时间：{0}μs}"
               Foreground="{Binding ExposureTime, 
                   Converter={StaticResource RangeThresholdConverter},
                   ConverterParameter='2000,5000'}"/>

    <!-- 真空度：阈值-90kPa、-80kPa -->
    <TextBlock Text="{Binding Vacuum, StringFormat=真空度：{0}kPa}"
               Foreground="{Binding Vacuum, 
                   Converter={StaticResource RangeThresholdConverter},
                   ConverterParameter='-90,-80'}"/>
</StackPanel>
```

### 核心亮点

- 一个转换器复用所有模拟量告警，通过参数灵活配置阈值；
- 支持任意数量的阈值分段，自动排序，适配正 / 负数值场景；
- 画刷全部静态缓存，高频刷新无 GC 压力。

------

## 实例 2：PLC 工程单位缩放双向转换器

### 场景

PLC 通信中，为了保证传输精度，通常会把浮点数放大 N 倍转为整数传输（比如 25.6℃ 存成 256，放大 10 倍；0.85mm 存成 850，放大 1000 倍）。

上位机显示时需要转回实际工程值，用户输入后还要转回整数下发到 PLC。

本转换器支持传入缩放系数，**双向完成「PLC 整数 ↔ 实际工程值」的换算**，是工业通信的通用基础设施。

### 转换器实现

csharp:

```c#
using System;
using System.Globalization;
using System.Windows.Data;

/// <summary>
/// PLC整数与实际工程值双向缩放转换器
/// 参数：缩放系数（10、100、1000），PLC值 = 实际值 * 系数
/// </summary>
public class PlcScaleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 读取PLC整数值，转为实际工程值
        if (!int.TryParse(value?.ToString(), out int plcValue))
            return 0d;

        if (!double.TryParse(parameter?.ToString(), out double scale))
            scale = 1d;

        return Math.Round(plcValue / scale, 3);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 用户输入的实际值，转回PLC整数
        if (!double.TryParse(value?.ToString(), out double actualValue))
            return 0;

        if (!double.TryParse(parameter?.ToString(), out double scale))
            scale = 1d;

        // 四舍五入取整，保证PLC数据合法
        return (int)Math.Round(actualValue * scale, MidpointRounding.AwayFromZero);
    }
}
```

### XAML 使用

xaml:

```xaml
<StackPanel Spacing="10">
    <!-- 温度：放大10倍，保留1位小数 -->
    <StackPanel Orientation="Horizontal">
        <TextBlock Width="100" VerticalAlignment="Center">设定温度：</TextBlock>
        <TextBox Width="150"
                 Text="{Binding PlcTemperature, 
                        Mode=TwoWay,
                        Converter={StaticResource PlcScaleConverter},
                        ConverterParameter=10,
                        StringFormat={}{0:F1} ℃,
                        UpdateSourceTrigger=LostFocus}"/>
    </StackPanel>

    <!-- 厚度：放大1000倍，保留3位小数 -->
    <StackPanel Orientation="Horizontal">
        <TextBlock Width="100" VerticalAlignment="Center">材料厚度：</TextBlock>
        <TextBox Width="150"
                 Text="{Binding PlcThickness, 
                        Mode=TwoWay,
                        Converter={StaticResource PlcScaleConverter},
                        ConverterParameter=1000,
                        StringFormat={}{0:F3} mm}"/>
    </StackPanel>
</StackPanel>
```

### 核心亮点

- 真正的双向转换，显示和回写自动完成缩放，业务代码无需关心换算逻辑；
- 通过参数适配不同缩放系数的 PLC 地址，一个转换器覆盖所有模拟量通信；
- 回写时自动四舍五入取整，避免非法值写入 PLC。

------

## 实例 3：缺陷等级样式转换器（返回复合 Style）

### 场景

视觉检测的缺陷标注框，根据严重等级不同，需要显示不同的颜色、线宽、线型：

- 严重缺陷：红色、2px 粗实线
- 一般缺陷：橙色、1.5px 实线
- 轻微缺陷：黄色、1px 虚线

如果分别绑定 `Stroke`、`StrokeThickness`、`StrokeDashArray` 三个属性，需要三个转换器，代码冗余且样式分散。

本转换器直接返回一个 `Style` 对象，**一次性控制图形元素的所有外观属性**，样式规则集中管理。

### 转换器实现

csharp:

```c#
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

/// <summary>
/// 缺陷等级转图形样式转换器
/// 输入：DefectLevel 枚举，输出：Shape 控件的 Style
/// </summary>
public class DefectLevelToStyleConverter : IValueConverter
{
    // 静态缓存样式，全局复用
    private static readonly Style CriticalStyle;
    private static readonly Style NormalStyle;
    private static readonly Style MinorStyle;

    // 静态构造函数，只初始化一次
    static DefectLevelToStyleConverter()
    {
        // 严重缺陷：红色粗实线
        CriticalStyle = new Style(typeof(Shape));
        CriticalStyle.Setters.Add(new Setter(Shape.StrokeProperty, Brushes.Red));
        CriticalStyle.Setters.Add(new Setter(Shape.StrokeThicknessProperty, 2d));
        CriticalStyle.Setters.Add(new Setter(Shape.FillProperty, new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0, 0))));
        CriticalStyle.Seal(); // 冻结样式，提升性能

        // 一般缺陷：橙色实线
        NormalStyle = new Style(typeof(Shape));
        NormalStyle.Setters.Add(new Setter(Shape.StrokeProperty, Brushes.Orange));
        NormalStyle.Setters.Add(new Setter(Shape.StrokeThicknessProperty, 1.5d));
        NormalStyle.Setters.Add(new Setter(Shape.FillProperty, new SolidColorBrush(Color.FromArgb(0x22, 0xFF, 0xA5, 0))));
        NormalStyle.Seal();

        // 轻微缺陷：黄色虚线
        MinorStyle = new Style(typeof(Shape));
        MinorStyle.Setters.Add(new Setter(Shape.StrokeProperty, Brushes.Yellow));
        MinorStyle.Setters.Add(new Setter(Shape.StrokeThicknessProperty, 1d));
        MinorStyle.Setters.Add(new Setter(Shape.StrokeDashArrayProperty, new DoubleCollection(new double[] { 4, 2 })));
        MinorStyle.Setters.Add(new Setter(Shape.FillProperty, Brushes.Transparent));
        MinorStyle.Seal();
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DefectLevel level)
            return MinorStyle;

        return level switch
        {
            DefectLevel.Critical => CriticalStyle,
            DefectLevel.Normal => NormalStyle,
            DefectLevel.Minor => MinorStyle,
            _ => MinorStyle
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

// 缺陷等级枚举
public enum DefectLevel
{
    Minor,      // 轻微
    Normal,     // 一般
    Critical    // 严重
}
```

### XAML 使用（批量缺陷框场景）

xaml:

```xaml
<ItemsControl ItemsSource="{Binding DefectList}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <Canvas Width="800" Height="600" Background="#1E1E1E"/>
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>

    <ItemsControl.ItemContainerStyle>
        <Style TargetType="ContentPresenter">
            <Setter Property="Canvas.Left" Value="{Binding X}"/>
            <Setter Property="Canvas.Top" Value="{Binding Y}"/>
        </Style>
    </ItemsControl.ItemContainerStyle>

    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <!-- 单个缺陷矩形，Style 直接绑定等级转换器 -->
            <Rectangle Width="{Binding Width}"
                       Height="{Binding Height}"
                       Style="{Binding Level, Converter={StaticResource DefectLevelToStyleConverter}}"/>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

### 核心亮点

- **样式集中管理**：所有缺陷等级的外观规则都在转换器里，调整样式只需要改一处；
- **性能最优**：样式静态构造 + `Seal()` 冻结，全程序共用同一份样式对象，无内存冗余；
- **简化绑定**：原本需要 3 个属性绑定 + 3 个转换器，现在 1 个 Style 绑定搞定。

------

## 实例 4：枚举 Description 通用转换器

### 场景

项目中有大量枚举（设备状态、缺陷类型、工位模式、告警等级），都需要转成友好的中文显示。如果每个枚举写一个转换器，代码冗余且维护成本高。

本转换器通过**反射读取枚举字段的 `[Description]` 特性**，自动返回中文描述，一次编写，所有枚举复用。

### 转换器实现

csharp:

```c#
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

/// <summary>
/// 枚举转描述文本通用转换器
/// 自动读取枚举的 [Description] 特性，无特性则返回枚举名
/// </summary>
public class EnumToDescriptionConverter : IValueConverter
{
    // 缓存反射结果，避免重复反射损耗性能
    private static readonly Dictionary<Enum, string> _descriptionCache = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || !value.GetType().IsEnum)
            return DependencyProperty.UnsetValue;

        Enum enumValue = (Enum)value;

        // 优先读缓存
        if (_descriptionCache.TryGetValue(enumValue, out string desc))
            return desc;

        // 反射读取Description特性
        FieldInfo field = enumValue.GetType().GetField(enumValue.ToString());
        DescriptionAttribute attr = field.GetCustomAttribute<DescriptionAttribute>();
        
        desc = attr?.Description ?? enumValue.ToString();
        _descriptionCache[enumValue] = desc;

        return desc;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
```

### 枚举定义（加 Description 特性）

csharp:

```c#
public enum DeviceStatus
{
    [Description("运行中")]
    Running,
    
    [Description("待机")]
    Standby,
    
    [Description("告警")]
    Alarm,
    
    [Description("离线")]
    Offline
}
```

### XAML 使用

xaml:

```xaml
<StackPanel Spacing="8">
    <TextBlock Text="{Binding CurrentStatus, Converter={StaticResource EnumToDescriptionConverter}}"/>
    <TextBlock Text="{Binding DefectType, Converter={StaticResource EnumToDescriptionConverter}}"/>
    <TextBlock Text="{Binding StationMode, Converter={StaticResource EnumToDescriptionConverter}}"/>
</StackPanel>
```

### 核心亮点

- **全枚举通用**：任意加了 `[Description]` 的枚举都能直接用，无需新增转换器；
- **缓存优化**：反射结果静态缓存，第一次反射后后续直接读缓存，性能接近硬编码；
- **降级友好**：没有 Description 特性时自动返回枚举名称，不会报错。

------

## 实例 5：对象有效性转可见性转换器（多边界判断）

### 场景

控制控件显隐的条件非常多：对象为 null、字符串为空、集合为空、数值为 0，都属于「无效状态」需要隐藏。

简单的 `BoolToVisibilityConverter` 只能处理布尔值，本转换器统一处理所有空值场景，同时支持反转参数，是项目级通用工具。

### 转换器实现

csharp:

```c#
using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

/// <summary>
/// 对象有效性转可见性转换器
/// 规则：null、空字符串、空集合、数值0 → 隐藏；否则显示
/// 参数"Invert"：反转逻辑
/// </summary>
public class ObjectToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isValid = CheckIsValid(value);

        // 反转参数
        if (parameter?.ToString().Equals("Invert", StringComparison.OrdinalIgnoreCase) == true)
            isValid = !isValid;

        return isValid ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();

    // 统一判断对象是否有效
    private bool CheckIsValid(object value)
    {
        // null 无效
        if (value == null) return false;

        // 空字符串无效
        if (value is string str)
            return !string.IsNullOrWhiteSpace(str);

        // 空集合无效
        if (value is ICollection collection)
            return collection.Count > 0;

        // 数值0无效
        if (value is int intVal) return intVal != 0;
        if (value is double doubleVal) return Math.Abs(doubleVal) > 1e-6;
        if (value is decimal decimalVal) return decimalVal != 0;

        // 其他类型默认有效
        return true;
    }
}
```

### XAML 使用

xaml:

```xaml
<StackPanel>
    <!-- 告警详情：有告警信息时显示 -->
    <Border Background="#FFF3CD" Padding="10"
            Visibility="{Binding AlarmMessage, Converter={StaticResource ObjectToVisibilityConverter}}">
        <TextBlock Text="{Binding AlarmMessage}" Foreground="DarkOrange"/>
    </Border>

    <!-- 空列表占位符：缺陷列表为空时显示 -->
    <TextBlock Text="暂无缺陷记录" Foreground="Gray" HorizontalAlignment="Center"
               Visibility="{Binding DefectList, 
                   Converter={StaticResource ObjectToVisibilityConverter},
                   ConverterParameter=Invert}"/>

    <!-- 设备详情：设备对象非空时显示 -->
    <Grid Visibility="{Binding CurrentDevice, Converter={StaticResource ObjectToVisibilityConverter}}">
        <!-- 详情内容 -->
    </Grid>
</StackPanel>
```

### 核心亮点

- 覆盖 null、空字符串、空集合、零值等所有常见空场景，健壮性极强；
- 支持反转参数，一个转换器同时满足「有效显示」和「无效显示占位符」两种场景；
- 替代项目中大量重复的可见性转换逻辑，统一行为规范。

------

## 高级注意事项

### 1. `ConverterParameter` 不支持绑定

`ConverterParameter` 不是依赖属性，只能传静态常量。如果需要动态变化的参数（比如阈值来自配置），请改用 `IMultiValueConverter` 多值转换器，把参数作为第二个绑定输入。

### 2. 不要在转换器里写业务逻辑

转换器只做「类型 / 样式转换」，复杂的业务判断、数据计算应该放在 ViewModel 中。比如 “根据设备状态 + 权限 + 告警等级决定是否显示”，这种多业务条件组合的逻辑，不适合塞在转换器里。

### 3. 高频场景加结果缓存

对于输入值范围有限、刷新频率极高的场景（比如状态枚举转颜色），一定要加静态缓存，避免重复计算和对象创建，这是工业软件高频刷新场景的重要优化点。

### 4. 调试技巧

转换器逻辑出错时，不会直接抛异常，只会绑定失败。可以在 `Convert` 方法内打断点，或临时加输出日志，排查输入值、参数是否符合预期。

------

## 选型总结

| 场景                         | 推荐方案                          |
| :--------------------------- | :-------------------------------- |
| 单类型简单转换               | 普通专用转换器                    |
| 同类多参数场景（阈值、缩放） | 带参数的通用转换器                |
| 多属性外观控制               | 返回 Style / 复合对象的转换器     |
| 所有枚举转中文               | 反射 + 缓存的通用枚举转换器       |
| 通用空值判断                 | 多边界有效性转换器                |
| 多个输入决定一个输出         | `IMultiValueConverter` 多值转换器 |