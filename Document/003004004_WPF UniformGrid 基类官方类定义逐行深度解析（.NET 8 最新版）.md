# 003004004_WPF UniformGrid 基类官方类定义逐行深度解析（.NET 8 最新版）

基于 **.NET 8 官方开源源码** 完整解析，包含所有公共 / 受保护成员、特性、设计意图和工业场景应用。`UniformGrid`是 WPF**唯一的均匀网格布局容器**，所有单元格大小完全相同，无需手动定义行和列，自动根据子元素数量计算最佳行列数，是工业上位机中实现**IO 状态矩阵、按钮矩阵、设备状态网格、数字键盘**的核心控件。

------

## 一、UniformGrid 在 WPF 类层次结构中的位置

plaintext:

```tex
System.Object
  ↳ System.Windows.Threading.DispatcherObject
    ↳ System.Windows.DependencyObject
      ↳ System.Windows.Media.Visual
        ↳ System.Windows.UIElement
          ↳ System.Windows.FrameworkElement
            ↳ System.Windows.Controls.Panel  ← 所有布局容器的基类
              ↳ System.Windows.Controls.Primitives.UniformGrid  ← 我们今天的主角
```

**核心设计意义**：

- 实现**均匀网格模型**：所有单元格大小完全相同，无需手动定义`RowDefinitions`和`ColumnDefinitions`
- 自动计算行列数：根据子元素数量和可用空间自动计算最佳行列数
- 轻量级高性能：布局逻辑简单，渲染效率高
- 易于使用：代码量极少，几行代码即可实现规则的矩阵布局
- 工业场景价值：非常适合展示规则排列的元素集合，如 IO 状态、按钮矩阵、设备状态网格

**重要说明**：

- 位于`System.Windows.Controls.Primitives`命名空间下，而不是`System.Windows.Controls`
- 不支持 UI 虚拟化，大数据量场景应使用`DataGrid`或`VirtualizingStackPanel`
- 所有单元格大小完全相同，不支持跨行或跨列

------

## 二、完整官方类定义（.NET 8 源码级）

csharp:

```c#
using System.Windows.Automation.Peers;
using System.Windows.Media;
using System.Windows.Markup;

namespace System.Windows.Controls.Primitives
{
    /// <summary>
    /// 表示一个所有单元格大小相同的均匀网格布局容器
    /// </summary>
    /// <remarks>
    /// UniformGrid 会自动计算最佳的行列数，使所有子元素均匀排列。
    /// 可以通过 Rows 和 Columns 属性手动指定行列数。
    /// 所有单元格的大小完全相同，不支持跨行或跨列。
    /// </remarks>
    [ContentProperty("Children")]
    [Localizability(LocalizationCategory.None)]
    public class UniformGrid : Panel
    {
        // ==============================================
        // 依赖属性定义（UniformGrid特有）
        // ==============================================
        public static readonly DependencyProperty RowsProperty;
        public static readonly DependencyProperty ColumnsProperty;
        public static readonly DependencyProperty FirstColumnProperty;

        // ==============================================
        // 静态构造函数
        // ==============================================
        static UniformGrid()
        {
            // 注册Rows依赖属性
            RowsProperty = DependencyProperty.Register(
                nameof(Rows),
                typeof(int),
                typeof(UniformGrid),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange),
                new ValidateValueCallback(IsValidRowsColumns));

            // 注册Columns依赖属性
            ColumnsProperty = DependencyProperty.Register(
                nameof(Columns),
                typeof(int),
                typeof(UniformGrid),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange),
                new ValidateValueCallback(IsValidRowsColumns));

            // 注册FirstColumn依赖属性
            FirstColumnProperty = DependencyProperty.Register(
                nameof(FirstColumn),
                typeof(int),
                typeof(UniformGrid),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange),
                new ValidateValueCallback(IsValidFirstColumn));

            // 重写默认样式键
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(UniformGrid),
                new FrameworkPropertyMetadata(typeof(UniformGrid)));
        }

        // ==============================================
        // 公共构造函数
        // ==============================================
        public UniformGrid();

        // ==============================================
        // 公共属性
        // ==============================================
        [Bindable(true)]
        [Category("Layout")]
        public int Rows { get; set; }

        [Bindable(true)]
        [Category("Layout")]
        public int Columns { get; set; }

        [Bindable(true)]
        [Category("Layout")]
        public int FirstColumn { get; set; }

        // ==============================================
        // 受保护内部属性（逻辑导航）
        // ==============================================
        protected internal override bool HasLogicalOrientation { get; }
        protected internal override Orientation LogicalOrientation { get; }

        // ==============================================
        // 受保护方法（布局核心）
        // ==============================================
        protected override AutomationPeer OnCreateAutomationPeer();
        protected override Size MeasureOverride(Size constraint);
        protected override Size ArrangeOverride(Size arrangeSize);
        private static bool IsValidRowsColumns(object value);
        private static bool IsValidFirstColumn(object value);
    }
}
```

------

## 三、类级特性逐行解析

### 1. `[ContentProperty("Children")]`

csharp:

```c#
[ContentProperty("Children")]
```

- **作用**：指定控件的默认内容属性
- **设计意图**：允许在 XAML 中直接编写子元素，无需显式指定`UniformGrid.Children`标签
- **核心意义**：极大简化 XAML 代码，实现 "零配置" 矩阵布局
- **工业场景价值**：快速编写 IO 状态矩阵、按钮矩阵等规则布局

### 2. `[Localizability(LocalizationCategory.None)]`

csharp:

```c#
[Localizability(LocalizationCategory.None)]
```

- **作用**：本地化特性，告诉本地化工具该类不需要本地化
- **设计意图**：UniformGrid 是纯布局容器，没有需要本地化的文本内容

------

## 四、静态构造函数解析（核心初始化逻辑）

静态构造函数是 UniformGrid 最关键的部分，负责所有核心依赖属性的注册。

### 1. `RowsProperty` 注册

csharp:

```c#
RowsProperty = DependencyProperty.Register(
    nameof(Rows),
    typeof(int),
    typeof(UniformGrid),
    new FrameworkPropertyMetadata(
        0,
        FrameworkPropertyMetadataOptions.AffectsMeasure |
        FrameworkPropertyMetadataOptions.AffectsArrange),
    new ValidateValueCallback(IsValidRowsColumns));
```

- **类型**：`int`
- **默认值**：`0`（自动计算行数）
- **元数据标志**：`AffectsMeasure`和`AffectsArrange`（属性变化会触发重新测量和排列）
- **验证回调**：`IsValidRowsColumns`，确保值大于等于 0

### 2. `ColumnsProperty` 注册

csharp:

```c#
ColumnsProperty = DependencyProperty.Register(
    nameof(Columns),
    typeof(int),
    typeof(UniformGrid),
    new FrameworkPropertyMetadata(
        0,
        FrameworkPropertyMetadataOptions.AffectsMeasure |
        FrameworkPropertyMetadataOptions.AffectsArrange),
    new ValidateValueCallback(IsValidRowsColumns));
```

- **类型**：`int`
- **默认值**：`0`（自动计算列数）
- **元数据标志**：与 Rows 相同
- **验证回调**：`IsValidRowsColumns`

### 3. `FirstColumnProperty` 注册

csharp:

```c#
FirstColumnProperty = DependencyProperty.Register(
    nameof(FirstColumn),
    typeof(int),
    typeof(UniformGrid),
    new FrameworkPropertyMetadata(
        0,
        FrameworkPropertyMetadataOptions.AffectsMeasure |
        FrameworkPropertyMetadataOptions.AffectsArrange),
    new ValidateValueCallback(IsValidFirstColumn));
```

- **类型**：`int`
- **默认值**：`0`（第一行第一个元素从第 0 列开始）
- **元数据标志**：与 Rows 相同
- **验证回调**：`IsValidFirstColumn`，确保值大于等于 0

------

## 五、核心依赖属性逐行解析

### 1. `Rows` 和 `Columns` 属性（自动计算核心）

csharp:

```c#
[Bindable(true)]
[Category("Layout")]
public int Rows { get; set; }

[Bindable(true)]
[Category("Layout")]
public int Columns { get; set; }
```

#### 逐句解析：

- **`[Category("Layout")]`**：在属性窗口中归类到 "布局" 组

- **默认值**：`0`（表示自动计算）

- **核心作用**：手动指定网格的行数和列数

- **自动计算规则**（当值为 0 时）：

  1. 如果只设置了`Rows`，则`Columns = (子元素数量 + Rows - 1) / Rows`（向上取整）
  2. 如果只设置了`Columns`，则`Rows = (子元素数量 + Columns - 1) / Columns`（向上取整）
  3. 如果都没设置，则计算最接近正方形的行列数：`Columns = (int)Math.Ceiling(Math.Sqrt(子元素数量))`，`Rows = (子元素数量 + Columns - 1) / Columns`

  

#### 工业场景最佳实践：

- **优先使用自动计算**：大多数情况下让 UniformGrid 自动计算最佳行列数
- **固定列数**：工业界面通常固定列数，让行数自动增长，如`Columns="4"`
- **避免同时固定行列数**：如果子元素数量超过`Rows*Columns`，超出的子元素会被裁剪

#### 示例：

xaml:

```
<!-- 自动计算行列数（最接近正方形） -->
<UniformGrid>
    <Button Content="1"/>
    <Button Content="2"/>
    <Button Content="3"/>
    <Button Content="4"/>
    <Button Content="5"/>
</UniformGrid>

<!-- 固定3列，行数自动计算 -->
<UniformGrid Columns="3">
    <Button Content="1"/>
    <Button Content="2"/>
    <Button Content="3"/>
    <Button Content="4"/>
    <Button Content="5"/>
</UniformGrid>
```

### 2. `FirstColumn` 属性（第一行偏移）

csharp:

```c#
[Bindable(true)]
[Category("Layout")]
public int FirstColumn { get; set; }
```

#### 逐句解析：

- **默认值**：`0`（第一行第一个元素从第 0 列开始）

- **核心作用**：设置第一行第一个元素的列偏移量，使第一行前面留出空单元格

- **工业场景应用**：

  - 日历控件：第一行从对应星期几的列开始
  - 矩阵对齐：使矩阵与其他元素对齐
  - 分页显示：第一页显示部分元素，后面的页从第 0 列开始

  

#### 示例：

xaml:

```xaml
<!-- 第一行从第2列开始，前面留2个空单元格 -->
<UniformGrid Columns="4" FirstColumn="2">
    <Button Content="1"/>
    <Button Content="2"/>
    <Button Content="3"/>
    <Button Content="4"/>
    <Button Content="5"/>
</UniformGrid>
```

------

## 六、受保护内部属性逐行解析（逻辑导航）

### 1. `HasLogicalOrientation` 属性

csharp:

```c#
protected internal override bool HasLogicalOrientation { get; }
```

- **官方源码实现**：返回`false`
- **核心作用**：告诉 WPF 框架，**这个面板没有单一的逻辑排列方向**
- **设计意图**：UniformGrid 是二维网格布局，没有明确的线性逻辑方向，因此不支持简单的上下左右箭头导航

### 2. `LogicalOrientation` 属性

csharp:

```c#
protected internal override Orientation LogicalOrientation { get; }
```

- **官方源码实现**：抛出`NotSupportedException`
- **核心作用**：由于没有单一逻辑方向，因此不支持此属性
- **注意**：UniformGrid 的键盘导航是二维的，按 Tab 键按子元素顺序导航，箭头键在网格内二维导航

------

## 七、受保护方法逐行解析（布局核心）

UniformGrid 重写了`Panel`基类的两个核心布局方法，实现了均匀网格的布局逻辑。

### 1. `MeasureOverride()` 方法（测量阶段）

csharp:

```c#
protected override Size MeasureOverride(Size constraint);
```

- **触发时机**：当 UniformGrid 需要测量自身大小时调用

- **官方源码实现（简化版）**：

  csharp:

  ```c#
  protected override Size MeasureOverride(Size constraint)
  {
      // 1. 计算实际的行数和列数
      UpdateComputedValues();
      
      // 2. 计算每个单元格的可用大小
      Size cellSize = new Size(
          constraint.Width / _columns,
          constraint.Height / _rows);
      
      // 3. 测量所有子元素，给每个子元素分配相同的单元格大小
      foreach (UIElement child in InternalChildren)
      {
          child.Measure(cellSize);
      }
      
      // 4. 返回总大小：行数*单元格高度 × 列数*单元格宽度
      return new Size(
          _columns * cellSize.Width,
          _rows * cellSize.Height);
  }
  ```

  

- **核心逻辑**：

  1. 计算实际的行数和列数（自动计算或使用手动指定的值）
  2. 计算每个单元格的大小：总可用大小除以行列数
  3. 给每个子元素分配相同的单元格大小进行测量
  4. 返回总大小

  

### 2. `ArrangeOverride()` 方法（排列阶段）

csharp:

```c#
protected override Size ArrangeOverride(Size arrangeSize);
```

- **触发时机**：当 UniformGrid 需要排列子元素时调用

- **官方源码实现（简化版）**：

  csharp:

  ```c#
  protected override Size ArrangeOverride(Size arrangeSize)
  {
      // 1. 计算每个单元格的最终大小
      Size cellSize = new Size(
          arrangeSize.Width / _columns,
          arrangeSize.Height / _rows);
      
      // 2. 初始化当前位置：第一行从FirstColumn开始
      int currentColumn = FirstColumn;
      int currentRow = 0;
      
      // 3. 遍历所有子元素，按顺序排列
      foreach (UIElement child in InternalChildren)
      {
          // 计算子元素的排列矩形
          Rect childRect = new Rect(
              currentColumn * cellSize.Width,
              currentRow * cellSize.Height,
              cellSize.Width,
              cellSize.Height);
          
          // 排列子元素
          child.Arrange(childRect);
          
          // 移动到下一个单元格
          currentColumn++;
          if (currentColumn >= _columns)
          {
              currentColumn = 0;
              currentRow++;
          }
      }
      
      // 返回最终大小
      return arrangeSize;
  }
  ```

  

- **核心逻辑**：

  1. 计算每个单元格的最终大小
  2. 从第一行的`FirstColumn`位置开始
  3. 按顺序排列每个子元素，从左到右，从上到下
  4. 每排列完一行，移动到下一行的第 0 列

  

------

## 八、UniformGrid 核心工作原理

### 8.1 完整布局流程

1. **父容器调用 UniformGrid 的 Measure 方法**，传入可用大小

2. **UniformGrid 调用 MeasureOverride 方法**：

   - 计算实际的行数和列数
   - 计算每个单元格的大小
   - 测量所有子元素
   - 返回总测量大小

   

3. **父容器调用 UniformGrid 的 Arrange 方法**，传入最终大小

4. **UniformGrid 调用 ArrangeOverride 方法**：

   - 计算每个单元格的最终大小
   - 从第一行的 FirstColumn 位置开始，按顺序排列所有子元素
   - 返回最终大小

   

### 8.2 自动计算行列数算法

csharp:

```c#
private void UpdateComputedValues()
{
    int childCount = InternalChildren.Count;
    
    // 如果没有子元素，行列数都为0
    if (childCount == 0)
    {
        _rows = 0;
        _columns = 0;
        return;
    }
    
    // 1. 处理手动指定的情况
    if (Rows > 0 && Columns > 0)
    {
        _rows = Rows;
        _columns = Columns;
        return;
    }
    
    if (Rows > 0)
    {
        // 只指定了行数，自动计算列数
        _rows = Rows;
        _columns = (childCount + Rows - 1) / Rows; // 向上取整
        return;
    }
    
    if (Columns > 0)
    {
        // 只指定了列数，自动计算行数
        _columns = Columns;
        _rows = (childCount + Columns - 1) / Columns; // 向上取整
        return;
    }
    
    // 2. 都没指定，计算最接近正方形的行列数
    _columns = (int)Math.Ceiling(Math.Sqrt(childCount));
    _rows = (childCount + _columns - 1) / _columns;
}
```

### 8.3 与其他网格布局的本质区别

| 布局容器        | 单元格大小           | 行列定义                                     | 跨行跨列 | 适用场景                    |
| :-------------- | :------------------- | :------------------------------------------- | :------- | :-------------------------- |
| **UniformGrid** | 所有单元格大小相同   | 自动计算或手动指定                           | ❌ 不支持 | 规则矩阵、IO 状态、按钮矩阵 |
| **Grid**        | 每个单元格大小可不同 | 手动定义 RowDefinitions 和 ColumnDefinitions | ✅ 支持   | 复杂布局、表单、数据表格    |
| **WrapPanel**   | 子元素大小可不同     | 自动换行                                     | ❌ 不支持 | 设备图标列表、卡片墙        |

------

## 九、工业上位机典型应用实例

UniformGrid 是工业上位机开发中非常常用的布局容器，特别适合展示规则排列的元素集合。

### 实例 1：8x8 IO 输入输出状态矩阵

这是工业界面最常见的应用，展示 PLC 的 IO 点状态。

xaml:

```xaml
<GroupBox Header="输入状态" Margin="10">
    <UniformGrid Columns="8" Margin="10">
        <!-- 8x8=64个IO点 -->
        <Ellipse Width="20" Height="20" Fill="#4CAF50" Margin="2"/>
        <Ellipse Width="20" Height="20" Fill="#4CAF50" Margin="2"/>
        <Ellipse Width="20" Height="20" Fill="#F44336" Margin="2"/>
        <Ellipse Width="20" Height="20" Fill="#4CAF50" Margin="2"/>
        <Ellipse Width="20" Height="20" Fill="#4CAF50" Margin="2"/>
        <Ellipse Width="20" Height="20" Fill="#F44336" Margin="2"/>
        <Ellipse Width="20" Height="20" Fill="#4CAF50" Margin="2"/>
        <Ellipse Width="20" Height="20" Fill="#4CAF50" Margin="2"/>
        
        <!-- 更多IO点... -->
    </UniformGrid>
</GroupBox>
```

### 实例 2：3x4 数字键盘

xaml:

```xaml
<Border Background="White" BorderBrush="#E0E0E0" BorderThickness="1" CornerRadius="4" Padding="10">
    <UniformGrid Columns="3" Rows="4" Margin="2">
        <Button Content="1" Height="40" Margin="2"/>
        <Button Content="2" Height="40" Margin="2"/>
        <Button Content="3" Height="40" Margin="2"/>
        <Button Content="4" Height="40" Margin="2"/>
        <Button Content="5" Height="40" Margin="2"/>
        <Button Content="6" Height="40" Margin="2"/>
        <Button Content="7" Height="40" Margin="2"/>
        <Button Content="8" Height="40" Margin="2"/>
        <Button Content="9" Height="40" Margin="2"/>
        <Button Content="." Height="40" Margin="2"/>
        <Button Content="0" Height="40" Margin="2"/>
        <Button Content="←" Height="40" Margin="2"/>
    </UniformGrid>
</Border>
```

### 实例 3：设备状态网格

xaml:

```xaml
<ScrollViewer VerticalScrollBarVisibility="Auto">
    <UniformGrid Columns="4" Margin="10">
        <!-- 设备1 -->
        <Border Background="White" BorderBrush="#E0E0E0" BorderThickness="1" CornerRadius="4" Margin="5" Padding="10">
            <StackPanel HorizontalAlignment="Center">
                <Ellipse Width="30" Height="30" Fill="#4CAF50" Margin="0 0 0 10">
                    <Ellipse.Effect>
                        <DropShadowEffect ShadowDepth="0" BlurRadius="5" Color="#4CAF50" Opacity="0.7"/>
                    </Ellipse.Effect>
                </Ellipse>
                <TextBlock Text="设备1" FontWeight="Bold" HorizontalAlignment="Center"/>
                <TextBlock Text="运行中" FontSize="12" Foreground="#4CAF50" HorizontalAlignment="Center"/>
            </StackPanel>
        </Border>

        <!-- 设备2 -->
        <Border Background="White" BorderBrush="#E0E0E0" BorderThickness="1" CornerRadius="4" Margin="5" Padding="10">
            <StackPanel HorizontalAlignment="Center">
                <Ellipse Width="30" Height="30" Fill="#F44336" Margin="0 0 0 10">
                    <Ellipse.Effect>
                        <DropShadowEffect ShadowDepth="0" BlurRadius="5" Color="#F44336" Opacity="0.7"/>
                    </Ellipse.Effect>
                </Ellipse>
                <TextBlock Text="设备2" FontWeight="Bold" HorizontalAlignment="Center"/>
                <TextBlock Text="报警中" FontSize="12" Foreground="#F44336" HorizontalAlignment="Center"/>
            </StackPanel>
        </Border>

        <!-- 更多设备... -->
    </UniformGrid>
</ScrollViewer>
```

------

## 十、最佳实践与常见问题（工业场景必看）

### 10.1 最佳实践

1. **优先使用自动计算**：大多数情况下让 UniformGrid 自动计算最佳行列数

2. **固定列数**：工业界面通常固定列数，让行数自动增长，如`Columns="4"`

3. **为子元素设置 Margin**：为子元素设置合适的 Margin，避免元素之间过于拥挤

4. **配合 ScrollViewer 使用**：当子元素数量较多时，使用 ScrollViewer 包裹 UniformGrid，提供滚动功能

5. **避免大数据量使用**：UniformGrid 不支持 UI 虚拟化，子元素数量超过 100 个时会出现性能问题，此时应使用 DataGrid

6. **设置子元素为 Stretch**：让子元素占满整个单元格，使布局更加整齐

   xaml:

   ```xaml
   <UniformGrid>
       <Button HorizontalAlignment="Stretch" VerticalAlignment="Stretch" Content="1"/>
       <Button HorizontalAlignment="Stretch" VerticalAlignment="Stretch" Content="2"/>
   </UniformGrid>
   ```

   

7. **工业界面保持简洁**：避免在 UniformGrid 中放置过于复杂的子元素，保持界面清晰易读

### 10.2 常见问题与解决方案

#### 问题 1：子元素大小不一致

**原因**：子元素没有设置`HorizontalAlignment="Stretch"`和`VerticalAlignment="Stretch"`

**解决方案**：为所有子元素设置这两个属性，让它们占满整个单元格

#### 问题 2：超出的子元素被裁剪

**原因**：同时固定了行数和列数，子元素数量超过了`Rows*Columns`

**解决方案**：只固定一个维度（行或列），让另一个维度自动计算

#### 问题 3：性能问题

**原因**：UniformGrid 不支持 UI 虚拟化，子元素数量过多时会生成所有 UI 容器

**解决方案**：

- 子元素数量超过 100 个时，使用 DataGrid 代替
- 简化子元素的 UI 结构，减少绑定数量
- 避免在子元素中使用复杂的效果和动画

#### 问题 4：第一行对齐问题

**原因**：没有正确设置`FirstColumn`属性

**解决方案**：根据需要设置`FirstColumn`属性，使第一行的元素正确对齐

#### 问题 5：键盘导航不直观

**原因**：UniformGrid 是二维网格，没有单一的逻辑方向

**解决方案**：

- 使用 Tab 键按子元素顺序导航
- 显式设置`TabIndex`属性调整导航顺序
- 复杂导航需求使用 Grid 代替

------

## 十一、官方设计意图总结

微软设计 UniformGrid 的核心目标是：

1. **提供最简单的均匀网格布局**：无需手动定义行和列，几行代码即可实现规则的矩阵布局
2. **自动计算行列数**：根据子元素数量和可用空间自动计算最佳行列数
3. **保持轻量级高性能**：布局逻辑简单，渲染效率高
4. **易于理解和使用**：布局语义最直观，学习成本最低
5. **满足规则矩阵布局需求**：专门针对规则排列的元素集合设计

------

## 总结

`UniformGrid`是 WPF 中唯一的均匀网格布局容器，它的核心特性包括：

- 所有单元格大小完全相同
- 自动计算最佳行列数，无需手动定义
- 支持第一行列偏移
- 轻量级高性能，布局逻辑简单
- 易于使用，代码量极少

在工业上位机开发中，UniformGrid 是实现 IO 状态矩阵、按钮矩阵、设备状态网格、数字键盘的首选控件。掌握它的正确使用方法，可以快速开发出整齐、美观的工业界面。