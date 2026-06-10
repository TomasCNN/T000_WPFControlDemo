# 004002005_WPF ContentControl 实用实例合集（工业场景优先）

`ContentControl`是 WPF 中**最具复用价值的基类**，它的核心价值在于**分离 "容器逻辑" 与 "内容呈现"**。与 UserControl 相比，它不需要预先定义固定的内容结构，而是通过`Content`属性承载任意对象，通过`ContentTemplate`定义呈现方式，特别适合构建可高度定制的通用组件。

以下是**8 个工业自动化场景最常用的 ContentControl 完整实例**，全部遵循 MVVM 设计模式和工业系统最佳实践，可直接集成到项目中。

------

## 一、布局容器类

### 1.1 工业风格分组框（替代原生 GroupBox）

比原生 GroupBox 更简洁美观，支持自定义标题样式和边框，是工业界面分组显示的首选。

xaml:

```xaml
<!-- Themes/Generic.xaml -->
<Style TargetType="{x:Type local:IndustrialGroupBox}">
    <Setter Property="Padding" Value="10"/>
    <Setter Property="BorderBrush" Value="LightGray"/>
    <Setter Property="Background" Value="White"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:IndustrialGroupBox}">
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="1"
                        CornerRadius="3">
                    <Grid>
                        <Grid.RowDefinitions>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="*"/>
                        </Grid.RowDefinitions>
                        
                        <!-- 标题栏 -->
                        <Border Background="#FFE5E5E5"
                                Padding="10,5"
                                BorderBrush="{TemplateBinding BorderBrush}"
                                BorderThickness="0,0,0,1">
                            <TextBlock Text="{TemplateBinding Header}"
                                       FontSize="13"
                                       FontWeight="Bold"
                                       Foreground="#FF333333"/>
                        </Border>
                        
                        <!-- 内容区域 -->
                        <ContentPresenter Grid.Row="1"
                                          Margin="{TemplateBinding Padding}"/>
                    </Grid>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

csharp:

```c#
// IndustrialGroupBox.cs
public class IndustrialGroupBox : ContentControl
{
    static IndustrialGroupBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(IndustrialGroupBox), 
            new FrameworkPropertyMetadata(typeof(IndustrialGroupBox)));
    }

    // 标题
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(string), typeof(IndustrialGroupBox), 
        new PropertyMetadata(""));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
}
```

**使用示例**：

xaml:

```xaml
<local:IndustrialGroupBox Header="设备参数" Margin="10">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
        
        <TextBlock Text="IP地址：" Grid.Row="0" Grid.Column="0" Margin="0,5"/>
        <TextBox Text="192.168.1.100" Grid.Row="0" Grid.Column="1" Margin="5,0"/>
        
        <TextBlock Text="端口号：" Grid.Row="1" Grid.Column="0" Margin="0,5"/>
        <TextBox Text="502" Grid.Row="1" Grid.Column="1" Margin="5,0"/>
    </Grid>
</local:IndustrialGroupBox>
```

------

### 1.2 可关闭的内容面板

支持右上角关闭按钮，用于显示临时内容（如通知、提示、侧边栏）。

xaml:

```xaml
<Style TargetType="{x:Type local:ClosablePanel}">
    <Setter Property="Background" Value="White"/>
    <Setter Property="BorderBrush" Value="LightGray"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:ClosablePanel}">
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="3">
                    <Grid>
                        <Grid.RowDefinitions>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="*"/>
                        </Grid.RowDefinitions>
                        
                        <!-- 标题栏 -->
                        <Border Background="#FFF5F5F5" Padding="10,5">
                            <Grid>
                                <TextBlock Text="{TemplateBinding Title}"
                                           FontSize="13"
                                           FontWeight="Bold"
                                           VerticalAlignment="Center"/>
                                <Button x:Name="PART_CloseButton"
                                        Content="×"
                                        FontSize="16"
                                        FontWeight="Bold"
                                        Background="Transparent"
                                        BorderThickness="0"
                                        HorizontalAlignment="Right"
                                        VerticalAlignment="Center"
                                        Padding="5,0"/>
                            </Grid>
                        </Border>
                        
                        <!-- 内容区域 -->
                        <ContentPresenter Grid.Row="1" Margin="10"/>
                    </Grid>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

csharp:

```c#
public class ClosablePanel : ContentControl
{
    static ClosablePanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ClosablePanel), 
            new FrameworkPropertyMetadata(typeof(ClosablePanel)));
    }

    // 标题
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(ClosablePanel), 
        new PropertyMetadata(""));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // 关闭命令
    public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register(
        nameof(CloseCommand), typeof(ICommand), typeof(ClosablePanel), 
        new PropertyMetadata(null));

    public ICommand CloseCommand
    {
        get => (ICommand)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (GetTemplateChild("PART_CloseButton") is Button closeButton)
        {
            closeButton.Click += (s, e) => CloseCommand?.Execute(null);
        }
    }
}
```

------

## 二、数据展示类

### 2.1 键值对显示控件（工业参数必备）

工业系统中最常用的参数显示控件，支持自定义键和值的样式，可批量显示设备参数。

xaml:

```xaml
<Style TargetType="{x:Type local:KeyValueDisplay}">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:KeyValueDisplay}">
                <Grid Margin="{TemplateBinding Margin}">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="{TemplateBinding KeyWidth}"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>
                    
                    <!-- 键 -->
                    <TextBlock Text="{TemplateBinding Key}"
                               Foreground="{TemplateBinding KeyForeground}"
                               VerticalAlignment="Center"/>
                    
                    <!-- 值 -->
                    <TextBlock Grid.Column="1"
                               Text="{TemplateBinding Value}"
                               Foreground="{TemplateBinding ValueForeground}"
                               FontWeight="{TemplateBinding ValueFontWeight}"
                               VerticalAlignment="Center"/>
                </Grid>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

csharp:

```c#
public class KeyValueDisplay : ContentControl
{
    static KeyValueDisplay()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(KeyValueDisplay), 
            new FrameworkPropertyMetadata(typeof(KeyValueDisplay)));
    }

    // 键
    public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
        nameof(Key), typeof(string), typeof(KeyValueDisplay), 
        new PropertyMetadata(""));

    public string Key
    {
        get => (string)GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    // 值
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(string), typeof(KeyValueDisplay), 
        new PropertyMetadata(""));

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    // 键宽度
    public static readonly DependencyProperty KeyWidthProperty = DependencyProperty.Register(
        nameof(KeyWidth), typeof(GridLength), typeof(KeyValueDisplay), 
        new PropertyMetadata(new GridLength(100)));

    public GridLength KeyWidth
    {
        get => (GridLength)GetValue(KeyWidthProperty);
        set => SetValue(KeyWidthProperty, value);
    }

    // 键颜色
    public static readonly DependencyProperty KeyForegroundProperty = DependencyProperty.Register(
        nameof(KeyForeground), typeof(Brush), typeof(KeyValueDisplay), 
        new PropertyMetadata(Brushes.Gray));

    public Brush KeyForeground
    {
        get => (Brush)GetValue(KeyForegroundProperty);
        set => SetValue(KeyForegroundProperty, value);
    }

    // 值颜色
    public static readonly DependencyProperty ValueForegroundProperty = DependencyProperty.Register(
        nameof(ValueForeground), typeof(Brush), typeof(KeyValueDisplay), 
        new PropertyMetadata(Brushes.Black));

    public Brush ValueForeground
    {
        get => (Brush)GetValue(ValueForegroundProperty);
        set => SetValue(ValueForegroundProperty, value);
    }

    // 值字体粗细
    public static readonly DependencyProperty ValueFontWeightProperty = DependencyProperty.Register(
        nameof(ValueFontWeight), typeof(FontWeight), typeof(KeyValueDisplay), 
        new PropertyMetadata(FontWeights.Normal));

    public FontWeight ValueFontWeight
    {
        get => (FontWeight)GetValue(ValueFontWeightProperty);
        set => SetValue(ValueFontWeightProperty, value);
    }
}
```

**使用示例**：

xaml:

```xaml
<StackPanel Margin="10">
    <local:KeyValueDisplay Key="设备ID：" Value="PLC-001" ValueFontWeight="Bold"/>
    <local:KeyValueDisplay Key="设备名称：" Value="一号传送带" ValueFontWeight="Bold"/>
    <local:KeyValueDisplay Key="运行状态：" Value="运行中" ValueForeground="Green" ValueFontWeight="Bold"/>
    <local:KeyValueDisplay Key="当前速度：" Value="1.2 m/s"/>
    <local:KeyValueDisplay Key="当前负载：" Value="65%"/>
</StackPanel>
```

------

### 2.2 空状态提示控件

当列表、表格没有数据时显示友好的提示信息，提升用户体验。

xaml:

```xaml
<Style TargetType="{x:Type local:EmptyStateControl}">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:EmptyStateControl}">
                <Grid Visibility="{TemplateBinding IsEmpty, Converter={StaticResource BoolToVisibilityConverter}}">
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                    </Grid.RowDefinitions>
                    
                    <!-- 图标 -->
                    <TextBlock Text="{TemplateBinding Icon}"
                               FontSize="64"
                               Foreground="LightGray"
                               HorizontalAlignment="Center"/>
                    
                    <!-- 标题 -->
                    <TextBlock Grid.Row="1"
                               Text="{TemplateBinding Title}"
                               FontSize="16"
                               FontWeight="Bold"
                               Foreground="Gray"
                               HorizontalAlignment="Center"
                               Margin="0,20,0,0"/>
                    
                    <!-- 描述 -->
                    <TextBlock Grid.Row="2"
                               Text="{TemplateBinding Description}"
                               FontSize="12"
                               Foreground="LightGray"
                               HorizontalAlignment="Center"
                               Margin="0,10,0,0"/>
                    
                    <!-- 操作按钮 -->
                    <ContentPresenter Grid.Row="3"
                                      HorizontalAlignment="Center"
                                      Margin="0,20,0,0"/>
                </Grid>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

csharp:

```c#
public class EmptyStateControl : ContentControl
{
    static EmptyStateControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(EmptyStateControl), 
            new FrameworkPropertyMetadata(typeof(EmptyStateControl)));
    }

    // 是否为空
    public static readonly DependencyProperty IsEmptyProperty = DependencyProperty.Register(
        nameof(IsEmpty), typeof(bool), typeof(EmptyStateControl), 
        new PropertyMetadata(true));

    public bool IsEmpty
    {
        get => (bool)GetValue(IsEmptyProperty);
        set => SetValue(IsEmptyProperty, value);
    }

    // 图标
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(string), typeof(EmptyStateControl), 
        new PropertyMetadata("📭"));

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    // 标题
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(EmptyStateControl), 
        new PropertyMetadata("暂无数据"));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // 描述
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(EmptyStateControl), 
        new PropertyMetadata("请添加数据后重试"));

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
}
```

**使用示例**：

xaml:

```xaml
<Grid>
    <!-- 数据表格 -->
    <DataGrid ItemsSource="{Binding AlarmLogs}" Visibility="{Binding HasData, Converter={StaticResource BoolToVisibilityConverter}}"/>
    
    <!-- 空状态 -->
    <local:EmptyStateControl IsEmpty="{Binding !HasData}"
                             Title="暂无报警记录"
                             Description="系统运行正常，没有报警信息">
        <Button Content="刷新数据" Command="{Binding RefreshCommand}"/>
    </local:EmptyStateControl>
</Grid>
```

------

## 三、交互控件类

### 3.1 下拉按钮（带菜单）

点击按钮弹出下拉菜单，支持自定义按钮内容和菜单内容，比原生 ComboBox 更灵活。

xaml:

```xaml
<Style TargetType="{x:Type local:DropDownButton}">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:DropDownButton}">
                <Grid>
                    <!-- 主按钮 -->
                    <ToggleButton x:Name="PART_ToggleButton"
                                  Content="{TemplateBinding Content}"
                                  IsChecked="{Binding IsDropDownOpen, RelativeSource={RelativeSource TemplatedParent}}"/>
                    
                    <!-- 下拉菜单 -->
                    <Popup x:Name="PART_Popup"
                           Placement="Bottom"
                           IsOpen="{Binding IsDropDownOpen, RelativeSource={RelativeSource TemplatedParent}}"
                           StaysOpen="False">
                        <Border Background="White"
                                BorderBrush="LightGray"
                                BorderThickness="1"
                                Padding="5">
                            <ContentPresenter Content="{TemplateBinding DropDownContent}"/>
                        </Border>
                    </Popup>
                </Grid>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

csharp:

```c#
public class DropDownButton : ContentControl
{
    static DropDownButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownButton), 
            new FrameworkPropertyMetadata(typeof(DropDownButton)));
    }

    // 下拉内容
    public static readonly DependencyProperty DropDownContentProperty = DependencyProperty.Register(
        nameof(DropDownContent), typeof(object), typeof(DropDownButton), 
        new PropertyMetadata(null));

    public object DropDownContent
    {
        get => GetValue(DropDownContentProperty);
        set => SetValue(DropDownContentProperty, value);
    }

    // 是否展开下拉
    public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
        nameof(IsDropDownOpen), typeof(bool), typeof(DropDownButton), 
        new PropertyMetadata(false));

    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
}
```

**使用示例**：

xaml:

```xaml
<local:DropDownButton Content="导出">
    <local:DropDownButton.DropDownContent>
        <StackPanel>
            <MenuItem Header="导出Excel" Command="{Binding ExportExcelCommand}"/>
            <MenuItem Header="导出PDF" Command="{Binding ExportPdfCommand}"/>
            <MenuItem Header="导出CSV" Command="{Binding ExportCsvCommand}"/>
        </StackPanel>
    </local:DropDownButton.DropDownContent>
</local:DropDownButton>
```

------

### 3.2 确认操作按钮

点击后弹出确认对话框，防止误操作，工业系统中用于重要操作（如启动、停止、复位）。

xaml:

```xaml
<Style TargetType="{x:Type local:ConfirmButton}">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:ConfirmButton}">
                <Button x:Name="PART_Button"
                        Content="{TemplateBinding Content}"
                        Style="{TemplateBinding ButtonStyle}"/>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

csharp:

```c#
public class ConfirmButton : ContentControl
{
    static ConfirmButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ConfirmButton), 
            new FrameworkPropertyMetadata(typeof(ConfirmButton)));
    }

    // 按钮样式
    public static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register(
        nameof(ButtonStyle), typeof(Style), typeof(ConfirmButton), 
        new PropertyMetadata(null));

    public Style ButtonStyle
    {
        get => (Style)GetValue(ButtonStyleProperty);
        set => SetValue(ButtonStyleProperty, value);
    }

    // 确认标题
    public static readonly DependencyProperty ConfirmTitleProperty = DependencyProperty.Register(
        nameof(ConfirmTitle), typeof(string), typeof(ConfirmButton), 
        new PropertyMetadata("确认操作"));

    public string ConfirmTitle
    {
        get => (string)GetValue(ConfirmTitleProperty);
        set => SetValue(ConfirmTitleProperty, value);
    }

    // 确认消息
    public static readonly DependencyProperty ConfirmMessageProperty = DependencyProperty.Register(
        nameof(ConfirmMessage), typeof(string), typeof(ConfirmButton), 
        new PropertyMetadata("确定要执行此操作吗？"));

    public string ConfirmMessage
    {
        get => (string)GetValue(ConfirmMessageProperty);
        set => SetValue(ConfirmMessageProperty, value);
    }

    // 执行命令
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(ConfirmButton), 
        new PropertyMetadata(null));

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (GetTemplateChild("PART_Button") is Button button)
        {
            button.Click += Button_Click;
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(ConfirmMessage, ConfirmTitle, 
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            Command?.Execute(null);
        }
    }
}
```

**使用示例**：

xaml:

```xaml
<local:ConfirmButton Content="停止设备"
                     ConfirmTitle="确认停止"
                     ConfirmMessage="确定要停止设备吗？正在运行的生产任务将被中断。"
                     Command="{Binding StopDeviceCommand}"
                     ButtonStyle="{StaticResource DangerButtonStyle}"/>
```

------

## 四、工业专用类

### 4.1 配方参数卡片

显示配方的参数信息，支持查看和编辑模式切换，是生产系统必备组件。

xaml:

```xaml
<Style TargetType="{x:Type local:RecipeParameterCard}">
    <Setter Property="Background" Value="White"/>
    <Setter Property="BorderBrush" Value="LightGray"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:RecipeParameterCard}">
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="3"
                        Padding="10">
                    <Grid>
                        <Grid.RowDefinitions>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="*"/>
                            <RowDefinition Height="Auto"/>
                        </Grid.RowDefinitions>
                        
                        <!-- 配方名称 -->
                        <TextBlock Text="{TemplateBinding RecipeName}"
                                   FontSize="16"
                                   FontWeight="Bold"/>
                        
                        <!-- 配方版本 -->
                        <TextBlock Grid.Row="1"
                                   Text="{TemplateBinding Version, StringFormat=版本: {0}}"
                                   Foreground="Gray"
                                   FontSize="12"
                                   Margin="0,5,0,15"/>
                        
                        <!-- 参数列表 -->
                        <ItemsControl Grid.Row="2"
                                      ItemsSource="{TemplateBinding Parameters}">
                            <ItemsControl.ItemTemplate>
                                <DataTemplate>
                                    <local:KeyValueDisplay Key="{Binding Name}"
                                                           Value="{Binding Value}"
                                                           Margin="0,3"/>
                                </DataTemplate>
                            </ItemsControl.ItemTemplate>
                        </ItemsControl>
                        
                        <!-- 操作按钮 -->
                        <StackPanel Grid.Row="3"
                                    Orientation="Horizontal"
                                    HorizontalAlignment="Right"
                                    Margin="0,15,0,0">
                            <Button Content="编辑"
                                    Style="{StaticResource PrimaryButtonStyle}"
                                    Margin="0,0,10,0"
                                    Command="{TemplateBinding EditCommand}"/>
                            <Button Content="应用"
                                    Style="{StaticResource SuccessButtonStyle}"
                                    Command="{TemplateBinding ApplyCommand}"/>
                        </StackPanel>
                    </Grid>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

csharp:

```c#
public class RecipeParameterCard : ContentControl
{
    static RecipeParameterCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RecipeParameterCard), 
            new FrameworkPropertyMetadata(typeof(RecipeParameterCard)));
    }

    // 配方名称
    public static readonly DependencyProperty RecipeNameProperty = DependencyProperty.Register(
        nameof(RecipeName), typeof(string), typeof(RecipeParameterCard), 
        new PropertyMetadata(""));

    public string RecipeName
    {
        get => (string)GetValue(RecipeNameProperty);
        set => SetValue(RecipeNameProperty, value);
    }

    // 配方版本
    public static readonly DependencyProperty VersionProperty = DependencyProperty.Register(
        nameof(Version), typeof(string), typeof(RecipeParameterCard), 
        new PropertyMetadata("1.0"));

    public string Version
    {
        get => (string)GetValue(VersionProperty);
        set => SetValue(VersionProperty, value);
    }

    // 参数列表
    public static readonly DependencyProperty ParametersProperty = DependencyProperty.Register(
        nameof(Parameters), typeof(IEnumerable<RecipeParameter>), typeof(RecipeParameterCard), 
        new PropertyMetadata(null));

    public IEnumerable<RecipeParameter> Parameters
    {
        get => (IEnumerable<RecipeParameter>)GetValue(ParametersProperty);
        set => SetValue(ParametersProperty, value);
    }

    // 编辑命令
    public static readonly DependencyProperty EditCommandProperty = DependencyProperty.Register(
        nameof(EditCommand), typeof(ICommand), typeof(RecipeParameterCard), 
        new PropertyMetadata(null));

    public ICommand EditCommand
    {
        get => (ICommand)GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }

    // 应用命令
    public static readonly DependencyProperty ApplyCommandProperty = DependencyProperty.Register(
        nameof(ApplyCommand), typeof(ICommand), typeof(RecipeParameterCard), 
        new PropertyMetadata(null));

    public ICommand ApplyCommand
    {
        get => (ICommand)GetValue(ApplyCommandProperty);
        set => SetValue(ApplyCommandProperty, value);
    }
}

public class RecipeParameter
{
    public string Name { get; set; }
    public string Value { get; set; }
}
```

------

### 4.2 生产状态卡片

实时显示生产线的运行状态、产量、效率等关键指标。

xaml:

```xaml
<Style TargetType="{x:Type local:ProductionStatusCard}">
    <Setter Property="Background" Value="White"/>
    <Setter Property="BorderBrush" Value="LightGray"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:ProductionStatusCard}">
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding StatusColor}"
                        BorderThickness="2"
                        CornerRadius="3"
                        Padding="15">
                    <Grid>
                        <Grid.RowDefinitions>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="*"/>
                        </Grid.RowDefinitions>
                        
                        <!-- 生产线名称和状态 -->
                        <StackPanel Orientation="Horizontal">
                            <Ellipse Width="12" Height="12"
                                     Fill="{TemplateBinding StatusColor}"
                                     Margin="0,0,8,0">
                                <Ellipse.Style>
                                    <Style TargetType="Ellipse">
                                        <Style.Triggers>
                                            <DataTrigger Binding="{Binding IsRunning, RelativeSource={RelativeSource TemplatedParent}}" Value="True">
                                                <DataTrigger.EnterActions>
                                                    <BeginStoryboard>
                                                        <Storyboard RepeatBehavior="Forever">
                                                            <DoubleAnimation Storyboard.TargetProperty="Opacity"
                                                                             From="1" To="0.5" Duration="0:0:1"
                                                                             AutoReverse="True"/>
                                                        </Storyboard>
                                                    </BeginStoryboard>
                                                </DataTrigger.EnterActions>
                                            </DataTrigger>
                                        </Style.Triggers>
                                    </Style>
                                </Ellipse.Style>
                            </Ellipse>
                            <TextBlock Text="{TemplateBinding LineName}"
                                       FontSize="16"
                                       FontWeight="Bold"/>
                        </StackPanel>
                        
                        <!-- 运行时间 -->
                        <TextBlock Grid.Row="1"
                                   Text="{TemplateBinding RunTime, StringFormat=运行时间: {0:hh\\:mm\\:ss}}"
                                   Foreground="Gray"
                                   Margin="0,8,0,15"/>
                        
                        <!-- 指标网格 -->
                        <UniformGrid Grid.Row="2" Columns="2" Rows="2">
                            <StackPanel>
                                <TextBlock Text="产量" Foreground="Gray" FontSize="12"/>
                                <TextBlock Text="{TemplateBinding Output}"
                                           FontSize="20"
                                           FontWeight="Bold"
                                           Foreground="#FF0078D7"/>
                            </StackPanel>
                            <StackPanel>
                                <TextBlock Text="效率" Foreground="Gray" FontSize="12"/>
                                <TextBlock Text="{TemplateBinding Efficiency, StringFormat={}{0:F1}%}"
                                           FontSize="20"
                                           FontWeight="Bold"
                                           Foreground="#FF28A745"/>
                            </StackPanel>
                            <StackPanel>
                                <TextBlock Text="良品率" Foreground="Gray" FontSize="12"/>
                                <TextBlock Text="{TemplateBinding YieldRate, StringFormat={}{0:F1}%}"
                                           FontSize="20"
                                           FontWeight="Bold"
                                           Foreground="#FF28A745"/>
                            </StackPanel>
                            <StackPanel>
                                <TextBlock Text="不良品" Foreground="Gray" FontSize="12"/>
                                <TextBlock Text="{TemplateBinding DefectCount}"
                                           FontSize="20"
                                           FontWeight="Bold"
                                           Foreground="#FFDC3545"/>
                            </StackPanel>
                        </UniformGrid>
                    </Grid>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

csharp:

```c#
public class ProductionStatusCard : ContentControl
{
    static ProductionStatusCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ProductionStatusCard), 
            new FrameworkPropertyMetadata(typeof(ProductionStatusCard)));
    }

    // 生产线名称
    public static readonly DependencyProperty LineNameProperty = DependencyProperty.Register(
        nameof(LineName), typeof(string), typeof(ProductionStatusCard), 
        new PropertyMetadata(""));

    public string LineName
    {
        get => (string)GetValue(LineNameProperty);
        set => SetValue(LineNameProperty, value);
    }

    // 状态颜色
    public static readonly DependencyProperty StatusColorProperty = DependencyProperty.Register(
        nameof(StatusColor), typeof(Brush), typeof(ProductionStatusCard), 
        new PropertyMetadata(Brushes.Green));

    public Brush StatusColor
    {
        get => (Brush)GetValue(StatusColorProperty);
        set => SetValue(StatusColorProperty, value);
    }

    // 是否运行中
    public static readonly DependencyProperty IsRunningProperty = DependencyProperty.Register(
        nameof(IsRunning), typeof(bool), typeof(ProductionStatusCard), 
        new PropertyMetadata(true));

    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    // 运行时间
    public static readonly DependencyProperty RunTimeProperty = DependencyProperty.Register(
        nameof(RunTime), typeof(TimeSpan), typeof(ProductionStatusCard), 
        new PropertyMetadata(TimeSpan.Zero));

    public TimeSpan RunTime
    {
        get => (TimeSpan)GetValue(RunTimeProperty);
        set => SetValue(RunTimeProperty, value);
    }

    // 产量
    public static readonly DependencyProperty OutputProperty = DependencyProperty.Register(
        nameof(Output), typeof(int), typeof(ProductionStatusCard), 
        new PropertyMetadata(0));

    public int Output
    {
        get => (int)GetValue(OutputProperty);
        set => SetValue(OutputProperty, value);
    }

    // 效率
    public static readonly DependencyProperty EfficiencyProperty = DependencyProperty.Register(
        nameof(Efficiency), typeof(double), typeof(ProductionStatusCard), 
        new PropertyMetadata(0.0));

    public double Efficiency
    {
        get => (double)GetValue(EfficiencyProperty);
        set => SetValue(EfficiencyProperty, value);
    }

    // 良品率
    public static readonly DependencyProperty YieldRateProperty = DependencyProperty.Register(
        nameof(YieldRate), typeof(double), typeof(ProductionStatusCard), 
        new PropertyMetadata(100.0));

    public double YieldRate
    {
        get => (double)GetValue(YieldRateProperty);
        set => SetValue(YieldRateProperty, value);
    }

    // 不良品数量
    public static readonly DependencyProperty DefectCountProperty = DependencyProperty.Register(
        nameof(DefectCount), typeof(int), typeof(ProductionStatusCard), 
        new PropertyMetadata(0));

    public int DefectCount
    {
        get => (int)GetValue(DefectCountProperty);
        set => SetValue(DefectCountProperty, value);
    }
}
```

------

## 五、ContentControl 最佳实践总结

1. **优先使用 ContentControl 而非 UserControl**：当你需要一个 "容器" 来承载可变内容时，ContentControl 比 UserControl 具有更高的复用性和灵活性。
2. **严格分离样式与逻辑**：所有外观都通过 ControlTemplate 定义，逻辑代码中不要包含任何 UI 相关的硬编码。
3. **使用依赖属性**：所有可配置的属性都应该定义为依赖属性，支持数据绑定和样式设置。
4. **利用 ContentTemplate**：对于复杂的内容呈现，使用 ContentTemplate 而不是硬编码内容结构。
5. **遵循工业设计原则**：简洁、清晰、高对比度，避免花哨的动画和效果，重点突出信息。