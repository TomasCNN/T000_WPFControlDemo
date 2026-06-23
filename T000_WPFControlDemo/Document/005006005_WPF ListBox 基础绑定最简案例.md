# 005006005_WPF ListBox 基础绑定最简案例

这是一个最基础的 `ListBox` 数据绑定示例，采用标准 MVVM 模式，实现「列表数据展示 + 选中项联动」两个核心功能，代码精简、结构清晰，可直接运行。

------

## 案例功能

1. ListBox 展示学生列表（编号 + 姓名）
2. 选中列表项后，下方自动显示选中的学生详情
3. 纯数据驱动，不涉及复杂样式与交互

------

## 1. 数据模型（Student.cs）

csharp:

```c#
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ListBoxDemo
{
    public class Student : INotifyPropertyChanged
    {
        private int _id;
        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
```

------

## 2. 视图模型（MainViewModel.cs）

csharp:

```c#
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ListBoxDemo
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // 列表数据源
        public ObservableCollection<Student> StudentList { get; set; }

        // 当前选中项
        private Student _selectedStudent;
        public Student SelectedStudent
        {
            get => _selectedStudent;
            set { _selectedStudent = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {
            // 初始化测试数据
            StudentList = new ObservableCollection<Student>
            {
                new Student { Id = 1, Name = "张三" },
                new Student { Id = 2, Name = "李四" },
                new Student { Id = 3, Name = "王五" },
                new Student { Id = 4, Name = "赵六" }
            };

            // 默认选中第一项
            SelectedStudent = StudentList[0];
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
```

------

## 3. 界面布局（MainWindow.xaml）

xaml:

```xaml
<Window x:Class="ListBoxDemo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:ListBoxDemo"
        Title="ListBox基础绑定示例" Height="350" Width="400">
    
    <!-- 设置数据上下文 -->
    <Window.DataContext>
        <local:MainViewModel/>
    </Window.DataContext>

    <Grid Margin="15">
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- ListBox 核心绑定 -->
        <ListBox ItemsSource="{Binding StudentList}"
                 SelectedItem="{Binding SelectedStudent}"
                 DisplayMemberPath="Name"
                 BorderBrush="#DDD" BorderThickness="1"/>

        <!-- 选中详情展示 -->
        <Border Grid.Row="1" Margin="0 10 0 0" Padding="10" Background="#F5F7FA" BorderBrush="#DDD" BorderThickness="1">
            <TextBlock>
                <Run Text="当前选中："/>
                <Run Text="{Binding SelectedStudent.Id, StringFormat=编号{0}}"/>
                <Run Text=" - "/>
                <Run Text="{Binding SelectedStudent.Name}"/>
            </TextBlock>
        </Border>
    </Grid>
</Window>
```

------

## 核心绑定说明

| 属性                                       | 作用                                                         |
| :----------------------------------------- | :----------------------------------------------------------- |
| `ItemsSource="{Binding StudentList}"`      | 绑定数据源集合，ListBox 会自动为集合中的每个元素生成一个列表项 |
| `SelectedItem="{Binding SelectedStudent}"` | 双向绑定选中项，选中列表项时自动同步到 ViewModel，ViewModel 修改值也会同步到 UI |
| `DisplayMemberPath="Name"`                 | 指定列表项显示的字段名，不设置则默认显示对象的 `ToString()` 结果 |

## 运行效果

启动程序后，ListBox 显示 4 个学生姓名，点击不同列表项，底部文本会实时更新为对应学生的编号和姓名。