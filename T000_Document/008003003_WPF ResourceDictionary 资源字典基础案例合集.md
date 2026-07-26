# 008003003_WPF `ResourceDictionary` 资源字典基础案例合集

以下 5 个案例从入门到进阶，覆盖资源字典最核心的基础用法，全部贴合工业上位机场景，代码可直接复制运行，承接样式、画刷等之前的知识点，帮你彻底掌握资源字典的定义、合并、复用、主题切换等核心能力。

------

## 前置说明

- `ResourceDictionary` 是 WPF 资源的容器，支持独立存为 `.xaml` 文件，通过 `MergedDictionaries` 合并到窗口 / 全局；
- 核心价值：**资源模块化拆分、一处定义多处复用、支撑主题切换**；
- 约定：所有资源字典文件统一放在项目的 `Themes` 或 `Styles` 文件夹下，路径清晰便于维护。

------

## 案例 1：独立样式资源字典（入门必学）

### 应用场景

把窗口内的样式、画刷抽离到独立的 `.xaml` 文件中，避免窗口 XAML 过于臃肿，实现单窗口内的资源复用，是资源字典最基础的用法。

### 实现步骤

1. 项目新建 `Styles` 文件夹，添加「资源字典」文件 `BaseStyles.xaml`；
2. 在字典内定义颜色、按钮样式等可复用资源；
3. 在窗口中通过 `MergedDictionaries` 合并该字典，即可正常引用。

### 完整代码

#### 1. 资源字典文件 `Styles/BaseStyles.xaml`

xaml:

```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- 主题色画刷 -->
    <SolidColorBrush x:Key="PrimaryBrush" Color="#2E7DFF"/>
    <SolidColorBrush x:Key="SuccessBrush" Color="LimeGreen"/>
    <SolidColorBrush x:Key="WarningBrush" Color="Orange"/>
    <SolidColorBrush x:Key="BorderBrush" Color="#E4E7ED"/>

    <!-- 通用操作按钮样式 -->
    <Style x:Key="OperateButtonStyle" TargetType="Button">
        <Setter Property="Width" Value="120"/>
        <Setter Property="Height" Value="32"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="Background" Value="{StaticResource PrimaryBrush}"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="Margin" Value="0 0 10 0"/>
    </Style>

    <!-- 全局文本框统一样式 -->
    <Style TargetType="TextBox">
        <Setter Property="Width" Value="250"/>
        <Setter Property="Height" Value="28"/>
        <Setter Property="Padding" Value="6 4"/>
        <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="VerticalContentAlignment" Value="Center"/>
    </Style>

</ResourceDictionary>
```

#### 2. 窗口中合并并引用

xaml:

```xaml
<Window x:Class="ResourceDictDemo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="案例1：独立资源字典" Height="350" Width="500">

    <Window.Resources>
        <ResourceDictionary>
            <!-- 合并外部独立资源字典 -->
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="/Styles/BaseStyles.xaml"/>
            </ResourceDictionary.MergedDictionaries>

            <!-- 当前窗口私有资源 -->
            <SolidColorBrush x:Key="TitleBrush" Color="#333"/>
        </ResourceDictionary>
    </Window.Resources>

    <StackPanel Margin="30" Spacing="15">
        <TextBlock Text="工位参数配置" FontSize="18" FontWeight="Bold"
                   Foreground="{StaticResource TitleBrush}"/>

        <!-- 自动应用字典里的全局TextBox样式 -->
        <TextBox Text="曝光时间：1500μs"/>
        <TextBox Text="增益值：1.2dB"/>

        <StackPanel Orientation="Horizontal">
            <!-- 引用字典里的按钮样式 -->
            <Button Content="保存参数" Style="{StaticResource OperateButtonStyle}"/>
            <Button Content="下发设备" Style="{StaticResource OperateButtonStyle}"/>
        </StackPanel>
    </StackPanel>
</Window>
```

### 核心要点

1. 资源从窗口代码中抽离，可被多个窗口复用，大幅减少重复代码；
2. `Source` 属性写相对路径，格式：`/文件夹名/文件名.xaml`；
3. 窗口私有资源优先级高于合并字典的资源，同名 Key 会覆盖合并字典。

------

## 案例 2：全局资源字典（App.xaml 注册）

### 应用场景

整个程序所有窗口共用的基础样式、主题色、转换器，统一放到 `App.xaml` 的全局资源中，一次合并，所有窗口直接引用，不用每个窗口重复合并。

### 实现步骤

1. 在 `App.xaml` 的 `Application.Resources` 中合并资源字典；
2. 任意窗口、控件都可以直接引用字典里的资源，无需再次合并。

### 完整代码

#### `App.xaml` 全局注册

xaml:

```xaml
<Application x:Class="ResourceDictDemo.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- 全局合并基础样式字典，所有窗口自动可用 -->
                <ResourceDictionary Source="/Styles/BaseStyles.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

#### 窗口直接使用（无需再次合并）

xaml:

```xaml
<Window x:Class="ResourceDictDemo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="案例2：全局资源字典">

    <Grid Margin="30">
        <!-- 直接引用全局字典里的样式，不用再合并 -->
        <Button Content="全局样式按钮" Style="{StaticResource OperateButtonStyle}"/>
    </Grid>
</Window>
```

### 核心要点

1. 全局资源作用域为整个程序，适合真正通用的基础样式、主题色、转换器；
2. 不要把所有资源都塞到全局，仅放通用内容，页面私有资源放窗口级，避免全局资源膨胀。

------

## 案例 3：模块化拆分（颜色字典 + 样式字典）

### 应用场景

大型项目按功能拆分资源字典：把颜色变量、控件样式、图标资源分开放，职责单一、便于维护，也是主题切换的基础架构。

本案例拆分两个字典：

- `Colors.xaml`：只放颜色、画刷变量；
- `Controls.xaml`：只放控件样式，样式内引用颜色变量。

### 完整代码

#### 1. 颜色字典 `Themes/Colors.xaml`

xaml:

```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- 语义化颜色变量：只定义值，不关心用途 -->
    <SolidColorBrush x:Key="ThemePrimary" Color="#2E7DFF"/>
    <SolidColorBrush x:Key="PageBg" Color="#F5F6FA"/>
    <SolidColorBrush x:Key="CardBg" Color="White"/>
    <SolidColorBrush x:Key="TextPrimary" Color="#222"/>
    <SolidColorBrush x:Key="BorderDefault" Color="#E4E7ED"/>

</ResourceDictionary>
```

#### 2. 样式字典 `Styles/Controls.xaml`

xaml:

```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- 样式引用颜色变量，实现样式与颜色解耦 -->
    <Style TargetType="TextBox">
        <Setter Property="Width" Value="250"/>
        <Setter Property="Height" Value="28"/>
        <Setter Property="Background" Value="{StaticResource CardBg}"/>
        <Setter Property="Foreground" Value="{StaticResource TextPrimary}"/>
        <Setter Property="BorderBrush" Value="{StaticResource BorderDefault}"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="Padding" Value="6 4"/>
    </Style>

    <Style x:Key="PrimaryButton" TargetType="Button">
        <Setter Property="Width" Value="120"/>
        <Setter Property="Height" Value="32"/>
        <Setter Property="Background" Value="{StaticResource ThemePrimary}"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Cursor" Value="Hand"/>
    </Style>

</ResourceDictionary>
```

#### 3. 合并顺序（关键）

必须**先合并颜色字典，再合并样式字典**，否则样式找不到颜色资源会报错。

xaml:

```xaml
<Window.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- 先合并基础变量 -->
            <ResourceDictionary Source="/Themes/Colors.xaml"/>
            <!-- 后合并样式，样式可以引用前面的颜色 -->
            <ResourceDictionary Source="/Styles/Controls.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Window.Resources>
```

### 核心要点

1. **分层思想**：变量层 + 样式层分离，修改颜色不用动样式，修改样式不用动颜色；
2. **合并顺序**：被依赖的字典放前面，依赖方放后面，资源只能引用前面合并的字典和自身的资源；
3. 这套架构是主题切换的基础：替换颜色字典，所有样式自动适配新颜色。

------

## 案例 4：基础主题切换（亮 / 暗双主题）

### 应用场景

通过动态替换资源字典，实现亮色 / 暗色主题一键切换，是资源字典最经典的进阶用法，工业现场明暗环境适配必备。

本案例用最简化的代码演示核心原理，理解后可扩展为完整主题体系。

### 实现步骤

1. 新建两个颜色字典，Key 完全相同，值不同（亮色、暗色）；
2. 主题相关属性用 `DynamicResource` 引用；
3. 点击按钮时，移除旧主题字典，添加新主题字典，界面自动更新。

### 完整代码

#### 1. 亮色主题 `Themes/LightTheme.xaml`

xaml:

```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <SolidColorBrush x:Key="PageBg" Color="#F5F6FA"/>
    <SolidColorBrush x:Key="CardBg" Color="White"/>
    <SolidColorBrush x:Key="TextPrimary" Color="#222"/>
    <SolidColorBrush x:Key="ThemePrimary" Color="#2E7DFF"/>
</ResourceDictionary>
```

#### 2. 暗色主题 `Themes/DarkTheme.xaml`

xaml:

```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <SolidColorBrush x:Key="PageBg" Color="#121212"/>
    <SolidColorBrush x:Key="CardBg" Color="#1E1E1E"/>
    <SolidColorBrush x:Key="TextPrimary" Color="#E5E5E5"/>
    <SolidColorBrush x:Key="ThemePrimary" Color="#4096FF"/>
</ResourceDictionary>
```

#### 3. 窗口界面（用动态资源引用主题色）

xaml:

```xaml
<Window x:Class="ResourceDictDemo.ThemeWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="案例4：基础主题切换" Height="300" Width="450"
        Background="{DynamicResource PageBg}">

    <Window.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- 默认加载亮色主题 -->
                <ResourceDictionary Source="/Themes/LightTheme.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Window.Resources>

    <StackPanel Margin="30" Spacing="15">
        <TextBlock Text="设备运行监控" FontSize="18" FontWeight="Bold"
                   Foreground="{DynamicResource TextPrimary}"/>

        <Border Background="{DynamicResource CardBg}" Padding="15" CornerRadius="4"
                BorderBrush="{DynamicResource ThemePrimary}" BorderThickness="1">
            <TextBlock Text="当前温度：26.8℃" Foreground="{DynamicResource TextPrimary}"/>
        </Border>

        <StackPanel Orientation="Horizontal" Spacing="10">
            <Button Content="亮色主题" Click="BtnLight_Click" Background="{DynamicResource ThemePrimary}"
                    Foreground="White" Width="100" Height="30" BorderThickness="0"/>
            <Button Content="暗色主题" Click="BtnDark_Click" Background="{DynamicResource ThemePrimary}"
                    Foreground="White" Width="100" Height="30" BorderThickness="0"/>
        </StackPanel>
    </StackPanel>
</Window>
```

#### 4. 后台切换逻辑

csharp:

```c#
public partial class ThemeWindow : Window
{
    public ThemeWindow()
    {
        InitializeComponent();
    }

    // 切换亮色
    private void BtnLight_Click(object sender, RoutedEventArgs e)
    {
        SwitchTheme("/Themes/LightTheme.xaml");
    }

    // 切换暗色
    private void BtnDark_Click(object sender, RoutedEventArgs e)
    {
        SwitchTheme("/Themes/DarkTheme.xaml");
    }

    // 核心：替换主题字典
    private void SwitchTheme(string themePath)
    {
        // 1. 移除旧主题字典
        var oldDict = this.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("/Themes/"));
        if (oldDict != null)
            this.Resources.MergedDictionaries.Remove(oldDict);

        // 2. 添加新主题字典
        var newDict = new ResourceDictionary
        {
            Source = new Uri(themePath, UriKind.Relative)
        };
        this.Resources.MergedDictionaries.Add(newDict);
    }
}
```

### 核心要点

1. **关键前提**：主题相关的属性必须用 `DynamicResource` 引用，静态资源不会更新；
2. **核心原理**：同名 Key 的字典替换，后添加的覆盖旧的，动态资源监听到变化自动更新 UI；
3. **扩展方向**：可以把切换逻辑封装成主题管理器，支持全局切换、持久化保存。

------

## 案例 5：资源字典复用图片资源

### 应用场景

把工业软件常用的状态图标、功能图标统一放到资源字典中管理，多处复用，避免每个页面重复引用图片文件，便于统一替换。

### 实现步骤

1. 项目新建 `Images` 文件夹，放入图标文件（如 `run.png`、`alarm.png`），生成操作设为 `Resource`；
2. 在资源字典中定义 `BitmapImage` 资源；
3. 界面通过 Key 引用图片资源。

### 完整代码

#### 1. 图标资源字典 `Styles/Icons.xaml`

xaml:

```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- 运行状态图标 -->
    <BitmapImage x:Key="IconRun" UriSource="Images/run.png"/>
    <!-- 告警状态图标 -->
    <BitmapImage x:Key="IconAlarm" UriSource="Images/alarm.png"/>
    <!-- 设备离线图标 -->
    <BitmapImage x:Key="IconOffline" UriSource="Images/offline.png"/>

</ResourceDictionary>
```

#### 2. 合并并引用

xaml:

```xaml
<Window.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="/Styles/Icons.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Window.Resources>

<StackPanel Margin="30" Spacing="10">
    <StackPanel Orientation="Horizontal" Spacing="10">
        <Image Source="{StaticResource IconRun}" Width="24" Height="24"/>
        <TextBlock Text="设备运行中" VerticalAlignment="Center"/>
    </StackPanel>

    <StackPanel Orientation="Horizontal" Spacing="10">
        <Image Source="{StaticResource IconAlarm}" Width="24" Height="24"/>
        <TextBlock Text="设备告警中" Foreground="Orange" VerticalAlignment="Center"/>
    </StackPanel>
</StackPanel>
```

### 核心要点

1. 图片资源统一管理，替换图标只需要修改字典文件，不用逐个页面改；
2. 图片生成操作必须设为 `Resource`，嵌入程序集，避免部署后路径丢失；
3. 小图标推荐用这种方式，大图片不建议放入资源字典，避免占用过多内存。

------

## 基础最佳实践总结

1. **按需拆分**：按功能拆分字典（颜色、样式、图标、多语言），不要把所有资源塞到一个文件里；
2. **合并顺序**：被依赖的基础字典放前面，业务样式字典放后面；
3. **静态优先，动态按需**：固定资源用 `StaticResource`，主题 / 多语言等需要切换的才用 `DynamicResource`；
4. **命名规范**：资源 Key 遵循「语义 + 类型」，如 `ThemePrimaryBrush`、`PrimaryButtonStyle`，见名知意；
5. **全局克制**：仅真正通用的资源放全局，页面私有资源放窗口级，减少全局资源查找开销。