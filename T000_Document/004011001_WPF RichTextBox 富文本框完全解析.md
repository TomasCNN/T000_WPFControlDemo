# 004011001_WPF RichTextBox 富文本框完全解析

WPF 的`RichTextBox`是基于**FlowDocument 流文档模型**的新一代富文本编辑控件，相比 WinForms 版本，它提供了更强大的排版能力、更好的性能和更灵活的扩展机制。在工业自动化场景中，它是构建现代化报警日志、操作记录、生产报表和设备文档系统的首选控件。

## 一、官方类定义与继承关系

### 1.1 基本信息

- **命名空间**：`System.Windows.Controls`

- **程序集**：`PresentationFramework.dll`

- **继承链**：

  plaintext:

  ```tex
  Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → TextBoxBase → RichTextBox
  ```

### 1.2 官方类签名

csharp:

```c#
[LocalizabilityAttribute(LocalizationCategory.Text)]
[TemplatePartAttribute(Name = "PART_ContentHost", Type = typeof(ScrollViewer))]
public class RichTextBox : TextBoxBase
```

### 1.3 核心设计理念：FlowDocument 流文档模型

WPF `RichTextBox`与 WinForms 版本的**本质区别**在于内容模型：

- WinForms `RichTextBox`基于 RTF 格式字符串
- WPF `RichTextBox`基于**强类型的 FlowDocument 对象模型**

`FlowDocument`是 WPF 中用于表示流文档的根元素，它由一系列`Block`元素（如`Paragraph`、`Table`、`List`、`Section`）组成，每个`Block`又包含`Inline`元素（如`Run`、`Span`、`InlineUIContainer`、`Hyperlink`）。这种强类型模型比 RTF 字符串更易于操作和扩展。

### 1.4 核心成员分类

#### 1.4.1 关键属性

| 属性                            | 类型                  | 说明                                               |
| :------------------------------ | :-------------------- | :------------------------------------------------- |
| `Document`                      | `FlowDocument`        | 获取或设置富文本内容的根文档（核心属性）           |
| `IsReadOnly`                    | `bool`                | 获取或设置是否为只读模式（工业场景通常设为`true`） |
| `Selection`                     | `TextSelection`       | 获取当前选中的文本范围                             |
| `CaretPosition`                 | `TextPointer`         | 获取或设置光标位置                                 |
| `HorizontalScrollBarVisibility` | `ScrollBarVisibility` | 水平滚动条可见性                                   |
| `VerticalScrollBarVisibility`   | `ScrollBarVisibility` | 垂直滚动条可见性                                   |
| `AcceptsReturn`                 | `bool`                | 是否接受回车键                                     |
| `AcceptsTab`                    | `bool`                | 是否接受制表符                                     |
| `IsDocumentEnabled`             | `bool`                | 是否启用文档中的交互元素（如超链接）               |
| `PageWidth`                     | `double`              | 获取或设置页面宽度                                 |
| `PageHeight`                    | `double`              | 获取或设置页面高度                                 |

#### 1.4.2 核心方法

| 方法                                      | 说明                           |
| :---------------------------------------- | :----------------------------- |
| `SelectAll()`                             | 选中所有内容                   |
| `Clear()`                                 | 清除所有内容                   |
| `Copy()` / `Cut()` / `Paste()`            | 剪贴板操作                     |
| `Undo()` / `Redo()`                       | 撤销 / 重做操作                |
| `ScrollToEnd()`                           | 滚动到文档末尾（工业日志必备） |
| `ScrollToHome()`                          | 滚动到文档开头                 |
| `ScrollToHorizontalOffset(double offset)` | 水平滚动到指定位置             |
| `ScrollToVerticalOffset(double offset)`   | 垂直滚动到指定位置             |

#### 1.4.3 常用事件

| 事件                 | 触发时机                 |
| :------------------- | :----------------------- |
| `TextChanged`        | 文档内容发生变化时       |
| `SelectionChanged`   | 选中内容发生变化时       |
| `DocumentChanged`    | `Document`属性发生变化时 |
| `LinkClicked`        | 点击超链接时             |
| `ContextMenuOpening` | 上下文菜单打开时         |

## 二、核心功能详解

### 2.1 强大的流文档支持

`FlowDocument`提供了远超 RTF 的排版能力：

- 自适应页面布局（自动分页、分栏）
- 支持复杂的表格、列表和段落格式
- 嵌入任意 WPF 控件（按钮、图表、进度条等）
- 矢量图形和图像支持
- 超链接和交互元素
- 样式和模板支持
- 打印和文档导出

### 2.2 格式控制能力

- 字体、字号、粗体、斜体、下划线、删除线
- 文本颜色、背景色、高亮
- 段落对齐、缩进、行间距、段间距
- 项目符号和编号列表（支持多级）
- 首字下沉、边框、阴影
- 字符间距和基线偏移

### 2.3 编辑与交互功能

- 完整的剪贴板操作（支持多种格式）
- 多级撤销 / 重做（可自定义撤销栈）
- 拖放操作（支持文件、图像等）
- 拼写检查（内置多语言支持）
- 上下文菜单（可自定义）
- 查找和替换（需自行实现，但 API 友好）

### 2.4 文件操作

支持多种格式的加载和保存：

- **XAML 格式**：WPF 原生格式，保留所有流文档特性
- **RTF 格式**：与 WinForms 兼容
- **纯文本格式**
- **XPS 格式**：用于打印和电子文档
- **HTML 格式**（有限支持）

### 2.5 工业场景典型应用

1. **分级报警日志**：不同级别报警用不同颜色、图标和字体显示
2. **设备操作审计**：记录操作员的所有操作，带时间戳和操作人
3. **生产报表生成**：生成带表格、图表和格式的生产数据报表
4. **设备操作手册**：显示带图片、视频和交互元素的电子手册
5. **实时数据监控**：显示带高亮和闪烁效果的实时参数
6. **故障诊断报告**：自动生成带格式的故障分析报告

## 三、基础使用方法

### 3.1 创建 RichTextBox

#### 方式 1：XAML 声明（推荐）

xaml:

```xaml
<Window x:Class="WpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="WPF RichTextBox示例" Height="450" Width="800">
    <Grid>
        <!-- 工业场景常用配置 -->
        <RichTextBox x:Name="richTextBox"
                     IsReadOnly="True"
                     VerticalScrollBarVisibility="Auto"
                     HorizontalScrollBarVisibility="Disabled"
                     FontFamily="微软雅黑"
                     FontSize="12"
                     Background="White">
            <!-- 初始化空文档 -->
            <RichTextBox.Document>
                <FlowDocument>
                    <Paragraph>
                        <Run>欢迎使用工业报警日志系统</Run>
                    </Paragraph>
                </FlowDocument>
            </RichTextBox.Document>
        </RichTextBox>
    </Grid>
</Window>
```

#### 方式 2：代码创建

csharp:

```c#
public MainWindow()
{
    InitializeComponent();
    
    var richTextBox = new RichTextBox
    {
        IsReadOnly = true,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        FontFamily = new FontFamily("微软雅黑"),
        FontSize = 12,
        Background = Brushes.White,
        Document = new FlowDocument()
    };
    
    this.Content = richTextBox;
}
```

### 3.2 基本文本操作

#### 追加文本

csharp:

```c#
// 追加纯文本
Paragraph para = new Paragraph();
para.Inlines.Add(new Run($"[{DateTime.Now:HH:mm:ss}] 系统启动完成"));
richTextBox.Document.Blocks.Add(para);

// 追加带格式的文本
Paragraph alarmPara = new Paragraph();
alarmPara.Inlines.Add(new Run("[严重] ") { Foreground = Brushes.Red, FontWeight = FontWeights.Bold });
alarmPara.Inlines.Add(new Run("温度过高报警") { Foreground = Brushes.Black });
richTextBox.Document.Blocks.Add(alarmPara);

// 自动滚动到底部
richTextBox.ScrollToEnd();
```

#### 设置文本格式

csharp:

```c#
// 创建带多种格式的段落
var para = new Paragraph();

// 红色粗体时间戳
para.Inlines.Add(new Run($"[{DateTime.Now:HH:mm:ss.fff}] ") 
{ 
    Foreground = Brushes.Gray,
    FontSize = 11
});

// 橙色警告
para.Inlines.Add(new Run("[警告] ") 
{ 
    Foreground = Brushes.Orange,
    FontWeight = FontWeights.Bold
});

// 绿色操作员
para.Inlines.Add(new Run("[操作员A] ") 
{ 
    Foreground = Brushes.DarkGreen
});

// 黑色消息
para.Inlines.Add(new Run("压力接近上限值") 
{ 
    Foreground = Brushes.Black
});

richTextBox.Document.Blocks.Add(para);
```

#### 插入图片

csharp:

```c#
// 插入图片到文档
var para = new Paragraph();
var image = new Image
{
    Source = new BitmapImage(new Uri("pack://application:,,,/Images/alarm.png")),
    Width = 16,
    Height = 16,
    Margin = new Thickness(0, 0, 4, 0)
};

para.Inlines.Add(image);
para.Inlines.Add(new Run(" 温度过高报警"));
richTextBox.Document.Blocks.Add(para);
```

#### 插入表格

csharp:

```c#
// 创建报警详情表格
var table = new Table();
table.CellSpacing = 0;
table.BorderThickness = new Thickness(1);
table.BorderBrush = Brushes.LightGray;

// 添加列
table.Columns.Add(new TableColumn { Width = new GridLength(100) });
table.Columns.Add(new TableColumn { Width = new GridLength(300) });

// 添加行
var row1 = new TableRow();
row1.Cells.Add(new TableCell(new Paragraph(new Run("报警时间")) { Background = Brushes.LightGray }));
row1.Cells.Add(new TableCell(new Paragraph(new Run(DateTime.Now.ToString()))));
table.Rows.Add(row1);

var row2 = new TableRow();
row2.Cells.Add(new TableCell(new Paragraph(new Run("报警级别")) { Background = Brushes.LightGray }));
row2.Cells.Add(new TableCell(new Paragraph(new Run("严重")) { Foreground = Brushes.Red }));
table.Rows.Add(row2);

richTextBox.Document.Blocks.Add(table);
```

### 3.3 文件操作

csharp:

```c#
// 加载RTF文件
TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
using (FileStream fs = new FileStream("alarm_log.rtf", FileMode.Open))
{
    range.Load(fs, DataFormats.Rtf);
}

// 保存为RTF文件
TextRange saveRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
using (FileStream fs = new FileStream("alarm_log.rtf", FileMode.Create))
{
    saveRange.Save(fs, DataFormats.Rtf);
}

// 保存为XAML文件
using (FileStream fs = new FileStream("alarm_log.xaml", FileMode.Create))
{
    XamlWriter.Save(richTextBox.Document, fs);
}
```

### 3.4 跨线程安全更新（工业场景必备）

WPF 中所有 UI 元素只能在创建它们的线程（Dispatcher 线程）上访问，从后台线程更新必须使用`Dispatcher`：

csharp:

```c#
/// <summary>
/// 线程安全的追加报警日志方法
/// </summary>
public void AppendAlarmSafe(AlarmLevel level, string message, string operatorName = "System")
{
    if (!Dispatcher.CheckAccess())
    {
        Dispatcher.Invoke(new Action(() => AppendAlarmSafe(level, message, operatorName)));
        return;
    }

    var para = new Paragraph { Margin = new Thickness(0) };
    
    // 时间戳
    para.Inlines.Add(new Run($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] ") 
    { 
        Foreground = Brushes.Gray,
        FontSize = 11
    });

    // 报警级别
    switch (level)
    {
        case AlarmLevel.Info:
            para.Inlines.Add(new Run("[信息] ") { Foreground = Brushes.Blue });
            break;
        case AlarmLevel.Warning:
            para.Inlines.Add(new Run("[警告] ") { Foreground = Brushes.Orange, FontWeight = FontWeights.Bold });
            break;
        case AlarmLevel.Error:
            para.Inlines.Add(new Run("[错误] ") { Foreground = Brushes.Red, FontWeight = FontWeights.Bold });
            break;
        case AlarmLevel.Critical:
            para.Inlines.Add(new Run("[严重] ") 
            { 
                Foreground = Brushes.White,
                Background = Brushes.Red,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(2, 0, 2, 0)
            });
            break;
    }

    // 操作员
    para.Inlines.Add(new Run($"[{operatorName}] ") { Foreground = Brushes.DarkGreen });
    
    // 消息
    para.Inlines.Add(new Run(message) { Foreground = Brushes.Black });

    richTextBox.Document.Blocks.Add(para);
    
    // 自动滚动到底部
    richTextBox.ScrollToEnd();
}

// 后台线程调用示例
Task.Run(() =>
{
    for (int i = 0; i < 10; i++)
    {
        AppendAlarmSafe(AlarmLevel.Info, $"采集数据: {i}");
        Thread.Sleep(1000);
    }
});
```

## 四、完整工业实例：WPF 版报警日志控件

下面是一个工业级 WPF 报警日志控件，功能与之前 WinForms 版本完全对应，但利用 WPF 特性提供了更好的用户体验。

### 4.1 报警级别枚举

csharp:

```c#
public enum AlarmLevel
{
    Info,       // 信息（蓝色）
    Warning,    // 警告（橙色）
    Error,      // 错误（红色）
    Critical    // 严重（红底白字）
}
```

### 4.2 工业报警日志用户控件

#### XAML 部分 (`IndustrialAlarmLog.xaml`)

xaml:

```xaml
<UserControl x:Class="WpfApp.IndustrialAlarmLog"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008" 
             mc:Ignorable="d" 
             d:DesignHeight="450" d:DesignWidth="800">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- 工具栏 -->
        <ToolBar Grid.Row="0">
            <Button x:Name="btnSave" Content="保存日志" Click="BtnSave_Click"/>
            <Button x:Name="btnPrint" Content="打印" Click="BtnPrint_Click"/>
            <Button x:Name="btnClear" Content="清空" Click="BtnClear_Click"/>
            <Separator/>
            <ComboBox x:Name="cboFilter" Width="120" SelectionChanged="CboFilter_SelectionChanged">
                <ComboBoxItem Content="全部" IsSelected="True"/>
                <ComboBoxItem Content="信息"/>
                <ComboBoxItem Content="警告"/>
                <ComboBoxItem Content="错误"/>
                <ComboBoxItem Content="严重"/>
            </ComboBox>
        </ToolBar>

        <!-- RichTextBox -->
        <RichTextBox x:Name="richTextBox" Grid.Row="1"
                     IsReadOnly="True"
                     VerticalScrollBarVisibility="Auto"
                     HorizontalScrollBarVisibility="Disabled"
                     FontFamily="微软雅黑"
                     FontSize="12"
                     Background="White"
                     BorderThickness="0">
            <RichTextBox.Document>
                <FlowDocument PagePadding="5">
                    <Paragraph>
                        <Run Foreground="Gray">[系统] 报警日志系统已启动</Run>
                    </Paragraph>
                </FlowDocument>
            </RichTextBox.Document>
        </RichTextBox>
    </Grid>
</UserControl>
```

#### 代码部分 (`IndustrialAlarmLog.xaml.cs`)

csharp:

```c#
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Win32;

namespace WpfApp
{
    public partial class IndustrialAlarmLog : UserControl
    {
        // 存储所有报警记录（用于过滤）
        private readonly List<AlarmRecord> _alarmRecords = new List<AlarmRecord>();

        public IndustrialAlarmLog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 添加一条报警记录
        /// </summary>
        public void AddAlarm(AlarmLevel level, string message, string operatorName = "System")
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(() => AddAlarm(level, message, operatorName)));
                return;
            }

            // 存储记录
            var record = new AlarmRecord(level, message, operatorName, DateTime.Now);
            _alarmRecords.Add(record);

            // 如果当前过滤级别匹配，则显示
            if (IsRecordVisible(record))
            {
                AppendRecordToDocument(record);
            }
        }

        /// <summary>
        /// 将记录追加到文档
        /// </summary>
        private void AppendRecordToDocument(AlarmRecord record)
        {
            var para = new Paragraph { Margin = new Thickness(0, 2, 0, 2) };
            
            // 时间戳
            para.Inlines.Add(new Run($"[{record.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] ") 
            { 
                Foreground = Brushes.Gray,
                FontSize = 11
            });

            // 报警级别
            switch (record.Level)
            {
                case AlarmLevel.Info:
                    para.Inlines.Add(new Run("[信息] ") { Foreground = Brushes.Blue });
                    break;
                case AlarmLevel.Warning:
                    para.Inlines.Add(new Run("[警告] ") { Foreground = Brushes.Orange, FontWeight = FontWeights.Bold });
                    break;
                case AlarmLevel.Error:
                    para.Inlines.Add(new Run("[错误] ") { Foreground = Brushes.Red, FontWeight = FontWeights.Bold });
                    break;
                case AlarmLevel.Critical:
                    para.Inlines.Add(new Run("[严重] ") 
                    { 
                        Foreground = Brushes.White,
                        Background = Brushes.Red,
                        FontWeight = FontWeights.Bold,
                        Padding = new Thickness(2, 0, 2, 0)
                    });
                    break;
            }

            // 操作员
            para.Inlines.Add(new Run($"[{record.OperatorName}] ") { Foreground = Brushes.DarkGreen });
            
            // 消息
            para.Inlines.Add(new Run(record.Message) { Foreground = Brushes.Black });

            richTextBox.Document.Blocks.Add(para);
            richTextBox.ScrollToEnd();
        }

        /// <summary>
        /// 检查记录是否符合当前过滤条件
        /// </summary>
        private bool IsRecordVisible(AlarmRecord record)
        {
            var selectedItem = cboFilter.SelectedItem as ComboBoxItem;
            if (selectedItem == null || selectedItem.Content.ToString() == "全部")
                return true;

            return Enum.TryParse(selectedItem.Content.ToString(), out AlarmLevel level) 
                   && record.Level == level;
        }

        /// <summary>
        /// 刷新显示的记录
        /// </summary>
        private void RefreshDisplay()
        {
            richTextBox.Document.Blocks.Clear();
            foreach (var record in _alarmRecords)
            {
                if (IsRecordVisible(record))
                {
                    AppendRecordToDocument(record);
                }
            }
        }

        // 保存日志
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "RTF文件(*.rtf)|*.rtf|文本文件(*.txt)|*.txt|XAML文件(*.xaml)|*.xaml",
                FileName = $"报警日志_{DateTime.Now:yyyyMMddHHmmss}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                var range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                using (var fs = new FileStream(saveDialog.FileName, FileMode.Create))
                {
                    string format = saveDialog.FilterIndex switch
                    {
                        1 => DataFormats.Rtf,
                        2 => DataFormats.Text,
                        3 => DataFormats.Xaml,
                        _ => DataFormats.Rtf
                    };
                    range.Save(fs, format);
                }
                MessageBox.Show("日志保存成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // 打印日志
        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintDocument(((IDocumentPaginatorSource)richTextBox.Document).DocumentPaginator, "报警日志");
            }
        }

        // 清空日志
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定要清空所有日志吗？", "确认", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _alarmRecords.Clear();
                richTextBox.Document.Blocks.Clear();
            }
        }

        // 过滤级别变更
        private void CboFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshDisplay();
        }
    }

    /// <summary>
    /// 报警记录数据模型
    /// </summary>
    public class AlarmRecord
    {
        public AlarmLevel Level { get; }
        public string Message { get; }
        public string OperatorName { get; }
        public DateTime Timestamp { get; }

        public AlarmRecord(AlarmLevel level, string message, string operatorName, DateTime timestamp)
        {
            Level = level;
            Message = message;
            OperatorName = operatorName;
            Timestamp = timestamp;
        }
    }
}
```

### 4.3 使用示例

xaml:

```xaml
<!-- MainWindow.xaml -->
<Window x:Class="WpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:WpfApp"
        Title="工业报警日志系统" Height="600" Width="1000">
    <Grid>
        <local:IndustrialAlarmLog x:Name="alarmLog"/>
    </Grid>
</Window>
```

csharp:

```c#
// MainWindow.xaml.cs
public MainWindow()
{
    InitializeComponent();
    
    // 模拟报警
    alarmLog.AddAlarm(AlarmLevel.Info, "系统启动完成");
    alarmLog.AddAlarm(AlarmLevel.Warning, "压力接近上限值", "操作员A");
    alarmLog.AddAlarm(AlarmLevel.Error, "温度传感器故障", "系统");
    alarmLog.AddAlarm(AlarmLevel.Critical, "紧急停机", "操作员B");
}
```

## 五、常见问题与最佳实践

### 5.1 大文本性能优化

当文档包含超过 10 万行文本时，WPF `RichTextBox`性能会下降，优化方法：

1. **启用 UI 虚拟化**（WPF 4.5 + 支持）：

   xaml:

   ```xaml
   <RichTextBox IsDocumentEnabled="True">
       <RichTextBox.Resources>
           <Style TargetType="ScrollViewer">
               <Setter Property="CanContentScroll" Value="True"/>
           </Style>
       </RichTextBox.Resources>
   </RichTextBox>
   ```

2. **限制最大行数**：超过指定行数时自动删除最早的记录

   csharp:

   ```c#
   if (_alarmRecords.Count > 10000)
   {
       // 删除前1000条记录
       _alarmRecords.RemoveRange(0, 1000);
       RefreshDisplay();
   }
   ```

3. **批量更新**：批量添加记录时，先禁用文档的自动重绘

   csharp:

   ```c#
   richTextBox.BeginChange();
   // 批量添加记录
   foreach (var record in batchRecords)
   {
       AppendRecordToDocument(record);
   }
   richTextBox.EndChange();
   ```

4. **避免频繁的格式变化**：尽量复用相同格式的`Run`元素

### 5.2 MVVM 模式下的使用

WPF `RichTextBox`不支持直接绑定`Document`属性，MVVM 模式下推荐使用**附加属性**：

csharp:

```c#
public static class RichTextBoxHelper
{
    public static readonly DependencyProperty DocumentProperty = DependencyProperty.RegisterAttached(
        "Document", typeof(FlowDocument), typeof(RichTextBoxHelper),
        new PropertyMetadata(null, OnDocumentChanged));

    public static void SetDocument(DependencyObject element, FlowDocument value)
    {
        element.SetValue(DocumentProperty, value);
    }

    public static FlowDocument GetDocument(DependencyObject element)
    {
        return (FlowDocument)element.GetValue(DocumentProperty);
    }

    private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RichTextBox richTextBox)
        {
            richTextBox.Document = e.NewValue as FlowDocument;
        }
    }
}
```

**XAML 绑定**：

xaml:

```xaml
<RichTextBox local:RichTextBoxHelper.Document="{Binding AlarmDocument}"/>
```

### 5.3 工业场景特殊优化

1. **自动滚动**：添加新记录时自动滚动到底部，但如果用户手动向上滚动，则暂停自动滚动
2. **报警闪烁**：严重报警可以添加闪烁效果
3. **右键菜单**：添加复制、导出选中内容等实用功能
4. **搜索功能**：实现带高亮的文本搜索
5. **日志归档**：自动将旧日志归档到文件，只保留最近的日志在内存中

## 六、WinForms vs WPF RichTextBox 对比

| 特性       | WinForms RichTextBox   | WPF RichTextBox                |
| :--------- | :--------------------- | :----------------------------- |
| 内容模型   | RTF 字符串             | FlowDocument 对象模型          |
| 排版能力   | 基础                   | 强大（自适应、分栏、矢量图形） |
| 性能       | 小文本较好，大文本较差 | 整体更好，支持虚拟化           |
| 扩展能力   | 有限                   | 极强（可嵌入任意 WPF 控件）    |
| 打印支持   | 基础                   | 原生支持，质量高               |
| MVVM 支持  | 差                     | 较好（通过附加属性）           |
| 现代化 UI  | 差                     | 优秀（样式、模板、动画）       |
| RTF 兼容性 | 好                     | 有限                           |

## 总结

WPF `RichTextBox`是工业自动化软件中构建富文本界面的最佳选择，它基于 FlowDocument 的强类型模型提供了远超 WinForms 版本的功能和灵活性。通过合理使用格式设置、跨线程更新和性能优化，你可以构建出专业、可靠、美观的工业级报警日志和报表系统。