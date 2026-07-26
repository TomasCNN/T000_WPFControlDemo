# 003008001_VirtualizingStackPanel 类

​	`VirtualizingStackPanel`（**虚拟化堆叠面板**）是 WPF **专为****大数据量列表** 设计的**高性能布局面板**，它是 `StackPanel` 的**虚拟化增强版**。

​	核心能力：**UI 虚拟化** —— **只渲染屏幕可视区域内的列表项**，非可视区域的项不创建 UI 元素，彻底解决**十万级 / 百万级数据列表卡顿、内存爆炸**问题。

------

## 一、类定义（官方源码级）

### 1. 核心命名空间

csharp：

```c#
using System.Windows.Controls;
```

### 2. 官方类声明

csharp：

```c#
public class VirtualizingStackPanel : 
    Panel,          // 继承布局基类
    IStackPanel,    // 实现堆叠布局接口
    IScrollInfo,    // 支持滚动逻辑
    IVirtualizingPanel // 虚拟化核心接口
```

### 3. 完整继承链

```markdown
Object
└─ DispatcherObject
   └─ DependencyObject
      └─ Visual
         └─ UIElement
            └─ FrameworkElement
               └─ Panel （布局容器基类）
                  └─ VirtualizingStackPanel （虚拟化堆叠面板）
```

### 4. 核心定位

> 专门用于 **ItemsControl 系列列表控件**（ListBox/ListView/ItemsControl）的**高性能虚拟化布局面板**，沿用 `StackPanel` 线性堆叠逻辑，同时实现 UI 虚拟化。

------

## 二、核心特性（与普通 StackPanel 本质区别）

| 特性         | VirtualizingStackPanel     | 普通 StackPanel     |
| :----------- | :------------------------- | :------------------ |
| **UI 渲染**  | 仅渲染**可视区域**的项     | 渲染**所有**子项    |
| **性能**     | 十万级数据**零卡顿**       | 千级数据就卡顿      |
| **内存占用** | 极低（仅创建可视 UI）      | 极高（创建所有 UI） |
| **使用场景** | 大数据列表、长列表         | 少量元素、简单布局  |
| **使用方式** | 必须作为 `ItemsPanel` 使用 | 直接手动添加子元素  |
| **滚动支持** | 原生支持滚动虚拟化         | 无滚动优化          |

------

## 三、核心属性（必掌握）

### 1. 基础布局属性（同 StackPanel）

- **`Orientation`**：排列方向

  - `Vertical`（默认）：垂直堆叠
  - `Horizontal`：水平堆叠

  

### 2. 虚拟化核心附加属性（作用于列表控件）

这些是**附加属性**，直接写在 `ListBox/ItemsControl` 上：

| 属性                                            | 取值                   | 作用                                                         |
| :---------------------------------------------- | :--------------------- | :----------------------------------------------------------- |
| **`VirtualizingStackPanel.IsVirtualizing`**     | `True/False`           | 开启 / 关闭虚拟化（默认 `True`）                             |
| **`VirtualizingStackPanel.VirtualizationMode`** | `Standard`/`Recycling` | 虚拟化模式：• `Standard`：标准（滚动时销毁重建）• `Recycling`：**回收复用**（最高性能） |

### 3. 关键约束

✅ **必须配合 `ScrollViewer` 使用**（列表控件自带）

✅ **必须作为 `ItemsPanel` 使用**（不能手动加子元素）

✅ **仅支持数据绑定列表**（不支持手动 `Children.Add`）

------

## 四、标准使用方法

​	`VirtualizingStackPanel` **不能像普通面板一样直接放按钮 / 文本**，它的**唯一用法**：

### 作为 **列表控件的 ItemsPanel（项布局面板）** 使用

1. **默认用法**：`ListBox/ListView` **内置默认面板就是它**，无需任何配置
2. **自定义用法**：给 `ItemsControl` 显式指定该面板
3. **高级用法**：开启 `Recycling` 回收模式

------

## 五、实战可运行实例（3 个经典场景）

### 环境准备

​	创建 WPF 窗口，绑定**10 万条测试数据**（测试高性能）

------

### 实例 1：默认 ListBox（自带虚拟化，最简用法）

​	`ListBox` / `ListView` **默认 ItemsPanel 就是 VirtualizingStackPanel**，直接绑定大数据即可。

xaml:

```xaml
<Window x:Class="VirtualizingStackPanelDemo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="虚拟化列表 - 默认ListBox" Height="400" Width="300">
    <Grid>
        <!-- ListBox 内置 VirtualizingStackPanel，直接绑定大数据 -->
        <ListBox x:Name="listBox" 
                 VirtualizingStackPanel.VirtualizationMode="Recycling"/>
    </Grid>
</Window>
```

#### C# 后台（绑定 10 万条数据）

csharp:

```c#
using System.Collections.Generic;
using System.Windows;

namespace VirtualizingStackPanelDemo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 生成 100000 条测试数据
            List<string> data = new List<string>();
            for (int i = 0; i < 100000; i++)
            {
                data.Add($"虚拟化列表项 {i}");
            }

            // 绑定数据
            listBox.ItemsSource = data;
        }
    }
}
```

✅ **效果**：10 万条数据**瞬间加载**，滚动丝滑，无卡顿。

------

### 实例 2：自定义 ItemsControl（显式指定虚拟化面板）

​	手动给普通 `ItemsControl` 设置 `VirtualizingStackPanel`，实现水平虚拟化列表。

xaml:

```xaml
<Window x:Class="VirtualizingStackPanelDemo.CustomItemsDemo"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="自定义ItemsControl-水平虚拟化" Height="200" Width="500">
    <Grid>
        <ItemsControl x:Name="itemsControl">
            <!-- 1. 开启滚动（虚拟化必须依赖ScrollViewer） -->
            <ItemsControl.Template>
                <ControlTemplate>
                    <ScrollViewer HorizontalScrollBarVisibility="Auto">
                        <ItemsPresenter/>
                    </ScrollViewer>
                </ControlTemplate>
            </ItemsControl.Template>

            <!-- 2. 指定 ItemsPanel = VirtualizingStackPanel -->
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <!-- 水平排列 + 虚拟化 -->
                    <VirtualizingStackPanel Orientation="Horizontal"/>
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>

            <!-- 列表项样式 -->
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <Border Width="80" Height="80" Background="LightBlue" 
                            Margin="5" CornerRadius="5">
                        <TextBlock Text="{Binding}" 
                                   HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </Grid>
</Window>
```

#### C# 后台

csharp:

```c#
public partial class CustomItemsDemo : Window
{
    public CustomItemsDemo()
    {
        InitializeComponent();
        // 绑定1000条数据
        List<string> data = new List<string>();
        for (int i = 0; i < 1000; i++) data.Add($"Item {i}");
        itemsControl.ItemsSource = data;
    }
}
```

✅ **效果**：水平长列表，仅渲染可视项，滚动流畅。

------

### 实例 3：开启最高性能 - 回收模式（Recycling）

xaml:

```xaml
<ListBox 
    x:Name="listBox" 
    <!-- 开启UI回收复用，极致性能 -->
    VirtualizingStackPanel.IsVirtualizing="True"
    VirtualizingStackPanel.VirtualizationMode="Recycling"/>
```

- `Standard`：滚动时销毁不可见项，新建可见项
- `Recycling`：**复用 UI 元素**，性能提升 50%+（推荐）

------

## 六、关键使用规则（新手必看）

1. **不能手动添加子元素**

   ❌ 错误：`virtualizingStackPanel.Children.Add(new Button());`

   ✅ 正确：仅通过 `ItemsSource` 数据绑定

2. **必须依赖 ScrollViewer**

   虚拟化需要滚动区域判断可视项，列表控件默认自带。

3. **仅用于线性列表**

   只支持垂直 / 水平堆叠，不支持网格、换行、停靠。

4. **默认开启虚拟化**

   `ListBox/ListView` 无需任何配置，直接用就是高性能。

------

## 七、VirtualizingStackPanel VS 其他布局面板

| 面板                       | 布局方式      | 性能 | 适用场景                |
| :------------------------- | :------------ | :--- | :---------------------- |
| **VirtualizingStackPanel** | 线性 + 虚拟化 | 极致 | 大数据列表（1 万 + 项） |
| StackPanel                 | 线性          | 低   | 少量元素（<100 项）     |
| WrapPanel                  | 流式换行      | 中   | 标签云、小列表          |
| DockPanel                  | 停靠          | 中   | 窗口框架                |
| Grid                       | 网格          | 高   | 表单、精细布局          |

------

## 八、总结（核心记忆点）

1. **类定义**：`VirtualizingStackPanel : Panel`，实现**IVirtualizingPanel**虚拟化接口

2. **核心能力**：**UI 虚拟化**，仅渲染可视项，大数据列表专用

3. **使用方式**：**作为列表控件的 ItemsPanel**（不能手动加子元素）

4. **核心属性**：`Orientation`（方向）、`VirtualizationMode`（回收模式）

5. **默认宿主**：`ListBox/ListView` 内置该面板，开箱即用

6. **一句话**：

   

   长列表、大数据，必用 VirtualizingStackPanel！普通小列表用 StackPanel！