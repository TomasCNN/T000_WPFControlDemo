# 003002004_WPF **Panel 基类** 官方类定义

这是**所有布局容器（Grid、StackPanel、Canvas、WrapPanel 等）的顶级抽象基类**，是 WPF 布局系统的心脏。

我会**严格按照官方源码结构**，把**字段、属性、方法、修饰符、作用、设计意图**一次性讲透。



> **📄 摘要**：Panel 是 WPF 布局体系的抽象基类，承载所有布局容器（Grid、StackPanel 等）的核心逻辑。本文深入拆解 Panel 的类定义，逐一分析其依赖属性、公共/受保护成员及布局核心方法，归纳出 Children vs InternalChildren、逻辑方向判定与 IsItemsHost 三大精髓。配合自定义 SimpleStackPanel 完整示例（含 ZIndex 与 Margin 处理），以及常见问题排查指南，帮助读者彻底掌握 WPF 自定义布局面板的开发与调试。



------

# 一、Panel 类完整官方定义（带完整成员）

csharp:

```c#
using System;
using System.Collections;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows.Controls
{
    [ContentProperty(nameof(Children))]
    public abstract class Panel : FrameworkElement, IAddChild
    {
        // 静态依赖属性
        public static readonly DependencyProperty BackgroundProperty;
        public static readonly DependencyProperty IsItemsHostProperty;
        public static readonly DependencyProperty ZIndexProperty;

        // 构造函数
        protected Panel();

        // 公共属性
        public Brush Background { get; set; }
        public bool IsItemsHost { get; set; }
        public UIElementCollection Children { get; }
        public bool HasLogicalOrientationPublic { get; }
        public Orientation LogicalOrientationPublic { get; }

        // 受保护 / 受保护内部 属性
        protected internal UIElementCollection InternalChildren { get; }
        protected internal virtual bool HasLogicalOrientation { get; }
        protected internal virtual Orientation LogicalOrientation { get; }
        protected override int VisualChildrenCount { get; }
        protected internal override IEnumerator LogicalChildren { get; }

        // 静态方法（ZIndex 附加属性）
        public static int GetZIndex(UIElement element);
        public static void SetZIndex(UIElement element, int value);

        // 公共方法
        public bool ShouldSerializeChildren();

        // 受保护方法
        protected virtual UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent);
        protected override Visual GetVisualChild(int index);
        protected virtual void OnIsItemsHostChanged(bool oldIsItemsHost, bool newIsItemsHost);
        protected override void OnRender(DrawingContext dc);
        protected internal override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved);

        // IAddChild 接口
        void IAddChild.AddChild(object value);
        void IAddChild.AddText(string text);
    }
}
```

------

# 二、逐行 **深度解析**

## 1. 类声明

csharp:

```c#
public abstract class Panel : FrameworkElement, IAddChild
```

- **`abstract`**：抽象类，**不能直接实例化**，只能被继承。
- **`FrameworkElement`**：WPF 核心基类，拥有布局、渲染、数据绑定、样式等能力。
- **`IAddChild`**：允许 XAML 直接添加子元素（如 `<Panel><Button/></Panel>`）。

------

## 2. 静态依赖属性

### **BackgroundProperty**

csharp:

```c#
public static readonly DependencyProperty BackgroundProperty;
```

面板背景画刷（SolidColorBrush、ImageBrush 等）。

### **IsItemsHostProperty**

csharp:

```c#
public static readonly DependencyProperty IsItemsHostProperty;
```

标记此 Panel 是否作为 `ItemsControl`（ListBox、ListView、DataGrid）的**项面板**。

### **ZIndexProperty**

csharp:

```c#
public static readonly DependencyProperty ZIndexProperty;
```

**附加属性**，控制子元素的**层叠显示顺序**。

值越大，越在上层。

------

## 3. 构造函数

csharp:

```c#
protected Panel();
```

- **protected**：只能由子类调用。
- 初始化 Children、InternalChildren、背景默认值（透明）。

------

## 4. 公共属性

### **Background**

csharp:

```c#
public Brush Background { get; set; }
```

获取或设置面板背景。

默认：**Transparent**（透明，但可命中测试）。

若要鼠标穿透，设为 `{x:Null}`。

------

### **IsItemsHost**

csharp:

```c#
public bool IsItemsHost { get; set; }
```

- `true` → Panel 是 ItemsControl 的项宿主。
- 此时 Children 由数据生成器管理，不允许手动添加子元素。

------

### **Children**

csharp:

```c#
public UIElementCollection Children { get; }
```

**对外公开的子元素集合**。

XAML 直接写的子元素都在这里。

------

### **HasLogicalOrientationPublic**

csharp:

```c#
public bool HasLogicalOrientationPublic { get; }
```

公开属性，包装 `HasLogicalOrientation`。

指示布局是否有明确方向（StackPanel → true；Grid → false）。

------

### **LogicalOrientationPublic**

csharp:

```c#
public Orientation LogicalOrientationPublic { get; }
```

公开属性，包装 `LogicalOrientation`。

获取方向：Horizontal / Vertical。

------

## 5. 受保护 / 受保护内部 属性（布局核心）

### **InternalChildren**

csharp:

```c#
protected internal UIElementCollection InternalChildren { get; }
```

**布局系统真正使用的子元素集合**。

- 自定义 Panel 必须遍历它，**不要用 Children**。

------

### **HasLogicalOrientation**

csharp:

```c#
protected internal virtual bool HasLogicalOrientation { get; }
```

- 默认：**false**
- StackPanel、WrapPanel 等线性布局会**重写为 true**。

------

### **LogicalOrientation**

csharp:

```c#
protected internal virtual Orientation LogicalOrientation { get; }
```

- 默认：**抛异常**
- 有方向的布局（StackPanel）重写并返回 Vertical / Horizontal。

------

### **VisualChildrenCount**

csharp:

```c#
protected override int VisualChildrenCount { get; }
```

返回可视化树子元素数量 → 即 `InternalChildren.Count`。

------

### **LogicalChildren**

csharp:

```c#
protected internal override IEnumerator LogicalChildren { get; }
```

返回逻辑树子元素枚举器。

------

## 6. 静态方法（ZIndex 附加属性）

### **GetZIndex**

csharp:

```c#
public static int GetZIndex(UIElement element);
```

获取元素的 ZIndex。

### **SetZIndex**

csharp:

```c#
public static void SetZIndex(UIElement element, int value);
```

设置元素的 ZIndex。

------

## 7. 公共方法

### **ShouldSerializeChildren**

csharp:

```c#
public bool ShouldSerializeChildren();
```

设计器专用：判断是否需要序列化 Children 集合。

------

## 8. 受保护方法（布局系统核心）

### **CreateUIElementCollection**

csharp:

```c#
protected virtual UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent);
```

创建子元素集合，可重写以自定义存储。

------

### **GetVisualChild**

csharp:

```c#
protected override Visual GetVisualChild(int index);
```

获取可视化树的第 index 个子元素。

------

### **OnIsItemsHostChanged**

csharp:

```c#
protected virtual void OnIsItemsHostChanged(bool oldIsItemsHost, bool newIsItemsHost);
```

当 `IsItemsHost` 改变时触发。

------

### **OnRender**

csharp:

```c#
protected override void OnRender(DrawingContext dc);
```

渲染背景。

------

### **OnVisualChildrenChanged**

csharp:

```c#
protected internal override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved);
```

子元素被添加 / 删除时触发，用于更新布局。

------

## 9. IAddChild 接口（XAML 解析用）

csharp:

```c#
void IAddChild.AddChild(object value);
void IAddChild.AddText(string text);
```

- `AddChild`：添加子元素（如 Button）。
- `AddText`：Panel 不支持文本，会抛出异常。

------

# 三、最核心的 3 个知识点（必须掌握）

## ✔ 1. **Children 与 InternalChildren 的区别**

- **Children**：公开集合，用于外部访问。

- **InternalChildren**：**布局系统专用集合**。

  

  自定义 Panel 必须遍历 InternalChildren！

------

## ✔ 2. **HasLogicalOrientation / LogicalOrientation**

- **StackPanel、WrapPanel、VirtualizingStackPanel** → 有方向 → 返回 true。
- **Grid、Canvas、UniformGrid** → 无方向 → 返回 false。

------

## ✔ 3. **IsItemsHost = 列表控件的项面板**

ListBox、DataGrid 的布局底层都是一个 Panel，且 `IsItemsHost=true`。

------

# 四、一句话总结

**Panel 是 WPF 所有布局容器的基类，负责管理子元素、提供布局框架、支持层叠顺序、并作为列表控件的项宿主。**

所有布局算法都必须继承 Panel 并实现测量与排列。


------

# 五、实战：自定义简单布局面板

下面演示如何继承 Panel 类，重写 MeasureOverride 和 ArrangeOverride，实现一个将所有子元素垂直居中堆叠的自定义面板——SimpleStackPanel。

## 5.1 SimpleStackPanel 完整代码（C#）

```csharp
using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfCustomPanelDemo
{
    /// <summary>
    /// 自定义面板：将子元素垂直居中堆叠，每个子元素宽度填满面板。
    /// </summary>
    public class SimpleStackPanel : Panel
    {
        // ========== 布局第一阶段：测量 ==========
        protected override Size MeasureOverride(Size constraint)
        {
            double totalHeight = 0;
            double maxWidth = 0;

            // 遍历所有子元素，调用 Measure 获取期望尺寸
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(constraint); // 给子元素父容器提供的可用尺寸
                totalHeight += child.DesiredSize.Height;
                maxWidth = Math.Max(maxWidth, child.DesiredSize.Width);
            }

            // 返回面板所需的理想尺寸
            return new Size(maxWidth, totalHeight);
        }

        // ========== 布局第二阶段：排列 ==========
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            double currentY = 0;

            foreach (UIElement child in InternalChildren)
            {
                // 子元素宽度填满面板，高度使用其期望高度，水平居中，垂直依次排列
                double childWidth = Math.Max(0, arrangeSize.Width);
                double childHeight = child.DesiredSize.Height;
                double x = 0; // 水平居中时 x=0（宽度已填满）
                double y = currentY;

                child.Arrange(new Rect(x, y, childWidth, childHeight));
                currentY += childHeight;
            }

            return arrangeSize;
        }
    }
}
```

## 5.2 XAML 使用示例

```xaml
<Window x:Class="WpfCustomPanelDemo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:WpfCustomPanelDemo"
        Title="SimpleStackPanel 示例" Height="300" Width="300">
    <local:SimpleStackPanel Background="AliceBlue">
        <Border Background="LightCoral" Height="40" />
        <Border Background="LightGreen" Height="60" />
        <Border Background="LightSkyBlue" Height="50" />
        <Border Background="LightGoldenrodYellow" Height="40" />
    </local:SimpleStackPanel>
</Window>
```

## 5.3 关键点说明

- **必须遍历 InternalChildren**：布局计算不能使用 Children 公共集合。
- **Measure 阶段**：必须调用每个子元素的 `Measure` 方法，否则子元素不会被测量。
- **Arrange 阶段**：必须调用每个子元素的 `Arrange` 方法并传入其最终矩形。
- 此面板简单垂直堆叠，宽度占满，高度按子元素期望高度累加，适合展示列表式卡片信息。

------


## 5.4 进阶：支持 ZIndex 和 Margin

原版 SimpleStackPanel 没有考虑子元素的 Margin 属性与 ZIndex 层叠顺序。下面展示如何重写 `GetVisualChild` 与 `VisualChildrenCount` 以支持 ZIndex，并在 `ArrangeOverride` 中正确处理 Margin 对最终排列矩形的影响。

### 完整 C# 代码

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfCustomPanelDemo
{
    /// <summary>
    /// 自定义面板：垂直居中堆叠，支持 ZIndex 层叠与 Margin 处理。
    /// </summary>
    public class SimpleStackPanel : Panel
    {
        /// <summary>
        /// 按 ZIndex 升序排列的子元素列表（缓存，避免重复排序）。
        /// </summary>
        private List<UIElement> GetSortedChildren()
        {
            var list = InternalChildren.Cast<UIElement>().ToList();
            list.Sort((a, b) =>
            {
                int za = Panel.GetZIndex(a);
                int zb = Panel.GetZIndex(b);
                return za.CompareTo(zb);
            });
            return list;
        }

        // ========== 可视化树支持（ZIndex 生效的关键）==========

        protected override int VisualChildrenCount => InternalChildren.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= InternalChildren.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            // 按 ZIndex 排序后返回对应索引的子元素
            return GetSortedChildren()[index];
        }

        // ========== 布局第一阶段：测量（考虑 Margin）==========

        protected override Size MeasureOverride(Size constraint)
        {
            double totalHeight = 0;
            double maxWidth = 0;

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(constraint);
                Size desired = child.DesiredSize;
                Thickness margin = child is FrameworkElement fe ? fe.Margin : new Thickness(0);

                totalHeight += desired.Height + margin.Top + margin.Bottom;
                maxWidth = Math.Max(maxWidth, desired.Width + margin.Left + margin.Right);
            }

            return new Size(maxWidth, totalHeight);
        }

        // ========== 布局第二阶段：排列（按 ZIndex 排序 + 扣除 Margin）==========

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            double currentY = 0;

            // 按 ZIndex 升序排列——ZIndex 越大的子元素越后 Arrange，渲染时越靠上
            foreach (UIElement child in GetSortedChildren())
            {
                Thickness margin = child is FrameworkElement fe ? fe.Margin : new Thickness(0);
                double childWidth  = Math.Max(0, arrangeSize.Width - margin.Left - margin.Right);
                double childHeight = child.DesiredSize.Height;
                double x = margin.Left;
                double y = currentY + margin.Top;

                child.Arrange(new Rect(x, y, childWidth, childHeight));
                currentY += child.DesiredSize.Height + margin.Top + margin.Bottom;
            }

            return arrangeSize;
        }
    }
}
```

### 关键说明

- **GetVisualChild + VisualChildrenCount**：WPF 渲染引擎遍历可视化树时调用这两个方法。重写后按 ZIndex 升序返回子元素，**ZIndex 越大的元素索引越大 → 后绘制 → 视觉上更靠上**。
- **Margin 处理**：在 `MeasureOverride` 中累加 `margin.Top + margin.Bottom`，使面板总高度包含间距；在 `ArrangeOverride` 中从 `arrangeSize.Width` 减去 `margin.Left + margin.Right` 得到子元素实际可用宽度，x 偏移量设为 `margin.Left` 以保留左边距。
- **排序缓存**：`GetSortedChildren()` 每次都重新排序。若子元素数量较大，可结合 `OnVisualChildrenChanged` 设置 dirty flag 按需刷新，避免性能损耗。


# 六、常见问题与排查

在自定义 Panel 开发过程中，大多数问题都集中在测量（Measure）与排列（Arrange）两个阶段。下面列出四个典型问题及其排查思路。

## 6.1 子元素完全不显示

**现象**：面板渲染后一片空白，所有子元素都不可见。

**常见原因**：

1. **未调用 `child.Measure()`**  
   如果 MeasureOverride 中没有对每个子元素调用 `Measure`，子元素的 `DesiredSize` 将为零，ArrangeOverride 中即使调用 `Arrange` 也可能安排一个零尺寸矩形，导致不显示。
2. **未调用 `child.Arrange()`**  
   即使 Measure 正确，ArrangeOverride 没有调用每个子元素的 `Arrange`，子元素仍然不会渲染。
3. **使用了 `Children` 而非 `InternalChildren`**  
   布局遍历必须使用 `InternalChildren`，如果误用 `Children`，部分元素可能被遗漏（尤其是当 Panel 被用作 ItemsControl 的项宿主时）。
4. **Arrange 矩形尺寸为零或负数**  
   检查传给 `child.Arrange` 的 `Rect` 是否合法，避免宽/高为 0。

**排查步骤**：

```csharp
protected override Size ArrangeOverride(Size arrangeSize)
{
    foreach (UIElement child in InternalChildren)
    {
        // 调试输出 Arrange 前的状态
        System.Diagnostics.Debug.WriteLine($"Arrange: {child} DesiredSize={child.DesiredSize} ArrangeRect={new Rect(0, 0, child.DesiredSize.Width, child.DesiredSize.Height)}");
        child.Arrange(new Rect(0, 0, child.DesiredSize.Width, child.DesiredSize.Height));
    }
    return arrangeSize;
}
```

## 6.2 布局循环（无限测量/排列）

**现象**：界面卡死，CPU 飙升，甚至抛出 `LayoutCycleException`。

**根本原因**：布局过程中修改了哪些属性/调用了 `InvalidateMeasure` 或 `InvalidateArrange`，导致 WPF 不断触发新一轮的布局过程。

**典型触发场景**：

- 在 ArrangeOverride 中修改子元素的 `Width`/`Height`/`Margin` 等影响布局的属性。
- 在 MeasureOverride 中根据测量结果修改子元素的属性，然后再次要求测量。
- 在 `OnVisualChildrenChanged` 中强制调用 `InvalidateMeasure` 而没有做好防重入判断。

**解决方法**：

- 布局方法应为**纯计算函数**，只读取子元素期望值，不能修改影响布局的属性。
- 如果确实需要根据最终尺寸调整子元素，使用 `Dispatcher.BeginInvoke` 延迟执行，但应尽量避免。
- 对于复杂场景，考虑重写 `OnVisualChildrenChanged` 时加一个标志位防止循环。

## 6.3 测量与排列结果不符合预期

**现象**：子元素位置错乱、尺寸异常，或者面板自身尺寸与预期不符。

**排查重点**：

1. **MeasureOverride 返回值的含义**：返回值是面板**期望的尺寸**，而非最终分配尺寸。如果返回 `constraint`（例如 Size(∞, ∞)），可能导致面板尺寸异常。
2. **ArrangeOverride 返回值**：返回值是面板**实际使用的尺寸**，通常直接返回 `arrangeSize`；如果返回不同值，可能导致面板最终尺寸与父容器分配的不一致。
3. **子元素期望尺寸的正确使用**：  
   - 在 Measure 阶段，`child.DesiredSize` 在 `child.Measure` 之后才有效。  
   - 在 Arrange 阶段，仍然可以使用 `child.DesiredSize`，也可以自己计算子元素最终尺寸（但不要小于 0）。
4. **约束的传递**：传给 `child.Measure(constraint)` 的参数应当合理。如果传入 `new Size(double.PositiveInfinity, double.PositiveInfinity)`，子元素按内容无限扩展；如果传入 `new Size(0, 0)`，子元素可能返回零尺寸。

**调试技巧**：在 MeasureOverride 和 ArrangeOverride 中使用 `Debug.WriteLine` 打印关键尺寸信息，结合 [Snoop](https://github.com/snoopwpf/snoopwpf) 工具可视化布局树。

## 6.4 如何系统调试自定义 Panel 的布局过程

**工具与步骤**：

1. **Snoop**：实时查看可视化树与逻辑树、属性值、测量/排列尺寸。右击元素选择 “Del” 可直接看到 `ActualWidth`、`ActualHeight`、`DesiredSize` 等。
2. **Visual Studio Live Visual Tree**：调试运行时打开 “实时可视化树”，可动态检查每个元素的布局信息。
3. **自定义调试输出**：在 MeasureOverride 和 ArrangeOverride 中加入 `System.Diagnostics.Debug.WriteLine`，用缩进区分调用层级。
4. **单元测试**：对自定义 Panel 编写基于 `Window` 的简单测试应用，固定输入验证输出。
5. **查看 `LayoutInformation`**：通过静态类 `System.Windows.Controls.Primitives.LayoutInformation` 获取 `GetLayoutSlot`（排列后的矩形）和 `GetLayoutClip`，帮助确认子元素实际放置位置。

**典型调试代码片段**：

```csharp
protected override Size MeasureOverride(Size constraint)
{
    Debug.WriteLine($"Panel MeasureOverride: constraint={constraint}");
    foreach (UIElement child in InternalChildren)
    {
        child.Measure(constraint);
        Debug.WriteLine($"  Child {child}: DesiredSize={child.DesiredSize}");
    }
    return new Size(0, 0); // 按需计算
}
```

掌握以上排查方法，可以解决绝大部分自定义 Panel 开发中的布局异常。


