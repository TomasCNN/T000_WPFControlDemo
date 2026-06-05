# 004010003_WPF TextBlock 工业级实战案例

## 一、基础文本显示案例

### 1. 纯文本与基础样式

最常用的参数值显示，工业界面 90% 的文本都属于此类。

xaml:

```xaml
<!-- 普通标签文本 -->
<TextBlock Text="设备编号：" FontSize="12" Foreground="#333"/>

<!-- 数值文本（等宽字体+右对齐，工业标准） -->
<TextBlock Text="1234.56" 
           FontFamily="Consolas" 
           FontSize="12" 
           Foreground="#222"
           TextAlignment="Right"
           Width="80"/>

<!-- 标题文本 -->
<TextBlock Text="生产线1号机" 
           FontSize="16" 
           FontWeight="Bold" 
           Foreground="#2C3E50"/>
```

### 2. 状态文本（颜色区分）

工业界面最核心的状态显示，用颜色直观区分设备状态。

xaml:

```xaml
<!-- 正常状态 -->
<TextBlock Text="运行中" Foreground="#2ECC71" FontWeight="Bold"/>

<!-- 警告状态 -->
<TextBlock Text="预警" Foreground="#F39C12" FontWeight="Bold"/>

<!-- 错误状态 -->
<TextBlock Text="故障" Foreground="#E74C3C" FontWeight="Bold"/>

<!-- 离线状态 -->
<TextBlock Text="离线" Foreground="#95A5A6" FontWeight="Bold"/>
```

------

## 二、格式化文本案例（Inlines 集合）

同一个 TextBlock 中显示不同样式的文本，工业状态详情必备。

### 1. 混合格式状态显示

xaml:

```c#
<TextBlock>
    <Run Text="设备状态：" FontWeight="Bold" Foreground="#333"/>
    <Run Text="正常运行" Foreground="#2ECC71" FontSize="14"/>
    <LineBreak/> <!-- 换行 -->
    <Run Text="当前温度：" FontWeight="Bold" Foreground="#333"/>
    <Run Text="25.6℃" Foreground="#3498DB"/>
    <LineBreak/>
    <Run Text="报警信息：" FontWeight="Bold" Foreground="#333"/>
    <Run Text="无" Foreground="#999" TextDecorations="Strikethrough"/>
</TextBlock>
```

### 2. 带图标的文本

结合`InlineUIContainer`嵌入图标，工业界面常用。

xaml:

```xaml
<TextBlock VerticalAlignment="Center">
    <InlineUIContainer>
        <Image Source="/Images/Success.png" Width="16" Height="16" Margin="0 0 5 0"/>
    </InlineUIContainer>
    <Run Text="操作成功" Foreground="#2ECC71" FontWeight="Bold"/>
</TextBlock>
```

### 3. 超链接文本

用于显示帮助文档、系统链接等。

xaml:

```xaml
<TextBlock>
    <Run Text="点击查看"/>
    <Hyperlink NavigateUri="https://help.example.com" RequestNavigate="Hyperlink_RequestNavigate">
        <Run Text="详细帮助文档"/>
    </Hyperlink>
</TextBlock>
```

csharp:

```c#
private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
{
    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
    e.Handled = true;
}
```

------

## 三、文本排版控制案例

解决工业界面最常见的文本溢出、布局混乱问题。

### 1. 单行文本截断（防止界面撑破）

**工业开发必加**，所有固定宽度的单行文本都应该设置。

xaml:

```xaml
<!-- 超出部分显示... -->
<TextBlock Text="设备编号：PRO-2025-LINE-001-SERIAL-123456"
           Width="200"
           TextTrimming="CharacterEllipsis"
           ToolTip="{Binding Text, RelativeSource={RelativeSource Self}}"/>
```

- 配合`ToolTip`显示完整文本，鼠标悬浮即可查看
- 工业界面所有参数名称、设备名称都应该这样写

### 2. 多行文本自动换行

用于日志、描述信息等长文本显示。

xaml:

```xaml
<TextBlock Text="这是一段很长的设备描述信息，当超出TextBlock的宽度时会自动换行显示，不会撑破界面布局。"
           Width="300"
           TextWrapping="Wrap"
           LineHeight="18" <!-- 行高1.5倍，提升可读性 -->
           Padding="5"/>
```

### 3. 最大行数限制

用于日志预览、消息提示等场景，避免文本过长占用过多空间。

xaml:

```xaml
<TextBlock Text="这是一段很长的日志信息，最多显示3行，超出部分会被截断并显示省略号。完整日志可以点击查看详情。"
           Width="300"
           TextWrapping="Wrap"
           TextTrimming="CharacterEllipsis"
           MaxLines="3"
           LineHeight="18"/>
```

### 4. 文本对齐

工业界面标准对齐方式：标签左对齐，数值右对齐，标题居中。

xaml:

```xaml
<Grid Width="300">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>

    <!-- 标签左对齐 -->
    <TextBlock Grid.Row="0" Grid.Column="0" Text="温度：" Margin="0 5 10 5"/>
    <!-- 数值右对齐 -->
    <TextBlock Grid.Row="0" Grid.Column="1" Text="25.6℃" TextAlignment="Right" Margin="0 5"/>

    <TextBlock Grid.Row="1" Grid.Column="0" Text="压力：" Margin="0 5 10 5"/>
    <TextBlock Grid.Row="1" Grid.Column="1" Text="0.85MPa" TextAlignment="Right" Margin="0 5"/>
</Grid>
```

------

## 四、附加属性统一设置样式案例

工业界面有大量重复的文本样式，使用附加属性可以**一行代码统一整个容器的文本样式**，极大减少代码冗余。

xaml:

```xaml
<!-- 父容器设置，所有子TextBlock自动继承 -->
<GroupBox Header="设备参数" 
          Margin="10"
          TextBlock.FontSize="12"
          TextBlock.Foreground="#333"
          TextBlock.VerticalAlignment="Center">
    <Grid Margin="10">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- 无需单独设置字体和颜色 -->
        <TextBlock Grid.Row="0" Grid.Column="0" Text="设备名称：" Margin="0 5 10 5"/>
        <TextBlock Grid.Row="0" Grid.Column="1" Text="生产线1号机"/>

        <TextBlock Grid.Row="1" Grid.Column="0" Text="运行状态：" Margin="0 5 10 5"/>
        <TextBlock Grid.Row="1" Grid.Column="1" Text="正常运行" Foreground="#2ECC71" FontWeight="Bold"/>

        <TextBlock Grid.Row="2" Grid.Column="0" Text="生产数量：" Margin="0 5 10 5"/>
        <TextBlock Grid.Row="2" Grid.Column="1" Text="1234" FontFamily="Consolas" TextAlignment="Right"/>
    </Grid>
</GroupBox>
```

------

## 五、工业场景专用案例

### 1. 设备状态卡片

工业上位机最常用的组件，展示设备核心信息。

xaml:

```xaml
<Border Width="300" 
        Height="180" 
        BorderBrush="#DDD" 
        BorderThickness="1" 
        CornerRadius="4" 
        Background="#F9F9F9"
        Margin="10">
    <StackPanel Margin="15">
        <!-- 标题 -->
        <TextBlock Text="生产线1号机" 
                   FontSize="18" 
                   FontWeight="Bold" 
                   Foreground="#2C3E50"
                   Margin="0 0 0 15"/>

        <!-- 参数列表 -->
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>

            <TextBlock Grid.Row="0" Grid.Column="0" Text="运行状态：" Margin="0 5 10 5"/>
            <TextBlock Grid.Row="0" Grid.Column="1" Text="正常运行" Foreground="#2ECC71" FontWeight="Bold"/>

            <TextBlock Grid.Row="1" Grid.Column="0" Text="当前温度：" Margin="0 5 10 5"/>
            <TextBlock Grid.Row="1" Grid.Column="1" Text="25.6℃" Foreground="#3498DB"/>

            <TextBlock Grid.Row="2" Grid.Column="0" Text="当前压力：" Margin="0 5 10 5"/>
            <TextBlock Grid.Row="2" Grid.Column="1" Text="0.85MPa" Foreground="#3498DB"/>

            <TextBlock Grid.Row="3" Grid.Column="0" Text="生产数量：" Margin="0 5 10 5"/>
            <TextBlock Grid.Row="3" Grid.Column="1" Text="1,234" Foreground="#9B59B6" FontFamily="Consolas"/>
        </Grid>
    </StackPanel>
</Border>
```

### 2. 实时日志显示

工业上位机必备功能，展示设备运行日志。

xaml:

```xaml
<Border BorderBrush="#DDD" 
        BorderThickness="1" 
        CornerRadius="4" 
        Background="#F5F5F5"
        Margin="10">
    <ScrollViewer VerticalScrollBarVisibility="Auto" 
                  MaxHeight="300"
                  ScrollChanged="ScrollViewer_ScrollChanged">
        <TextBlock x:Name="txtLog"
                   TextWrapping="Wrap"
                   Padding="10"
                   FontFamily="Consolas"
                   FontSize="12"
                   LineHeight="18"/>
    </ScrollViewer>
</Border>
```

csharp:

```c#
// 日志更新方法（性能优化版）
private StringBuilder _logBuilder = new StringBuilder();

public void AddLog(string message)
{
    // 批量拼接，避免频繁赋值Text属性
    _logBuilder.AppendLine($"[{DateTime.Now:HH:mm:ss}] {message}");
    
    // 最多保留1000行日志，防止内存泄漏
    if (_logBuilder.Length > 100000)
    {
        _logBuilder.Remove(0, 50000);
    }
    
    // 只在UI线程更新一次
    Dispatcher.InvokeAsync(() =>
    {
        txtLog.Text = _logBuilder.ToString();
        // 自动滚动到底部
        scrollViewer.ScrollToEnd();
    });
}
```

### 3. 参数配置表单

工业界面最常见的表单布局，结合 Label 和 TextBlock。

xaml:

```xaml
<GroupBox Header="通信参数配置" Margin="10" Padding="10">
    <Grid TextBlock.FontSize="12" TextBlock.VerticalAlignment="Center">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto" SharedSizeGroup="Label"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Label用于输入框标签，支持Alt快捷键 -->
        <Label Grid.Row="0" Grid.Column="0" Content="_波特率：" Target="{Binding ElementName=txtBaudRate}"/>
        <TextBox Grid.Row="0" Grid.Column="1" x:Name="txtBaudRate" Text="9600" Margin="5"/>

        <Label Grid.Row="1" Grid.Column="0" Content="数据_位：" Target="{Binding ElementName=txtDataBits}"/>
        <TextBox Grid.Row="1" Grid.Column="1" x:Name="txtDataBits" Text="8" Margin="5"/>

        <Label Grid.Row="2" Grid.Column="0" Content="_校验位：" Target="{Binding ElementName=cmbParity}"/>
        <ComboBox Grid.Row="2" Grid.Column="1" x:Name="cmbParity" Margin="5" SelectedIndex="0">
            <ComboBoxItem Content="无校验"/>
            <ComboBoxItem Content="奇校验"/>
            <ComboBoxItem Content="偶校验"/>
        </ComboBox>

        <Label Grid.Row="3" Grid.Column="0" Content="停止_位：" Target="{Binding ElementName=txtStopBits}"/>
        <TextBox Grid.Row="3" Grid.Column="1" x:Name="txtStopBits" Text="1" Margin="5"/>
    </Grid>
</GroupBox>
```

------

## 六、性能优化案例

### 1. 批量更新文本（错误 vs 正确）

❌ **错误写法**（性能极差，每次赋值都重新格式化整个文本）：

csharp:

```c#
// 循环1000次，会触发1000次布局和渲染
for (int i = 0; i < 1000; i++)
{
    txtLog.Text += i.ToString() + Environment.NewLine;
}
```

✅ **正确写法**（性能提升 100 倍以上）：

csharp:

```c#
// 批量拼接，只触发1次布局和渲染
StringBuilder sb = new StringBuilder();
for (int i = 0; i < 1000; i++)
{
    sb.AppendLine(i.ToString());
}
txtLog.Text = sb.ToString();
```

### 2. 避免频繁修改 Text 属性

工业场景中经常有每秒更新多次的实时数据，直接修改 Text 属性会导致频繁的布局和渲染。

✅ **优化方案**：使用节流机制，限制更新频率

csharp:

```c#
private DateTime _lastUpdateTime = DateTime.MinValue;
private double _currentValue;

public void UpdateValue(double value)
{
    _currentValue = value;
    
    // 最多每秒更新10次
    if ((DateTime.Now - _lastUpdateTime).TotalMilliseconds >= 100)
    {
        _lastUpdateTime = DateTime.Now;
        txtValue.Text = value.ToString("F2");
    }
}
```

------

## 七、常见问题解决方案案例

### 1. 文本模糊问题

工业界面必须开启像素对齐，否则文本会发虚。

xaml:

```xaml
<TextBlock Text="设备运行中"
           UseLayoutRounding="True"
           SnapsToDevicePixels="True"/>
```

### 2. 文本垂直居中问题

TextBlock 默认垂直居中，但有时候会因为行高问题出现偏移。

xaml:

```xaml
<TextBlock Text="25.6℃"
           VerticalAlignment="Center"
           LineHeight="24"
           LineStackingStrategy="BlockLineHeight"/>
```

### 3. 等宽字体对齐问题

数值显示必须使用等宽字体，否则小数点无法对齐。

xaml:

```xaml
<!-- 所有数值都用Consolas等宽字体 -->
<TextBlock Text="123.45" FontFamily="Consolas" TextAlignment="Right" Width="80"/>
<TextBlock Text="67.8" FontFamily="Consolas" TextAlignment="Right" Width="80"/>
<TextBlock Text="9.0" FontFamily="Consolas" TextAlignment="Right" Width="80"/>
```



------

## 八、案例总结

所有案例都严格遵循工业开发最佳实践：

1. **性能优先**：使用 StringBuilder 批量更新，避免频繁修改 Text 属性
2. **布局规范**：标签左对齐，数值右对齐，单行文本必加截断
3. **样式统一**：使用附加属性统一设置容器内文本样式
4. **用户体验**：状态用颜色区分，长文本加 ToolTip，日志自动滚动
5. **健壮性**：限制日志行数，防止内存泄漏，开启像素对齐