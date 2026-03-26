# 003004003_在代码中设置UniformGrid控件的属性

​	在 **C# 后台代码** 中设置 `UniformGrid` 的属性**非常简单**，因为它的核心属性（`Columns`/`Rows`/`FirstColumn`）都是**直接可赋值的普通 CLR 属性**，无需复杂的依赖属性操作。

## 前置知识

1. 命名空间：`UniformGrid` 属于 `System.Windows.Controls`

2. 核心属性（代码中直接赋值）：

   - `Columns`：列数 → `int`
   - `Rows`：行数 → `int`
   - `FirstColumn`：起始列 → `int`
   - 通用布局属性：`Margin`/`Background`/`Width`/`Height` 等

   

------

## 场景 1：纯代码动态创建 UniformGrid（完全后台生成）

​	适用：动态生成布局、无 XAML 定义的场景

### 完整代码（WPF 窗口）

#### 	XAML（空窗口，仅作为容器）

xaml:

```xaml
<Window x:Class="UniformGridCodeDemo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="代码设置UniformGrid" Height="300" Width="300">
    <!-- 后台动态创建控件，这里为空 -->
    <Grid x:Name="RootGrid"/>
</Window>
```

#### C# 后台代码（逐行解析）

csharp:

```c#
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UniformGridCodeDemo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // 1. 创建 UniformGrid 实例
            UniformGrid uniformGrid = new UniformGrid();

            // ===================== 核心属性设置 =====================
            // 2. 设置 列数 = 3
            uniformGrid.Columns = 3;
            // 3. 设置 行数 = 3
            uniformGrid.Rows = 3;
            // 4. 设置 起始列（空出前1列，可选）
            // uniformGrid.FirstColumn = 1;

            // ===================== 通用样式属性设置 =====================
            // 设置背景色
            uniformGrid.Background = Brushes.LightGray;
            // 设置外边距
            uniformGrid.Margin = new Thickness(10);
            // 设置宽高
            uniformGrid.Width = 250;
            uniformGrid.Height = 250;

            // ===================== 动态添加子元素 =====================
            for (int i = 1; i <= 9; i++)
            {
                Button btn = new Button();
                btn.Content = i;
                btn.FontSize = 18;
                // 添加到 UniformGrid
                uniformGrid.Children.Add(btn);
            }

            // 5. 将 UniformGrid 添加到窗口根容器
            RootGrid.Children.Add(uniformGrid);
        }
    }
}
```

------

# 场景 2：XAML 中定义，后台修改属性（最常用）

​	适用：XAML 已写好`UniformGrid`，后台**动态修改属性**（如切换行列数）

### 步骤

1. XAML 中给`UniformGrid`设置 `x:Name`（命名）
2. 后台直接用**名称。属性**赋值

### 完整代码

xaml:

```xaml
<Window x:Class="UniformGridCodeDemo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="修改UniformGrid属性" Height="300" Width="400">
    <Grid>
        <!-- 命名：ugDemo，后台通过这个名称访问 -->
        <UniformGrid x:Name="ugDemo" Margin="10" Background="LightBlue">
            <Button Content="A"/>
            <Button Content="B"/>
            <Button Content="C"/>
            <Button Content="D"/>
        </UniformGrid>

        <!-- 点击按钮，修改UniformGrid属性 -->
        <Button Content="修改为2列" Click="Btn_Change" VerticalAlignment="Bottom" Margin="10"/>
    </Grid>
</Window>
```

#### C# 后台代码

csharp:

```c#
using System.Windows;

namespace UniformGridCodeDemo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // 按钮点击事件：动态修改属性
        private void Btn_Change(object sender, RoutedEventArgs e)
        {
            // 直接用 x:Name 访问，修改属性
            ugDemo.Columns = 2;    // 修改列数为2
            ugDemo.Rows = 2;       // 修改行数为2
            ugDemo.FirstColumn = 0;// 重置起始列
            ugDemo.Background = Brushes.LightGreen; // 修改背景
        }
    }
}
```

------

## 常用属性代码速查表

| 功能           | XAML 写法          | C# 代码写法                               |
| :------------- | :----------------- | :---------------------------------------- |
| 设置列数       | `Columns="3"`      | `uniformGrid.Columns = 3;`                |
| 设置行数       | `Rows="2"`         | `uniformGrid.Rows = 2;`                   |
| 设置起始列     | `FirstColumn="1"`  | `uniformGrid.FirstColumn = 1;`            |
| 设置背景       | `Background="Red"` | `uniformGrid.Background = Brushes.Red;`   |
| 设置边距       | `Margin="10"`      | `uniformGrid.Margin = new Thickness(10);` |
| 清空所有子元素 | -                  | `uniformGrid.Children.Clear();`           |
| 添加子元素     | -                  | `uniformGrid.Children.Add(控件);`         |

------

## 核心注意事项

1. **无需调用刷新方法**

   

   修改 `Columns`/`Rows`后，WPF 布局系统会自动重绘，不用写额外代码。

2. **FirstColumn 仅对第一行生效**

   只空出第一行的前列，第二行自动从头填充。

3. **子元素管理**

   通过 `Children` 集合添加 / 删除 / 清空元素，和所有`Panel`用法一致。

------

# 总结

1. **代码设置属性 = 直接赋值**：`控件名.Columns = 数字`

2. 两种用法：

   - 纯代码：`new UniformGrid()` → 赋值 → 添加到窗口
   - 已有控件：`x:Name` → 直接修改属性

   

3. 语法极简，是 WPF 中最容易代码操控的布局面板！