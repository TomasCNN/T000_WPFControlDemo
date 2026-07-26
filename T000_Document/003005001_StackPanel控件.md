# 003005001_StackPanel控件

​	`StackPanel` 是 WPF **最简单、最基础、最常用**的**线性布局面板**，继承自 `Panel` 基类，是所有线性排列布局的首选控件。

​	它的核心逻辑只有一句话：**将子元素按照 垂直 / 水平 单一方向，依次 “堆叠” 排列，一个挨着一个，不换行、不重叠**。

------

## 一、官方类定义与继承结构

### 1. 核心类声明（源码级）

csharp：

```c#
public class StackPanel : Panel
```

- **公开类**：可直接实例化、可继承自定义
- **非密封类**：支持扩展
- **直接继承 `Panel`**：拥有子元素管理、可视化树、布局系统全部能力
- **无额外接口**：结构极简，仅实现基础线性堆叠

### 2. 完整继承链（WPF 标准层级）

```markdown
Object
└─ DispatcherObject（UI线程绑定）
   └─ DependencyObject（依赖属性系统）
      └─ Visual（渲染/命中测试）
         └─ UIElement（基础布局/事件）
            └─ FrameworkElement（样式/绑定/生命周期）
               └─ Panel（子元素容器/布局契约）
                  └─ StackPanel（线性堆叠布局）
```

### 3. 官方核心定位

> 用于在**单行或单列**中排列子元素的基础布局容器，是 WPF 布局系统的「基础砖块」。

------

## 二、StackPanel 核心特性（必背）

1. **单向排列**：仅支持 **垂直（默认）** 或 **水平** 一个方向

2. **依次堆叠**：子元素按添加顺序，一个紧接一个排列

3. **不自动换行**：内容超出容器边界会直接截断（无滚动）

4. **无比例分配**：不像 Grid 支持 `*` 占比，尺寸由子元素自身决定

5. **自适应尺寸**：

   - 垂直堆叠：宽度 = 最宽子元素宽度，高度 = 所有子元素高度之和
   - 水平堆叠：高度 = 最高子元素高度，宽度 = 所有子元素宽度之和

   

6. **极简结构**：无行列定义、无复杂配置，开箱即用

------

## 三、核心属性（全部掌握即可精通）

StackPanel 配置极少，**核心属性只有 1 个**，其余为通用布局属性：

### 1. 【唯一核心属性】`Orientation`（排列方向）

| 属性值       | 说明                     | 默认值 |
| :----------- | :----------------------- | :----- |
| `Vertical`   | **垂直堆叠**（从上到下） | ✅ 默认 |
| `Horizontal` | **水平堆叠**（从左到右） | -      |

xaml:

```xaml
<!-- 垂直排列（默认，可省略） -->
<StackPanel Orientation="Vertical">
<!-- 水平排列 -->
<StackPanel Orientation="Horizontal">
```

### 2. 通用布局属性（继承自 Panel/FrameworkElement）

| 属性                  | 作用                            |
| :-------------------- | :------------------------------ |
| `Children`            | 子元素集合（所有布局面板核心）  |
| `Background`          | 面板背景画刷                    |
| `Margin`              | 面板外边距                      |
| `Padding`             | 面板内边距（子元素与边框间距）  |
| `HorizontalAlignment` | 水平对齐（左 / 中 / 右 / 拉伸） |
| `VerticalAlignment`   | 垂直对齐（上 / 中 / 下 / 拉伸） |

------

## 四、底层布局原理（Measure + Arrange）

​	所有 WPF 布局都遵循 **两步布局法**，StackPanel 的实现**最简单**：

### 第一步：测量阶段 `MeasureOverride`

1. 遍历所有子元素，让每个子元素计算自己的大小
2. **垂直模式**：
   - 总宽度 = 所有子元素中**最大宽度**
   - 总高度 = 所有子元素**高度累加**
3. **水平模式**：
   - 总高度 = 所有子元素中**最大高度**
   - 总宽度 = 所有子元素**宽度累加**
4. 向父容器报告自己需要的尺寸

### 第二步：排列阶段 `ArrangeOverride`

1. 按子元素添加顺序**依次定位**
2. 垂直模式：从上到下，每个子元素的 `Y 坐标` 是上一个元素的底部位置
3. 水平模式：从左到右，每个子元素的 `X 坐标` 是上一个元素的右侧位置
4. 不处理溢出，直接排列到容器外

------

## 五、标准使用方法（XAML + C#）

### 方式 1：XAML 使用（推荐，主流开发）

#### ① 默认垂直堆叠（最常用：表单、列表）

xaml:

```c#
<StackPanel Margin="10" Background="White">
    <!-- 从上到下依次排列 -->
    <TextBlock Text="用户名"/>
    <TextBox Height="30" Margin="0 5"/>
    <TextBlock Text="密码"/>
    <PasswordBox Height="30" Margin="0 5"/>
    <Button Content="登录" Height="35" Background="#0078D7"/>
</StackPanel>
```

#### ② 水平堆叠（导航栏、按钮组）

xaml:

```c#
<StackPanel Orientation="Horizontal" Margin="10" Background="LightGray">
    <!-- 从左到右依次排列 -->
    <Button Content="首页" Margin="2"/>
    <Button Content="产品" Margin="2"/>
    <Button Content="关于" Margin="2"/>
    <Button Content="我的" Margin="2"/>
</StackPanel>
```

### 方式 2：C# 后台代码创建

csharp:

```c#
// 1. 创建实例
StackPanel stackPanel = new StackPanel();
// 2. 设置方向（水平）
stackPanel.Orientation = Orientation.Horizontal;
// 3. 添加子元素
stackPanel.Children.Add(new Button { Content = "按钮1" });
stackPanel.Children.Add(new Button { Content = "按钮2" });
// 4. 添加到窗口
this.Content = stackPanel;
```

------

## 六、关键使用细节（新手必看）

### 1. 子元素拉伸填充

​	StackPanel **默认不会让子元素填充剩余空间**，需要手动设置：

xaml:

```xaml
<!-- 垂直StackPanel中，让控件宽度填满 -->
<TextBox HorizontalAlignment="Stretch"/>

<!-- 水平StackPanel中，让控件高度填满 -->
<Button VerticalAlignment="Stretch"/>
```

### 2. 溢出处理（内容超出容器）

​	StackPanel **不会自动换行 / 滚动**，超出部分直接隐藏，解决方案：

xaml:

```xaml
<!-- 包裹 ScrollViewer 实现滚动 -->
<ScrollViewer VerticalScrollBarVisibility="Auto">
    <StackPanel>
        <!-- 大量子元素 -->
    </StackPanel>
</ScrollViewer>
```

### 3. 间距控制

- 元素之间间距：使用子元素的 `Margin`
- 整体内边距：使用 StackPanel 的 `Padding`

------

## 七、StackPanel VS 其他布局面板（核心区别）

​	结合你学过的 `Grid` / `UniformGrid`，一眼区分使用场景：

| 控件            | 排列方式            | 复杂度 | 适用场景                            |
| :-------------- | :------------------ | :----- | :---------------------------------- |
| **StackPanel**  | 单向线性堆叠        | 极低   | 简单垂直 / 水平排列、表单、导航栏   |
| **Grid**        | 二维网格（行 + 列） | 高     | 复杂界面、表单对齐、响应式布局      |
| **UniformGrid** | 等大单元格网格      | 低     | 九宫格、计算器、等尺寸按钮          |
| **WrapPanel**   | 线性自动换行        | 低     | 标签流、图片墙（StackPanel 升级版） |

------

## 八、适用场景 ✅ & 禁用场景 ❌

### ✅ 推荐使用

1. 简单垂直表单（登录 / 注册）
2. 水平导航栏、工具栏按钮组
3. 列表项内部布局
4. 极简线性排列布局
5. 作为复杂布局的**子容器**（嵌套使用）

### ❌ 禁止使用

1. 复杂二维界面（用 Grid）
2. 需要等比例分配空间（用 Grid/UniformGrid）
3. 需要自动换行（用 WrapPanel）
4. 高性能大数据列表（用 ListBox/ListView）

------

## 九、官方自定义规范（继承 StackPanel）

如果你需要扩展 StackPanel，遵循微软官方规范：

1. **直接继承 `StackPanel`**，不继承 `Panel`
2. 重写 `MeasureOverride`/`ArrangeOverride` **必须调用 `base.方法`**
3. 扩展属性用**依赖属性**
4. 不破坏原生线性堆叠逻辑

csharp:

```c#
// 官方规范自定义 StackPanel（带间距）
public class CustomStackPanel : StackPanel
{
    // 依赖属性 + 布局重写（同之前Grid自定义规范）
}
```

------

## 十、总结（极简核心记忆）

1. **类定义**：`StackPanel : Panel`，WPF 最基础线性布局面板

2. **核心能力**：**垂直 / 水平 单向依次堆叠**，不换行、不重叠

3. **唯一核心属性**：`Orientation`（控制排列方向）

4. **布局规则**：

   - 垂直：宽取最大，高累加
   - 水平：高取最大，宽累加

   

5. **使用口诀**：

   

   简单线性用 Stack，复杂网格用 Grid，等大格子用 Uniform，自动换行用 Wrap！