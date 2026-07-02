# 008003002_WPF 基于 `ResourceDictionary` 实现主题切换完整方案

在 WPF 中实现主题切换的核心是 **「同 Key 异值的资源字典 + `DynamicResource` 动态更新 + 运行时替换合并字典」**。本质是将所有和主题相关的颜色、画刷、字体等资源抽离到独立字典文件中，不同主题使用完全相同的资源 Key、不同的属性值；运行时通过替换 `MergedDictionaries` 中的主题字典，配合 `DynamicResource` 的动态监听特性，实现所有界面一键自动换肤。

这套方案是工业软件明暗主题适配、多工位风格切换的标准实现，完全解耦业务逻辑与 UI 外观，符合 MVVM 架构。

------

## 一、核心实现原理

### 1. 两大技术基石

1. **`ResourceDictionary` 合并机制**

   一个主字典可以合并多个子字典；当多个字典存在同名 Key 时，**后合并的字典会覆盖先合并的字典**。通过移除旧主题字典、添加新主题字典，就能批量替换所有主题资源。

2. **`DynamicResource` 动态资源**

   动态资源不会在加载时一次性取值，而是运行时实时监听资源字典的变化。主题字典替换后，所有用 `DynamicResource` 引用的属性会自动读取新值，界面瞬间更新，无需手动刷新。

> 对比：`StaticResource` 只在加载时取一次值，资源替换后不会更新，因此**主题相关的资源必须用 `DynamicResource` 引用**。

### 2. 整体架构设计（最佳实践）

工业级项目推荐采用「基础样式层 + 主题色层」的双层架构，避免每个主题都重复写一遍控件样式，维护成本极低：

- **主题色层**：只存放颜色、画刷、字体、尺寸等主题变量，每个主题一个文件，Key 完全一致；
- **基础样式层**：存放所有控件的统一样式、模板、触发器，样式内的颜色全部引用主题色的动态资源；
- **切换逻辑**：只替换主题色层的字典，基础样式永远不变，换主题等于换一套颜色变量。

------

## 二、完整实现步骤（可直接落地）

### 第一步：创建主题色资源字典

在项目中新建 `Themes` 文件夹，分别创建亮色、暗色两个主题字典文件，**保证所有资源 Key 完全相同，只修改值**。

#### 1. 亮色主题 `LightTheme.xaml`

xaml:

```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- 页面背景 -->
    <SolidColorBrush x:Key="PageBackgroundBrush" Color="#F5F6FA"/>
    <!-- 卡片背景 -->
    <SolidColorBrush x:Key="CardBackgroundBrush" Color="White"/>
    <!-- 一级文本 -->
    <SolidColorBrush x:Key="PrimaryTextBrush" Color="#222222"/>
    <!-- 二级文本 -->
    <SolidColorBrush x:Key="SecondaryTextBrush" Color="#666666"/>
    <!-- 主题主色 -->
    <SolidColorBrush x:Key="ThemePrimaryBrush" Color="#2E7DFF"/>
    <!-- 边框色 -->
    <SolidColorBrush x:Key="BorderBrush" Color="#E4E7ED"/>
    <!-- 成功色 -->
    <SolidColorBrush x:Key="SuccessBrush" Color="#00B42A"/>
    <!-- 告警色 -->
    <SolidColorBrush x:Key="WarningBrush" Color="#FF7D00"/>
    <!-- 危险色 -->
    <SolidColorBrush x:Key="DangerBrush" Color="#F53F3F"/>

</ResourceDictionary>
```

#### 2. 暗色主题 `DarkTheme.xaml`

xaml:

```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- 页面背景 -->
    <SolidColorBrush x:Key="PageBackgroundBrush" Color="#121212"/>
    <!-- 卡片背景 -->
    <SolidColorBrush x:Key="CardBackgroundBrush" Color="#1E1E1E"/>
    <!-- 一级文本 -->
    <SolidColorBrush x:Key="PrimaryTextBrush" Color="#E5E5E5"/>
    <!-- 二级文本 -->
    <SolidColorBrush x:Key="SecondaryTextBrush" Color="#999999"/>
    <!-- 主题主色 -->
    <SolidColorBrush x:Key="ThemePrimaryBrush" Color="#4096FF"/>
    <!-- 边框色 -->
    <SolidColorBrush x:Key="BorderBrush" Color="#333333"/>
    <!-- 成功色 -->
    <SolidColorBrush x:Key="SuccessBrush" Color="#23C343"/>
    <!-- 告警色 -->
    <SolidColorBrush x:Key="WarningBrush" Color="#FF9A2E"/>
    <!-- 危险色 -->
    <SolidColorBrush x:Key="DangerBrush" Color="#FF4D4F"/>

</ResourceDictionary>
```

> 关键规则：两个文件的 `x:Key` 必须一字不差，大小写一致；缺 Key 会导致切换主题后部分控件样式丢失。

------

### 第二步：抽取基础样式字典

新建 `Styles/BaseStyles.xaml`，存放所有控件的通用样式。**样式中所有和主题相关的颜色，全部用 `DynamicResource` 引用主题色的 Key**，这样样式只需要写一套，自动适配所有主题。

xaml:

```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- 全局文本框样式：颜色全部引用动态主题资源 -->
    <Style TargetType="TextBox">
        <Setter Property="Width" Value="250"/>
        <Setter Property="Height" Value="32"/>
        <Setter Property="Padding" Value="8 4"/>
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="VerticalContentAlignment" Value="Center"/>
        <!-- 主题相关属性用 DynamicResource -->
        <Setter Property="Background" Value="{DynamicResource CardBackgroundBrush}"/>
        <Setter Property="Foreground" Value="{DynamicResource PrimaryTextBrush}"/>
        <Setter Property="BorderBrush" Value="{DynamicResource BorderBrush}"/>
        <Style.Triggers>
            <Trigger Property="IsFocused" Value="True">
                <Setter Property="BorderBrush" Value="{DynamicResource ThemePrimaryBrush}"/>
            </Trigger>
        </Style.Triggers>
    </Style>

    <!-- 主按钮样式 -->
    <Style x:Key="PrimaryButtonStyle" TargetType="Button">
        <Setter Property="Width" Value="120"/>
        <Setter Property="Height" Value="32"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="Background" Value="{DynamicResource ThemePrimaryBrush}"/>
        <Style.Triggers>
            <Trigger Property="IsMouseOver" Value="True">
                <Setter Property="Opacity" Value="0.85"/>
            </Trigger>
            <Trigger Property="IsEnabled" Value="False">
                <Setter Property="Background" Value="{DynamicResource BorderBrush}"/>
                <Setter Property="Foreground" Value="{DynamicResource SecondaryTextBrush}"/>
            </Trigger>
        </Style.Triggers>
    </Style>

    <!-- 卡片边框样式 -->
    <Style x:Key="CardBorderStyle" TargetType="Border">
        <Setter Property="Background" Value="{DynamicResource CardBackgroundBrush}"/>
        <Setter Property="BorderBrush" Value="{DynamicResource BorderBrush}"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="CornerRadius" Value="4"/>
        <Setter Property="Padding" Value="15"/>
    </Style>

</ResourceDictionary>
```

------

### 第三步：全局注册默认主题

在 `App.xaml` 中合并基础样式和默认主题，让整个程序默认加载亮色主题。

xaml:

```xaml
<Application x:Class="ThemeDemo.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- 1. 先合并基础样式 -->
                <ResourceDictionary Source="/Styles/BaseStyles.xaml"/>
                <!-- 2. 后合并默认亮色主题（后合并的优先级更高，覆盖基础样式中的资源） -->
                <ResourceDictionary Source="/Themes/LightTheme.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

> 注意顺序：基础样式在前，主题色在后，保证主题色可以被样式正确引用。

------

### 第四步：实现主题切换核心逻辑

通过操作 `Application.Current.Resources.MergedDictionaries`，移除旧主题字典、添加新主题字典，即可完成全局主题切换。

#### 封装主题管理类（推荐）

新建 `ThemeManager.cs` 统一管理主题切换，避免重复代码：

csharp:

```c#
using System;
using System.Linq;
using System.Windows;

/// <summary>
/// 主题管理器：统一处理主题切换逻辑
/// </summary>
public static class ThemeManager
{
    private const string LightThemePath = "/Themes/LightTheme.xaml";
    private const string DarkThemePath = "/Themes/DarkTheme.xaml";

    /// <summary>
    /// 切换主题
    /// </summary>
    /// <param name="isDark">true=暗色，false=亮色</param>
    public static void SwitchTheme(bool isDark)
    {
        string themePath = isDark ? DarkThemePath : LightThemePath;
        var appResources = Application.Current.Resources;

        // 1. 移除所有旧的主题字典（过滤出主题目录下的字典）
        var oldThemes = appResources.MergedDictionaries
            .Where(d => d.Source != null && d.Source.OriginalString.Contains("/Themes/"))
            .ToList();

        foreach (var old in oldThemes)
        {
            appResources.MergedDictionaries.Remove(old);
        }

        // 2. 添加新主题字典
        ResourceDictionary newTheme = new ResourceDictionary
        {
            Source = new Uri(themePath, UriKind.Relative)
        };
        appResources.MergedDictionaries.Add(newTheme);
    }
}
```

#### 界面调用示例

在窗口中放两个切换按钮，点击调用切换方法：

csharp:

```c#
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // 切换亮色主题
    private void BtnLightTheme_Click(object sender, RoutedEventArgs e)
    {
        ThemeManager.SwitchTheme(false);
    }

    // 切换暗色主题
    private void BtnDarkTheme_Click(object sender, RoutedEventArgs e)
    {
        ThemeManager.SwitchTheme(true);
    }
}
```

------

### 第五步：界面使用示例

主界面中，所有和主题相关的背景、文本颜色，都用 `DynamicResource` 引用主题 Key。

xaml:

```c#
<Window x:Class="ThemeDemo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="工业上位机主题切换演示" Height="450" Width="600"
        Background="{DynamicResource PageBackgroundBrush}">

    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition/>
        </Grid.RowDefinitions>

        <!-- 顶部切换按钮 -->
        <StackPanel Orientation="Horizontal" Spacing="10" Margin="0 0 0 20">
            <Button Content="亮色主题" Click="BtnLightTheme_Click" Style="{StaticResource PrimaryButtonStyle}"/>
            <Button Content="暗色主题" Click="BtnDarkTheme_Click" Style="{StaticResource PrimaryButtonStyle}"/>
        </StackPanel>

        <!-- 设备状态卡片 -->
        <Border Grid.Row="1" Style="{StaticResource CardBorderStyle}">
            <StackPanel Spacing="15">
                <TextBlock Text="设备运行监控" FontSize="18" FontWeight="Bold"
                           Foreground="{DynamicResource PrimaryTextBrush}"/>

                <StackPanel Orientation="Horizontal" Spacing="10">
                    <Ellipse Width="16" Height="16" Fill="{DynamicResource SuccessBrush}"/>
                    <TextBlock Text="设备运行中" VerticalAlignment="Center"
                               Foreground="{DynamicResource PrimaryTextBrush}"/>
                </StackPanel>

                <TextBlock Text="当前温度：26.8℃"
                           Foreground="{DynamicResource SecondaryTextBrush}"/>

                <TextBox Text="曝光时间参数"/>
                <TextBox Text="增益值参数"/>

                <Button Content="下发参数" Style="{StaticResource PrimaryButtonStyle}" HorizontalAlignment="Left"/>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```

**运行效果**：点击亮色 / 暗色按钮，整个窗口的背景、卡片、文本、按钮、输入框会同步切换配色，所有控件自动适配，无需任何额外刷新代码。

------

## 三、MVVM 模式下的切换方式

如果项目遵循 MVVM 架构，可以把主题切换封装成命令，绑定到 ViewModel 中：

csharp:

```c#
public class MainViewModel : ViewModelBase
{
    private bool _isDarkTheme;
    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set 
        {
            if (SetProperty(ref _isDarkTheme, value))
            {
                ThemeManager.SwitchTheme(value);
                // 可选：保存到配置文件
                ConfigHelper.Save("Theme", value ? "Dark" : "Light");
            }
        }
    }
}
```

界面中用复选框或切换开关绑定 `IsDarkTheme` 即可，完全符合 MVVM 分层。

------

## 四、进阶：主题持久化

用户选择的主题需要下次启动自动生效，只需在程序启动时读取配置，加载对应主题：

csharp:

```c#
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // 读取保存的主题配置
        string savedTheme = ConfigHelper.Get("Theme", "Light");
        bool isDark = savedTheme == "Dark";
        ThemeManager.SwitchTheme(isDark);
    }
}
```

------

## 五、关键注意事项与最佳实践

### 1. 必须用 `DynamicResource` 引用主题资源

这是最核心的规则：

- ✅ 主题色、主题字体、随主题变化的属性 → `DynamicResource`
- ✅ 固定不变的样式结构、静态资源 → `StaticResource`
- ❌ 主题资源用 `StaticResource`：切换主题后界面不会更新，是新手最高频的坑。

### 2. 主题 Key 必须严格一致

所有主题字典的资源 Key 必须完全相同（大小写、拼写），缺 Key 会导致切换后对应控件显示默认值，出现样式错乱。

- 最佳实践：先定义一个标准主题作为基准，其他主题复制后改值，不要手动逐个写 Key。

### 3. 样式与主题色分离

不要把样式写在主题字典里，否则每个主题都要维护一套样式，维护成本爆炸。

- 正确分层：主题字典只放「颜色、画刷、尺寸、字体」等变量；基础样式字典放「控件结构、触发器、布局」等固定逻辑。

### 4. 避免重复合并字典

不要在每个窗口都重复合并主题字典和基础样式字典，会创建多份字典实例，造成内存浪费。

- 正确做法：全局只在 `App.xaml` 合并一次，所有窗口自动继承全局资源。

### 5. 性能优化

1. 控制动态资源的使用范围：只在主题相关的属性上用 `DynamicResource`，固定属性优先用静态资源；
2. 大数据量列表的行模板中，尽量减少动态资源数量，避免滚动时频繁解析资源；
3. 主题字典尽量精简，只放主题相关资源，不要把业务专用资源塞进去。

### 6. 合并顺序决定优先级

- 后合并的字典优先级更高，会覆盖先合并的同名资源；
- 窗口级资源优先级高于全局资源，如果窗口内定义了同名 Key，会覆盖全局主题色，排查问题时优先检查。

### 7. 窗口级主题覆盖

如果某个子窗口需要单独的主题，只在窗口的 `Resources` 中合并对应主题字典即可，窗口级优先级高于全局，不会影响其他窗口。

------

## 六、常见问题排查

1. **切换主题后部分颜色不变？**
   - 检查对应属性是不是用了 `StaticResource`，改成 `DynamicResource`；
   - 检查新主题字典里有没有对应的 Key；
   - 检查是不是控件上写了本地值，本地值优先级高于资源。
2. **切换主题后样式错乱？**
   - 检查两个主题的 Key 是否完全一致；
   - 检查基础样式是否正确引用了动态资源。
3. **切换主题卡顿？**
   - 检查是否存在大量动态资源，优化静态 / 动态资源的使用；
   - 检查是否重复合并了多次字典，导致资源查找链路过长。

------

## 总结

基于 `ResourceDictionary` 的主题切换是 WPF 最原生、性能最优的换肤方案，核心总结为三句话：

1. **资源分层**：主题色变量和基础样式分离，一套样式适配多主题；
2. **动态引用**：所有主题相关属性用 `DynamicResource`，支持实时更新；
3. **运行替换**：操作合并字典实现主题切换，全局一键生效。

这套方案完全适配工业上位机的明暗主题需求，也可以扩展为多工位风格切换、客户定制主题等场景，扩展性和可维护性都非常优秀。