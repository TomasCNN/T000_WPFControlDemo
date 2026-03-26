# 002001001_DispatcherObject类

​	用大白话讲，`DispatcherObject` 就是 WPF 给所有 “UI 相关对象”（比如窗口、按钮、文本框）配的 **“线程门禁管理员”** —— 核心就管一件事：确保 UI 对象只能被 “创建它的那个线程（UI 线程）” 操作，不让其他线程随便乱动，避免 UI 乱套、崩溃。

## 先搞懂两个关键类比（新手秒懂）

- 把**UI 线程**比作 “奶茶店的操作台”：所有 UI 对象（按钮、窗口）都是 “操作台上的奶茶杯”，只能在这个操作台上做加冰、加奶、贴标签这些操作；
- 把**其他线程**（比如后台采集数据的线程）比作 “店外的顾客”：顾客不能直接冲进来碰操作台的杯子，想给杯子加东西，必须通过店里的 “跑腿小哥”（`Dispatcher`）传话，由小哥到操作台上完成操作；
- 而`DispatcherObject`就是给每个 “奶茶杯（UI 对象）” 贴的 **“专属门禁贴”** —— 只有操作台（UI 线程）能直接碰，外人（其他线程）碰之前必须先查门禁，或者找跑腿小哥。

## 核心关键点（通俗版）

### 1. 谁会 “戴这个门禁”？

​	所有和 UI 相关的对象都继承自`DispatcherObject`（相当于都贴了门禁贴）：

- ✅ 窗口（Window）、按钮（Button）、文本框（TextBox）、布局面板（Grid/StackPanel）等；
- ❌ 普通的业务对象（比如 ViewModel、数据模型、工具类）不继承 —— 它们是 “店外的东西”，没门禁，随便哪个线程都能碰。

### 2. 这个 “门禁” 能做啥？

#### （1）查权限：`CheckAccess()` 方法

​	就像 “刷门禁卡”，判断当前线程能不能直接操作这个 UI 对象：

- 能操作（当前是 UI 线程）：返回`true`，可以直接改属性（比如`btn.Text = "采集完成"`）；
- 不能操作（当前是其他线程）：返回`false`，直接改就会报错，必须找 “跑腿小哥”。

#### （2）找跑腿小哥：`Dispatcher` 属性

​	每个`DispatcherObject`都有个`Dispatcher`属性，这就是 “跑腿小哥”—— 其他线程想操作 UI 对象，就通过这个小哥把 “操作指令”（比如改按钮文字）送到 UI 线程执行。

### 举个实际例子（新手能对应上）

​	比如你在后台线程采集 PLC 数据，想把数据显示到文本框里，直接写会报错：

csharp:

```c#
// 错误做法：后台线程直接碰UI对象（相当于顾客冲操作台）
private void CollectDataThread()
{
    // 采集数据
    string data = "温度：25℃";
    // 直接改文本框——报错！因为当前是后台线程，不是UI线程
    txtTemperature.Text = data; 
}
```

正确做法：通过`DispatcherObject`的`Dispatcher`（跑腿小哥）传话：

csharp:

```c#
private void CollectDataThread()
{
    string data = "温度：25℃";
    // 先查门禁：当前线程能不能直接操作文本框
    if (txtTemperature.CheckAccess())
    {
        // 能操作，直接改
        txtTemperature.Text = data;
    }
    else
    {
        // 不能操作，找跑腿小哥送指令到UI线程
        txtTemperature.Dispatcher.Invoke(() => 
        {
            txtTemperature.Text = data;
        });
    }
}
```

## 一句话总结

​	`DispatcherObject`就是 WPF 给 UI 对象加的 “线程规矩”：UI 对象只能在 UI 线程里动，其他线程想动必须通过`Dispatcher`（跑腿小哥）传话，它本身不干活，只负责 “管规矩、查权限、找小哥”。