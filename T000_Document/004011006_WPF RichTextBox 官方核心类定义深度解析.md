# 004011006_WPF RichTextBox 官方核心类定义深度解析

**类定义源码：**

```c#
public class RichTextBox : TextBoxBase, IAddChild
{
    public static readonly DependencyProperty IsDocumentEnabledProperty;
 
    public RichTextBox();
    public RichTextBox(FlowDocument document);
 
    public FlowDocument Document { get; set; }
    public bool IsDocumentEnabled { get; set; }
    public TextSelection Selection { get; }
    public TextPointer CaretPosition { get; set; }
    protected internal override IEnumerator LogicalChildren { get; }
 
    public TextPointer GetNextSpellingErrorPosition(TextPointer position, LogicalDirection direction);
    public TextPointer GetPositionFromPoint(Point point, bool snapToText);
    public SpellingError GetSpellingError(TextPointer position);
    public TextRange GetSpellingErrorRange(TextPointer position);
    public bool ShouldSerializeDocument();
    protected override Size MeasureOverride(Size constraint);
    protected override AutomationPeer OnCreateAutomationPeer();
    protected override void OnDpiChanged(DpiScale oldDpiScaleInfo, DpiScale newDpiScaleInfo);
}
```



## 一、类声明与继承关系

csharp：

```c#
public class RichTextBox : TextBoxBase, IAddChild
```

### 1.1 基类：TextBoxBase

RichTextBox 继承自`TextBoxBase`，这是所有 WPF 文本编辑控件的抽象基类，提供了所有文本编辑控件共有的核心能力：

- 剪贴板操作（Copy/Cut/Paste）
- 撤销 / 重做（Undo/Redo）
- 文本选择
- 只读模式（IsReadOnly）
- 拼写检查
- 自动换行（WordWrap）
- 滚动控制

**工业场景意义**：所有 TextBoxBase 的属性和方法都可以在 RichTextBox 中直接使用，比如报警日志中最常用的`IsReadOnly="True"`和`ScrollToEnd()`方法。

### 1.2 接口：IAddChild

这是 RichTextBox 实现的一个关键接口，**XAML 解析器通过这个接口将 FlowDocument 子元素添加到 RichTextBox 中**。

csharp：

```c#
public interface IAddChild
{
    void AddChild(object value);
    void AddText(string text);
}
```

**官方设计意图**：

- 支持 XAML 语法：`<RichTextBox><FlowDocument>...</FlowDocument></RichTextBox>`
- XAML 解析器会自动调用`AddChild()`方法将 FlowDocument 赋值给`Document`属性
- 如果不实现这个接口，就无法在 XAML 中直接嵌套 FlowDocument

**常见陷阱**：不要手动调用`AddChild()`方法，应该直接使用`Document`属性。

------

## 二、静态依赖属性

csharp:

```c#
public static readonly DependencyProperty IsDocumentEnabledProperty;
```

这是 RichTextBox 唯一的自有依赖属性（其他依赖属性都继承自 TextBoxBase）。

### 2.1 依赖属性本质

- 遵循 WPF 依赖属性命名规范：`[属性名]Property`
- 静态只读字段，在静态构造函数中注册
- 支持数据绑定、样式、动画、继承和默认值

### 2.2 对应 CLR 包装器

csharp:

```c#
public bool IsDocumentEnabled { get; set; }
```

### 2.3 官方说明与工业应用

| 项           | 官方值                                | 工业场景说明                             |
| :----------- | :------------------------------------ | :--------------------------------------- |
| **作用**     | 获取或设置是否启用文档中的交互元素    | 控制报警日志中的超链接、按钮是否可点击   |
| **默认值**   | `false`                               | 出于安全考虑，默认禁用所有交互元素       |
| **交互元素** | Hyperlink、Button、TextBox 等嵌入控件 | 点击报警日志中的设备 ID 跳转到设备详情页 |
| **最佳实践** | 不需要交互时保持默认值`false`         | 提高系统安全性，防止误点击               |

**重要注意事项**：即使`IsReadOnly="True"`，只要`IsDocumentEnabled="True"`，超链接仍然可以点击。

------

## 三、构造函数

RichTextBox 有两个官方构造函数，对应两种初始化方式：

### 3.1 无参构造函数

csharp:

```c#
public RichTextBox();
```

**官方行为**：

- 创建一个空的 RichTextBox 实例
- 自动初始化`Document`属性为一个**新的空 FlowDocument 实例**
- 所有属性使用默认值

**使用场景**：

- 设计器拖放创建的控件默认使用这个构造函数
- 动态创建控件时使用
- 后续通过代码赋值`Document`属性

### 3.2 带文档参数的构造函数

csharp:

```c#
public RichTextBox(FlowDocument document);
```

**官方行为**：

- 创建 RichTextBox 实例并将指定的 FlowDocument 作为其内容
- 如果`document`为`null`，会抛出`ArgumentNullException`

**工业场景最佳实践**：

csharp:

```c#
// 预加载报警日志模板
var alarmTemplate = new FlowDocument();
alarmTemplate.Blocks.Add(new Paragraph(new Run("=== 报警日志 ===")));

// 直接使用模板创建RichTextBox
var alarmLogBox = new RichTextBox(alarmTemplate);
```

**性能优势**：避免了先创建空文档再替换的额外开销，对于频繁创建 RichTextBox 的场景更高效。

------

## 四、核心属性详解

这四个属性是 RichTextBox 区别于其他文本控件的核心，也是工业开发中最常用的属性。

### 4.1 Document 属性（核心中的核心）

csharp:

```c#
public FlowDocument Document { get; set; }
```

**官方定义**：获取或设置 RichTextBox 的内容根元素。

#### 核心特性

1. **不能为空**：RichTextBox 会自动维护一个非空的 FlowDocument 实例，设置为`null`会抛出异常
2. **强类型内容模型**：所有内容都存储在 FlowDocument 对象树中，而不是字符串
3. **完全替换**：设置此属性会**完全替换**RichTextBox 的所有内容
4. **双向关联**：修改 FlowDocument 的内容会立即反映在 RichTextBox 中

#### 工业场景应用

csharp:

```c#
// 1. 切换不同生产线的报警日志
richTextBox.Document = ProductionLineA.AlarmLogDocument;

// 2. 加载设备操作手册
var manualDoc = XamlReader.Load(File.OpenRead("Manual.xaml")) as FlowDocument;
richTextBox.Document = manualDoc;

// 3. 清空日志（推荐方式）
richTextBox.Document.Blocks.Clear();

// 4. 导出日志
TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
range.Save(stream, DataFormats.Rtf);
```

#### 官方设计意图

微软将内容与容器分离，使得同一个 FlowDocument 可以在多个 RichTextBox 之间共享，也可以独立于 UI 进行操作和持久化。

### 4.2 Selection 属性

csharp:

```c#
public TextSelection Selection { get; }
```

**官方定义**：获取当前选中的文本范围。

#### 核心特性

1. **只读属性**：不能直接赋值，但可以通过其方法修改选择范围
2. **永远不为 null**：即使没有选中任何内容，Selection 也会返回一个有效实例（Start 和 End 指向同一个位置）
3. **TextRange 子类**：继承自 TextRange，提供了更多选择相关的方法

#### 常用成员

csharp:

```c#
// 获取选中的纯文本
string selectedText = richTextBox.Selection.Text;

// 为选中内容应用格式
richTextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);

// 选中所有内容
richTextBox.Selection.SelectAll();

// 清除选择
richTextBox.Selection.Select(richTextBox.CaretPosition, richTextBox.CaretPosition);
```

#### 工业场景应用

- 复制选中的报警条目
- 批量修改选中日志的格式
- 右键菜单操作选中的内容

### 4.3 CaretPosition 属性

csharp:

```c#
public TextPointer CaretPosition { get; set; }
```

**官方定义**：获取或设置输入光标的位置。

#### 核心特性

1. **TextPointer 类型**：WPF 特有的文本位置抽象，不是整数索引
2. **不可变**：TextPointer 是不可变的，设置此属性会创建一个新的 TextPointer
3. **自动滚动**：设置此属性会自动将光标滚动到视图中（WPF 4.0+）

#### 工业场景应用

csharp:

```c#
// 自动滚动到最新的报警条目
richTextBox.CaretPosition = richTextBox.Document.ContentEnd;
richTextBox.ScrollToCaret();

// 将光标移动到指定报警的开头
TextPointer alarmStart = FindAlarmStart(alarmId);
richTextBox.CaretPosition = alarmStart;
```

### 4.4 LogicalChildren 属性

csharp:

```c#
protected internal override IEnumerator LogicalChildren { get; }
```

**官方定义**：获取 RichTextBox 的逻辑子元素枚举器。

#### 核心特性

1. **保护内部属性**：只能在类内部或同一程序集中访问
2. **唯一子元素**：RichTextBox 的逻辑子元素只有一个 ——`Document`属性指向的 FlowDocument
3. **WPF 布局系统使用**：布局系统通过这个属性遍历逻辑树

#### 官方实现

csharp:

```c#
protected internal override IEnumerator LogicalChildren
{
    get
    {
        yield return Document;
    }
}
```

------

## 五、核心方法详解

### 5.1 拼写检查相关方法

WPF RichTextBox 内置了多语言拼写检查功能，这是 WinForms 版本没有的特性。

csharp:

```c#
// 获取下一个拼写错误的位置
public TextPointer GetNextSpellingErrorPosition(TextPointer position, LogicalDirection direction);

// 获取指定位置的拼写错误
public SpellingError GetSpellingError(TextPointer position);

// 获取指定位置拼写错误的文本范围
public TextRange GetSpellingErrorRange(TextPointer position);
```

#### 工业场景应用

- 检查操作员输入的备注信息是否有拼写错误
- 自动修正常见的拼写错误
- 高亮显示拼写错误的单词

#### 使用示例

csharp:

```c#
// 检查整个文档的拼写错误
TextPointer current = richTextBox.Document.ContentStart;
while (current != null)
{
    TextPointer errorStart = richTextBox.GetNextSpellingErrorPosition(current, LogicalDirection.Forward);
    if (errorStart == null) break;

    TextRange errorRange = richTextBox.GetSpellingErrorRange(errorStart);
    // 高亮显示拼写错误
    errorRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
    
    current = errorRange.End;
}
```

### 5.2 GetPositionFromPoint 方法

csharp:

```c#
public TextPointer GetPositionFromPoint(Point point, bool snapToText);
```

**官方定义**：将屏幕坐标转换为文档中的 TextPointer 位置。

#### 参数说明

- `point`：相对于 RichTextBox 左上角的屏幕坐标
- `snapToText`：如果点不在任何文本上，是否自动吸附到最近的文本位置

#### 工业场景应用

这是实现交互功能的核心方法：

- 右键菜单定位：点击右键时获取点击位置的报警条目
- 点击跳转：点击报警 ID 跳转到对应的设备详情页
- 拖放操作：实现拖放文本到 RichTextBox

#### 使用示例

csharp:

```c#
private void richTextBox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
{
    Point position = e.GetPosition(richTextBox);
    TextPointer pointer = richTextBox.GetPositionFromPoint(position, true);
    
    // 获取点击位置所在的段落
    Paragraph paragraph = pointer.Paragraph;
    if (paragraph != null)
    {
        // 显示针对该段落的右键菜单
        ShowContextMenu(paragraph);
    }
}
```

### 5.3 ShouldSerializeDocument 方法

csharp:

```c#
public bool ShouldSerializeDocument();
```

**官方定义**：指示设计器是否应该序列化`Document`属性。

#### 官方实现逻辑

csharp:

```c#
public bool ShouldSerializeDocument()
{
    // 只有当Document不是默认的空文档时才序列化
    return Document != null && Document.Blocks.Count > 0;
}
```

#### 作用

- 控制 Visual Studio 设计器的序列化行为
- 避免序列化空文档，减少 XAML 文件大小
- 设计器会自动调用这个方法，不需要手动调用

### 5.4 重写的布局与生命周期方法

csharp:

```c#
// 测量控件所需大小
protected override Size MeasureOverride(Size constraint);

// 创建自动化对等体
protected override AutomationPeer OnCreateAutomationPeer();

// 处理DPI变化
protected override void OnDpiChanged(DpiScale oldDpiScaleInfo, DpiScale newDpiScaleInfo);
```

#### 1. MeasureOverride

- WPF 布局系统的核心方法
- RichTextBox 重写它来根据 FlowDocument 的内容计算自己的所需大小
- 不需要手动调用，布局系统会自动调用

#### 2. OnCreateAutomationPeer

- 为 RichTextBox 创建自动化对等体，支持 UI 自动化
- 工业系统中用于屏幕阅读器、自动化测试等无障碍功能
- 官方返回`RichTextBoxAutomationPeer`实例

#### 3. OnDpiChanged

- .NET Framework 4.6.2 + 新增的方法
- 处理高 DPI 屏幕的显示问题
- 确保 RichTextBox 在不同 DPI 设置下都能清晰显示
- 工业场景中对于 4K 显示器的支持至关重要

------

## 六、官方类设计总结

### 6.1 核心设计哲学

RichTextBox 的官方设计严格遵循了**单一职责原则**：

- RichTextBox 只负责**显示和编辑**FlowDocument
- FlowDocument 只负责**存储和表示**富文本内容
- TextPointer/TextRange 只负责**操作**文本和格式

这种分离设计使得：

- 同一个 FlowDocument 可以在多个 RichTextBox 之间共享
- 内容可以独立于 UI 进行操作和持久化
- 易于扩展和维护

### 6.2 与 WinForms RichTextBox 的本质区别

| 特性         | WPF RichTextBox           | WinForms RichTextBox |
| :----------- | :------------------------ | :------------------- |
| **内容模型** | FlowDocument 强类型对象树 | RTF 字符串           |
| **操作方式** | TextPointer/TextRange     | 整数索引             |
| **拼写检查** | 内置多语言支持            | 无                   |
| **扩展能力** | 可嵌入任意 WPF 控件       | 有限                 |
| **DPI 支持** | 原生支持高 DPI            | 模糊                 |

### 6.3 工业开发最佳实践

1. **优先使用 Document 操作内容**：不要将 RichTextBox 转换为字符串再操作，直接操作 FlowDocument 对象树
2. **只读模式**：报警日志、操作记录等显示控件必须设置`IsReadOnly="True"`
3. **禁用交互**：不需要超链接时保持`IsDocumentEnabled="False"`
4. **性能优化**：
   - 批量更新时使用`richTextBox.BeginChange()`和`EndChange()`
   - 限制最大行数，避免内存无限增长
   - 静态内容使用 TextBlock 而非 RichTextBox

这个官方类定义揭示了 WPF RichTextBox 的核心本质：它不是一个简单的文本框，而是一个**完整的流文档显示和编辑容器**。理解这个设计本质，是正确使用和扩展 RichTextBox 的关键。