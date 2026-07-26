# 004010001_WPF TextBlock 官方类定义 + 逐行深度解析

## 一、完整官方类定义（100% 匹配你提供的成员）

csharp:

```c#
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Automation.Peers;
using System.Windows.Markup;
using System.Windows.Media.TextFormatting;

namespace System.Windows.Controls
{
    /// <summary>
    /// 轻量级高性能文本显示控件，支持格式化文本与高级排版
    /// </summary>
    [Localizability(LocalizationCategory.Text)]
    [ContentProperty(nameof(Inlines))]
    public class TextBlock : FrameworkElement, IContentHost, IAddChildInternal, IAddChild, IServiceProvider
    {
        // ==============================================
        // 你提供的成员：静态依赖属性（完整18个）
        // ==============================================
        public static readonly DependencyProperty BaselineOffsetProperty;
        public static readonly DependencyProperty IsHyphenationEnabledProperty;
        public static readonly DependencyProperty TextWrappingProperty;
        public static readonly DependencyProperty TextAlignmentProperty;
        public static readonly DependencyProperty PaddingProperty;
        public static readonly DependencyProperty LineStackingStrategyProperty;
        public static readonly DependencyProperty LineHeightProperty;
        public static readonly DependencyProperty TextEffectsProperty;
        public static readonly DependencyProperty TextDecorationsProperty;
        public static readonly DependencyProperty TextTrimmingProperty;
        public static readonly DependencyProperty ForegroundProperty;
        public static readonly DependencyProperty FontSizeProperty;
        public static readonly DependencyProperty FontStretchProperty;
        public static readonly DependencyProperty FontWeightProperty;
        public static readonly DependencyProperty FontStyleProperty;
        public static readonly DependencyProperty FontFamilyProperty;
        public static readonly DependencyProperty TextProperty;
        public static readonly DependencyProperty BackgroundProperty;

        // ==============================================
        // 补充：静态构造函数（官方必需）
        // ==============================================
        static TextBlock();

        // ==============================================
        // 你提供的成员：公共构造函数
        // ==============================================
        public TextBlock();
        public TextBlock(Inline inline);

        // ==============================================
        // 你提供的成员：公共属性（完整24个）
        // ==============================================
        public FontWeight FontWeight { get; set; }
        public FontStyle FontStyle { get; set; }
        public FontFamily FontFamily { get; set; }
        public string Text { get; set; }
        public TextPointer ContentEnd { get; }
        public Typography Typography { get; }
        public LineBreakCondition BreakAfter { get; }
        public LineBreakCondition BreakBefore { get; }
        public FontStretch FontStretch { get; set; }
        public double BaselineOffset { get; set; }
        public double FontSize { get; set; }
        public TextWrapping TextWrapping { get; set; }
        public Brush Background { get; set; }
        public TextDecorationCollection TextDecorations { get; set; }
        public TextEffectCollection TextEffects { get; set; }
        public double LineHeight { get; set; }
        public LineStackingStrategy LineStackingStrategy { get; set; }
        public Thickness Padding { get; set; }
        public TextAlignment TextAlignment { get; set; }
        public TextTrimming TextTrimming { get; set; }
        public TextPointer ContentStart { get; }
        public bool IsHyphenationEnabled { get; set; }
        public Brush Foreground { get; set; }
        public InlineCollection Inlines { get; }

        // ==============================================
        // 你提供的成员：受保护属性
        // ==============================================
        protected virtual IEnumerator<IInputElement> HostedElementsCore { get; }
        protected override int VisualChildrenCount { get; }
        protected internal override IEnumerator LogicalChildren { get; }

        // ==============================================
        // 你提供的成员：静态方法（附加属性访问器）
        // ==============================================
        public static double GetBaselineOffset(DependencyObject element);
        public static FontFamily GetFontFamily(DependencyObject element);
        public static double GetFontSize(DependencyObject element);
        public static FontStretch GetFontStretch(DependencyObject element);
        public static FontStyle GetFontStyle(DependencyObject element);
        public static FontWeight GetFontWeight(DependencyObject element);
        public static Brush GetForeground(DependencyObject element);
        public static double GetLineHeight(DependencyObject element);
        public static LineStackingStrategy GetLineStackingStrategy(DependencyObject element);
        public static TextAlignment GetTextAlignment(DependencyObject element);
        public static void SetBaselineOffset(DependencyObject element, double value);
        public static void SetFontFamily(DependencyObject element, FontFamily value);
        public static void SetFontSize(DependencyObject element, double value);
        public static void SetFontStretch(DependencyObject element, FontStretch value);
        public static void SetFontStyle(DependencyObject element, FontStyle value);
        public static void SetFontWeight(DependencyObject element, FontWeight value);
        public static void SetForeground(DependencyObject element, Brush value);
        public static void SetLineHeight(DependencyObject element, double value);
        public static void SetLineStackingStrategy(DependencyObject element, LineStackingStrategy value);
        public static void SetTextAlignment(DependencyObject element, TextAlignment value);

        // ==============================================
        // 你提供的成员：公共实例方法
        // ==============================================
        public TextPointer GetPositionFromPoint(Point point, bool snapToText);
        public bool ShouldSerializeBaselineOffset();
        public bool ShouldSerializeInlines(XamlDesignerSerializationManager manager);
        public bool ShouldSerializeText();

        // ==============================================
        // 你提供的成员：受保护方法
        // ==============================================
        protected sealed override Size ArrangeOverride(Size arrangeSize);
        protected virtual ReadOnlyCollection<Rect> GetRectanglesCore(ContentElement child);
        protected override Visual GetVisualChild(int index);
        protected sealed override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters);
        protected virtual IInputElement InputHitTestCore(Point point);
        protected sealed override Size MeasureOverride(Size constraint);
        protected virtual void OnChildDesiredSizeChangedCore(UIElement child);
        protected override AutomationPeer OnCreateAutomationPeer();
        protected sealed override void OnPropertyChanged(DependencyPropertyChangedEventArgs e);
        protected sealed override void OnRender(DrawingContext ctx);

        // ==============================================
        // 补充：显式接口实现
        // ==============================================
        void IAddChild.AddChild(object value);
        void IAddChild.AddText(string text);
        IEnumerator IContentHost.EnumerateChildren();
        Rect[] IContentHost.GetRectangles(ContentElement child);
        void IContentHost.OnChildDesiredSizeChanged(UIElement child);
        object IServiceProvider.GetService(Type serviceType);
    }
}
```

------

## 二、逐行深度解析（含所有新增成员）

### 1. 类声明与接口实现

csharp:

```c#
public class TextBlock : FrameworkElement, IContentHost, IAddChildInternal, IAddChild, IServiceProvider
```

#### ① 核心继承关系

- **直接父类：FrameworkElement**（不是 ContentControl！这是与 Label 最本质的区别）
- 比 Label 少了 Control 和 ContentControl 两层继承，**无控件模板开销**，渲染速度是 Label 的 3-5 倍

#### ② 接口实现详解

| 接口                | 作用                                                   |
| :------------------ | :----------------------------------------------------- |
| `IAddChild`         | 支持 XAML 直接嵌套文本和内联元素                       |
| `IAddChildInternal` | WPF 内部使用的子元素添加接口，提供更高效的子元素管理   |
| `IContentHost`      | 作为内联文本元素（Run、Span 等）的宿主，负责布局和渲染 |
| `IServiceProvider`  | 提供文本格式化服务，支持高级排版功能                   |

------

### 2. 静态构造函数（官方内部实现）

csharp:

```c#
static TextBlock()
{
    // 注册所有18个依赖属性
    BaselineOffsetProperty = DependencyProperty.RegisterAttached(
        nameof(BaselineOffset),
        typeof(double),
        typeof(TextBlock),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Inherits));

    // 省略其他17个依赖属性注册...

    // 覆盖默认样式
    DefaultStyleKeyProperty.OverrideMetadata(
        typeof(TextBlock),
        new FrameworkPropertyMetadata(typeof(TextBlock)));

    // 强制不可聚焦
    FocusableProperty.OverrideMetadata(typeof(TextBlock), new FrameworkPropertyMetadata(false));
}
```

- 所有文本相关属性都标记了`AffectsMeasure`和`AffectsRender`，确保文本变化时自动重新布局
- 所有字体属性都标记了`Inherits`，子元素自动继承父元素的字体设置
- TextBlock 默认不可聚焦，符合纯文本显示的语义

------

### 3. 核心依赖属性完整解析（含新增）

#### ① 基础文本属性

| 属性         | 作用                         | 默认值                          | 工业场景使用说明                               |
| :----------- | :--------------------------- | :------------------------------ | :--------------------------------------------- |
| `Text`       | 要显示的纯文本内容           | `string.Empty`                  | 优先使用数据绑定，避免频繁直接赋值             |
| `Inlines`    | 内联元素集合，支持格式化文本 | 空集合                          | 用于显示不同颜色、样式的混合文本               |
| `Foreground` | 文本颜色                     | `SystemColors.ControlTextBrush` | 状态文本用不同颜色区分（绿 = 正常，红 = 错误） |
| `Background` | 文本背景色                   | `null`                          | 用于高亮显示重要信息                           |
| `Padding`    | 文本内边距                   | `0`                             | 适当增加内边距提升可读性                       |

#### ② 排版布局属性

| 属性                   | 作用             | 默认值          | 工业场景使用说明                               |
| :--------------------- | :--------------- | :-------------- | :--------------------------------------------- |
| `TextWrapping`         | 文本换行方式     | `NoWrap`        | 多行文本设为`Wrap`，长文本自动换行             |
| `TextTrimming`         | 文本超出截断方式 | `None`          | 单行文本设为`CharacterEllipsis`，末尾显示`...` |
| `TextAlignment`        | 文本对齐方式     | `Left`          | 数值右对齐，标签左对齐，标题居中               |
| `LineHeight`           | 行高             | `0`（自动计算） | 多行文本设为 1.2-1.5 倍字体大小，提升可读性    |
| `LineStackingStrategy` | 行堆叠策略       | `MaxHeight`     | 工业界面保持默认即可                           |
| `MaxLines`             | 最大显示行数     | `0`（无限制）   | 日志预览设为 3-5 行，避免界面溢出              |
| `BaselineOffset`       | 基线偏移量       | `0`             | 用于调整文本与其他元素的垂直对齐               |
| `IsHyphenationEnabled` | 是否启用自动断字 | `false`         | 英文长文本可设为`true`，中文无效               |

#### ③ 字体样式属性

| 属性          | 作用     | 默认值       | 工业场景使用说明                     |
| :------------ | :------- | :----------- | :----------------------------------- |
| `FontFamily`  | 字体家族 | 系统默认字体 | 工业界面推荐使用微软雅黑或思源黑体   |
| `FontSize`    | 字体大小 | `12`         | 工业界面常用 12-14px，标题用 16-18px |
| `FontWeight`  | 字体粗细 | `Normal`     | 重要信息用`Bold`突出显示             |
| `FontStyle`   | 字体样式 | `Normal`     | 注释或次要信息用`Italic`             |
| `FontStretch` | 字体拉伸 | `Normal`     | 一般保持默认                         |

#### ④ 高级文本效果属性

| 属性              | 作用              | 默认值   | 工业场景使用说明                 |
| :---------------- | :---------------- | :------- | :------------------------------- |
| `TextDecorations` | 文本装饰          | `null`   | 支持下划线、删除线、上划线       |
| `TextEffects`     | 文本特效          | `null`   | 支持阴影、发光、模糊等特效       |
| `Typography`      | OpenType 字体特性 | 自动创建 | 支持连字、小型大写字母等高级排版 |

------

### 4. 静态附加属性访问器解析

TextBlock 提供了 10 对静态 Get/Set 方法，这些是**附加属性的访问器**：

csharp

```c#
public static double GetBaselineOffset(DependencyObject element);
public static void SetBaselineOffset(DependencyObject element, double value);
// 省略其他9对...
```

- 允许在任何`DependencyObject`上设置 TextBlock 的文本属性

- 最常用的场景是在`ListBoxItem`、`Button`等控件上设置字体属性

- 示例：

  xaml:

  ```c#
  <Button TextBlock.FontSize="14" TextBlock.FontWeight="Bold" Content="确定"/>
  ```

- 这是 WPF 中非常实用的特性，可以统一设置容器内所有文本的样式

------

### 5. 核心方法逐行解析（含新增）

#### ① 布局与渲染核心方法（全部 sealed！）

csharp:

```c#
protected sealed override Size MeasureOverride(Size constraint);
protected sealed override Size ArrangeOverride(Size arrangeSize);
protected sealed override void OnRender(DrawingContext ctx);
```

- **全部标记为 sealed**：不允许子类重写，这是 TextBlock 高性能的关键保证
- 微软对这三个方法进行了深度优化，使用了底层文本格式化引擎
- 直接调用 GDI + 的低级绘图 API，没有任何中间层开销

#### ② 内容导航与命中测试

csharp:

```c#
public TextPointer GetPositionFromPoint(Point point, bool snapToText);
protected sealed override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters);
protected virtual IInputElement InputHitTestCore(Point point);
```

- `GetPositionFromPoint`：根据坐标获取对应的文本指针，用于实现文本选择、右键菜单等功能
- `HitTestCore`：重写命中测试逻辑，精确判断鼠标是否点击在文本上
- `InputHitTestCore`：输入命中测试，处理键盘和鼠标输入

#### ③ 序列化与设计时支持

csharp:

```c#
public bool ShouldSerializeBaselineOffset();
public bool ShouldSerializeInlines(XamlDesignerSerializationManager manager);
public bool ShouldSerializeText();
```

- 用于 XAML 设计器序列化，判断哪些属性需要保存到 XAML 文件中
- 工业开发中一般不需要直接调用

#### ④ 内容宿主接口实现

csharp:

```c#
protected virtual ReadOnlyCollection<Rect> GetRectanglesCore(ContentElement child);
protected virtual void OnChildDesiredSizeChangedCore(UIElement child);
```

- `GetRectanglesCore`：获取指定内联元素的边界矩形
- `OnChildDesiredSizeChangedCore`：当内联元素大小变化时触发，重新计算布局

------

### 6. 受保护属性解析

csharp:

```c#
protected virtual IEnumerator<IInputElement> HostedElementsCore { get; }
protected override int VisualChildrenCount { get; }
protected internal override IEnumerator LogicalChildren { get; }
```

- `HostedElementsCore`：枚举所有托管的输入元素
- `VisualChildrenCount`：可视化子元素数量，TextBlock 一般没有可视化子元素
- `LogicalChildren`：逻辑子元素枚举器，返回`Inlines`集合中的元素

------

## 三、TextBlock 核心功能深度解析

### 1. 极致的文本渲染性能

- **无控件模板开销**：直接继承自 FrameworkElement，没有 ControlTemplate 的中间层
- **底层文本引擎**：使用 WPF 的 TextFormatter 引擎，经过微软多年优化
- **硬件加速**：支持 DirectX 硬件加速渲染，大量文本也能保持流畅
- **内存优化**：内存占用仅为 Label 的 1/3，适合大量文本显示

### 2. 强大的格式化文本支持

通过`Inlines`集合，可以在同一个 TextBlock 中显示多种样式的文本：

xaml:

```xaml
<TextBlock>
    <Run Text="设备名称：" FontWeight="Bold" Foreground="#333"/>
    <Run Text="生产线1号机" Foreground="#666"/>
    <LineBreak/>
    <Run Text="运行状态：" FontWeight="Bold" Foreground="#333"/>
    <Run Text="正常" Foreground="#2ECC71" FontSize="14"/>
    <LineBreak/>
    <Run Text="报警信息：" FontWeight="Bold" Foreground="#333"/>
    <Run Text="无" Foreground="#999" TextDecorations="Strikethrough"/>
</TextBlock>
```

### 3. 丰富的排版控制

- 支持自动换行、文本截断、最大行数
- 支持文本对齐、行高、基线偏移
- 支持自动断字（英文）
- 支持 OpenType 字体特性

### 4. 完整的附加属性支持

所有文本属性都可以作为附加属性使用，统一设置容器内所有文本的样式：

xaml:

```xaml
<StackPanel TextBlock.FontSize="14" TextBlock.Foreground="#333">
    <TextBlock Text="标签1"/>
    <TextBlock Text="标签2"/>
    <TextBlock Text="标签3"/>
</StackPanel>
```

------

## 四、工业级使用方法与最佳实践

### 1. 基础文本显示

xaml:

```xaml
<!-- 纯文本 -->
<TextBlock Text="设备运行中"/>

<!-- 带样式的文本 -->
<TextBlock Text="温度：25.6℃" 
           FontSize="14" 
           FontWeight="Bold" 
           Foreground="#3498DB"/>

<!-- 右对齐数值 -->
<TextBlock Text="1234.56" 
           TextAlignment="Right" 
           Width="100"
           FontFamily="Consolas"/>
```

### 2. 文本溢出处理

xaml:

```xaml
<!-- 单行截断显示... -->
<TextBlock Text="这是一段很长的文本，超出部分会被截断" 
           TextTrimming="CharacterEllipsis" 
           Width="200"/>

<!-- 多行显示，最多3行 -->
<TextBlock Text="这是一段很长的文本，最多显示3行，超出部分会被截断" 
           TextWrapping="Wrap" 
           TextTrimming="CharacterEllipsis" 
           MaxLines="3" 
           Width="200"/>
```

### 3. 格式化文本

xaml:

```xaml
<TextBlock>
    <Run Text="报警级别：" FontWeight="Bold"/>
    <Run Text="紧急" Foreground="#E74C3C" FontSize="16"/>
    <LineBreak/>
    <Run Text="报警时间：" FontWeight="Bold"/>
    <Run Text="2024-05-20 14:30:00"/>
    <LineBreak/>
    <Run Text="报警内容：" FontWeight="Bold"/>
    <Run Text="温度超过上限" TextDecorations="Underline"/>
</TextBlock>
```

### 4. 附加属性统一设置样式

xaml:

```xaml
<GroupBox Header="设备参数" TextBlock.FontSize="12" TextBlock.Foreground="#333">
    <StackPanel>
        <TextBlock Text="温度：25.6℃"/>
        <TextBlock Text="压力：0.8MPa"/>
        <TextBlock Text="速度：10m/min"/>
    </StackPanel>
</GroupBox>
```

------

## 五、完整工业级实例

### 实例 1：设备状态详情面板

xaml:

```xaml
<Border Width="300" 
        Height="180" 
        BorderBrush="#DDD" 
        BorderThickness="1" 
        CornerRadius="4" 
        Background="#F9F9F9"
        Margin="10">
    <StackPanel Margin="15">
        <TextBlock Text="生产线1号机" 
                   FontSize="18" 
                   FontWeight="Bold" 
                   Foreground="#2C3E50"
                   Margin="0 0 0 15"/>

        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>

            <TextBlock Grid.Row="0" Grid.Column="0" Text="运行状态：" Margin="0 5 10 5"/>
            <TextBlock Grid.Row="0" Grid.Column="1" Text="正常运行" Foreground="#2ECC71" FontWeight="Bold"/>

            <TextBlock Grid.Row="1" Grid.Column="0" Text="当前温度：" Margin="0 5 10 5"/>
            <TextBlock Grid.Row="1" Grid.Column="1" Text="25.6℃" Foreground="#3498DB"/>

            <TextBlock Grid.Row="2" Grid.Column="0" Text="当前压力：" Margin="0 5 10 5"/>
            <TextBlock Grid.Row="2" Grid.Column="1" Text="0.85MPa" Foreground="#3498DB"/>

            <TextBlock Grid.Row="3" Grid.Column="0" Text="生产数量：" Margin="0 5 10 5"/>
            <TextBlock Grid.Row="3" Grid.Column="1" Text="1,234" Foreground="#9B59B6" FontFamily="Consolas"/>
        </Grid>
    </StackPanel>
</Border>
```

### 实例 2：实时日志显示

xaml:

```xaml
<Border BorderBrush="#DDD" 
        BorderThickness="1" 
        CornerRadius="4" 
        Background="#F5F5F5"
        Margin="10">
    <ScrollViewer VerticalScrollBarVisibility="Auto" MaxHeight="300">
        <TextBlock Text="{Binding LogText}" 
                   TextWrapping="Wrap" 
                   Padding="10"
                   FontFamily="Consolas"
                   FontSize="12"
                   LineHeight="18"/>
    </ScrollViewer>
</Border>
```

------

## 六、工业开发最佳实践

### 1. 性能优化

- **永远用 TextBlock 显示纯文本**：不要用 Label，性能差 3-5 倍
- **避免频繁修改 Text 属性**：使用数据绑定，WPF 会自动优化更新
- **大量文本使用虚拟化**：列表中的文本使用`VirtualizingStackPanel`
- **简化 Inlines 集合**：不要嵌套过多复杂的内联元素

### 2. 文本布局

- **单行文本必加 TextTrimming**：防止界面溢出
- **数值文本右对齐**：符合工业界面习惯
- **多行文本必加 TextWrapping**：自动换行
- **适当增加行高**：提升可读性

### 3. 样式统一

- **使用附加属性统一设置**：在父容器上设置 TextBlock 的字体属性
- **定义全局样式**：在 App.xaml 中定义 TextBlock 的默认样式
- **状态颜色标准化**：统一正常、警告、错误、离线的颜色

### 4. 常见坑与避坑指南

- ❌ 不要用 Label 显示纯文本
- ❌ 不要在 TextBlock 中嵌套复杂控件
- ❌ 不要设置太小的字体大小（小于 12px）
- ✅ 数值文本使用等宽字体（Consolas）
- ✅ 重要信息使用粗体和不同颜色突出显示

------

## 七、终极结论

### TextBlock 的本质

**TextBlock 是 WPF 中专门为纯文本显示优化的轻量级控件，它没有控件模板开销，使用底层文本引擎直接渲染，性能极高。它不仅支持简单的纯文本显示，还提供了丰富的格式化和排版功能，是 WPF 中显示文本的首选控件。**

它的核心优势在于：

1. **极致的性能**：渲染速度快，内存占用低
2. **丰富的功能**：支持格式化文本、高级排版、文本特效
3. **灵活的使用方式**：支持附加属性，可统一设置容器内所有文本样式
4. **良好的兼容性**：支持所有 WPF 特性，如数据绑定、样式、动画

在工业上位机开发中，TextBlock 是构建状态显示、日志输出、参数展示等界面的基础控件，掌握它的正确使用方法，是开发高性能、高质量工业界面的关键。