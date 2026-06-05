# 004006002_WPF **CheckBox 官方基类定义**（.NET 9 最新源码）

## 一、CheckBox 完整官方类定义（原汁原味）

csharp：

```c#
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace System.Windows.Controls
{
    /// <summary>
    ///     复选框控件，使用户可以在 true、false 和 indeterminate 三种状态之间切换
    /// </summary>
    public class CheckBox : ToggleButton
    {
        //------------------------------------------------------
        // 静态构造函数
        //------------------------------------------------------
        static CheckBox();

        //------------------------------------------------------
        //  构造函数
        //------------------------------------------------------
        public CheckBox();

        //------------------------------------------------------
        // 依赖属性
        //------------------------------------------------------
        /// <summary>
        ///     表示内容的垂直对齐方式
        /// </summary>
        public static readonly DependencyProperty ContentVerticalAlignmentProperty;

        //------------------------------------------------------
        //  公共属性
        //------------------------------------------------------
        /// <summary>
        ///     获取或设置内容的垂直对齐方式
        /// </summary>
        [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
        public VerticalAlignment ContentVerticalAlignment
        {
            get { return (VerticalAlignment)GetValue(ContentVerticalAlignmentProperty); }
            set { SetValue(ContentVerticalAlignmentProperty, value); }
        }
    }
}
```

------

## 二、逐行深度解析（底层原理 + 设计意图）

### 1. 命名空间

csharp：

```c#
namespace System.Windows.Controls
```

- 属于 WPF 基础控件库
- 所有界面控件（Button/TextBox/Panel 等）都在这里

------

### 2. 类声明（最关键）

csharp:

```c#
public class CheckBox : ToggleButton
```

#### ① `public`

- 公共类，任何项目都能直接使用

#### ② `class`

- 标准 C# 类，不是结构体、不是接口

#### ③ **`: ToggleButton`（继承）**

**CheckBox 99% 的功能都来自 ToggleButton！**

#### CheckBox 完整继承链（必须背）

plaintext:

```tex
object
   ↳ DispatcherObject
      ↳ DependencyObject
         ↳ Visual
            ↳ UIElement
               ↳ FrameworkElement
                  ↳ Control
                     ↳ ContentControl
                        ↳ ButtonBase
                           ↳ ToggleButton  ← 核心父类
                              ↳ CheckBox   ← 你要的类
```

------

## 三、CheckBox 到底是什么？

### 官方结论：

**CheckBox = ToggleButton + 复选框默认样式 + 1 个属性**

它**没有自己的状态逻辑、没有自己的选中逻辑、没有自己的事件**。

**ToggleButton 才是真正的基类**。

------

## 四、静态构造函数（底层初始化）

csharp:

```c#
static CheckBox();
```

### 官方内部实现（你看不到但必须知道）

csharp:

```c#
static CheckBox()
{
    // 覆写默认样式
    DefaultStyleKeyProperty.OverrideMetadata(
        typeof(CheckBox),
        new FrameworkPropertyMetadata(typeof(CheckBox)));

    // 水平居中、垂直居中
    VerticalContentAlignmentProperty.OverrideMetadata(
        typeof(CheckBox),
        new FrameworkPropertyMetadata(VerticalAlignment.Center));

    // 允许聚焦
    FocusableProperty.OverrideMetadata(
        typeof(CheckBox),
        new FrameworkPropertyMetadata(true));
}
```

### 作用：

1. 给 CheckBox 应用**默认复选框样式**
2. 设置内容**垂直居中**
3. 设置可点击、可聚焦
4. 注册唯一的依赖属性 `ContentVerticalAlignment`

------

## 五、实例构造函数

csharp:

```c#
public CheckBox();
```

- 初始化控件
- 无特殊逻辑
- 全部继承父类

------

## 六、CheckBox **唯一自己定义的依赖属性**

### `ContentVerticalAlignmentProperty`

csharp:

```c#
public static readonly DependencyProperty ContentVerticalAlignmentProperty;
```

### 官方注册代码：

csharp:

```c#
ContentVerticalAlignmentProperty =
    DependencyProperty.Register(
        nameof(ContentVerticalAlignment),
        typeof(VerticalAlignment),
        typeof(CheckBox),
        new FrameworkPropertyMetadata(
            VerticalAlignment.Center,
            FrameworkPropertyMetadataOptions.AffectsRender));
```

### 作用：

**控制复选框右侧文字的垂直对齐**

- Center（默认）
- Top
- Bottom
- Stretch

### 这是 CheckBox 唯一的 “自有功能”

除此之外，**所有功能全部来自 ToggleButton**。

------

## 七、公共属性包装器

csharp:

```c#
public VerticalAlignment ContentVerticalAlignment
{
    get { return (VerticalAlignment)GetValue(ContentVerticalAlignmentProperty); }
    set { SetValue(ContentVerticalAlignmentProperty, value); }
}
```

- 标准依赖属性 CLR 包装
- 供 XAML 和 C# 使用

------

## 八、CheckBox 从 ToggleButton 继承了哪些核心功能？

这才是 CheckBox 的**灵魂**：

### 1. 三态支持

csharp:

```c#
public bool? IsChecked { get; set; }
public bool IsThreeState { get; set; }
```

- true → 勾选
- false → 未勾选
- null → 不确定 / 半选

### 2. 核心事件

csharp:

```c#
event RoutedEventHandler Checked;
event RoutedEventHandler Unchecked;
event RoutedEventHandler Indeterminate;
```

### 3. 内容显示

csharp:

```c#
public object Content { get; set; }
```

放文字、图片、布局都行。

### 4. 命令、点击、样式、模板

全部来自父类。

------

## 九、CheckBox 类定义总结（极简版）

### **CheckBox 是一个只包含 1 个属性的 ToggleButton 子类。**

它的官方类定义只有 4 样东西：

1. 继承 ToggleButton
2. 静态构造（设置样式）
3. 一个依赖属性：ContentVerticalAlignment
4. 一个实例构造

**没有自己的状态逻辑**

**没有自己的点击逻辑**

**没有自己的三态逻辑**

**没有自己的事件逻辑**

全部来自父类 **ToggleButton**。

------

## 十、一张图看懂 CheckBox 家族结构

plaintext:

```tex
ToggleButton（三态开关基类）
   ├── RadioButton（单选，互斥）
   └── CheckBox（复选，三态，多了Content垂直对齐）
```

------

## 十一、你可能想问的终极问题

### 问：CheckBox 为什么这么简单？

### 答：

**WPF 设计原则：控件 = 数据逻辑 + 外观模板**

- **ToggleButton：负责状态逻辑（三态）**
- **CheckBox：只负责外观（复选框）+ 1 个对齐属性**

这就是 WPF 强大的**分离式架构**。

------

## 十二、最终总结（面试 / 考试必背）

1. **CheckBox 直接继承自 ToggleButton**
2. **ToggleButton 继承自 ButtonBase**
3. **CheckBox 只有 1 个自己的属性：ContentVerticalAlignment**
4. **选中、三态、点击、事件、命令 全部来自父类 ToggleButton**
5. **CheckBox = 三态按钮 + 复选框样式**
6. **它是密封功能类，不可再被继承（非抽象）**