# 005010004_WPF `ContextMenu` 上下文菜单基类相关案例

## 一、基础入门案例

### 1. 最简右键菜单（图标 + 分隔线 + 系统命令）

直接挂载到控件的 `ContextMenu` 属性，右键自动弹出，复用 WPF 内置系统命令，无需额外写后台逻辑。

xaml:

```xaml
<TextBox Width="200" Text="右键测试文本">
    <TextBox.ContextMenu>
        <ContextMenu>
            <MenuItem Header="复制" Command="ApplicationCommands.Copy">
                <MenuItem.Icon>
                    <Image Source="/Images/copy.png" Width="16" Height="16"/>
                </MenuItem.Icon>
            </MenuItem>
            <MenuItem Header="粘贴" Command="ApplicationCommands.Paste"/>
            <Separator/>
            <MenuItem Header="全选" Command="ApplicationCommands.SelectAll"/>
        </ContextMenu>
    </TextBox.ContextMenu>
</TextBox>
```

**说明**：

- `ApplicationCommands` 是 WPF 内置路由命令，自动匹配宿主控件的对应能力（文本框的复制 / 粘贴 / 全选无需手写代码）。
- `Separator` 用于菜单项分组分隔线；`MenuItem.Icon` 用于设置菜单图标。

### 2. 全局资源复用（多控件共享菜单）

多个控件需要相同菜单时，定义为资源统一引用，避免重复代码。

xaml:

```xaml
<Window.Resources>
    <ContextMenu x:Key="CommonContextMenu">
        <MenuItem Header="刷新" Command="NavigationCommands.Refresh"/>
        <MenuItem Header="导出"/>
        <Separator/>
        <MenuItem Header="属性"/>
    </ContextMenu>
</Window.Resources>

<!-- 多个控件复用同一个菜单 -->
<Button Content="按钮A" ContextMenu="{StaticResource CommonContextMenu}"/>
<Button Content="按钮B" ContextMenu="{StaticResource CommonContextMenu}"/>
```

------

## 二、MVVM 模式绑定案例（核心避坑）

`ContextMenu` 底层承载在独立的 `Popup` 窗口中，**不属于主窗口视觉树**，直接用 `RelativeSource FindAncestor` 绑定会失效，以下是工业界标准解决方案。

### 1. ViewModel 命令绑定（PlacementTarget 桥接法）

通过 `PlacementTarget` 间接拿到宿主控件的 `DataContext`，实现命令与数据绑定。

**XAML 代码**：

xaml:

```xaml
<Button Content="右键我" DataContext="{Binding}">
    <Button.ContextMenu>
        <ContextMenu>
            <!-- 核心：通过 PlacementTarget 桥接主视觉树的 DataContext -->
            <MenuItem Header="删除数据"
                      Command="{Binding PlacementTarget.DataContext.DeleteCommand, RelativeSource={RelativeSource AncestorType=ContextMenu}}"
                      CommandParameter="{Binding PlacementTarget.DataContext.CurrentId, RelativeSource={RelativeSource AncestorType=ContextMenu}}"/>
            <MenuItem Header="查看详情"
                      Command="{Binding PlacementTarget.DataContext.DetailCommand, RelativeSource={RelativeSource Self}}"/>
        </ContextMenu>
    </Button.ContextMenu>
</Button>
```

**ViewModel 简易实现**：

csharp:

```c#
public class MainViewModel : INotifyPropertyChanged
{
    public int CurrentId { get; set; }
    public ICommand DeleteCommand { get; }
    public ICommand DetailCommand { get; }

    public MainViewModel()
    {
        DeleteCommand = new RelayCommand(OnDelete);
        DetailCommand = new RelayCommand(OnShowDetail);
    }

    private void OnDelete(object obj) => MessageBox.Show($"删除ID：{obj}");
    private void OnShowDetail(object obj) => MessageBox.Show("查看详情");

    public event PropertyChangedEventHandler PropertyChanged;
}

// 简易 RelayCommand 实现
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;
    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
    public void Execute(object parameter) => _execute(parameter);
}
```

### 2. 代码控制菜单显隐（IsOpen 双向绑定）

通过 ViewModel 直接控制菜单弹出与关闭，无需后台操作 UI。

xaml:

```xaml
<Button Content="点击弹出菜单" x:Name="Btn">
    <Button.ContextMenu>
        <ContextMenu IsOpen="{Binding IsMenuOpen, Mode=TwoWay}"
                     PlacementTarget="{Binding ElementName=Btn}"
                     Placement="Bottom">
            <MenuItem Header="菜单项1"/>
            <MenuItem Header="菜单项2"/>
        </ContextMenu>
    </Button.ContextMenu>
</Button>
```

ViewModel 中添加属性：

csharp:

```c#
private bool _isMenuOpen;
public bool IsMenuOpen
{
    get => _isMenuOpen;
    set { _isMenuOpen = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMenuOpen))); }
}
```

------

## 三、弹出位置控制案例

### 1. 常用 Placement 定位模式

指定菜单相对于目标控件的弹出基准，替代默认的 “鼠标位置弹出”。

xaml:

```xaml
<Button x:Name="DemoBtn" Content="下方弹出菜单" Width="120">
    <Button.ContextMenu>
        <ContextMenu Placement="Bottom"
                     PlacementTarget="{Binding ElementName=DemoBtn}"
                     HorizontalOffset="0"
                     VerticalOffset="5">
            <MenuItem Header="选项A"/>
            <MenuItem Header="选项B"/>
        </ContextMenu>
    </Button.ContextMenu>
</Button>
```

**常用 `PlacementMode` 枚举值**：

- `MousePoint`：默认值，以鼠标坐标为原点弹出
- `Bottom` / `Top` / `Right` / `Left`：对齐目标控件的对应方位
- `Center`：在目标控件正中心弹出
- `Relative`：相对于目标左上角，完全由偏移量控制位置

### 2. 自定义弹出位置（CustomPopupPlacementCallback）

内置模式不满足需求时，通过回调手动计算坐标，自动避让屏幕边界。

**XAML 代码**：

xaml:

```xaml
<Button x:Name="CustomBtn" Content="自定义位置菜单">
    <Button.ContextMenu>
        <ContextMenu Placement="Custom"
                     CustomPopupPlacementCallback="GetCustomMenuPosition"
                     PlacementTarget="{Binding ElementName=CustomBtn}">
            <MenuItem Header="自定义位置1"/>
            <MenuItem Header="自定义位置2"/>
        </ContextMenu>
    </Button.ContextMenu>
</Button>
```

**后台回调代码**：

csharp:

```c#
private CustomPopupPlacement[] GetCustomMenuPosition(Size popupSize, Size targetSize, Point offset)
{
    // 候选位置按优先级排序，WPF自动选择第一个不超出屏幕的位置
    var position1 = new CustomPopupPlacement(
        new Point(targetSize.Width + 5, 0),  // 目标右侧5像素
        PopupPrimaryAxis.Horizontal);

    var position2 = new CustomPopupPlacement(
        new Point(-popupSize.Width - 5, 0),  // 目标左侧5像素（右侧超出屏幕时备用）
        PopupPrimaryAxis.Horizontal);

    return new[] { position1, position2 };
}
```

------

## 四、样式与外观自定义

### 1. 自定义整体样式（圆角 + 去阴影 + 自定义背景）

重写 `ContextMenu` 控件模板，修改默认外观。

xaml:

```xaml
<Window.Resources>
    <Style TargetType="ContextMenu">
        <Setter Property="HasDropShadow" Value="False"/>
        <Setter Property="Background" Value="#2b2b2b"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="BorderBrush" Value="#3e3e42"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="Padding" Value="2"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="ContextMenu">
                    <Border Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            CornerRadius="6"
                            Padding="{TemplateBinding Padding}">
                        <ItemsPresenter/>
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</Window.Resources>
```

### 2. 自定义菜单项悬停效果

修改 `MenuItem` 默认的高亮样式，适配深色主题。

xaml:

```xaml
<Style TargetType="MenuItem">
    <Setter Property="Padding" Value="12,6"/>
    <Setter Property="Background" Value="Transparent"/>
    <Style.Triggers>
        <Trigger Property="IsHighlighted" Value="True">
            <Setter Property="Background" Value="#007acc"/>
            <Setter Property="Foreground" Value="White"/>
        </Trigger>
        <Trigger Property="IsEnabled" Value="False">
            <Setter Property="Foreground" Value="#666"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

------

## 五、业务场景实战

### 1. DataGrid 行右键菜单（获取当前行数据）

右键 DataGrid 某一行时，菜单可获取当前行的业务数据，是后台管理系统的高频场景。

xaml:

```xaml
<DataGrid x:Name="UserGrid" ItemsSource="{Binding UserList}" AutoGenerateColumns="False">
    <DataGrid.RowStyle>
        <Style TargetType="DataGridRow">
            <!-- 将主ViewModel存入Tag，用于绑定命令 -->
            <Setter Property="Tag" Value="{Binding DataContext, ElementName=UserGrid}"/>
            <Setter Property="ContextMenu">
                <Setter.Value>
                    <ContextMenu>
                        <!-- 命令绑定主ViewModel，参数为当前行数据 -->
                        <MenuItem Header="编辑用户"
                                  Command="{Binding PlacementTarget.Tag.EditUserCommand, RelativeSource={RelativeSource Self}}"
                                  CommandParameter="{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}"/>
                        <MenuItem Header="删除用户"
                                  Command="{Binding PlacementTarget.Tag.DeleteUserCommand, RelativeSource={RelativeSource Self}}"
                                  CommandParameter="{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}"/>
                    </ContextMenu>
                </Setter.Value>
            </Setter>
        </Style>
    </DataGrid.RowStyle>
    <DataGrid.Columns>
        <DataGridTextColumn Header="用户名" Binding="{Binding UserName}"/>
        <DataGridTextColumn Header="手机号" Binding="{Binding Phone}"/>
    </DataGrid.Columns>
</DataGrid>
```

**关键说明**：

- `PlacementTarget.DataContext`：当前行的业务实体对象
- `PlacementTarget.Tag`：我们预先存入的主 ViewModel，用于绑定全局命令

### 2. 动态生成菜单项（绑定集合）

菜单项不固定、需根据权限 / 配置动态生成时，通过 `ItemsSource` 绑定集合实现。

**ViewModel 定义菜单模型与集合**：

csharp:

```c#
public class MenuItemModel
{
    public string Header { get; set; }
    public ICommand Command { get; set; }
    public object CommandParameter { get; set; }
}

// ViewModel 中
public ObservableCollection<MenuItemModel> DynamicMenus { get; set; } = new();

// 初始化时动态添加
DynamicMenus.Add(new MenuItemModel { Header = "新增", Command = AddCommand });
DynamicMenus.Add(new MenuItemModel { Header = "导出", Command = ExportCommand });
```

**XAML 绑定**：

xaml:

```xaml
<ContextMenu ItemsSource="{Binding PlacementTarget.DataContext.DynamicMenus, RelativeSource={RelativeSource Self}}">
    <ContextMenu.ItemContainerStyle>
        <Style TargetType="MenuItem">
            <Setter Property="Header" Value="{Binding Header}"/>
            <Setter Property="Command" Value="{Binding Command}"/>
            <Setter Property="CommandParameter" Value="{Binding CommandParameter}"/>
        </Style>
    </ContextMenu.ItemContainerStyle>
</ContextMenu>
```

### 3. 多级子菜单

嵌套 `MenuItem` 实现层级菜单，支持无限级嵌套。

xaml：

```
<ContextMenu>
    <MenuItem Header="文件">
        <MenuItem Header="新建"/>
        <MenuItem Header="打开">
            <MenuItem Header="本地文件"/>
            <MenuItem Header="最近文件"/>
        </MenuItem>
        <Separator/>
        <MenuItem Header="保存"/>
    </MenuItem>
    <MenuItem Header="编辑">
        <MenuItem Header="撤销"/>
        <MenuItem Header="重做"/>
    </MenuItem>
</ContextMenu>
```