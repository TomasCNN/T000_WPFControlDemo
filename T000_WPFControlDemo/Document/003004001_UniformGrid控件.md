# 003004001_UniformGrid控件

​	`UniformGrid` 是 WPF **内置的极简均匀网格布局面板**，继承自 `Panel` 基类，是 `Grid` 的**轻量化兄弟控件**。

​	它的核心特点：**自动生成等宽、等高的单元格网格**，**无需手动定义行 / 列**，子元素会自动按顺序填充，专门用于**等尺寸元素的快速布局**。

------

## 一、核心基础信息

### 1. 继承链

```markdown
Panel 基类
   ↓
UniformGrid（密封类，可直接实例化）
```

### 2. 官方类定义

csharp：

```c#
public class UniformGrid : System.Windows.Controls.Panel
```

### 3. 核心定位

​	**均匀等分网格容器**：自动将所有子元素放在**大小完全相同**的单元格中，是实现九宫格、计算器按键、图标墙的最优选择。

------

## 二、核心功能（与 Grid 最大区别）

| 特性        | UniformGrid                                    | Grid                   |
| :---------- | :--------------------------------------------- | :--------------------- |
| 行 / 列定义 | **自动生成**，无需手动写 Row/ColumnDefinitions | 必须手动定义行列       |
| 单元格大小  | **全部等宽等高**（强制均匀）                   | 可单独设置每行每列尺寸 |
| 跨行跨列    | **不支持**                                     | 完美支持               |
| 使用复杂度  | 极简（1-2 个属性搞定）                         | 复杂（支持高级布局）   |
| 适用场景    | 等尺寸元素、快速布局                           | 复杂表单、界面分区     |

------

## 三、核心属性（仅 4 个，全部必学）

​	UniformGrid 没有多余属性，配置极简：

| 属性名          | 类型                | 默认值 | 作用                                 |
| :-------------- | :------------------ | :----- | :----------------------------------- |
| **Rows**        | int                 | 0      | 固定**行数**，0 = 自动计算           |
| **Columns**     | int                 | 0      | 固定**列数**，0 = 自动计算           |
| **FirstColumn** | int                 | 0      | 起始列索引（空出前面的列，用于占位） |
| **Children**    | UIElementCollection | -      | 继承自 Panel，子元素集合             |

### 工作规则（自动布局逻辑）

1. **不设置 Rows/Columns**：自动根据子元素数量，生成**最接近正方形**的网格（如 9 个元素 = 3×3）；
2. **只设置 Rows**：列数自动计算（如 Rows=2，5 个子元素 = 2 行 3 列）；
3. **只设置 Columns**：行数自动计算（如 Columns=3，7 个子元素 = 3 行 3 列）；
4. **同时设置 Rows+Columns**：强制使用指定行列数，多余单元格为空。

------

## 四、标准使用方法

1. 直接声明 `<UniformGrid>` 容器；
2. 可选设置 `Rows`/`Columns` 固定网格大小；
3. 直接添加子元素，**无需设置任何附加属性**（自动排列）；
4. 完成布局。

------

## 五、实战代码实例（可直接复制运行）

### 实例 1：基础九宫格（最常用：计算器 / 图标）

xaml：

```xaml
<Window x:Class="UniformGridDemo.UniformGridWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="九宫格 UniformGrid" Height="300" Width="300">
    <!-- 3行3列 均匀网格，所有按钮等大 -->
    <UniformGrid Rows="3" Columns="3" Background="LightGray">
        <!-- 子元素自动按顺序填充，无需设置行列 -->
        <Button Content="1"/>
        <Button Content="2"/>
        <Button Content="3"/>
        <Button Content="4"/>
        <Button Content="5"/>
        <Button Content="6"/>
        <Button Content="7"/>
        <Button Content="8"/>
        <Button Content="9"/>
    </UniformGrid>
</Window>
```

✅ 效果：9 个按钮**完全等大**，自动填满 3×3 网格。

------

### 实例 2：自动网格（不设行列，智能适配）

xaml:

```xaml
<UniformGrid Background="LightBlue">
    <!-- 6个子元素 → 自动生成 2行3列 均匀网格 -->
    <Border Background="White" Margin="2"/>
    <Border Background="White" Margin="2"/>
    <Border Background="White" Margin="2"/>
    <Border Background="White" Margin="2"/>
    <Border Background="White" Margin="2"/>
    <Border Background="White" Margin="2"/>
</UniformGrid>
```

✅ 效果：自动计算行列，所有单元格等大。

------

### 实例 3：FirstColumn 空出起始列（占位效果）

xaml:

```xaml
<!-- 2行4列，从第2列开始填充（FirstColumn=2，空出前2列） -->
<UniformGrid Rows="2" Columns="4" FirstColumn="2" Background="LightGray">
    <Button Content="A"/>
    <Button Content="B"/>
    <Button Content="C"/>
</UniformGrid>
```

✅ 效果：第一行前 2 列空白，元素从第 3 列开始排列。

------

### 实例 4：图片墙（等尺寸图片布局）

xaml:

```xaml
<UniformGrid Columns="2" Margin="10">
    <Image Source="pic1.jpg" Stretch="Fill" Margin="2"/>
    <Image Source="pic2.jpg" Stretch="Fill" Margin="2"/>
    <Image Source="pic3.jpg" Stretch="Fill" Margin="2"/>
    <Image Source="pic4.jpg" Stretch="Fill" Margin="2"/>
</UniformGrid>
```

✅ 效果：2 列自动分行，所有图片区域大小完全一致。

------

## 六、逐句解析核心代码（实例 1）

xaml:

```c#
<UniformGrid Rows="3" Columns="3" Background="LightGray">
```

- 声明均匀网格面板；
- `Rows="3"`：固定 3 行；
- `Columns="3"`：固定 3 列；
- 背景色浅灰。

xaml:

```xaml
<Button Content="1"/>
```

- 直接添加按钮，**不用写 Grid.Row/Column**；
- UniformGrid 自动按**从左到右、从上到下**顺序排列。

xaml:

```xaml
</UniformGrid>
```

- 闭合容器，布局完成。

------

## 七、适用场景 & 不适用场景

### ✅ 推荐使用

1. 计算器按键、拨号键盘；
2. 九宫格、图标网格、图片墙；
3. 等尺寸按钮组、标签组；
4. 快速简易的均匀布局。

### ❌ 不推荐使用

1. 表单、复杂界面分区（用 Grid）；
2. 需要不同大小单元格的布局；
3. 需要跨行 / 跨列的布局。

------

## 八、总结（核心记忆点）

1. **基类**：`UniformGrid` 继承自 `Panel`，和 Grid 平级；
2. **核心优势**：**自动等大单元格、无需定义行列、代码极简**；
3. **关键属性**：`Rows`（行数）、`Columns`（列数）、`FirstColumn`（起始列）；
4. **排列规则**：从左到右、从上到下自动填充；
5. **一句话**：**等尺寸元素选 UniformGrid，复杂布局选 Grid**。