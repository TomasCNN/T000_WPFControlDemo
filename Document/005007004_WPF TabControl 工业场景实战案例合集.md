# 005007004_WPF `TabControl` 工业场景实战案例合集



以下案例全部贴合工业上位机、产线监控、设备管理、生产报表等真实业务场景，覆盖静态分页、动态多标签、侧边导航、状态指示、数据懒加载等核心用法，每个案例明确标注对应特性，可直接复用到工业项目中。

------

## 案例 1：单设备详情静态分页（基础标准用法）

### 场景说明

单台设备的详情界面，按业务维度分为「实时监控、参数配置、报警记录、维护信息」4 个标签页，避免单页内容过多导致操作混乱，是工业设备管理系统的标准布局。

### 对应核心特性

- 静态 `TabItem` 固定标签
- 标签头 + 内容区双层结构
- 原生标签切换交互

xaml:

```xaml
<Window x:Class="IndustrialDemo.DeviceDetailWindow"
        Title="设备详情 - 喷涂机器人A01" Height="520" Width="750">
    <Grid Margin="12">
        <TabControl BorderBrush="#DDD" BorderThickness="1" Background="White">
            <!-- 标签1：实时监控 -->
            <TabItem Header="实时监控">
                <Border Background="#F5F9FF" Padding="20">
                    <WrapPanel>
                        <Border Width="180" Height="90" Margin="0 0 15 15" Background="White" BorderBrush="#E0E0E0" BorderThickness="1" Padding="12">
                            <StackPanel>
                                <TextBlock Foreground="#666" Text="当前温度"/>
                                <TextBlock FontSize="24" FontWeight="Bold" Foreground="#1890FF" Text="45.2 ℃" Margin="0 8 0 0"/>
                            </StackPanel>
                        </Border>
                        <Border Width="180" Height="90" Margin="0 0 15 15" Background="White" BorderBrush="#E0E0E0" BorderThickness="1" Padding="12">
                            <StackPanel>
                                <TextBlock Foreground="#666" Text="当前压力"/>
                                <TextBlock FontSize="24" FontWeight="Bold" Foreground="#52C41A" Text="0.35 MPa" Margin="0 8 0 0"/>
                            </StackPanel>
                        </Border>
                        <Border Width="180" Height="90" Background="White" BorderBrush="#E0E0E0" BorderThickness="1" Padding="12">
                            <StackPanel>
                                <TextBlock Foreground="#666" Text="运行状态"/>
                                <TextBlock FontSize="24" FontWeight="Bold" Foreground="#52C41A" Text="正常运行" Margin="0 8 0 0"/>
                            </StackPanel>
                        </Border>
                    </WrapPanel>
                </Border>
            </TabItem>

            <!-- 标签2：参数配置 -->
            <TabItem Header="参数配置">
                <Border Background="#FAFFFA" Padding="20">
                    <StackPanel Width="320">
                        <TextBlock FontSize="14" FontWeight="Bold" Text="工艺参数设置" Margin="0 0 0 12"/>
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="100"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            <Grid.RowDefinitions>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition Height="Auto"/>
                            </Grid.RowDefinitions>
                            <TextBlock Grid.Row="0" Text="目标温度" VerticalAlignment="Center"/>
                            <TextBox Grid.Row="0" Grid.Column="1" Text="85.5" Margin="0 0 0 10"/>
                            <TextBlock Grid.Row="1" Text="目标压力" VerticalAlignment="Center"/>
                            <TextBox Grid.Row="1" Grid.Column="1" Text="0.32" Margin="0 0 0 10"/>
                            <TextBlock Grid.Row="2" Text="输送速度" VerticalAlignment="Center"/>
                            <TextBox Grid.Row="2" Grid.Column="1" Text="0.5"/>
                        </Grid>
                        <Button Content="保存参数" HorizontalAlignment="Left" Padding="16 4" Margin="0 15 0 0" Background="#1890FF" Foreground="White" BorderThickness="0"/>
                    </StackPanel>
                </Border>
            </TabItem>

            <!-- 标签3：报警记录 -->
            <TabItem Header="报警记录">
                <Border Background="#FFF5F5" Padding="15">
                    <ListBox BorderThickness="0" Background="Transparent">
                        <ListBoxItem Height="32">
                            <Grid>
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="140"/>
                                    <ColumnDefinition Width="80"/>
                                    <ColumnDefinition Width="*"/>
                                </Grid.ColumnDefinitions>
                                <TextBlock Text="2024-06-18 09:12:30"/>
                                <TextBlock Grid.Column="1" Text="严重" Foreground="#F5222D"/>
                                <TextBlock Grid.Column="2" Text="温度超限报警"/>
                            </Grid>
                        </ListBoxItem>
                        <ListBoxItem Height="32">
                            <Grid>
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="140"/>
                                    <ColumnDefinition Width="80"/>
                                    <ColumnDefinition Width="*"/>
                                </Grid.ColumnDefinitions>
                                <TextBlock Text="2024-06-18 10:05:18"/>
                                <TextBlock Grid.Column="1" Text="警告" Foreground="#FAAD14"/>
                                <TextBlock Grid.Column="2" Text="压力轻微波动"/>
                            </Grid>
                        </ListBoxItem>
                    </ListBox>
                </Border>
            </TabItem>

            <!-- 标签4：维护信息 -->
            <TabItem Header="维护信息">
                <Border Background="#FFF9F0" Padding="20">
                    <StackPanel>
                        <TextBlock Text="设备编号：ROBOT-A01" Margin="0 0 0 8"/>
                        <TextBlock Text="上次维护时间：2024-05-10" Margin="0 0 0 8"/>
                        <TextBlock Text="维护人员：张工" Margin="0 0 0 8"/>
                        <TextBlock Text="下次维护时间：2024-08-10" Margin="0 0 0 8"/>
                        <TextBlock Text="累计运行时长：1280 小时" Margin="0 0 0 8"/>
                    </StackPanel>
                </Border>
            </TabItem>
        </TabControl>
    </Grid>
</Window>
```

------

## 案例 2：多设备动态标签页（MVVM + 关闭按钮）

### 场景说明

中控监控台支持同时打开多台设备的详情标签，类似浏览器多标签，标签头显示设备编号、运行状态指示灯、关闭按钮，支持动态新增、关闭标签，是多设备集中监控的标准交互模式。

### 对应核心特性

- `ItemsSource` 动态绑定标签集合
- `ItemTemplate` 自定义标签头（状态灯 + 关闭按钮）
- `SelectedItem` 双向绑定选中标签
- `ContentTemplate` 统一内容模板

### 1. 标签页数据模型

csharp:

```c#
public class DeviceTabItem : INotifyPropertyChanged
{
    private string _deviceCode;
    public string DeviceCode
    {
        get => _deviceCode;
        set { _deviceCode = value; OnPropertyChanged(); }
    }

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set { _isRunning = value; OnPropertyChanged(); }
    }

    private bool _isAlarm;
    public bool IsAlarm
    {
        get => _isAlarm;
        set { _isAlarm = value; OnPropertyChanged(); }
    }

    public string TabTitle => $"设备 {DeviceCode}";
    public string DetailContent => $"{DeviceCode} 实时运行详情面板";

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### 2. 主视图模型

csharp:

```c#
public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<DeviceTabItem> DeviceTabs { get; set; }

    private DeviceTabItem _selectedTab;
    public DeviceTabItem SelectedTab
    {
        get => _selectedTab;
        set { _selectedTab = value; OnPropertyChanged(); }
    }

    public MainViewModel()
    {
        DeviceTabs = new ObservableCollection<DeviceTabItem>
        {
            new DeviceTabItem { DeviceCode = "A01", IsRunning = true, IsAlarm = false },
            new DeviceTabItem { DeviceCode = "A02", IsRunning = true, IsAlarm = true },
            new DeviceTabItem { DeviceCode = "B01", IsRunning = false, IsAlarm = false }
        };
        SelectedTab = DeviceTabs.First();
    }

    // 关闭标签
    public void CloseTab(DeviceTabItem tab)
    {
        if (DeviceTabs.Contains(tab))
        {
            DeviceTabs.Remove(tab);
            if (DeviceTabs.Count > 0)
                SelectedTab = DeviceTabs.Last();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### 3. XAML 界面

xaml:

```xaml
<Window.DataContext>
    <local:MainViewModel/>
</Window.DataContext>

<Grid Margin="12">
    <TabControl ItemsSource="{Binding DeviceTabs}"
                SelectedItem="{Binding SelectedTab}"
                BorderBrush="#DDD" BorderThickness="1">

        <!-- 自定义标签头：状态灯 + 标题 + 关闭按钮 -->
        <TabControl.ItemTemplate>
            <DataTemplate>
                <DockPanel Width="130" Height="28" VerticalAlignment="Center">
                    <!-- 关闭按钮 -->
                    <Button DockPanel.Dock="Right" Content="×" 
                            Width="18" Height="18" Padding="0" Margin="6 0 0 0"
                            Background="Transparent" BorderThickness="0" 
                            Foreground="#999" Cursor="Hand"
                            Click="CloseTab_Click"/>
                    
                    <!-- 状态指示灯 -->
                    <Ellipse DockPanel.Dock="Left" Width="8" Height="8" 
                             VerticalAlignment="Center" Margin="0 0 6 0">
                        <Ellipse.Style>
                            <Style TargetType="Ellipse">
                                <Setter Property="Fill" Value="#BFBFBF"/>
                                <Style.Triggers>
                                    <DataTrigger Binding="{Binding IsRunning}" Value="True">
                                        <Setter Property="Fill" Value="#52C41A"/>
                                    </DataTrigger>
                                    <DataTrigger Binding="{Binding IsAlarm}" Value="True">
                                        <Setter Property="Fill" Value="#F5222D"/>
                                    </DataTrigger>
                                </Style.Triggers>
                            </Style>
                        </Ellipse.Style>
                    </Ellipse>
                    
                    <!-- 标签标题 -->
                    <TextBlock Text="{Binding TabTitle}" VerticalAlignment="Center" TextTrimming="CharacterEllipsis"/>
                </DockPanel>
            </DataTemplate>
        </TabControl.ItemTemplate>

        <!-- 统一内容模板 -->
        <TabControl.ContentTemplate>
            <DataTemplate>
                <Border Padding="20" Background="#F8F9FA">
                    <TextBlock Text="{Binding DetailContent}" FontSize="14"/>
                </Border>
            </DataTemplate>
        </TabControl.ContentTemplate>
    </TabControl>
</Grid>
```

### 4. 关闭按钮后台逻辑

csharp:

```c#
private void CloseTab_Click(object sender, RoutedEventArgs e)
{
    var button = sender as Button;
    var tab = button?.DataContext as DeviceTabItem;
    var vm = DataContext as MainViewModel;
    vm?.CloseTab(tab);
}
```

------

## 案例 3：左侧垂直导航式系统配置

### 场景说明

系统参数配置界面，标签条放在左侧，做成侧边导航式布局，分为通讯配置、报警配置、用户管理、日志管理四大模块，宽屏工控机上操作路径更短，符合工业软件左侧导航的使用习惯。

### 对应核心特性

- `TabStripPlacement="Left"` 左侧停靠标签
- 自定义标签容器样式（高度、对齐方式）
- 分类清晰，适合多模块配置界面

xaml:

```xaml
<Window Title="系统参数配置" Height="500" Width="800">
    <Grid Margin="12">
        <TabControl TabStripPlacement="Left"
                    BorderBrush="#DDD" BorderThickness="1"
                    Background="White">

            <!-- 标签容器样式：统一高度与对齐 -->
            <TabControl.ItemContainerStyle>
                <Style TargetType="TabItem">
                    <Setter Property="Height" Value="44"/>
                    <Setter Property="Padding" Value="20 0"/>
                    <Setter Property="HorizontalContentAlignment" Value="Left"/>
                    <Setter Property="VerticalContentAlignment" Value="Center"/>
                    <Setter Property="FontSize" Value="13"/>
                </Style>
            </TabControl.ItemContainerStyle>

            <!-- 通讯配置 -->
            <TabItem Header="通讯配置">
                <Border Padding="25" Background="#FAFBFC">
                    <StackPanel Width="350">
                        <TextBlock FontSize="15" FontWeight="Bold" Text="PLC通讯参数" Margin="0 0 0 15"/>
                        <TextBlock Text="IP地址" Margin="0 0 0 4"/>
                        <TextBox Text="192.168.1.100" Margin="0 0 0 12"/>
                        <TextBlock Text="端口号" Margin="0 0 0 4"/>
                        <TextBox Text="502" Margin="0 0 0 12"/>
                        <TextBlock Text="通讯超时(ms)" Margin="0 0 0 4"/>
                        <TextBox Text="3000" Margin="0 0 0 15"/>
                        <Button Content="测试连接" Width="100" Padding="12 4"/>
                    </StackPanel>
                </Border>
            </TabItem>

            <!-- 报警配置 -->
            <TabItem Header="报警配置">
                <Border Padding="25" Background="#FAFBFC">
                    <StackPanel Width="350">
                        <TextBlock FontSize="15" FontWeight="Bold" Text="报警阈值设置" Margin="0 0 0 15"/>
                        <TextBlock Text="温度上限(℃)" Margin="0 0 0 4"/>
                        <TextBox Text="90" Margin="0 0 0 12"/>
                        <TextBlock Text="压力上限(MPa)" Margin="0 0 0 4"/>
                        <TextBox Text="0.5" Margin="0 0 0 12"/>
                        <CheckBox Content="报警时触发声光提示" Margin="0 5 0 0"/>
                    </StackPanel>
                </Border>
            </TabItem>

            <!-- 用户管理 -->
            <TabItem Header="用户管理">
                <Border Padding="25" Background="#FAFBFC">
                    <TextBlock>用户权限与账号管理页面</TextBlock>
                </Border>
            </TabItem>

            <!-- 日志管理 -->
            <TabItem Header="日志管理">
                <Border Padding="25" Background="#FAFBFC">
                    <TextBlock>系统日志查询与导出配置页面</TextBlock>
                </Border>
            </TabItem>
        </TabControl>
    </Grid>
</Window>
```

------

## 案例 4：生产报表中心（切换懒加载）

### 场景说明

生产数据报表中心，分为产量报表、良率报表、设备 OEE 报表三个标签，切换到对应标签才加载数据，避免窗口初始化时同时查询三个报表导致启动卡顿，是大数据报表界面的标准性能优化方案。

### 对应核心特性

- `SelectionChanged` 标签切换事件
- 按需加载数据，提升启动速度
- 多分类报表集中管理

### 1. XAML 界面

xaml:

```xaml
<Grid Margin="12">
    <TabControl x:Name="ReportTabControl"
                SelectionChanged="ReportTabControl_SelectionChanged"
                BorderBrush="#DDD" BorderThickness="1">
        <TabItem Header="产量报表" Tag="Output">
            <Grid>
                <TextBlock x:Name="OutputReportPlaceholder" Text="加载中..." Foreground="#999" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                <DataGrid x:Name="OutputGrid" Visibility="Collapsed" AutoGenerateColumns="False">
                    <!-- 报表列定义 -->
                </DataGrid>
            </Grid>
        </TabItem>
        <TabItem Header="良率报表" Tag="Yield">
            <Grid>
                <TextBlock x:Name="YieldReportPlaceholder" Text="加载中..." Foreground="#999" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                <DataGrid x:Name="YieldGrid" Visibility="Collapsed" AutoGenerateColumns="False"/>
            </Grid>
        </TabItem>
        <TabItem Header="设备OEE报表" Tag="OEE">
            <Grid>
                <TextBlock x:Name="OeeReportPlaceholder" Text="加载中..." Foreground="#999" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                <DataGrid x:Name="OeeGrid" Visibility="Collapsed" AutoGenerateColumns="False"/>
            </Grid>
        </TabItem>
    </TabControl>
</Grid>
```

### 2. 后台懒加载逻辑

csharp:

```c#
private Dictionary<string, bool> _loadedTabs = new Dictionary<string, bool>();

private void ReportTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (e.AddedItems.Count == 0) return;
    
    var tab = e.AddedItems[0] as TabItem;
    var tag = tab?.Tag?.ToString();
    
    if (string.IsNullOrEmpty(tag) || _loadedTabs.ContainsKey(tag)) 
        return;

    // 切换到该标签时才加载数据
    _loadedTabs.Add(tag, true);
    LoadReportData(tag);
}

private void LoadReportData(string reportType)
{
    // 模拟异步查询数据库
    Task.Run(() =>
    {
        // 执行数据库查询...
        Thread.Sleep(500);
        
        Dispatcher.Invoke(() =>
        {
            switch (reportType)
            {
                case "Output":
                    OutputGrid.ItemsSource = GetOutputData();
                    OutputReportPlaceholder.Visibility = Visibility.Collapsed;
                    OutputGrid.Visibility = Visibility.Visible;
                    break;
                case "Yield":
                    YieldGrid.ItemsSource = GetYieldData();
                    YieldReportPlaceholder.Visibility = Visibility.Collapsed;
                    YieldGrid.Visibility = Visibility.Visible;
                    break;
                case "OEE":
                    OeeGrid.ItemsSource = GetOeeData();
                    OeeReportPlaceholder.Visibility = Visibility.Collapsed;
                    OeeGrid.Visibility = Visibility.Visible;
                    break;
            }
        });
    });
}
```

------

## 案例 5：产线工位状态监控标签

### 场景说明

产线多工位集中监控，每个标签对应一个工位，标签头直接显示工位运行状态（运行 / 报警 / 待机），操作人员通过标签颜色就能快速掌握整条产线的运行状态，无需逐个点开查看。

### 对应核心特性

- `ItemContainerStyle` 自定义标签背景色
- 数据触发器联动状态变色
- 标签头状态可视化，提升监控效率

xaml:

```xaml
<Window Title="产线工位监控" Height="450" Width="700">
    <Grid Margin="12">
        <TabControl ItemsSource="{Binding StationList}"
                    SelectedItem="{Binding SelectedStation}"
                    DisplayMemberPath="StationName"
                    BorderBrush="#DDD" BorderThickness="1">

            <!-- 标签容器样式：根据状态变色 -->
            <TabControl.ItemContainerStyle>
                <Style TargetType="TabItem">
                    <Setter Property="Padding" Value="16 6"/>
                    <Setter Property="Background" Value="#F0F0F0"/>
                    <Setter Property="Foreground" Value="#666"/>
                    <Style.Triggers>
                        <!-- 运行中：绿色 -->
                        <DataTrigger Binding="{Binding Status}" Value="Running">
                            <Setter Property="Background" Value="#F6FFED"/>
                            <Setter Property="Foreground" Value="#389E0D"/>
                        </DataTrigger>
                        <!-- 报警：红色 -->
                        <DataTrigger Binding="{Binding Status}" Value="Alarm">
                            <Setter Property="Background" Value="#FFF1F0"/>
                            <Setter Property="Foreground" Value="#CF1322"/>
                        </DataTrigger>
                        <!-- 选中态 -->
                        <Trigger Property="IsSelected" Value="True">
                            <Setter Property="Background" Value="White"/>
                            <Setter Property="Foreground" Value="#1890FF"/>
                            <Setter Property="FontWeight" Value="Bold"/>
                        </Trigger>
                    </Style.Triggers>
                </Style>
            </TabControl.ItemContainerStyle>

            <!-- 内容模板：工位详情 -->
            <TabControl.ContentTemplate>
                <DataTemplate>
                    <Border Padding="20" Background="#F8F9FA">
                        <StackPanel>
                            <TextBlock FontSize="16" FontWeight="Bold" Text="{Binding StationName}"/>
                            <Separator Margin="0 10"/>
                            <TextBlock Text="{Binding CurrentProduct, StringFormat=当前产品：{0}}"/>
                            <TextBlock Margin="0 5" Text="{Binding OutputToday, StringFormat=今日产量：{0}}"/>
                            <TextBlock Text="{Binding StatusText, StringFormat=运行状态：{0}}"/>
                        </StackPanel>
                    </Border>
                </DataTemplate>
            </TabControl.ContentTemplate>
        </TabControl>
    </Grid>
</Window>
```

------

## 工业场景最佳实践总结

1. **固定分类用静态标签，动态多开用绑定**
   - 设备详情、系统设置等固定分类，直接写静态 `TabItem`，结构清晰性能好；
   - 多设备、多文档等动态场景用 `ItemsSource` 绑定，统一管理。
2. **大数据页面必须懒加载**
   - 报表、历史记录、日志等数据量大的页面，不要在窗口初始化时全量加载；
   - 在 `SelectionChanged` 中按需加载，大幅提升启动速度，降低初始内存占用。
3. **标签头增加状态可视化**
   - 设备、工位、报警等监控场景，在标签头增加颜色指示灯或背景色，操作人员扫一眼就能掌握全局状态，大幅提升监控效率。
4. **工控宽屏优先左侧导航**
   - 工业现场大多使用宽屏显示器，`TabStripPlacement="Left"` 左侧垂直标签操作路径更短，符合工控软件的使用习惯。
5. **页面状态下沉到 ViewModel**
   - 由于 TabControl 默认切换会重建页面，输入值、选中项、展开状态等全部绑定到 ViewModel，切换后自动恢复，避免状态丢失。