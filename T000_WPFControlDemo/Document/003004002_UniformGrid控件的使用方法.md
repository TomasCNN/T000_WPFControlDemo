# 003004002_UniformGrid控件的使用方法

​	`UniformGrid` 是**零配置、全自动、等尺寸**的网格布局面板，它是 WPF 中**最简单**的布局容器，专门用于排列**大小完全一致**的元素。

------

## 一、前置基础

### 1. 官方定位

​	继承自 `Panel` 基类，**无需手动定义行 / 列**，自动将所有子元素放入**等宽、等高**的单元格中。

### 2. 核心特点

✅ 所有单元格大小**完全相同**

✅ 子元素**无需附加属性**（不用写 `Grid.Row`）

✅ 自动排列，代码极简

❌ **不支持**跨行、跨列

❌ 不支持自定义单元格尺寸

------

## 二、核心属性（仅 3 个，掌握即可精通）

​	UniformGrid 没有冗余属性，这 3 个是全部配置项：

| 属性名        | 数据类型 | 默认值 | 详细作用                                 |
| :------------ | :------- | :----- | :--------------------------------------- |
| `Columns`     | `int`    | 0      | **固定列数**，0 = 自动计算               |
| `Rows`        | `int`    | 0      | **固定行数**，0 = 自动计算               |
| `FirstColumn` | `int`    | 0      | **起始填充列**（空出前面的列，用于占位） |

------

## 三、核心布局规则（使用的底层逻辑）

​	UniformGrid 会根据你设置的 `Rows`/`Columns`，**自动计算网格**，遵循固定规则：

### 规则 1：不设置 Rows/Columns（全自动）

​	根据子元素数量，自动生成**最接近正方形**的网格

- 3 个元素 → 1 行 3 列
- 6 个元素 → 2 行 3 列
- 9 个元素 → 3 行 3 列

### 规则 2：仅设置 `Columns`（固定列，自动算行）

​	例：`Columns="2"`，5 个子元素 → **自动生成 3 行 2 列**

### 规则 3：仅设置 `Rows`（固定行，自动算列）

​	例：`Rows="2"`，5 个子元素 → **自动生成 2 行 3 列**

### 规则 4：同时设置 Rows+Columns（强制网格）

​	例：`Rows="2" Columns="3"`，无论多少子元素，都固定为 2 行 3 列，多余单元格为空

### 规则 5：排列顺序

​	**从左到右 → 从上到下** 依次填充

### 规则 6：`FirstColumn` 占位

​	例：`FirstColumn="2"` → 前 2 列空白，元素从第 3 列开始填充

------

## 四、标准使用步骤（XAML + C# 双版本）

### 方式 1：XAML 中使用（推荐，开发主流）

xaml:

```xaml
<!-- 1. 声明 UniformGrid 容器 -->
<UniformGrid>
    <!-- 2. 直接添加子元素，无需任何布局属性 -->
    <控件1/>
    <控件2/>
    <控件3/>
</UniformGrid>
```

### 方式 2：C# 后台代码使用

csharp:

```c#
// 1. 创建 UniformGrid 实例
UniformGrid grid = new UniformGrid();
// 2. 设置行列
grid.Columns = 3;
grid.Rows = 3;
// 3. 添加子元素
grid.Children.Add(new Button { Content = "1" });
grid.Children.Add(new Button { Content = "2" });
// 4. 添加到界面
this.Content = grid;
```

------

## 五、分场景实战（可直接复制运行，逐句解析）

### 场景 1：最简用法 → 无任何配置（全自动布局）

​	**效果**：6 个元素自动生成 2 行 3 列 等大网格

xaml:

```xaml
<Window ...>
    <!-- 无Rows/Columns，全自动布局 -->
    <UniformGrid Background="LightGray">
        <!-- 子元素自动排列，无任何附加属性 -->
        <Button Content="元素1"/>
        <Button Content="元素2"/>
        <Button Content="元素3"/>
        <Button Content="元素4"/>
        <Button Content="元素5"/>
        <Button Content="元素6"/>
    </UniformGrid>
</Window>
```

------

### 场景 2：最常用 → 固定行列（九宫格 / 计算器）

​	**效果**：强制 3 行 3 列，9 个等大按钮

xaml:

```xaml
<UniformGrid 
    Rows="3"        <!-- 固定3行 -->
    Columns="3"      <!-- 固定3列 -->
    Margin="10"      <!-- 外边距 -->
    Background="White">

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
```

------

### 场景 3：仅固定列数 → 自动适配行数（图片墙）

​	**效果**：固定 2 列，行数自动计算

xaml:

```xaml
<!-- 固定2列，4张图片 → 自动2行2列 -->
<UniformGrid Columns="2" Margin="5">
    <Image Source="pic1.jpg" Stretch="Fill" Margin="2"/>
    <Image Source="pic2.jpg" Stretch="Fill" Margin="2"/>
    <Image Source="pic3.jpg" Stretch="Fill" Margin="2"/>
    <Image Source="pic4.jpg" Stretch="Fill" Margin="2"/>
</UniformGrid>
```

------

### 场景 4：FirstColumn 占位 → 空出前列

​	**效果**：4 列网格，元素从第 2 列开始填充，前 2 列空白

xaml:

```xaml
<UniformGrid 
    Columns="4" 
    FirstColumn="2"  <!-- 从第2列开始填充（索引从0开始） -->
    Background="LightGray">
    
    <Button Content="A"/>
    <Button Content="B"/>
    <Button Content="C"/>
</UniformGrid>
```

------

### 场景 5：配合样式 → 等尺寸导航按钮组

xaml:

```xaml
<UniformGrid Columns="4" Height="60">
    <Button Content="首页" Background="#0078D7" Foreground="White"/>
    <Button Content="产品" Background="#0078D7" Foreground="White"/>
    <Button Content="关于" Background="#0078D7" Foreground="White"/>
    <Button Content="我的" Background="#0078D7" Foreground="White"/>
</UniformGrid>
```

------

## 六、关键使用规范 & 避坑指南

### 1. ✅ 正确用法

- 用于**等尺寸元素**：按钮、图片、图标、标签
- 快速布局：计算器、九宫格、图片墙、导航栏
- 配合 `Margin` 控制元素间距

### 2. ❌ 禁止用法

- **不要用于表单 / 复杂界面**（用 Grid）
- **不要尝试跨行跨列**（不支持）
- **不要设置不同大小的单元格**（强制等大）

### 3. 隐藏元素的处理

`Visibility="Collapsed"` 的元素**不占用单元格**；

`Visibility="Hidden"` 的元素**占用单元格**。

------

## 七、UniformGrid vs Grid（核心区别，避免用错）

| 对比项      | UniformGrid | Grid                   |
| :---------- | :---------- | :--------------------- |
| 行 / 列定义 | 自动生成    | 手动定义               |
| 单元格大小  | 全部等大    | 可自定义               |
| 子元素配置  | 无附加属性  | 需要 `Grid.Row/Column` |
| 跨行跨列    | 不支持      | 完美支持               |
| 复杂度      | 极简        | 复杂                   |
| 适用场景    | 等尺寸元素  | 复杂表单 / 界面分区    |

------

## 八、总结（一句话牢记）

1. **UniformGrid = 全自动等大网格面板**，继承自 `Panel`；
2. **核心配置**：`Rows`(行数)、`Columns`(列数)、`FirstColumn`(起始列)；
3. **使用规则**：子元素自动左→右、上→下排列，无需任何附加属性；
4. **最佳场景**：九宫格、计算器、图片墙、等尺寸按钮组；
5. **一句话口诀**：**元素等大用 UniformGrid，复杂布局用 Grid**。