# 004011005_WPF RichTextBox 官方类定义完全解析

WPF `RichTextBox` 是基于**FlowDocument 流文档模型**的富文本编辑控件，其设计理念与 WinForms 版本有本质区别。本文将严格基于微软官方文档，从**类定义、继承关系、核心成员、设计原理**四个维度进行深度解析，并结合工业自动化场景说明其实际应用。

## 一、基本信息与官方类签名

### 1.1 核心元数据

| 项           | 官方值                                                       | 说明                                         |
| :----------- | :----------------------------------------------------------- | :------------------------------------------- |
| **命名空间** | `System.Windows.Controls`                                    | WPF 控件标准命名空间                         |
| **程序集**   | `PresentationFramework.dll`                                  | WPF 核心框架程序集                           |
| **继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → TextBoxBase → RichTextBox` | 完整继承层级                                 |
| **线程安全** | 仅 UI 线程安全                                               | 所有成员只能在创建它的 Dispatcher 线程上访问 |
| **支持版本** | .NET Framework 3.0+ / .NET Core 3.0+ / .NET 5+               | 所有 WPF 支持版本                            |

### 1.2 官方类定义（带特性）

csharp:

```c#
[System.Windows.LocalizabilityAttribute(System.Windows.LocalizationCategory.Text)]
[System.Windows.TemplatePartAttribute(Name="PART_ContentHost", Type=typeof(System.Windows.Controls.ScrollViewer))]
public class RichTextBox : System.Windows.Controls.Primitives.TextBoxBase
```

#### 特性详解

1. **`LocalizabilityAttribute(LocalizationCategory.Text)`**
   - 标记该控件主要包含文本内容
   - 指导本地化工具将其内容视为可本地化资源
   - 工业场景中用于多语言报警日志的自动翻译
2. **`TemplatePartAttribute(Name="PART_ContentHost", Type=typeof(ScrollViewer))`**
   - 控件模板必须包含一个名为`PART_ContentHost`的`ScrollViewer`
   - 该 ScrollViewer 用于承载 FlowDocument 的可视化内容
   - 如果自定义模板中缺少此部分，控件将无法正常工作

## 二、继承关系深度解析

理解继承链是掌握 RichTextBox 能力边界的关键：

### 2.1 各父类的核心贡献

| 父类                   | 提供的核心能力       | RichTextBox 中的体现                                         |
| :--------------------- | :------------------- | :----------------------------------------------------------- |
| **`DispatcherObject`** | WPF 线程模型基础     | 所有操作必须通过 Dispatcher 调度，确保 UI 线程安全           |
| **`DependencyObject`** | 依赖属性系统         | RichTextBox 的所有属性几乎都是依赖属性，支持绑定、样式、动画 |
| **`Visual`**           | 可视化渲染基础       | 提供 2D 图形渲染能力，支持透明度、变换等                     |
| **`UIElement`**        | 输入事件、布局、焦点 | 处理鼠标、键盘输入，参与 WPF 布局系统                        |
| **`FrameworkElement`** | 样式、模板、数据绑定 | 支持 MVVM 模式，可通过 ControlTemplate 自定义外观            |
| **`Control`**          | 控件基类功能         | 提供 Background、Foreground、Font 等通用控件属性             |
| **`TextBoxBase`**      | 文本编辑基础能力     | 提供撤销 / 重做、剪贴板操作、文本选择等通用文本编辑功能      |

### 2.2 与 TextBox 的核心区别

| 特性             | TextBox              | RichTextBox                            |
| :--------------- | :------------------- | :------------------------------------- |
| **内容模型**     | 纯字符串             | FlowDocument 强类型对象模型            |
| **格式支持**     | 仅整体格式           | 支持字符级、段落级、文档级格式         |
| **复杂元素**     | 不支持               | 支持表格、图片、列表、超链接、嵌入控件 |
| **排版能力**     | 基础                 | 强大（分页、分栏、自适应布局）         |
| **性能**         | 纯文本场景更好       | 富文本场景更好                         |
| **工业场景应用** | 简单输入框、单行显示 | 报警日志、操作记录、报表、说明书       |

## 三、核心设计原理：FlowDocument 流文档模型

这是 WPF RichTextBox 与 WinForms 版本最本质的区别。WinForms RichTextBox 基于 RTF 字符串，而 WPF RichTextBox 基于**强类型的 FlowDocument 对象树**。

### 3.1 文档元素树结构

plaintext:

```tex
FlowDocument（根）
├─ Block（块级元素）
│  ├─ Paragraph（段落）
│  │  └─ Inline（行内元素）
│  │     ├─ Run（文本段）
│  │     ├─ Span（格式容器）
│  │     ├─ Bold / Italic / Underline（格式标签）
│  │     ├─ LineBreak（换行）
│  │     ├─ Hyperlink（超链接）
│  │     └─ InlineUIContainer（嵌入WPF控件）
│  ├─ Table（表格）
│  │  ├─ TableRowGroup
│  │  │  └─ TableRow
│  │  │     └─ TableCell
│  │  │        └─ Block
│  ├─ List（列表）
│  │  └─ ListItem
│  │     └─ Block
│  └─ Section（节）
│     └─ Block
```

### 3.2 关键文本操作抽象

WPF 不使用简单的整数索引操作文本，而是提供了两个核心抽象：

1. **`TextPointer`**
   - 表示文档中两个字符之间的**不可变位置**
   - 包含逻辑方向（Forward/Backward）信息
   - 可以遍历文档的所有元素边界
   - 解决了跨元素文本操作的问题
2. **`TextRange`**
   - 表示两个`TextPointer`之间的文本范围
   - 提供统一的 API 操作任意范围的文本和格式
   - 支持加载 / 保存多种格式（XAML、RTF、纯文本）

### 3.3 官方设计意图

微软设计 FlowDocument 模型的核心目标是：

1. **分离内容与表现**：文档内容与可视化样式分离，支持主题切换
2. **强类型安全**：避免 RTF 字符串操作的语法错误和安全问题
3. **统一编程模型**：与 WPF 其他控件使用相同的依赖属性和事件模型
4. **支持复杂排版**：满足现代应用对文档排版的高级需求

## 四、核心成员官方解析

### 4.1 内容相关属性（最重要）

#### `Document` 属性

csharp:

```c#
public System.Windows.Documents.FlowDocument Document { get; set; }
```

- **作用**：获取或设置 RichTextBox 的内容根元素
- **默认值**：一个空的 FlowDocument 实例
- **官方说明**：RichTextBox 的所有内容都存储在这个属性中，修改此属性将替换整个文档
- **工业场景应用**：动态加载不同设备的操作手册、切换不同生产线的报警日志

#### `IsReadOnly` 属性

csharp:

```c#
public bool IsReadOnly { get; set; }
```

- **作用**：获取或设置是否允许用户编辑内容
- **默认值**：`false`（可编辑）
- **官方说明**：设置为 true 时，用户仍然可以选择和复制文本，只是不能修改
- **工业场景最佳实践**：报警日志、操作记录等显示控件**必须设置为 true**，防止用户误修改

#### `IsDocumentEnabled` 属性

csharp:

```c#
public bool IsDocumentEnabled { get; set; }
```

- **作用**：获取或设置是否启用文档中的交互元素
- **默认值**：`false`
- **官方说明**：设置为 true 时，文档中的 Hyperlink、Button 等交互元素才能响应点击
- **常见陷阱**：即使 IsReadOnly 为 true，只要 IsDocumentEnabled 为 true，超链接仍然可以点击

### 4.2 选择与光标相关属性

#### `Selection` 属性

csharp:

```c#
public System.Windows.Documents.TextSelection Selection { get; }
```

- **作用**：获取当前选中的文本范围
- **官方说明**：这是一个只读属性，但可以通过其方法修改选择范围
- **常用成员**：
  - `Selection.Text`：获取或设置选中的纯文本
  - `Selection.ApplyPropertyValue()`：为选中内容应用格式
  - `Selection.Start` / `Selection.End`：获取选择的起止位置

#### `CaretPosition` 属性

csharp:

```c#
public System.Windows.Documents.TextPointer CaretPosition { get; set; }
```

- **作用**：获取或设置光标的位置
- **官方说明**：光标位置始终位于两个字符之间，设置此属性会移动光标并清除当前选择

### 4.3 外观与行为属性

#### 滚动条相关

csharp:

```c#
public System.Windows.Controls.ScrollBarVisibility HorizontalScrollBarVisibility { get; set; }
public System.Windows.Controls.ScrollBarVisibility VerticalScrollBarVisibility { get; set; }
```

- **默认值**：Horizontal=Disabled，Vertical=Auto
- **工业场景最佳实践**：
  - 报警日志：Horizontal=Disabled（自动换行），Vertical=Auto
  - 代码编辑器：Horizontal=Auto，Vertical=Auto

#### 输入行为

csharp:

```c#
public bool AcceptsReturn { get; set; }
public bool AcceptsTab { get; set; }
```

- `AcceptsReturn`：是否接受回车键插入换行（默认 true）
- `AcceptsTab`：是否接受制表键插入 Tab 字符（默认 false，Tab 用于切换焦点）

### 4.4 核心方法

#### 导航方法

csharp:

```c#
public void ScrollToEnd();
public void ScrollToHome();
public void ScrollToVerticalOffset(double offset);
```

- **`ScrollToEnd()`**：滚动到文档末尾（工业报警日志必备，新报警自动滚动）
- **`ScrollToHome()`**：滚动到文档开头

#### 内容操作

csharp:

```c#
public void SelectAll();
public void Clear();
public void Copy();
public void Cut();
public void Paste();
public void Undo();
public void Redo();
```

- 所有方法都与 TextBoxBase 保持一致，提供统一的文本编辑体验

#### 位置转换

csharp:

```c#
public System.Windows.Documents.TextPointer GetPositionFromPoint(System.Windows.Point point, bool snapToText);
```

- **作用**：将屏幕坐标转换为文档中的 TextPointer
- **官方说明**：参数`snapToText`指定当点不在任何文本上时，是否自动吸附到最近的文本位置
- **工业场景应用**：实现点击报警日志条目显示详情、右键菜单定位

### 4.5 常用事件

#### `TextChanged` 事件

csharp:

```c#
public event System.Windows.Controls.TextChangedEventHandler TextChanged;
```

- **触发时机**：文档内容发生任何变化时
- **官方说明**：即使是通过代码修改内容也会触发此事件
- **工业场景应用**：实时统计报警数量、自动保存日志

#### `SelectionChanged` 事件

csharp:

```c#
public event System.Windows.RoutedEventHandler SelectionChanged;
```

- **触发时机**：用户或代码改变文本选择范围时
- **应用**：更新工具栏的格式按钮状态（粗体、斜体等）

#### `LinkClicked` 事件

csharp:

```c#
public event System.Windows.RoutedEventHandler LinkClicked;
```

- **触发时机**：用户点击文档中的 Hyperlink 元素时
- **官方说明**：只有当`IsDocumentEnabled="True"`时才会触发
- **工业场景应用**：点击报警日志中的设备 ID 跳转到设备详情页

#### `ContextMenuOpening` 事件

csharp:

```c#
public event System.Windows.Controls.ContextMenuEventHandler ContextMenuOpening;
```

- **触发时机**：用户右键点击控件时
- **应用**：自定义右键菜单，添加复制、导出、搜索等功能

## 五、关键内部机制解析

### 5.1 文本表示机制

WPF RichTextBox 不将文档存储为单个字符串，而是拆分为多个`Run`元素。每个`Run`表示一段具有相同格式的文本。这种设计的优点是：

- 修改格式时不需要重新解析整个字符串
- 支持高效的增量更新
- 便于实现复杂的格式嵌套

### 5.2 编辑事务与撤销栈

RichTextBox 内部维护一个**编辑事务栈**，用于实现撤销 / 重做功能：

- 每个用户操作（输入、删除、格式修改）都会被封装为一个事务
- 调用`Undo()`会弹出栈顶事务并反转其操作
- 调用`BeginChange()`和`EndChange()`可以将多个操作合并为一个事务

### 5.3 布局与渲染

FlowDocument 采用**流式布局**：

- 内容会根据控件大小自动重排
- 支持分页、分栏、段落前后间距
- 所有元素都使用 WPF 的矢量渲染，支持任意缩放而不失真

## 六、官方设计意图与最佳实践

### 6.1 官方推荐使用场景

微软官方建议在以下场景使用 RichTextBox：

1. 需要显示或编辑带格式的文本
2. 需要包含表格、列表、图片等复杂元素
3. 需要支持超链接或其他交互元素
4. 需要生成可打印的文档

### 6.2 工业场景最佳实践

1. **只读模式**：报警日志、操作记录等显示控件必须设置`IsReadOnly="True"`
2. **自动滚动**：添加新内容后调用`ScrollToEnd()`，实现新报警自动滚动
3. **禁用交互元素**：不需要超链接时设置`IsDocumentEnabled="False"`，提高安全性
4. **性能优化**：
   - 大文档使用`BeginChange()`/`EndChange()`批量更新
   - 限制最大行数，避免内存无限增长
   - 静态内容使用`TextBlock`而非`RichTextBox`
5. **MVVM 支持**：由于`Document`不是依赖属性，需要使用附加属性实现绑定

### 6.3 官方不推荐使用场景

- 纯文本输入（使用`TextBox`）
- 密码输入（使用`PasswordBox`）
- 大量数据的表格显示（使用`DataGrid`）
- 代码编辑（使用专业代码编辑器控件）

## 七、与官方文档的对应说明

本文所有内容均严格基于微软官方 WPF 文档：

- [RichTextBox Class (System.Windows.Controls) | Microsoft Learn](https://link.wtturl.cn/?target=https%3A%2F%2Flearn.microsoft.com%2Fen-us%2Fdotnet%2Fapi%2Fsystem.windows.controls.richtextbox&scene=im&aid=582478&lang=zh)
- [Flow Document Overview | Microsoft Learn](https://link.wtturl.cn/?target=https%3A%2F%2Flearn.microsoft.com%2Fen-us%2Fdotnet%2Fdesktop%2Fwpf%2Fadvanced%2Fflow-document-overview&scene=im&aid=582478&lang=zh)
- [TextBoxBase Class (System.Windows.Controls.Primitives) | Microsoft Learn](https://link.wtturl.cn/?target=https%3A%2F%2Flearn.microsoft.com%2Fen-us%2Fdotnet%2Fapi%2Fsystem.windows.controls.primitives.textboxbase&scene=im&aid=582478&lang=zh)

理解 RichTextBox 的官方类定义和设计原理，是正确使用和扩展该控件的基础。在工业自动化场景中，它是构建报警日志、操作记录、生产报表等富文本界面的核心控件。