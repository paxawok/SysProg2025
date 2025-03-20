# Звіт
**Тема:** Паралелізм даних і задач. Використання бібліотеки TPL (.NET)
**Виконане завдання:** Створення та запуск двох паралельних задач та використання методу Parallel.Invoke() з лямбда-виразами.

## 1. Теоретична частина
Паралелізм дозволяє одночасно виконувати кілька завдань, що покращує швидкість роботи програм. Для зручності створення паралельних задач у .NET використовується Task Parallel Library (TPL), яка базується на класі `Task` (а не на класі `Thread`).

### Основні поняття:
- **Задача (Task)** — абстракція, що представляє асинхронну операцію.
- **Методи створення задач:**
  - Конструктор `new Task(Action дія)` і метод `Start()`.
  - `Task.Factory.StartNew()` створює та відразу запускає задачу.
- `Parallel.Invoke()` запускає декілька задач паралельно.
- Метод `Wait()` призупиняє основний потік до завершення задачі.
- Методи `WaitAll()` та `WaitAny()` використовуються для очікування завершення кількох задач.

## 2. Практична частина
### Реалізація задач
У рамках лабораторної роботи було реалізовано:

1. **Завдання 1 і 2:** Створені дві паралельні задачі, які виконують один і той самий метод `TaskMethod`:
```csharp
Task task1 = new Task(TaskMethod, 1);
Task task2 = new Task(TaskMethod, 2);
task1.Start();
task2.Start();
Task.WaitAll(task1, task2);
```

Метод `TaskMethod` включає затримку, пропорційну ID задачі (200мс * ID задачі):
```csharp
static void TaskMethod(object taskNumber)
{
    int num = (int)taskNumber;
    Console.WriteLine($"Task {num} (ID: {Task.CurrentId}) started");
    
    for (int i = 0; i < 5; i++)
    {
        // Затримка пропорційно ідентифікатору задачі
        int delay = 200 * Task.CurrentId.Value;
        Thread.Sleep(delay);
        Console.WriteLine($"Task {num} (ID: {Task.CurrentId}) step {i}, waited for {delay}ms");
    }
    
    Console.WriteLine($"Task {num} (ID: {Task.CurrentId}) completed");
}
```

2. **Завдання 3:** Створена задача з використанням лямбда-виразу:
```csharp
Task lambdaTask = Task.Factory.StartNew(() =>
{
    Console.WriteLine($"Lambda task (ID: {Task.CurrentId}) started");
    
    for (int i = 0; i < 3; i++)
    {
        Thread.Sleep(300);
        Console.WriteLine($"Lambda task step {i}");
    }
    
    Console.WriteLine($"Lambda task (ID: {Task.CurrentId}) completed");
});

lambdaTask.Wait();
```

3. **Завдання 4:** Використання методу `Parallel.Invoke()` з лямбда-виразами:
```csharp
Parallel.Invoke(
    // Перший лямбда-вираз
    () =>
    {
        Console.WriteLine($"First lambda in Invoke (Thread ID: {Thread.CurrentThread.ManagedThreadId}) started");
        for (int i = 0; i < 3; i++)
        {
            Thread.Sleep(250);
            Console.WriteLine($"First lambda: step {i}");
        }
        Console.WriteLine("First lambda completed");
    },
    
    // Другий лямбда-вираз
    () =>
    {
        Console.WriteLine($"Second lambda in Invoke (Thread ID: {Thread.CurrentThread.ManagedThreadId}) started");
        for (int i = 0; i < 4; i++)
        {
            Thread.Sleep(200);
            Console.WriteLine($"Second lambda: step {i}");
        }
        Console.WriteLine("Second lambda completed");
    }
);
```

## 3. Результати роботи
### Очікуваний вивід програми:
```
=== Завдання 1 і 2: Дві паралельні задачі з очікуванням WaitAll() ===
Starting tasks...
ID of task1: 1
ID of task2: 2
Task 1 (ID: 1) started
Task 2 (ID: 2) started
Task 1 (ID: 1) step 0, waited for 200ms
Task 2 (ID: 2) step 0, waited for 400ms
Task 1 (ID: 1) step 1, waited for 200ms
Task 1 (ID: 1) step 2, waited for 200ms
Task 2 (ID: 2) step 1, waited for 400ms
Task 1 (ID: 1) step 3, waited for 200ms
Task 1 (ID: 1) step 4, waited for 200ms
Task 1 (ID: 1) completed
Task 2 (ID: 2) step 2, waited for 400ms
Task 2 (ID: 2) step 3, waited for 400ms
Task 2 (ID: 2) step 4, waited for 400ms
Task 2 (ID: 2) completed
All tasks completed!

=== Завдання 3: Задача у вигляді лямбда-виразу ===
Lambda task (ID: 3) started
Lambda task step 0
Lambda task step 1
Lambda task step 2
Lambda task (ID: 3) completed
Lambda task finished!

=== Завдання 4: Parallel.Invoke з лямбда-виразами ===
First lambda in Invoke (Thread ID: 1) started
Second lambda in Invoke (Thread ID: 4) started
Second lambda: step 0
First lambda: step 0
Second lambda: step 1
First lambda: step 1
Second lambda: step 2
First lambda: step 2
Second lambda: step 3
First lambda completed
Second lambda completed
All invoke lambdas completed!

Program finished. Press any key to exit.
```

### Аналіз результатів:
- Задачі виконувалися паралельно, що підтверджується виводом у консоль.
- Затримка в задачах була пропорційною до їх ідентифікаторів, що добре видно з різниці в швидкості виконання task1 і task2.
- Використання `Task.WaitAll()` забезпечило очікування завершення обох задач перед продовженням програми.
- Лямбда-вирази були успішно використані як для задачі, так і для методу `Parallel.Invoke()`.
- Метод `Parallel.Invoke()` автоматично виконав обидва лямбда-вирази паралельно, що демонструє ефективність бібліотеки TPL.

## 4. Висновки:
- Використання TPL робить програмування багатопоточних додатків простішим та ефективнішим.
- Методи, що надає TPL, дозволяють легко керувати паралельним виконанням задач без необхідності ручного управління потоками.
- Лямбда-вирази є зручним інструментом для швидкого створення анонімних задач у паралельних обчисленнях.
- Використання ідентифікаторів задач дозволяє розрізняти задачі та керувати їх поведінкою навіть при використанні одного і того ж методу.

---
*Роботу виконав: Черевач Юрій*