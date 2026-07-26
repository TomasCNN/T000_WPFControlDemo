# 007005002_WPF `ObservableCollection` 基础实例

这个实例聚焦最核心的特性：**集合元素增删时，UI 列表自动同步更新**，这也是 `ObservableCollection` 和普通 `List<T>` 最本质的区别。

实例场景：一个人员列表，支持「添加、删除选中项、清空」三个操作，所有操作只修改集合数据，UI 自动刷新，无需手动操作控件。

------

## 一、完整代码实现

### 1. 引用命名空间

`ObservableCollection` 位于 `System.Collections.ObjectModel` 命名空间，使用前需要引入。

### 2. 数据项类 + 视图模型

csharp:

```c#
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// ------------------------------
// 1. 数据项：单个人员信息
// ------------------------------
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

// ------------------------------
// 2. 视图模型：承载集合数据
// ------------------------------
public class MainViewModel
{
    /// <summary>
    /// 核心：可观察集合，增删自动通知UI
    /// </summary>
    public ObservableCollection<Person> PersonList { get; set; }

    public MainViewModel()
    {
        // 初始化3条默认数据
        PersonList = new ObservableCollection<Person>()
        {
            new Person { Name = "张三", Age = 25 },
            new Person { Name = "李四", Age = 30 },
            new Person { Name = "王五", Age = 28 }
        };
    }

    /// <summary> 添加一条新数据 </summary>
    public void AddNewPerson()
    {
        PersonList.Add(new Person
        {
            Name = $"新员工_{PersonList.Count + 1}",
            Age = 22
        });
    }

    /// <summary> 删除指定的人员 </summary>
    public void RemovePerson(Person person)
    {
        if (person != null)
        {
            PersonList.Remove(person);
        }
    }

    /// <summary> 清空所有数据 </summary>
    public void ClearAll()
    {
        PersonList.Clear();
    }
}
```

### 3. 窗口后台：设置数据上下文

csharp:

```c#
public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow()
    {
        InitializeComponent();
        
        // 初始化视图模型，设置为窗口的数据源
        _vm = new MainViewModel();
        this.DataContext = _vm;
    }

    // 添加按钮点击
    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        _vm.AddNewPerson();
    }

    // 删除选中项按钮点击
    private void BtnRemove_Click(object sender, RoutedEventArgs e)
    {
        // 获取列表中选中的项
        var selected = PersonListBox.SelectedItem as Person;
        _vm.RemovePerson(selected);
    }

    // 清空按钮点击
    private void BtnClear_Click(object sender, RoutedEventArgs e)
    {
        _vm.ClearAll();
    }
}
```

### 4. XAML 界面：集合绑定

xaml:

```xaml
<Window x:Class="Demo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="ObservableCollection基础示例" Height="450" Width="400">
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition/>
        </Grid.RowDefinitions>

        <!-- 顶部操作按钮 -->
        <StackPanel Orientation="Horizontal" Spacing="10" Margin="0 0 0 15">
            <Button Content="添加人员" Click="BtnAdd_Click" Width="100" Height="30"/>
            <Button Content="删除选中" Click="BtnRemove_Click" Width="100" Height="30"/>
            <Button Content="清空列表" Click="BtnClear_Click" Width="100" Height="30"/>
        </StackPanel>

        <!-- 列表控件：ItemsSource 绑定集合 -->
        <ListBox x:Name="PersonListBox"
                 Grid.Row="1"
                 ItemsSource="{Binding PersonList}"
                 FontSize="14"
                 BorderBrush="#DDD" BorderThickness="1">
            <!-- 数据模板：定义每一行的显示样式 -->
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <StackPanel Orientation="Horizontal" Spacing="30" Padding="8">
                        <TextBlock Text="{Binding Name, StringFormat=姓名：{0}}"/>
                        <TextBlock Text="{Binding Age, StringFormat=年龄：{0}岁}"/>
                    </StackPanel>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>
    </Grid>
</Window>
```

------

## 二、运行效果与核心说明

### 实际效果

1. **添加**：点击「添加人员」，列表会自动多出一行，不需要手动调用控件的刷新方法；
2. **删除**：选中某一行，点击「删除选中」，对应行会自动消失；
3. **清空**：点击「清空列表」，所有行瞬间全部移除。

全程只修改了 `PersonList` 集合本身，没有写任何操作 `ListBox` 的代码，UI 完全由数据自动驱动。

### 对比：如果换成普通 `List<T>`

把 `ObservableCollection<Person>` 改成 `List<Person>`，同样的操作逻辑，界面会完全没有反应：

- 原因：`List<T>` 没有集合变更通知，WPF 不知道集合里的元素变了，自然不会更新 UI；
- 这就是 `ObservableCollection` 存在的核心意义：给集合加上「变更主动通知」的能力。

------

## 三、进阶补充（必知细节）

### 1. 修改元素属性，界面会更新吗？

**不会**。

`ObservableCollection` 只负责通知「集合结构的变化」（增、删、清空、移动），不负责通知「元素内部属性的变化」。

比如执行 `PersonList[0].Name = "新名字"`，界面上的姓名不会自动刷新。

如果需要元素属性变化也同步 UI，**数据项类必须自己实现 `INotifyPropertyChanged` 接口**：

csharp:

```c#
public class Person : INotifyPropertyChanged
{
    private string _name;
    public string Name
    {
        get => _name;
        set 
        { 
            _name = value;
            OnPropertyChanged();
        }
    }

    private int _age;
    public int Age
    {
        get => _age;
        set 
        { 
            _age = value; 
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

### 2. 跨线程修改会报错

`ObservableCollection` 默认只能在 UI 线程修改，后台线程（比如 PLC 通信线程、算法线程）直接增删元素会抛出「跨线程操作无效」的异常。

简单解决方式：把操作调度到 UI 线程执行

csharp:

```c#
Application.Current.Dispatcher.Invoke(() =>
{
    PersonList.Add(newPerson);
});
```

### 3. 批量添加避免卡顿

循环调用 `Add` 会每次都触发 UI 刷新，添加几百上千条时会卡顿。

批量导入数据时，建议直接新建一个集合并替换整体属性，或自定义 `AddRange` 扩展方法。

------

## 四、一句话总结

`ObservableCollection<T>` = 普通集合功能 + 自动变更通知，是 WPF 列表数据绑定的标配数据源，只要集合结构变化，UI 自动同步，彻底替代手动操作控件集合的开发方式。