# 004011004_WPF RichTextBox 搜索功能完全实现指南

WPF RichTextBox 的搜索功能与 WinForms 版本有本质区别 —— 它基于**TextPointer 文本指针模型**而非简单的字符串索引。这使得搜索功能更强大，但实现也更复杂。本文将从基础原理到工业级完整实现，全面讲解 WPF RichTextBox 搜索功能的开发方法。

## 一、核心原理：WPF 文本模型基础

### 1.1 关键概念

- **FlowDocument**：RichTextBox 的内容根元素，由多个 Block（段落、表格等）组成
- **TextPointer**：表示文档中两个字符之间的位置，是 WPF 操作文本的核心
- **TextRange**：表示两个 TextPointer 之间的文本范围
- **LogicalDirection**：文本遍历方向（Forward/Backward）

### 1.2 搜索基本流程

WPF 搜索的本质是：

1. 从起始 TextPointer 开始，按指定方向遍历文档
2. 逐个读取文本段（Run）并查找匹配字符串
3. 找到匹配后，创建 TextRange 并应用高亮格式
4. 滚动到匹配位置并选中

## 二、基础搜索功能实现

### 2.1 最简单的全文搜索

csharp:

```c#
/// <summary>
/// 在RichTextBox中查找指定文本
/// </summary>
/// <param name="richTextBox">目标RichTextBox</param>
/// <param name="searchText">要查找的文本</param>
/// <returns>找到的文本范围，未找到返回null</returns>
public static TextRange FindText(RichTextBox richTextBox, string searchText)
{
    if (string.IsNullOrEmpty(searchText))
        return null;

    TextPointer start = richTextBox.Document.ContentStart;
    TextPointer end = richTextBox.Document.ContentEnd;
    
    return FindTextInRange(start, end, searchText, StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// 在指定范围内查找文本
/// </summary>
private static TextRange FindTextInRange(TextPointer start, TextPointer end, 
    string searchText, StringComparison comparison)
{
    TextPointer current = start;
    
    while (current != null && current.CompareTo(end) < 0)
    {
        // 检查当前位置是否是文本
        if (current.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
        {
            // 获取当前文本段的内容
            string textRun = current.GetTextInRun(LogicalDirection.Forward);
            
            // 在文本段中查找匹配
            int index = textRun.IndexOf(searchText, comparison);
            if (index >= 0)
            {
                // 计算匹配位置的TextPointer
                TextPointer matchStart = current.GetPositionAtOffset(index);
                TextPointer matchEnd = matchStart.GetPositionAtOffset(searchText.Length);
                return new TextRange(matchStart, matchEnd);
            }
        }
        
        // 移动到下一个文本上下文
        current = current.GetNextContextPosition(LogicalDirection.Forward);
    }
    
    return null;
}
```

### 2.2 高亮显示匹配结果

csharp:

```c#
/// <summary>
/// 高亮显示指定文本范围
/// </summary>
public static void HighlightRange(TextRange range, Brush highlightBrush)
{
    if (range == null)
        return;
    
    // 应用背景色
    range.ApplyPropertyValue(TextElement.BackgroundProperty, highlightBrush);
    
    // 选中并滚动到视图
    range.Start.Paragraph.BringIntoView();
}

/// <summary>
/// 清除所有高亮
/// </summary>
public static void ClearAllHighlights(RichTextBox richTextBox)
{
    TextRange wholeDocument = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
    wholeDocument.ApplyPropertyValue(TextElement.BackgroundProperty, null);
}
```

## 三、完整工业级搜索功能实现

下面是一个包含**查找下一个 / 上一个、循环查找、高亮、大小写敏感、全字匹配**的完整搜索实现。

### 3.1 XAML 界面

xaml:

```xaml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>
    
    <!-- 搜索工具栏 -->
    <ToolBar Grid.Row="0">
        <TextBox x:Name="txtSearch" Width="150" KeyDown="TxtSearch_KeyDown"
                 Watermark="输入搜索内容..."/>
        <Button x:Name="btnFindNext" Content="查找下一个" Click="BtnFindNext_Click"/>
        <Button x:Name="btnFindPrev" Content="查找上一个" Click="BtnFindPrev_Click"/>
        <Button x:Name="btnClearHighlight" Content="清除高亮" Click="BtnClearHighlight_Click"/>
        <Separator/>
        <CheckBox x:Name="chkCaseSensitive" Content="区分大小写"/>
        <CheckBox x:Name="chkWholeWord" Content="全字匹配"/>
    </ToolBar>
    
    <!-- RichTextBox -->
    <RichTextBox x:Name="richTextBox" Grid.Row="1"
                 VerticalScrollBarVisibility="Auto"/>
</Grid>
```

### 3.2 后台代码实现

csharp:

```c#
public partial class SearchableRichTextBox : UserControl
{
    // 保存上次搜索位置和文本
    private TextPointer _lastSearchPosition;
    private string _currentSearchText;
    private readonly Brush _highlightBrush = Brushes.Yellow;

    public SearchableRichTextBox()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 查找下一个匹配项
    /// </summary>
    private void FindNext()
    {
        string searchText = txtSearch.Text.Trim();
        if (string.IsNullOrEmpty(searchText))
            return;

        // 如果搜索文本改变，重置搜索位置
        if (searchText != _currentSearchText)
        {
            ClearAllHighlights(richTextBox);
            _lastSearchPosition = null;
            _currentSearchText = searchText;
        }

        TextPointer startPosition;
        if (_lastSearchPosition == null)
        {
            // 从头开始搜索
            startPosition = richTextBox.Document.ContentStart;
        }
        else
        {
            // 从上次匹配位置的下一个字符开始
            startPosition = _lastSearchPosition.GetPositionAtOffset(1);
        }

        TextRange foundRange = FindTextInRange(
            startPosition, 
            richTextBox.Document.ContentEnd, 
            searchText,
            chkCaseSensitive.IsChecked == true ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase,
            chkWholeWord.IsChecked == true);

        if (foundRange != null)
        {
            // 清除之前的高亮
            ClearAllHighlights(richTextBox);
            
            // 高亮新匹配项
            HighlightRange(foundRange, _highlightBrush);
            
            // 保存当前位置
            _lastSearchPosition = foundRange.End;
        }
        else
        {
            // 循环查找：到达末尾后从头开始
            if (MessageBox.Show("已到达文档末尾，是否从头开始搜索？", "搜索", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _lastSearchPosition = null;
                FindNext();
            }
        }
    }

    /// <summary>
    /// 查找上一个匹配项
    /// </summary>
    private void FindPrevious()
    {
        string searchText = txtSearch.Text.Trim();
        if (string.IsNullOrEmpty(searchText))
            return;

        if (searchText != _currentSearchText)
        {
            ClearAllHighlights(richTextBox);
            _lastSearchPosition = null;
            _currentSearchText = searchText;
        }

        TextPointer startPosition;
        if (_lastSearchPosition == null)
        {
            // 从末尾开始搜索
            startPosition = richTextBox.Document.ContentEnd;
        }
        else
        {
            // 从上次匹配位置的前一个字符开始
            startPosition = _lastSearchPosition.GetPositionAtOffset(-1);
        }

        TextRange foundRange = FindTextInRangeBackward(
            startPosition, 
            richTextBox.Document.ContentStart, 
            searchText,
            chkCaseSensitive.IsChecked == true ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase,
            chkWholeWord.IsChecked == true);

        if (foundRange != null)
        {
            ClearAllHighlights(richTextBox);
            HighlightRange(foundRange, _highlightBrush);
            _lastSearchPosition = foundRange.Start;
        }
        else
        {
            if (MessageBox.Show("已到达文档开头，是否从末尾开始搜索？", "搜索", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _lastSearchPosition = null;
                FindPrevious();
            }
        }
    }

    /// <summary>
    /// 正向查找文本（支持全字匹配）
    /// </summary>
    private TextRange FindTextInRange(TextPointer start, TextPointer end, 
        string searchText, StringComparison comparison, bool wholeWord = false)
    {
        TextPointer current = start;
        
        while (current != null && current.CompareTo(end) < 0)
        {
            if (current.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
            {
                string textRun = current.GetTextInRun(LogicalDirection.Forward);
                int index = textRun.IndexOf(searchText, comparison);
                
                while (index >= 0)
                {
                    TextPointer matchStart = current.GetPositionAtOffset(index);
                    TextPointer matchEnd = matchStart.GetPositionAtOffset(searchText.Length);
                    
                    // 检查全字匹配
                    if (!wholeWord || IsWholeWord(matchStart, matchEnd))
                    {
                        return new TextRange(matchStart, matchEnd);
                    }
                    
                    // 继续查找下一个匹配
                    index = textRun.IndexOf(searchText, index + 1, comparison);
                }
            }
            
            current = current.GetNextContextPosition(LogicalDirection.Forward);
        }
        
        return null;
    }

    /// <summary>
    /// 反向查找文本
    /// </summary>
    private TextRange FindTextInRangeBackward(TextPointer start, TextPointer end, 
        string searchText, StringComparison comparison, bool wholeWord = false)
    {
        TextPointer current = start;
        
        while (current != null && current.CompareTo(end) > 0)
        {
            if (current.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.Text)
            {
                string textRun = current.GetTextInRun(LogicalDirection.Backward);
                int index = textRun.LastIndexOf(searchText, comparison);
                
                while (index >= 0)
                {
                    TextPointer matchStart = current.GetPositionAtOffset(-(textRun.Length - index));
                    TextPointer matchEnd = matchStart.GetPositionAtOffset(searchText.Length);
                    
                    if (!wholeWord || IsWholeWord(matchStart, matchEnd))
                    {
                        return new TextRange(matchStart, matchEnd);
                    }
                    
                    index = textRun.LastIndexOf(searchText, index - 1, comparison);
                }
            }
            
            current = current.GetNextContextPosition(LogicalDirection.Backward);
        }
        
        return null;
    }

    /// <summary>
    /// 检查是否是全字匹配
    /// </summary>
    private bool IsWholeWord(TextPointer start, TextPointer end)
    {
        // 检查单词前边界
        bool isStartOfWord = start.GetPointerContext(LogicalDirection.Backward) != TextPointerContext.Text ||
                             !char.IsLetterOrDigit(start.GetCharacterAtPosition(LogicalDirection.Backward));
        
        // 检查单词后边界
        bool isEndOfWord = end.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text ||
                           !char.IsLetterOrDigit(end.GetCharacterAtPosition(LogicalDirection.Forward));
        
        return isStartOfWord && isEndOfWord;
    }

    // 事件处理
    private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            FindNext();
        }
    }

    private void BtnFindNext_Click(object sender, RoutedEventArgs e)
    {
        FindNext();
    }

    private void BtnFindPrev_Click(object sender, RoutedEventArgs e)
    {
        FindPrevious();
    }

    private void BtnClearHighlight_Click(object sender, RoutedEventArgs e)
    {
        ClearAllHighlights(richTextBox);
        _lastSearchPosition = null;
        txtSearch.Clear();
    }
}
```

## 四、高级功能扩展

### 4.1 批量高亮所有匹配项

csharp:

```c#
/// <summary>
/// 高亮文档中所有匹配项
/// </summary>
public void HighlightAllMatches(string searchText)
{
    ClearAllHighlights(richTextBox);
    
    if (string.IsNullOrEmpty(searchText))
        return;

    TextPointer current = richTextBox.Document.ContentStart;
    int matchCount = 0;

    while (current != null && current.CompareTo(richTextBox.Document.ContentEnd) < 0)
    {
        var range = FindTextInRange(current, richTextBox.Document.ContentEnd, searchText, 
            StringComparison.OrdinalIgnoreCase);
        
        if (range == null)
            break;

        range.ApplyPropertyValue(TextElement.BackgroundProperty, _highlightBrush);
        current = range.End;
        matchCount++;
    }

    MessageBox.Show($"找到 {matchCount} 个匹配项", "搜索结果", 
        MessageBoxButton.OK, MessageBoxImage.Information);
}
```

### 4.2 异步搜索（大文档优化）

对于超过 10 万行的大文档，同步搜索会阻塞 UI，应该使用异步搜索：

csharp:

```c#
/// <summary>
/// 异步查找文本
/// </summary>
public async Task<TextRange> FindTextAsync(string searchText, IProgress<int> progress)
{
    return await Task.Run(() =>
    {
        TextPointer current = richTextBox.Document.ContentStart;
        int totalLength = new TextRange(richTextBox.Document.ContentStart, 
            richTextBox.Document.ContentEnd).Text.Length;
        int processedLength = 0;

        while (current != null && current.CompareTo(richTextBox.Document.ContentEnd) < 0)
        {
            if (current.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
            {
                string textRun = current.GetTextInRun(LogicalDirection.Forward);
                processedLength += textRun.Length;
                
                // 报告进度
                progress.Report((int)((double)processedLength / totalLength * 100));

                int index = textRun.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    TextPointer matchStart = current.GetPositionAtOffset(index);
                    TextPointer matchEnd = matchStart.GetPositionAtOffset(searchText.Length);
                    return new TextRange(matchStart, matchEnd);
                }
            }

            current = current.GetNextContextPosition(LogicalDirection.Forward);
        }

        return null;
    });
}
```

### 4.3 搜索结果导航

csharp:

```c#
/// <summary>
/// 搜索结果导航器
/// </summary>
public class SearchResultNavigator
{
    private readonly RichTextBox _richTextBox;
    private readonly List<TextRange> _results = new();
    private int _currentIndex = -1;

    public SearchResultNavigator(RichTextBox richTextBox)
    {
        _richTextBox = richTextBox;
    }

    /// <summary>
    /// 查找所有匹配项
    /// </summary>
    public void FindAll(string searchText)
    {
        _results.Clear();
        _currentIndex = -1;

        TextPointer current = _richTextBox.Document.ContentStart;
        while (current != null)
        {
            var range = FindTextInRange(current, _richTextBox.Document.ContentEnd, searchText);
            if (range == null)
                break;

            _results.Add(range);
            current = range.End;
        }
    }

    /// <summary>
    /// 导航到下一个结果
    /// </summary>
    public void GoToNext()
    {
        if (_results.Count == 0)
            return;

        _currentIndex = (_currentIndex + 1) % _results.Count;
        HighlightCurrentResult();
    }

    /// <summary>
    /// 导航到上一个结果
    /// </summary>
    public void GoToPrevious()
    {
        if (_results.Count == 0)
            return;

        _currentIndex = (_currentIndex - 1 + _results.Count) % _results.Count;
        HighlightCurrentResult();
    }

    private void HighlightCurrentResult()
    {
        ClearAllHighlights(_richTextBox);
        var range = _results[_currentIndex];
        range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
        range.Start.Paragraph.BringIntoView();
    }
}
```

## 五、常见问题与解决方案

### 5.1 搜索不到包含特殊字符的文本

**问题**：包含换行符、制表符等特殊字符的文本搜索不到。

**解决方案**：在搜索前统一处理文本中的空白字符：

csharp:

```c#
private string NormalizeText(string text)
{
    return text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\t", " ");
}
```

### 5.2 高亮后文本格式丢失

**问题**：应用高亮背景色后，原有的文本格式（如粗体、颜色）丢失。

**解决方案**：使用`TextRange.GetPropertyValue`保留原有格式，只修改背景色：

csharp:

```c#
// 错误方式：会覆盖所有格式
range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);

// 正确方式：只修改背景色，保留其他格式
var existingBackground = range.GetPropertyValue(TextElement.BackgroundProperty);
if (existingBackground == DependencyProperty.UnsetValue)
{
    range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
}
```

### 5.3 大文档搜索性能差

**优化方案**：

1. 限制单次搜索的最大范围
2. 使用后台线程异步搜索
3. 对文档建立索引（适合静态文档）
4. 分页搜索，只搜索可视区域附近的内容

### 5.4 滚动不正确

**问题**：找到匹配项后没有正确滚动到视图中。

**解决方案**：使用`BringIntoView`方法，并确保段落是可见的：

csharp:

```c#
   1// 确保匹配项所在的段落可见2range.Start.Paragraph.BringIntoView();34// 额外滚动一点，让匹配项在视图中间5var scrollViewer = GetScrollViewer(richTextBox);6if (scrollViewer != null)7{8    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - scrollViewer.ViewportHeight / 2);9}1011// 获取RichTextBox的ScrollViewer12private ScrollViewer GetScrollViewer(DependencyObject element)13{14    if (element is ScrollViewer viewer)15        return viewer;1617    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)18    {19        var child = VisualTreeHelper.GetChild(element, i);20        var result = GetScrollViewer(child);21        if (result != null)22            return result;23    }2425    return null;26}c#
```

## 六、最佳实践总结

1. **使用 TextPointer 而非字符串索引**：WPF 的文本模型是基于指针的，不要尝试将文档转换为字符串再搜索
2. **处理跨 Run 的匹配**：当搜索文本跨越多个 Run 元素时，需要特殊处理
3. **保留原有格式**：只修改需要的属性（如背景色），不要覆盖其他格式
4. **优化大文档性能**：使用异步搜索和进度提示，避免 UI 阻塞
5. **提供完整的搜索体验**：支持查找下一个 / 上一个、循环查找、全字匹配、大小写敏感
6. **及时清除高亮**：搜索结束或文本改变时，清除所有高亮效果