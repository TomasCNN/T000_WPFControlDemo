

# 999999006-15_C# 特性 (Attribute) 与装饰器 (Decorator)代码解析

**源码：**

```c#
// 装饰器函数
public static Func<TInput, TResult> WithLogging<TInput, TResult>(
    this Func<TInput, TResult> func, 
    string operationName)
{
    return input =>
    {
        Console.WriteLine($"开始执行 {operationName}，参数：{input}");
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = func(input);
            Console.WriteLine($"执行 {operationName} 成功，耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"执行 {operationName} 失败：{ex.Message}");
            throw;
        }
    };
}

// 原函数
public int Add(int a, int b) => a + b;

// 使用装饰器包装原函数
var addWithLogging = Add.WithLogging("加法运算");

// 调用包装后的函数
Console.WriteLine(addWithLogging(1, 2));
```

这段代码实现了一个**通用的函数式日志装饰器**，它可以为任意单输入单输出的函数添加执行日志和性能计时功能，是装饰器模式最简洁的实现方式之一。下面我会逐句拆解语法、作用和背后的原理。

## 一、装饰器函数定义（核心部分）

csharp:

```c#
// 装饰器函数
public static Func<TInput, TResult> WithLogging<TInput, TResult>(
    this Func<TInput, TResult> func, 
    string operationName)
```

### 逐句解析：

1. `public static Func<TInput, TResult> WithLogging`
   - `public static`：这是一个公共静态方法，所以可以作为扩展方法使用
   - `Func<TInput, TResult>`：返回值类型是一个泛型委托，表示 "接收一个 TInput 类型参数，返回 TResult 类型结果" 的函数
   - `WithLogging`：装饰器方法名，意思是 "为函数添加日志功能"
2. `<TInput, TResult>`
   - 泛型参数，让这个装饰器可以适配**任意输入类型和任意输出类型**的函数
   - `TInput`：原函数的输入参数类型
   - `TResult`：原函数的返回值类型
3. `this Func<TInput, TResult> func`
   - `this`关键字：表示这是一个**扩展方法**，可以直接在`Func<TInput, TResult>`类型的实例上调用
   - `func`：被装饰的原函数，是我们要包装的目标函数
4. `string operationName`
   - 额外参数，用于在日志中标识当前操作的名称，方便区分不同函数的日志

------

csharp:

```c#
{
    return input =>
    {
```

### 逐句解析：

1. `return input =>`
   - 返回一个**Lambda 表达式**，这个 Lambda 表达式就是我们创建的**包装函数**
   - 它的签名和原函数完全相同：接收一个`TInput`类型的参数`input`，返回`TResult`类型的结果
   - 这是装饰器的核心：**返回一个和原函数签名完全相同的新函数**，这样调用者可以像调用原函数一样调用包装后的函数

------

csharp:

```c#
        Console.WriteLine($"开始执行 {operationName}，参数：{input}");
        var stopwatch = Stopwatch.StartNew();
```

### 逐句解析：

1. `Console.WriteLine($"开始执行 {operationName}，参数：{input}");`
   - **前置逻辑**：在调用原函数之前执行
   - 打印操作名称和传入的参数值，用于调试和日志记录
2. `var stopwatch = Stopwatch.StartNew();`
   - 创建并启动一个秒表，用于测量原函数的执行时间
   - `Stopwatch`是.NET 中用于高精度计时的类，`StartNew()`是静态工厂方法，创建并立即启动计时

------

csharp:

```c#
        try
        {
            var result = func(input);
            Console.WriteLine($"执行 {operationName} 成功，耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
```

### 逐句解析：

1. `try`
   - 使用 try-catch 块捕获原函数执行过程中可能抛出的任何异常
   - 确保即使原函数出错，我们也能记录错误日志
2. `var result = func(input);`
   - **调用原函数**：这是整个装饰器中唯一执行原业务逻辑的地方
   - 将包装函数接收到的参数`input`原封不动地传递给原函数`func`
   - 保存原函数的返回值`result`，用于后续返回给调用者
3. `Console.WriteLine($"执行 {operationName} 成功，耗时：{stopwatch.ElapsedMilliseconds}ms");`
   - **成功后置逻辑**：原函数执行成功后执行
   - 打印操作成功的日志和执行耗时（毫秒）
   - `stopwatch.ElapsedMilliseconds`获取从秒表启动到现在经过的毫秒数
4. `return result;`
   - 将原函数的返回值原封不动地返回给调用者
   - 确保包装后的函数和原函数的行为完全一致，调用者不会感知到任何差异

------

csharp:

```c#
        catch (Exception ex)
        {
            Console.WriteLine($"执行 {operationName} 失败：{ex.Message}");
            throw;
        }
    };
}
```

### 逐句解析：

1. `catch (Exception ex)`
   - 捕获原函数抛出的所有异常
2. `Console.WriteLine($"执行 {operationName} 失败：{ex.Message}");`
   - **失败后置逻辑**：原函数执行出错时执行
   - 打印操作失败的日志和异常信息
3. `throw;`
   - **重新抛出原始异常**
   - ⚠️ 关键细节：这里使用`throw;`而不是`throw ex;`
     - `throw;`：保留原始异常的堆栈跟踪，便于调试
     - `throw ex;`：会重置堆栈跟踪，丢失原始异常的发生位置
   - 确保调用者能够收到和原函数完全相同的异常，不改变原函数的错误行为
4. `};`
   - 结束 Lambda 表达式
   - `}`：结束 WithLogging 方法

## 二、原函数定义

csharp:

```c#
// 原函数
public int Add(int a, int b) => a + b;
```

### 解析：

- 这是一个简单的加法函数，接收两个 int 类型参数 a 和 b，返回它们的和

- 使用了 C# 6.0 引入的**表达式体成员**语法，等价于：

  csharp:

  ```c#
  public int Add(int a, int b)
  {
      return a + b;
  }
  ```

## 三、装饰器的使用

csharp:

```c#
// 使用装饰器包装原函数
var addWithLogging = Add.WithLogging("加法运算");
```

### 解析：

1. `Add.WithLogging("加法运算")`
   - 调用我们定义的扩展方法`WithLogging`，将原函数`Add`作为第一个参数传入
   - 第二个参数`"加法运算"`是操作名称，用于日志标识
   - 这里利用了 C# 的**方法组转换**：编译器会自动将方法名`Add`转换为匹配的委托类型
2. `var addWithLogging`
   - 接收装饰器返回的包装函数
   - 现在`addWithLogging`是一个和`Add`签名相同的函数，但它已经被添加了日志和计时功能

### ⚠️ 原代码的编译错误与修正

**注意**：这段代码实际上**无法编译通过**！原因是：

- 原函数`Add`有**两个参数**，对应的委托类型是`Func<int, int, int>`（两个输入，一个输出）
- 但我们的装饰器`WithLogging`只接受`Func<TInput, TResult>`（一个输入，一个输出）

**修正方案**：将装饰器修改为支持两个参数的版本：

csharp:

```c#
// 支持两个参数的日志装饰器
public static Func<T1, T2, TResult> WithLogging<T1, T2, TResult>(
    this Func<T1, T2, TResult> func, 
    string operationName)
{
    return (a, b) =>
    {
        Console.WriteLine($"开始执行 {operationName}，参数：{a}, {b}");
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = func(a, b);
            Console.WriteLine($"执行 {operationName} 成功，耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"执行 {operationName} 失败：{ex.Message}");
            throw;
        }
    };
}
```

修正后，`Add.WithLogging("加法运算")`就可以正常编译了。

## 四、调用包装后的函数

csharp:

```c#
// 调用包装后的函数
Console.WriteLine(addWithLogging(1, 2));
```

### 解析：

1. `addWithLogging(1, 2)`
   - 调用包装后的函数，传入参数 1 和 2
   - **实际执行流程**：
     1. 执行 Lambda 表达式中的前置逻辑：打印 "开始执行 加法运算，参数：1, 2"
     2. 启动秒表
     3. 调用原函数`Add(1, 2)`，得到结果 3
     4. 执行成功后置逻辑：打印 "执行 加法运算 成功，耗时：0ms"
     5. 返回结果 3
2. `Console.WriteLine(...)`
   - 打印包装函数的返回值 3

### 最终输出结果：

plaintext:

```tex
开始执行 加法运算，参数：1, 2
执行 加法运算 成功，耗时：0ms
3
```

## 五、核心原理总结

这个装饰器实现了**开闭原则**：对扩展开放，对修改关闭。它：

1. **完全不修改原函数`Add`的代码**
2. **动态添加了日志和计时功能**
3. **保持了原函数的签名和行为**：调用者可以像调用原函数一样调用包装后的函数
4. **通用可复用**：可以为任何符合签名的函数添加日志功能，而不需要重复编写日志代码

## 六、与特性的本质区别对比

| 对比项         | 这段装饰器代码                 | C# 特性                    |
| :------------- | :----------------------------- | :------------------------- |
| 本质           | **行为包装器**                 | **元数据容器**             |
| 执行时机       | 调用函数时**主动执行**         | 需要外部代码**被动读取**   |
| 是否修改原行为 | ✅ 直接修改了函数的执行流程     | ❌ 完全不修改原代码行为     |
| 额外代码       | 不需要任何外部代码处理         | 需要手动编写读取特性的代码 |
| 性能           | 每次调用都有轻微的函数调用开销 | 只有第一次读取时有反射开销 |

这就是为什么我们说 "特性不是装饰器"：特性只是给代码贴标签，而装饰器是直接改变代码的执行方式。