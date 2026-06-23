# 005009004_WPF `MenuItem` 工业场景实战实例合集

以下实例全部贴合工业上位机、设备管理、生产监控等真实业务场景，覆盖**复选开关、图标快捷键、MVVM 命令绑定、右键上下文菜单、动态权限菜单、子菜单懒加载**等核心能力，每个案例明确标注对应 `MenuItem` 特性，可直接复用到工业项目中。

------

## 案例 1：工业系统多级主菜单（基础嵌套 + 分隔线 + 访问键）

### 场景说明

窗口顶部系统主菜单，包含「文件、操作、视图、帮助」四大分类，支持多级子菜单、分组分隔线、Alt 快捷访问键，是工业上位机软件的标准顶部导航。

### 对应核心特性

- `Header` 菜单标题 + `_` 访问键标记
- 子菜单递归嵌套（MenuItem 自身也是集合容器）
- `Separator` 分组分隔线
- 自动识别 `TopLevelHeader` / `SubmenuItem` 角色并切换样式

xaml:

```xaml
<Window x:Class="MenuItemDemo.MainWindow"
        Title="生产监控系统" Height="450" Width="700">
    <DockPanel>
        <!-- 顶部主菜单栏 -->
        <Menu DockPanel.Dock="Top">
            <!-- 文件菜单（顶级头菜单） -->
            <MenuItem Header="_文件">
                <MenuItem Header="导出生产数据" InputGestureText="Ctrl+E"/>
                <MenuItem Header="打印报表"/>
                <Separator/>
                <MenuItem Header="退出系统" InputGestureText="Alt+F4"/>
            </MenuItem>

            <!-- 操作菜单 -->
            <MenuItem Header="_操作">
                <MenuItem Header="全线启动"/>
                <MenuItem Header="全线停止"/>
                <MenuItem Header="紧急复位"/>
                <Separator/>
                <MenuItem Header="配方管理">
                    <MenuItem Header="加载配方"/>
                    <MenuItem Header="保存配方"/>
                    <MenuItem Header="配方参数配置"/>
                </MenuItem>
            </MenuItem>

            <!-- 视图菜单 -->
            <MenuItem Header="_视图">
                <MenuItem Header="显示工具栏" IsCheckable="True" IsChecked="True"/>
                <MenuItem Header="显示状态栏" IsCheckable="True" IsChecked="True"/>
            </MenuItem>

            <!-- 帮助菜单 -->
            <MenuItem Header="_帮助">
                <MenuItem Header="操作手册"/>
                <Separator/>
                <MenuItem Header="关于系统"/>
            </MenuItem>
        </Menu>

        <!-- 主内容区 -->
        <Grid Background="#F8F9FA">
            <TextBlock Text="生产监控主界面" HorizontalAlignment="Center" VerticalAlignment="Center" Foreground="#999"/>
        </Grid>
    </DockPanel>
</Window>
```

### 要点解析

1. **访问键**：`Header` 中 `_` 开头的字符为 Alt 快捷访问键，如 `_文件` 对应 `Alt+F` 快速打开菜单；
2. **自动角色识别**：顶级菜单、子菜单、带子项的菜单会自动计算 `Role` 属性，加载对应外观模板，无需手动指定；
3. **分隔线**：`<Separator/>` 用于菜单内部分组，自动应用 `SeparatorStyleKey` 样式。

------

## 案例 2：视图开关复选菜单（IsCheckable + StaysOpenOnClick）

### 场景说明

视图菜单中提供「显示工具栏、显示状态栏、深色主题、全屏模式」等开关选项，点击后菜单保持打开状态，支持连续切换多个选项，是工业界面视图配置的常用交互。

### 对应核心特性

- `IsCheckable` 开启复选模式
- `IsChecked` 绑定选中状态
- `StaysOpenOnClick` 点击后保持菜单展开
- `Checked` / `Unchecked` 状态变更事件

xaml:

```xaml
<MenuItem Header="_视图">
    <MenuItem Header="显示工具栏" 
              IsCheckable="True" IsChecked="True"
              StaysOpenOnClick="True"
              Checked="Toolbar_Checked" Unchecked="Toolbar_Unchecked"/>
    
    <MenuItem Header="显示状态栏" 
              IsCheckable="True" IsChecked="True"
              StaysOpenOnClick="True"
              Checked="StatusBar_Checked" Unchecked="StatusBar_Unchecked"/>
    
    <Separator/>
    
    <MenuItem Header="深色主题" 
              IsCheckable="True"
              StaysOpenOnClick="True"
              Checked="DarkTheme_Checked" Unchecked="DarkTheme_Unchecked"/>
    
    <MenuItem Header="全屏模式" 
              IsCheckable="True"
              StaysOpenOnClick="True"
              Checked="FullScreen_Checked" Unchecked="FullScreen_Unchecked"/>
</MenuItem>
```

#### 后台事件处理

csharp:

```c#
private void Toolbar_Checked(object sender, RoutedEventArgs e)
{
    // 显示工具栏逻辑
    MainToolBar.Visibility = Visibility.Visible;
}

private void Toolbar_Unchecked(object sender, RoutedEventArgs e)
{
    // 隐藏工具栏逻辑
    MainToolBar.Visibility = Visibility.Collapsed;
}
```

### 要点解析

1. **StaysOpenOnClick**：默认点击菜单项后菜单会自动关闭，设置为 `true` 可保持展开，适合连续切换的复选场景；
2. **MVVM 替代方案**：可将 `IsChecked` 双向绑定到 ViewModel 的布尔属性，替代事件处理，保持架构整洁。

------

## 案例 3：带图标与快捷键的设备操作菜单

### 场景说明

操作菜单中的「启动、停止、复位」等核心操作，左侧增加状态色图标，右侧标注快捷键提示，直观区分操作类型，提升工控操作效率。

### 对应核心特性

- `Icon` 菜单左侧图标（支持任意 UI 元素，不局限于图片）
- `InputGestureText` 快捷键提示文本

xaml:

```xaml
<MenuItem Header="_操作">
    <MenuItem Header="全线启动" InputGestureText="F5">
        <MenuItem.Icon>
            <!-- 用圆形色块作为状态图标，工业场景常用 -->
            <Ellipse Width="12" Height="12" Fill="#52C41A" VerticalAlignment="Center"/>
        </MenuItem.Icon>
    </MenuItem>
    
    <MenuItem Header="全线停止" InputGestureText="F6">
        <MenuItem.Icon>
            <Ellipse Width="12" Height="12" Fill="#FAAD14" VerticalAlignment="Center"/>
        </MenuItem.Icon>
    </MenuItem>
    
    <MenuItem Header="紧急复位" InputGestureText="Ctrl+R">
        <MenuItem.Icon>
            <Ellipse Width="12" Height="12" Fill="#F5222D" VerticalAlignment="Center"/>
        </MenuItem.Icon>
    </MenuItem>
</MenuItem>
```

### 要点解析

1. **Icon 属性**：类型为 `object`，支持文本、图形、图片、自定义控件等任意内容，工业场景常用简单几何图形替代图标文件，减少资源依赖；

2. **InputGestureText**：仅用于显示快捷键文本，不会自动绑定实际按键功能；实际快捷键需要通过 `Window.InputBindings` 配合命令实现：

   xaml:

   ```xaml
   <Window.InputBindings>
       <KeyBinding Key="F5" Command="{Binding StartAllCommand}"/>
   </Window.InputBindings>
   ```

------

## 案例 4：MVVM 命令绑定（Command + CommandParameter）

### 场景说明

设备操作菜单绑定 ViewModel 中的命令，替代 `Click` 事件，支持权限控制（无权限自动灰显）、参数传递，是工业软件 MVVM 架构的标准实现。

### 对应核心特性

- `Command` 绑定命令对象（实现 `ICommand` 接口）
- `CommandParameter` 传递命令参数
- 自动联动 `CanExecute` 控制菜单项启用状态

#### 1. 简易 RelayCommand 实现

csharp:

```c#
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
    public void Execute(object parameter) => _execute(parameter);
    public event EventHandler CanExecuteChanged;
}
```

#### 2. ViewModel 命令定义

csharp:

```c#
public class MainViewModel : INotifyPropertyChanged
{
    public ICommand StartDeviceCommand { get; }
    public ICommand StopDeviceCommand { get; }

    public MainViewModel()
    {
        StartDeviceCommand = new RelayCommand(ExecuteStartDevice, CanStartDevice);
        StopDeviceCommand = new RelayCommand(ExecuteStopDevice, CanStopDevice);
    }

    private bool CanStartDevice(object parameter)
    {
        // 权限校验 + 设备状态校验，返回false时菜单项自动灰显
        return UserHasPermission && !IsDeviceRunning;
    }

    private void ExecuteStartDevice(object parameter)
    {
        var deviceId = parameter?.ToString();
        // 执行设备启动逻辑
    }

    // 其他方法略...
}
```

#### 3. XAML 命令绑定

xaml:

```xaml
<Menu DockPanel.Dock="Top" DataContext="{Binding}">
    <MenuItem Header="_操作">
        <MenuItem Header="启动设备" 
                  Command="{Binding StartDeviceCommand}"
                  CommandParameter="PLC-001"/>
        <MenuItem Header="停止设备" 
                  Command="{Binding StopDeviceCommand}"
                  CommandParameter="PLC-001"/>
    </MenuItem>
</Menu>
```

### 要点解析

1. **自动禁用**：当 `CanExecute` 返回 `false` 时，菜单项自动灰显且不可点击，无需手动绑定 `IsEnabled`；
2. **参数传递**：`CommandParameter` 可传递设备编号、操作类型等业务数据，适合通用命令复用；
3. **架构优势**：命令逻辑集中在 ViewModel 中，便于单元测试与权限统一管控。

------

## 案例 5：设备列表右键上下文菜单

### 场景说明

设备列表的每一项支持右键操作，弹出「启动、停止、查看详情、导出记录」快捷菜单，是工业设备监控系统最高频的交互方式。

### 对应核心特性

- `ContextMenu` 右键菜单容器
- `PlacementTarget` 解决右键菜单数据上下文绑定问题
- `CommandParameter` 传递当前选中的设备对象

xaml:

```xaml
<ListBox ItemsSource="{Binding DeviceList}"
         DisplayMemberPath="DeviceName"
         BorderBrush="#DDD" BorderThickness="1"
         Width="260" Margin="10">
    <ListBox.ItemContainerStyle>
        <Style TargetType="ListBoxItem">
            <Setter Property="Padding" Value="8 4"/>
            <!-- 为每个列表项绑定右键菜单 -->
            <Setter Property="ContextMenu">
                <Setter.Value>
                    <ContextMenu>
                        <!-- 
                            核心：ContextMenu 在独立 Popup 中，不在主视觉树
                            通过 PlacementTarget.DataContext 获取列表项的数据上下文
                        -->
                        <MenuItem Header="启动设备"
                                  Command="{Binding PlacementTarget.DataContext.StartDeviceCommand, 
                                            RelativeSource={RelativeSource AncestorType=ContextMenu}}"
                                  CommandParameter="{Binding}"/>
                        
                        <MenuItem Header="停止设备"
                                  Command="{Binding PlacementTarget.DataContext.StopDeviceCommand, 
                                            RelativeSource={RelativeSource AncestorType=ContextMenu}}"
                                  CommandParameter="{Binding}"/>
                        
                        <Separator/>
                        
                        <MenuItem Header="查看详情"
                                  Command="{Binding PlacementTarget.DataContext.ViewDetailCommand, 
                                            RelativeSource={RelativeSource AncestorType=ContextMenu}}"
                                  CommandParameter="{Binding}"/>
                        
                        <MenuItem Header="导出运行记录"
                                  Command="{Binding PlacementTarget.DataContext.ExportRecordCommand, 
                                            RelativeSource={RelativeSource AncestorType=ContextMenu}}"
                                  CommandParameter="{Binding}"/>
                    </ContextMenu>
                </Setter.Value>
            </Setter>
        </Style>
    </ListBox.ItemContainerStyle>
</ListBox>
```

### 要点解析

1. **绑定痛点**：`ContextMenu` 位于独立的 `Popup` 窗口中，不在主界面视觉树内，无法直接继承父级 `DataContext`；
2. **标准解决方案**：通过 `RelativeSource` 找到 `ContextMenu` 自身，再通过 `PlacementTarget.DataContext` 获取右键目标元素的数据上下文；
3. **参数传递**：`CommandParameter="{Binding}"` 直接传递当前设备对象，命令中可获取完整设备信息。

------

## 案例 6：动态权限菜单（数据驱动生成）

### 场景说明

根据登录用户的角色权限动态生成菜单，管理员可见全部菜单，操作员仅可见操作与查询菜单，避免硬编码权限判断，适合多角色工业管理系统。

### 对应核心特性

- `HierarchicalDataTemplate` 分层数据模板（递归生成多级菜单）
- `ItemsSource` 绑定菜单集合
- 纯数据驱动，权限逻辑集中在 ViewModel

#### 1. 菜单数据模型

csharp:

```c#
public class MenuItemModel : INotifyPropertyChanged
{
    public string MenuHeader { get; set; }
    public string IconColor { get; set; }
    public ICommand MenuCommand { get; set; }
    public object CommandParam { get; set; }
    public ObservableCollection<MenuItemModel> Children { get; set; } = new ObservableCollection<MenuItemModel>();
}
```

#### 2. ViewModel 动态构建菜单

csharp:

```c#
public class MainViewModel
{
    public ObservableCollection<MenuItemModel> SystemMenus { get; set; }

    public MainViewModel(string userRole)
    {
        SystemMenus = new ObservableCollection<MenuItemModel>();
        
        // 所有角色都可见
        SystemMenus.Add(new MenuItemModel { MenuHeader = "_文件" });
        SystemMenus.Add(new MenuItemModel { MenuHeader = "_视图" });
        
        // 操作员和管理员可见
        if (userRole is "操作员" or "管理员")
        {
            SystemMenus.Add(new MenuItemModel
            {
                MenuHeader = "_操作",
                Children = new ObservableCollection<MenuItemModel>
                {
                    new() { MenuHeader = "全线启动", IconColor = "#52C41A" },
                    new() { MenuHeader = "全线停止", IconColor = "#FAAD14" }
                }
            });
        }
        
        // 仅管理员可见
        if (userRole == "管理员")
        {
            SystemMenus.Add(new MenuItemModel
            {
                MenuHeader = "_系统",
                Children = new ObservableCollection<MenuItemModel>
                {
                    new() { MenuHeader = "用户权限管理" },
                    new() { MenuHeader = "系统参数配置" }
                }
            });
        }
        
        SystemMenus.Add(new MenuItemModel { MenuHeader = "_帮助" });
    }
}
```

#### 3. XAML 分层模板绑定

xaml:

```xaml
<Menu DockPanel.Dock="Top" ItemsSource="{Binding SystemMenus}">
    <Menu.ItemTemplate>
        <HierarchicalDataTemplate DataType="{x:Type local:MenuItemModel}"
                                  ItemsSource="{Binding Children}">
            <StackPanel Orientation="Horizontal">
                <Ellipse Width="10" Height="10" Fill="{Binding IconColor}" 
                         VerticalAlignment="Center" Margin="0 0 6 0"/>
                <TextBlock Text="{Binding MenuHeader}"/>
            </StackPanel>
        </HierarchicalDataTemplate>
    </Menu.ItemTemplate>
</Menu>
```

### 要点解析

1. **分层模板复用**：和 TreeView 原理一致，`HierarchicalDataTemplate` 会递归应用到所有子层级，自动生成多级菜单；
2. **权限集中管控**：菜单生成逻辑统一在 ViewModel 中，便于维护和扩展，避免 XAML 中散落大量权限判断；
3. **扩展性**：可进一步扩展为从数据库、配置文件加载菜单结构，实现完全动态化。

------

## 案例 7：子菜单异步懒加载

### 场景说明

配方管理、产品型号等菜单项数量多、数据来自数据库，初始不加载子菜单，点击展开时才异步查询加载，避免启动时加载大量数据拖慢速度。

### 对应核心特性

- `SubmenuOpened` 子菜单展开事件
- 异步数据加载
- 一次性加载，重复展开不重复查询

xaml:

```xaml
<MenuItem Header="配方管理" SubmenuOpened="RecipeMenu_SubmenuOpened">
    <!-- 占位项，展开后替换 -->
    <MenuItem Header="加载中..." IsEnabled="False"/>
</MenuItem>
```

#### 后台异步加载逻辑

csharp:

```c#
private bool _recipeMenuLoaded = false;

private void RecipeMenu_SubmenuOpened(object sender, RoutedEventArgs e)
{
    if (_recipeMenuLoaded) return;
    var parentMenu = sender as MenuItem;
    if (parentMenu == null) return;

    // 异步加载配方列表
    Task.Run(() =>
    {
        // 模拟数据库查询耗时
        Thread.Sleep(400);
        var recipeList = GetRecipeListFromDb();

        Dispatcher.Invoke(() =>
        {
            parentMenu.Items.Clear();
            foreach (var recipe in recipeList)
            {
                var item = new MenuItem
                {
                    Header = recipe.RecipeName,
                    Command = LoadRecipeCommand,
                    CommandParameter = recipe.RecipeCode
                };
                parentMenu.Items.Add(item);
            }
            _recipeMenuLoaded = true;
        });
    });
}
```

### 要点解析

1. **性能优化**：对于层级深、数据量大的菜单，懒加载可大幅提升界面启动速度；
2. **加载标记**：用布尔变量标记是否已加载，避免重复展开重复查询；
3. **用户体验**：初始添加「加载中...」占位项，避免用户感知卡顿。

------

## 工业场景最佳实践总结

1. **优先命令绑定**：工业系统推荐统一用 `Command` 替代 `Click` 事件，便于权限管控、单元测试和逻辑复用；
2. **高频操作放右键**：设备启停、参数查看等高频操作下沉到右键上下文菜单，缩短操作路径，提升工控效率；
3. **复选菜单保持展开**：视图切换、功能开关类的复选菜单项，务必设置 `StaysOpenOnClick="True"`，避免点击一次就关闭；
4. **大菜单做懒加载**：配方、产品、设备等数据量大的子菜单，采用 `SubmenuOpened` 异步加载，优化启动性能；
5. **权限动态生成**：多角色系统不要在 XAML 硬编码 `IsEnabled`，通过 ViewModel 动态构建菜单集合，权限逻辑集中管理。

